/// <summary>
/// UI事件监听器接口
/// 实现此接口以响应 UIEventGameEvent
/// </summary>
public interface IUIEventListener
{
    /// <summary>
    /// 当UI事件被触发时调用
    /// </summary>
    /// <param name="eventData">事件数据</param>
    void OnEventRaised(UIEventData eventData);
}
