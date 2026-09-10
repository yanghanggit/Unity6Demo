using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 家园总览（HomeOverview）控制器。
/// 占位阶段：用 mock 数据生成 home stage 卡片，点击只打日志。
/// 后续接入：改用 FetchEntitiesGroupAsync(all_of: HomeComponent + StageComponent) 取真实数据。
/// </summary>
public class HomeOverviewController : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private PanelRenderer _panelRenderer;
    [SerializeField] private VisualTreeAsset _cardTemplate;

    private ScrollView _stageScroll;

    // mock 数据（占位）：后续替换为后端返回的 home stage 实体名
    private static readonly string[] MockStageNames =
    {
        "村口", "训练场", "猎人小屋", "史家宅",
        "铁匠铺", "药圃", "集市", "祠堂",
    };

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

    void OnPanelLoaded(PanelRenderer pr, VisualElement root)
    {
        _stageScroll = root.Q<ScrollView>("stage-scroll");
        Debug.Assert(_stageScroll != null, "[HomeOverview] 未找到 stage-scroll，请检查 HomeOverview.uxml");
        Debug.Assert(_cardTemplate != null, "[HomeOverview] _cardTemplate 未赋值，请在 Inspector 拖入 HomeStageCard.uxml");

        PopulateMockCards();
    }

    /// <summary>用 mock 数据重建卡片列表。</summary>
    private void PopulateMockCards()
    {
        _stageScroll.Clear();
        foreach (var name in MockStageNames)
            _stageScroll.Add(BuildCard(name));
    }

    /// <summary>根据模板克隆一张卡片，填入 stage 名并绑定点击。</summary>
    private VisualElement BuildCard(string stageName)
    {
        var card = _cardTemplate.CloneTree();
        card.Q<Label>("card-name").text = stageName;
        card.RegisterCallback<ClickEvent>(_ => OnStageClicked(stageName));
        return card;
    }

    /// <summary>点击某张卡片（home stage）。占位：只打日志，后续跳转 HomeStage。</summary>
    private void OnStageClicked(string stageName)
    {
        Debug.Log($"[HomeOverview] 点击 home stage: {stageName}");
    }
}
