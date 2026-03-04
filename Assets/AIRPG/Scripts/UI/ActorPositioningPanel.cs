using System.Collections.Generic;
using UnityEngine;


public class ActorPositioningPanel : MonoBehaviour
{
    public static readonly int MaxPositioningObjects = 6;


    [Header("UI Components")]
    [SerializeField] private ActorPositioningObject[] _positioningObjects;
    [SerializeField] private CardBuildPanel _cardBuildPanel;
    [SerializeField] private EnemyHandPanel _enemyHandPanel;

    // 卡牌构筑数据管理对象，负责维护当前的构筑状态和数据
    public List<EntitySerialization> ActorEntities { get; set; }
    public List<EntitySerialization> EnemyEntities
    {
        get
        {
            List<EntitySerialization> enemyEntities = new();
            foreach (var entity in ActorEntities)
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
            List<EntitySerialization> expeditionMemberEntities = new();
            foreach (var entity in ActorEntities)
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
        var actorEntity = _positioningObjects[index].CachedActorEntity;

        var enemyComponent = GameUtils.GetComponent<EnemyComponent>(actorEntity);
        if (enemyComponent != null)
        {
            ShowEnemyHandPanel(actorEntity);

        }
        else
        {
            ShowCardBuildPanelForActor(actorEntity);
        }
    }


    void Start()
    {
        Debug.Assert(_positioningObjects != null && _positioningObjects.Length == MaxPositioningObjects, "Positioning objects array is not assigned in the inspector.");
        Debug.Assert(_cardBuildPanel != null, "_cardBuildPanel is null");
        Debug.Assert(_enemyHandPanel != null, "_enemyHandPanel is null");
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
            _positioningObjects[enemyIndices[i]].CachedActorEntity = enemies[i];
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
            _positioningObjects[memberIndices[i]].CachedActorEntity = members[i];
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

        if (GameContext.Instance.IsLoggedIn)
        {
            Round round = GameUtils.GetLastRound(GameContext.Instance.Dungeon);
            Debug.Assert(round != null, "CombatOnGoingState: No round data found for current dungeon");
            var actionOrderEntities = GameContext.Instance.GetActorEntities(round.action_order);
            Debug.Assert(actionOrderEntities != null && actionOrderEntities.Count > 0, "CombatOnGoingState: No action order entities found, cannot refresh view");
            _cardBuildPanel.SetupForActor(actorEntity, actionOrderEntities);
        }
        else
        {
            //mock 数据，所有的角色都显示同样的构筑界面
            _cardBuildPanel.SetupForActor(actorEntity, ActorEntities);
        }
    }

    ///<summary>
    /// 隐藏卡牌构筑面板
    /// </summary>
    public void HideCardBuildPanel()
    {
        _cardBuildPanel.gameObject.SetActive(false);

        if (ActorEntities != null && ActorEntities.Count > 0)
        {
            RefreshPositioningView();
        }
    }

    public void ShowEnemyHandPanel(EntitySerialization actorEntity)
    {
        _enemyHandPanel.gameObject.SetActive(true);
        //_enemyHandPanel.CurrentActor = actorEntity;
        _enemyHandPanel.SetupForActor(actorEntity);
    }

    public void HideEnemyHandPanel()
    {
        _enemyHandPanel.gameObject.SetActive(false);

        if (ActorEntities != null && ActorEntities.Count > 0)
        {
            RefreshPositioningView();
        }
    }
}
