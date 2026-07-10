using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 世界坐标跟随文本标签的独立 Panel 控制器：挂在专属的 PanelRenderer（PanelSettings 的
/// Sort Order 比 HUD 低）上，与 HUD 的 PanelRenderer 完全独立。因为这个 Panel 里没有任何
/// 可交互元素，不会与 HUD 的按钮争抢点击路由；也因为 WorldCameraController / WorldClickDetector
/// 都直接轮询 Pointer.current、不经过 UI Toolkit 的事件路由，所以这里加多少个 PanelRenderer
/// 都不会影响拖拽平移和精灵点击。
/// 负责把 WorldLabel 的世界坐标换算成 Panel 坐标，每帧更新对应 Label 的位置和文本。
/// </summary>
[RequireComponent(typeof(PanelRenderer))]
public class WorldLabelPanelController : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private PanelRenderer _panelRenderer;

    [Header("摄像机")]
    [SerializeField] private Camera _camera;

    private VisualElement _labelsRoot;
    private readonly Dictionary<WorldLabel, Label> _labelElements = new();
    private readonly List<WorldLabel> _removalBuffer = new();

    void OnEnable()
    {
        if (_panelRenderer == null)
            _panelRenderer = GetComponent<PanelRenderer>();
        if (_camera == null)
            _camera = Camera.main;

        Debug.Assert(_panelRenderer != null, "_panelRenderer is null");
        if (_panelRenderer != null)
            _panelRenderer.RegisterUIReloadCallback(OnPanelLoaded);
    }

    void OnDisable()
    {
        Debug.Assert(_panelRenderer != null, "_panelRenderer is null");
        if (_panelRenderer != null)
            _panelRenderer.UnregisterUIReloadCallback(OnPanelLoaded);

        _labelsRoot = null;
        _labelElements.Clear();
    }

    /// <summary>
    /// Panel 加载完成回调：查询 world-labels-root 容器。
    /// 这个 Panel 里没有任何可交互元素，pickingMode = Ignore 只是防御性设置。
    /// </summary>
    void OnPanelLoaded(PanelRenderer pr, VisualElement root)
    {
        _labelsRoot = root.Q<VisualElement>("world-labels-root");
        Debug.Assert(_labelsRoot != null, "world-labels-root is null");
        if (_labelsRoot != null)
            _labelsRoot.pickingMode = PickingMode.Ignore;

        _labelElements.Clear();
    }

    void LateUpdate()
    {
        if (_labelsRoot == null || _camera == null)
            return;

        SyncLabelElements();

        foreach (var pair in _labelElements)
            UpdateLabelPosition(pair.Key, pair.Value);
    }

    /// <summary>
    /// 让 _labelElements 与当前激活的 WorldLabel 集合保持一致：新增的创建 Label，消失的移除 Label。
    /// </summary>
    private void SyncLabelElements()
    {
        foreach (var worldLabel in WorldLabel.ActiveLabels)
        {
            if (_labelElements.ContainsKey(worldLabel))
                continue;

            var label = new Label(worldLabel.Text);
            label.AddToClassList("world-label");
            _labelsRoot.Add(label);
            _labelElements[worldLabel] = label;
        }

        if (_labelElements.Count <= WorldLabel.ActiveLabels.Count)
            return;

        _removalBuffer.Clear();
        foreach (var worldLabel in _labelElements.Keys)
        {
            if (!WorldLabel.ActiveLabels.Contains(worldLabel))
                _removalBuffer.Add(worldLabel);
        }
        foreach (var worldLabel in _removalBuffer)
        {
            _labelElements[worldLabel].RemoveFromHierarchy();
            _labelElements.Remove(worldLabel);
        }
    }

    private void UpdateLabelPosition(WorldLabel worldLabel, Label labelElement)
    {
        labelElement.text = worldLabel.Text;

        Vector3 worldPos = worldLabel.transform.position + worldLabel.WorldOffset;
        Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);

        // 摄像机背后（正交摄像机一般不会发生，兜底隐藏）
        bool behindCamera = screenPos.z < 0;
        labelElement.style.display = behindCamera ? DisplayStyle.None : DisplayStyle.Flex;
        if (behindCamera)
            return;

        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(_labelsRoot.panel, new Vector2(screenPos.x, screenPos.y));
        labelElement.style.left = panelPos.x;
        labelElement.style.top = panelPos.y;
    }
}
