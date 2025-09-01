using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SpriteScene : MonoBehaviour
{
    [SerializeField] private Image targetImage;   // 预置的占位图已在此
    [SerializeField] private string imageUrl;     // 远程图片URL

    [Header("按钮控制")]
    public Button _startButton;
    public Button _generateSingleButton; // 图片生成按钮
    public Button _generateBatchButton; // 图片生成按钮

    [Header("Action 组件")]
    public LoadTextureAction _loadTextureAction; // 纹理加载组件
    public GenerateImageAction _generateImageAction; // 图片生成组件

    [Header("图片生成配置")]
    [SerializeField] private string defaultPrompt = "a beautiful landscape"; // 默认提示词
    [SerializeField] private List<string> batchPrompts = new List<string>(); // 批量生成提示词
    [SerializeField] private string customModelName = "ideogram-v3-turbo"; // 本页使用的自定义模型

    void Start()
    {
        Debug.Assert(targetImage != null, "targetImage is null");
        Debug.Assert(!string.IsNullOrEmpty(imageUrl), "imageUrl is empty");
        Debug.Assert(_startButton != null, "_startButton is null");
        Debug.Assert(_generateSingleButton != null, "_generateSingleButton is null");
        Debug.Assert(_generateBatchButton != null, "_generateBatchButton is null");
        Debug.Assert(_loadTextureAction != null, "_loadTextureAction is null");
        Debug.Assert(_generateImageAction != null, "_generateImageAction is null");

        // 初始化批量提示词（如果为空）
        if (batchPrompts.Count == 0)
        {
            batchPrompts.AddRange(new string[] {
                "a beautiful sunset over mountains",
                "a cute cat playing with a ball",
                "a futuristic city skyline"
            });
        }
    }

    public void OnClickLoad()
    {
        StartCoroutine(LoadTextureAndApplyToImage());
    }

    public void OnClickTest()
    {
        Debug.Log("Test button clicked - no action assigned");
    }

    /// <summary>
    /// 新增：点击生成按钮 - 单张图片生成
    /// </summary>
    public void OnClickGenerateSingle()
    {
        StartCoroutine(GenerateSingleImageAndApply());
    }

    /// <summary>
    /// 新增：点击生成按钮 - 批量图片生成（选择第一张）
    /// </summary>
    public void OnClickGenerateBatch()
    {
        StartCoroutine(GenerateBatchImageAndApply());
    }

    private IEnumerator LoadTextureAndApplyToImage()
    {
        // 使用 LoadTextureAction 加载纹理
        yield return StartCoroutine(_loadTextureAction.LoadAndApply(imageUrl));

        // 检查是否加载成功
        if (_loadTextureAction.HasTexture())
        {
            // 创建 Sprite 并应用到 Image
            var sprite = _loadTextureAction.CreateSpriteFromCurrentTexture(100f);
            if (sprite != null)
            {
                targetImage.sprite = sprite;
                Debug.Log($"Image applied successfully: {_loadTextureAction.GetTextureInfo()}");
            }
            else
            {
                Debug.LogError("Failed to create sprite from loaded texture");
            }
        }
        else
        {
            Debug.LogWarning("Texture loading failed, keeping placeholder image");
        }
    }

    /// <summary>
    /// 生成单张图片并应用到UI
    /// 流程：GenerateImageAction生成 -> LoadTextureAction下载 -> 应用到Image
    /// </summary>
    private IEnumerator GenerateSingleImageAndApply()
    {
        Debug.Log($"开始生成单张图片: {defaultPrompt}，使用模型: {customModelName}");

        // 创建自定义请求，指定模型
        var request = new GenerateImageRequest(defaultPrompt)
        {
            model_name = customModelName // 使用本页指定的模型而不是默认的 sdxl-lightning
        };

        // 第一步：使用 GenerateImageAction 生成图片
        GenerateImageResponse generateResult = null;
        yield return StartCoroutine(_generateImageAction.GenerateSingleImageCoroutine(request, (result) =>
        {
            generateResult = result;
        }));

        // 检查生成是否成功
        if (generateResult == null || !generateResult.success)
        {
            Debug.LogError($"图片生成失败: {generateResult?.message}");
            yield break;
        }

        Debug.Log($"图片生成成功: {generateResult.image_url}");

        // 第二步：使用 LoadTextureAction 下载生成的图片
        yield return StartCoroutine(_loadTextureAction.LoadAndApply(generateResult.image_url));

        // 第三步：应用到UI
        if (_loadTextureAction.HasTexture())
        {
            var sprite = _loadTextureAction.CreateSpriteFromCurrentTexture(100f);
            if (sprite != null)
            {
                targetImage.sprite = sprite;
                Debug.Log($"生成的图片已应用到UI: {_loadTextureAction.GetTextureInfo()}");
            }
            else
            {
                Debug.LogError("Failed to create sprite from generated texture");
            }
        }
        else
        {
            Debug.LogWarning("生成的图片下载失败");
        }
    }

    /// <summary>
    /// 批量生成图片并应用第一张到UI
    /// 流程：GenerateImageAction批量生成 -> 选择第一张 -> LoadTextureAction下载 -> 应用到Image
    /// </summary>
    private IEnumerator GenerateBatchImageAndApply()
    {
        if (batchPrompts.Count == 0)
        {
            Debug.LogError("批量提示词列表为空");
            yield break;
        }

        Debug.Log($"开始批量生成图片: {batchPrompts.Count} 张，使用模型: {customModelName}");

        // 创建自定义请求，指定模型
        var request = new GenerateBatchImagesRequest(batchPrompts)
        {
            model_name = customModelName // 使用本页指定的模型而不是默认的 sdxl-lightning
        };

        // 第一步：使用 GenerateImageAction 批量生成图片
        GenerateBatchImagesResponse batchResult = null;
        yield return StartCoroutine(_generateImageAction.GenerateBatchImagesCoroutine(request, (result) =>
        {
            batchResult = result;
        }));

        // 检查生成是否成功
        if (batchResult == null || !batchResult.success || batchResult.images == null || batchResult.images.Count == 0)
        {
            Debug.LogError($"批量图片生成失败: {batchResult?.message}");
            yield break;
        }

        Debug.Log($"批量图片生成成功: {batchResult.total_count} 张");

        // 第二步：随机选择一张图片进行下载
        int randomIndex = Random.Range(0, batchResult.images.Count);
        var randomImage = batchResult.images[randomIndex];
        Debug.Log($"随机选择第 {randomIndex + 1} 张图片进行下载: {randomImage.image_url}");

        // 第三步：使用 LoadTextureAction 下载选中的图片
        yield return StartCoroutine(_loadTextureAction.LoadAndApply(randomImage.image_url));

        // 第四步：应用到UI
        if (_loadTextureAction.HasTexture())
        {
            var sprite = _loadTextureAction.CreateSpriteFromCurrentTexture(100f);
            if (sprite != null)
            {
                targetImage.sprite = sprite;
                Debug.Log($"批量生成的随机图片已应用到UI: {_loadTextureAction.GetTextureInfo()}");
                Debug.Log($"该图片提示词: {randomImage.prompt}");
            }
            else
            {
                Debug.LogError("Failed to create sprite from batch generated texture");
            }
        }
        else
        {
            Debug.LogWarning("批量生成的图片下载失败");
        }
    }

}