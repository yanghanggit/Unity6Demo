using UnityEngine;

public class TabContentPanelController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject[] _tabPanels; // 各个Tab页的内容面板

    void Awake()
    {
        Debug.Assert(_tabPanels != null && _tabPanels.Length == 5, "_tabPanels is null or empty");
    }

    /// <summary>
    /// 显示指定索引的Tab内容面板，并隐藏其他面板
    /// </summary>
    /// <param name="tabIndex"></param>
    public void ShowTabPanel(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= _tabPanels.Length)
        {
            Debug.LogError($"Tab索引 {tabIndex} 超出范围");
            return;
        }

        for (int i = 0; i < _tabPanels.Length; i++)
        {
            _tabPanels[i].SetActive(i == tabIndex);
        }
    }

    /// <summary>
    /// 隐藏所有Tab内容面板
    /// </summary>
    public void HideAllTabPanels()
    {
        foreach (var panel in _tabPanels)
        {
            panel.SetActive(false);
        }
    }
}
