using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeSceneMainStatePanel : MonoBehaviour, IStringGameEventListener
{
    [Header("UI Components")]
    [SerializeField] private Image _background;                 // 顶部信息栏
    [SerializeField] private LoopHorizontalScrollRect _scrollView; // 动态滚动视图
    [SerializeField] private GameObject _chatBubblePanel;      // 聊天气泡面板
    [SerializeField] private Image _currentActorIcon;      // 当前选中角色图标

    [Header("Events")]
    [SerializeField] private StringGameEvent _onActorClickedEvent; // 角色点击事件

    private string _selectedActorName = string.Empty;

    public string SelectedActorName => _selectedActorName;


    void Start()
    {
        Debug.Assert(_background != null, "Background Image component is not assigned in the inspector.");
        Debug.Assert(_scrollView != null, "ScrollView component is not assigned in the inspector.");
        Debug.Assert(SpriteCacheManager.Instance != null, "SpriteCacheManager instance is not available.");
        Debug.Assert(_chatBubblePanel != null, "ChatBubblePanel GameObject is not assigned in the inspector.");
        Debug.Assert(_onActorClickedEvent != null, "onActorClickedEvent is null");

        _onActorClickedEvent.RegisterListener(this);

        // 初始化滚动视图
        _chatBubblePanel.SetActive(false); // 初始状态隐藏聊天气泡面板
        _currentActorIcon.gameObject.SetActive(false); // 初始状态隐藏当前角色图标
        _selectedActorName = string.Empty;

        // 根据登录状态设置背景图
        if (GameContext.Instance.IsLoggedIn)
        {
            var stageSprite = SpriteCacheManager.Instance.GetSprite(HomeScene.CachedHomeStageName);
            if (stageSprite != null)
            {
                _background.sprite = stageSprite;
            }
            else
            {
                Debug.LogWarning("Stage sprite not found for: " + HomeScene.CachedHomeStageName);
            }
        }
        else
        {
            var stageSprite = SpriteCacheManager.Instance.GetSprite(MockData.MockStageName);
            if (stageSprite != null)
            {
                _background.sprite = stageSprite;
            }
            else
            {
                Debug.LogWarning("Stage sprite not found for: " + MockData.MockStageName);
            }
        }
    }

    void OnDestroy()
    {
        // 注销角色点击事件监听器
        if (_onActorClickedEvent != null)
        {
            _onActorClickedEvent.UnregisterListener(this);
        }
    }

    /// <summary>
    /// 当前角色图标被点击时调用,隐藏聊天气泡和当前角色图标,并清空选中角色状态
    /// </summary>
    public void OnClickCloseChatBubble()
    {
        Debug.Log($"Current actor icon clicked: {_selectedActorName}");
        _chatBubblePanel.SetActive(false); // 初始状态隐藏聊天气泡面板
        _currentActorIcon.gameObject.SetActive(false); // 初始状态隐藏当前角色图标
        _selectedActorName = string.Empty;
    }

    /// <summary>
    /// 刷新主状态面板显示,包括根据当前玩家所在Stage刷新角色列表,以及如果有选中角色则刷新当前角色图标和聊天气泡显示
    /// </summary>
    /// <returns></returns>
    public async UniTaskVoid RefreshView()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.Log("[HomeScene] GameContext Root is null");
            HomeScene.ActorNamesOnStage.Clear();


            var mockData = MockData.CreateActorData();
            for (int i = 0; i < mockData.Count; i++)
            {
                HomeScene.ActorNamesOnStage.Add(mockData[i].name);
            }

            _scrollView.totalCount = HomeScene.ActorNamesOnStage.Count;
            _scrollView.RefillCells();
            return;
        }

        var stagesState = await GameStateSync.Instance.GetStagesState();
        if (stagesState == null)
        {
            Debug.LogError("[HomeScene] Failed to get stages state from server");
            HomeScene.ActorNamesOnStage.Clear();
            _scrollView.totalCount = HomeScene.ActorNamesOnStage.Count;
            _scrollView.RefillCells();
            return;
        }

        foreach (var kvp in stagesState)
        {
            if (kvp.Value.Contains(GameContext.Instance.PlayerActorName))
            {
                HomeScene.ActorNamesOnStage = kvp.Value;
                break;
            }
        }

        HomeScene.ActorNamesOnStage.Remove(GameContext.Instance.PlayerActorName); // 移除玩家角色自己
        Debug.Log($"Actors in current stage: {string.Join(", ", HomeScene.ActorNamesOnStage)}");
        _scrollView.totalCount = HomeScene.ActorNamesOnStage.Count;
        _scrollView.RefillCells();
    }

    public void OnEventRaised(string actorName)
    {
        Debug.Log($"[HomeScene] Actor clicked: {actorName}");

        // 防止重复选择同一角色
        if (_selectedActorName == actorName)
        {
            Debug.Log($"[HomeScene] Actor {actorName} is already selected.");
            return;
        }

        // 更新选中的角色名称
        _selectedActorName = actorName;
        Debug.Log($"[HomeScene] Selected actor updated to: {_selectedActorName}");

        // 更新显示
        _currentActorIcon.gameObject.SetActive(true);
        _chatBubblePanel.SetActive(true);

        // 更新UI显示选中的角色
        RefreshSelectedActorPortrait(_selectedActorName);

        // 根据选中角色刷新聊天气泡内容
        RefreshActorEventSummary(_selectedActorName);
    }

    /// <summary>
    /// 根据选中的角色名称更新当前角色图标显示
    /// </summary>
    /// <param name="actorName"></param>
    private void RefreshSelectedActorPortrait(string actorName)
    {
        Debug.Log($"[HomeScene] Handling selection for actor: {actorName}");

        // selectedActorName 必须在 HomeScene.ActorNamesOnStage 内
        Debug.Assert(HomeScene.ActorNamesOnStage.Contains(actorName), $"Selected actor {actorName} is not in the current stage.");

        // // 更新当前角色的Sprite显示
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(actorName);
        Debug.Assert(cachedSprite != null, "Player actor sprite is null for entity: " + actorName);
        if (cachedSprite != null)
        {

            _currentActorIcon.sprite = cachedSprite;
        }
        else
        {
            Debug.LogWarning($"Sprite not found for selected actor: {actorName}");
        }
    }

    /// <summary>
    /// 根据选中的角色名称更新聊天气泡内容
    /// </summary>
    /// <param name="actorName"></param>
    public void RefreshActorEventSummary(string actorName)
    {
        var latestRoundEventsForActor = GameContext.Instance.GetLatestRoundEventsForActor(actorName);
        if (latestRoundEventsForActor.Count > 0)
        {
            List<string> agentEventSummaries = new();
            foreach (var agentEvent in latestRoundEventsForActor)
            {
                Debug.Log($"[HomeScene] Last event for {actorName}: {agentEvent.GetType().Name}");
                var summary = GameUtils.FormatAgentEventSummary(agentEvent);
                agentEventSummaries.Add(summary);
            }

            // 设置内容
            if (agentEventSummaries.Count > 0)
            {
                _chatBubblePanel.GetComponentInChildren<Text>().text = string.Join("\n", agentEventSummaries);
            }
        }
        else
        {
            _chatBubblePanel.GetComponentInChildren<TMP_Text>().text = $"{actorName}: 暂无最近事件";
        }
    }
}
