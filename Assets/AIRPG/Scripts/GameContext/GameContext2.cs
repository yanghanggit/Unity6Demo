using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// GameContext的部分类扩展
/// 负责管理游戏中的实体关系、场景映射和地牢数据
/// </summary>
public partial class GameContext
{
    /// <summary>
    /// 场景与角色的映射关系
    /// Key: 场景名称, Value: 该场景中的角色列表
    /// </summary>
    private Dictionary<string, List<string>> _stageActorMapping = new();

    /// <summary>
    /// 所有角色实体的序列化数据列表
    /// </summary>
    private List<EntitySerialization> _actorEntities = new();

    /// <summary>
    /// 所有场景实体的序列化数据列表
    /// </summary>
    private List<EntitySerialization> _stageEntities = new();

    /// <summary>
    /// 地牢数据对象
    /// </summary>
    private Dungeon _dungeon = new();

    /// <summary>
    /// 获取或设置场景与角色的映射关系
    /// Key为场景名称，Value为该场景中的角色名称列表
    /// </summary>
    public Dictionary<string, List<string>> StageActorMapping
    {
        get
        {
            return _stageActorMapping;
        }

        set
        {
            if (value == null)
            {
                Debug.LogError("Mapping is null");
                return;
            }
            _stageActorMapping = value;
        }
    }

    /// <summary>
    /// 获取游戏中所有角色的名称列表
    /// 通过遍历场景映射关系，汇总所有场景中的角色
    /// </summary>
    public List<string> ActorNames
    {
        get
        {
            List<string> allActors = new();
            foreach (var kvp in _stageActorMapping)
            {
                allActors.AddRange(kvp.Value);
            }
            return allActors;
        }
    }

    /// <summary>
    /// 获取游戏中所有场景的名称列表
    /// </summary>
    public List<string> StageNames
    {
        get
        {
            return new List<string>(_stageActorMapping.Keys);
        }
    }

    public List<string> EntityNames
    {
        get
        {
            // 目前仅包含角色和场景两类实体，后续如果有其他类型的实体需要管理，可以在这里进行扩展
            return new List<string>(ActorNames).Concat(StageNames).ToList();
        }
    }

    /// <summary>
    /// 根据角色名称查找该角色所在的场景
    /// </summary>
    /// <param name="actorName">角色名称</param>
    /// <returns>返回场景名称，如果未找到则返回空字符串</returns>
    public string GetActorStage(string actorName)
    {
        foreach (var kvp in _stageActorMapping)
        {
            if (kvp.Value.Contains(actorName))
            {
                return kvp.Key;
            }
        }
        return "";
    }

    /// <summary>
    /// 获取指定场景中的所有角色列表
    /// </summary>
    /// <param name="stageName">场景名称</param>
    /// <returns>返回该场景中的角色列表，如果场景不存在则返回空列表</returns>
    public List<string> GetActorsInStage(string stageName)
    {
        if (_stageActorMapping.ContainsKey(stageName))
        {
            return new List<string>(_stageActorMapping[stageName]);  // 返回副本
        }
        return new List<string>();
    }

    /// <summary>
    /// 获取当前角色所在场景中的其他角色列表(不包括当前角色自己)
    /// </summary>
    /// <returns>返回除当前角色外的场景中所有角色列表</returns>
    public List<string> GetActorsInCurrentStage()
    {
        // 获取当前角色所属场景
        var stageName = GetActorStage(PlayerActorName);
        Debug.Assert(stageName != "", "[GameContext] Current actor's stage name is empty");

        // 获取该场景中的所有角色
        var actorsInStage = GetActorsInStage(stageName);
        return actorsInStage;
    }

    /// <summary>
    /// 获取或设置当前地牢数据
    /// </summary>
    public Dungeon Dungeon
    {
        get
        {
            return _dungeon;
        }

        set
        {
            if (value == null)
            {
                Debug.LogError("Dungeon is null");
                return;
            }
            _dungeon = value;
        }
    }

    /// <summary>
    /// 获取或设置所有角色实体的序列化数据列表
    /// 用于存储和管理角色的持久化数据
    /// </summary>
    public List<EntitySerialization> ActorEntities
    {
        get
        {
            return _actorEntities;
        }
        set
        {
            if (value == null)
            {
                Debug.LogError("ActorEntitiesSerialization is null");
                return;
            }
            _actorEntities = value;
        }
    }

    /// <summary>
    /// 获取或设置所有场景实体的序列化数据列表
    /// 用于存储和管理场景的持久化数据
    /// </summary>
    public List<EntitySerialization> StageEntities
    {
        get
        {
            return _stageEntities;
        }
        set
        {
            if (value == null)
            {
                Debug.LogError("StageEntitiesSerialization is null");
                return;
            }
            _stageEntities = value;
        }
    }

    /// <summary>
    /// 根据角色名称获取对应的实体序列化数据
    /// </summary>
    /// <param name="actorName">角色名称</param>
    /// <returns>返回对应的EntitySerialization对象，如果未找到则返回null</returns>
    public EntitySerialization GetActorEntity(string actorName)
    {
        foreach (var entitySerialization in _actorEntities)
        {
            if (entitySerialization.name == actorName)
            {
                return entitySerialization;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据角色名称列表获取对应的实体序列化数据列表
    /// </summary> <param name="actorNames">角色名称列表</param>
    /// <returns>返回对应的EntitySerialization对象列表，如果未找到则返回空列表</returns>
    public List<EntitySerialization> GetActorEntities(List<string> actorNames)
    {
        var result = new List<EntitySerialization>();
        foreach (var actorName in actorNames)
        {
            var actorEntity = GetActorEntity(actorName);
            if (actorEntity != null)
            {
                result.Add(actorEntity);
            }
            else
            {
                Debug.LogWarning($"GetActorEntities: No EntitySerialization found for actor '{actorName}'");
            }
        }
        return result;
    }

    /// <summary>
    /// 根据场景名称获取对应的实体序列化数据
    /// </summary>
    /// <param name="stageName">场景名称</param>
    /// <returns>返回对应的EntitySerialization对象，如果未找到则返回null</returns>
    public EntitySerialization GetStageEntity(string stageName)
    {
        foreach (var entitySerialization in _stageEntities)
        {
            if (entitySerialization.name == stageName)
            {
                return entitySerialization;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取当前舞台中所有活着的盟友实体
    /// </summary>
    /// <returns>活着的盟友实体列表</returns>
    public List<EntitySerialization> GetAliveExpeditionMembersInCurrentCombatStage()
    {
        var aliveExpeditionMembers = new List<EntitySerialization>();
        var stageName = GetActorStage(PlayerActorName);
        var actorsInStage = GetActorsInStage(stageName);

        for (int i = 0; i < actorsInStage.Count; i++)
        {
            var actorEntity = GetActorEntity(actorsInStage[i]);
            Debug.Assert(actorEntity != null, "actorEntity is null");

            var expeditionMemberComponent = GameUtils.GetComponent<ExpeditionMemberComponent>(actorEntity);
            if (expeditionMemberComponent == null)
            {
                // 不是盟友，跳过
                continue;
            }

            var deathComponent = GameUtils.GetComponent<DeathComponent>(actorEntity);
            if (deathComponent != null)
            {
                // 已经死亡，跳过
                continue;
            }

            aliveExpeditionMembers.Add(actorEntity);
        }

        return aliveExpeditionMembers;
    }

    /// <summary>
    /// 获取当前舞台中所有活着的敌人实体
    /// </summary>
    /// <returns>活着的敌人实体列表</returns>
    public List<EntitySerialization> GetAliveEnemiesInCurrentCombatStage()
    {
        var aliveEnemies = new List<EntitySerialization>();
        var stageName = GetActorStage(PlayerActorName);
        var actorsInStage = GetActorsInStage(stageName);

        for (int i = 0; i < actorsInStage.Count; i++)
        {
            var actorEntity = GetActorEntity(actorsInStage[i]);
            Debug.Assert(actorEntity != null, "actorEntity is null");

            var enemyComponent = GameUtils.GetComponent<EnemyComponent>(actorEntity);
            if (enemyComponent == null)
            {
                // 不是敌人，跳过
                continue;
            }

            var deathComponent = GameUtils.GetComponent<DeathComponent>(actorEntity);
            if (deathComponent != null)
            {
                // 已经死亡，跳过
                continue;
            }

            aliveEnemies.Add(actorEntity);
        }

        return aliveEnemies;
    }
}