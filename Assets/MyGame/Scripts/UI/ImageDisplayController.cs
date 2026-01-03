using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/* 测试数据记录
30 - ✅ 图片生成完成! 总耗时: 10.64秒, 平均: 10.64秒/张
INFO:     192.168.2.134:56001 - "POST /api/generate/v1 HTTP/1.1" 200 OK
INFO:     192.168.2.134:56001 - "GET /images/nano-banana_6d2db373-3e7f-41cb-88b7-d63a81f18255.png HTTP/1.1" 200 OK
*/

[RequireComponent(typeof(Image))]

/// <summary>
/// 图片显示控制器
/// 用于生成AI图片并显示在Image组件上
/// 需要挂载在包含Image组件的GameObject上
/// </summary>
public class ImageDisplayController : MonoBehaviour
{
    [Header("Actor Settings")]
    public string ActorName { get; set; }

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
    [SerializeField] private bool _useMockMode = true;
    [SerializeField] private float _mockGenerationDelay = 2.0f;
    [SerializeField] private string _mockImageUrl = "/images/nano-banana_fb300c55-3130-4ac2-9e9e-19ee8da8f3e1.png";

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

        // 保险用，后续只用 SpriteManager 来管理默认图标
        if (_targetImage.sprite != null)
        {
            Debug.LogWarning("[ImageDisplayController] Target Image already has a sprite assigned, it will be replaced.");
            DestroyImmediate(_targetImage.sprite, true);
            _targetImage.sprite = null;
        }
    }

    void Start()
    {
        // 检查是否已有缓存的Sprite, 有就直接显示
        var cachedSprite = SpriteManager.Instance.GetSprite(ActorName);
        if (cachedSprite != null)
        {
            Debug.Log($"[ImageDisplayController] Found cached sprite for actor '{ActorName}', displaying it directly.");
            _targetImage.sprite = cachedSprite;
            return;
        }

        // 显示默认图标，因为后面会等待图片生成完成后替换
        var defaultActorIcon = SpriteManager.Instance.GetSprite(SpriteManager.DefaultIconKey);
        _targetImage.sprite = defaultActorIcon;

        // 测试生成图片，有可能图片生成服务器是不开的，所以要加个判断
        if (ApiEndpointsManager.ImageRootResponse != null)
        {
            // 这里临时写死就是用玩家角色的外观作为提示词
            var playerActor = GameContext.Instance.GetActorEntitySerialization(ActorName);
            var appearanceComponent = GameUtils.GetComponent<AppearanceComponent>(playerActor);
            Debug.Assert(appearanceComponent != null, "[ImageDisplayController] AppearanceComponent is null for player actor: " + playerActor.name);

            // 生成提示词，拿玩家角色的外观描述
            var prompt = appearanceComponent.appearance;
            Debug.Log($"[ImageDisplayController] Starting image generation with prompt: \n{prompt}");
            StartCoroutine(GenerateAndDisplayImage(prompt));
        }
        else
        {
            //不会替换了，因为服务器没开
            Debug.LogWarning("[ImageDisplayController] Image generation API endpoint is not configured, skipping image generation test");
        }
    }

    /// <summary>
    /// 协调函数：生成图片并显示
    /// 调用 GenerateImage 生成图片，然后在回调中调用 LoadAndDisplayImage 显示图片
    /// </summary>
    /// <param name="prompt">生成图片的提示词</param>
    private IEnumerator GenerateAndDisplayImage(string prompt)
    {
        // Mock模式：跳过API调用，直接使用模拟URL
        _useMockMode = true;
        if (_useMockMode)
        {
            yield return GenerateAndDisplayImageMock();
            yield break;
        }

        yield return GenerateImage(
            prompt,
            _modelName,
            _imageWidth,
            _imageHeight,
            _numInferenceSteps,
            (generateResult) =>
            {
                // 图片生成完成后的回调
                if (generateResult != null && generateResult.images.Count > 0)
                {
                    // 加载并显示第一张图片
                    StartCoroutine(LoadAndDisplayImage(generateResult.images[0]));
                }
                else
                {
                    Debug.LogWarning("[ImageDisplayController] No images generated in callback");
                }
            }
        );
    }

    /// <summary>
    /// 第一步：调用图片生成API并返回生成结果
    /// </summary>
    /// <param name="prompt">提示词</param>
    /// <param name="modelName">模型名称</param>
    /// <param name="width">图片宽度</param>
    /// <param name="height">图片高度</param>
    /// <param name="numInferenceSteps">推理步数</param>
    /// <param name="onComplete">生成完成后的回调函数，接收生成结果</param>
    private IEnumerator GenerateImage(
        string prompt,
        string modelName,
        int width,
        int height,
        int numInferenceSteps,
        System.Action<ImageGenerationResponse> onComplete)
    {
        var configs = new List<ImageGenerationConfig>
        {
            new() { prompt = prompt, model = modelName, width = width, height = height, num_inference_steps = numInferenceSteps}
        };

        yield return _generateImageApi.Call(ImageService.GenerateImageApiUrl, configs);

        if (!_generateImageApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[ImageDisplayController] GenerateImageApi call failed: {_generateImageApi.ReqResult.responseText}");
            onComplete?.Invoke(null);
            yield break;
        }

        if (_generateImageApi.ReqResult == null)
        {
            Debug.LogError("[ImageDisplayController] GenerateImageApi request result is null");
            onComplete?.Invoke(null);
            yield break;
        }


        Debug.Assert(_generateImageApi.RespData != null, "[ImageDisplayController] GenerateImageApi response data is null");

        Debug.Log($"[ImageDisplayController] Generated {_generateImageApi.RespData.images.Count} images, elapsed time: {_generateImageApi.RespData.elapsed_time}s");
        foreach (var img in _generateImageApi.RespData.images)
        {
            Debug.Log($"[ImageDisplayController] Image: {img.filename}, URL: {img.url}, Prompt: {img.prompt}, Model: {img.model}");
        }

        // 调用回调函数，传递生成结果
        onComplete?.Invoke(_generateImageApi.RespData);
    }

    /// <summary>
    /// 第二步：根据生成结果加载并显示图片
    /// </summary>
    /// <param name="imageInfo">图片信息对象</param>
    private IEnumerator LoadAndDisplayImage(GeneratedImage imageInfo)
    {
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

        if (_textureLoader.Result.IsSuccess && _textureLoader.Result != null)
        {
            // 通过 SpriteManager 创建、缓存并显示 Sprite
            var texture = _textureLoader.LoadedTexture;
            string spriteKey = ActorName;

            var sprite = SpriteManager.Instance.AddSprite(spriteKey, texture);
            if (sprite != null)
            {
                _targetImage.sprite = sprite;
                Debug.Log($"[ImageDisplayController] Image displayed and cached with key '{spriteKey}': {texture.width}x{texture.height}");
            }
            else
            {
                Debug.LogError($"[ImageDisplayController] Failed to create sprite from texture");
            }
        }
        else
        {
            Debug.LogError($"[ImageDisplayController] Failed to load image: {_textureLoader.Result?.Error}");
        }
    }

    /// <summary>
    /// Mock实现：模拟图片生成并显示
    /// 用于测试图片加载和显示功能，跳过实际的AI生成API调用
    /// </summary>
    private IEnumerator GenerateAndDisplayImageMock()
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
        yield return LoadAndDisplayImage(mockImageInfo);
    }
}

