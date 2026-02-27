/// <summary>
/// 滚动视图 Cell 数据更新接口
/// 独立于 Mosframe，供 LoopScrollDataSourceAdapter 使用
/// </summary>
public interface IScrollViewItem
{
    /// <summary>
    /// 根据索引刷新 Cell 显示内容
    /// </summary>
    void onUpdateItem(int index);
}
