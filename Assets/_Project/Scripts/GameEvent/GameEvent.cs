using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基础游戏事件,用于实现解耦的事件系统
/// 使用方式:
/// 1. 在 Unity Editor 中创建事件资源: 右键 -> Create -> Events -> Game Event
/// 2. 在发送方脚本中引用并调用 Raise()
/// 3. 在接收方脚本中注册监听器
/// </summary>
[CreateAssetMenu(fileName = "GameEvent", menuName = "Scriptable Objects/Game Event", order = 0)]
public class GameEvent : ScriptableObject
{
    /// <summary>
    /// 事件监听器列表
    /// </summary>
    private readonly List<IGameEventListener> _listeners = new List<IGameEventListener>();

    /// <summary>
    /// 触发事件,通知所有监听器
    /// </summary>
    public void Raise()
    {
        // 从后向前遍历,防止在回调中移除监听器时出现问题
        for (int i = _listeners.Count - 1; i >= 0; i--)
        {
            _listeners[i].OnEventRaised();
        }
    }

    /// <summary>
    /// 注册监听器
    /// </summary>
    public void RegisterListener(IGameEventListener listener)
    {
        if (!_listeners.Contains(listener))
        {
            _listeners.Add(listener);
        }
    }

    /// <summary>
    /// 取消注册监听器
    /// </summary>
    public void UnregisterListener(IGameEventListener listener)
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
