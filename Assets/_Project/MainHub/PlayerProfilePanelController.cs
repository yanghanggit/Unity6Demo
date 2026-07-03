using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class PlayerProfilePanelController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _text;

    void Awake()
    {
        Debug.Assert(_text != null, "_text is null");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 关闭玩家信息面板
    /// </summary>
    public void OnClickCloseButton()
    {
        Debug.Log("点击了关闭按钮");
        HidePanel(); // 隐藏玩家信息面板
    }

    /// <summary>
    /// 显示玩家信息面板
    /// </summary>
    public void ShowPanel()
    {
        gameObject.SetActive(true); // 显示玩家信息面板

        if (GameManager.Instance.IsServerConnected && GameManager.Instance.Session != null)
        {
            ShowLivePanelAsync().Forget();
        }
        else
        {
            ShowMockPanel();
        }
    }

    // 组件显示顺序（对应 Python _COMPONENT_ORDER）
    private static readonly string[] _componentOrder =
    {
        nameof(ActorComponent),
        nameof(CharacterStatsComponent),
        nameof(AppearanceComponent),
        nameof(InventoryComponent),
        nameof(CostumeComponent),
        nameof(DeckComponent),
        nameof(DrawPileComponent),
        nameof(ExhaustPileComponent),
        nameof(DiscardPileComponent),
    };

    // 不展示的冗余组件（对应 Python _render_component 中的过滤逻辑）
    private static readonly HashSet<string> _skippedComponents = new()
    {
        nameof(IdentityComponent),
        nameof(NPCComponent),
        nameof(PlayerComponent),
        nameof(PartyRosterComponent),
    };

    /// <summary>
    /// 从服务器拉取玩家角色实体详情并渲染，对应 Python PlayerStatusScreen._load_status。
    /// </summary>
    private async UniTaskVoid ShowLivePanelAsync()
    {
        var session = GameManager.Instance.Session;
        var ct = this.GetCancellationTokenOnDestroy();
        _text.text = "正在加载玩家信息…";

        try
        {
            var resp = await GameManager.Instance.ServerClient.FetchEntitiesDetailsAsync(
                session.UserName, session.GameName, new[] { session.ActorName }, ct);

            var sb = new StringBuilder();

            // 基本信息（对应 Python _load_status 基础信息段）
            sb.AppendLine("── 基本信息 ──────────────────────");
            sb.AppendLine($"  玩家：{session.UserName}");
            sb.AppendLine($"  游戏：{session.GameName}");
            sb.AppendLine($"  玩家角色：{session.ActorName}");
            sb.AppendLine();

            // 世界设定
            sb.AppendLine("── 游戏世界设定 ───────────────────");
            sb.AppendLine($"  {session.Blueprint.campaign_setting}");
            sb.AppendLine();

            if (resp.entities_serialization.Count == 0)
            {
                sb.AppendLine($"（未找到玩家角色实体：{session.ActorName}）");
            }
            else
            {
                foreach (var entity in resp.entities_serialization)
                {
                    sb.AppendLine($"── {entity.name} ──────────────────────");

                    // 按 _componentOrder 排序（对应 Python sorted_comps）
                    entity.components.Sort((a, b) =>
                    {
                        int ia = Array.IndexOf(_componentOrder, a.name);
                        int ib = Array.IndexOf(_componentOrder, b.name);
                        if (ia < 0) ia = _componentOrder.Length;
                        if (ib < 0) ib = _componentOrder.Length;
                        return ia.CompareTo(ib);
                    });

                    foreach (var comp in entity.components)
                    {
                        if (_skippedComponents.Contains(comp.name)) continue;
                        var rendered = RenderComponent(comp.name, comp.data);
                        if (!string.IsNullOrEmpty(rendered))
                        {
                            sb.AppendLine(rendered);
                            sb.AppendLine();
                        }
                    }
                }
            }

            _text.text = sb.ToString();
        }
        catch (Exception e)
        {
            _text.text = $"❌ 玩家角色实体查询失败：{e.Message}";
        }
    }

    /// <summary>
    /// 将服务端组件数据渲染为可读文本，对应 Python _render_component。
    /// 返回空字符串表示跳过。
    /// </summary>
    private static string RenderComponent(string name, JObject data)
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
                            var itemName = item["name"]?.ToString() ?? "";
                            var itemDesc = item["description"]?.ToString() ?? "";
                            var itemCount = item["count"]?.ToString() ?? "1";
                            lines.Add($"    • {itemName} ×{itemCount} — {itemDesc}");
                        }
                    }
                    break;
                }
            case nameof(CostumeComponent):
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
    /// 使用 Mock 数据填充并显示面板，模拟服务端各组件（参考 player_status.py）。
    /// 显示管线与 ShowLivePanelAsync 完全相同，均通过 RenderComponent 渲染。
    /// </summary>
    private void ShowMockPanel()
    {
        // 最小化 Blueprint：仅需 campaign_setting 用于头部展示
        GameManager.Instance.SetSession(
            new PlayerSession { name = "mock_player", actor = "白鸟日向", game = "幻境传说01" },
            new Blueprint { campaign_setting = "一个充满奇幻色彩的异世界，玩家将在这里经历冒险与成长。" });
        var session = GameManager.Instance.Session;

        // 构造 mock 组件 DTO，转换为与服务端相同的 ComponentSerialization 格式
        var mockComponents = new List<ComponentSerialization>
        {
            ToComp(new ActorComponent
            {
                name = session.ActorName,
                character_sheet_name = "剑士",
                current_stage = "小镇广场",
            }),
            ToComp(new CharacterStatsComponent
            {
                name = session.ActorName,
                stats = new CharacterStats { hp = 28, max_hp = 30, attack = 8, defense = 5, energy = 3, speed = 4 },
            }),
            ToComp(new AppearanceComponent
            {
                name = session.ActorName,
                base_body = "身形纤细，银发及腰，眼神清澈而坚定。",
                appearance = "穿着白色短裙，配以银色腰带，手持细长的银剑。",
            }),
            // InventoryComponent / CostumeComponent 含 Item 子类，其类型级 AnyItemConverter.WriteJson
            // 会递归调用自身导致栈溢出，因此手动构建 JObject，与 RenderComponent 读取格式保持一致。
            new() {
                name = nameof(InventoryComponent),
                data = new JObject
                {
                    ["name"]  = session.ActorName,
                    ["items"] = new JArray
                    {
                        new JObject { ["name"] = "回血药剂", ["description"] = "恢复生命值 5 点", ["count"] = 2 },
                        new JObject { ["name"] = "铁矿石",   ["description"] = "常见的金属材料",   ["count"] = 3 },
                    },
                },
            },
            new() {
                name = nameof(CostumeComponent),
                data = new JObject
                {
                    ["name"] = session.ActorName,
                    ["item"] = new JObject { ["name"] = "雪白连衣裙", ["description"] = "白色蕾丝花边连衣裙，令人心旷神怡。" },
                },
            },
            ToComp(new DeckComponent
            {
                name = session.ActorName,
                keywords = new List<string> { "斩击", "防御姿态", "精准刺击" },
            }),
            ToComp(new DrawPileComponent
            {
                name = session.ActorName,
                cards = Enumerable.Range(0, 8).Select(_ => new Card()).ToList(),
            }),
            ToComp(new ExhaustPileComponent { name = session.ActorName }),
            ToComp(new DiscardPileComponent
            {
                name = session.ActorName,
                cards = Enumerable.Range(0, 2).Select(_ => new Card()).ToList(),
            }),
        };

        var sb = new StringBuilder();
        sb.AppendLine("── 基本信息 ──────────────────────");
        sb.AppendLine($"  玩家：{session.UserName}");
        sb.AppendLine($"  游戏：{session.GameName}");
        sb.AppendLine($"  玩家角色：{session.ActorName}");
        sb.AppendLine();
        sb.AppendLine("── 游戏世界设定 ───────────────────");
        sb.AppendLine($"  {session.Blueprint.campaign_setting}");
        sb.AppendLine();
        sb.AppendLine($"── {session.ActorName} ──────────────────────");
        foreach (var comp in mockComponents)
        {
            var rendered = RenderComponent(comp.name, comp.data);
            if (!string.IsNullOrEmpty(rendered))
            {
                sb.AppendLine(rendered);
                sb.AppendLine();
            }
        }
        _text.text = sb.ToString();
    }

    /// <summary>
    /// 将组件 DTO 序列化为 ComponentSerialization，供 RenderComponent 统一处理。
    /// name 由类型名自动推导，无需手动传入。
    /// </summary>
    private static ComponentSerialization ToComp<T>(T component) where T : class
    {
        return new ComponentSerialization
        {
            name = typeof(T).Name,
            data = JObject.FromObject(component),
        };
    }

    /// <summary>
    /// 隐藏玩家信息面板
    /// </summary>
    public void HidePanel()
    {
        gameObject.SetActive(false); // 隐藏玩家信息面板
    }
}
