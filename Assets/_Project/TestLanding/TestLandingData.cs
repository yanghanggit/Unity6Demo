using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// TestLanding 页面的数据源：驱动 UXML 里中间 Label 显示的文本。
/// 通过 UI Toolkit Runtime Data Binding 绑定到 TestLanding.uxml，
/// 不需要额外写 C# 代码去查询/赋值 UI 元素——修改这个资源的 Test Text 字段，
/// UI 会自动刷新。
/// </summary>
/**
binding-mode="ToTarget" —— 这个不是默认值，是我特意显式声明的。我查了下 Unity 官方文档确认：BindingMode 一共 4 种：
TwoWay（默认值）：源 ↔ UI 双向同步
ToTarget：只从源 → UI（单向，只读展示）
ToSource：只从 UI → 源（单向，UI 改会写回数据源）
ToTargetOnce：只在绑定建立那一刻同步一次，之后除非手动 MarkDirty 否则不再更新
*/
[CreateAssetMenu(fileName = "TestLandingData", menuName = "TestLanding/Test Landing Data")]
public class TestLandingData : ScriptableObject, INotifyBindablePropertyChanged
{
    [SerializeField] private string m_TestText = "Test";

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    [CreateProperty]
    public string testText
    {
        get => m_TestText;
        set
        {
            if (m_TestText == value)
                return;

            m_TestText = value;
            Notify();
        }
    }

    private void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    /// <summary>
    /// Inspector 里直接编辑 m_TestText 字段时，Unity 是通过序列化系统直接写入字段的，
    /// 不会经过 testText 的 setter，所以不会触发 Notify()。
    /// OnValidate 在 Inspector 修改后一定会被调用（Edit Mode / Play Mode 都会），
    /// 在这里补发一次通知，让 Inspector 手动改值时 UI 也能实时刷新。
    /// </summary>
    private void OnValidate()
    {
        Notify(nameof(testText));
    }
}
