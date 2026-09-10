using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// 家园总览（HomeOverview）控制器。
/// 从服务器拉取 stages state（stage → actor 名单）生成卡片；不缓存，每次现拉。
/// 联网+已登录走真实 API，否则用 MockGameData 离线调试。
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

        PopulateCardsAsync().Forget();
    }

    /// <summary>拉取 stages state 并重建卡片列表（不缓存，每次现拉）。联网+已登录走真实 API，否则用 MockGameData 离线调试。</summary>
    private async UniTaskVoid PopulateCardsAsync()
    {
        _stageScroll.Clear();

        if (GameManager.Instance.IsServerConnected && GameManager.Instance.Session != null)
        {
            await FetchStagesFromServerAsync();
        }
        else
        {
            PopulateMockCards();
        }
    }

    private async UniTask FetchStagesFromServerAsync()
    {
        var session = GameManager.Instance.Session;
        try
        {
            var state = await GameManager.Instance.ServerClient.FetchStagesStateAsync(session.UserName, session.GameName);
            foreach (var kv in state.mapping)
                _stageScroll.Add(BuildCard(kv.Key, kv.Value));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HomeOverview] 拉取 stages state 失败: {e.Message}");
        }
    }

    /// <summary>
    /// 使用 MockGameData 构建并显示卡片列表（离线调试用）。
    /// </summary>
    private void PopulateMockCards()
    {
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

    /// <summary>
    /// 进入指定的 home stage 场景。
    /// </summary>
    /// <param name="stageName"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 登出当前用户（若有登录态）并切回 PlayerLobby 场景。
    /// </summary>
    /// <returns></returns>
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
