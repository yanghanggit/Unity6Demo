// 背景/全屏 Sprite 自适应缩放组件。
// 把 SpriteRenderer 等比缩放到铺满（Cover）或完整显示（Fit）正交相机视口，避免手动计算 scale。
// 挂到带 SpriteRenderer 的物体上即可；相机留空时自动使用 Camera.main。

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    public enum Mode
    {
        /// <summary>等比铺满视口，超出部分裁切（背景图推荐）。</summary>
        Cover,
        /// <summary>等比缩放、完整显示，多余空间留空。</summary>
        Fit,
    }

    [Tooltip("Cover=铺满裁切（背景推荐）；Fit=完整显示。")]
    [SerializeField] private Mode _mode = Mode.Cover;

    [Tooltip("目标相机；留空则用 Camera.main。")]
    [SerializeField] private Camera _camera;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_camera == null) _camera = Camera.main;
    }

    private void Start()
    {
        Apply();
    }

    /// <summary>
    /// 立即重新计算并应用缩放。可在 Inspector 右键组件 → Apply 手动触发，
    /// 或运行期视口变化后调用。
    /// </summary>
    [ContextMenu("Apply")]
    public void Apply()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_camera == null) _camera = Camera.main;

        if (_spriteRenderer == null || _spriteRenderer.sprite == null)
        {
            Debug.LogWarning("[BackgroundScaler] 未找到 SpriteRenderer 或未设置 Sprite。", this);
            return;
        }

        if (_camera == null || !_camera.orthographic)
        {
            Debug.LogWarning("[BackgroundScaler] 需要正交相机（Orthographic）。", this);
            return;
        }

        Vector2 viewport = GetViewportWorldSize();
        Vector2 sprite = _spriteRenderer.sprite.bounds.size;

        float scale = _mode == Mode.Cover
            ? Mathf.Max(viewport.x / sprite.x, viewport.y / sprite.y)
            : Mathf.Min(viewport.x / sprite.x, viewport.y / sprite.y);

        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private Vector2 GetViewportWorldSize()
    {
        float height = _camera.orthographicSize * 2f;
        float width = height * _camera.aspect;
        return new Vector2(width, height);
    }
}
