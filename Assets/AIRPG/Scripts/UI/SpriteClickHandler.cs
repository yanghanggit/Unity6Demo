using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.EventSystems;

/// <summary>
/// 简单的精灵点击处理器
/// 使用方法：
/// 1. 将此脚本挂载到需要响应点击的精灵GameObject上
/// 2. 确保该GameObject有Collider2D组件（如BoxCollider2D）
/// 
/// 更新日志：
/// - 已升级为使用新Input System (Unity 6+)
/// - 使用Physics2D.Raycast进行点击检测，保持原有Collider2D检测逻辑
/// </summary>
public class SpriteClickHandler : MonoBehaviour
{
    // 点击事件
    public event Action<SpriteClickHandler> OnSpriteClicked;

    private Camera _mainCamera;

    void Start()
    {
        // 检查是否有Collider2D组件
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"SpriteClickHandler on {gameObject.name} requires a Collider2D component to detect mouse events.");
        }

        // 缓存主摄像机引用
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError($"SpriteClickHandler on {gameObject.name}: Main Camera not found!");
        }
    }

    void Update()
    {
        // 检测鼠标左键是否在当前帧按下
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 检查是否点击在UI上
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log($"Clicked on UI, ignoring sprite click. so sprite {gameObject.name} ignore this event.");
                return; // 如果点击在UI上，不处理这个事件
            }

            // 从鼠标位置发射射线到世界坐标
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

            // 检查是否点击到当前对象
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Debug.Log($"Sprite {gameObject.name} clicked!");

                // 触发点击事件
                OnSpriteClicked?.Invoke(this);
            }
        }
    }
}
