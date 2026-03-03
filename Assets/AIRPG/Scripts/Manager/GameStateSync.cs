using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
//using System.Linq;

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
    /// <returns>成功时返回 GameContext.Instance.StageActorMapping（Dictionary&lt;string, List&lt;string&gt;&gt;，作为 cache 引用），失败时返回 null</returns>
    public async UniTask<Dictionary<string, List<string>>> RefreshStageActorMappingFromServer()
    {
        if (string.IsNullOrEmpty(GameContext.Instance.UserName) || string.IsNullOrEmpty(GameContext.Instance.GameName) || string.IsNullOrEmpty(GameContext.Instance.PlayerActor))
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

        // 更新全局映射关系（作为 cache）
        GameContext.Instance.StageActorMapping = _stagesStateApi.RespData.mapping;
        return GameContext.Instance.StageActorMapping;
    }

    /// <summary>
    /// 从服务器刷新指定场景列表的详情数据
    /// </summary>
    /// <param name="stages">需要获取详情的场景名称列表</param>
    /// <returns>成功时返回 GameContext.Instance.StageEntities（List&lt;EntitySerialization&gt;，作为 cache 引用），失败时返回 null</returns>
    public async UniTask<List<EntitySerialization>> RefreshStageDetailsFromServer(List<string> stages)
    {
        if (stages == null || stages.Count == 0)
        {
            Debug.LogWarning("[GameStateSync] Stage list is empty, skip fetching stage details");
            return null;
        }

        // 获取场景详情数据
        await _entityDetailsApi.Call(GameContext.Instance.EntityDetailsUrl, stages);

        if (_entityDetailsApi.ReqResult == null)
        {
            Debug.LogError("[GameStateSync] Failed to fetch stage details from server: request result is null");
            return null;
        }

        if (!_entityDetailsApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[GameStateSync] Failed to fetch stage details from server: {_entityDetailsApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_entityDetailsApi.RespData != null, "[GameStateSync] EntityDetailsApi response data is null");

        // 更新全局场景详情数据（作为 cache）
        GameContext.Instance.StageEntities = _entityDetailsApi.RespData.entities_serialization;
        return GameContext.Instance.StageEntities;
    }

    /// <summary>
    /// 从服务器刷新指定实体列表的详情数据，并按类型分别存入 ActorEntities 和 StageEntities
    /// </summary>
    /// <param name="entityNames">需要获取详情的实体名称列表（可包含演员或场景）</param>
    /// <returns>成功时返回所有实体的浅拷贝列表（List&lt;EntitySerialization&gt;），失败时返回 null</returns>
    public async UniTask<List<EntitySerialization>> RefreshEntityDetailsFromServer(List<string> entityNames)
    {
        if (entityNames == null || entityNames.Count == 0)
        {
            Debug.LogWarning("[GameStateSync] Entity list is empty, skip fetching entity details");
            return null;
        }

        // 获取演员详情数据
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


        var actorEntities = new List<EntitySerialization>();
        var stageEntities = new List<EntitySerialization>();

        for (int i = 0; i < _entityDetailsApi.RespData.entities_serialization.Count; i++)
        {
            var entity = _entityDetailsApi.RespData.entities_serialization[i];
            Debug.Log($"[GameStateSync] Fetched entity details from server: {entity.name} (index {i})");

            var actorComponent = GameUtils.GetComponent<ActorComponent>(entity);
            if (actorComponent != null)
            {
                Debug.Log($"[GameStateSync] ActorComponent - name: {actorComponent.name}, character_sheet_name: {actorComponent.character_sheet_name}, current_stage: {actorComponent.current_stage}");
                actorEntities.Add(entity);
                continue;
            }

            var stageComponent = GameUtils.GetComponent<StageComponent>(entity);
            if (stageComponent != null)
            {
                Debug.Log($"[GameStateSync] StageComponent - name: {stageComponent.name}, character_sheet_name: {stageComponent.character_sheet_name}");
                stageEntities.Add(entity);
                continue;
            }
        }

        // 更新全局演员及场景实体详情数据（作为 cache）
        GameContext.Instance.ActorEntities = actorEntities;
        GameContext.Instance.StageEntities = stageEntities;

        // 返回浅拷贝，避免外部修改全局缓存数据
        return new List<EntitySerialization>(_entityDetailsApi.RespData.entities_serialization);
    }

    /// <summary>
    /// 从服务器刷新场景-演员映射关系及场景实体详情数据
    /// 执行游戏状态同步，依次获取：
    /// 1. 场景与演员的映射关系(StageActorMapping)
    /// 2. 所有场景及实体的详细信息(StageEntities)
    /// 适用于需要获取完整游戏状态的场景，如初始化、场景切换等
    /// </summary>
    /// <returns>成功返回 <see cref="GameSyncError.None"/>，失败返回对应错误码</returns>
    public async UniTask<GameSyncError> RefreshStageActorMappingAndEntitiesFromServer()
    {
        // 步骤1: 刷新场景映射关系
        var stageActorMapping = await RefreshStageActorMappingFromServer();
        if (stageActorMapping == null)
        {
            Debug.LogError("RefreshMappingAndEntitiesFromServer failed at step 1");
            return GameSyncError.FetchMappingFailed;
        }

        // 步骤2: 获取场景详情数据
        var allEntities = await RefreshStageDetailsFromServer(GameContext.Instance.EntityNames);
        if (allEntities == null)
        {
            Debug.LogError("RefreshMappingAndEntitiesFromServer failed at step 2");
            return GameSyncError.FetchStageDetailsFailed;
        }

        return GameSyncError.None;
    }

    /// <summary>
    /// 从服务器刷新场景-演员映射关系及演员详情数据
    /// 执行部分游戏状态同步，依次获取：
    /// 1. 场景与演员的映射关系(StageActorMapping)
    /// 2. 所有演员的详细信息(ActorEntitiesSerialization)
    /// 相比 RefreshStageActorMappingAndEntitiesFromServer，此方法不获取场景详情，适用于只需要演员数据的场景
    /// </summary>
    /// <returns>成功返回 <see cref="GameSyncError.None"/>，失败返回对应错误码</returns>
    public async UniTask<GameSyncError> RefreshStageActorMappingAndActorDetailsFromServer()
    {
        // 步骤1: 刷新场景映射关系
        var stageActorMapping = await RefreshStageActorMappingFromServer();
        if (stageActorMapping == null)
        {
            Debug.LogError("RefreshMappingAndActorsFromServer failed at step 1");
            return GameSyncError.FetchMappingFailed;
        }

        // 步骤2: 刷新所有演员的详情数据
        var actorEntities = await RefreshEntityDetailsFromServer(GameContext.Instance.ActorNames);
        if (actorEntities == null)
        {
            Debug.LogError("RefreshMappingAndActorsFromServer failed at step 2");
            return GameSyncError.FetchActorDetailsFailed;
        }

        return GameSyncError.None;
    }


    /// <summary>
    /// 从服务器刷新地下城数据
    /// 获取地下城详细信息并更新到 GameContext.Instance.Dungeon
    /// </summary>
    /// <returns>成功时返回 GameContext.Instance.Dungeon（作为 cache 引用），失败时返回 null</returns>
    public async UniTask<Dungeon> RefreshDungeonFromServer()
    {
        if (string.IsNullOrEmpty(GameContext.Instance.UserName) || string.IsNullOrEmpty(GameContext.Instance.GameName) || string.IsNullOrEmpty(GameContext.Instance.PlayerActor))
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

        //GameContext.Instance.StageActorMapping = _dungeonStateApi.RespData.mapping;
        GameContext.Instance.Dungeon = _dungeonStateApi.RespData.dungeon;
        return GameContext.Instance.Dungeon;
    }

    /// <summary>
    /// 从服务器刷新指定演员所在场景中所有实体的详情数据
    /// </summary>
    /// <param name="actorName">演员名称，将获取该演员所在场景的所有实体详情</param>
    /// <returns>成功时返回所有实体的浅拷贝列表（List&lt;EntitySerialization&gt;），失败时返回 null</returns>
    public async UniTask<List<EntitySerialization>> RefreshActorsInStageFromServer(string actorName)
    {
        // 获取指定演员所在场景的所有演员列表
        var stageName = GameContext.Instance.GetActorStage(actorName);
        if (string.IsNullOrEmpty(stageName))
        {
            Debug.LogError($"[GameStateSync] Actor '{actorName}' stage not found in mapping");
            return null;
        }

        var actorsInStage = GameContext.Instance.GetActorsInStage(stageName);
        if (actorsInStage.Count == 0)
        {
            Debug.LogWarning($"[GameStateSync] No actors found in stage '{stageName}'");
            return null;
        }

        // 刷新当前场景中所有演员的详情数据
        var actorEntities = await RefreshEntityDetailsFromServer(actorsInStage);
        if (actorEntities == null)
        {
            Debug.LogError($"[GameStateSync] Failed to refresh actors in stage '{stageName}'");
            return null;
        }

        return actorEntities;
    }

    /// <summary>
    /// 从服务器刷新地下城数据及玩家所在场景的演员详情
    /// 依次获取：1. 地下城状态和映射关系  2. 玩家所在场景中所有演员的详细信息
    /// </summary>
    /// <returns>成功返回 <see cref="GameSyncError.None"/>，失败返回对应错误码</returns>
    public async UniTask<GameSyncError> RefreshDungeonAndActorsFromServer()
    {
        // 步骤1: 刷新地下城数据
        var dungeon = await RefreshDungeonFromServer();
        if (dungeon == null)
        {
            Debug.LogError("RefreshDungeonAndActorsFromServer failed at step 1");
            return GameSyncError.FetchDungeonFailed;
        }

        // 步骤2: 刷新玩家所在场景中所有演员的详情数据
        var actorEntities = await RefreshActorsInStageFromServer(GameContext.Instance.PlayerActor);
        if (actorEntities == null)
        {
            Debug.LogError("RefreshDungeonAndActorsFromServer failed at step 2");
            return GameSyncError.FetchActorsInStageFailed;
        }

        return GameSyncError.None;
    }
}