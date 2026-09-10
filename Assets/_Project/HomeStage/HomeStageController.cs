using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// 家园场景（HomeStage）控制器：进入某个 home stage 后展示其中的角色（Actor）。
/// 联网+已登录走真实 API，否则用 MockGameData 离线调试；不缓存，每次现拉。
/// </summary>
public class HomeStageController : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private PanelRenderer _panelRenderer;
    [SerializeField] private VisualTreeAsset _cardTemplate;

    private ScrollView _actorScroll;
    private Label _titleLabel;
    private Button _backButton;

    private const string HomeOverviewSceneName = "HomeOverview";

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
        _backButton = root.Q<Button>("btn-back");
        Debug.Assert(_actorScroll != null, "[HomeStage] 未找到 actor-scroll，请检查 HomeStage.uxml");
        Debug.Assert(_backButton != null, "[HomeStage] 未找到 btn-back，请检查 HomeStage.uxml");
        Debug.Assert(_cardTemplate != null, "[HomeStage] _cardTemplate 未赋值，请在 Inspector 拖入 ActorCard.uxml");

        _backButton.clicked += OnBackClicked;

        ApplyTitle();
        PopulateCardsAsync().Forget();
    }

    /// <summary>标题显示当前 stage 显示名（进入前由 HomeOverview 写入 GameManager.CurrentStageName）。</summary>
    private void ApplyTitle()
    {
        var stageName = GameManager.Instance.CurrentStageName;
        if (_titleLabel != null)
            _titleLabel.text = string.IsNullOrEmpty(stageName) ? "场景" : EntityNameUtils.GetDisplayName(stageName);
    }

    /// <summary>取当前 stage 的 Actor 列表并生成卡片。联网+已登录走真实 API，否则用 MockGameData 离线调试。</summary>
    private async UniTaskVoid PopulateCardsAsync()
    {
        _actorScroll.Clear();

        var stageName = GameManager.Instance.CurrentStageName;
        if (string.IsNullOrEmpty(stageName))
            return;

        if (GameManager.Instance.IsServerConnected && GameManager.Instance.Session != null)
        {
            await FetchActorsFromServerAsync(stageName);
        }
        else
        {
            PopulateMockActors(stageName);
        }
    }

    private async UniTask FetchActorsFromServerAsync(string stageName)
    {
        var session = GameManager.Instance.Session;
        try
        {
            var state = await GameManager.Instance.ServerClient.FetchStagesStateAsync(session.UserName, session.GameName);
            if (!state.mapping.TryGetValue(stageName, out var actorNames))
            {
                Debug.LogWarning($"[HomeStage] 服务器未返回 stage '{stageName}' 的 actor 列表");
                return;
            }

            var details = await GameManager.Instance.ServerClient.FetchEntitiesDetailsAsync(session.UserName, session.GameName, actorNames);
            foreach (var entity in details.entities)
                _actorScroll.Add(BuildCard(entity));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HomeStage] 拉取 actor 失败: {e.Message}");
        }
    }

    private void PopulateMockActors(string stageName)
    {
        var state = MockGameData.BuildStagesState();
        if (!state.mapping.TryGetValue(stageName, out var actorNames))
        {
            Debug.LogWarning($"[HomeStage] mock 未找到 stage '{stageName}' 的 actor 列表");
            return;
        }

        var details = MockGameData.BuildEntitiesDetails(actorNames);
        foreach (var entity in details.entities)
            _actorScroll.Add(BuildCard(entity));
    }

    /// <summary>根据 Actor 实体克隆一张卡片，填入显示名并绑定点击。</summary>
    private VisualElement BuildCard(EntitySerialization entity)
    {
        var card = _cardTemplate.CloneTree();
        card.Q<Label>("card-name").text = EntityNameUtils.GetDisplayName(entity.name);
        card.RegisterCallback<ClickEvent>(_ => OnActorClicked(entity.name));
        return card;
    }

    /// <summary>点击某张卡片（Actor）。占位：只打日志，后续接入对话/详情等交互。</summary>
    private void OnActorClicked(string actorName)
    {
        Debug.Log($"[HomeStage] 点击 Actor: {EntityNameUtils.GetDisplayName(actorName)} ({actorName})");
    }

    /// <summary>返回按钮：清空当前 stage 并切回 HomeOverview 场景。</summary>
    private void OnBackClicked()
    {
        GameManager.Instance.CurrentStageName = "";
        LoadHomeOverviewAsync().Forget();
    }

    private async UniTaskVoid LoadHomeOverviewAsync()
    {
        await SceneManager.LoadSceneAsync(HomeOverviewSceneName);
    }
}
