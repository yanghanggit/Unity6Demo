// ECS 组件显示与序列化工具，供 UI 层统一调用。
// 对应 Python player_status.py 中的 _render_component 逻辑。
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public static class ComponentUtils
{
    /// <summary>
    /// 将服务端组件数据渲染为可读文本，对应 Python _render_component。
    /// 返回空字符串表示跳过。
    /// </summary>
    public static string RenderComponent(string name, JObject data)
    {
        var lines = new List<string> { $"  ◆ {name}" };

        switch (name)
        {
            case nameof(ActorComponent):
                {
                    var sheetName = data["character_sheet_name"]?.ToString();
                    var stage = data["current_stage"]?.ToString();
                    if (!string.IsNullOrEmpty(sheetName))
                        lines.Add($"    职业模板：{sheetName}");
                    if (!string.IsNullOrEmpty(stage))
                        lines.Add($"    当前场景：{stage}");
                    break;
                }
            case nameof(AppearanceComponent):
                {
                    var baseBody = data["base_body"]?.ToString();
                    var appearance = data["appearance"]?.ToString();
                    if (!string.IsNullOrEmpty(baseBody))
                    {
                        lines.Add("    基础体型：");
                        lines.Add($"    {baseBody}");
                    }
                    if (!string.IsNullOrEmpty(appearance))
                    {
                        lines.Add("    当前外观：");
                        lines.Add($"    {appearance}");
                    }
                    if (string.IsNullOrEmpty(baseBody) && string.IsNullOrEmpty(appearance))
                        lines.Add("    （暂无描述）");
                    break;
                }
            case nameof(CharacterStatsComponent):
                {
                    if (data["stats"] is JObject stats)
                        lines.Add($"    HP {stats["hp"]} / {stats["max_hp"]}" +
                                  $"   攻击 {stats["attack"]}" +
                                  $"   防御 {stats["defense"]}" +
                                  $"   行动 {stats["energy"]}" +
                                  $"   速度 {stats["speed"]}");
                    break;
                }
            case nameof(InventoryComponent):
                {
                    var items = data["items"] as JArray;
                    if (items == null || items.Count == 0)
                    {
                        lines.Add("    （背包为空）");
                    }
                    else
                    {
                        lines.Add($"    共 {items.Count} 件道具：");
                        foreach (var item in items)
                        {
                            var itemName  = item["name"]?.ToString() ?? "";
                            var itemDesc  = item["description"]?.ToString() ?? "";
                            var itemCount = item["count"]?.ToString() ?? "1";
                            lines.Add($"    • {itemName} ×{itemCount} — {itemDesc}");
                        }
                    }
                    break;
                }
            case nameof(WornCostumeComponent):
                {
                    if (data["item"] is not JObject item)
                    {
                        lines.Add("    （未穿戴时装）");
                    }
                    else
                    {
                        lines.Add("    当前穿戴时装：");
                        lines.Add($"    • {item["name"]} — {item["description"]}");
                    }
                    break;
                }
            case nameof(DeckComponent):
                {
                    if (data["keywords"] is JArray keywords)
                        for (int i = 0; i < keywords.Count; i++)
                            lines.Add($"    {i + 1}. {keywords[i]}");
                    break;
                }
            case nameof(DrawPileComponent):
                {
                    var cards = data["cards"] as JArray;
                    lines.Add(cards == null || cards.Count == 0 ? "    （空）" : $"    共 {cards.Count} 张");
                    break;
                }
            case nameof(ExhaustPileComponent):
                {
                    var cards = data["cards"] as JArray;
                    lines.Add(cards == null || cards.Count == 0 ? "    （空）" : $"    共 {cards.Count} 张已消耗");
                    break;
                }
            case nameof(DiscardPileComponent):
                {
                    var cards = data["cards"] as JArray;
                    lines.Add(cards == null || cards.Count == 0 ? "    （空）" : $"    共 {cards.Count} 张已弃置");
                    break;
                }
            default:
                {
                    // 通用展示：key-value，跳过 name 字段
                    foreach (var prop in data.Properties())
                    {
                        if (prop.Name == "name") continue;
                        lines.Add($"    {prop.Name}：{prop.Value}");
                    }
                    break;
                }
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// 将组件 DTO 序列化为 ComponentSerialization，供 RenderComponent 统一处理。
    /// name 由类型名自动推导，无需手动传入。
    /// 注意：含 Item 子类字段（InventoryComponent、WornCostumeComponent）因 AnyItemConverter
    /// 的类型级属性会导致递归溢出，须手动构建 JObject，不可使用本方法。
    /// </summary>
    public static ComponentSerialization ToComp<T>(T component) where T : class
    {
        return new ComponentSerialization
        {
            name = typeof(T).Name,
            data = JObject.FromObject(component),
        };
    }
}
