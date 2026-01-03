using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 缓存远程图片控制器基类
/// 提供通用的远程图片生成、加载、缓存和显示功能
/// 需要挂载在包含Image组件的GameObject上
/// </summary>
[RequireComponent(typeof(Image))]
public class CachedRemoteImageController : MonoBehaviour
{
    [Header("Target Components")]
    [SerializeField] protected Image _targetImage;

    [Header("API Components")]
    [SerializeField] protected GenerateImageApi _generateImageApi;
    [SerializeField] protected TextureLoader _textureLoader;

    [Header("Generation Settings")]
    [SerializeField] protected string _modelName = "nano-banana";
    [SerializeField] protected int _imageWidth = 512;
    [SerializeField] protected int _imageHeight = 512;
    [SerializeField] protected int _numInferenceSteps = 4;

    [Header("Test Settings")]
    [SerializeField] protected bool _useMockMode = false;
    [SerializeField] protected float _mockGenerationDelay = 2.0f;
    [SerializeField] protected string _mockImageUrl = "/images/nano-banana_fb300c55-3130-4ac2-9e9e-19ee8da8f3e1.png";

    protected virtual void Awake()
    {
        // 自动获取组件引用（如果未在Inspector中赋值）
        if (_targetImage == null)
        {
            _targetImage = GetComponent<Image>();
        }

        if (_generateImageApi == null)
        {
            _generateImageApi = gameObject.AddComponent<GenerateImageApi>();
        }

        if (_textureLoader == null)
        {
            _textureLoader = gameObject.AddComponent<TextureLoader>();
        }

        // 组件检测
        Debug.Assert(_targetImage != null, "[CachedRemoteImageController] Image component is null on GameObject: " + gameObject.name);
        Debug.Assert(_targetImage.sprite == null, "[CachedRemoteImageController] Image component already has a sprite assigned on GameObject: " + gameObject.name);
        Debug.Assert(_generateImageApi != null, "[CachedRemoteImageController] GenerateImageApi is null");
        Debug.Assert(_textureLoader != null, "[CachedRemoteImageController] TextureLoader is null");

        // 保险用，后续只用 SpriteManager 来管理默认图标，如果有残留的图标（例如是编辑器场景测试时留下的），则删除它。
        // 否则使用 spriteManager 做生命周期管理时会出问题。
        if (_targetImage.sprite != null)
        {
            Debug.LogWarning("[CachedRemoteImageController] Target Image already has a sprite assigned, it will be replaced.");
            DestroyImmediate(_targetImage.sprite, true);
            _targetImage.sprite = null;
        }
    }

    /// <summary>
    /// 启动图片加载流程
    /// 供派生类在适当时机调用
    /// </summary>
    /// <param name="prompt">图片生成提示词</param>
    /// <param name="imageUrlStorageKey">图片URL存储键</param>
    /// <param name="spriteCacheKey">精灵缓存键</param>
    protected void StartImageLoadingProcess(string prompt, string imageUrlStorageKey, string spriteCacheKey)
    {
        // 检查是否已有缓存的Sprite, 有就直接显示
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(spriteCacheKey);
        if (cachedSprite != null)
        {
            Debug.Log($"[CachedRemoteImageController] Found cached sprite with key '{spriteCacheKey}', displaying it directly.");
            _targetImage.sprite = cachedSprite;
            return;
        }

        // 未命中内存缓存，立即显示默认图标（后续需要网络操作）
        var defaultActorIcon = SpriteCacheManager.Instance.GetSprite(SpriteCacheManager.DefaultIconKey);
        _targetImage.sprite = defaultActorIcon;

        // 检查图片生成服务器是否开启
        if (ApiEndpointsManager.ImageRootResponse == null)
        {
            Debug.LogWarning("[CachedRemoteImageController] Image generation API endpoint is not configured, skipping all network operations");
            return;
        }

        // 检查是否有持久化的URL映射
        if (ImageService.HasImageUrl(imageUrlStorageKey))
        {
            string cachedUrl = ImageService.GetImageUrl(imageUrlStorageKey);
            Debug.Log($"[CachedRemoteImageController] Found cached URL with key '{imageUrlStorageKey}', loading directly (skip generation)");

            // Mock逻辑分叉：加载缓存URL
            if (_useMockMode)
            {
                StartCoroutine(LoadAndDisplayImageFromUrlMock(cachedUrl, imageUrlStorageKey, spriteCacheKey, (success) =>
                {
                    if (!success)
                    {
                        ImageService.RemoveImageUrl(imageUrlStorageKey);
                        Debug.LogWarning($"[CachedRemoteImageController] Cached URL is invalid, removed mapping for key '{imageUrlStorageKey}'. Will regenerate on next launch.");
                    }
                }));
            }
            else
            {
                StartCoroutine(LoadAndDisplayImageFromUrl(cachedUrl, imageUrlStorageKey, spriteCacheKey, (success) =>
                {
                    if (!success)
                    {
                        ImageService.RemoveImageUrl(imageUrlStorageKey);
                        Debug.LogWarning($"[CachedRemoteImageController] Cached URL is invalid, removed mapping for key '{imageUrlStorageKey}'. Will regenerate on next launch.");
                    }
                }));
            }
            return;
        }

        // 没有URL映射，需要生成新图片
        Debug.Log($"[CachedRemoteImageController] No cached URL found, starting image generation with prompt: \n{prompt}");

        // Mock逻辑分叉：生成图片
        if (_useMockMode)
        {
            StartCoroutine(GenerateAndDisplayImageMock(imageUrlStorageKey, spriteCacheKey));
        }
        else
        {
            var configs = new List<ImageGenerationConfig>
            {
                new() { prompt = prompt, model = _modelName, width = _imageWidth, height = _imageHeight, num_inference_steps = _numInferenceSteps}
            };

            StartCoroutine(GenerateAndDisplayImage(configs, imageUrlStorageKey, spriteCacheKey));
        }
    }

    /// <summary>
    /// 协调函数：生成图片并显示
    /// 调用 GenerateImage 生成图片，然后在回调中调用 LoadAndDisplayImage 显示图片
    /// </summary>
    /// <param name="configs">图片生成配置列表，包含提示词、模型、尺寸等参数</param>
    /// <param name="imageUrlStorageKey">图片URL存储键</param>
    /// <param name="spriteCacheKey">精灵缓存键</param>
    protected IEnumerator GenerateAndDisplayImage(List<ImageGenerationConfig> configs, string imageUrlStorageKey, string spriteCacheKey)
    {
        // Early return: 检查参数
        if (configs == null || configs.Count == 0)
        {
            Debug.LogError("[CachedRemoteImageController] Configs is null or empty");
            yield break;
        }

        yield return GenerateImage(
            configs,
            (generateResult) =>
            {
                // Early return: 检查生成结果
                if (generateResult == null || generateResult.images.Count == 0)
                {
                    Debug.LogWarning("[CachedRemoteImageController] No images generated in callback");
                    return;
                }

                // 成功路径：加载并显示第一张图片
                StartCoroutine(LoadAndDisplayImage(generateResult.images[0], imageUrlStorageKey, spriteCacheKey));
            }
        );
    }

    /// <summary>
    /// 调用图片生成API并返回生成结果
    /// </summary>
    /// <param name="configs">图片生成配置列表，包含提示词、模型名称、图片尺寸、推理步数等参数</param>
    /// <param name="onComplete">生成完成后的回调函数，接收生成结果</param>
    protected IEnumerator GenerateImage(
        List<ImageGenerationConfig> configs,
        System.Action<ImageGenerationResponse> onComplete)
    {
        yield return _generateImageApi.Call(ImageService.GenerateImageApiUrl, configs);

        // Early return: 先检查 ReqResult 是否为 null
        if (_generateImageApi.ReqResult == null)
        {
            Debug.LogError("[CachedRemoteImageController] GenerateImageApi request result is null");
            onComplete?.Invoke(null);
            yield break;
        }

        // Early return: 检查请求是否成功
        if (!_generateImageApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[CachedRemoteImageController] GenerateImageApi call failed: {_generateImageApi.ReqResult.responseText}");
            onComplete?.Invoke(null);
            yield break;
        }

        // Early return: 检查响应数据
        if (_generateImageApi.RespData == null)
        {
            Debug.LogError("[CachedRemoteImageController] GenerateImageApi response data is null");
            onComplete?.Invoke(null);
            yield break;
        }

        // 成功路径
        Debug.Log($"[CachedRemoteImageController] Generated {_generateImageApi.RespData.images.Count} images, elapsed time: {_generateImageApi.RespData.elapsed_time}s");
        foreach (var img in _generateImageApi.RespData.images)
        {
            Debug.Log($"[CachedRemoteImageController] Image: {img.filename}, URL: {img.url}, Prompt: {img.prompt}, Model: {img.model}");
        }

        onComplete?.Invoke(_generateImageApi.RespData);
    }

    /// <summary>
    /// 根据生成结果加载并显示图片
    /// </summary>
    /// <param name="imageInfo">图片信息对象</param>
    /// <param name="imageUrlStorageKey">图片URL存储键</param>
    /// <param name="spriteCacheKey">精灵缓存键</param>
    protected IEnumerator LoadAndDisplayImage(GeneratedImage imageInfo, string imageUrlStorageKey, string spriteCacheKey)
    {
        // Early return: 检查参数
        if (imageInfo == null)
        {
            Debug.LogError("[CachedRemoteImageController] ImageInfo is null");
            yield break;
        }

        Debug.Log($"[CachedRemoteImageController] Loading image from: {imageInfo.url}");

        // 加载图片纹理
        yield return _textureLoader.LoadTexture(
            ApiEndpointsManager.ImageApiBaseUrl.TrimEnd('/') + imageInfo.url
        );

        // Early return: 检查加载结果
        if (_textureLoader.Result == null || !_textureLoader.Result.IsSuccess)
        {
            Debug.LogError($"[CachedRemoteImageController] Failed to load image: {_textureLoader.Result?.Error}");
            yield break;
        }

        // 保存URL映射到持久化存储
        ImageService.SetImageUrl(imageUrlStorageKey, imageInfo.url);
        Debug.Log($"[CachedRemoteImageController] Saved URL mapping for key '{imageUrlStorageKey}'");

        // 通过 SpriteManager 创建、缓存并显示 Sprite
        var texture = _textureLoader.LoadedTexture;
        if (texture == null)
        {
            Debug.LogError($"[CachedRemoteImageController] Texture is null despite successful load result");
            yield break;
        }

        var sprite = SpriteCacheManager.Instance.AddSprite(spriteCacheKey, texture);
        if (sprite == null)
        {
            Debug.LogError($"[CachedRemoteImageController] Failed to create sprite from texture");
            yield break;
        }

        // 成功路径
        _targetImage.sprite = sprite;
        Debug.Log($"[CachedRemoteImageController] Image displayed and cached with key '{spriteCacheKey}': {texture.width}x{texture.height}");
    }

    /// <summary>
    /// 直接从URL加载并显示图片
    /// 用于加载已知URL的图片，跳过生成步骤
    /// </summary>
    /// <param name="imageUrl">图片URL（相对路径）</param>
    /// <param name="imageUrlStorageKey">图片URL存储键，用于失败时删除映射</param>
    /// <param name="spriteCacheKey">精灵缓存键</param>
    /// <param name="onComplete">加载完成回调，参数为加载是否成功</param>
    protected IEnumerator LoadAndDisplayImageFromUrl(string imageUrl, string imageUrlStorageKey, string spriteCacheKey, System.Action<bool> onComplete)
    {
        Debug.Log($"[CachedRemoteImageController] Loading image from cached URL: {imageUrl}");

        // 加载图片纹理
        yield return _textureLoader.LoadTexture(
            ApiEndpointsManager.ImageApiBaseUrl.TrimEnd('/') + imageUrl
        );

        // Early return: 检查加载结果
        if (_textureLoader.Result == null || !_textureLoader.Result.IsSuccess)
        {
            Debug.LogError($"[CachedRemoteImageController] Failed to load image from cached URL: {_textureLoader.Result?.Error}");
            onComplete?.Invoke(false);
            yield break;
        }

        // 通过 SpriteManager 创建、缓存并显示 Sprite
        var texture = _textureLoader.LoadedTexture;
        if (texture == null)
        {
            Debug.LogError($"[CachedRemoteImageController] Texture is null despite successful load result");
            yield break;
        }

        var sprite = SpriteCacheManager.Instance.AddSprite(spriteCacheKey, texture);
        if (sprite == null)
        {
            Debug.LogError($"[CachedRemoteImageController] Failed to create sprite from texture");
            yield break;
        }

        // 成功路径
        _targetImage.sprite = sprite;
        Debug.Log($"[CachedRemoteImageController] Image loaded from cached URL and displayed: {texture.width}x{texture.height}");
        onComplete?.Invoke(true);
    }

    /// <summary>
    /// Mock实现：模拟从缓存URL加载图片
    /// 用于测试缓存URL加载流程，使用mock URL替代实际URL
    /// </summary>
    /// <param name="originalUrl">原始URL（仅用于日志记录）</param>
    /// <param name="imageUrlStorageKey">图片URL存储键，用于失败时删除映射</param>
    /// <param name="spriteCacheKey">精灵缓存键</param>
    /// <param name="onComplete">加载完成回调，参数为加载是否成功</param>
    protected IEnumerator LoadAndDisplayImageFromUrlMock(string originalUrl, string imageUrlStorageKey, string spriteCacheKey, System.Action<bool> onComplete)
    {
        Debug.Log($"[CachedRemoteImageController] 🔧 Mock模式：模拟从缓存URL加载");
        Debug.Log($"[CachedRemoteImageController] 🔧 原始URL: {originalUrl}");
        Debug.Log($"[CachedRemoteImageController] 🔧 使用Mock URL: {_mockImageUrl}");

        // 模拟网络延迟
        yield return new WaitForSeconds(_mockGenerationDelay);

        Debug.Log($"[CachedRemoteImageController] ✅ Mock加载完成");

        // 使用mock URL加载
        yield return LoadAndDisplayImageFromUrl(_mockImageUrl, imageUrlStorageKey, spriteCacheKey, onComplete);
    }

    /// <summary>
    /// Mock实现：模拟图片生成并显示
    /// 用于测试图片加载和显示功能，跳过实际的AI生成API调用
    /// </summary>
    /// <param name="imageUrlStorageKey">图片URL存储键</param>
    /// <param name="spriteCacheKey">精灵缓存键</param>
    protected IEnumerator GenerateAndDisplayImageMock(string imageUrlStorageKey, string spriteCacheKey)
    {
        Debug.Log($"[CachedRemoteImageController] 🔧 Mock模式启动，模拟生成延迟: {_mockGenerationDelay}秒");

        // 模拟生成等待
        yield return new WaitForSeconds(_mockGenerationDelay);

        Debug.Log($"[CachedRemoteImageController] ✅ Mock生成完成，使用URL: {_mockImageUrl}");

        // 创建模拟的图片信息对象
        var mockImageInfo = new GeneratedImage
        {
            url = _mockImageUrl,
            filename = System.IO.Path.GetFileName(_mockImageUrl),
            prompt = "Mock test image",
            model = "nano-banana"
        };

        // 加载并显示图片
        yield return LoadAndDisplayImage(mockImageInfo, imageUrlStorageKey, spriteCacheKey);
    }
}
