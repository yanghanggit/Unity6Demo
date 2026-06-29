using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI Toolkit ListView 演示：背包/库存系统。
/// 演示要点：
///   1. makeItem / bindItem 虚拟化渲染（只渲染可见行，数据源可以无限大）
///   2. itemsSource 绑定任意 IList 数据
///   3. 运行时动态增删数据 + RefreshItems() 局部刷新
///   4. 实时搜索过滤（不重建 ListView，只切换数据源引用）
///   5. selectionChanged 事件响应
/// 场景设置见文件末尾注释。
/// </summary>
public class InventoryListViewDemo : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private UIDocument _uiDocument;

    // ── 数据层 ──────────────────────────────────────────────────────
    private readonly List<InventoryItem> _allItems      = new();
    private readonly List<InventoryItem> _filteredItems = new();
    private int _nextId = 1;

    // ── UI 引用 ─────────────────────────────────────────────────────
    private ListView  _listView;
    private TextField _searchField;
    private Label     _countLabel;
    private Label     _detailText;
    private Button    _btnAdd;
    private Button    _btnRemove;
    private Button    _btnBulk;

    // ── 布局常量 ──────────────────────────────────────────────────────
    private const float k_FixedItemHeight = 90f;   // 与 USS .list-item height 保持一致

    // ────────────────────────────────────────────────────────────────
    void Start()
    {
        var root = _uiDocument.rootVisualElement;

        _listView    = root.Q<ListView>("inventory-list-view");
        _searchField = root.Q<TextField>("search-field");
        _countLabel  = root.Q<Label>("label-count");
        _detailText  = root.Q<Label>("detail-text");
        _btnAdd      = root.Q<Button>("btn-add");
        _btnRemove   = root.Q<Button>("btn-remove");
        _btnBulk     = root.Q<Button>("btn-add-bulk");

        // 初始种子数据（直接操作列表，不触发刷新）
        for (int i = 0; i < 30; i++)
            _allItems.Add(InventoryItem.CreateRandom(_nextId++));
        _filteredItems.AddRange(_allItems);

        // 配置 ListView（只需调用一次）
        SetupListView();

        // 注册事件
        _searchField.RegisterValueChangedCallback(evt => ApplyFilter(evt.newValue));
        _btnAdd.clicked    += () => AddItems(1);
        _btnRemove.clicked += RemoveSelected;
        _btnBulk.clicked   += () => AddItems(50);
    }

    // ── ListView 初始化 ──────────────────────────────────────────────

    void SetupListView()
    {
        // ① makeItem：返回一个空的 VisualElement 模板（Unity 会缓存并复用它）
        //    这里完全用代码构建；也可以用 UXML VisualTreeAsset.Instantiate()
        _listView.makeItem = () =>
        {
            var row = new VisualElement();
            row.AddToClassList("list-item");

            // 稀有度色块
            var badge = new VisualElement();
            badge.AddToClassList("rarity-badge");
            badge.name = "rarity-badge";
            var badgeLabel = new Label();
            badgeLabel.AddToClassList("rarity-badge-label");
            badgeLabel.name = "badge-label";
            badge.Add(badgeLabel);

            // 物品名
            var nameLabel = new Label();
            nameLabel.AddToClassList("item-name");
            nameLabel.name = "item-name";

            // 数量
            var quantityLabel = new Label();
            quantityLabel.AddToClassList("item-quantity");
            quantityLabel.name = "item-quantity";

            // 稀有度文字
            var rarityLabel = new Label();
            rarityLabel.AddToClassList("item-rarity-text");
            rarityLabel.name = "item-rarity";

            row.Add(badge);
            row.Add(nameLabel);
            row.Add(quantityLabel);
            row.Add(rarityLabel);
            return row;
        };

        // ② bindItem：将 _filteredItems[index] 的数据写入 makeItem 创建的元素
        //    每次滚动进入视口时调用，index 是当前可见行对应的数据索引
        _listView.bindItem = (element, index) =>
        {
            if (index < 0 || index >= _filteredItems.Count) return;
            var data = _filteredItems[index];

            // 色块
            var badge = element.Q<VisualElement>("rarity-badge");
            badge.style.backgroundColor = new StyleColor(data.RarityColor);
            element.Q<Label>("badge-label").text = data.RarityShortText;

            element.Q<Label>("item-name").text     = data.Name;
            element.Q<Label>("item-quantity").text = $"x{data.Quantity}";

            var rarityLabel = element.Q<Label>("item-rarity");
            rarityLabel.text        = data.Rarity;
            rarityLabel.style.color = new StyleColor(data.RarityColor);
        };

        // ③ 数据源（直接引用 _filteredItems，后续增删列表后调用 RefreshItems 即可）
        _listView.itemsSource = _filteredItems;

        // ④ 虚拟化：FixedHeight 是性能最优方案（所有行等高）
        //    DynamicHeight 支持不等高行（稍有性能代价）
        _listView.virtualizationMethod  = CollectionVirtualizationMethod.FixedHeight;
        _listView.fixedItemHeight       = k_FixedItemHeight;   // 90px，与 USS 中 .list-item height 一致

        _listView.selectionType = SelectionType.Single;
        _listView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;

        // ⑤ 选中事件
        _listView.selectionChanged += OnSelectionChanged;

        Refresh();
    }

    // ── 数据操作 ─────────────────────────────────────────────────────

    void AddItems(int count)
    {
        for (int i = 0; i < count; i++)
            _allItems.Add(InventoryItem.CreateRandom(_nextId++));
        ApplyFilter(_searchField.value);
    }

    void RemoveSelected()
    {
        if (_listView.selectedItem is InventoryItem selected)
        {
            _allItems.Remove(selected);
            _detailText.text = "已删除物品，点击列表中的物品查看详情";
            ApplyFilter(_searchField.value);
        }
    }

    /// <summary>
    /// 搜索过滤：直接操作 _filteredItems 内容，然后通知 ListView 刷新。
    /// 不需要重新赋值 itemsSource，RefreshItems() 足够。
    /// </summary>
    void ApplyFilter(string query)
    {
        _filteredItems.Clear();
        if (string.IsNullOrWhiteSpace(query))
            _filteredItems.AddRange(_allItems);
        else
            _filteredItems.AddRange(
                _allItems.Where(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                               || x.Rarity.Contains(query, StringComparison.OrdinalIgnoreCase)));
        Refresh();
    }

    /// <summary>
    /// 局部刷新：只重新 bind 可见行，不重建整个列表。
    /// </summary>
    void Refresh()
    {
        _listView.RefreshItems();
        _countLabel.text = $"显示 {_filteredItems.Count} 件 / 总计 {_allItems.Count} 件";
    }

    void OnSelectionChanged(IEnumerable<object> selection)
    {
        var item = selection.FirstOrDefault() as InventoryItem;
        if (item == null)
        {
            _detailText.text = "点击列表中的物品查看详情";
            return;
        }
        _detailText.text = $"[{item.Rarity}]  {item.Name}\n数量：{item.Quantity}    ID：{item.Id}";
    }

}

// ════════════════════════════════════════════════════════════════════
// 数据模型
// ════════════════════════════════════════════════════════════════════

[Serializable]
public class InventoryItem
{
    // 随机物品名词库
    private static readonly string[] s_Prefixes = { "铁", "钢", "精金", "暗金", "秘银", "龙骨", "圣光", "幽灵", "烈焰", "寒冰" };
    private static readonly string[] s_Names    = { "长剑", "短刃", "盾牌", "法杖", "弓", "长矛", "战斧", "匕首", "护甲", "头盔", "手套", "腰带", "靴子", "项链", "戒指", "药水" };
    private static readonly string[] s_Rarities = { "普通", "精良", "稀有", "史诗", "传说" };

    public int    Id;
    public string Name;
    public int    Quantity;
    public string Rarity;

    // 色块上显示的极短文字
    public string RarityShortText => Rarity switch
    {
        "普通" => "N",
        "精良" => "G",
        "稀有" => "R",
        "史诗" => "E",
        "传说" => "L",
        _     => "?"
    };

    public Color RarityColor => Rarity switch
    {
        "普通" => new Color(0.55f, 0.55f, 0.55f),
        "精良" => new Color(0.12f, 0.75f, 0.20f),
        "稀有" => new Color(0.15f, 0.50f, 1.00f),
        "史诗" => new Color(0.60f, 0.10f, 0.90f),
        "传说" => new Color(1.00f, 0.60f, 0.05f),
        _     => Color.white
    };

    public static InventoryItem CreateRandom(int id)
    {
        string rarity = s_Rarities[UnityEngine.Random.Range(0, s_Rarities.Length)];
        string name   = s_Prefixes[UnityEngine.Random.Range(0, s_Prefixes.Length)]
                      + s_Names[UnityEngine.Random.Range(0, s_Names.Length)]
                      + $" #{id:000}";
        return new InventoryItem
        {
            Id       = id,
            Name     = name,
            Quantity = UnityEngine.Random.Range(1, 999),
            Rarity   = rarity
        };
    }
}

/*
 * ═══════════════════════════════════════════════════════════
 * 【Inspector 操作步骤】
 * ═══════════════════════════════════════════════════════════
 *
 * 1. 创建 PanelSettings 资产（只需一次）
 *    Project 窗口右键 > Create > UI Toolkit > Panel Settings Asset
 *    建议放在 Assets/_Project/Settings/ 目录下
 *    保持默认设置即可（Scale Mode: Scale With Screen Size）
 *
 * 2. 打开场景 TestUIToolkit.unity
 *
 * 3. 在 Hierarchy 窗口创建空 GameObject，命名为 "UIToolkitListViewDemo"
 *
 * 4. 在该 GameObject 上添加两个组件：
 *    a) UI Document  （UnityEngine.UIElements.UIDocument）
 *       - Panel Settings  →  拖入步骤 1 创建的 PanelSettings 资产
 *       - Source Asset    →  拖入 InventoryListView.uxml
 *    b) Inventory List View Demo （本脚本）
 *       - Ui Document     →  拖入同一 GameObject 上的 UIDocument 组件
 *
 * 5. 运行场景即可。
 *
 * ═══════════════════════════════════════════════════════════
 * 【关键知识点说明】
 * ═══════════════════════════════════════════════════════════
 *
 * ► 虚拟化（Virtualization）
 *   ListView 内部维护一个"回收池"，只创建视口内可见行数量的 VisualElement。
 *   滚动时将移出视口的元素重新绑定新数据（bindItem），而不是销毁再创建。
 *   因此无论 itemsSource 有多少条，内存占用和 Draw Call 数量始终固定。
 *
 * ► makeItem vs UXML 模板
 *   本例用代码构建行模板。另一种方式是把行模板写成单独的 .uxml 文件，
 *   然后用 VisualTreeAsset.Instantiate() 作为 makeItem 的返回值，
 *   可以在 UI Builder 里可视化设计行布局。
 *
 * ► RefreshItems() vs Rebuild()
 *   RefreshItems()：只重新执行 bindItem，不重建 DOM，性能好，用于数据变更。
 *   Rebuild()     ：销毁所有行并重建，用于 fixedItemHeight 或行模板结构变化时。
 *
 * ► DynamicHeight 模式
 *   若行高不固定，改为：virtualizationMethod = DynamicHeight
 *   此时 fixedItemHeight 失效，ListView 会在第一次显示时测量每行真实高度。
 * ═══════════════════════════════════════════════════════════
 */
