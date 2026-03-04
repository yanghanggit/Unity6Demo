using System.Collections.Generic;
//using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CombatOnGoingState : MonoBehaviour, ICombatState
{
    [Header("UI Components")]
    [SerializeField] private ActorPositioningPanel _actorPositioningPanel; // 角色站位面板控制器
    [SerializeField] private ArbitrationPanel _arbitrationPanel; // 仲裁面板对象
    [SerializeField] private CombatTopBar _topBar; // 顶部UI控制器
    // 用于存储 mock 数据的字段
    private List<EntitySerialization> _mockActorData;

    // 实现 ICombatState 接口的 CombatScene 属性，用于接收当前战斗场景的引用
    public ICombatScene CombatScene { get; set; }

    void Awake()
    {
        // 创建 mock 数据
        _mockActorData = MockData.CreateActorData();
    }

    void Start()
    {
        Debug.Assert(_topBar != null, "_topBar is null");
        Debug.Assert(_actorPositioningPanel != null, "_actorPositioningPanel is null");
        Debug.Assert(_arbitrationPanel != null, "_arbitrationPanel is null");
    }

    /// <summary>
    /// 点击顶部信息按钮的处理逻辑
    /// </summary>
    public void OnClickPlayButton()
    {
        Debug.Log("Top Info Button Clicked");
        OnPlayAsync().Forget();
    }

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
                ShowArbitrationPanel();
            }
            else
            {
                CombatScene.OnEnterPostCombatState();
            }
            return;
        }
        else
        {
            // 先刷新一次
            // var refreshErr = await GameStateSync.Instance.RefreshCombatStateFromServer();
            // if (refreshErr != GameSyncError.None)
            // {
            //     Debug.LogError($"[DungeonCombatScene] Failed to refresh combat state from server: {refreshErr}");
            //     return;
            // }

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
        _actorPositioningPanel.HideCardBuildPanel();
        _actorPositioningPanel.HideEnemyHandPanel();

        // 先关掉仲裁面板，避免显示错误数据
        HideArbitrationPanel();

        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("CombatOnGoingState: Player is not logged in, using mock data to display action order panel");

            // 使用 mock 数据来刷新顶部信息显示
            _topBar.SetText("Mock Dungeon | Mock Stage | 回合数: 1");

            // positioning 显示。
            _actorPositioningPanel.gameObject.SetActive(true);
            _actorPositioningPanel.ActorEntities = _mockActorData;
            _actorPositioningPanel.RefreshPositioningView();
            return;

        }

        OnEnterAsync().Forget();
    }

    private async UniTaskVoid OnEnterAsync()
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

        var actorEntities = await GameStateSync.Instance.GetEntities(actorNamesInStage);
        if (actorEntities == null)
        {
            Debug.LogError("CombatOnGoingState: Actor entities data is null, cannot refresh combat view");
            return;
        }

        // 等待1秒钟，模拟加载过程中的等待时间，提升用户体验
        //await UniTask.Delay(1000 * 60);


        // 刷新顶部信息显示，包含当前地下城、关卡和回合数等信息
        var topBarInfo = $"{DungeonCombatScene2.DungeonName} | {DungeonCombatScene2.StageName} | 回合数: {combat.rounds.Count}";
        _topBar.SetText(topBarInfo);

        // 站位面板显示
        _actorPositioningPanel.gameObject.SetActive(true);
        _actorPositioningPanel.ActorEntities = actorEntities;
        _actorPositioningPanel.RefreshPositioningView();
    }

    /// <summary>
    /// 从当前的角色实体列表中筛选出需要在站位面板中显示的角色实体，通常是玩家队伍中的角色。
    /// </summary>    
    public void HideArbitrationPanel()
    {
        _arbitrationPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示仲裁面板并刷新显示内容，通常在点击顶部信息按钮时调用。
    /// </summary>
    public void ShowArbitrationPanel()
    {
        _arbitrationPanel.gameObject.SetActive(true);
        _arbitrationPanel.LastRound = GameUtils.GetLastRound(GameContext.Instance.Dungeon); // 显示最新的回合信息
    }
}
