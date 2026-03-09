using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 卡牌要素滚动视图项组件
/// 用于在动态滚动视图中显示单个卡牌要素的信息和交互
/// </summary>
public class HomeSceneStateScrollViewItem : UIBehaviour, IScrollViewItem
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _title; // card名称文本

    // 保存当前索引，用于事件传递
    private int _currentIndex = -1;

    /// <summary>
    /// 当组件被启用时调用
    /// 注册按钮点击事件监听
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    /// <summary>
    /// 当组件被禁用时调用
    /// 注销按钮点击事件监听,防止内存泄漏
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnDestroy()
    {
      
    }

   
    /// <summary>
    /// 实现IDynamicScrollViewItem接口的更新方法
    /// 根据索引更新显示的卡牌要素信息
    /// </summary>
    /// <param name="index">在滚动视图中的索引位置</param>
    public void OnUpdateItem(int index)
    {
        Debug.Assert(_title != null, "_title is null in HomeSceneStateScrollViewItem");

        // 保存当前索引
        _currentIndex = index;

        _title.text = $"Home Scene State {index}";
    }
}
