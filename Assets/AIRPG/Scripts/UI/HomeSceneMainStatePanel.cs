using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeSceneMainStatePanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _background;                 // 顶部信息栏
    [SerializeField] private LoopHorizontalScrollRect _scrollView; // 动态滚动视图
    [SerializeField] private GameObject _chatBubblePanel;      // 聊天气泡面板
    [SerializeField] private Image _currentActorIcon;      // 当前选中角色图标

    void Start()
    {
        Debug.Assert(_background != null, "Background Image component is not assigned in the inspector.");
        Debug.Assert(_scrollView != null, "ScrollView component is not assigned in the inspector.");
        Debug.Assert(SpriteCacheManager.Instance != null, "SpriteCacheManager instance is not available.");
        Debug.Assert(_chatBubblePanel != null, "ChatBubblePanel GameObject is not assigned in the inspector.");

        // 初始化滚动视图
        _chatBubblePanel.SetActive(false); // 初始状态隐藏聊天气泡面板
        _currentActorIcon.gameObject.SetActive(false); // 初始状态隐藏当前角色图标

        // 根据登录状态设置背景图
        if (GameContext.Instance.IsLoggedIn)
        {
            var stageSprite = SpriteCacheManager.Instance.GetSprite(HomeScene.CachedStageName);
            if (stageSprite != null)
            {
                _background.sprite = stageSprite;
            }
            else
            {
                Debug.LogWarning("Stage sprite not found for: " + HomeScene.CachedStageName);
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

    /// <summary>
    /// 刷新主状态面板显示,包括根据当前玩家所在Stage刷新角色列表,以及如果有选中角色则刷新当前角色图标和聊天气泡显示
    /// </summary>
    /// <returns></returns>
    public async UniTaskVoid RefreshActorsSync()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.Log("[HomeScene] GameContext Root is null");
            HomeScene.CachedActorNames.Clear();


            var mockData = MockData.CreateActorData();
            for (int i = 0; i < mockData.Count; i++)
            {
                HomeScene.CachedActorNames.Add(mockData[i].name);
            }

            _scrollView.totalCount = HomeScene.CachedActorNames.Count;
            _scrollView.RefillCells();
            return;
        }

        var stagesState = await GameStateSync.Instance.GetStagesState();
        if (stagesState == null)
        {
            Debug.LogError("[HomeScene] Failed to get stages state from server");
            HomeScene.CachedActorNames.Clear();
            _scrollView.totalCount = HomeScene.CachedActorNames.Count;
            _scrollView.RefillCells();
            return;
        }

        foreach (var kvp in stagesState)
        {
            if (kvp.Value.Contains(GameContext.Instance.PlayerActorName))
            {
                HomeScene.CachedActorNames = kvp.Value;
                break;
            }
        }

        HomeScene.CachedActorNames.Remove(GameContext.Instance.PlayerActorName); // 移除玩家角色自己
        Debug.Log($"Actors in current stage: {string.Join(", ", HomeScene.CachedActorNames)}");
        _scrollView.totalCount = HomeScene.CachedActorNames.Count;
        _scrollView.RefillCells();
    }

    /// <summary>
    /// 
    /// </summary>
    public void HideActorDetails()
    {
        _chatBubblePanel.SetActive(false);
        _currentActorIcon.gameObject.SetActive(false);
    }

    public void ShowActorDetails(string actorName)
    {
        Debug.Log($"[HomeScene] Actor clicked: {actorName}");

        // 更新显示
        _currentActorIcon.gameObject.SetActive(true);
        _chatBubblePanel.SetActive(true);

        // 更新UI显示选中的角色
        RefreshSelectedActorPortrait(actorName);

        // 根据选中角色刷新聊天气泡内容
        RefreshActorEventSummary(actorName);
    }

    /// <summary>
    /// 根据选中的角色名称更新当前角色图标显示
    /// </summary>
    /// <param name="actorName"></param>
    private void RefreshSelectedActorPortrait(string actorName)
    {
        Debug.Log($"[HomeScene] Handling selection for actor: {actorName}");

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
                SetChatBubble(string.Join("\n", agentEventSummaries));
            }
        }
        else
        {
            SetChatBubble(string.Empty);
        }
    }

    public void SetChatBubble(string content)
    {
        _chatBubblePanel.GetComponentInChildren<TMP_Text>().text = content;
    }
}
