using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerInfoBar : MonoBehaviour
{

    [Header("UI Components")]
    [SerializeField] private Button _headIconButton;
    [SerializeField] private TMP_Text _playerInfoText;

    [Header("API Components")]
    [SerializeField] private GenerateImageApi _generateImageApi;
    [SerializeField] private TextureLoader _textureLoader;

    // 头像点击回调
    public event Action OnHeadIconClickedCallback;

    void Start()
    {
        Debug.Assert(_headIconButton != null, "_headIconButton is null");
        Debug.Assert(_playerInfoText != null, "_playerInfoText is null");
        Debug.Assert(_generateImageApi != null, "_generateImageApi is null");
        Debug.Assert(_textureLoader != null, "_textureLoader is null");

        // 先清除！
        _playerInfoText.text = "";

        // 后刷新！
        RefreshPlayerInfo();

        // 测试
        if (ApiEndpointsManager.ImageRootResponse != null)
        {
            StartCoroutine(GenerateAndDisplayImage());
        }
    }

    void Update()
    {

    }

    private void RefreshPlayerInfo()
    {
        // 设置图片
        if (ApiEndpointsManager.ImageRootResponse == null)
        {
            var playerActor = GameContext.Instance.GetActorEntitySerialization(GameContext.Instance.ActorName);
            Debug.Assert(playerActor != null, "Player actor entity serialization is null for actor name: " + GameContext.Instance.ActorName);
            if (playerActor != null)
            {
                var actorSprite = SpriteManager.Instance.GetSprite(playerActor.name);
                Debug.Assert(actorSprite != null, "Player actor sprite is null for entity: " + playerActor.name);
                var buttonImage = _headIconButton.GetComponent<Image>();
                buttonImage.sprite = actorSprite;
            }
        }

        // 设置文本
        var playerName = GameContext.Instance.UserName;
        var actorName = GameContext.Instance.ActorName;
        _playerInfoText.text = $"{playerName}\n{GameUtils.GetDisplayName(actorName)}";
    }

    public void OnHeadIconClicked()
    {
        Debug.Log("Head icon clicked!");
        OnHeadIconClickedCallback?.Invoke();
    }

    /// <summary>
    /// 协调函数：生成图片并显示
    /// 调用 GenerateImage 生成图片，然后在回调中调用 LoadAndDisplayImage 显示图片
    /// </summary>
    private IEnumerator GenerateAndDisplayImage()
    {
        var playerActor = GameContext.Instance.GetActorEntitySerialization(GameContext.Instance.ActorName);
        var appearanceComponent = GameUtils.GetComponent<AppearanceComponent>(playerActor);
        Debug.Assert(appearanceComponent != null, "AppearanceComponent is null for player actor: " + playerActor.name);

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

        yield return _generateImageApi.Call(ImageServiceContext.Instance.GenerateImageApiUrl, configs);

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

        var buttonImage = _headIconButton.GetComponent<Image>();

        if (_textureLoader.Result != null && _textureLoader.Result.IsSuccess)
        {
            // 销毁旧的 Sprite 防止内存泄漏
            if (buttonImage.sprite != null)
            {
                DestroyImmediate(buttonImage.sprite, true);
            }

            // 创建新的 Sprite 并显示
            var texture = _textureLoader.LoadedTexture;
            buttonImage.sprite = Sprite.Create(
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
