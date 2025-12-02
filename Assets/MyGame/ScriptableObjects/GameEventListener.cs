using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 游戏事件监听器组件
/// 挂载到 GameObject 上,在 Inspector 中配置响应事件
/// 无需编写代码即可响应事件
/// </summary>
public class GameEventListener : MonoBehaviour, IGameEventListener
{
    [Header("监听的事件")]
    [Tooltip("要监听的 GameEvent 资源")]
    [SerializeField] private GameEvent _gameEvent;

    [Header("响应回调")]
    [Tooltip("事件触发时执行的回调")]
    [SerializeField] private UnityEvent _response;

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

    public void OnEventRaised()
    {
        _response?.Invoke();
    }
}
