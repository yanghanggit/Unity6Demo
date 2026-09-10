using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 家园场景（HomeStage）控制器：进入某个 home stage 后展示其中的角色（Actor）。
/// 占位阶段：用 mock 数据生成 Actor 卡片，点击只打日志。
/// 后续接入：改用 FetchEntitiesGroupAsync / FetchStagesStateAsync 取该 stage 内真实角色。
/// </summary>
public class HomeStageController : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private PanelRenderer _panelRenderer;
    [SerializeField] private VisualTreeAsset _cardTemplate;

    private ScrollView _actorScroll;
    private Label _titleLabel;

    // mock 数据（占位）：后续替换为当前 stage 内真实 Actor 名单
    private static readonly string[] MockActorNames =
    {
        "旅行者", "铁匠", "村长", "药师",
        "猎人", "史官", "商人", "村民",
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
        _actorScroll = root.Q<ScrollView>("actor-scroll");
        _titleLabel = root.Q<Label>("title");
        Debug.Assert(_actorScroll != null, "[HomeStage] 未找到 actor-scroll，请检查 HomeStage.uxml");
        Debug.Assert(_cardTemplate != null, "[HomeStage] _cardTemplate 未赋值，请在 Inspector 拖入 ActorCard.uxml");

        ApplyTitle();
        PopulateMockCards();
    }

    /// <summary>标题显示当前 stage 名（进入前由 HomeOverview 写入 GameManager.CurrentStageName）。</summary>
    private void ApplyTitle()
    {
        var stageName = GameManager.Instance.CurrentStageName;
        if (_titleLabel != null)
            _titleLabel.text = string.IsNullOrEmpty(stageName) ? "场景" : stageName;
    }

    /// <summary>用 mock 数据重建 Actor 卡片列表。</summary>
    private void PopulateMockCards()
    {
        _actorScroll.Clear();
        foreach (var name in MockActorNames)
            _actorScroll.Add(BuildCard(name));
    }

    /// <summary>根据模板克隆一张卡片，填入 Actor 名并绑定点击。</summary>
    private VisualElement BuildCard(string actorName)
    {
        var card = _cardTemplate.CloneTree();
        card.Q<Label>("card-name").text = actorName;
        card.RegisterCallback<ClickEvent>(_ => OnActorClicked(actorName));
        return card;
    }

    /// <summary>点击某张卡片（Actor）。占位：只打日志，后续接入对话/详情等交互。</summary>
    private void OnActorClicked(string actorName)
    {
        Debug.Log($"[HomeStage] 点击 Actor: {actorName}");
    }
}
