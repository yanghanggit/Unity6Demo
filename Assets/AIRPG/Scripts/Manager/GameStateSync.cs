using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>
/// 游戏状态同步管理器
/// 负责从服务器拉取游戏状态数据，包括场景-演员映射、实体详情、地下城状态及战斗状态
/// 使用单例模式，可被频繁调用以保持客户端与服务器数据同步
/// </summary>
public class GameStateSync : MonoBehaviour
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static GameStateSync Instance { get; private set; }

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
    public async UniTask<List<SessionMessage>> GetSessionMessages()
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
                GameContext.Instance.LastSequenceId
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
            if (api.RespLastSequenceId >= 0)
            {
                GameContext.Instance.LastSequenceId = api.RespLastSequenceId;
            }

            return new List<SessionMessage>(api.RespData.session_messages);
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }
}