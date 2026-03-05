using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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
        InitializeApiEndpoints().Forget();
    }

    /// <summary>
    /// 新增：点击生成按钮 - 单张图片生成
    /// </summary>
    public void OnClickGenerateAndDownload()
    {
        TestGenerateAndDisplayImage().Forget();
    }

    /// <summary>
    /// 异步初始化API端点配置
    /// 从指定的基础URL获取根API配置，成功后激活登录按钮
    /// </summary>
    private async UniTaskVoid InitializeApiEndpoints()
    {
        await _rootApi.Call(_baseUrl);

        if (_rootApi.ReqResult == null)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_baseUrl}: request result is null");
            return;
        }

        if (!_rootApi.ReqResult.isSuccess)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_baseUrl}: {_rootApi.ReqResult.responseText}");
            return;
        }

        Debug.Assert(_rootApi.RespData != null, "ImageRootApi response data is null");

        // 设置ImageServerContext的基础URL
        ImageService.BaseUrl = _baseUrl;
    }

    /// <summary>
    /// 协调函数：生成图片并显示
    /// </summary>
    private async UniTaskVoid TestGenerateAndDisplayImage()
    {
        var generateResult = await GenerateImage(
            _prompt,
            _modelName,
            _imageWidth,
            _imageHeight,
            _numInferenceSteps
        );

        if (generateResult != null && generateResult.images.Count > 0)
        {
            await LoadAndDisplayImage(generateResult.images[0]);
        }
        else
        {
            Debug.LogWarning("[TestImageServerScene] No images generated");
        }
    }

    /// <summary>
    /// 第一步：调用图片生成API并返回生成结果
    /// </summary>
    private async UniTask<ImageGenerationResponse> GenerateImage(
        string prompt,
        string modelName,
        int width,
        int height,
        int numInferenceSteps)
    {
        var configs = new List<ImageGenerationConfig>
        {
            new() { prompt = prompt, model = modelName, width = width, height = height, num_inference_steps = numInferenceSteps}
        };

        await _generateImageApi.Call(ImageService.GenerateImageApiUrl, configs);

        if (_generateImageApi.ReqResult == null)
        {
            Debug.LogError("GenerateImageApi request result is null");
            return null;
        }

        if (!_generateImageApi.ReqResult.isSuccess)
        {
            Debug.LogError($"GenerateImageApi call failed: {_generateImageApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_generateImageApi.RespData != null, "GenerateImageApi response data is null");

        Debug.Log($"Generated {_generateImageApi.RespData.images.Count} images, elapsed time: {_generateImageApi.RespData.elapsed_time}s");
        foreach (var img in _generateImageApi.RespData.images)
        {
            Debug.Log($"Image: {img.filename}, URL: {img.url}, Prompt: {img.prompt}, Model: {img.model}");
        }

        return _generateImageApi.RespData;
    }

    /// <summary>
    /// 第二步：根据生成结果加载并显示图片
    /// </summary>
    /// <param name="imageInfo">图片信息对象</param>
    private async UniTask LoadAndDisplayImage(GeneratedImage imageInfo)
    {
        if (imageInfo == null)
        {
            Debug.LogError("[TestImageServerScene] ImageInfo is null");
            return;
        }

        Debug.Log($"[TestImageServerScene] Loading image from: {imageInfo.url}");

        // 加载图片纹理
        await _textureLoader.LoadTexture(
            ImageService.BaseUrl.TrimEnd('/') + imageInfo.url
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

