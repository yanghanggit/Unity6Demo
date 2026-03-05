using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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
    

    [Header("API Components")]
    [SerializeField] private TasksStatusApi _tasksStatusApi; // 轮询任务状态的 API 组件

    void Start()
    {
        Debug.Assert(_actionOrderPanel != null, "_actionOrderPanel is null");
        Debug.Assert(_mainText != null, "Main Text component is not assigned in the inspector.");
        Debug.Assert(_scrollView != null, "Scroll View component is not assigned in the inspector.");
        Debug.Assert(_onCardElementClickedEvent != null, "_onCardElementClickedEvent is null");
        Debug.Assert(_onCardBuilderDataChangedEvent != null, "_onCardBuilderDataChangedEvent is null");
        
        Debug.Assert(_iconImage != null, "_iconImage is null");
        Debug.Assert(_statsText != null, "_statsText is null");
        Debug.Assert(_tasksStatusApi != null, "_tasksStatusApi is null");

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
    public void SetupForActor(EntitySerialization actorEntity, List<EntitySerialization> allActors)
    {
        Debug.Assert(actorEntity != null, "Current actor data is null");

        // 先更新行动顺序面板数据
        _actionOrderPanel.UpdateByActorEntities(allActors);

        // 初始化 CardBuilder 数据
        CardBuilder.Clear();
        CardBuilder.Build = new CardBuildData
        {
            owner = actorEntity
        };

        //
        UpdateMainText(CardBuilder.Build);

        //
        LoadCardElements(actorEntity, allActors);

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
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Not logged in, cannot execute build action");
            _mainText.text = "请先登录以执行构筑行动";
            return;
        }

        // 目标角色、技能和状态效果都是必选的，缺一不可，否则无法执行抽卡行动
        if (CardBuilder.Build.targetActors == null || CardBuilder.Build.targetActors.Count == 0)
        {
            Debug.LogWarning("No target actors selected, cannot execute escape action");
            _mainText.text = "请至少选择一个目标角色";
            return;
        }

        if (CardBuilder.Build.skill == null || CardBuilder.Build.skill.name == "")
        {
            Debug.LogWarning("No skill selected, cannot execute escape action");
            _mainText.text = "请至少选择一个技能";
            return;
        }

        if (CardBuilder.Build.statusEffects == null || CardBuilder.Build.statusEffects.Count == 0)
        {
            Debug.LogWarning("No status effects selected, cannot execute escape action");
            _mainText.text = "请至少选择一个状态效果";
            return;
        }

        // 调整一下显示，让玩家知道正在执行抽卡行动。
        _mainText.text = "正在执行抽卡行动...";

        // 创建抽卡行动
        var allyDrawAction = new AllyDrawCardAction
        {
            entity_name = CardBuilder.Build.owner.name,
            skill_name = CardBuilder.Build.skill.name,
            target_names = CardBuilder.Build.targetActors != null ? CardBuilder.Build.targetActors.ConvertAll(actor => actor.name) : new List<string>(),
            status_effect_names = CardBuilder.Build.statusEffects != null ? CardBuilder.Build.statusEffects.ConvertAll(effect => effect.name) : new List<string>()
        };

        ExecuteDrawCards(allyDrawAction).Forget();
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

            // case UIEventType.ActorPositioningClicked:
            //     Debug.Log($"角色站位被点击，目标角色: {eventData.targetId}");
            //     OnHandleActorPositioningClicked(eventData).Forget();
            //     break;

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

    /// <summary>
    /// 执行抽卡操作并轮询任务状态，完成后显示手牌
    /// 调用服务器 draw_cards 接口，获取任务ID后轮询查询任务状态
    /// 当任务完成时，刷新数据并显示角色手牌信息
    /// </summary>
    private async UniTaskVoid ExecuteDrawCards(AllyDrawCardAction allyDrawAction)
    {
        string taskId = await DungeonGamePlayManager.Instance.DrawCards(new List<AllyDrawCardAction> { allyDrawAction }, false);
        if (string.IsNullOrEmpty(taskId))
        {
            Debug.LogError("DrawCards API call failed, no task ID returned");
            return;
        }

        Debug.Log($"DrawCards initiated successfully, task ID: {taskId}");
        var taskRecord = await PollTaskStatus(taskId);
        if (taskRecord == null)
        {
            Debug.LogError($"Failed to get task record for task ID: {taskId}");
            return;
        }

        // 刷新角色数据以获取最新的手牌信息
        var actorEntities = await GameStateSync.Instance.GetEntities(new List<string> { allyDrawAction.entity_name });
        if (actorEntities == null)
        {
            Debug.LogError($"Failed to refresh actor entities from server for actor: {allyDrawAction.entity_name}");
            return;
        }

        // 更新当前角色数据
        var updatedActor = GameContext.Instance.GetActorEntity(allyDrawAction.entity_name);
        CardBuilder.Clear();
        CardBuilder.Build = new CardBuildData
        {
            owner = updatedActor
        };

        UpdateMainText(CardBuilder.Build);
    }

    /// <summary>
    /// 轮询查询任务状态直到完成或失败
    /// 委托 TasksStatusApi 执行轮询逻辑，完成后通过回调函数返回结果
    /// </summary>
    /// <param name="taskId">要查询的任务ID</param>
    /// <param name="onComplete">轮询完成后的回调函数，参数为(成功标志, 消息, 任务记录)</param>
    private async UniTask<TaskRecord> PollTaskStatus(string taskId)
    {
        return await _tasksStatusApi.PollTaskStatus(
            GameContext.Instance.TasksStatusUrl,
            taskId);
    }


    

}
