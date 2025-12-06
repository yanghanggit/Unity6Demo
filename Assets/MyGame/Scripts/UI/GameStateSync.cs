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
    }

    /// <summary>
    /// 从服务器刷新场景映射关系
    /// 获取场景与演员的映射关系并更新到GameContext
    /// </summary>
    /// <param name="onComplete">完成回调，参数1为是否成功，参数2为消息</param>
    /// <returns>协程迭代器，成功返回true，失败返回false</returns>
    public IEnumerator RefreshMappingFromServer(Action<bool, string> onComplete = null)
    {
        if (string.IsNullOrEmpty(GameContext.Instance.UserName) || string.IsNullOrEmpty(GameContext.Instance.GameName) || string.IsNullOrEmpty(GameContext.Instance.ActorName))
        {
            string errorMsg = "[GameStateSync] UserName, GameName, or ActorName is not set in GameContext";
            Debug.LogError(errorMsg);
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        // 获取场景与演员映射关系
        yield return _stagesStateApi.Call(GameContext.Instance.StagesStateUrl);
        
        if (_stagesStateApi.ReqResult == null)
        {
            string errorMsg = "[GameStateSync] Failed to fetch stages state from server: request result is null";
            Debug.LogError(errorMsg);
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        if (!_stagesStateApi.ReqResult.isSuccess)
        {
            string errorMsg = $"[GameStateSync] Failed to fetch stages state from server: {_stagesStateApi.ReqResult.responseText}";
            Debug.LogError(errorMsg);
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        Debug.Assert(_stagesStateApi.RespData != null, "[GameStateSync] StagesStateApi response data is null");

        // 更新全局映射关系
        GameContext.Instance.StageActorMapping = _stagesStateApi.RespData.mapping;
        string successMsg = "[GameStateSync] Successfully refreshed stages mapping from server";
        Debug.Log(successMsg);
        onComplete?.Invoke(true, successMsg);
    }

    /// <summary>
    /// 从服务器刷新指定场景列表的详情数据
    /// 获取场景详情并更新到GameContext
    /// </summary>
    /// <param name="stages">需要获取详情的场景名称列表</param>
    /// <param name="onComplete">完成回调，参数1为是否成功，参数2为消息</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator RefreshStageDetailsFromServer(List<string> stages, Action<bool, string> onComplete = null)
    {
        if (stages == null || stages.Count == 0)
        {
            string warningMsg = "[GameStateSync] Stage list is empty, skip fetching stage details";
            Debug.LogWarning(warningMsg);
            onComplete?.Invoke(false, warningMsg);
            yield break;
        }

        // 获取场景详情数据
        yield return _entityDetailsApi.Call(GameContext.Instance.EntityDetailsUrl, stages);
        
        if (_entityDetailsApi.ReqResult == null)
        {
            string errorMsg = "[GameStateSync] Failed to fetch stage details from server: request result is null";
            Debug.LogError(errorMsg);
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        if (!_entityDetailsApi.ReqResult.isSuccess)
        {
            string errorMsg = $"[GameStateSync] Failed to fetch stage details from server: {_entityDetailsApi.ReqResult.responseText}";
            Debug.LogError(errorMsg);
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        Debug.Assert(_entityDetailsApi.RespData != null, "[GameStateSync] EntityDetailsApi response data is null");

        // 更新全局场景详情数据
        GameContext.Instance.StageEntitiesSerialization = _entityDetailsApi.RespData.entities_serialization;

        string successMsg = $"[GameStateSync] Successfully refreshed {stages.Count} stage details from server";
        Debug.Log(successMsg);
        var stageEntitiesSerialization = GameContext.Instance.StageEntitiesSerialization;
        for (int i = 0; i < stageEntitiesSerialization.Count; i++)
        {
            var entitySerialization = stageEntitiesSerialization[i];
            try
            {
                // 直接将 EntitySerialization 序列化为 JSON 字符串
                string jsonString = JsonConvert.SerializeObject(entitySerialization, Formatting.Indented);
                Debug.Log($"Stage[{i}] JSON:\n{jsonString}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to serialize Stage[{i}] to JSON: {ex.Message}");
            }
        }
        onComplete?.Invoke(true, successMsg);
    }

    /// <summary>
    /// 从服务器刷新指定演员列表的详情数据
    /// 获取演员详情并更新到GameContext
    /// </summary>
    /// <param name="actors">需要获取详情的演员名称列表</param>
    /// <param name="onComplete">完成回调，参数1为是否成功，参数2为消息</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator RefreshActorDetailsFromServer(List<string> actors, Action<bool, string> onComplete = null)
    {
        if (actors == null || actors.Count == 0)
        {
            string warningMsg = "[GameStateSync] Actor list is empty, skip fetching actor details";
            Debug.LogWarning(warningMsg);
            onComplete?.Invoke(false, warningMsg);
            yield break;
        }

        // 获取演员详情数据
        yield return _entityDetailsApi.Call(GameContext.Instance.EntityDetailsUrl, actors);
        
        if (_entityDetailsApi.ReqResult == null)
        {
            string errorMsg = "[GameStateSync] Failed to fetch actor details from server: request result is null";
            Debug.LogError(errorMsg);
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        if (!_entityDetailsApi.ReqResult.isSuccess)
        {
            string errorMsg = $"[GameStateSync] Failed to fetch actor details from server: {_entityDetailsApi.ReqResult.responseText}";
            Debug.LogError(errorMsg);
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        Debug.Assert(_entityDetailsApi.RespData != null, "[GameStateSync] EntityDetailsApi response data is null");

        // 更新全局演员详情数据
        GameContext.Instance.ActorEntitiesSerialization = _entityDetailsApi.RespData.entities_serialization;

        string successMsg = $"[GameStateSync] Successfully refreshed {actors.Count} actor details from server";
        Debug.Log(successMsg);

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
        onComplete?.Invoke(true, successMsg);
    }

    /// <summary>
    /// 从服务器刷新场景映射关系及所有实体详情数据
    /// 执行完整的游戏状态同步，依次获取：
    /// 1. 场景与演员的映射关系(Mapping)
    /// 2. 所有演员的详细信息(ActorEntitiesSerialization)
    /// 3. 所有场景的详细信息(StageEntitiesSerialization)
    /// 适用于需要获取完整游戏状态的场景，如初始化、场景切换等
    /// </summary>
    /// <param name="onComplete">完成回调，参数1为是否成功，参数2为消息</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator RefreshMappingAndEntitiesFromServer(Action<bool, string> onComplete = null)
    {
        bool stepSuccess = true;
        string stepMessage = "";

        // 步骤1: 刷新场景映射关系
        yield return RefreshMappingFromServer((success, msg) => 
        {
            stepSuccess = success;
            stepMessage = msg;
        });

        if (!stepSuccess)
        {
            onComplete?.Invoke(false, $"RefreshMappingAndEntitiesFromServer failed at step 1: {stepMessage}");
            yield break;
        }

        // 临时测试，将  GameContext.Instance.Mapping 与 GameContext.Instance.AllActors 打印出来
        Debug.Log("[GameStateSync] Current Mapping:");
        foreach (var kvp in GameContext.Instance.StageActorMapping)
        {
            Debug.Log($"Stage: {kvp.Key}, Actors: {string.Join(", ", kvp.Value)}");
        }
        Debug.Log("[GameStateSync] All Actors: " + string.Join(", ", GameContext.Instance.AllActors));

        // 步骤2: 刷新所有演员的详情数据
        yield return RefreshActorDetailsFromServer(GameContext.Instance.AllActors, (success, msg) => 
        {
            stepSuccess = success;
            stepMessage = msg;
        });

        if (!stepSuccess)
        {
            onComplete?.Invoke(false, $"RefreshMappingAndEntitiesFromServer failed at step 2: {stepMessage}");
            yield break;
        }

        // 步骤3: 获取场景详情数据
        yield return RefreshStageDetailsFromServer(GameContext.Instance.AllStages, (success, msg) => 
        {
            stepSuccess = success;
            stepMessage = msg;
        });

        if (!stepSuccess)
        {
            onComplete?.Invoke(false, $"RefreshMappingAndEntitiesFromServer failed at step 3: {stepMessage}");
            yield break;
        }

        onComplete?.Invoke(true, "RefreshMappingAndEntitiesFromServer completed successfully");
    }


    /// <summary>
    /// 从服务器刷新场景映射关系及演员详情数据
    /// 执行部分游戏状态同步，依次获取：
    /// 1. 场景与演员的映射关系(Mapping)
    /// 2. 所有演员的详细信息(ActorEntitiesSerialization)
    /// 相比RefreshStagesMappingAndEntitiesFromServer，此方法不获取场景详情，适用于只需要演员数据的场景
    /// </summary>
    /// <param name="onComplete">完成回调，参数1为是否成功，参数2为消息</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator RefreshMappingAndActorsFromServer(Action<bool, string> onComplete = null)
    {
        bool stepSuccess = true;
        string stepMessage = "";

        // 步骤1: 刷新场景映射关系
        yield return RefreshMappingFromServer((success, msg) => 
        {
            stepSuccess = success;
            stepMessage = msg;
        });

        if (!stepSuccess)
        {
            onComplete?.Invoke(false, $"RefreshMappingAndActorsFromServer failed at step 1: {stepMessage}");
            yield break;
        }

        // 临时测试，将  GameContext.Instance.Mapping 与 GameContext.Instance.AllActors 打印出来
        Debug.Log("[GameStateSync] Current Mapping:");
        foreach (var kvp in GameContext.Instance.StageActorMapping)
        {
            Debug.Log($"Stage: {kvp.Key}, Actors: {string.Join(", ", kvp.Value)}");
        }
        Debug.Log("[GameStateSync] All Actors: " + string.Join(", ", GameContext.Instance.AllActors));

        // 步骤2: 刷新所有演员的详情数据
        yield return RefreshActorDetailsFromServer(GameContext.Instance.AllActors, (success, msg) => 
        {
            stepSuccess = success;
            stepMessage = msg;
        });

        if (!stepSuccess)
        {
            onComplete?.Invoke(false, $"RefreshMappingAndActorsFromServer failed at step 2: {stepMessage}");
            yield break;
        }

        onComplete?.Invoke(true, "RefreshMappingAndActorsFromServer completed successfully");
    }


    /// <summary>
    /// 从服务器刷新地下城数据
    /// 获取地下城的映射关系和地下城详细信息
    /// </summary>
    /// <param name="onComplete">完成回调，参数1为是否成功，参数2为消息</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator RefreshDungeonFromServer(Action<bool, string> onComplete = null)
    {
        if (string.IsNullOrEmpty(GameContext.Instance.UserName) || string.IsNullOrEmpty(GameContext.Instance.GameName) || string.IsNullOrEmpty(GameContext.Instance.ActorName))
        {
            string errorMsg = "[GameStateSync] UserName, GameName, or ActorName is not set in GameContext";
            Debug.LogError(errorMsg);
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        yield return _dungeonStateApi.Call(GameContext.Instance.DungeonStateUrl);
        
        if (_dungeonStateApi.ReqResult == null)
        {
            string errorMsg = "[GameStateSync] Failed to fetch dungeon state from server: request result is null";
            Debug.LogError(errorMsg);
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        if (!_dungeonStateApi.ReqResult.isSuccess)
        {
            string errorMsg = $"[GameStateSync] Failed to fetch dungeon state from server: {_dungeonStateApi.ReqResult.responseText}";
            Debug.LogError(errorMsg);
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        Debug.Assert(_dungeonStateApi.RespData != null, "[GameStateSync] DungeonStateApi response data is null");

        // 更新全局地下城数据
        GameContext.Instance.StageActorMapping = _dungeonStateApi.RespData.mapping;
        GameContext.Instance.Dungeon = _dungeonStateApi.RespData.dungeon;

        string successMsg = "[GameStateSync] Successfully refreshed dungeon state from server";
        Debug.Log(successMsg);
        onComplete?.Invoke(true, successMsg);
    }

    /// <summary>
    /// 从服务器刷新地下城与演员数据
    /// 依次获取：1. 地下城状态和映射关系  2. 当前场景中所有演员的详细信息
    /// </summary>
    /// <param name="onComplete">完成回调，参数1为是否成功，参数2为消息</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator RefreshDungeonAndActorsFromServer(Action<bool, string> onComplete = null)
    {
        bool stepSuccess = true;
        string stepMessage = "";

        // 步骤1: 刷新地下城数据
        yield return RefreshDungeonFromServer((success, msg) => 
        {
            stepSuccess = success;
            stepMessage = msg;
        });

        if (!stepSuccess)
        {
            onComplete?.Invoke(false, $"RefreshDungeonAndActorsFromServer failed at step 1: {stepMessage}");
            yield break;
        }

        // 步骤2: 获取当前演员所在场景的所有演员列表
        var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);
        if (string.IsNullOrEmpty(stageName))
        {
            string errorMsg = "[GameStateSync] Current actor's stage not found in mapping";
            Debug.LogError(errorMsg);
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        var actorsInStage = GameContext.Instance.GetActorsInStage(stageName);
        if (actorsInStage.Count == 0)
        {
            string warningMsg = "[GameStateSync] No actors found in the current actor's stage";
            Debug.LogWarning(warningMsg);
            onComplete?.Invoke(false, warningMsg);
            yield break;
        }

        // 步骤3: 刷新当前场景中所有演员的详情数据
        yield return RefreshActorDetailsFromServer(actorsInStage, (success, msg) => 
        {
            stepSuccess = success;
            stepMessage = msg;
        });

        if (!stepSuccess)
        {
            onComplete?.Invoke(false, $"RefreshDungeonAndActorsFromServer failed at step 3: {stepMessage}");
            yield break;
        }

        onComplete?.Invoke(true, "RefreshDungeonAndActorsFromServer completed successfully");
    }
}


