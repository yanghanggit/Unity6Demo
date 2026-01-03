using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 图片显示控制器
/// 用于生成AI图片并显示在Image组件上
/// 需要挂载在包含Image组件的GameObject上
/// </summary>
[RequireComponent(typeof(Image))]
public class ActorPortraitController : MonoBehaviour
{
    [Header("Actor Settings")]
    public string EntityName { get; set; }
    public string Prompt { get; set; }
    public string ImageUrlStorageKey { get; set; }
    public string SpriteCacheKey { get; set; }

    [Header("Target Components")]
    [SerializeField] private Image _targetImage;

    [Header("API Components")]
    [SerializeField] private GenerateImageApi _generateImageApi;
    [SerializeField] private TextureLoader _textureLoader;

    [Header("Generation Settings")]
    [SerializeField] private string _modelName = "nano-banana";
    [SerializeField] private int _imageWidth = 512;
    [SerializeField] private int _imageHeight = 512;
    [SerializeField] private int _numInferenceSteps = 4;

    [Header("Test Settings")]
    [SerializeField] private bool _useMockMode = false;
    [SerializeField] private float _mockGenerationDelay = 2.0f;
    [SerializeField] private string _mockImageUrl = "/images/nano-banana_fb300c55-3130-4ac2-9e9e-19ee8da8f3e1.png"; //服务器上的一张测试图片，确保有，需要服务器开发确认！目前是有的。


    void Awake()
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
        Debug.Assert(_targetImage != null, "[ImageDisplayController] Image component is null on GameObject: " + gameObject.name);
        Debug.Assert(_targetImage.sprite == null, "[ImageDisplayController] Image component already has a sprite assigned on GameObject: " + gameObject.name); //这里必须要删除，否则后续会出现内存错误。
        Debug.Assert(_generateImageApi != null, "[ImageDisplayController] GenerateImageApi is null");
        Debug.Assert(_textureLoader != null, "[ImageDisplayController] TextureLoader is null");

        // 保险用，后续只用 SpriteManager 来管理默认图标，如果有残留的图标（例如是编辑器场景测试时留下的），则删除它。
        // 否则使用 spriteManager 做生命周期管理时会出问题。
        if (_targetImage.sprite != null)
        {
            Debug.LogWarning("[ImageDisplayController] Target Image already has a sprite assigned, it will be replaced.");
            DestroyImmediate(_targetImage.sprite, true);
            _targetImage.sprite = null;
        }
    }

    void Start()
    {
        Debug.Assert(!string.IsNullOrEmpty(EntityName), "[ImageDisplayController] ActorName is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(Prompt), "[ImageDisplayController] Prompt is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(ImageUrlStorageKey), "[ImageDisplayController] ImageUrlStorageKey is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(SpriteCacheKey), "[ImageDisplayController] SpriteCacheKey is null or empty");
        Debug.Assert(ImageUrlStorageKey == SpriteCacheKey, "[ImageDisplayController] For simplicity, ImageUrlStorageKey should equal SpriteCacheKey in current implementation");

        // 缓存两个key到局部变量，后续调用链通过参数传递
        string imageUrlStorageKey = ImageUrlStorageKey;
        string spriteCacheKey = SpriteCacheKey;

        // 检查是否已有缓存的Sprite, 有就直接显示
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(spriteCacheKey);
        if (cachedSprite != null)
        {
            Debug.Log($"[ImageDisplayController] Found cached sprite with key '{spriteCacheKey}', displaying it directly.");
            _targetImage.sprite = cachedSprite;
            return;
        }

        // 未命中内存缓存，立即显示默认图标（后续需要网络操作）
        var defaultActorIcon = SpriteCacheManager.Instance.GetSprite(SpriteCacheManager.DefaultIconKey);
        _targetImage.sprite = defaultActorIcon;

        // 检查图片生成服务器是否开启
        if (ApiEndpointsManager.ImageRootResponse == null)
        {
            Debug.LogWarning("[ImageDisplayController] Image generation API endpoint is not configured, skipping all network operations");
            return;
        }

        // 获取提示词
        //var genPrompt = GetPrompt();

        // 检查是否有持久化的URL映射
        if (ImageService.HasImageUrl(imageUrlStorageKey))
        {
            string cachedUrl = ImageService.GetImageUrl(imageUrlStorageKey);
            Debug.Log($"[ImageDisplayController] Found cached URL with key '{imageUrlStorageKey}', loading directly (skip generation)");

            // Mock逻辑分叉：加载缓存URL
            if (_useMockMode)
            {
                StartCoroutine(LoadAndDisplayImageFromUrlMock(cachedUrl, imageUrlStorageKey, spriteCacheKey, (success) =>
                {
                    if (!success)
                    {
                        ImageService.RemoveImageUrl(imageUrlStorageKey);
                        Debug.LogWarning($"[ImageDisplayController] Cached URL is invalid, removed mapping for key '{imageUrlStorageKey}'. Will regenerate on next launch.");
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
                        Debug.LogWarning($"[ImageDisplayController] Cached URL is invalid, removed mapping for key '{imageUrlStorageKey}'. Will regenerate on next launch.");
                    }
                }));
            }
            return;
        }

        // 没有URL映射，需要生成新图片
        Debug.Log($"[ImageDisplayController] No cached URL found, starting image generation with prompt: \n{Prompt}");

        // Mock逻辑分叉：生成图片
        if (_useMockMode)
        {
            StartCoroutine(GenerateAndDisplayImageMock(imageUrlStorageKey, spriteCacheKey));
        }
        else
        {
            var configs = new List<ImageGenerationConfig>
            {
                new() { prompt = Prompt, model = _modelName, width = _imageWidth, height = _imageHeight, num_inference_steps = _numInferenceSteps}
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
    private IEnumerator GenerateAndDisplayImage(List<ImageGenerationConfig> configs, string imageUrlStorageKey, string spriteCacheKey)
    {
        // Early return: 检查参数
        if (configs == null || configs.Count == 0)
        {
            Debug.LogError("[ImageDisplayController] Configs is null or empty");
            yield break;
        }

        yield return GenerateImage(
            configs,
            (generateResult) =>
            {
                // Early return: 检查生成结果
                if (generateResult == null || generateResult.images.Count == 0)
                {
                    Debug.LogWarning("[ImageDisplayController] No images generated in callback");
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
    private IEnumerator GenerateImage(
        List<ImageGenerationConfig> configs,
        System.Action<ImageGenerationResponse> onComplete)
    {
        yield return _generateImageApi.Call(ImageService.GenerateImageApiUrl, configs);

        // Early return: 先检查 ReqResult 是否为 null
        if (_generateImageApi.ReqResult == null)
        {
            Debug.LogError("[ImageDisplayController] GenerateImageApi request result is null");
            onComplete?.Invoke(null);
            yield break;
        }

        // Early return: 检查请求是否成功
        if (!_generateImageApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[ImageDisplayController] GenerateImageApi call failed: {_generateImageApi.ReqResult.responseText}");
            onComplete?.Invoke(null);
            yield break;
        }

        // Early return: 检查响应数据
        if (_generateImageApi.RespData == null)
        {
            Debug.LogError("[ImageDisplayController] GenerateImageApi response data is null");
            onComplete?.Invoke(null);
            yield break;
        }

        // 成功路径
        Debug.Log($"[ImageDisplayController] Generated {_generateImageApi.RespData.images.Count} images, elapsed time: {_generateImageApi.RespData.elapsed_time}s");
        foreach (var img in _generateImageApi.RespData.images)
        {
            Debug.Log($"[ImageDisplayController] Image: {img.filename}, URL: {img.url}, Prompt: {img.prompt}, Model: {img.model}");
        }

        onComplete?.Invoke(_generateImageApi.RespData);
    }

    /// <summary>
    /// 第二步：根据生成结果加载并显示图片
    /// </summary>
    /// <param name="imageInfo">图片信息对象</param>
    /// <param name="imageUrlStorageKey">图片URL存储键</param>
    /// <param name="spriteCacheKey">精灵缓存键</param>
    private IEnumerator LoadAndDisplayImage(GeneratedImage imageInfo, string imageUrlStorageKey, string spriteCacheKey)
    {
        // Early return: 检查参数
        if (imageInfo == null)
        {
            Debug.LogError("[ImageDisplayController] ImageInfo is null");
            yield break;
        }

        Debug.Log($"[ImageDisplayController] Loading image from: {imageInfo.url}");

        // 加载图片纹理
        yield return _textureLoader.LoadTexture(
            ApiEndpointsManager.ImageApiBaseUrl.TrimEnd('/') + imageInfo.url
        );

        // Early return: 检查加载结果
        if (_textureLoader.Result == null || !_textureLoader.Result.IsSuccess)
        {
            Debug.LogError($"[ImageDisplayController] Failed to load image: {_textureLoader.Result?.Error}");
            yield break;
        }

        // 保存URL映射到持久化存储
        ImageService.SetImageUrl(imageUrlStorageKey, imageInfo.url);
        Debug.Log($"[ImageDisplayController] Saved URL mapping for actor '{imageUrlStorageKey}'");

        // 通过 SpriteManager 创建、缓存并显示 Sprite
        var texture = _textureLoader.LoadedTexture;
        if (texture == null)
        {
            Debug.LogError($"[ImageDisplayController] Texture is null despite successful load result");
            yield break;
        }

        //string spriteKey = ActorName;

        var sprite = SpriteCacheManager.Instance.AddSprite(spriteCacheKey, texture);
        if (sprite == null)
        {
            Debug.LogError($"[ImageDisplayController] Failed to create sprite from texture");
            yield break;
        }

        // 成功路径
        _targetImage.sprite = sprite;
        Debug.Log($"[ImageDisplayController] Image displayed and cached with key '{spriteCacheKey}': {texture.width}x{texture.height}");
    }

    /// <summary>
    /// 直接从URL加载并显示图片
    /// 用于加载已知URL的图片，跳过生成步骤
    /// </summary>
    /// <param name="imageUrl">图片URL（相对路径）</param>
    /// <param name="imageUrlStorageKey">图片URL存储键，用于失败时删除映射</param>
    /// <param name="spriteCacheKey">精灵缓存键</param>
    /// <param name="onComplete">加载完成回调，参数为加载是否成功</param>
    private IEnumerator LoadAndDisplayImageFromUrl(string imageUrl, string imageUrlStorageKey, string spriteCacheKey, System.Action<bool> onComplete)
    {
        Debug.Log($"[ImageDisplayController] Loading image from cached URL: {imageUrl}");

        // 加载图片纹理
        yield return _textureLoader.LoadTexture(
            ApiEndpointsManager.ImageApiBaseUrl.TrimEnd('/') + imageUrl
        );

        // Early return: 检查加载结果
        if (_textureLoader.Result == null || !_textureLoader.Result.IsSuccess)
        {
            Debug.LogError($"[ImageDisplayController] Failed to load image from cached URL: {_textureLoader.Result?.Error}");
            onComplete?.Invoke(false);
            yield break;
        }

        // 通过 SpriteManager 创建、缓存并显示 Sprite
        var texture = _textureLoader.LoadedTexture;
        if (texture == null)
        {
            Debug.LogError($"[ImageDisplayController] Texture is null despite successful load result");
            yield break;
        }

        //string spriteKey = ActorName;

        var sprite = SpriteCacheManager.Instance.AddSprite(spriteCacheKey, texture);
        if (sprite == null)
        {
            Debug.LogError($"[ImageDisplayController] Failed to create sprite from texture");
            yield break;
        }

        // 成功路径
        _targetImage.sprite = sprite;
        Debug.Log($"[ImageDisplayController] Image loaded from cached URL and displayed: {texture.width}x{texture.height}");
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
    private IEnumerator LoadAndDisplayImageFromUrlMock(string originalUrl, string imageUrlStorageKey, string spriteCacheKey, System.Action<bool> onComplete)
    {
        Debug.Log($"[ImageDisplayController] 🔧 Mock模式：模拟从缓存URL加载");
        Debug.Log($"[ImageDisplayController] 🔧 原始URL: {originalUrl}");
        Debug.Log($"[ImageDisplayController] 🔧 使用Mock URL: {_mockImageUrl}");

        // 模拟网络延迟
        yield return new WaitForSeconds(_mockGenerationDelay);

        Debug.Log($"[ImageDisplayController] ✅ Mock加载完成");

        // 使用mock URL加载
        yield return LoadAndDisplayImageFromUrl(_mockImageUrl, imageUrlStorageKey, spriteCacheKey, onComplete);
    }

    /// <summary>
    /// Mock实现：模拟图片生成并显示
    /// 用于测试图片加载和显示功能，跳过实际的AI生成API调用
    /// </summary>
    /// <param name="imageUrlStorageKey">图片URL存储键</param>
    /// <param name="spriteCacheKey">精灵缓存键</param>
    private IEnumerator GenerateAndDisplayImageMock(string imageUrlStorageKey, string spriteCacheKey)
    {
        Debug.Log($"[ImageDisplayController] 🔧 Mock模式启动，模拟生成延迟: {_mockGenerationDelay}秒");

        // 模拟生成等待
        yield return new WaitForSeconds(_mockGenerationDelay);

        Debug.Log($"[ImageDisplayController] ✅ Mock生成完成，使用URL: {_mockImageUrl}");

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



