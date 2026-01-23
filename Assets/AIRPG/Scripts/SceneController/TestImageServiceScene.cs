using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TestImageServiceScene : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private string _baseUrl = "http://192.168.2.134:8300/";

    [Header("Image Generation Settings")]
    [SerializeField] private string _modelName = "nano-banana";
    [SerializeField] private string _prompt = "可爱的小狗坐在椅子上～";
    [SerializeField] private int _imageWidth = 1080;
    [SerializeField] private int _imageHeight = 1920;
    [SerializeField] private int _numInferenceSteps = 4;

    [Header("API Components")]
    [SerializeField] private RootApi _rootApi;
    [SerializeField] private GenerateImageApi _generateImageApi;
    [SerializeField] private TextureLoader _textureLoader;

    [Header("UI Components")]
    [SerializeField] private Image _imageDisplay;

    void Start()
    {
        Debug.Assert(!string.IsNullOrEmpty(_baseUrl), "_baseImageServerUrl is null");
        Debug.Assert(_rootApi != null, "_rootApi is null");
        Debug.Assert(_generateImageApi != null, "_generateImageApi is null");
        Debug.Assert(_textureLoader != null, "_textureLoader is null");
        Debug.Assert(_imageDisplay != null, "_imageDisplay is null");

        // 初始化API端点
        StartCoroutine(InitializeApiEndpoints());
    }

    /// <summary>
    /// 新增：点击生成按钮 - 单张图片生成
    /// </summary>
    public void OnClickGenerateAndDownload()
    {
        StartCoroutine(TestGenerateAndDisplayImage());
    }

    /// <summary>
    /// 异步初始化API端点配置
    /// 从指定的基础URL获取根API配置，成功后激活登录按钮
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator InitializeApiEndpoints()
    {
        yield return _rootApi.Call(_baseUrl);

        if (_rootApi.ReqResult == null)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_baseUrl}: request result is null");
            yield break;
        }

        if (!_rootApi.ReqResult.isSuccess)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_baseUrl}: {_rootApi.ReqResult.responseText}");
            yield break;
        }

        Debug.Assert(_rootApi.RespData != null, "ImageRootApi response data is null");

        // 设置ImageServerContext的基础URL
        ApiEndpointsManager.ImageApiBaseUrl = _baseUrl;
    }

    /// <summary>
    /// 协调函数：生成图片并显示
    /// 调用 GenerateImage 生成图片，然后在回调中调用 LoadAndDisplayImage 显示图片
    /// </summary>
    private IEnumerator TestGenerateAndDisplayImage()
    {
        yield return GenerateImage(
            _prompt,
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
                    Debug.LogWarning("[TestImageServerScene] No images generated in callback");
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

        if (_generateImageApi.ReqResult == null)
        {
            Debug.LogError("GenerateImageApi request result is null");
            onComplete?.Invoke(null);
            yield break;
        }

        if (!_generateImageApi.ReqResult.isSuccess)
        {
            Debug.LogError($"GenerateImageApi call failed: {_generateImageApi.ReqResult.responseText}");
            onComplete?.Invoke(null);
            yield break;
        }

        Debug.Assert(_generateImageApi.RespData != null, "GenerateImageApi response data is null");

        Debug.Log($"Generated {_generateImageApi.RespData.images.Count} images, elapsed time: {_generateImageApi.RespData.elapsed_time}s");
        foreach (var img in _generateImageApi.RespData.images)
        {
            Debug.Log($"Image: {img.filename}, URL: {img.url}, Prompt: {img.prompt}, Model: {img.model}");
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
            Debug.LogError("[TestImageServerScene] ImageInfo is null");
            yield break;
        }

        Debug.Log($"[TestImageServerScene] Loading image from: {imageInfo.url}");

        // 加载图片纹理
        yield return _textureLoader.LoadTexture(
            ApiEndpointsManager.ImageApiBaseUrl.TrimEnd('/') + imageInfo.url
        );

        if (_textureLoader.Result != null && _textureLoader.Result.IsSuccess)
        {
            // 销毁旧的 Sprite 防止内存泄漏
            if (_imageDisplay.sprite != null)
            {
                DestroyImmediate(_imageDisplay.sprite, true);
            }

            // 创建新的 Sprite 并显示
            var texture = _textureLoader.LoadedTexture;
            _imageDisplay.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            Debug.Log($"[TestImageServerScene] Image displayed: {texture.width}x{texture.height}");
        }
        else
        {
            Debug.LogError($"[TestImageServerScene] Failed to load image: {_textureLoader.Result?.Error}");
        }
    }
}

