using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// TextInputField（单行文本输入框）最简演示。
/// 演示要点：
///   1. TextField.value 读写：读取用户输入
///   2. 两种确认方式：点击"确认"按钮 / 输入框内按回车
///   3. 确认后把输入回显到结果 Label + 控制台日志
/// </summary>
public class TestTextInputFieldDemo : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private PanelRenderer _panelRenderer;

    private TextField _inputField;
    private Button _confirmButton;
    private Label _resultLabel;

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
        _inputField = root.Q<TextField>("input-field");
        _confirmButton = root.Q<Button>("btn-confirm");
        _resultLabel = root.Q<Label>("result-label");

        Debug.Assert(_inputField != null, "[TestTextInputField] 未找到 input-field，请检查 UXML");
        Debug.Assert(_confirmButton != null, "[TestTextInputField] 未找到 btn-confirm，请检查 UXML");
        Debug.Assert(_resultLabel != null, "[TestTextInputField] 未找到 result-label，请检查 UXML");

        // 两种确认方式：点按钮 / 输入框内按回车
        _confirmButton.clicked += OnConfirmClicked;
        _inputField.RegisterCallback<KeyDownEvent>(OnInputKeyDown);
    }

    private void OnInputKeyDown(KeyDownEvent evt)
    {
        // 主键盘 Enter / 小键盘 Enter
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            Confirm();
    }

    private void OnConfirmClicked()
    {
        Confirm();
    }

    private void Confirm()
    {
        var text = _inputField.value ?? string.Empty;
        Debug.Log($"[TestTextInputField] 确认输入: \"{text}\"");

        _resultLabel.text = string.IsNullOrWhiteSpace(text)
            ? "（空输入）"
            : $"已确认：{text}";
    }
}
