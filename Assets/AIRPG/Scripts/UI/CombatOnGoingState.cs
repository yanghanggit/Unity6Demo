using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatOnGoingState : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private ActionOrderPanel _actionOrderPanel; // 行动顺序面板控制器
    [SerializeField] private CardBuildPanel _cardBuildPanel; // 卡牌构筑面板控制器
    [SerializeField] private TMP_Text _infoText; // 信息文本显示对象

    private List<EntitySerialization> _mockActorData;

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
    }

    /// <summary>
    /// 点击顶部信息按钮的处理逻辑
    /// </summary>
    public void OnClickInfoButton()
    {
        Debug.Log("Top Info Button Clicked");
    }

    public void OnShow()
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
            return;
        }



    }
}
