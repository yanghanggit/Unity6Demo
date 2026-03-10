using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// 游戏状态同步管理器
/// 负责从服务器拉取游戏状态数据，包括场景-演员映射、实体详情、地下城状态及战斗状态
/// 使用单例模式，可被频繁调用以保持客户端与服务器数据同步
/// </summary>
public class GameStateSync : MonoBehaviour
{
    //GameStateUpdated
    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onGameStateUpdatedEvent; // 游戏状态更新事件，携带最新的 GameContext 数据

    /// <summary>
    /// 单例实例
    /// </summary>
    public static GameStateSync Instance { get; private set; }

    private int _lastSessionSequenceId = 0;
    private bool _isSessionPolling = false;

    private void Awake()
    {
        // 单例模式处理
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("[GameStateSync] Duplicate instance detected, destroying the new one.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Assert(_onGameStateUpdatedEvent != null, "_onGameStateUpdatedEvent is not assigned in the inspector.");

        // 由同步管理器自行启动会话轮询，调用方无需感知内部细节。
        ResetSessionMessageCursor();
        StartSessionMessagesPolling(destroyCancellationToken).Forget();
    }

    /// <summary>
    /// 创建一个独立的临时 API 实例，挂载于自身 Transform 下。
    /// 每次调用均产生隔离对象，避免并发时共享 ReqResult / RespData 导致竞态。
    /// 调用方负责在使用完毕后通过 finally 块 Destroy 该实例的 gameObject。
    /// </summary>
    private T CreateApi<T>() where T : BaseApiClient
    {
        var go = new GameObject(typeof(T).Name);
        go.transform.SetParent(transform);
        go.hideFlags = HideFlags.HideInHierarchy;
        return go.AddComponent<T>();
    }

    /// <summary>
    /// 重置会话消息拉取游标，通常在进入 HomeScene 时调用。
    /// </summary>
    public void ResetSessionMessageCursor(int lastSequenceId = 0)
    {
        _lastSessionSequenceId = Mathf.Max(0, lastSequenceId);
    }

    /// <summary>
    /// 启动会话消息轮询。若已在轮询中，则忽略重复启动。
    /// </summary>
    public async UniTask StartSessionMessagesPolling(CancellationToken cancellationToken, int intervalMs = 3000)
    {
        if (_isSessionPolling)
        {
            Debug.LogWarning("[GameStateSync] Session polling is already running, skip duplicate start.");
            return;
        }

        _isSessionPolling = true;
        try
        {
            while (true)
            {
                await FetchPlayerSessionMessages();
                bool cancelled = await UniTask.Delay(intervalMs, ignoreTimeScale: true, cancellationToken: cancellationToken).SuppressCancellationThrow();
                if (cancelled)
                {
                    break;
                }
            }
        }
        finally
        {
            _isSessionPolling = false;
        }
    }

    /// <summary>
    /// 从服务器拉取玩家会话消息，并更新本地最新序列号。
    /// </summary>
    private async UniTask FetchPlayerSessionMessages()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[GameStateSync] Player is not logged in, cannot fetch session messages");
            return;
        }

        var (sessionMessages, stagesState) = await UniTask.WhenAll(
            GetSessionMessages(_lastSessionSequenceId),
            GetStagesState()
        );

        if (sessionMessages == null || stagesState == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch session messages or stages state from server");
            return;
        }

        // 保存旧状态用于比较
        var oldStagesState = GameContext.Instance.StagesState;
        bool stagesStateChanged = !AreStagesStateEqual(oldStagesState, stagesState);
        bool hasNewMessages = sessionMessages.Count > 0;

        // 更新全局场景-演员映射状态，供其他模块查询使用
        GameContext.Instance.StagesState = stagesState;

        // 处理会话消息，更新本地事件历史和最新序列ID
        if (hasNewMessages)
        {
            GameContext.Instance.CollectEventsByActor(sessionMessages);
            _lastSessionSequenceId = sessionMessages[^1].sequence_id;
        }

        // 只有在状态变化或收到新消息时才触发事件
        if (stagesStateChanged || hasNewMessages)
        {
            var eventData = new UIEventData(UIEventType.GameStateUpdated);
            _onGameStateUpdatedEvent.Raise(eventData);
            Debug.Log($"[GameStateSync] Game state updated - StagesChanged: {stagesStateChanged}, NewMessages: {hasNewMessages}");
        }
    }

    /// <summary>
    /// 比较两个 StagesState 字典是否相等（深度比较）
    /// </summary>
    private bool AreStagesStateEqual(Dictionary<string, List<string>> state1, Dictionary<string, List<string>> state2)
    {
        // 如果两者都为 null，认为相等
        if (state1 == null && state2 == null) return true;
        // 如果只有一个为 null，不相等
        if (state1 == null || state2 == null) return false;
        // 如果键数量不同，不相等
        if (state1.Count != state2.Count) return false;

        // 逐个比较每个键值对
        foreach (var kvp in state1)
        {
            if (!state2.TryGetValue(kvp.Key, out var list2)) return false;

            var list1 = kvp.Value;
            // 比较列表内容
            if (list1 == null && list2 == null) continue;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            // 比较列表元素（假设顺序可能不同，使用集合比较）
            var set1 = new HashSet<string>(list1);
            var set2 = new HashSet<string>(list2);
            if (!set1.SetEquals(set2)) return false;
        }

        return true;
    }

    /// <summary>
    /// 从服务器获取场景-演员映射关系
    /// </summary>
    /// <returns>成功时返回 Dictionary&lt;string, List&lt;string&gt;&gt;（场景名 → 演员名列表），失败时返回 null</returns>
    public async UniTask<Dictionary<string, List<string>>> GetStagesState()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[GameStateSync] Player is not logged in, skip fetching stages state from server");
            return null;
        }

        // 每次创建独立实例，避免并发时共享状态导致竞态
        var api = CreateApi<StagesStateApi>();
        try
        {
            await api.Call(GameContext.Instance.StagesStateUrl);

            if (api.ReqResult == null)
            {
                Debug.LogError("[GameStateSync] Failed to fetch stages state from server: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[GameStateSync] Failed to fetch stages state from server: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "[GameStateSync] StagesStateApi response data is null");
            return api.RespData.mapping;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 从服务器获取指定实体列表的详情数据
    /// </summary>
    /// <param name="entityNames">需要获取详情的实体名称列表（可包含演员或场景）</param>
    /// <returns>成功时返回所有实体的浅拷贝列表（List&lt;EntitySerialization&gt;），失败时返回 null</returns>
    public async UniTask<List<EntitySerialization>> GetEntities(List<string> entityNames)
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[GameStateSync] Player is not logged in, skip fetching entity details from server");
            return null;
        }

        if (entityNames == null || entityNames.Count == 0)
        {
            Debug.LogWarning("[GameStateSync] Entity list is empty, skip fetching entity details");
            return null;
        }

        // 每次创建独立实例，避免并发时共享状态导致竞态
        var api = CreateApi<EntityDetailsApi>();
        try
        {
            await api.Call(GameContext.Instance.EntityDetailsUrl, entityNames);

            if (api.ReqResult == null)
            {
                Debug.LogError("[GameStateSync] Failed to fetch entity details from server: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[GameStateSync] Failed to fetch entity details from server: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "[GameStateSync] EntityDetailsApi response data is null");
            return new List<EntitySerialization>(api.RespData.entities_serialization);
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 从服务器获取地下城数据
    /// </summary>
    /// <returns>成功时返回 <see cref="Dungeon"/>，失败时返回 null</returns>
    public async UniTask<Dungeon> GetDungeon()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[GameStateSync] Player is not logged in, skip fetching dungeon state from server");
            return null;
        }

        // 每次创建独立实例，避免并发时共享状态导致竞态
        var api = CreateApi<DungeonStateApi>();
        try
        {
            await api.Call(GameContext.Instance.DungeonStateUrl);

            if (api.ReqResult == null)
            {
                Debug.LogError("[GameStateSync] Failed to fetch dungeon state from server: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[GameStateSync] Failed to fetch dungeon state from server: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "[GameStateSync] DungeonStateApi response data is null");
            return api.RespData.dungeon;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 从服务器获取当前地下城战斗状态
    /// </summary>
    /// <returns>成功时返回 <see cref="Combat"/>，失败时返回 null</returns>
    public async UniTask<Combat> GetCombat()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[GameStateSync] Player is not logged in, skip fetching combat state from server");
            return null;
        }

        // 每次创建独立实例，避免并发时共享状态导致竞态
        var api = CreateApi<DungeonCombatApi>();
        try
        {
            await api.Call(GameContext.Instance.DungeonCombatUrl);

            if (api.ReqResult == null)
            {
                Debug.LogError("[GameStateSync] Failed to fetch dungeon combat state from server: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[GameStateSync] Failed to fetch dungeon combat state from server: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "[GameStateSync] DungeonCombatApi response data is null");
            return api.RespData.combat;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 从服务器获取会话消息，并更新序列ID
    /// </summary>
    /// <returns>成功时返回会话消息列表，失败时返回 null</returns>
    public async UniTask<List<SessionMessage>> GetSessionMessages(int lastSequenceId)
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[GameStateSync] Player is not logged in, skip fetching session messages from server");
            return null;
        }

        // 每次创建独立实例，避免并发时共享状态导致竞态
        var api = CreateApi<SessionMessagesApi>();
        try
        {
            await api.Call(
                GameContext.Instance.SessionMessagesUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName,
                lastSequenceId
            );

            if (api.ReqResult == null)
            {
                Debug.LogError("[GameStateSync] Failed to fetch session messages from server: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[GameStateSync] Failed to fetch session messages from server: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "[GameStateSync] SessionMessagesApi response data is null");

            // 更新最后一个序列ID
            return new List<SessionMessage>(api.RespData.session_messages);
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }
}