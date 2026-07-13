using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂在需要在其正上方悬浮显示文本的世界对象上（比如 Scene1/Scene2）。
/// 文本内容在 Inspector 里配置；实际的 UI Toolkit Label 元素由 WorldLabelPanelController
/// 统一创建、维护位置和文本，本组件只负责暴露数据。
/// </summary>
public class WorldLabel : MonoBehaviour
{
    /// <summary>当前场景中所有已启用的 WorldLabel，供 WorldLabelPanelController 统一遍历。</summary>
    public static readonly HashSet<WorldLabel> ActiveLabels = new();

    [SerializeField] private string _text = "";

    [Tooltip("相对自身 Transform 的世界坐标偏移，用于把文本放在对象上方")]
    [SerializeField] private Vector3 _worldOffset = new(0, 0.6f, 0);

    public string Text
    {
        get => _text;
        set => _text = value;
    }
    
    public Vector3 WorldOffset => _worldOffset;

    void OnEnable() => ActiveLabels.Add(this);
    void OnDisable() => ActiveLabels.Remove(this);
}
