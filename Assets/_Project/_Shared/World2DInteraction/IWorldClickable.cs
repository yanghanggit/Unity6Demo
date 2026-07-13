/// <summary>
/// 场景内可被点击的 2D 对象需要实现的接口，由 <see cref="WorldClickDetector"/> 在检测到有效点击时调用。
/// </summary>
public interface IWorldClickable
{
    /// <summary>
    /// 当该对象被判定为"点击"（而非拖拽）时调用。
    /// </summary>
    void OnWorldClick();
}
