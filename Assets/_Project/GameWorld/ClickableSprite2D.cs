using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 通用的"可点击精灵"组件：挂在带 Collider2D 的对象上，点击时触发 <see cref="onClick"/>。
/// 具体点击行为在 Inspector 里通过 UnityEvent 绑定，无需为每个对象单独写脚本。
/// 需要场景中存在 <see cref="WorldClickDetector"/> 才能收到点击回调。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ClickableSprite2D : MonoBehaviour, IWorldClickable
{
    [SerializeField] private UnityEvent onClick;

    public void OnWorldClick()
    {
        onClick?.Invoke();
    }
}
