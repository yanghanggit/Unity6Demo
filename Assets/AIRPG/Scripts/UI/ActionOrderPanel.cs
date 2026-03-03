using System.Collections.Generic;
using UnityEngine;

public class ActionOrderPanel : MonoBehaviour
{

    [Header("Prefab References")]
    [SerializeField] private ActionOrderObject _actionOrderObjectPrefab; // 角色槽位预制体


    private List<EntitySerialization> _actorEntities; // 角色数据列表

    public List<EntitySerialization> ActorEntities
    {
        get => _actorEntities;
        set
        {
            Debug.Assert(value != null && value.Count > 0, "ActorEntities cannot be null or empty");
            _actorEntities = value;
            UpdateByActorEntities(_actorEntities);
        }
    }

    void Start()
    {
        Debug.Assert(_actionOrderObjectPrefab != null, "_actionOrderObjectPrefab is null");
    }


    /// <summary>
    /// 根据传入的角色实体序列化数据列表，动态生成角色槽位UI对象，并设置数据
    /// </summary>
    /// <param name="actorEntities"></param>
    private void UpdateByActorEntities(List<EntitySerialization> actorEntities)
    {
        // 根据传入的角色实体序列化数据列表，动态生成角色槽位UI对象，并设置数据
        for (int i = 0; i < actorEntities.Count; i++)
        {
            var actorEntity = actorEntities[i];
            var actionOrderObject = Instantiate(_actionOrderObjectPrefab, transform);
            actionOrderObject.name = $"ActionOrder_{i}";
            actionOrderObject.ActorEntity = actorEntity;
        }
    }
}
