using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

/// <summary>
/// Actor 头像控制器
/// 用于生成并显示 Actor 的 AI 头像图片
/// 继承自 CachedRemoteImageController，专注于 Actor 特定的业务逻辑
/// </summary>
public class ActorPortraitController : CachedRemoteImageController
{
    [Header("Actor Settings")]
    public string ActorName { get; set; }
    public string Prompt { get; set; }
    public string ImageUrlStorageKey { get; set; }
    public string SpriteCacheKey { get; set; }

    // [Header("Test Settings")]
    // [SerializeField] private bool _useMockMode = false;
    // [SerializeField] private float _mockGenerationDelay = 2.0f;
    // [SerializeField] private string _mockImageUrl = "/images/nano-banana_fb300c55-3130-4ac2-9e9e-19ee8da8f3e1.png";

    void Start()
    {
        // 参数验证
        Debug.Assert(!string.IsNullOrEmpty(ActorName), "[ActorPortraitController] ActorName is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(Prompt), "[ActorPortraitController] Prompt is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(ImageUrlStorageKey), "[ActorPortraitController] ImageUrlStorageKey is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(SpriteCacheKey), "[ActorPortraitController] SpriteCacheKey is null or empty");
        Debug.Assert(ImageUrlStorageKey == SpriteCacheKey, "[ActorPortraitController] For simplicity, ImageUrlStorageKey should equal SpriteCacheKey in current implementation");

        //检查是否已有缓存的Sprite, 有就直接显示
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(SpriteCacheKey);
        if (cachedSprite != null)
        {
            Debug.Log($"[ActorPortraitController] Found cached sprite with key '{SpriteCacheKey}', displaying it directly.");
            _targetImage.sprite = cachedSprite;
            return;
        }

        // 未命中内存缓存，立即显示默认图标（后续需要网络操作）
        _targetImage.sprite = SpriteCacheManager.Instance.GetSprite(SpriteCacheManager.DefaultIconKey);

        // 检查图片生成服务器是否开启
        // if (ApiEndpointsManager.ImageRootResponse == null)
        // {
        //     Debug.LogWarning("[ActorPortraitController] Image generation API endpoint is not configured, skipping all network operations");
        //     return;
        // }

        // // 检查是否有持久化的URL映射
        // if (ImageService.HasImageUrl(ImageUrlStorageKey))
        // {
        //     string cachedUrl = ImageService.GetImageUrl(ImageUrlStorageKey);
        //     Debug.Log($"[ActorPortraitController] Found cached URL with key '{ImageUrlStorageKey}', loading directly (skip generation)");

        //     // Mock逻辑分叉：加载缓存URL
        //     if (_useMockMode)
        //     {
        //         StartCoroutine(LoadAndDisplayImageFromUrlMock(cachedUrl, ImageUrlStorageKey, SpriteCacheKey, (success) =>
        //         {
        //             if (!success)
        //             {
        //                 ImageService.RemoveImageUrl(ImageUrlStorageKey);
        //                 Debug.LogWarning($"[ActorPortraitController] Cached URL is invalid, removed mapping for key '{ImageUrlStorageKey}'. Will regenerate on next launch.");
        //             }
        //         }));
        //     }
        //     else
        //     {
        //         StartCoroutine(LoadAndDisplayImageFromUrl(cachedUrl, ImageUrlStorageKey, SpriteCacheKey, (success) =>
        //         {
        //             if (!success)
        //             {
        //                 ImageService.RemoveImageUrl(ImageUrlStorageKey);
        //                 Debug.LogWarning($"[ActorPortraitController] Cached URL is invalid, removed mapping for key '{ImageUrlStorageKey}'. Will regenerate on next launch.");
        //             }
        //         }));
        //     }
        //     return;
        // }

        // // 没有URL映射，需要生成新图片
        // Debug.Log($"[ActorPortraitController] No cached URL found, starting image generation with prompt: \n{Prompt}");

        // // Mock逻辑分叉：生成图片
        // if (_useMockMode)
        // {
        //     StartCoroutine(GenerateAndDisplayImageMock(ImageUrlStorageKey, SpriteCacheKey));
        // }
        // else
        // {
        //     var configs = new List<ImageGenerationConfig>
        //     {
        //         new() { prompt = Prompt, model = _modelName, width = _imageWidth, height = _imageHeight, num_inference_steps = _numInferenceSteps}
        //     };

        //     StartCoroutine(GenerateAndDisplayImage(configs, ImageUrlStorageKey, SpriteCacheKey));
        // }
    }

    /// <summary>
    /// Mock实现：模拟从缓存URL加载图片
    /// 用于测试缓存URL加载流程，使用mock URL替代实际URL
    /// </summary>
    /// <param name="originalUrl">原始URL（仅用于日志记录）</param>
    /// <param name="imageUrlStorageKey">图片URL存储键，用于失败时删除映射</param>
    /// <param name="spriteCacheKey">精灵缓存键</param>
    /// <param name="onComplete">加载完成回调，参数为加载是否成功</param>
    // private IEnumerator LoadAndDisplayImageFromUrlMock(string originalUrl, string imageUrlStorageKey, string spriteCacheKey, System.Action<bool> onComplete)
    // {
    //     Debug.Log($"[ActorPortraitController] 🔧 Mock模式：模拟从缓存URL加载");
    //     Debug.Log($"[ActorPortraitController] 🔧 原始URL: {originalUrl}");
    //     Debug.Log($"[ActorPortraitController] 🔧 使用Mock URL: {_mockImageUrl}");

    //     // 模拟网络延迟
    //     yield return new WaitForSeconds(_mockGenerationDelay);

    //     Debug.Log($"[ActorPortraitController] ✅ Mock加载完成");

    //     // 使用mock URL加载
    //     yield return LoadAndDisplayImageFromUrl(_mockImageUrl, imageUrlStorageKey, spriteCacheKey, onComplete);
    // }

    // /// <summary>
    // /// Mock实现：模拟图片生成并显示
    // /// 用于测试图片加载和显示功能，跳过实际的AI生成API调用
    // /// </summary>
    // /// <param name="imageUrlStorageKey">图片URL存储键</param>
    // /// <param name="spriteCacheKey">精灵缓存键</param>
    // private IEnumerator GenerateAndDisplayImageMock(string imageUrlStorageKey, string spriteCacheKey)
    // {
    //     Debug.Log($"[ActorPortraitController] 🔧 Mock模式启动，模拟生成延迟: {_mockGenerationDelay}秒");

    //     // 模拟生成等待
    //     yield return new WaitForSeconds(_mockGenerationDelay);

    //     Debug.Log($"[ActorPortraitController] ✅ Mock生成完成，使用URL: {_mockImageUrl}");

    //     // 创建模拟的图片信息对象
    //     var mockImageInfo = new GeneratedImage
    //     {
    //         url = _mockImageUrl,
    //         filename = System.IO.Path.GetFileName(_mockImageUrl),
    //         prompt = "Mock test image",
    //         model = "nano-banana"
    //     };

    //     // 加载并显示图片
    //     yield return LoadAndDisplayImage(mockImageInfo, imageUrlStorageKey, spriteCacheKey);
    // }
}



