using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 卡牌要素滚动视图项组件
/// 用于在动态滚动视图中显示单个卡牌要素的信息和交互
/// </summary>
public class CardElementScrollViewItem : UIBehaviour, IScrollViewItem, IUIEventListener
{
    [Header("UI Components")]

    [SerializeField] private TMP_Text _title; // card名称文本
    [SerializeField] private Image _background; // 背景图片
    [SerializeField] private Button _button; // 覆盖层按钮,用于接收点击

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onCardElementClickedEvent; // 卡牌点击事件, 这个事件自己不可以再听了，是发送端，不能再监听了，否则会死循环。
    [SerializeField] private UIEventGameEvent _onCardBuilderDataChangedEvent; // CardBuilder 数据变化事件

    // 保存当前索引，用于事件传递
    private int _currentIndex = -1;

    /// <summary>
    /// 当组件被启用时调用
    /// 注册按钮点击事件监听
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();

        // 先移除listener，确保不会重复添加
        _button.onClick.RemoveListener(OnClick);
        _button.onClick.AddListener(OnClick);

        // 注册 CardBuilder 数据变化监听
        if (_onCardBuilderDataChangedEvent != null)
        {
            _onCardBuilderDataChangedEvent.RegisterListener(this);
        }
    }

    /// <summary>
    /// 当组件被禁用时调用
    /// 注销按钮点击事件监听,防止内存泄漏
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();

        // 注销按钮点击事件监听
        _button.onClick.RemoveListener(OnClick);

        // 取消注册
        if (_onCardBuilderDataChangedEvent != null)
        {
            _onCardBuilderDataChangedEvent.UnregisterListener(this);
        }
    }

    protected override void OnDestroy()
    {
        // 确保在销毁时清理事件监听，防止内存泄漏
        if (_onCardBuilderDataChangedEvent != null)
        {
            _onCardBuilderDataChangedEvent.UnregisterListener(this);
        }
    }

    /// <summary>
    /// 按钮点击事件处理
    /// 纯事件触发器：发送事件通知，不直接修改数据或UI显示状态
    /// 数据修改与UI更新由 TestDungeonCombatScenePrototype.OnEventRaised 负责
    /// </summary>
    void OnClick()
    {
        Debug.Log($"Clicked on {_title.text} at index {_currentIndex}");
        Debug.Assert(CardBuilder.Build.owner != null, "CardBuilder.Build.owner is null");

        // 创建并发送结构化的事件数据，通知需要刷新UI
        var elementData = CardBuilder.GetElement(_currentIndex);
        // 创建并发送结构化的事件数据，通知系统哪个卡牌要素被点击了
        var eventData = new UIEventData(
            UIEventType.CardElementScrollViewItemClick,
            elementData.Name,
            _currentIndex
        );

        // 触发事件，通知系统哪个卡牌要素被点击了
        Debug.Assert(_onCardElementClickedEvent != null, "_onCardElementClickedEvent is null");
        _onCardElementClickedEvent.Raise(eventData);
    }

    /// <summary>
    /// 实现IDynamicScrollViewItem接口的更新方法
    /// 根据索引更新显示的卡牌要素信息
    /// </summary>
    /// <param name="index">在滚动视图中的索引位置</param>
    public void OnUpdateItem(int index)
    {
        // 验证所有必需的UI组件引用
        Debug.Assert(_title != null, "_title != null");
        Debug.Assert(_background != null, "_background != null");

        // 保存当前索引
        _currentIndex = index;

        // 从 CardElementCollection 获取对应的卡牌要素数据
        var elementData = CardBuilder.GetElement(index);
        if (elementData == null)
        {
            _title.text = $"[错误] 索引 {index} 无数据";
            _background.color = Color.red;
            return;
        }

        // 根据要素类型显示对应的名字和基础颜色
        string elementName = string.Empty;
        Color baseColor = Color.white;

        switch (elementData.elementType)
        {
            case CardElementType.TargetActor:
                elementName = elementData.targetActor?.name ?? "[空角色]";
                baseColor = Color.cyan;
                break;

            case CardElementType.Skill:
                elementName = elementData.skill?.name ?? "[空技能]";
                baseColor = Color.green;
                break;

            case CardElementType.StatusEffect:
                elementName = elementData.statusEffect?.name ?? "[空状态]";
                baseColor = Color.yellow;
                break;

            case CardElementType.None:
            default:
                elementName = "[未知类型]";
                baseColor = Color.gray;
                break;
        }

        // 检查该要素是否已被选中
        bool isSelected = CardBuilder.IsElementSelectedInBuild(elementData);

        // 如果已选中，在名称后添加标记并调整显示效果
        string displayName = GameUtils.GetDisplayName(elementName);
        if (isSelected)
        {
            displayName += "\n(已选中)";
            baseColor = Color.red;// 已选中的项目背景色改为红色，便于测试阶段查看效果
        }

        _title.text = displayName;
        _background.color = baseColor;
    }

    /// <summary>
    /// IUIEventListener 接口实现
    /// 监听 CardBuilder 数据变化事件，自动刷新显示
    /// </summary>
    public void OnEventRaised(UIEventData eventData)
    {
        if (eventData.eventType == UIEventType.CardBuilderDataChanged)
        {
            // 刷新当前项显示状态
            OnUpdateItem(_currentIndex);
        }
    }
}
