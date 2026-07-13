using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class TopHUDController : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private PanelRenderer _panelRenderer;

    private IPanel _registeredPanel;

    void OnEnable()
    {
        if (_panelRenderer == null)
            _panelRenderer = GetComponent<PanelRenderer>();

        Debug.Assert(_panelRenderer != null, "_panelRenderer is null");
        if (_panelRenderer != null)
            _panelRenderer.RegisterUIReloadCallback(OnPanelLoaded);
    }

    void OnDisable()
    {
        Debug.Assert(_panelRenderer != null, "_panelRenderer is null");
        if (_panelRenderer != null)
            _panelRenderer.UnregisterUIReloadCallback(OnPanelLoaded);

        if (_registeredPanel != null)
        {
            UIPointerGate.Unregister(_registeredPanel);
            _registeredPanel = null;
        }
    }

    /// <summary>
    /// Panel 加载完成回调：查询 UI 元素并注册按钮事件
    /// </summary>
    void OnPanelLoaded(PanelRenderer pr, VisualElement root)
    {
        // 只将自己的 top-root 容器设为不拦截点击（而不是整个共享 root），
        // 否则它会锤住同一棵树里 bottom-root 邨个全屏容器的点击
        var topRoot = root.Q<VisualElement>("top-root");
        Debug.Assert(topRoot != null, "top-root is null");
        if (topRoot != null)
            topRoot.pickingMode = PickingMode.Ignore;

        var testButton = root.Q<Button>("btn-top-test");
        Debug.Assert(testButton != null, "btn-top-test is null");

        testButton.clicked += OnClickTestButton;

        // 把本 Panel 注册进全局点击网关，避免点击本 HUD 按钮时事件穿透到场景世界对象
        _registeredPanel = root.panel;
        UIPointerGate.Register(_registeredPanel);
    }

    void OnClickTestButton()
    {
        Debug.Log("[TopHUD] 测试按钮被点击");
    }
}
