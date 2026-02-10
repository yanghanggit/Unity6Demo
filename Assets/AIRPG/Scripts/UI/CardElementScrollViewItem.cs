using Mosframe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 卡牌要素滚动视图项组件
/// 用于在动态滚动视图中显示单个卡牌要素的信息和交互
/// </summary>
public class CardElementScrollViewItem : UIBehaviour, IDynamicScrollViewItem
{
    [Header("UI Components")]

    [SerializeField] private TMP_Text _title; // card名称文本
    [SerializeField] private Image _background; // 背景图片
    [SerializeField] private Button _overlayButton;             // 覆盖层按钮,用于接收点击
    [SerializeField] private UIEventGameEvent _onCardElementClickedEvent; // 卡牌点击事件

    [Header("Debug Colors")]
    private readonly Color[] colors = new Color[] {
            Color.cyan,
            Color.green,
        };

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
        _overlayButton.onClick.RemoveListener(OnClick);
        _overlayButton.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// 当组件被禁用时调用
    /// 注销按钮点击事件监听,防止内存泄漏
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        _overlayButton.onClick.RemoveListener(OnClick);
    }

    /// <summary>
    /// 按钮点击事件处理
    /// 触发UI事件,传递结构化的事件数据
    /// </summary>
    void OnClick()
    {
        Debug.Log($"Clicked on {_title.text} at index {_currentIndex}");

        var elementData = CardElementCollection.GetElement(_currentIndex);

        // 创建并发送结构化的事件数据
        var eventData = new UIEventData(
            UIEventType.CardElementScrollViewItemClick, // 事件类型
            elementData.Name, // 传递要素名称
            _currentIndex
        );

        _onCardElementClickedEvent.Raise(eventData);
    }

    /// <summary>
    /// 实现IDynamicScrollViewItem接口的更新方法
    /// 根据索引更新显示的卡牌要素信息
    /// </summary>
    /// <param name="index">在滚动视图中的索引位置</param>
    public void onUpdateItem(int index)
    {
        // 验证所有必需的UI组件引用
        Debug.Assert(_title != null, "_title != null");
        Debug.Assert(_background != null, "_background != null");

        // 保存当前索引
        _currentIndex = index;

        // 从 CardElementCollection 获取对应的卡牌要素数据
        var elementData = CardElementCollection.GetElement(index);

        if (elementData == null)
        {
            _title.text = $"[错误] 索引 {index} 无数据";
            _background.color = Color.red;
            return;
        }

        // 根据要素类型显示对应的名字
        string elementName = string.Empty;
        switch (elementData.elementType)
        {
            case CardElementType.TargetActor:
                elementName = elementData.targetActor?.name ?? "[空角色]";
                _background.color = colors[0]; // cyan for actors
                break;

            case CardElementType.Skill:
                elementName = elementData.skill?.name ?? "[空技能]";
                _background.color = colors[1]; // green for skills
                break;

            case CardElementType.StatusEffect:
                elementName = elementData.statusEffect?.name ?? "[空状态]";
                _background.color = Color.yellow; // yellow for effects
                break;

            case CardElementType.None:
            default:
                elementName = "[未知类型]";
                _background.color = Color.gray;
                break;
        }

        _title.text = GameUtils.GetDisplayName(elementName);
    }
}
