using UnityEngine;
using UnityEngine.UI;

public class DungeonOverviewListPanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private LoopVerticalScrollRect _scrollView; // 动态滚动视图

    void Start()
    {
        Debug.Assert(_scrollView != null, "ScrollView component is not assigned in the inspector.");
    }

    /// <summary>
    /// 刷新地下城概览列表显示,根据当前地下城数据刷新关卡列表显示
    /// </summary>
    public void RefreshScrollView()
    {
        _scrollView.totalCount = DungeonOverviewScene.DungeonOverviews.Count;
        _scrollView.RefillCells();
    }
}
