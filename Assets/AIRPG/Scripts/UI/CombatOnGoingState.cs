using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CombatOnGoingState : MonoBehaviour, ICombatState, IUIEventListener
{
    [Header("UI Components")]
    [SerializeField] private CombatTopBar _topBar; // 顶部UI控制器
    [SerializeField] private ActorPositioningPanel _actorPositioningPanel; // 角色站位面板控制器
    [SerializeField] private ArbitrationPanel _arbitrationPanel; // 仲裁面板对象
    [SerializeField] private CardBuildPanel _cardBuildPanel; // 卡牌构筑面板
    [SerializeField] private EnemyHandPanel _enemyHandPanel; // 敌方手牌面板

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onActorPositioningClickedEvent; // 角色站位点击事件

    public ICombatScene CombatScene { get; set; } // 实现 ICombatState 接口的 CombatScene 属性，用于接收当前战斗场景的引用

    void OnDestroy()
    {
        if (_onActorPositioningClickedEvent != null)
        {
            _onActorPositioningClickedEvent.UnregisterListener(this);
        }
    }

    void Start()
    {
        Debug.Assert(_topBar != null, "_topBar is null");
        Debug.Assert(_actorPositioningPanel != null, "_actorPositioningPanel is null");
        Debug.Assert(_arbitrationPanel != null, "_arbitrationPanel is null");
        Debug.Assert(_cardBuildPanel != null, "_cardBuildPanel is null");
        Debug.Assert(_enemyHandPanel != null, "_enemyHandPanel is null");
        Debug.Assert(_onActorPositioningClickedEvent != null, "_onActorPositioningClickedEvent is null");

        _onActorPositioningClickedEvent.RegisterListener(this);
    }

    /// <summary>
    /// 点击顶部信息按钮的处理逻辑
    /// </summary>
    public void OnClickPlayButton()
    {
        Debug.Log("Top Info Button Clicked");
        OnPlayAsync().Forget();
    }

    /// <summary>
    /// 关闭卡牌构筑面板，并重新刷新站位面板以显示最新数据
    /// </summary>
    public void OnClickCloseCardBuildPanel()
    {
        _cardBuildPanel.gameObject.SetActive(false);
        RefreshPositioningAsync().Forget(); // 重新刷新界面显示，确保站位面板等内容是最新的
    }

    /// <summary>
    /// 关闭敌方手牌面板
    /// </summary>
    public void OnClickCloseEnemyHandPanel()
    {
        _enemyHandPanel.gameObject.SetActive(false);
        RefreshPositioningAsync().Forget(); // 重新刷新界面显示，确保站位面板等内容是最新的
    }

    /// <summary>
    /// 关闭仲裁面板
    /// </summary>
    public void OnClickCloseArbitrationPanel()
    {
        _arbitrationPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// 点击出牌按钮后的异步处理逻辑：拉取最新战斗状态，根据结果决定显示仲裁面板还是跳转至战后状态。
    /// 未登录时使用随机 mock 数据模拟两种结果。
    /// </summary>
    private async UniTaskVoid OnPlayAsync()
    {
        // 获取当前战斗状态，如果当前战斗对象不存在则默认设置为 NONE，并在日志中输出警告信息
        CombatState lastCombatState = CombatState.NONE;

        if (!GameContext.Instance.IsLoggedIn)
        {
            // 模拟未登录用户的战斗状态，这里直接设置为 ONGOING，后续可以根据需要调整为其他状态
            await UniTask.Delay(100);

            // 随机一个0～100 之间的数，模拟不同的战斗状态
            int randomValue = Random.Range(0, 101);
            if (randomValue < 50)
            {
                _arbitrationPanel.gameObject.SetActive(true);
                //_arbitrationPanel.LastRound = GameUtils.GetLastRound(GameContext.Instance.Dungeon); // 显示最新的回合信息
            }
            else
            {
                CombatScene.OnEnterPostCombatState();
            }
            return;
        }
        else
        {
            var combat = await GameStateSync.Instance.GetCombat();
            if (combat == null)
            {
                Debug.LogError("[DungeonCombatScene] Combat data is null after refresh");
                return;
            }

            lastCombatState = combat.state;
            Debug.Log($"[DungeonCombatScene] Last combat state: {lastCombatState}");
        }

        //
        switch (lastCombatState)
        {
            case CombatState.ONGOING:
                Debug.Log("[DungeonCombatScene] Combat is ongoing, showing ongoing UI");
                break;

            case CombatState.COMPLETE:
                Debug.Log("[DungeonCombatScene] Combat is complete, showing post-combat UI");
                break;

            case CombatState.POST_COMBAT:
                Debug.Log("[DungeonCombatScene] Combat is in post-combat state, showing post-combat UI");
                break;

            default:
                Debug.LogWarning($"Unknown combat state: {lastCombatState}, skipping combat initialization");
                break;
        }
    }

    /// <summary>
    /// 进入战斗进行中状态时的处理逻辑，包含根据当前游戏状态刷新 UI 显示内容的逻辑。
    /// </summary>
    public void OnEnter()
    {
        // 进入战斗进行中状态时，默认先隐藏卡牌构筑面板和敌人手牌面板，确保界面干净
        _cardBuildPanel.gameObject.SetActive(false);
        _enemyHandPanel.gameObject.SetActive(false);
        _arbitrationPanel.gameObject.SetActive(false);
        _actorPositioningPanel.gameObject.SetActive(false);

        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("CombatOnGoingState: Player is not logged in, using mock data to display action order panel");

            // 使用 mock 数据来刷新顶部信息显示
            _topBar.SetText("Mock Dungeon | Mock Stage | 回合数: 1");

            // positioning 显示。
            _actorPositioningPanel.gameObject.SetActive(true);
            _actorPositioningPanel.RefreshView(MockData.CreateActorData(), new List<string>());
        }
        else
        {
            RefreshPositioningAsync().Forget();
        }
    }

    /// <summary>
    /// 异步刷新站位面板及顶部信息栏：
    /// 并行拉取战斗数据与场景-演员映射，找到玩家所在场景的演员列表，
    /// 按创建顺序排序后更新站位面板显示。
    /// </summary>
    private async UniTaskVoid RefreshPositioningAsync()
    {
        // 阶段1：并行获取战斗状态和场景-演员映射关系（两者互相独立）
        var (combat, stagesState) = await UniTask.WhenAll(
            GameStateSync.Instance.GetCombat(),
            GameStateSync.Instance.GetStagesState()
        );

        if (combat == null)
        {
            Debug.LogError("CombatOnGoingState: Dungeon data is null, cannot refresh combat view");
            return;
        }

        if (stagesState == null)
        {
            Debug.LogError("CombatOnGoingState: Stages state data is null, cannot determine current stage and actors");
            return;
        }

        // 阶段2：依据映射结果获取当前场景中的演员列表
        List<string> actorNamesInStage = new();
        foreach (var kvp in stagesState)
        {
            if (kvp.Value.Contains(GameContext.Instance.PlayerActorName))
            {
                actorNamesInStage = kvp.Value;
                break;
            }
        }

        var actorEntitiesInStage = await GameStateSync.Instance.GetEntities(actorNamesInStage);
        if (actorEntitiesInStage == null)
        {
            Debug.LogError("CombatOnGoingState: Actor entities data is null, cannot refresh combat view");
            return;
        }

        // 刷新顶部信息显示，包含当前地下城、关卡和回合数等信息
        var topBarInfo = $"{DungeonCombatScene2.CachedDungeonName} | {DungeonCombatScene2.CachedStageName} | 回合数: {combat.rounds.Count}";
        _topBar.SetText(topBarInfo);

        //
        var round = combat.rounds.Count > 0 ? combat.rounds[^1] : null;
        Debug.Assert(round != null, "Combat has no rounds data");

        // 排序一下，不然每次都是乱的。
        var sortedByCreationOrder = GameUtils.SortActorsByCreationOrder(actorEntitiesInStage);
        Debug.Log($"Sorted actor entities by creation order: {string.Join(", ", sortedByCreationOrder.ConvertAll(e => e.name))}");

        // 站位面板显示
        _actorPositioningPanel.gameObject.SetActive(true);
        _actorPositioningPanel.RefreshView(sortedByCreationOrder, round.action_order);
    }

    /// <summary>
    /// 实现 IUIEventListener 接口的方法，用于接收 UI 事件并根据事件类型执行相应的处理逻辑。
    /// </summary>
    public void OnEventRaised(UIEventData eventData)
    {
        Debug.Log($"[TestDungeonCombatScenePrototype] {eventData}");
        Debug.Log($"OnEventRaised: {eventData.eventType}, TargetId: {eventData.targetId}, Index: {eventData.index}, ExtraData: {eventData.extraData}");

        switch (eventData.eventType)
        {

            case UIEventType.ActorPositioningClicked:
                {
                    Debug.Log($"角色站位被点击，目标角色: {eventData.targetId}");
                    var actorName = eventData.targetId;
                    if (!GameContext.Instance.IsLoggedIn)
                    {
                        var mockData = MockData.CreateActorData();
                        var selectedActor = mockData.Find(actor => actor.name == actorName);
                        Debug.Assert(selectedActor != null, $"MockData does not contain actor with name: {actorName}");
                        var enemyComponent = GameUtils.GetComponent<EnemyComponent>(selectedActor);
                        if (enemyComponent != null)
                        {
                            _enemyHandPanel.gameObject.SetActive(true);
                            _enemyHandPanel.SetupForActor(selectedActor);
                        }
                        else
                        {
                            _cardBuildPanel.gameObject.SetActive(true);
                            _cardBuildPanel.SetupForActor(selectedActor, mockData);
                        }
                    }
                    else
                    {
                        // 已登录用户的处理逻辑
                        Debug.Log($"Player is logged in, handling actor positioning click with real data for actor: {actorName}");
                        OnHandleActorPositioningClicked(eventData).Forget();
                    }
                }
                break;

            default:
                Debug.LogWarning($"未处理的事件类型: {eventData.eventType}");
                break;
        }
    }

    /// <summary>
    /// 处理已登录用户点击站位角色的异步逻辑：
    /// 拉取战斗及演员数据，判断点击目标是敌方或己方，
    /// 敌方显示手牌面板，己方按行动顺序排序后显示卡牌构筑面板。
    /// </summary>
    /// <param name="eventData">包含被点击角色名称（targetId）的 UI 事件数据</param>
    private async UniTaskVoid OnHandleActorPositioningClicked(UIEventData eventData)
    {
        Debug.Log($"角色站位被点击，目标角色: {eventData.targetId}");
        // 这里可以添加点击角色站位的处理逻辑，例如显示该角色的详细信息或者切换选中状态等

        // 阶段1：并行获取战斗状态和场景-演员映射关系（两者互相独立）
        var (combat, stagesState) = await UniTask.WhenAll(
            GameStateSync.Instance.GetCombat(),
            GameStateSync.Instance.GetStagesState()
        );

        if (combat == null || combat.rounds == null)
        {
            Debug.LogError("CombatOnGoingState: Combat or rounds data is null, cannot handle actor positioning click");
            return;
        }

        // 阶段2：依据映射结果获取当前场景中的演员列表
        List<string> actorNamesInStage = new();
        foreach (var kvp in stagesState)
        {
            if (kvp.Value.Contains(GameContext.Instance.PlayerActorName))
            {
                actorNamesInStage = kvp.Value;
                break;
            }
        }

        var actorEntitiesInStage = await GameStateSync.Instance.GetEntities(actorNamesInStage);
        if (actorEntitiesInStage == null)
        {
            Debug.LogError("CombatOnGoingState: Actor entities data is null, cannot refresh combat view");
            return;
        }

        // 从 actorEntities 中找到被点击的角色实体数据
        var actorName = eventData.targetId;
        EntitySerialization selectedActorEntity = null;
        foreach (var entity in actorEntitiesInStage)
        {
            if (entity.name == actorName)
            {
                selectedActorEntity = entity;
                break;
            }
        }

        if (selectedActorEntity == null)
        {
            Debug.LogError($"No actor entity found with name: {actorName}");
            return;
        }

        var enemyComponent = GameUtils.GetComponent<EnemyComponent>(selectedActorEntity);
        if (enemyComponent != null)
        {
            Debug.Log($"Clicked on enemy actor: {selectedActorEntity.name}, showing enemy hand panel");
            _enemyHandPanel.gameObject.SetActive(true);
            _enemyHandPanel.SetupForActor(selectedActorEntity);
        }
        else
        {
            var round = combat.rounds.Count > 0 ? combat.rounds[^1] : null;
            Debug.Assert(round != null, "Combat has no rounds data");

            // 请注意 actorEntitiesInStage，如果不在 round.action_order 中的就移除。
            // 如果在就进行排序，确保顺序和 round.action_order 一致
            List<EntitySerialization> sortedActorEntities = GameUtils.SortActorsByActionOrder(actorEntitiesInStage, round.action_order);
            Debug.Assert(sortedActorEntities.Count > 0, "Sorted actor entities list is empty after sorting by action order");
            Debug.Log($"Sorted actor entities by action order: {string.Join(", ", sortedActorEntities.ConvertAll(e => e.name))}");

            // 最后再把剩余的（不在 action_order 中的）添加到列表末尾
            Debug.Log($"Clicked on member actor: {selectedActorEntity.name}, showing card build panel");
            _cardBuildPanel.gameObject.SetActive(true);
            _cardBuildPanel.SetupForActor(selectedActorEntity, sortedActorEntities);
        }
    }
}
