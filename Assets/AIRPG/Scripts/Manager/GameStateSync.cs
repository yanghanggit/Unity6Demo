using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;



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
    }

    /// <summary>
    /// 从服务器刷新场景映射关系
    /// 获取场景与演员的映射关系并更新到GameContext
    /// </summary>
    /// <returns>是否成功</returns>
    public async UniTask<bool> RefreshMappingFromServer()
    {
        if (string.IsNullOrEmpty(GameContext.Instance.UserName) || string.IsNullOrEmpty(GameContext.Instance.GameName) || string.IsNullOrEmpty(GameContext.Instance.PlayerActor))
        {
            Debug.LogError("[GameStateSync] UserName, GameName, or ActorName is not set in GameContext");
            return false;
        }

        // 获取场景与演员映射关系
        await _stagesStateApi.Call(GameContext.Instance.StagesStateUrl);

        if (_stagesStateApi.ReqResult == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch stages state from server: request result is null");
            return false;
        }

        if (!_stagesStateApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[GameStateSync] Failed to fetch stages state from server: {_stagesStateApi.ReqResult.responseText}");
            return false;
        }

        Debug.Assert(_stagesStateApi.RespData != null, "[GameStateSync] StagesStateApi response data is null");

        // 更新全局映射关系
        GameContext.Instance.StageActorMapping = _stagesStateApi.RespData.mapping;
        return true;
    }

    /// <summary>
    /// 从服务器刷新指定场景列表的详情数据
    /// </summary>
    /// <param name="stages">需要获取详情的场景名称列表</param>
    /// <returns>是否成功</returns>
    public async UniTask<bool> RefreshStageDetailsFromServer(List<string> stages)
    {
        if (stages == null || stages.Count == 0)
        {
            Debug.LogWarning("[GameStateSync] Stage list is empty, skip fetching stage details");
            return false;
        }

        // 获取场景详情数据
        await _entityDetailsApi.Call(GameContext.Instance.EntityDetailsUrl, stages);

        if (_entityDetailsApi.ReqResult == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch stage details from server: request result is null");
            return false;
        }

        if (!_entityDetailsApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[GameStateSync] Failed to fetch stage details from server: {_entityDetailsApi.ReqResult.responseText}");
            return false;
        }

        Debug.Assert(_entityDetailsApi.RespData != null, "[GameStateSync] EntityDetailsApi response data is null");

        // 更新全局场景详情数据
        GameContext.Instance.StageEntitiesSerialization = _entityDetailsApi.RespData.entities_serialization;
        return true;
    }

    /// <summary>
    /// 从服务器刷新指定演员列表的详情数据
    /// </summary>
    /// <param name="actors">需要获取详情的演员名称列表</param>
    /// <returns>是否成功</returns>
    public async UniTask<bool> RefreshActorDetailsFromServer(List<string> actors)
    {
        if (actors == null || actors.Count == 0)
        {
            Debug.LogWarning("[GameStateSync] Actor list is empty, skip fetching actor details");
            return false;
        }

        // 获取演员详情数据
        await _entityDetailsApi.Call(GameContext.Instance.EntityDetailsUrl, actors);

        if (_entityDetailsApi.ReqResult == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch actor details from server: request result is null");
            return false;
        }

        if (!_entityDetailsApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[GameStateSync] Failed to fetch actor details from server: {_entityDetailsApi.ReqResult.responseText}");
            return false;
        }

        Debug.Assert(_entityDetailsApi.RespData != null, "[GameStateSync] EntityDetailsApi response data is null");

        // 更新全局演员详情数据
        GameContext.Instance.ActorEntitiesSerialization = _entityDetailsApi.RespData.entities_serialization;
        return true;
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
    /// <summary>
    /// 执行完整的游戏状态同步：Mapping + 演员详情 + 场景详情
    /// </summary>
    public async UniTask<bool> RefreshMappingAndEntitiesFromServer()
    {
        // 步骤1: 刷新场景映射关系
        if (!await RefreshMappingFromServer())
        {
            Debug.LogError("RefreshMappingAndEntitiesFromServer failed at step 1");
            return false;
        }

        // 步骤2: 刷新所有演员的详情数据
        if (!await RefreshActorDetailsFromServer(GameContext.Instance.AllActors))
        {
            Debug.LogError("RefreshMappingAndEntitiesFromServer failed at step 2");
            return false;
        }

        // 步骤3: 获取场景详情数据
        if (!await RefreshStageDetailsFromServer(GameContext.Instance.AllStages))
        {
            Debug.LogError("RefreshMappingAndEntitiesFromServer failed at step 3");
            return false;
        }

        return true;
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
    /// <summary>
    /// 执行部分游戏状态同步：Mapping + 演员详情（不获取场景详情）
    /// </summary>
    public async UniTask<bool> RefreshMappingAndActorsFromServer()
    {
        // 步骤1: 刷新场景映射关系
        if (!await RefreshMappingFromServer())
        {
            Debug.LogError("RefreshMappingAndActorsFromServer failed at step 1");
            return false;
        }

        // 步骤2: 刷新所有演员的详情数据
        if (!await RefreshActorDetailsFromServer(GameContext.Instance.AllActors))
        {
            Debug.LogError("RefreshMappingAndActorsFromServer failed at step 2");
            return false;
        }

        return true;
    }


    /// <summary>
    /// 从服务器刷新地下城数据
    /// 获取地下城的映射关系和地下城详细信息
    /// </summary>
    /// <param name="onComplete">完成回调，参数1为是否成功，参数2为消息</param>
    /// <returns>协程迭代器</returns>
    /// <summary>
    /// 从服务器刷新地下城数据
    /// </summary>
    public async UniTask<bool> RefreshDungeonFromServer()
    {
        if (string.IsNullOrEmpty(GameContext.Instance.UserName) || string.IsNullOrEmpty(GameContext.Instance.GameName) || string.IsNullOrEmpty(GameContext.Instance.PlayerActor))
        {
            Debug.LogError("[GameStateSync] UserName, GameName, or ActorName is not set in GameContext");
            return false;
        }

        await _dungeonStateApi.Call(GameContext.Instance.DungeonStateUrl);

        if (_dungeonStateApi.ReqResult == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch dungeon state from server: request result is null");
            return false;
        }

        if (!_dungeonStateApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[GameStateSync] Failed to fetch dungeon state from server: {_dungeonStateApi.ReqResult.responseText}");
            return false;
        }

        Debug.Assert(_dungeonStateApi.RespData != null, "[GameStateSync] DungeonStateApi response data is null");

        GameContext.Instance.StageActorMapping = _dungeonStateApi.RespData.mapping;
        GameContext.Instance.Dungeon = _dungeonStateApi.RespData.dungeon;
        return true;
    }

    /// <summary>
    /// 从服务器刷新指定演员所在场景中所有演员的详情数据
    /// </summary>
    /// <param name="actorName">演员名称，将获取该演员所在场景的所有演员详情</param>
    /// <returns>是否成功</returns>
    public async UniTask<bool> RefreshActorsInActorStageFromServer(string actorName)
    {
        // 获取指定演员所在场景的所有演员列表
        var stageName = GameContext.Instance.GetActorStage(actorName);
        if (string.IsNullOrEmpty(stageName))
        {
            Debug.LogError($"[GameStateSync] Actor '{actorName}' stage not found in mapping");
            return false;
        }

        var actorsInStage = GameContext.Instance.GetActorsInStage(stageName);
        if (actorsInStage.Count == 0)
        {
            Debug.LogWarning($"[GameStateSync] No actors found in stage '{stageName}'");
            return false;
        }

        // 刷新当前场景中所有演员的详情数据
        if (!await RefreshActorDetailsFromServer(actorsInStage))
        {
            Debug.LogError($"[GameStateSync] Failed to refresh actors in stage '{stageName}'");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 从服务器刷新地下城与演员数据
    /// 依次获取：1. 地下城状态和映射关系  2. 当前场景中所有演员的详细信息
    /// </summary>
    /// <param name="onComplete">完成回调，参数1为是否成功，参数2为消息</param>
    /// <returns>协程迭代器</returns>
    /// <summary>
    /// 从服务器刷新地下城与演员数据
    /// </summary>
    public async UniTask<bool> RefreshDungeonAndActorsFromServer()
    {
        // 步骤1: 刷新地下城数据
        if (!await RefreshDungeonFromServer())
        {
            Debug.LogError("RefreshDungeonAndActorsFromServer failed at step 1");
            return false;
        }

        // 步骤2: 刷新玩家所在场景中所有演员的详情数据
        if (!await RefreshActorsInActorStageFromServer(GameContext.Instance.PlayerActor))
        {
            Debug.LogError("RefreshDungeonAndActorsFromServer failed at step 2");
            return false;
        }

        return true;
    }
}