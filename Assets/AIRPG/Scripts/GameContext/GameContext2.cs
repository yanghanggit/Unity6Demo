using System.Collections.Generic;
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
    private Dictionary<string, List<string>> _stageActorMapping = new Dictionary<string, List<string>>();

    /// <summary>
    /// 所有角色实体的序列化数据列表
    /// </summary>
    private List<EntitySerialization> _actorEntitiesSerialization = new List<EntitySerialization>();

    /// <summary>
    /// 所有场景实体的序列化数据列表
    /// </summary>
    private List<EntitySerialization> _stageEntitiesSerialization = new List<EntitySerialization>();

    /// <summary>
    /// 地牢数据对象
    /// </summary>
    private Dungeon _dungeon = new Dungeon();

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
    public List<string> AllActors
    {
        get
        {
            List<string> allActors = new List<string>();
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
    public List<string> AllStages
    {
        get
        {
            return new List<string>(_stageActorMapping.Keys);
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
    public List<string> GetOtherActorsInCurrentStage()
    {
        // 获取当前角色所属场景
        var stageName = GetActorStage(PlayerActor);
        Debug.Assert(stageName != "", "[GameContext] Current actor's stage name is empty");
        
        // 获取该场景中的所有角色
        var actorsInStage = GetActorsInStage(stageName);
        
        // 移除当前角色自己
        actorsInStage.Remove(PlayerActor);
        
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
    public List<EntitySerialization> ActorEntitiesSerialization
    {
        get
        {
            return _actorEntitiesSerialization;
        }
        set
        {
            if (value == null)
            {
                Debug.LogError("ActorEntitiesSerialization is null");
                return;
            }
            _actorEntitiesSerialization = value;
        }
    }

    /// <summary>
    /// 获取或设置所有场景实体的序列化数据列表
    /// 用于存储和管理场景的持久化数据
    /// </summary>
    public List<EntitySerialization> StageEntitiesSerialization
    {
        get
        {
            return _stageEntitiesSerialization;
        }
        set
        {
            if (value == null)
            {
                Debug.LogError("StageEntitiesSerialization is null");
                return;
            }
            _stageEntitiesSerialization = value;
        }
    }

    /// <summary>
    /// 根据角色名称获取对应的实体序列化数据
    /// </summary>
    /// <param name="actorName">角色名称</param>
    /// <returns>返回对应的EntitySerialization对象，如果未找到则返回null</returns>
    public EntitySerialization GetActorEntitySerialization(string actorName)
    {
        foreach (var entitySerialization in _actorEntitiesSerialization)
        {
            if (entitySerialization.name == actorName)
            {
                return entitySerialization;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据场景名称获取对应的实体序列化数据
    /// </summary>
    /// <param name="stageName">场景名称</param>
    /// <returns>返回对应的EntitySerialization对象，如果未找到则返回null</returns>
    public EntitySerialization GetStageEntitySerialization(string stageName)
    {
        foreach (var entitySerialization in _stageEntitiesSerialization)
        {
            if (entitySerialization.name == stageName)
            {
                return entitySerialization;
            }
        }
        return null;
    }
}