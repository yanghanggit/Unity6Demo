using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class DungeonCombatScene2 : MonoBehaviour, IUIEventListener
{
    [Header("Scene Settings")]
    [SerializeField] private string _preScene = "MainScene";
    [SerializeField] private string _nextScene = "DungeonCombatScene2";

    [Header("UI Components")]
    [SerializeField] private Image _backgroundImage; // 场景背景图片
    [SerializeField] private TMP_Text _combatInfoText; // 战斗信息显示对象
    [SerializeField] private TMP_Text _mainText; // 主文本显示对象
    [SerializeField] private LoopHorizontalScrollRect _scrollView; // 动态滚动视图
    [SerializeField] private ActionOrderObject[] _actionOrderObjects; // 角色槽位数组
    [SerializeField] private GameObject _mainGameObject; // 场景主对象，用于测试 Find 和事件系统的目标对象
    [SerializeField] private GameObject _bottomGameObject; // 行动顺序对象，用于测试 Find 和事件系统的目标对象

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onCardElementClickedEvent; // 卡牌点击事件
    [SerializeField] private UIEventGameEvent _onActionOrderClickedEvent; // 角色槽位点击事件
    [SerializeField] private UIEventGameEvent _onCardBuilderDataChangedEvent; // CardBuilder 数据变化事件    

    [Header("API Components")]
    [SerializeField] private TasksStatusApi _tasksStatusApi;

    private List<EntitySerialization> _mockActorData;// Mock 数据 - 用于测试

    void Awake()
    {
        // 创建 mock 数据
        _mockActorData = MockData.CreateActorData();
    }

    void Start()
    {
        // 断言检查，确保所有必要的组件和数据都已正确设置
        Debug.Assert(_combatInfoText != null, "_combatInfoText is null");
        Debug.Assert(_mainText != null, "Main Text component is not assigned in the inspector.");
        Debug.Assert(_actionOrderObjects != null && _actionOrderObjects.Length > 0, "Action order objects are not assigned in the inspector.");
        Debug.Assert(_scrollView != null, "ScrollView component is not assigned in the inspector.");
        Debug.Assert(_backgroundImage != null, "Background Image component is not assigned in the inspector.");
        Debug.Assert(_onCardElementClickedEvent != null, "_onCardClickedEvent is null");
        Debug.Assert(_onActionOrderClickedEvent != null, "_onActionOrderClickedEvent is null");
        Debug.Assert(_onCardBuilderDataChangedEvent != null, "_onCardBuilderDataChangedEvent is null");
        //Debug.Assert(_onHandComponentChangedEvent != null, "_onHandComponentChangedEvent is null");
        Debug.Assert(_mockActorData != null && _mockActorData.Count > 0, "Mock actor data is not initialized");
        Debug.Assert(SpriteCacheManager.Instance != null, "SpriteCacheManager instance is null");
        Debug.Assert(_tasksStatusApi != null, "TasksStatusApi component is not assigned in the inspector.");
        Debug.Assert(_mainGameObject != null, "_mainGameObject is null");
        Debug.Assert(_bottomGameObject != null, "_actionOrderObject is null");

        // 注册事件监听器
        _onCardElementClickedEvent.RegisterListener(this);
        _onActionOrderClickedEvent.RegisterListener(this);
        _onCardBuilderDataChangedEvent.RegisterListener(this);

        // 第一次更新，显示空数据的文本。
        CardBuilder.Clear();

        // 根据当前战斗状态更新主对象的可交互状态
        UpdateCombatUIVisibility();

        // 初始化设置info的文本，展示当前地下城和关卡信息
        UpdateCombatInfoText();

        // 更新背景
        UpdateBackgroundImage();

        // 根据当前 CardBuilder.Build 的状态更新主文本显示
        UpdateMainTextWithCardBuildData(CardBuilder.Build);

        // 根据当前地下城状态更新角色槽位显示
        UpdateActionOrder();

        // 刷新场景初始化
        if (GameContext.Instance.IsLoggedIn)
        {
            ExecuteCombatInit().Forget();
        }
        else
        {
            ExecuteCombatInitMock().Forget();
        }
    }

    void OnDestroy()
    {
        // 确保在对象销毁时取消注册事件监听器，避免内存泄漏或错误调用
        if (_onCardElementClickedEvent != null)
        {
            _onCardElementClickedEvent.UnregisterListener(this);
        }

        if (_onActionOrderClickedEvent != null)
        {
            _onActionOrderClickedEvent.UnregisterListener(this);
        }

        if (_onCardBuilderDataChangedEvent != null)
        {
            _onCardBuilderDataChangedEvent.UnregisterListener(this);
        }
    }

    /// <summary>
    /// 点击 Setting 按钮
    /// </summary>
    public void OnClickSetting()
    {
        Debug.Log("Setting button clicked");
    }

    /// <summary>
    /// 点击 Play 按钮
    /// </summary>
    public void OnClickPlay()
    {
        Debug.Log("Play button clicked");

        var combatState = GameUtils.GetLastCombatState(GameContext.Instance.Dungeon);
        switch (combatState)
        {
            case CombatState.INITIALIZATION:
                Debug.LogWarning("Combat is in initialization state, cannot execute escape action");
                break;

            case CombatState.ONGOING:

                // 如果未登录，就不要处理这段提交代码。
                if (!GameContext.Instance.IsLoggedIn)
                {
                    Debug.Log("Simulating successful escape for non-logged-in user");
                    break;
                }

                if (CardBuilder.Build.owner == null)
                {
                    Debug.LogWarning("No actor selected, cannot execute escape action");
                    break;
                }

                var enemyComponent = GameUtils.GetComponent<EnemyComponent>(CardBuilder.Build.owner);
                if (enemyComponent != null)
                {
                    // 敌人直接执行抽卡行动，传入空的行动列表和启用敌人抽卡的标志
                    ExecuteDrawCards(new List<AllyDrawCardAction>(), true).Forget();
                }
                else
                {

                    // 目标角色、技能和状态效果都是必选的，缺一不可，否则无法执行抽卡行动
                    if (CardBuilder.Build.targetActors == null || CardBuilder.Build.targetActors.Count == 0)
                    {
                        Debug.LogWarning("No target actors selected, cannot execute escape action");
                        break;
                    }

                    if (CardBuilder.Build.skill == null || CardBuilder.Build.skill.name == "")
                    {
                        Debug.LogWarning("No skill selected, cannot execute escape action");
                        break;
                    }

                    if (CardBuilder.Build.statusEffects == null || CardBuilder.Build.statusEffects.Count == 0)
                    {
                        Debug.LogWarning("No status effects selected, cannot execute escape action");
                        break;
                    }

                    // 创建抽卡行动
                    var allyDrawAction = new AllyDrawCardAction
                    {
                        entity_name = CardBuilder.Build.owner.name,
                        skill_name = CardBuilder.Build.skill.name,
                        target_names = CardBuilder.Build.targetActors != null ? CardBuilder.Build.targetActors.ConvertAll(actor => actor.name) : new List<string>(),
                        status_effect_names = CardBuilder.Build.statusEffects != null ? CardBuilder.Build.statusEffects.ConvertAll(effect => effect.name) : new List<string>()
                    };

                    // 调用抽卡接口，传入构建的行动数据
                    ExecuteDrawCards(new List<AllyDrawCardAction> { allyDrawAction }, false).Forget();

                }

                break;

            case CombatState.COMPLETE:
                Debug.LogWarning("Combat is already complete, cannot execute escape action");
                break;

            default:
                Debug.LogWarning($"Unknown combat state: {combatState}");
                break;
        }
    }

    /// <summary>
    /// IUIEventListener 接口实现
    /// 处理所有UI事件的统一入口
    /// </summary>
    public void OnEventRaised(UIEventData eventData)
    {
        Debug.Log($"[TestDungeonCombatScenePrototype] {eventData}");
        Debug.Log($"OnEventRaised: {eventData.eventType}, TargetId: {eventData.targetId}, Index: {eventData.index}, ExtraData: {eventData.extraData}");
        //_mainText.text = $"事件: {eventData.eventType}\n目标: {eventData.targetId}\n索引: {eventData.index}\n额外: {eventData.extraData}";

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

            case UIEventType.ActionOrderClick:

                Debug.Log($"处理角色槽位点击事件，目标: {eventData.targetId}, 索引: {eventData.index}");

                var allActors = GetCurrentRoundActors();

                if (allActors == null)
                {
                    Debug.LogWarning("DungeonCombatScene: No actor data available");
                    break;
                }

                // 从角色列表中查找对应的角色
                var selectedActor = allActors.Find(actor => actor.name == eventData.targetId);
                if (selectedActor == null)
                {
                    Debug.LogWarning($"未找到名为 {eventData.targetId} 的角色数据");
                    break;
                }

                // 清空并设置新的 Build 数据
                CardBuilder.Clear();
                CardBuilder.Build = new CardBuildData { owner = selectedActor };

                // 获取选中角色的手牌组件，检查是否存在
                var handComponent = GameUtils.GetComponent<HandComponent>(selectedActor);
                if (handComponent != null)
                {
                    // 有卡牌，就不能加载 Build 数据了，先显示手牌数据。
                    UpdateMainTextWithHandData(selectedActor);
                    LoadCardElementsFromActor(selectedActor, allActors);
                    break;
                }

                // 如果是敌人就需要特殊处理数据
                var enemyComponent = GameUtils.GetComponent<EnemyComponent>(selectedActor);
                if (enemyComponent != null)
                {
                    // 先显示敌人的战斗属性和状态效果信息，后续可以扩展显示更多内容。
                    UpdateMainTextWithEnemyData(selectedActor);
                    LoadCardElementsFromActor(selectedActor, allActors); // 敌人不加载目标角色数据
                    break;
                }


                // 根据选中角色加载卡牌要素数据，敌人不加载目标角色数据
                LoadCardElementsFromActor(selectedActor, allActors);

                // 派发 CardBuilder 数据已改变事件
                _onCardBuilderDataChangedEvent.Raise(new UIEventData(UIEventType.CardBuilderDataChanged));

                break;

            case UIEventType.CardBuilderDataChanged:
                Debug.Log("CardBuilder data changed event received");
                // 这里可以添加额外的逻辑来响应 CardBuilder 数据变化，例如更新 UI 或触发其他游戏机制
                // 更新主文本显示
                UpdateMainTextWithCardBuildData(CardBuilder.Build);
                break;

            default:
                Debug.LogWarning($"未处理的事件类型: {eventData.eventType}");
                break;
        }

    }

    /// <summary>
    /// 更新主文本显示，展示当前卡牌构建数据的状态
    /// </summary>
    private void UpdateMainTextWithCardBuildData(CardBuildData Build)
    {
        Debug.Assert(Build != null, "CardBuildData is null");
        _mainText.text = GameUtils.FormatCardBuildData(Build);
    }

    /// <summary>
    /// 更新主文本显示，展示当前选中角色的手牌信息
    /// </summary>
    private void UpdateMainTextWithHandData(EntitySerialization actor)
    {
        var handComponent = GameUtils.GetComponent<HandComponent>(actor);
        Debug.Assert(handComponent != null, $"HandComponent not found for actor: {actor.name}");
        _mainText.text = GameUtils.FormatHandComponent(handComponent);
    }

    /// <summary>
    /// 更新主文本显示，展示当前选中敌人的战斗属性和状态效果信息
    /// </summary>
    private void UpdateMainTextWithEnemyData(EntitySerialization actor)
    {
        // 临时先写最简单的。
        _mainText.text = actor.name;
    }


    /// <summary>
    /// 更新角色槽位显示，根据当前 mock 数据刷新每个槽位的角色信息
    /// </summary>
    private void UpdateActionOrder()
    {
        if (GameContext.Instance.IsLoggedIn)
        {
            // 这里是正式内容。
            // 获取最新的地下城回合信息
            Round round = GameUtils.GetLastRound(GameContext.Instance.Dungeon);
            if (round == null)
            {
                Debug.LogWarning("DungeonCombatScene: No round data found for current dungeon");
                return;
            }

            if (round.action_order == null || round.action_order.Count == 0)
            {
                Debug.LogWarning("DungeonCombatScene: No action order data found in current round");
                SetActionOrder(new List<EntitySerialization>()); // 传入空列表，隐藏所有槽位
                return;
            }

            Debug.Log($"DungeonCombatScene: Updating actor with {round.action_order.Count} actors in action order");

            // 根据当前回合的行动顺序获取对应的角色实体数据列表
            List<EntitySerialization> actorsInActionOrder = GameContext.Instance.GetActorEntitiesSerialization(round.action_order);

            // 根据当前回合的行动顺序更新角色槽位显示
            SetActionOrder(actorsInActionOrder);
        }
        else
        {
            // mock 数据的显示逻辑
            SetActionOrder(_mockActorData);
        }
    }

    /// <summary>
    /// 更新战斗信息文本，显示当前地下城和关卡信息
    /// </summary>
    private void UpdateCombatInfoText()
    {
        if (GameContext.Instance.IsLoggedIn)
        {
            var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.PlayerActor);
            _combatInfoText.text = $"{GameContext.Instance.Dungeon.name} | {stageName}";

            Combat currentCombat = GameUtils.GetLastCombat(GameContext.Instance.Dungeon);
            if (currentCombat != null)
            {
                var rounds = currentCombat.rounds != null ? currentCombat.rounds.Count : 0;
                _combatInfoText.text += $" | 回合数: {rounds}";
            }
            else
            {
                // _combatInfoText.text += " | 无战斗数据";
                Debug.LogWarning("DungeonCombatScene: No combat data found for current dungeon");
            }
        }
        else
        {
            Debug.LogWarning("DungeonCombatScene: Player is not logged in, cannot update combat info text");
            _combatInfoText.text = "---";
        }
    }

    /// <summary>
    /// 根据当前角色所在的地下城和关卡，动态更新场景背景图片
    /// </summary>
    private void UpdateBackgroundImage()
    {
        if (GameContext.Instance.IsLoggedIn)
        {
            var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.PlayerActor);
            Debug.Assert(stageName != "", "[GameStateSync] Current actor's stage name is empty");
            // 获取当前角色所在场景
            var cachedSprite = SpriteCacheManager.Instance.GetSprite(stageName);
            if (cachedSprite != null)
            {
                _backgroundImage.GetComponent<Image>().sprite = cachedSprite;
            }
            else
            {
                Debug.LogWarning($"DungeonCombatScene: Background sprite not found for stage: {stageName}");
                _backgroundImage.GetComponent<Image>().sprite = null;
            }
        }
        else
        {
            Debug.LogWarning("DungeonCombatScene: Player is not logged in, cannot update background image");

            var mockStageName = "场景.山林边缘";
            var cachedSprite = SpriteCacheManager.Instance.GetSprite(mockStageName);
            if (cachedSprite != null)
            {
                _backgroundImage.GetComponent<Image>().sprite = cachedSprite;
            }
            else
            {
                Debug.LogWarning($"DungeonCombatScene: Background sprite not found for mock stage: {mockStageName}");
                _backgroundImage.GetComponent<Image>().sprite = null;
            }
        }
    }

    /// <summary>
    /// 根据传入的角色列表设置角色槽位的显示内容
    /// 列表长度不足时，多余的槽位会被隐藏
    /// </summary>
    private void SetActionOrder(List<EntitySerialization> actors)
    {
        for (int i = 0; i < _actionOrderObjects.Length; i++)
        {
            if (i < actors.Count)
            {
                _actionOrderObjects[i].gameObject.SetActive(true);
                _actionOrderObjects[i].SetData(actors[i]);
                _actionOrderObjects[i].RefreshUI();
            }
            else
            {
                _actionOrderObjects[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 获取当前回合的所有演员列表
    /// 如果已登录，从地下城的最后一个回合获取行动顺序对应的演员数据
    /// 如果未登录，返回 mock 数据
    /// </summary>
    /// <returns>当前回合的演员列表，如果获取失败则返回 null</returns>
    private List<EntitySerialization> GetCurrentRoundActors()
    {
        List<EntitySerialization> allActors = null;
        if (GameContext.Instance.IsLoggedIn)
        {
            Round round = GameUtils.GetLastRound(GameContext.Instance.Dungeon);
            if (round != null && round.action_order != null)
            {
                Debug.Log($"Current round action order: {string.Join(", ", round.action_order)}");
                allActors = GameContext.Instance.GetActorEntitiesSerialization(round.action_order);
            }
            else
            {
                Debug.LogWarning("DungeonCombatScene: No round or action order data found for current dungeon");
            }
        }
        else
        {
            allActors = _mockActorData;
        }
        return allActors;
    }

    /// <summary>
    /// 根据当前战斗状态更新主对象的可交互状态
    /// </summary>
    private void UpdateCombatUIVisibility()
    {
        // 首先获取当前战斗状态，如果当前战斗对象不存在则默认设置为不可交互
        var combatState = GameUtils.GetLastCombatState(GameContext.Instance.Dungeon);

        // 当前战斗状态不是 ONGOING，主对象不可交互；当前战斗状态是 ONGOING，主对象可交互
        bool isInteractable = false;
        switch (combatState)
        {
            case CombatState.INITIALIZATION:
                isInteractable = false;
                break;
            case CombatState.ONGOING:
                isInteractable = true;
                break;
            case CombatState.COMPLETE:
                isInteractable = false;
                break;
            default:
                Debug.LogWarning($"Unknown combat state: {combatState}");
                break;
        }

        // 更新主对象和行动顺序对象的可见性
        _mainGameObject.SetActive(isInteractable);
        _bottomGameObject.SetActive(isInteractable);
    }

    /// <summary>
    /// 从选中角色加载卡牌要素数据
    /// 只负责维护 CardElements 部分，不处理 Build 数据
    /// </summary>
    /// <param name="selectedActor">选中的角色</param>
    /// <param name="allActors">当前可用的角色列表，其余角色将作为卡牌目标候选</param>
    private void LoadCardElementsFromActor(EntitySerialization selectedActor, List<EntitySerialization> allActors)
    {
        if (selectedActor == null)
        {
            Debug.LogWarning("[LoadCardElementsFromActor] selectedActor is null");
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
            if (actor.name != selectedActor.name)
            {
                CardBuilder.AddElement(new CardElementData(actor));
                targetCount++;
            }
        }
        Debug.Log($"[LoadCardElementsFromActor] 添加了 {targetCount} 个目标角色");

        // 更新滚动视图
        _scrollView.totalCount = CardBuilder.Count;
        _scrollView.RefillCells(); // 重建列表并回到顶部
        Debug.Log($"[LoadCardElementsFromActor] 总共加载 {CardBuilder.Count} 个卡牌要素");
    }

    /// <summary>
    /// 初始化战斗并刷新地下城状态
    /// 调用服务器 combat_init 接口开始战斗，成功后刷新并显示当前地下城状态
    /// </summary>
    private async UniTaskVoid ExecuteCombatInit()
    {
        bool apiSuccess = await GameStateSync.Instance.RefreshDungeonFromServer();

        if (!apiSuccess)
        {
            Debug.LogError("[DungeonCombatScene] Failed to refresh dungeon data");
            return;
        }

        var currentCombatState = GameUtils.GetLastCombatState(GameContext.Instance.Dungeon);
        if (currentCombatState != CombatState.INITIALIZATION)
        {
            Debug.LogWarning($"Current combat state is {currentCombatState}, expected INITIALIZATION. Proceeding with caution.");
            return;
        }

        var messages = await DungeonGamePlayManager.Instance.CombatInit();
        if (messages == null)
        {
            Debug.LogError("Combat initialization failed");
            return;
        }

        apiSuccess = await GameStateSync.Instance.RefreshDungeonAndActorsFromServer();
        if (!apiSuccess)
        {
            Debug.LogError("Failed to refresh dungeon data after combat init");
            return;
        }

        currentCombatState = GameUtils.GetLastCombatState(GameContext.Instance.Dungeon);
        if (currentCombatState != CombatState.ONGOING)
        {
            Debug.LogWarning($"Current combat state is {currentCombatState} after combat_init, expected ONGOING. Proceeding with caution.");
            return;
        }

        // 根据当前战斗状态更新主对象的可交互状态
        UpdateCombatUIVisibility();

        // 更新标题info
        UpdateCombatInfoText();

        // 更新角色槽位显示
        UpdateActionOrder();
    }

    /// <summary>
    /// 模拟战斗初始化流程，直接设置UI状态为可交互
    /// 仅用于测试UI状态切换逻辑，不涉及实际的服务器交互
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid ExecuteCombatInitMock()
    {
        await UniTask.Delay(1000);

        // 尝试性质刷新地下城状态，模拟服务器交互的结果
        UpdateCombatUIVisibility();
        _mainGameObject.SetActive(true);
        _bottomGameObject.SetActive(true);

        //
        UpdateCombatInfoText();

        //
        UpdateActionOrder();
    }

    /// <summary>
    /// 执行抽卡操作并轮询任务状态，完成后显示手牌
    /// 调用服务器 draw_cards 接口，获取任务ID后轮询查询任务状态
    /// 当任务完成时，刷新数据并显示角色手牌信息
    /// </summary>
    private async UniTaskVoid ExecuteDrawCards(List<AllyDrawCardAction> specifiedActions, bool enableEnemyDraw)
    {
        string taskId = await DungeonGamePlayManager.Instance.DrawCards(specifiedActions, enableEnemyDraw);
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

        bool apiSuccess = await GameStateSync.Instance.RefreshDungeonAndActorsFromServer();
        if (!apiSuccess)
        {
            Debug.LogError("Failed to refresh dungeon and actors data");
            return;
        }

        // 根据当前战斗状态更新主对象的可交互状态
        if (CardBuilder.Build.owner != null)
        {
            var refreshedOwner = GameContext.Instance.GetActorEntitySerialization(CardBuilder.Build.owner.name);
            Debug.Assert(refreshedOwner != null, $"Failed to get refreshed actor data for owner: {CardBuilder.Build.owner.name}");

            //这个时候，owner的数据已经被刷新了，GameContext会变化，所以需要根据名字再对其一次。
            CardBuilder.Clear();
            CardBuilder.Build = new CardBuildData { owner = refreshedOwner };

            var handComponent = GameUtils.GetComponent<HandComponent>(CardBuilder.Build.owner);
            if (handComponent != null)
            {
                // 有卡牌，就不能加载 Build 数据了，先显示手牌数据。
                UpdateMainTextWithHandData(CardBuilder.Build.owner);

                var allActors = GetCurrentRoundActors();
                LoadCardElementsFromActor(CardBuilder.Build.owner, allActors);
            }
        }

        // 任务完成后，获取当前选中角色的数据并显示手牌信息
        //_onHandComponentChangedEvent.Raise(new UIEventData(UIEventType.HandComponentChanged));
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

