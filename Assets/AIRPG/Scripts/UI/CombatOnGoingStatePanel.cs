using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CombatOnGoingStatePanel : MonoBehaviour, ICombatState, IUIEventListener
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
        RefreshCombatPanels(); // 重新刷新界面显示，确保站位面板等内容是最新的
    }

    /// <summary>
    /// 关闭敌方手牌面板
    /// </summary>
    public void OnClickCloseEnemyHandPanel()
    {
        _enemyHandPanel.gameObject.SetActive(false);
        RefreshCombatPanels(); // 重新刷新界面显示，确保站位面板等内容是最新的
    }

    /// <summary>
    /// 关闭仲裁面板
    /// </summary>
    public void OnClickCloseArbitrationPanel()
    {
        _arbitrationPanel.gameObject.SetActive(false);
        RefreshCombatPanels(); // 重新刷新界面显示，确保站位面板等内容是最新的
    }

    /// <summary>
    /// 点击出牌按钮后的异步处理逻辑：拉取最新战斗状态，根据结果决定显示仲裁面板还是跳转至战后状态。
    /// 未登录时使用随机 mock 数据模拟两种结果。
    /// </summary>
    private async UniTaskVoid OnPlayAsync()
    {
        // 获取当前战斗状态，如果当前战斗对象不存在则默认设置为 NONE，并在日志中输出警告信息
        CombatState lastCombatState;
        List<EntitySerialization> actorEntitiesInStage = new();

        if (!GameContext.Instance.IsLoggedIn)
        {
            await UniTask.Delay(0); // 模拟异步等待
            lastCombatState = CombatState.COMPLETE; // 模拟战斗完成状态
            Debug.LogWarning($"Player is not logged in, using mock combat state: {lastCombatState}");
        }
        else
        {
            // 阶段1：并行获取战斗状态和场景-演员映射关系（两者互相独立）
            var (combat, stagesState) = await UniTask.WhenAll(
                GameStateSync.Instance.GetCombat(),
                GameStateSync.Instance.GetStagesState()
            );

            if (combat == null || stagesState == null)
            {
                Debug.LogError("CombatOnGoingState: Dungeon data is null, cannot refresh combat view");
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

            actorEntitiesInStage = await GameStateSync.Instance.GetEntities(actorNamesInStage);
            if (actorEntitiesInStage == null)
            {
                Debug.LogError("CombatOnGoingState: Actor entities data is null, cannot refresh combat view");
                return;
            }

            lastCombatState = combat.state;
            Debug.Log($"[DungeonCombatScene] Last combat state: {lastCombatState}");
        }

        //
        switch (lastCombatState)
        {
            case CombatState.ONGOING:
                {
                    // 每个人都有手牌了，就进行战斗演绎
                    var hasHandComponentEntities = GameUtils.FilterEntitiesByComponent<HandComponent>(actorEntitiesInStage);
                    var hasDeathComponentEntities = GameUtils.FilterEntitiesByComponent<DeathComponent>(actorEntitiesInStage);
                    int aliveCount = actorEntitiesInStage.Count - hasDeathComponentEntities.Count;
                    if (aliveCount > 0 && hasHandComponentEntities.Count >= aliveCount)
                    {
                        // 所有活着的角色都有手牌了，可以执行行动了
                        Debug.Log("[DungeonCombatScene] Combat is ongoing, showing ongoing UI");
                        _arbitrationPanel.gameObject.SetActive(true);
                        _arbitrationPanel.EnterArbitrationPhaseAsync().Forget(); // 进入仒裁阶段
                    }
                    else
                    {
                        // 还有角色没有手牌，或者所有角色都死了，不能执行行动
                        Debug.Log("[DungeonCombatScene] Combat is ongoing but not all actors are ready, showing ongoing UI without arbitration");
                    }
                }
                break;

            case CombatState.COMPLETE:
                Debug.Log("[DungeonCombatScene] Combat is complete, showing post-combat UI");
                CombatScene.OnEnterPostCombatState();
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

        // 刷新顶部信息和站位面板（含未登录 mock 回退，由各组件内部自行处理）
        _topBar.gameObject.SetActive(true);
        _actorPositioningPanel.gameObject.SetActive(true);

        RefreshCombatPanels(); // 刷新界面显示，确保内容是最新的
    }

    /// <summary>
    /// 刷新顶部信息栏和站位面板，数据获取由各组件内部自行负责。
    /// </summary>
    private void RefreshCombatPanels()
    {
        _topBar.RefreshCombatStatusAsync().Forget();
        _actorPositioningPanel.RefreshCombatViewAsync().Forget();
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
                    var actorName = eventData.targetId;
                    var isEnemy = !string.IsNullOrEmpty(eventData.extraData) && eventData.extraData == "Enemy";
                    if (isEnemy)
                    {
                        _enemyHandPanel.gameObject.SetActive(true);
                        _enemyHandPanel.SetupForActorAsync(actorName).Forget();
                    }
                    else
                    {
                        _cardBuildPanel.gameObject.SetActive(true);
                        _cardBuildPanel.SetupForActorAsync(actorName).Forget();
                    }
                }
                break;

            default:
                Debug.LogWarning($"未处理的事件类型: {eventData.eventType}");
                break;
        }
    }
}
