using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DungeonOverviewScrollViewItem : UIBehaviour, IScrollViewItem
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _title; // card名称文本

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onDungeonOverviewItemClickedEvent; // 地下城概览列表项被点击事件, 这个事件自己不可以再听了，是发送端，不能再监听了，否则会死循环。

    // 保存当前索引，用于事件传递
    private int _currentIndex = -1;

    /// <summary>
    /// 按钮点击事件处理方法
    /// </summary>
    public void OnClickItem()
    {
        var data = GetData();
        if (data == null)
        {
            Debug.LogError($"No data found for index {_currentIndex} in DungeonOverviews list");
            return;
        }

        // // 创建并发送结构化的事件数据，通知需要刷新UI
        var eventData = new UIEventData(
            UIEventType.DungeonOverviewItemClicked,
            data.name, // 可以根据需要传递更多数据，例如关卡名称、角色列表等   
            _currentIndex
        );

        // 触发事件，通知系统哪个地下城概览被点击了
        _onDungeonOverviewItemClickedEvent.Raise(eventData);
    }

    /// <summary>
    /// 实现IDynamicScrollViewItem接口的更新方法
    /// </summary>
    /// <param name="index"></param>
    public void OnUpdateItem(int index)
    {
        Debug.Assert(_title != null, "_title is not assigned in the inspector.");
        Debug.Assert(_onDungeonOverviewItemClickedEvent != null, "_onDungeonOverviewItemClickedEvent is not assigned in the inspector.");
        Debug.Assert(_onDungeonOverviewItemClickedEvent != null, "_onDungeonOverviewItemClickedEvent is null");

        // 保存当前索引
        _currentIndex = index;

        var data = GetData();
        if (data == null)
        {
            Debug.LogError($"No data found for index {_currentIndex} in DungeonOverviews list");
            return;
        }
        _title.text = data.name; // 更新UI显示
    }

    /// <summary>
    ///  获取当前索引对应的数据
    /// </summary>
    /// <returns></returns>
    private Dungeon GetData()
    {
        if (_currentIndex < 0 || _currentIndex >= DungeonOverviewScene.CachedDungeonOverviews.Count)
        {
            Debug.LogError($"Index {_currentIndex} is out of range for DungeonOverviews list");
            return null;
        }
        return DungeonOverviewScene.CachedDungeonOverviews[_currentIndex];
    }
}
