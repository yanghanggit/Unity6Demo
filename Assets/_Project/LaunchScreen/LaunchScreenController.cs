using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/**
是的，你的这套设置基本是标准的手机竖屏设置，核心参数都选对了。不过，有一个关键参数（Match 滑块）会直接影响不同屏幕比例的适配效果，需要重点确认一下。

我从几个核心维度帮你拆解分析：

✅ 符合标准的设置
渲染模式（Render Mode）：选择 Screen Space Overlay，这是手游UI最标准的方式，UI永远显示在3D场景最上层。

缩放模式（Scale Mode）：选择 Scale With Screen Size，这是适配多分辨率屏幕的必选项。

参考分辨率（Reference Resolution）：设为 1080 x 1920，这是标准的竖屏 FHD+ 分辨率，可以作为很好的设计基准。

⚠️ 需要重点确认的参数：Screen Match Mode 的 Match 滑块
这是决定你UI在不同竖屏手机（比如 19.5:9 的刘海屏 vs 16:9 的传统屏）上是否变形或裁切的核心。

你截图中的设置：选择了 Match Width Or Height，并同时勾选了 Width 和 Height（在 Unity 中这代表一个 0~1 的滑块）。

给你的建议（非常重要）：强烈建议将 Match 滑块的值设为 0（即完全匹配宽度）。

原因很简单：竖屏手机的宽度变化范围很小（通常在 360~430 逻辑像素之间），而高度变化极大（因为有状态栏、底部手势条、刘海等）。如果匹配高度（Match = 1），当屏幕变长时，UI 会被纵向拉伸变形；如果取中间值 0.5，则宽高会同时缩放，依然会变形。只有固定匹配宽度（Match = 0），UI 的宽度始终填满屏幕，高度则自适应增加，内容不会变形，只是上下会显示更多或更少的背景空间。

📦 其他参数检查
动态图集（Dynamic Atlas）：保持默认（64~4096）即可，手游开发通常开启，能有效合并纹理降低 Draw Call。

清除设置（Clear Settings）：因为用的是 Screen Space Overlay，这个清除设置只对 Render Texture 模式有效，对当前的设置没有影响，无需理会。

总结一下：你的设置完全符合手机竖屏标准，只要把 Match 滑块拉到最左边（Width = 0），就是最稳妥、最通用的竖屏适配方案了。如果项目有特殊需求（比如固定高度做卷轴效果），再根据需要调整即可。
*/

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