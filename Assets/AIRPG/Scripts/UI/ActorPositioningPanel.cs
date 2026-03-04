using System.Collections.Generic;
using UnityEngine;


public class ActorPositioningPanel : MonoBehaviour
{
    // Maximum number of positioning objects that can be displayed in the panel
    public static readonly int MaxPositioningObjects = 6;


    [Header("UI Components")]
    [SerializeField] private ActorPositioningObject[] _positioningObjects;
    [SerializeField] private CardBuildPanel _cardBuildPanel;

    // 角色数据列表，包含所有需要在站位面板中显示的角色实体数据
    private List<EntitySerialization> _actorEntities;


    // 卡牌构筑数据管理对象，负责维护当前的构筑状态和数据
    public List<EntitySerialization> ActorEntities
    {
        get => _actorEntities;
        set
        {
            Debug.Assert(value != null && value.Count > 0, "ActorEntities cannot be null or empty");
            _actorEntities = value;
        }
    }

    public List<EntitySerialization> EnemyEntities
    {
        get
        {
            if (_actorEntities == null)
            {
                Debug.LogError("ActorEntities is not set. Cannot retrieve EnemyEntities.");
                return new List<EntitySerialization>();
            }
            List<EntitySerialization> enemyEntities = new();
            foreach (var entity in _actorEntities)
            {
                var enemyComponent = GameUtils.GetComponent<EnemyComponent>(entity);
                if (enemyComponent != null)
                {
                    enemyEntities.Add(entity);
                }
            }
            return enemyEntities;
        }
    }

    public List<EntitySerialization> ExpeditionMemberEntities
    {
        get
        {
            if (_actorEntities == null)
            {
                Debug.LogError("ActorEntities is not set. Cannot retrieve ExpeditionMemberEntities.");
                return new List<EntitySerialization>();
            }

            List<EntitySerialization> expeditionMemberEntities = new();
            foreach (var entity in _actorEntities)
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


    /// <summary>
    /// 点击站位对象的处理逻辑，目前仅输出日志，后续可以扩展为显示角色详细信息等功能
    /// </summary>
    /// <param name="index"></param>
    public void OnClickPositioningObject(int index)
    {
        Debug.Log($"Clicked on positioning object at index: {index}");
        var actorEntity = _positioningObjects[index].ActorEntity;

        ShowCardBuildPanelForActor(actorEntity);
    }


    void Start()
    {
        Debug.Assert(_positioningObjects != null && _positioningObjects.Length == MaxPositioningObjects, "Positioning objects array is not assigned in the inspector.");
        Debug.Assert(_cardBuildPanel != null, "_cardBuildPanel is null");
    }

    /// <summary>
    /// 刷新界面显示，根据当前的角色数据列表更新每个定位对象的显示状态和内容。
    /// </summary>
    public void RefreshPositioningView()
    {
        // 先隐藏所有定位对象
        for (int i = 0; i < MaxPositioningObjects; i++)
        {
            _positioningObjects[i].gameObject.SetActive(false);

            /// 移除之前绑定的点击事件，避免重复绑定导致的多次触发问题，然后再重新绑定当前的点击事件处理方法
            _positioningObjects[i].button.onClick.RemoveAllListeners();
            int index = i; // 需要一个局部变量来捕获当前的索引值，否则会导致闭包问题，所有按钮都会绑定到最后一个索引
            _positioningObjects[i].button.onClick.AddListener(() => OnClickPositioningObject(index));
        }

        // 敌人占位 index 0~2，根据数量决定摆放位置
        var enemies = EnemyEntities;
        int[] enemyIndices = enemies.Count switch
        {
            1 => new[] { 1 },
            2 => new[] { 0, 2 },
            _ => new[] { 0, 1, 2 }
        };
        for (int i = 0; i < enemies.Count && i < enemyIndices.Length; i++)
        {
            _positioningObjects[enemyIndices[i]].ActorEntity = enemies[i];
            _positioningObjects[enemyIndices[i]].gameObject.SetActive(true);
            _positioningObjects[enemyIndices[i]].RefreshView();
        }

        // 探险队成员占位 index 3~5，根据数量决定摆放位置
        var members = ExpeditionMemberEntities;
        int[] memberIndices = members.Count switch
        {
            1 => new[] { 4 },
            2 => new[] { 3, 5 },
            _ => new[] { 3, 4, 5 }
        };
        for (int i = 0; i < members.Count && i < memberIndices.Length; i++)
        {
            _positioningObjects[memberIndices[i]].ActorEntity = members[i];
            _positioningObjects[memberIndices[i]].gameObject.SetActive(true);
            _positioningObjects[memberIndices[i]].RefreshView();
        }
    }

    /// <summary>
    /// 刷新界面显示，根据当前的角色数据列表更新每个定位对象的显示状态和内容。
    /// </summary>
    public void ShowCardBuildPanelForActor(EntitySerialization actorEntity)
    {
        _cardBuildPanel.gameObject.SetActive(true);
        _cardBuildPanel.ActorEntities = ActorEntities;
        _cardBuildPanel.CurrentActor = actorEntity;
    }

    ///<summary>
    /// 隐藏卡牌构筑面板
    /// </summary>
    public void HideCardBuildPanel()
    {
        _cardBuildPanel.gameObject.SetActive(false);
    }
}
