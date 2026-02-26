/// <summary>
/// UI事件类型枚举
/// 定义所有可能的UI交互事件类型
/// </summary>
public enum UIEventType
{
    None = 0, // 无事件
    CardElementScrollViewItemClick = 10, // CombatSceneUI 卡牌滚动视图项点击事件
    ActorOrderSlotClick = 20, // CombatSceneUI 角色执行顺序槽位相关事件
    CardBuilderDataChanged = 30, // CardBuilder.Build 数据被修改时触发
}

/// <summary>
/// UI事件数据类
/// 用于在UI组件之间传递结构化的事件信息
/// </summary>
[System.Serializable]
public class UIEventData
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public UIEventType eventType;

    /// <summary>
    /// 目标对象ID（如角色名、卡牌ID等）
    /// </summary>
    public string targetId;

    /// <summary>
    /// 索引位置（用于列表项、槽位等）
    /// </summary>
    public int index;

    /// <summary>
    /// 额外的自定义数据（可选）
    /// </summary>
    public string extraData;

    /// <summary>
    /// 构造函数 - 完整参数
    /// </summary>
    public UIEventData(UIEventType type, string id = "", int idx = -1, string extra = "")
    {
        eventType = type;
        targetId = id ?? "";
        index = idx;
        extraData = extra ?? "";
    }

    /// <summary>
    /// 构造函数 - 仅事件类型
    /// </summary>
    public UIEventData(UIEventType type)
    {
        eventType = type;
        targetId = "";
        index = -1;
        extraData = "";
    }

    /// <summary>
    /// 获取事件的描述性字符串（用于调试）
    /// </summary>
    public override string ToString()
    {
        return $"[UIEvent] Type: {eventType}, ID: {targetId}, Index: {index}, Extra: {extraData}";
    }
}
