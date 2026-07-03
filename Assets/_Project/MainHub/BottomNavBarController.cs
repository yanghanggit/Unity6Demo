using UnityEngine;
using TMPro;

public class BottomNavBarController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text[] _navTabButtonTexts; // 导航标签按钮文本数组，按顺序对应标签顺序
    [SerializeField] private TabContentPanelController _tabContentPanelController; // 主内容区（Tab页内容）控制器

    void Awake()
    {
        Debug.Assert(_navTabButtonTexts != null && _navTabButtonTexts.Length == (int)MainHubTab.Count, "_navTabButtonTexts is null or empty");
        Debug.Assert(_tabContentPanelController != null, "_tabContentPanelController is null");
    }

    void Start()
    {
        for (int i = 0; i < _navTabButtonTexts.Length; i++)
        {
            _navTabButtonTexts[i].text = $"标签{i + 1}"; // TODO: 待替换为实际标签名
        }

        _tabContentPanelController.HideAllTabPanels(); // 初始隐藏所有Tab内容

        OnNavTabButtonClicked((int)MainHubTab.Home);
    }

    /// <summary>供 Unity 按钮事件回调使用（传入枚举对应的整数下标）。</summary>
    public void OnNavTabButtonClicked(int tabIndex)
    {
        Debug.Log($"导航标签按钮 {tabIndex} 被点击");
        _tabContentPanelController.ShowTabPanel((MainHubTab)tabIndex);
    }

}
