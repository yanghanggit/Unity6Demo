/// <summary>
/// 游戏事件监听器接口
/// 实现此接口以响应 GameEvent
/// </summary>
public interface IGameEventListener
{
    /// <summary>
    /// 当事件被触发时调用
    /// </summary>
    void OnEventRaised();
}
