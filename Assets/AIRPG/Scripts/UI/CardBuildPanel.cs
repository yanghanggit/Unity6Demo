using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardBuildPanel : MonoBehaviour, IUIEventListener
{
    [Header("UI Components")]
    [SerializeField] private ActionOrderPanel _actionOrderPanel; // 行动顺序面板控制器
    [SerializeField] private TMP_Text _mainText; // 主文本显示对象
    [SerializeField] private LoopHorizontalScrollRect _scrollView; // 动态滚动视图
    [SerializeField] private Image _iconImage; // 角色头像显示对象
    [SerializeField] private TMP_Text _statsText; // 角色属性显示对象

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onCardElementClickedEvent; // 卡牌点击事件
    [SerializeField] private UIEventGameEvent _onCardBuilderDataChangedEvent; // CardBuilder 数据变化事件 

    // 卡牌构筑数据管理对象，负责维护当前的构筑状态和数据
    private List<EntitySerialization> _actorEntities; // 角色数据列表
    private EntitySerialization _currentActor; // 当前选中的角色数据

    // 卡牌构筑数据管理对象，负责维护当前的构筑状态和数据
    public List<EntitySerialization> ActorEntities
    {
        get => _actorEntities;
        set
        {
            Debug.Assert(value != null && value.Count > 0, "ActorEntities cannot be null or empty");
            _actorEntities = value;
            _actionOrderPanel.ActorEntities = _actorEntities;
        }
    }

    // 卡牌构筑数据管理对象，负责维护当前的构筑状态和数据
    public EntitySerialization CurrentActor
    {
        get => _currentActor;
        set
        {
            Debug.Assert(value != null, "CurrentActor cannot be null");
            _currentActor = value;
            SetupForActor(_currentActor);
        }
    }

    void Start()
    {
        Debug.Assert(_actionOrderPanel != null, "_actionOrderPanel is null");
        Debug.Assert(_mainText != null, "Main Text component is not assigned in the inspector.");
        Debug.Assert(_scrollView != null, "Scroll View component is not assigned in the inspector.");
        // Debug.Assert(_buildButton != null, "Build Button component is not assigned in the inspector.");
        Debug.Assert(_onCardElementClickedEvent != null, "_onCardElementClickedEvent is null");
        Debug.Assert(_onCardBuilderDataChangedEvent != null, "_onCardBuilderDataChangedEvent is null");
        Debug.Assert(_iconImage != null, "_iconImage is null");
        Debug.Assert(_statsText != null, "_statsText is null");

        // 注册事件监听器
        _onCardElementClickedEvent.RegisterListener(this);
        _onCardBuilderDataChangedEvent.RegisterListener(this);
    }

    void OnDestroy()
    {
        if (_onCardElementClickedEvent != null)
        {
            _onCardElementClickedEvent.UnregisterListener(this);
        }

        if (_onCardBuilderDataChangedEvent != null)
        {
            _onCardBuilderDataChangedEvent.UnregisterListener(this);
        }
    }

    /// <summary>
    /// 根据当前角色数据更新构筑按钮的状态
    /// </summary>
    public void SetupForActor(EntitySerialization actorEntity)
    {
        Debug.Assert(actorEntity != null, "Current actor data is null");

        // 初始化 CardBuilder 数据
        CardBuilder.Clear();
        CardBuilder.Build = new CardBuildData
        {
            owner = _currentActor
        };

        //
        UpdateMainText(CardBuilder.Build);

        //
        LoadCardElements(actorEntity, _actorEntities);

        // 更新角色头像显示
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(actorEntity.name);
        if (cachedSprite != null)
        {
            _iconImage.GetComponent<Image>().sprite = cachedSprite;
        }
        else
        {
            Debug.LogWarning($"DungeonCombatScene: Background sprite not found for stage: {actorEntity.name}");
            _iconImage.GetComponent<Image>().sprite = null;
        }

        // 更新主文本显示当前角色名称
        var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(actorEntity);
        Debug.Assert(combatStatsComponent != null, $"CombatStatsComponent is missing for actor: {actorEntity.name}");
        _statsText.text = GameUtils.GetDisplayName(actorEntity.name) + "\n" +
                   $"HP:{combatStatsComponent.stats.hp}/{combatStatsComponent.stats.max_hp}\n" +
                   $"Attack:{combatStatsComponent.stats.attack}\n" +
                   $"Defense:{combatStatsComponent.stats.defense}";
    }

    /// <summary>
    /// 点击构筑按钮的处理逻辑
    /// </summary>
    public void OnClickBuildButton()
    {
        // 这里可以添加点击构筑按钮的逻辑
        Debug.Log("Build Button Clicked " + _currentActor.name);
    }

    /// <summary>
    /// 点击关闭按钮的处理逻辑
    /// </summary>
    public void OnClickCloseButton()
    {
        Debug.Log("Close Button Clicked");
        gameObject.SetActive(false);
    }

    /// <summary>
    /// UI事件响应函数，根据事件类型进行不同的处理
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEventRaised(UIEventData eventData)
    {
        Debug.Log($"[TestDungeonCombatScenePrototype] {eventData}");
        Debug.Log($"OnEventRaised: {eventData.eventType}, TargetId: {eventData.targetId}, Index: {eventData.index}, ExtraData: {eventData.extraData}");

        switch (eventData.eventType)
        {
            case UIEventType.CardElementScrollViewItemClick:
                Debug.Log($"处理卡牌要素滚动视图项点击事件，目标: {eventData.targetId}, 索引: {eventData.index}");

                // 切换要素在 Build 中的状态（存在则删除，不存在则添加）
                if (!CardBuilder.TryToggleElementInBuild(eventData.index))
                {
                    Debug.LogWarning($"[TestDungeonCombatScenePrototype] TryToggleElementInBuild 失败，索引: {eventData.index}");
                    break;
                }

                // 派发 CardBuilder 数据已改变事件
                _onCardBuilderDataChangedEvent.Raise(new UIEventData(UIEventType.CardBuilderDataChanged));

                //
                break;

            case UIEventType.CardBuilderDataChanged:
                Debug.Log("CardBuilder data changed event received");
                UpdateMainText(CardBuilder.Build);
                break;

            default:
                Debug.LogWarning($"未处理的事件类型: {eventData.eventType}");
                break;
        }

    }

    /// <summary>
    /// 更新主文本显示，展示当前卡牌构建数据的状态
    /// </summary>
    private void UpdateMainText(CardBuildData cardBuild)
    {
        Debug.Assert(cardBuild != null, "CardBuildData is null");
        if (cardBuild.owner == null)
        {
            _mainText.text = "未选中角色";
            return;
        }


        _mainText.text = string.Empty;

        var handComponent = GameUtils.GetComponent<HandComponent>(cardBuild.owner);
        if (handComponent != null)
        {
            _mainText.text += "=== 手牌数据 ===\n\n";
            _mainText.text += GameUtils.FormatHandComponent(handComponent);
            _mainText.text += "\n\n";
        }

        _mainText.text += GameUtils.FormatCardBuildData(cardBuild);
    }

    /// <summary>
    /// 从选中角色加载卡牌要素数据
    /// 只负责维护 CardElements 部分，不处理 Build 数据
    /// </summary>
    /// <param name="selectedActor">选中的角色</param>
    /// <param name="allActors">当前可用的角色列表，其余角色将作为卡牌目标候选</param>
    private void LoadCardElements(EntitySerialization selectedActor, List<EntitySerialization> allActors)
    {
        if (selectedActor == null)
        {
            Debug.LogWarning("[LoadCardElementsFromActor] selectedActor is null");
            _scrollView.totalCount = CardBuilder.Count;
            _scrollView.RefillCells(); // 重建列表并回到顶部
            return;
        }

        // 1. 添加角色的技能
        var skillBook = GameUtils.GetComponent<SkillBookComponent>(selectedActor);
        if (skillBook?.skills != null)
        {
            foreach (var skill in skillBook.skills)
            {
                CardBuilder.AddElement(new CardElementData(skill));
            }
            Debug.Log($"[LoadCardElementsFromActor] 添加了 {skillBook.skills.Count} 个技能");
        }

        // 2. 添加角色的状态效果
        var combatStats = GameUtils.GetComponent<CombatStatsComponent>(selectedActor);
        if (combatStats?.status_effects != null)
        {
            foreach (var effect in combatStats.status_effects)
            {
                CardBuilder.AddElement(new CardElementData(effect));
            }
            Debug.Log($"[LoadCardElementsFromActor] 添加了 {combatStats.status_effects.Count} 个状态效果");
        }

        // 3. 添加其他角色作为目标（排除自己）
        int targetCount = 0;
        foreach (var actor in allActors)
        {
            CardBuilder.AddElement(new CardElementData(actor));
            targetCount++;
        }
        Debug.Log($"[LoadCardElementsFromActor] 添加了 {targetCount} 个目标角色");

        // 更新滚动视图
        _scrollView.totalCount = CardBuilder.Count;
        _scrollView.RefillCells(); // 重建列表并回到顶部
        Debug.Log($"[LoadCardElementsFromActor] 总共加载 {CardBuilder.Count} 个卡牌要素");
    }
}
