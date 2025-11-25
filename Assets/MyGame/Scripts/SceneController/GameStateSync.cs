using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

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
    public EntityDetailsApi _entityDetailsApi;

    /// <summary>
    /// 地下城状态API接口
    /// </summary>
    public DungeonStateApi _dungeonStateApi;

    /// <summary>
    /// 会话消息API接口
    /// 用于从服务器获取游戏会话消息列表，支持基于序列ID的增量拉取
    /// </summary>
    public SessionMessagesApi _sessionMessagesApi;

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
        Debug.Assert(_entityDetailsApi != null, "_actorDetailApi is null");
        Debug.Assert(_dungeonStateApi != null, "_dungeonStateApi is null");
        Debug.Assert(_sessionMessagesApi != null, "_sessionMessagesApi is null");
    }

    /// <summary>
    /// 从服务器刷新场景映射关系
    /// 获取场景与演员的映射关系并更新到GameContext
    /// </summary>
    /// <returns>协程迭代器，成功返回true，失败返回false</returns>
    public IEnumerator RefreshStagesMappingFromServer()
    {
        if (_stagesStateApi == null)
        {
            Debug.LogError("[GameStateSync] StagesStateApi is not initialized");
            yield break;
        }

        if (string.IsNullOrEmpty(GameContext.Instance.UserName) || string.IsNullOrEmpty(GameContext.Instance.GameName) || string.IsNullOrEmpty(GameContext.Instance.ActorName))
        {
            Debug.LogError("[GameStateSync] UserName, GameName, or ActorName is not set in GameContext");
            yield break;
        }

        // 获取场景与演员映射关系
        yield return _stagesStateApi.Call(GameContext.Instance.StagesStateUrl);
        if (_stagesStateApi.RespData == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch stages state from server");
            yield break;
        }

        // 更新全局映射关系
        GameContext.Instance.Mapping = _stagesStateApi.RespData.mapping;
        Debug.Log("[GameStateSync] Successfully refreshed stages mapping from server");


        // yield return _entityDetailsApi.Call(GameContext.Instance.EntityDetailsUrl, GameContext.Instance.AllStages);
        // if (_entityDetailsApi.RespData == null)
        // {
        //     Debug.LogError("[GameStateSync] Failed to fetch actor details from server");
        //     yield break;
        // }

        // // 更新全局演员详情数据
        // var stageEntitiesSerialization = _entityDetailsApi.RespData.entities_serialization;
        // Debug.Log($"[GameStateSync] Successfully refreshed {stageEntitiesSerialization.Count} stage details from server");

    }

    /// <summary>
    /// 从服务器刷新指定演员列表的详情数据
    /// 获取演员详情并更新到GameContext
    /// </summary>
    /// <param name="actors">需要获取详情的演员名称列表</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator RefreshActorDetailsFromServer(List<string> actors)
    {
        if (_entityDetailsApi == null)
        {
            Debug.LogError("[GameStateSync] ActorDetailsApi is not initialized");
            yield break;
        }

        if (actors == null || actors.Count == 0)
        {
            Debug.LogWarning("[GameStateSync] Actor list is empty, skip fetching actor details");
            yield break;
        }

        // 获取演员详情数据
        yield return _entityDetailsApi.Call(GameContext.Instance.EntityDetailsUrl, actors);
        if (_entityDetailsApi.RespData == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch actor details from server");
            yield break;
        }

        // 更新全局演员详情数据
        GameContext.Instance.ActorEntitiesSerialization = _entityDetailsApi.RespData.entities_serialization;

        Debug.Log($"[GameStateSync] Successfully refreshed {actors.Count} actor details from server");

        // 打印演员详情的调试信息
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
            catch (Exception ex)
            {
                Debug.LogError($"Failed to serialize Actor[{i}] to JSON: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 从服务器刷新场景与演员数据
    /// 依次获取：1. 场景与演员的映射关系  2. 所有演员的详细信息
    /// </summary>
    /// <returns>协程迭代器</returns>
    public IEnumerator RefreshStagesAndActorsFromServer()
    {
        // 步骤1: 刷新场景映射关系
        yield return RefreshStagesMappingFromServer();

        // 临时测试，将  GameContext.Instance.Mapping 与 GameContext.Instance.AllActors 打印出来
        Debug.Log("[GameStateSync] Current Mapping:");
        foreach (var kvp in GameContext.Instance.Mapping)
        {
            Debug.Log($"Stage: {kvp.Key}, Actors: {string.Join(", ", kvp.Value)}");
        }
        Debug.Log("[GameStateSync] All Actors: " + string.Join(", ", GameContext.Instance.AllActors));

        // 步骤2: 刷新所有演员的详情数据
        yield return RefreshActorDetailsFromServer(GameContext.Instance.AllActors);
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

        if (string.IsNullOrEmpty(GameContext.Instance.UserName) || string.IsNullOrEmpty(GameContext.Instance.GameName) || string.IsNullOrEmpty(GameContext.Instance.ActorName))
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

    /// <summary>
    /// 从服务器刷新地下城与演员数据
    /// 依次获取：1. 地下城状态和映射关系  2. 当前场景中所有演员的详细信息
    /// </summary>
    /// <returns>协程迭代器</returns>
    public IEnumerator RefreshDungeonAndActorsFromServer()
    {
        // 步骤1: 刷新地下城数据
        yield return RefreshDungeonFromServer();

        // 步骤2: 获取当前演员所在场景的所有演员列表
        var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);
        if (string.IsNullOrEmpty(stageName))
        {
            Debug.LogError("[GameStateSync] Current actor's stage not found in mapping");
            yield break;
        }

        var actorsInStage = GameContext.Instance.GetActorsInStage(stageName);
        if (actorsInStage.Count == 0)
        {
            Debug.LogWarning("[GameStateSync] No actors found in the current actor's stage");
            yield break;
        }

        // 步骤3: 刷新当前场景中所有演员的详情数据
        yield return RefreshActorDetailsFromServer(actorsInStage);
    }


    /// <summary>
    /// 从服务器获取会话消息
    /// 获取最新的会话消息列表并更新序列ID
    /// </summary>
    /// <param name="onMessagesReceived">回调函数，用于接收获取到的会话消息列表</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator FetchSessionMessagesFromServer(Action<List<SessionMessage>> onMessagesReceived)
    {
        if (_sessionMessagesApi == null)
        {
            Debug.LogError("[GameStateSync] SessionMessagesApi is not initialized");
            yield break;
        }

        if (string.IsNullOrEmpty(GameContext.Instance.UserName) ||
            string.IsNullOrEmpty(GameContext.Instance.GameName))
        {
            Debug.LogError("[GameStateSync] UserName or GameName is not set in GameContext");
            yield break;
        }

        // 获取会话消息
        yield return _sessionMessagesApi.Call(GameContext.Instance.SessionMessagesUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            GameContext.Instance.LastSequenceId);

        if (_sessionMessagesApi.RespData == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch session messages from server");
            yield break;
        }

        // 更新最后一个序列ID
        if (_sessionMessagesApi.RespLastSequenceId >= 0)
        {
            GameContext.Instance.LastSequenceId = _sessionMessagesApi.RespLastSequenceId;
        }

        // 复制会话消息列表
        var sessionMessages = new List<SessionMessage>(_sessionMessagesApi.RespData.session_messages);

        Debug.Log($"[GameStateSync] Successfully fetched {sessionMessages.Count} session messages from server");

        // 通过回调返回消息列表
        onMessagesReceived?.Invoke(sessionMessages);
    }
}
