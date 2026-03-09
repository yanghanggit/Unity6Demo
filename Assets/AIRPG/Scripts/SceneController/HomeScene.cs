using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// 主场景控制器
/// 负责管理主场景的UI交互、角色选择和状态切换
/// </summary>
public class HomeScene : MonoBehaviour
{
    public static readonly string PreScene = "MainScene";   // 上一个场景名称

    public static string CachedHomeStageName = string.Empty;
    public static List<string> ActorNamesOnStage = new();

    [SerializeField] private HomeSceneMainStatePanel _homeSceneMainStatePanel; // 主状态面板组件
    [SerializeField] private HomeSceneInputStatePanel _homeSceneInputStatePanel; // 输入状态面板组件

    //
    void Awake()
    {
        // 如果有从 MainScene 传递过来的配置,使用它
        if (!GameContext.Instance.IsLoggedIn)
        {
            CachedHomeStageName = MockData.MockStageName;
        }
        else
        {
            Debug.Log($"[CachedHomeStageName: {CachedHomeStageName}]");
        }
    }

    // Unity生命周期方法
    /// <summary>
    /// 场景初始化方法
    /// 执行组件引用验证和初始UI状态设置
    /// 注册所有事件监听器
    /// </summary>
    void Start()
    {
        Debug.Assert(SpriteCacheManager.Instance != null, "SpriteCacheManager instance is null");
        Debug.Assert(_homeSceneMainStatePanel != null, "_homeSceneMainStatePanel is null");
        Debug.Assert(_homeSceneInputStatePanel != null, "_homeSceneInputStatePanel is null");

        // 初始UI状态设置
        _homeSceneMainStatePanel.gameObject.SetActive(true); // 显示主状态面板
        _homeSceneInputStatePanel.gameObject.SetActive(false); // 隐藏输入状态面板

        // 刷新角色列表
        _homeSceneMainStatePanel.RefreshView().Forget();
    }

    // UI按钮回调方法
    /// <summary>
    /// 运行按钮点击回调
    /// TODO: 实现游戏开始逻辑
    /// </summary>
    public void OnClickPlanning()
    {
        Debug.Log("Planning button clicked in HomeScene.");
        AdvanceHomeState().Forget();
    }

    /// <summary>
    /// 返回按钮点击回调
    /// TODO: 实现返回上一场景逻辑
    /// </summary>
    public void OnClickBackMainScene()
    {
        Debug.Log("Back to MainScene button clicked in HomeScene.");
        ReturnToMainScene().Forget();
    }

    /// <summary>
    /// 当前角色图标被点击时调用,隐藏聊天气泡和当前角色图标,并清空选中角色状态
    /// </summary>
    public void OnClickSelectActor()
    {
        Debug.Log($"Current actor icon clicked: {_homeSceneMainStatePanel.SelectedActorName}");
        _homeSceneInputStatePanel.gameObject.SetActive(true); // 显示输入状态面板
        _homeSceneInputStatePanel.OnActivate(_homeSceneMainStatePanel.SelectedActorName); // 设置输入字段为选中角色名称
    }

    /// <summary>
    /// 发送消息按钮点击回调
    /// 验证游戏状态、角色选择和输入内容后,执行说话动作
    /// </summary>
    public void OnClickSend()
    {
        Debug.Log("Send Message button clicked");

        _homeSceneInputStatePanel.gameObject.SetActive(false); // 隐藏输入状态面板

        var inputText = _homeSceneInputStatePanel.GetInputText();
        if (string.IsNullOrEmpty(inputText))
        {
            Debug.LogWarning("Input text is empty, cannot send message");
            return;
        }

        ExecuteSpeakAction(_homeSceneMainStatePanel.SelectedActorName, _homeSceneInputStatePanel.GetInputText()).Forget();
    }

    /// <summary>
    /// 如果玩家不在目标 Stage 中则切换到该 Stage,已在目标 Stage 则直接返回成功
    /// </summary>
    /// <param name="targetStageName">目标 Stage 名称</param>
    /// <param name="onComplete">完成回调,参数为是否成功进入目标 Stage</param>
    /// <returns>协程迭代器</returns>
    private async UniTask<bool> SwitchToStageIfNeeded(string targetStageName)
    {
        var stagesState = await GameStateSync.Instance.GetStagesState();
        if (stagesState == null)
        {
            Debug.LogError("Failed to refresh stage-actor mapping from server");
            return false;
        }

        var currentStageName = string.Empty;
        foreach (var kvp in stagesState)
        {
            if (kvp.Value.Contains(GameContext.Instance.PlayerActorName))
            {
                currentStageName = kvp.Key;
                break;
            }
        }

        if (currentStageName == targetStageName)
        {
            Debug.Log($"[HomeScene] Already in target stage {targetStageName}, no need to switch.");
            return false;
        }

        bool isStageSwitchSuccessful = await HomeGamePlayManager.Instance.SwitchStage(targetStageName);
        if (!isStageSwitchSuccessful)
        {
            Debug.LogError($"[HomeScene] SwitchStage to {targetStageName} failed");
            return false;
        }

        //await GameStateSync.Instance.RefreshMappingAndActorsFromServer();
        Debug.Log($"[HomeScene] Successfully switched to stage: {targetStageName}");
        return true;
    }

    /// <summary>
    /// 返回主场景的协程
    /// 检查游戏是否已正确设置,切换到监视之屋Stage,然后加载MainScene场景
    /// </summary>
    /// <returns>协程迭代器</returns>
    private async UniTaskVoid ReturnToMainScene()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, cannot return to main scene");
            return;
        }

        bool switchSuccess = await SwitchToStageIfNeeded(GameContext.Instance.PlayerOnlyStageName);
        if (!switchSuccess)
        {
            Debug.LogError($"[HomeScene] Failed to ensure in {GameContext.Instance.PlayerOnlyStageName}");
            return;
        }

        await UniTask.Yield();
        SceneManager.LoadScene(PreScene);
    }

    /// <summary>
    /// 推进家园场景状态的协程
    /// 调用 HomeGamePlayManager 推进场景中所有角色(包括NPC)的行动,并同步最新的游戏状态
    /// </summary>
    /// <returns>协程迭代器</returns>
    private async UniTaskVoid AdvanceHomeState()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, cannot advance home state");
            return;
        }

        bool isGameAdvanceSuccessful = await HomeGamePlayManager.Instance.AdvanceGame(new List<string>());
        if (!isGameAdvanceSuccessful)
        {
            Debug.LogError("[HomeScene] AdvanceGame failed");
            return;
        }
    }

    /// <summary>
    /// 执行说话动作的协程
    /// 调用 HomeGamePlayManager 发送消息到目标角色,并同步最新的游戏状态
    /// </summary>
    /// <param name="targetActorName">目标角色名称</param>
    /// <param name="messageContent">消息内容</param>
    /// <returns>协程迭代器</returns>
    private async UniTaskVoid ExecuteSpeakAction(string targetActorName, string messageContent)
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, cannot execute speak action");
            return;
        }

        bool speakSuccess = await HomeGamePlayManager.Instance.SpeakToActor(targetActorName, messageContent);
        if (!speakSuccess)
        {
            Debug.LogError("[HomeScene] SpeakToActor failed");
            return;
        }

        Debug.Log("[HomeScene] Speak action completed successfully");
    }
}




