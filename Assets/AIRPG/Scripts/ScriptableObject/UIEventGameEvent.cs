using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI事件游戏事件
/// 用于传递 UIEventData 结构化数据的事件系统
/// 用法：
/// 1. 在 Unity Editor 中创建事件资源: 右键 -> Create -> Scriptable Objects -> UI Event Game Event
/// 2. 在发送方脚本中引用并调用 Raise(UIEventData)
/// 3. 在接收方脚本中实现 IUIEventListener 接口并注册监听器
/// </summary>
[CreateAssetMenu(fileName = "UIEventGameEvent", menuName = "Scriptable Objects/UI Event Game Event", order = 2)]
public class UIEventGameEvent : ScriptableObject
{
    /// <summary>
    /// 事件监听器列表
    /// </summary>
    private readonly List<IUIEventListener> _listeners = new();

    /// <summary>
    /// 触发事件并传递 UIEventData 参数
    /// </summary>
    /// <param name="eventData">事件数据</param>
    public void Raise(UIEventData eventData)
    {
        // 从后向前遍历,防止在回调中移除监听器时出现问题
        for (int i = _listeners.Count - 1; i >= 0; i--)
        {
            _listeners[i].OnEventRaised(eventData);
        }
    }

    /// <summary>
    /// 注册监听器
    /// </summary>
    /// <param name="listener">要注册的监听器</param>
    public void RegisterListener(IUIEventListener listener)
    {
        if (!_listeners.Contains(listener))
        {
            _listeners.Add(listener);
        }
    }

    /// <summary>
    /// 取消注册监听器
    /// </summary>
    /// <param name="listener">要取消注册的监听器</param>
    public void UnregisterListener(IUIEventListener listener)
    {
        _listeners.Remove(listener);
    }

    /// <summary>
    /// 在编辑器中重新加载时清空监听器列表
    /// </summary>
    private void OnEnable()
    {
        // 确保 Play Mode 和 Edit Mode 之间状态清理
        _listeners.Clear();
    }
}
