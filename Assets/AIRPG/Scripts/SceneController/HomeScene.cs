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
public class HomeScene : MonoBehaviour, IUIEventListener
{
    public static readonly string PreScene = "MainScene";   // 上一个场景名称
    public static string CachedStageName = string.Empty;
    public static List<string> CachedActorNames = new();

    [Header("UI Components")]
    [SerializeField] private HomeSceneMainStatePanel _homeSceneMainStatePanel; // 主状态面板组件
    [SerializeField] private HomeSceneInputStatePanel _homeSceneInputStatePanel; // 输入状态面板组件

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onHomeSceneActorItemClickedEvent; // 角色点击事件

    [SerializeField] private UIEventGameEvent _onGameStateUpdatedEvent; // 游戏状态更新事件，携带最新的 GameContext 数据

    private string _currentSelectedActor = string.Empty; // 当前选中角色名称
    //
    void Awake()
    {
        // 如果有从 MainScene 传递过来的配置,使用它
        if (!GameContext.Instance.IsLoggedIn)
        {
            CachedStageName = MockData.MockStageName;
        }
        else
        {
            Debug.Log($"[CachedHomeStageName: {CachedStageName}]");
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
        Debug.Assert(_onHomeSceneActorItemClickedEvent != null, "onActorClickedEvent is null");
        Debug.Assert(_onGameStateUpdatedEvent != null, "_onGameStateUpdatedEvent is null");

        // 注册角色点击事件监听器
        _onHomeSceneActorItemClickedEvent.RegisterListener(this);
        _onGameStateUpdatedEvent.RegisterListener(this);

        // 刚进入主场景时，先隐藏玩家信息面板和场景列表，等数据加载完成后再显示
        _currentSelectedActor = string.Empty;


        // 初始UI状态设置
        _homeSceneMainStatePanel.gameObject.SetActive(true); // 显示主状态面板
        _homeSceneInputStatePanel.gameObject.SetActive(false); // 隐藏输入状态面板

        // 刷新角色列表
        _homeSceneMainStatePanel.RefreshActorsSync().Forget();
    }

    void OnDestroy()
    {
        // 注销角色点击事件监听器
        if (_onHomeSceneActorItemClickedEvent != null)
        {
            _onHomeSceneActorItemClickedEvent.UnregisterListener(this);
        }

        if (_onGameStateUpdatedEvent != null)
        {
            _onGameStateUpdatedEvent.UnregisterListener(this);
        }
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
        //Debug.Log($"Current actor icon clicked: {HomeScene.CachedSelectedActor}");
        _homeSceneInputStatePanel.gameObject.SetActive(true); // 显示输入状态面板
        Debug.Assert(!string.IsNullOrEmpty(_currentSelectedActor), "Selected actor is null or empty when trying to activate input panel");
        _homeSceneInputStatePanel.OnActivate(_currentSelectedActor); // 设置输入字段为选中角色名称
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

        if (string.IsNullOrEmpty(_currentSelectedActor))
        {
            Debug.LogWarning("No actor selected, cannot send message");
            return;
        }

        ExecuteSpeakAction(_currentSelectedActor, _homeSceneInputStatePanel.GetInputText()).Forget();
    }

    /// <summary>
    /// 当前角色图标被点击时调用,隐藏聊天气泡和当前角色图标,并清空选中角色状态
    /// </summary>
    public void OnClickCloseChatBubble()
    {
        _currentSelectedActor = string.Empty;
        _homeSceneMainStatePanel.HideActorDetails();
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actorName"></param>
    // public void OnEventRaised(string actorName)
    // {
    //     Debug.Log($"[HomeScene] Actor clicked: {actorName}");
    //     _currentSelectedActor = actorName;
    //     _homeSceneMainStatePanel.ShowActorDetails(actorName);
    // }

    /// <summary>
    /// 监听游戏状态更新事件,根据最新的 GameContext 数据刷新主场景
    /// 
    public void OnEventRaised(UIEventData eventData)
    {
        Debug.Log($"[TestDungeonCombatScenePrototype] {eventData}");
        Debug.Log($"OnEventRaised: {eventData.eventType}, TargetId: {eventData.targetId}, Index: {eventData.index}, ExtraData: {eventData.extraData}");

        switch (eventData.eventType)
        {
            case UIEventType.GameStateUpdated:
                {
                    Debug.Log("[HomeScene] Received GameStateUpdated event, refreshing main state panel");

                    var latestRoundEventsForActor = GameContext.Instance.GetLatestRoundEventsForActor(_currentSelectedActor);
                    if (latestRoundEventsForActor.Count > 0)
                    {
                        List<string> agentEventSummaries = new();
                        foreach (var agentEvent in latestRoundEventsForActor)
                        {
                            Debug.Log($"[HomeScene] Last event for {_currentSelectedActor}: {agentEvent.GetType().Name}");
                            var summary = GameUtils.FormatAgentEventSummary(agentEvent);
                            agentEventSummaries.Add(summary);
                        }

                        // 设置内容
                        if (agentEventSummaries.Count > 0)
                        {
                            // _chatBubblePanel.GetComponentInChildren<Text>().text = string.Join("\n", agentEventSummaries);
                            _homeSceneMainStatePanel.SetChatBubble(string.Join("\n", agentEventSummaries));
                        }
                    }
                }
                break;

            case UIEventType.HomeSceneActorItemClicked:
                {
                    Debug.Log($"[HomeScene] Actor item clicked: {eventData.targetId}");
                    _currentSelectedActor = eventData.targetId;
                    _homeSceneMainStatePanel.ShowActorDetails(_currentSelectedActor);
                }
                break;

            default:
                Debug.LogWarning($"未处理的事件类型: {eventData.eventType}");
                break;
        }
    }
}


