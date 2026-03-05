//using System;
using System.Collections.Generic;
using UnityEngine;


public class ActorPositioningPanel : MonoBehaviour
{
    public static readonly int MaxPositioningObjects = 6; // 本游戏目前的最高敌人数量为3，最高探险队成员数量为3，因此总共需要6个定位对象来显示所有角色。

    [Header("UI Components")]
    [SerializeField] private ActorPositioningObject[] _positioningObjects;

    void Start()
    {
        Debug.Assert(_positioningObjects != null && _positioningObjects.Length == MaxPositioningObjects, "Positioning objects array is not assigned in the inspector.");
    }

    /// <summary>
    /// 刷新界面显示，根据当前的角色数据列表更新每个定位对象的显示状态和内容。
    /// </summary>
    public void RefreshView(List<EntitySerialization> sortedActorEntities, List<string> actionOrder)
    {
        // 先隐藏所有定位对象
        for (int i = 0; i < MaxPositioningObjects; i++)
        {
            _positioningObjects[i].gameObject.SetActive(false);
        }

        // 敌人占位 index 0~2，根据数量决定摆放位置
        var enemies = GetEnemyEntities(sortedActorEntities);
        int[] enemyIndices = enemies.Count switch
        {
            1 => new[] { 1 },
            2 => new[] { 0, 2 },
            _ => new[] { 0, 1, 2 }
        };
        for (int i = 0; i < enemies.Count && i < enemyIndices.Length; i++)
        {
            //_positioningObjects[enemyIndices[i]].ActorEntity = enemies[i];
            _positioningObjects[enemyIndices[i]].gameObject.SetActive(true);
            _positioningObjects[enemyIndices[i]].RefreshView(enemies[i], actionOrder);
        }

        // 探险队成员占位 index 3~5，根据数量决定摆放位置
        var members = ExpeditionMemberEntities(sortedActorEntities);
        int[] memberIndices = members.Count switch
        {
            1 => new[] { 4 },
            2 => new[] { 3, 5 },
            _ => new[] { 3, 4, 5 }
        };
        for (int i = 0; i < members.Count && i < memberIndices.Length; i++)
        {
            //_positioningObjects[memberIndices[i]].ActorEntity = members[i];
            _positioningObjects[memberIndices[i]].gameObject.SetActive(true);
            _positioningObjects[memberIndices[i]].RefreshView(members[i], actionOrder);
        }
    }

    private List<EntitySerialization> GetEnemyEntities(List<EntitySerialization> actorEntities)
    {
        List<EntitySerialization> enemyEntities = new();
        foreach (var entity in actorEntities)
        {
            var enemyComponent = GameUtils.GetComponent<EnemyComponent>(entity);
            if (enemyComponent != null)
            {
                enemyEntities.Add(entity);
            }
        }
        return enemyEntities;
    }

    private List<EntitySerialization> ExpeditionMemberEntities(List<EntitySerialization> actorEntities)
    {
        List<EntitySerialization> expeditionMemberEntities = new();
        foreach (var entity in actorEntities)
        {
            var expeditionMemberComponent = GameUtils.GetComponent<ExpeditionMemberComponent>(entity);
            if (expeditionMemberComponent != null)
            {
                expeditionMemberEntities.Add(entity);
            }
        }
        return expeditionMemberEntities;
    }
}
