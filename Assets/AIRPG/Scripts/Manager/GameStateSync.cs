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

    /// <summary>
    /// 场景状态API接口
    /// </summary>
    public StagesStateApi _stagesStateApi;

    /// <summary>
    /// 演员详情API接口
    /// </summary>
    public EntityDetailsApi _entityDetailsApi;

    /// <summary>
    /// 地下城状态API接口
    /// </summary>
    public DungeonStateApi _dungeonStateApi;

    /// <summary>
    /// 地下城战斗状态API接口
    /// </summary>
    public DungeonCombatApi _dungeonCombatApi;

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
        Debug.Assert(_stagesStateApi != null, "_stagesStateApi is null");
        Debug.Assert(_entityDetailsApi != null, "_actorDetailApi is null");
        Debug.Assert(_dungeonStateApi != null, "_dungeonStateApi is null");
        Debug.Assert(_dungeonCombatApi != null, "_dungeonCombatApi is null");
    }

    /// <summary>
    /// 从服务器获取场景-演员映射关系
    /// </summary>
    /// <returns>成功时返回 Dictionary&lt;string, List&lt;string&gt;&gt;（场景名 → 演员名列表），失败时返回 null</returns>
    public async UniTask<Dictionary<string, List<string>>> GetStagesState()
    {
        if (string.IsNullOrEmpty(GameContext.Instance.UserName) || string.IsNullOrEmpty(GameContext.Instance.GameName) || string.IsNullOrEmpty(GameContext.Instance.PlayerActorName))
        {
            Debug.LogError("[GameStateSync] UserName, GameName, or ActorName is not set in GameContext");
            return null;
        }

        // 获取场景与演员映射关系
        await _stagesStateApi.Call(GameContext.Instance.StagesStateUrl);

        if (_stagesStateApi.ReqResult == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch stages state from server: request result is null");
            return null;
        }

        if (!_stagesStateApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[GameStateSync] Failed to fetch stages state from server: {_stagesStateApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_stagesStateApi.RespData != null, "[GameStateSync] StagesStateApi response data is null");
        return _stagesStateApi.RespData.mapping;
    }

    /// <summary>
    /// 从服务器获取指定实体列表的详情数据
    /// </summary>
    /// <param name="entityNames">需要获取详情的实体名称列表（可包含演员或场景）</param>
    /// <returns>成功时返回所有实体的浅拷贝列表（List&lt;EntitySerialization&gt;），失败时返回 null</returns>
    public async UniTask<List<EntitySerialization>> GetEntities(List<string> entityNames)
    {
        if (entityNames == null || entityNames.Count == 0)
        {
            Debug.LogWarning("[GameStateSync] Entity list is empty, skip fetching entity details");
            return null;
        }

        // 获取实体详情数据（可包含演员和场景两类实体）
        await _entityDetailsApi.Call(GameContext.Instance.EntityDetailsUrl, entityNames);

        if (_entityDetailsApi.ReqResult == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch entity details from server: request result is null");
            return null;
        }

        if (!_entityDetailsApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[GameStateSync] Failed to fetch entity details from server: {_entityDetailsApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_entityDetailsApi.RespData != null, "[GameStateSync] EntityDetailsApi response data is null");
        return new List<EntitySerialization>(_entityDetailsApi.RespData.entities_serialization);
    }

    /// <summary>
    /// 从服务器获取地下城数据
    /// </summary>
    /// <returns>成功时返回 <see cref="Dungeon"/>，失败时返回 null</returns>
    public async UniTask<Dungeon> GetDungeon()
    {
        if (string.IsNullOrEmpty(GameContext.Instance.UserName) || string.IsNullOrEmpty(GameContext.Instance.GameName) || string.IsNullOrEmpty(GameContext.Instance.PlayerActorName))
        {
            Debug.LogError("[GameStateSync] UserName, GameName, or ActorName is not set in GameContext");
            return null;
        }

        await _dungeonStateApi.Call(GameContext.Instance.DungeonStateUrl);

        if (_dungeonStateApi.ReqResult == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch dungeon state from server: request result is null");
            return null;
        }

        if (!_dungeonStateApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[GameStateSync] Failed to fetch dungeon state from server: {_dungeonStateApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_dungeonStateApi.RespData != null, "[GameStateSync] DungeonStateApi response data is null");
        return _dungeonStateApi.RespData.dungeon;
    }

    /// <summary>
    /// 从服务器获取当前地下城战斗状态
    /// </summary>
    /// <returns>成功时返回 <see cref="Combat"/>，失败时返回 null</returns>
    public async UniTask<Combat> GetCombat()
    {
        if (string.IsNullOrEmpty(GameContext.Instance.UserName) || string.IsNullOrEmpty(GameContext.Instance.GameName) || string.IsNullOrEmpty(GameContext.Instance.PlayerActorName))
        {
            Debug.LogError("[GameStateSync] UserName, GameName, or ActorName is not set in GameContext");
            return null;
        }

        await _dungeonCombatApi.Call(GameContext.Instance.DungeonCombatUrl);

        if (_dungeonCombatApi.ReqResult == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch dungeon combat state from server: request result is null");
            return null;
        }

        if (!_dungeonCombatApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[GameStateSync] Failed to fetch dungeon combat state from server: {_dungeonCombatApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_dungeonCombatApi.RespData != null, "[GameStateSync] DungeonCombatApi response data is null");
        return _dungeonCombatApi.RespData.combat;
    }
}