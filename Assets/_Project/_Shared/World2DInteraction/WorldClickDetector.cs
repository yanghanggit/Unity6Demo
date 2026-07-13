using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 场景内 2D 对象的点击检测入口，不依赖 uGUI 的 EventSystem / Physics2DRaycaster，
/// 直接用 Physics2D + 新版 Input System（Pointer.current）实现，与 <see cref="WorldCameraController"/> 保持同一套输入范式。
/// 判定逻辑：按下时记录屏幕坐标，抬起时若位移未超过 <see cref="_clickMoveThreshold"/> 则视为"点击"，
/// 否则视为拖拽（不触发点击），从而与摄像机拖拽平移天然不冲突。
/// </summary>
public class WorldClickDetector : MonoBehaviour
{
    [Header("摄像机")]
    [SerializeField] private Camera _camera;

    [Header("点击判定")]
    [Tooltip("按下到抬起之间，屏幕像素位移超过该阈值则判定为拖拽，不触发点击")]
    [SerializeField] private float _clickMoveThreshold = 8f;

    private Vector2 _pressScreenPos;
    private bool _pressOverUI;

    void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;

        Debug.Assert(_camera != null, "_camera is null");
    }

    void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null)
            return;

        if (pointer.press.wasPressedThisFrame)
        {
            _pressScreenPos = pointer.position.ReadValue();
            _pressOverUI = UIPointerGate.IsPointerOverUI(_pressScreenPos);
        }
        else if (pointer.press.wasReleasedThisFrame)
        {
            Vector2 releaseScreenPos = pointer.position.ReadValue();
            if (Vector2.Distance(_pressScreenPos, releaseScreenPos) > _clickMoveThreshold)
                return; // 位移过大，判定为拖拽，不触发点击

            // 按下或抬起落在 HUD 等 UI Toolkit 可交互元素上时，视为 UI 操作，场景点击不响应，
            // 避免 UI 按钮点击穿透到下方的世界对象。
            if (_pressOverUI || UIPointerGate.IsPointerOverUI(releaseScreenPos))
                return;

            TryClickAt(releaseScreenPos);
        }
    }

    private void TryClickAt(Vector2 screenPos)
    {
        Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _camera.nearClipPlane));
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null && hit.TryGetComponent<IWorldClickable>(out var clickable))
            clickable.OnWorldClick();
    }
}
