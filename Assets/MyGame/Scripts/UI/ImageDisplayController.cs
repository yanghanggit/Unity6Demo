using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/*
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
    [Header("Target Components")]
    [SerializeField] private Image _targetImage;

    [Header("API Components")]
    [SerializeField] private GenerateImageApi _generateImageApi;
    [SerializeField] private TextureLoader _textureLoader;

    void Start()
    {
        // 自动获取Image组件
        if (_targetImage == null)
        {
            _targetImage = GetComponent<Image>();
        }

        // 组件检测
        Debug.Assert(_targetImage != null, "[ImageDisplayController] Image component is null on GameObject: " + gameObject.name);
        Debug.Assert(_generateImageApi != null, "[ImageDisplayController] GenerateImageApi is null");
        Debug.Assert(_textureLoader != null, "[ImageDisplayController] TextureLoader is null");

        // 测试生成图片
        if (ApiEndpointsManager.ImageRootResponse != null)
        {
            StartCoroutine(GenerateAndDisplayImage());
        }
    }

    /// <summary>
    /// 协调函数：生成图片并显示
    /// 调用 GenerateImage 生成图片，然后在回调中调用 LoadAndDisplayImage 显示图片
    /// </summary>
    private IEnumerator GenerateAndDisplayImage()
    {
        var playerActor = GameContext.Instance.GetActorEntitySerialization(GameContext.Instance.ActorName);
        var appearanceComponent = GameUtils.GetComponent<AppearanceComponent>(playerActor);
        Debug.Assert(appearanceComponent != null, "[ImageDisplayController] AppearanceComponent is null for player actor: " + playerActor.name);

        var prompt = appearanceComponent.appearance;
        var modelName = "nano-banana";
        var imageWidth = 1024;
        var imageHeight = 1024;
        var numInferenceSteps = 4;

        yield return GenerateImage(
            prompt,
            modelName,
            imageWidth,
            imageHeight,
            numInferenceSteps,
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

        yield return _generateImageApi.Call(ImageServiceContext.Instance.GenerateImageApiUrl, configs);

        if (_generateImageApi.ReqResult == null)
        {
            Debug.LogError("[ImageDisplayController] GenerateImageApi request result is null");
            onComplete?.Invoke(null);
            yield break;
        }

        if (!_generateImageApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[ImageDisplayController] GenerateImageApi call failed: {_generateImageApi.ReqResult.responseText}");
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

        if (_textureLoader.Result != null && _textureLoader.Result.IsSuccess)
        {
            // 销毁旧的 Sprite 防止内存泄漏
            if (_targetImage.sprite != null)
            {
                DestroyImmediate(_targetImage.sprite, true);
            }

            // 创建新的 Sprite 并显示
            var texture = _textureLoader.LoadedTexture;
            _targetImage.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            Debug.Log($"[ImageDisplayController] Image displayed: {texture.width}x{texture.height}");
        }
        else
        {
            Debug.LogError($"[ImageDisplayController] Failed to load image: {_textureLoader.Result?.Error}");
        }
    }
}
