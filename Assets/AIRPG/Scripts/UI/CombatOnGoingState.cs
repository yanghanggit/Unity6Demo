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
            _topBar.SetInfoText("Mock Dungeon | Mock Stage | 回合数: 1");

            // positioning 显示。
            _actorPositioningPanel.gameObject.SetActive(true);
            _actorPositioningPanel.ActorEntities = _mockActorData;
            _actorPositioningPanel.RefreshPositioningView();
            _actorPositioningPanel.HideCardBuildPanel();


            // 先关掉仲裁面板，避免显示错误数据
            HideArbitrationPanel();
            return;
        }

        //Debug.Log("Refreshing Combat OnGoing State view with real game data");

        OnEnterAsync().Forget();
    }

    /// <summary>
    /// 异步刷新战斗进行中状态的 UI 显示，包含从服务器获取最新战斗状态数据并更新各个面板显示。
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid OnEnterAsync()
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
        _topBar.SetInfoText(topBarInfo);
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
