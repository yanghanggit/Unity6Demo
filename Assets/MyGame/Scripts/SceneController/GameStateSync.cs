using System.Collections;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 游戏状态同步管理器
/// 负责从服务器刷新游戏状态数据，包括场景-演员映射关系和演员详情
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
    public ActorDetailsApi _actorDetailApi;

    /// <summary>
    /// 地下城状态API接口
    /// </summary>
    public DungeonStateApi _dungeonStateApi;

    private void Awake()
    {
        // 单例模式处理
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        Debug.Assert(_stagesStateApi != null, "_stagesStateApi is null");
        Debug.Assert(_actorDetailApi != null, "_actorDetailApi is null");
        Debug.Assert(_dungeonStateApi != null, "_dungeonStateApi is null");
    }

    /// <summary>
    /// 从服务器刷新场景与演员数据
    /// 依次获取：1. 场景与演员的映射关系  2. 所有演员的详细信息
    /// </summary>
    /// <returns>协程迭代器</returns>
    public IEnumerator RefreshStagesAndActorsFromServer()
    {
        if (_stagesStateApi == null || _actorDetailApi == null)
        {
            Debug.LogError("[GameStateSync] APIs are not initialized");
            yield break;
        }

        if (GameContext.Instance.UserName == "" || GameContext.Instance.GameName == "" || GameContext.Instance.ActorName == "")
        {
            Debug.LogError("[GameStateSync] UserName, GameName, or ActorName is not set in GameContext");
            yield break;
        }

        // 步骤1: 获取场景与演员映射关系
        yield return _stagesStateApi.Call(GameContext.Instance.HomeStateUrl);
        if (_stagesStateApi.RespData == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch stages state from server");
            yield break;
        }

        // 更新全局映射关系
        GameContext.Instance.Mapping = _stagesStateApi.RespData.mapping;

        // 临时测试，将  GameContext.Instance.Mapping 与 GameContext.Instance.AllActors 打印出来
        Debug.Log("[GameStateSync] Current Mapping:");
        foreach (var kvp in GameContext.Instance.Mapping)
        {
            Debug.Log($"Stage: {kvp.Key}, Actors: {string.Join(", ", kvp.Value)}");
        }
        Debug.Log("[GameStateSync] All Actors: " + string.Join(", ", GameContext.Instance.AllActors));

        // 步骤2: 获取所有演员的详情数据
        yield return _actorDetailApi.Call(GameContext.Instance.ActorDetailsUrl, GameContext.Instance.AllActors);
        if (_actorDetailApi.RespData == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch actor details from server");
            yield break;
        }

        // 更新全局演员详情数据
        GameContext.Instance.ActorEntitiesSerialization = _actorDetailApi.RespData.actor_entities_serialization;

        Debug.Log("[GameStateSync] Successfully refreshed game state from server");

        // 打印 GameContext.Instance.ActorEntitiesSerialization 的详细信息
        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;
        for (int i = 0; i < actorEntitiesSerialization.Count; i++)
        {
            var entitySerialization = actorEntitiesSerialization[i];
            try
            {
                // 直接将 EntitySerialization 序列化为 JSON 字符串
                string jsonString = JsonConvert.SerializeObject(entitySerialization, Formatting.Indented);
                Debug.Log($"Actor[{i}] JSON:\n{jsonString}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to serialize Actor[{i}] to JSON: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 从服务器刷新地下城数据
    /// 获取地下城的映射关系和地下城详细信息
    /// </summary>
    /// <returns>协程迭代器</returns>
    public IEnumerator RefreshDungeonFromServer()
    {
        if (_dungeonStateApi == null)
        {
            Debug.LogError("[GameStateSync] DungeonStateApi is not initialized");
            yield break;
        }

        if (GameContext.Instance.UserName == "" || GameContext.Instance.GameName == "" || GameContext.Instance.ActorName == "")
        {
            Debug.LogError("[GameStateSync] UserName, GameName, or ActorName is not set in GameContext");
            yield break;
        }

        yield return _dungeonStateApi.Call(GameContext.Instance.DungeonStateUrl);
        if (_dungeonStateApi.RespData == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch dungeon state from server");
            yield break;
        }

        // 更新全局地下城数据
        GameContext.Instance.Mapping = _dungeonStateApi.RespData.mapping;
        GameContext.Instance.Dungeon = _dungeonStateApi.RespData.dungeon;

        Debug.Log("[GameStateSync] Successfully refreshed dungeon state from server");
    }
}
