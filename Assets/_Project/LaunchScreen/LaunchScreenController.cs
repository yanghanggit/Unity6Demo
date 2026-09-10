using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LaunchScreenController : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private PanelRenderer _panelRenderer;

    /// 内部数据
    private const string PlayerLobbySceneName = "PlayerLobby";
    /// UI 引用（在 OnPanelLoaded 中查询）
    private Button _button;
    private Label _statusLabel;

    // OnEnable/OnDisable 用于注册 Panel 加载回调，比 Start() 更可靠
    // （UXML 是异步加载的，Start() 时刻 Panel 可能还未就绪）
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

    /// <summary>
    /// Panel 加载完成回调：查询 UI 元素并启动初始加载
    /// </summary>
    void OnPanelLoaded(PanelRenderer pr, VisualElement root)
    {
        _button = root.Q<Button>("btn-start");
        _statusLabel = root.Q<Label>("status-label");
        Debug.Assert(_button != null, "_button is null");
        Debug.Assert(_statusLabel != null, "_statusLabel is null");

        _button.clicked += OnClick;

        // 启动异步加载服务器信息
        LoadAsync().Forget();
    }

    /// <summary>
    /// 点击事件处理函数：切入大厅场景
    /// </summary>
    public void OnClick()
    {
        /// 点击后按钮不可交互，避免重复点击
        EnterLobbyAsync().Forget();
    }

    /// <summary>
    /// 异步加载服务器信息
    /// </summary>
    private async UniTaskVoid LoadAsync()
    {
        _button.SetEnabled(false);
        SetStatusText("连接中...");
        try
        {
            JObject info = await GameServerClientHolder.Instance.Client.FetchServerInfoAsync(
                this.GetCancellationTokenOnDestroy());
            Debug.Log($"Server info: {info}");

            // 服务器连通，存储服务器信息，按钮恢复可交互
            GameManager.Instance.ServerInfo = info;
            _button.SetEnabled(true);
            SetStatusText(string.Empty);
        }
        catch (GameServerClient.ServerException ex)
        {
            Debug.LogError($"无法连接服务器 [{ex.StatusCode}]: {ex.Message}");
            // 连接失败：按钮保持不可交互（开发阶段暂不做重试）
            SetStatusText("连接失败");
        }
    }

    /// <summary>
    /// 异步切入大厅场景
    /// </summary>
    private async UniTaskVoid EnterLobbyAsync()
    {
        _button.SetEnabled(false);
        await SceneManager.LoadSceneAsync(PlayerLobbySceneName);
    }

    /// <summary>
    /// 设置状态文本
    /// </summary>
    private void SetStatusText(string statusText)
    {
        if (_statusLabel != null)
            _statusLabel.text = statusText ?? string.Empty;
    }
}