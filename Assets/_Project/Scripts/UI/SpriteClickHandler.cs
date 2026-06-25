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
        // 检测输入（支持鼠标和触摸）
        bool hasInput = false;
        Vector2 inputPosition = Vector2.zero;

        // 优先检测触摸输入（移动设备）
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            hasInput = true;
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        // 如果没有触摸，检测鼠标输入（桌面）
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            hasInput = true;
            inputPosition = Mouse.current.position.ReadValue();
        }

        // 如果有输入，处理点击
        if (hasInput)
        {
            // 检查是否点击在UI上
            // 注意：移动端需要使用触摸ID来检测，这里使用-1作为通用检测
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1))
            {
                Debug.Log($"Clicked on UI, ignoring sprite click. so sprite {gameObject.name} ignore this event.");
                return; // 如果点击在UI上，不处理这个事件
            }

            // 从输入位置发射射线到世界坐标
            Vector2 worldPosition = _mainCamera.ScreenToWorldPoint(inputPosition);
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
