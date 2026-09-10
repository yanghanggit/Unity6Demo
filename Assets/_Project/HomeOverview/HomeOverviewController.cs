using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// 家园总览（HomeOverview）控制器。
/// 占位阶段：用 MockGameData（正式 ECS 结构）生成 home stage 卡片，每张卡显示 stage 名及其 actor 名单。
/// 后续接入：把 MockGameData 换成 GameServerClient.FetchStagesStateAsync / FetchEntitiesDetailsAsync。
/// </summary>
public class HomeOverviewController : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private PanelRenderer _panelRenderer;
    [SerializeField] private VisualTreeAsset _cardTemplate;

    private ScrollView _stageScroll;
    private Button _backButton;

    private const string HomeStageSceneName = "HomeStage";
    private const string PlayerLobbySceneName = "PlayerLobby";

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
        _backButton = root.Q<Button>("btn-back");
        Debug.Assert(_stageScroll != null, "[HomeOverview] 未找到 stage-scroll，请检查 HomeOverview.uxml");
        Debug.Assert(_backButton != null, "[HomeOverview] 未找到 btn-back，请检查 HomeOverview.uxml");
        Debug.Assert(_cardTemplate != null, "[HomeOverview] _cardTemplate 未赋值，请在 Inspector 拖入 HomeStageCard.uxml");

        _backButton.clicked += OnBackClicked;

        PopulateCards();
    }

    /// <summary>用 mock 数据重建 home stage 卡片列表。</summary>
    private void PopulateCards()
    {
        _stageScroll.Clear();
        var state = MockGameData.BuildStagesState();
        foreach (var kv in state.mapping)
            _stageScroll.Add(BuildCard(kv.Key, kv.Value));
    }

    /// <summary>根据模板克隆一张卡片，填入 stage 名、该 stage 内 actor 名单，并绑定点击。</summary>
    private VisualElement BuildCard(string stageName, List<string> actorNames)
    {
        var card = _cardTemplate.CloneTree();
        card.Q<Label>("card-name").text = EntityNameUtils.GetDisplayName(stageName);
        card.Q<Label>("card-actors").text = string.Join("、", actorNames.Select(EntityNameUtils.GetDisplayName));
        card.RegisterCallback<ClickEvent>(_ => OnStageClicked(stageName));
        return card;
    }

    /// <summary>点击某张卡片（home stage）：记录 stage 名并切入 HomeStage 场景。</summary>
    private void OnStageClicked(string stageName)
    {
        GameManager.Instance.CurrentStageName = stageName;
        EnterHomeStageAsync(stageName).Forget();
    }

    private async UniTaskVoid EnterHomeStageAsync(string stageName)
    {
        Debug.Log($"[HomeOverview] 进入 home stage: {EntityNameUtils.GetDisplayName(stageName)}");
        await SceneManager.LoadSceneAsync(HomeStageSceneName);
    }

    /// <summary>返回大厅：登出（若有登录态）并切回 PlayerLobby 场景。</summary>
    private void OnBackClicked()
    {
        if (_backButton != null) _backButton.SetEnabled(false);
        LogoutAndBackToLobbyAsync().Forget();
    }

    private async UniTaskVoid LogoutAndBackToLobbyAsync()
    {
        var session = GameManager.Instance.Session;
        if (session != null)
        {
            try
            {
                var msg = await GameManager.Instance.ServerClient.LogoutAsync(session.UserName, session.GameName);
                Debug.Log($"[HomeOverview] 登出成功: {msg}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[HomeOverview] 登出失败（仍返回大厅）: {e.Message}");
            }
            GameManager.Instance.ClearSession();
        }

        await SceneManager.LoadSceneAsync(PlayerLobbySceneName);
    }
}
