using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 字符串游戏事件监听器组件
/// </summary>
public class StringGameEventListener : MonoBehaviour, IStringGameEventListener
{
    [Header("监听的事件")]
    [SerializeField] private StringGameEvent _gameEvent;

    [Header("响应回调")]
    [SerializeField] private UnityEvent<string> _response;

    private void OnEnable()
    {
        if (_gameEvent != null)
        {
            _gameEvent.RegisterListener(this);
        }
    }

    private void OnDisable()
    {
        if (_gameEvent != null)
        {
            _gameEvent.UnregisterListener(this);
        }
    }

    public void OnEventRaised(string value)
    {
        _response?.Invoke(value);
    }
}
