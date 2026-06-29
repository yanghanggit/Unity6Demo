using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI Toolkit ListView / ScrollView 最简演示。
/// 演示要点：
///   1. makeItem / bindItem 虚拟化渲染（只渲染可见行）
///   2. itemsSource 绑定 List&lt;string&gt;
///   3. 运行时添加数据 + RefreshItems() 刷新
/// </summary>
public class InventoryListViewDemo : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private PanelRenderer _panelRenderer;

    // ── 数据：字符串列表，每条是一个物品名 ─────────────────────────────
    private readonly List<string> _allItems = new();
    private int _nextId = 1;

    // ── UI 引用 ─────────────────────────────────────────────────────
    private ListView _listView;
    private Label    _countLabel;
    private Button   _btnAdd;

    // 行高（与 UXML fixed-item-height 及 USS .list-item-label 保持一致）
    private const float k_FixedItemHeight = 72f;

    // ────────────────────────────────────────────────────────────────
    void Start()
    {
        // PanelRenderer 没有公开的 rootVisualElement 属性，正确的公开 API 是 RegisterUIReloadCallback。
        // 源码显示：若 Panel 已初始化（Awake 完成后），回调会立即同步触发。
        _panelRenderer.RegisterUIReloadCallback(OnPanelLoaded);
    }

    void OnDestroy()
    {
        if (_panelRenderer != null)
            _panelRenderer.UnregisterUIReloadCallback(OnPanelLoaded);
    }

    /// <summary>
    /// Panel 已加载时回调。
    /// • 游戏运行时：仅在 Start() 后立即触发一次。
    /// • Editor 热重载 UXML/USS 时会再次触发，新根元素传入，旧元素被替换不会重复监听。
    /// </summary>
    void OnPanelLoaded(PanelRenderer pr, VisualElement root)
    {
        // 查询 UXML 中定义的元素
        _listView   = root.Q<ListView>("inventory-list-view");
        _countLabel = root.Q<Label>("label-count");
        _btnAdd     = root.Q<Button>("btn-add");

        // 首次加载时生成初始数据（热重载时不重复添加）
        if (_allItems.Count == 0)
            for (int i = 0; i < 20; i++)
                _allItems.Add($"物品 #{_nextId++:000}");

        SetupListView();

        _btnAdd.clicked += AddItem;
    }

    // ── ListView 初始化 ──────────────────────────────────────────────

    void SetupListView()
    {
        // ① makeItem：为每行创建一个 Label（Unity 会缓存复用，不会无限创建）
        _listView.makeItem = () =>
        {
            var label = new Label();
            label.AddToClassList("list-item-label");
            return label;
        };

        // ② bindItem：把数据填入元素
        //    每当一行滚动进入视口就调用，index 是 _allItems 中的下标
        _listView.bindItem = (element, index) =>
            ((Label)element).text = _allItems[index];

        // ③ 数据源：ListView 直接引用这个列表，修改后调用 RefreshItems() 即可
        _listView.itemsSource = _allItems;

        // ④ FixedHeight 虚拟化：行高固定，性能最优
        _listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
        _listView.fixedItemHeight      = k_FixedItemHeight;

        _listView.selectionType = SelectionType.Single;
        _listView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;

        Refresh();
    }

    // ── 数据操作 ─────────────────────────────────────────────────────

    void AddItem()
    {
        _allItems.Add($"物品 #{_nextId++:000}");
        Refresh();
    }

    void Refresh()
    {
        _listView.RefreshItems();   // 只重新 bind 可见行，不重建 DOM
        _countLabel.text = $"共 {_allItems.Count} 件";
    }

}

/*
 * ═══════════════════════════════════════════════════════════
 * 【关键知识点：ListView 是带虚拟化的 ScrollView】
 * ═══════════════════════════════════════════════════════════
 *
 * ► ListView 内部就是一个 ScrollView
 *   它只创建视口内可见行数量的 VisualElement，滚动时把移出视口的
 *   元素复用（重新 bindItem），而不是销毁再创建。
 *   无论有多少条数据，内存和渲染开销始终固定。
 *
 * ► makeItem / bindItem 分工
 *   makeItem：创建一个空行模板（只调用少量次，元素会被缓存复用）
 *   bindItem：把数据填入行模板（每次该行进入视口时调用）
 *
 * ► RefreshItems() vs Rebuild()
 *   RefreshItems()：只重新执行 bindItem，不重建 DOM，用于数据内容变更
 *   Rebuild()     ：销毁所有行并重建，用于 fixedItemHeight 或模板结构变化时
 *
 * ► 如果只需要普通滚动容器（不用虚拟化）
 *   直接用 <ui:ScrollView>，在里面循环添加子元素，无需 makeItem/bindItem。
 * ═══════════════════════════════════════════════════════════
 */
