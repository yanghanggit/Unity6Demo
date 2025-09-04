using UnityEngine;
using System;
using UnityEngine.EventSystems;

/// <summary>
/// 简单的精灵点击处理器
/// 使用方法：
/// 1. 将此脚本挂载到需要响应点击的精灵GameObject上
/// 2. 确保该GameObject有Collider2D组件（如BoxCollider2D）
/// </summary>
public class SpriteClickHandler : MonoBehaviour
{
    // 点击事件
    public event Action<SpriteClickHandler> OnSpriteClicked;

    void Start()
    {
        // 检查是否有Collider2D组件
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"SpriteClickHandler on {gameObject.name} requires a Collider2D component to detect mouse events.");
        }
    }

    /// <summary>
    /// 鼠标按下事件 - Unity内置方法
    /// 需要对象有Collider2D组件才能触发
    /// </summary>
    void OnMouseDown()
    {
        // 检查是否点击在UI上
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Clicked on UI, ignoring sprite click.");
            return; // 如果点击在UI上，不处理这个事件
        }

        Debug.Log($"Sprite {gameObject.name} clicked!");

        // 触发点击事件
        OnSpriteClicked?.Invoke(this);
    }
}
