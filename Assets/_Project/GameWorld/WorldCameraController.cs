using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 拖拽平移摄像机：世界比摄像机可视范围大，通过鼠标/触摸拖拽来平移摄像机。
/// 使用 Pointer.current（Mouse/Touchscreen 的公共基类）统一处理鼠标与触摸输入。100 像素=1 世界单位
/// </summary>
[RequireComponent(typeof(Camera))]
public class WorldCameraController : MonoBehaviour
{
    [Header("摄像机")]
    [SerializeField] private Camera _camera;

    [Header("世界边界（裁剪摄像机可视范围，防止拖出地图）")]
    [SerializeField] private Rect _worldBounds = new(-20, -20, 40, 40);

    private bool _dragging;
    private Vector2 _lastPointerScreenPos;

    void Awake()
    {
        Debug.Assert(_camera != null, "_camera is null");
    }

    void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null)
            return;

        if (pointer.press.wasPressedThisFrame)
        {
            _dragging = true;
            _lastPointerScreenPos = pointer.position.ReadValue();
        }
        else if (pointer.press.wasReleasedThisFrame)
        {
            _dragging = false;
        }
        else if (_dragging)
        {
            Vector2 currentScreenPos = pointer.position.ReadValue();
            Vector2 screenDelta = currentScreenPos - _lastPointerScreenPos;
            _lastPointerScreenPos = currentScreenPos;

            if (screenDelta == Vector2.zero)
                return;

            // 屏幕像素位移转换为世界坐标位移（正交摄像机下与深度无关）
            Vector3 worldDeltaAtOrigin = _camera.ScreenToWorldPoint(new Vector3(screenDelta.x, screenDelta.y, _camera.nearClipPlane));
            Vector3 worldDeltaAtZero = _camera.ScreenToWorldPoint(new Vector3(0, 0, _camera.nearClipPlane));
            Vector3 worldDelta = worldDeltaAtOrigin - worldDeltaAtZero;

            Vector3 newPos = _camera.transform.position - worldDelta;
            _camera.transform.position = ClampToWorldBounds(newPos);
        }
    }

    /// <summary>
    /// 按摄像机半宽高裁剪位置，确保可视范围始终落在世界边界内。
    /// </summary>
    private Vector3 ClampToWorldBounds(Vector3 pos)
    {
        float halfHeight = _camera.orthographicSize;
        float halfWidth = halfHeight * _camera.aspect;

        float minX = _worldBounds.xMin + halfWidth;
        float maxX = _worldBounds.xMax - halfWidth;
        float minY = _worldBounds.yMin + halfHeight;
        float maxY = _worldBounds.yMax - halfHeight;

        // 世界比可视范围小时，直接居中，避免 Clamp 上下限颠倒
        pos.x = minX <= maxX ? Mathf.Clamp(pos.x, minX, maxX) : _worldBounds.center.x;
        pos.y = minY <= maxY ? Mathf.Clamp(pos.y, minY, maxY) : _worldBounds.center.y;
        return pos;
    }

    /// <summary>
    /// 在 Scene 视图里画出 _worldBounds 的矩形线框（黄色），方便对齐背景美术资源；
    /// 同时画出摄像机中心实际可移动的范围（青色，已按半宽高内缩）。
    /// </summary>
    void OnDrawGizmos()
    {
        Vector3 center = new(_worldBounds.center.x, _worldBounds.center.y, 0);
        Vector3 size = new(_worldBounds.width, _worldBounds.height, 0);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);

        var cam = _camera != null ? _camera : GetComponent<Camera>();
        if (cam == null || !cam.orthographic)
            return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float innerWidth = _worldBounds.width - halfWidth * 2;
        float innerHeight = _worldBounds.height - halfHeight * 2;

        if (innerWidth > 0 && innerHeight > 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(center, new Vector3(innerWidth, innerHeight, 0));
        }
    }
}
