using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CombatOnGoingState : MonoBehaviour, ICombatState
{
    [Header("UI Components")]
    [SerializeField] private ActorPositioningPanel _actorPositioningPanel; // 角色站位面板控制器
    [SerializeField] private TMP_Text _infoText; // 信息文本显示对象
    [SerializeField] private ArbitrationPanel _arbitrationPanel; // 仲裁面板对象

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
        Debug.Assert(_actorPositioningPanel != null, "_actorPositioningPanel is null");
        //Debug.Assert(_actionOrderPanel != null, "_actionOrderPanel is null");
        //Debug.Assert(_cardBuildPanel != null, "_cardBuildPanel is null");
        Debug.Assert(_infoText != null, "_infoText is null");
        Debug.Assert(_arbitrationPanel != null, "_arbitrationPanel is null");
    }

    /// <summary>
    /// 点击顶部信息按钮的处理逻辑
    /// </summary>
    public void OnClickInfoButton()
    {
        Debug.Log("Top Info Button Clicked");
        _arbitrationPanel.gameObject.SetActive(true);
        _arbitrationPanel.LastRound = GameUtils.GetLastRound(GameContext.Instance.Dungeon); // 显示最新的回合信息
    }

    /// <summary>
    /// 点击仲裁面板关闭按钮的处理逻辑
    /// </summary>
    public void OnClickCloseArbitrationPanel()
    {
        Debug.Log("Close Arbitration Panel Button Clicked");
        _arbitrationPanel.gameObject.SetActive(false);

        // OnEnter(); // 重新显示当前状态的 UI

        // 测试一下
        // CombatScene.SwitchCombatState(CombatState.POST_COMBAT);
    }

    /// <summary>
    /// 进入战斗进行中状态时的处理逻辑，包含根据当前游戏状态刷新 UI 显示内容的逻辑。
    /// </summary>
    public void OnEnter()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("CombatOnGoingState: Player is not logged in, using mock data to display action order panel");

            // 使用 mock 数据来刷新顶部信息显示
            CombatScene.SetTopBarInfo("Mock Dungeon | Mock Stage | 回合数: 1");

            // positioning 显示。
            _actorPositioningPanel.gameObject.SetActive(true);
            _actorPositioningPanel.ActorEntities = _mockActorData;
            _actorPositioningPanel.RefreshPositioningView();
            _actorPositioningPanel.HideCardBuildPanel();

            // 使用 mock 数据来显示行动顺序面板
            // _actionOrderPanel.gameObject.SetActive(true);
            // _actionOrderPanel.ActorEntities = _mockActorData;

            // // 初始化卡牌构筑面板，默认选中第一个角色
            // _cardBuildPanel.gameObject.SetActive(true);
            // _cardBuildPanel.ActorEntities = _mockActorData;
            // _cardBuildPanel.CurrentActor = _mockActorData[0]; // 默认选中

            // //
            // _infoText.text = "1/3 角色行动中... (使用 mock 数据)";

            // //
            // _arbitrationPanel.gameObject.SetActive(false); // 默认隐藏仲裁面板
            return;
        }

        Debug.Log("Refreshing Combat OnGoing State view with real game data");

        RefreshAsync().Forget();
    }

    /// <summary>
    /// 异步刷新战斗进行中状态的 UI 显示，包含从服务器获取最新战斗状态数据并更新各个面板显示。
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid RefreshAsync()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("CombatOnGoingState: Player is not logged in, using mock data for ActionOrderPanelActorEntities");
            return;
        }

        var refreshErr = await GameStateSync.Instance.RefreshCombatStateFromServer();
        if (refreshErr != GameSyncError.None)
        {
            Debug.LogError($"CombatOnGoingState: Failed to refresh combat state from server, error: {refreshErr}");
            return;
        }

        Round round = GameUtils.GetLastRound(GameContext.Instance.Dungeon);
        if (round == null || round.action_order == null)
        {
            Debug.LogWarning("CombatOnGoingState: No round or action order data found for current dungeon");
            return;
        }

        var actionOrderEntities = GameContext.Instance.GetActorEntities(round.action_order);
        if (actionOrderEntities == null || actionOrderEntities.Count == 0)
        {
            Debug.LogWarning("CombatOnGoingState: No action order entities found, cannot refresh view");
            return;
        }

        Combat currentCombat = GameUtils.GetLastCombat(GameContext.Instance.Dungeon);
        Debug.Assert(currentCombat != null, "CombatOnGoingState: Current combat is null, cannot refresh view");

        // 顶部信息！
        var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.PlayerActorName);
        var topBarInfo = $"{GameContext.Instance.Dungeon.name} | {stageName} | 回合数: {currentCombat.rounds.Count}";
        CombatScene.SetTopBarInfo(topBarInfo);

        // 显示行动顺序面板并设置数据
        // _actionOrderPanel.gameObject.SetActive(true);
        // _actionOrderPanel.ActorEntities = actionOrderEntities;

        // 显示卡牌构筑面板并设置数据
        // _cardBuildPanel.gameObject.SetActive(true);
        // _cardBuildPanel.ActorEntities = actionOrderEntities;
        // _cardBuildPanel.CurrentActor = actionOrderEntities[0]; // 默认选中第一个角色

        // 显示顶部信息文本
        _infoText.text = $"1/{actionOrderEntities.Count} 角色行动中...";

        // 默认隐藏仲裁面板，直到玩家点击信息按钮
        _arbitrationPanel.gameObject.SetActive(false);
    }
}
