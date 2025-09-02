using UnityEngine;

/// <summary>
/// 背景控制器 - 用于自动调整背景Sprite大小以填充屏幕
/// </summary>
public class BackgroundController : MonoBehaviour
{
    [Header("背景设置")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("填充模式")]
    [SerializeField] private FillMode fillMode = FillMode.Fill;
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    
    /// <summary>
    /// 填充模式枚举
    /// </summary>
    public enum FillMode
    {
        Fill,       // 完全填充屏幕，可能会拉伸变形
        Fit,        // 适应屏幕，保持宽高比，可能有黑边
        Crop        // 裁剪填充，保持宽高比，可能会裁剪部分内容
    }
    
    private Camera mainCamera;
    private Vector2 originalSpriteSize;
    
    void Start()
    {
        InitializeBackground();
    }
    
    void OnValidate()
    {
        // 在编辑器中实时预览效果
        if (Application.isPlaying)
        {
            UpdateBackgroundScale();
        }
    }
    
    /// <summary>
    /// 初始化背景
    /// </summary>
    private void InitializeBackground()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("BackgroundController: 找不到主摄像机!");
            return;
        }
        
        // 获取或创建SpriteRenderer
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        // 记录原始Sprite尺寸
        if (spriteRenderer.sprite != null)
        {
            originalSpriteSize = spriteRenderer.sprite.bounds.size;
            UpdateBackgroundScale();
        }
        else
        {
            Debug.LogWarning("BackgroundController: 请为SpriteRenderer分配一个Sprite!");
        }
    }
    
    /// <summary>
    /// 更新背景缩放
    /// </summary>
    public void UpdateBackgroundScale()
    {
        if (mainCamera == null || spriteRenderer == null || spriteRenderer.sprite == null)
            return;
        
        // 获取摄像机视野的世界坐标尺寸
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        // 获取Sprite的原始尺寸
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        
        // 计算缩放比例
        float scaleX = cameraWidth / spriteSize.x;
        float scaleY = cameraHeight / spriteSize.y;
        
        Vector3 newScale = Vector3.one;
        
        switch (fillMode)
        {
            case FillMode.Fill:
                // 完全填充，可能变形
                newScale = new Vector3(scaleX, scaleY, 1f);
                break;
                
            case FillMode.Fit:
                // 适应屏幕，保持宽高比，取较小的缩放值
                float fitScale = Mathf.Min(scaleX, scaleY);
                newScale = new Vector3(fitScale, fitScale, 1f);
                break;
                
            case FillMode.Crop:
                // 裁剪填充，保持宽高比，取较大的缩放值
                float cropScale = Mathf.Max(scaleX, scaleY);
                newScale = new Vector3(cropScale, cropScale, 1f);
                break;
        }
        
        // 应用缩放
        transform.localScale = newScale;
        
        // 确保背景位置与摄像机中心对齐
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 position = new Vector3(cameraPosition.x, cameraPosition.y, 10f); // Z轴确保在其他游戏对象后面
        transform.position = position;
        
        // 调试信息
        if (showDebugInfo)
        {
            Debug.Log($"BackgroundController: 摄像机尺寸({cameraWidth:F2}, {cameraHeight:F2}), " +
                     $"Sprite尺寸({spriteSize.x:F2}, {spriteSize.y:F2}), " +
                     $"缩放比例({newScale.x:F2}, {newScale.y:F2})");
        }
    }
    
    /// <summary>
    /// 设置背景Sprite
    /// </summary>
    /// <param name="sprite">要设置的Sprite</param>
    public void SetBackgroundSprite(Sprite sprite)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        spriteRenderer.sprite = sprite;
        if (sprite != null)
        {
            originalSpriteSize = sprite.bounds.size;
            UpdateBackgroundScale();
        }
    }
    
    /// <summary>
    /// 设置填充模式
    /// </summary>
    /// <param name="mode">填充模式</param>
    public void SetFillMode(FillMode mode)
    {
        fillMode = mode;
        UpdateBackgroundScale();
    }
    
    /// <summary>
    /// 当屏幕尺寸改变时调用（主要用于编辑器）
    /// </summary>
    void OnRenderObject()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            UpdateBackgroundScale();
        }
    }
}
