using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatOnGoingState : MonoBehaviour, ICombatState
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _infoText; // 信息文本显示对象
    [SerializeField] private ActionOrderPanel _actionOrderPanel; // 行动顺序面板控制器
    [SerializeField] private CardBuildPanel _cardBuildPanel; // 卡牌构筑面板控制器
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
        Debug.Assert(_actionOrderPanel != null, "_actionOrderPanel is null");
        Debug.Assert(_cardBuildPanel != null, "_cardBuildPanel is null");
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

        RefreshView(); // 重新显示当前状态的 UI

        // 测试一下
        // CombatScene.SwitchCombatState(CombatState.POST_COMBAT);
    }

    /// <summary>
    /// 根据新的战斗状态切换 UI 显示和交互逻辑
    /// </summary>
    public void RefreshView()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("CombatOnGoingState: Player is not logged in, using mock data to display action order panel");

            // 使用 mock 数据来显示行动顺序面板
            _actionOrderPanel.gameObject.SetActive(true);
            _actionOrderPanel.ActorEntities = _mockActorData;

            // 初始化卡牌构筑面板，默认选中第一个角色
            _cardBuildPanel.gameObject.SetActive(true);
            _cardBuildPanel.ActorEntities = _mockActorData;
            _cardBuildPanel.CurrentActor = _mockActorData[0]; // 默认选中

            //
            _infoText.text = "1/3 角色行动中... (使用 mock 数据)";

            //
            _arbitrationPanel.gameObject.SetActive(false); // 默认隐藏仲裁面板
            return;
        }
    }
}
