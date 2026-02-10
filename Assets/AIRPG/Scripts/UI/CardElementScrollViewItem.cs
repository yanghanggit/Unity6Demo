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
    /// 修改 CardBuilder.Build 数据，然后触发UI事件通知刷新
    /// </summary>
    void OnClick()
    {
        Debug.Log($"Clicked on {_title.text} at index {_currentIndex}");

        // 尝试切换要素状态
        if (!TryToggleElementInBuild(_currentIndex))
        {
            return; // 修改失败，不发送事件
        }

        // 创建并发送结构化的事件数据，通知需要刷新UI
        var elementData = CardBuilder.GetElement(_currentIndex);
        var eventData = new UIEventData(
            UIEventType.CardElementScrollViewItemClick,
            elementData.Name,
            _currentIndex
        );

        _onCardElementClickedEvent.Raise(eventData);

        onUpdateItem(_currentIndex); // 立即更新显示状态
    }

    /// <summary>
    /// 切换指定索引的要素在构建数据中的状态（存在则删除，不存在则添加）
    /// </summary>
    /// <param name="elementIndex">要素索引</param>
    /// <returns>是否成功修改</returns>
    private bool TryToggleElementInBuild(int elementIndex)
    {
        // 检查卡牌构建数据是否存在
        if (CardBuilder.Build == null)
        {
            Debug.LogWarning("卡牌构建数据不存在，请先选择构建者（点击角色槽位）");
            return false;
        }

        // 从 CardBuilder 获取对应的要素数据
        var elementData = CardBuilder.GetElement(elementIndex);
        if (elementData == null)
        {
            Debug.LogWarning($"未找到索引为 {elementIndex} 的卡牌要素数据");
            return false;
        }

        // 根据要素类型修改 CardBuilder.Build 数据
        switch (elementData.elementType)
        {
            case CardElementType.TargetActor:
                if (elementData.targetActor != null)
                {
                    // 检查是否已存在（根据 name 判断）
                    var existingActorIndex = CardBuilder.Build.targetActors.FindIndex(
                        actor => actor.name == elementData.targetActor.name);

                    if (existingActorIndex >= 0)
                    {
                        // 已存在，删除该目标角色
                        CardBuilder.Build.targetActors.RemoveAt(existingActorIndex);
                        Debug.Log($"删除目标角色: {elementData.targetActor.name}");
                    }
                    else
                    {
                        // 不存在，添加新的目标角色
                        CardBuilder.Build.targetActors.Add(elementData.targetActor);
                        Debug.Log($"添加目标角色: {elementData.targetActor.name}");
                    }
                }
                break;

            case CardElementType.Skill:
                // 检查当前技能是否与要操作的技能相同（根据 name 判断）
                if (CardBuilder.Build.skill != null &&
                    !string.IsNullOrEmpty(CardBuilder.Build.skill.name) &&
                    CardBuilder.Build.skill.name == elementData.skill?.name)
                {
                    // 已存在相同技能，删除（设置为空技能）
                    CardBuilder.Build.skill = new Skill();
                    Debug.Log($"删除技能: {elementData.skill?.name}");
                }
                else
                {
                    // 不存在或不同，设置为新技能
                    CardBuilder.Build.skill = elementData.skill;
                    Debug.Log($"设置技能: {elementData.skill?.name ?? "[空技能]"}");
                }
                break;

            case CardElementType.StatusEffect:
                if (elementData.statusEffect != null)
                {
                    // 检查是否已存在（根据 name 判断）
                    var existingEffectIndex = CardBuilder.Build.statusEffects.FindIndex(
                        effect => effect.name == elementData.statusEffect.name);

                    if (existingEffectIndex >= 0)
                    {
                        // 已存在，删除该状态效果
                        CardBuilder.Build.statusEffects.RemoveAt(existingEffectIndex);
                        Debug.Log($"删除状态效果: {elementData.statusEffect.name}");
                    }
                    else
                    {
                        // 不存在，添加新的状态效果
                        CardBuilder.Build.statusEffects.Add(elementData.statusEffect);
                        Debug.Log($"添加状态效果: {elementData.statusEffect.name}");
                    }
                }
                break;

            case CardElementType.None:
            default:
                Debug.LogWarning($"未知的卡牌要素类型: {elementData.elementType}");
                return false;
        }

        return true;
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
        bool isSelected = IsElementSelectedInBuild(elementData);

        // 如果已选中，在名称后添加标记并调整显示效果
        string displayName = GameUtils.GetDisplayName(elementName);
        if (isSelected)
        {
            displayName += " <已选择>";
            // 已选中的项目背景色改为红色，便于测试阶段查看效果
            baseColor = Color.red;
        }

        _title.text = displayName;
        _background.color = baseColor;
    }

    /// <summary>
    /// 检查指定要素是否已在 CardBuilder.Build 中被选中
    /// </summary>
    /// <param name="elementData">要检查的要素数据</param>
    /// <returns>是否已选中</returns>
    private bool IsElementSelectedInBuild(CardElementData elementData)
    {
        if (CardBuilder.Build == null || elementData == null)
        {
            return false;
        }

        switch (elementData.elementType)
        {
            case CardElementType.TargetActor:
                if (elementData.targetActor != null)
                {
                    // 检查 targetActors 列表中是否包含该角色
                    return CardBuilder.Build.targetActors.Exists(
                        actor => actor.name == elementData.targetActor.name);
                }
                break;

            case CardElementType.Skill:
                // 检查 skill 是否匹配（name 相同且非空）
                if (elementData.skill != null &&
                    !string.IsNullOrEmpty(elementData.skill.name))
                {
                    return CardBuilder.Build.skill != null &&
                           !string.IsNullOrEmpty(CardBuilder.Build.skill.name) &&
                           CardBuilder.Build.skill.name == elementData.skill.name;
                }
                break;

            case CardElementType.StatusEffect:
                if (elementData.statusEffect != null)
                {
                    // 检查 statusEffects 列表中是否包含该效果
                    return CardBuilder.Build.statusEffects.Exists(
                        effect => effect.name == elementData.statusEffect.name);
                }
                break;
        }

        return false;
    }
}
