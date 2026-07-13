// 实体命名工具。
// 服务端实体名以 "." 分段组织，例如 "场景.A"、"旅行者.B"、"角色.学者.D"，
// 前面各段是分类/注释信息，最后一段才是真正的显示名。
public static class EntityNameUtils
{
    /// <summary>
    /// 从形如 "场景.A" / "角色.学者.D" 的多段实体名中提取最后一段作为显示名。
    /// 若不含 "."，或输入为空，则原样返回。
    /// </summary>
    public static string GetDisplayName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return fullName;

        int lastDotIndex = fullName.LastIndexOf('.');
        return lastDotIndex < 0 ? fullName : fullName[(lastDotIndex + 1)..];
    }
}
