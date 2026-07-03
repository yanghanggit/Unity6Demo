using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 带字符串参数的游戏事件
/// 用于传递字符串数据的事件
/// </summary>
[CreateAssetMenu(fileName = "StringGameEvent", menuName = "Scriptable Objects/String Game Event", order = 1)]
public class StringGameEvent : ScriptableObject
{
    private readonly List<IStringGameEventListener> _listeners = new List<IStringGameEventListener>();

    /// <summary>
    /// 触发事件并传递字符串参数
    /// </summary>
    public void Raise(string value)
    {
        for (int i = _listeners.Count - 1; i >= 0; i--)
        {
            _listeners[i].OnEventRaised(value);
        }
    }

    public void RegisterListener(IStringGameEventListener listener)
    {
        if (!_listeners.Contains(listener))
        {
            _listeners.Add(listener);
        }
    }

    public void UnregisterListener(IStringGameEventListener listener)
    {
        _listeners.Remove(listener);
    }

    private void OnEnable()
    {
        _listeners.Clear();
    }
}
