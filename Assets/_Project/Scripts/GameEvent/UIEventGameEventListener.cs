using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// UI事件监听器组件（可选）
/// 挂载到 GameObject 上，在 Inspector 中配置响应事件
/// 无需编写代码即可响应 UIEventGameEvent
/// </summary>
public class UIEventGameEventListener : MonoBehaviour, IUIEventListener
{
    [Header("监听的事件")]
    [Tooltip("要监听的 UIEventGameEvent 资源")]
    [SerializeField] private UIEventGameEvent _gameEvent;

    [Header("响应回调")]
    [Tooltip("事件触发时执行的回调，参数为 UIEventData 的 JSON 字符串")]
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

    /// <summary>
    /// IUIEventListener 接口实现
    /// 将 UIEventData 转换为字符串后传递给 UnityEvent
    /// </summary>
    public void OnEventRaised(UIEventData eventData)
    {
        // 将事件数据转换为可读字符串
        string eventString = eventData.ToString();
        _response?.Invoke(eventString);
    }
}
