using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

/// <summary>
/// 主场景数据结构，包含当前场景的关键信息，如场景名、在场景中的角色列表、所属地下城等
/// </summary>
[System.Serializable]
public class HomeSceneData
{
    public string stageName;
    public List<EntitySerialization> actorsOnStage = new();
    public string dungeonName = string.Empty;
}

/// <summary>
/// 主场景控制器(MainScene)
/// 负责管理主场景的UI交互、场景切换、玩家信息显示等核心功能
/// 作为玩家进入游戏后的主要控制中心,提供前往不同游戏场景的入口
/// </summary>
public class MainScene : MonoBehaviour, IUIEventListener
{
    public static readonly List<HomeSceneData> HomeScenes = new();

    public static readonly string PreSceneName = "LoginScene";
    public static readonly string NextSceneName = "HomeScene";
    public static readonly string DungeonOverviewSceneName = "DungeonOverviewScene";

    [Header("UI Components")]
    [SerializeField] private PlayerTopBar _topBar;
    [SerializeField] private GameObject _playerInfoPanel;
    [SerializeField] private HomeScenesPanel _homeScenesPanel;

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onMainSceneHomeSceneItemClickedEvent;

    void OnDestroy()
    {
        if (_onMainSceneHomeSceneItemClickedEvent != null)
        {
            _onMainSceneHomeSceneItemClickedEvent.UnregisterListener(this);
        }
    }

    /// <summary>
    /// 场景启动初始化方法
    /// 验证所有必需的组件引用,注册UI事件回调,刷新游戏状态
    /// </summary>
    void Start()
    {
        Debug.Assert(_topBar != null, "_topBar is null");
        Debug.Assert(_playerInfoPanel != null, "_playerInfoPanel is null");
        Debug.Assert(_homeScenesPanel != null, "_homeScenesPanel is null");
        Debug.Assert(_onMainSceneHomeSceneItemClickedEvent != null, "_onMainSceneHomeSceneItemClickedEvent is null");

        // 注册 MainSceneHomeSceneItemClicked 事件监听器
        _onMainSceneHomeSceneItemClickedEvent.RegisterListener(this);

        // 刚进入主场景时，先隐藏玩家信息面板和场景列表，等数据加载完成后再显示
        _topBar.gameObject.SetActive(true);
        _playerInfoPanel.gameObject.SetActive(false);
        _homeScenesPanel.gameObject.SetActive(true);

        // 启动时立即刷新游戏状态
        _homeScenesPanel.RereshViewAsync().Forget();
    }

    /// <summary>
    /// 返回按钮点击事件处理
    /// 触发登出流程并返回登录场景
    /// </summary>
    public void OnClickLogout()
    {
        LogoutAsync().Forget();
    }

    /// <summary>
    /// 运行按钮点击事件处理
    /// 推进游戏状态，让所有角色执行行动
    /// </summary>
    public void OnClickAdvanceGameState()
    {
        Debug.Log("Run button clicked in MainScene.");
        AdvanceGameStateAsync().Forget();
    }

    /// <summary>
    /// 玩家头像点击事件回调
    /// 显示玩家详细信息面板
    /// </summary>
    public void OnClickTopBarPlayerIcon()
    {
        //Debug.Log("Head icon clicked in MainScene!");
        _playerInfoPanel.SetActive(true);
    }

    /// <summary>
    /// 打开地牢浏览场景的协程
    /// 直接加载 ViewDungeonScene 场景
    /// </summary>
    private async UniTaskVoid LoadDungeonOverviewScene()
    {
        await UniTask.Yield();
        SceneManager.LoadScene(DungeonOverviewSceneName);
    }

    /// <summary>
    /// 返回登录场景的协程
    /// 1. 使用 SessionManager 执行登出
    /// 2. 加载登录场景
    /// </summary>
    private async UniTaskVoid LogoutAsync()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, skipping logout process");
            return;
        }

        bool isLogoutSuccessful = await SessionManager.Instance.Logout();
        if (!isLogoutSuccessful)
        {
            Debug.LogError("[MainScene] Logout failed");
            return;
        }

        await UniTask.Yield();
        SceneManager.LoadScene(PreSceneName);
    }

    /// <summary>
    /// 推进游戏状态的协程
    /// 调用 HomeGamePlayManager 推进所有角色的行动，并同步最新的游戏状态
    /// </summary>
    /// <returns>协程迭代器</returns>
    private async UniTaskVoid AdvanceGameStateAsync()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, cannot advance game state");
            return;
        }

        bool isGameSuccessfullyAdvanced = await HomeGamePlayManager.Instance.AdvanceGame(new List<string>());
        if (!isGameSuccessfullyAdvanced)
        {
            Debug.LogError("[MainScene] AdvanceGame failed");
            return;
        }

        Debug.Log("[MainScene] Game state advanced successfully");
        _homeScenesPanel.RereshViewAsync().Forget();
    }

    /// <summary>
    /// 将玩家角色转移到指定的 Stage(服务器状态) 和 Scene(Unity 场景)
    /// 完整流程:
    /// 1. 检查玩家当前是否已在目标 Stage
    /// 2. 如果不在,调用服务器 API 切换 Stage
    /// 3. 刷新游戏状态并验证转换事件
    /// 4. 设置待处理的场景配置并加载目标 Unity 场景
    /// </summary>
    /// <param name="sceneConfig">目标场景的配置数据(包含 StageName 和 SceneDisplayName)</param>
    private async UniTaskVoid TransitionToScene(string targetStageName)
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, cannot transition to scene");
            await UniTask.Yield();
            HomeScene.CachedStageName = targetStageName;
            SceneManager.LoadScene(NextSceneName);
            return;
        }


        var stagesState = await GameStateSync.Instance.GetStagesState();
        if (stagesState == null)
        {
            Debug.LogError("Failed to refresh stage-actor mapping from server");
            return;
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

        //Debug.Log($"Current stage: {currentStageName}, Target stage: {HomeScene.CachedHomeStageName}");
        if (currentStageName != targetStageName)
        {
            bool switchSuccess = await HomeGamePlayManager.Instance.SwitchStage(targetStageName);

            if (!switchSuccess)
            {
                Debug.LogError($"[MainScene] SwitchStage to {targetStageName} failed");
                return;
            }
        }
        else
        {
            Debug.Log($"Already in target stage: {targetStageName}, no need to switch.");
        }

        await UniTask.Yield();
        HomeScene.CachedStageName = targetStageName;
        SceneManager.LoadScene(NextSceneName);
    }

    /// <summary>
    /// 处理 UI 事件
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEventRaised(UIEventData eventData)
    {
        Debug.Log($"[TestDungeonCombatScenePrototype] {eventData}");
        Debug.Log($"OnEventRaised: {eventData.eventType}, TargetId: {eventData.targetId}, Index: {eventData.index}, ExtraData: {eventData.extraData}");

        switch (eventData.eventType)
        {

            case UIEventType.MainSceneHomeSceneItemClicked:
                {
                    var stageName = eventData.targetId;
                    var dungeonName = eventData.extraData;
                    Debug.Log($"MainSceneHomeSceneItemClicked: StageName: {stageName}, Index: {eventData.index}");

                    if (!string.IsNullOrEmpty(dungeonName))
                    {
                        LoadDungeonOverviewScene().Forget();
                        return;
                    }

                    TransitionToScene(stageName).Forget();
                }
                break;

            default:
                Debug.LogWarning($"未处理的事件类型: {eventData.eventType}");
                break;
        }
    }
}
