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
[ExecuteAlways]
public class InventoryListViewDemo : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private PanelRenderer _panelRenderer;
    [SerializeField] private int _initialItemCount = 100;

    // ── 数据：字符串列表，每条是一个物品名 ─────────────────────────────
    private readonly List<string> _allItems = new();

    // ── UI 引用 ─────────────────────────────────────────────────────
    private ListView _listView;
    private Label    _countLabel;
    private Button   _btnAdd;

    // 行高 = padding-top(16) + card(112) + padding-bottom(16) = 144px
    private const float k_FixedItemHeight = 144f;

    // ────────────────────────────────────────────────────────────────
    // OnEnable/OnDisable 在 Edit Mode 和 Play Mode 均可靠触发（含脚本重编译后）
    // Start/OnDestroy 在 Edit Mode 下行为不稳定，不适合 [ExecuteAlways] 场景
    void OnEnable()
    {
        if (_panelRenderer != null)
            _panelRenderer.RegisterUIReloadCallback(OnPanelLoaded);
    }

    void OnDisable()
    {
        if (_panelRenderer != null)
            _panelRenderer.UnregisterUIReloadCallback(OnPanelLoaded);
    }

    // Inspector 修改序列化字段时触发，重建数据并刷新列表
    void OnValidate()
    {
        _allItems.Clear();
        for (int i = 0; i < _initialItemCount; i++)
            _allItems.Add($"物品 #{_allItems.Count + 1:000}");
        if (_listView != null)
            Refresh();
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
            for (int i = 0; i < _initialItemCount; i++)
                _allItems.Add($"物品 #{_allItems.Count + 1:000}");

        SetupListView();

        _btnAdd.clicked += AddItem;
    }

    // ── ListView 初始化 ──────────────────────────────────────────────

    void SetupListView()
    {
        // ① makeItem：outer wrapper（内联样式保证间距可靠）+ card + label
        //    outer 的 padding 区域透出深色背景形成间隙；点击事件经 card→outer→item 向上冒泡，选中正常触发
        //    选中高亮由 USS ".unity-list-view__item:checked .list-item-card" 负责显示在 card 上
        _listView.makeItem = () =>
        {
            var outer = new VisualElement();
            outer.style.flexGrow = 1;
            outer.style.paddingTop    = 16;
            outer.style.paddingBottom = 16;
            outer.style.paddingLeft   = 12;
            outer.style.paddingRight  = 12;
            outer.style.backgroundColor = new Color(12f / 255f, 14f / 255f, 22f / 255f);

            var card = new VisualElement();
            card.AddToClassList("list-item-card");
            var label = new Label();
            label.AddToClassList("list-item-label");
            card.Add(label);
            outer.Add(card);
            return outer;
        };

        // ② bindItem：Q<Label>() 从 outer 内部查找 label 填入数据
        _listView.bindItem = (element, index) =>
            element.Q<Label>().text = _allItems[index];

        // ③ 数据源：ListView 直接引用这个列表，修改后调用 RefreshItems() 即可
        _listView.itemsSource = _allItems;

        // ④ FixedHeight 虚拟化：行高固定，性能最优
        _listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
        _listView.fixedItemHeight      = k_FixedItemHeight;

        _listView.selectionType = SelectionType.Single;
        _listView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;

        Refresh();
    }

    // ── 数据操作 ─────────────────────────────────────────────────────

    void AddItem()
    {
        _allItems.Add($"物品 #{_allItems.Count + 1:000}");
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
