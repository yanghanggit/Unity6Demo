using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TestImageServerScene : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private string _baseImageServerUrl = "http://192.168.2.121:8300/";

    [Header("API Components")]
    [SerializeField] private ImageRootApi _imageRootApi;

    void Start()
    {
        Debug.Assert(_imageRootApi != null, "_imageRootApi is null");
        Debug.Assert(!string.IsNullOrEmpty(_baseImageServerUrl), "_baseImageServerUrl is null");

        StartCoroutine(InitializeApiEndpoints());
    }

    public void OnClickLoad()
    {
        //StartCoroutine(LoadTextureAndApplyToImage());
    }

    /// <summary>
    /// 新增：点击生成按钮 - 单张图片生成
    /// </summary>
    public void OnClickGenerateSingle()
    {
        //StartCoroutine(GenerateSingleImageAndApply());
    }

    /// <summary>
    /// 新增：点击生成按钮 - 批量图片生成（选择第一张）
    /// </summary>
    public void OnClickGenerateBatch()
    {
        //StartCoroutine(GenerateBatchImageAndApply());
    }

    /// <summary>
    /// 异步初始化API端点配置
    /// 从指定的基础URL获取根API配置，成功后激活登录按钮
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator InitializeApiEndpoints()
    {
        yield return _imageRootApi.Call(_baseImageServerUrl);

        if (_imageRootApi.ReqResult == null)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_baseImageServerUrl}: request result is null");
            yield break;
        }

        if (!_imageRootApi.ReqResult.isSuccess)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_baseImageServerUrl}: {_imageRootApi.ReqResult.responseText}");
            yield break;
        }

        Debug.Assert(_imageRootApi.RespData != null, "ImageRootApi response data is null");

        // 设置图片服务根响应数据
        RootResp.SetImageRoot(_imageRootApi.RespData);
    }


    // private IEnumerator LoadTextureAndApplyToImage()
    // {
    //     // 使用 LoadTextureAction 加载纹理
    //     yield return StartCoroutine(_loadTextureAction.LoadAndApply(imageUrl));

    //     // 检查是否加载成功
    //     if (_loadTextureAction.HasTexture())
    //     {
    //         // 创建 Sprite 并应用到 Image
    //         var sprite = _loadTextureAction.CreateSpriteFromCurrentTexture(100f);
    //         if (sprite != null)
    //         {
    //             targetImage.sprite = sprite;
    //             Debug.Log($"Image applied successfully: {_loadTextureAction.GetTextureInfo()}");
    //         }
    //         else
    //         {
    //             Debug.LogError("Failed to create sprite from loaded texture");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("Texture loading failed, keeping placeholder image");
    //     }
    // }

    // /// <summary>
    // /// 生成单张图片并应用到UI
    // /// 流程：GenerateImageAction生成 -> LoadTextureAction下载 -> 应用到Image
    // /// </summary>
    // private IEnumerator GenerateSingleImageAndApply()
    // {
    //     Debug.Log($"开始生成单张图片: {defaultPrompt}，使用模型: {customModelName}");

    //     // 创建自定义请求，指定模型（使用新的数据结构）
    //     var request = new GenerateImagesRequest
    //     {
    //         prompts = new List<string> { defaultPrompt },
    //         model_name = customModelName // 使用本页指定的模型而不是默认的 sdxl-lightning
    //     };

    //     // 第一步：使用 GenerateImageAction 生成图片
    //     GenerateImagesResponse generateResult = null;
    //     yield return StartCoroutine(_generateImageAction.GenerateImagesCoroutine(request, (result) =>
    //     {
    //         generateResult = result;
    //     }));

    //     // 检查生成是否成功
    //     if (generateResult == null || !generateResult.success || generateResult.images == null || generateResult.images.Count == 0)
    //     {
    //         Debug.LogError($"图片生成失败: {generateResult?.message}");
    //         yield break;
    //     }

    //     // 获取第一张生成的图片
    //     var firstImage = generateResult.images[0];
    //     Debug.Log($"图片生成成功: {firstImage.image_url}");

    //     // 第二步：使用 LoadTextureAction 下载生成的图片
    //     yield return StartCoroutine(_loadTextureAction.LoadAndApply(firstImage.image_url));

    //     // 第三步：应用到UI
    //     if (_loadTextureAction.HasTexture())
    //     {
    //         var sprite = _loadTextureAction.CreateSpriteFromCurrentTexture(100f);
    //         if (sprite != null)
    //         {
    //             targetImage.sprite = sprite;
    //             Debug.Log($"生成的图片已应用到UI: {_loadTextureAction.GetTextureInfo()}");
    //         }
    //         else
    //         {
    //             Debug.LogError("Failed to create sprite from generated texture");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("生成的图片下载失败");
    //     }
    // }

    // /// <summary>
    // /// 批量生成图片并应用第一张到UI
    // /// 流程：GenerateImageAction批量生成 -> 选择第一张 -> LoadTextureAction下载 -> 应用到Image
    // /// </summary>
    // private IEnumerator GenerateBatchImageAndApply()
    // {
    //     if (batchPrompts.Count == 0)
    //     {
    //         Debug.LogError("批量提示词列表为空");
    //         yield break;
    //     }

    //     Debug.Log($"开始批量生成图片: {batchPrompts.Count} 张，使用模型: {customModelName}");

    //     // 创建自定义请求，指定模型（使用新的数据结构）
    //     var request = new GenerateImagesRequest
    //     {
    //         prompts = batchPrompts,
    //         model_name = customModelName // 使用本页指定的模型而不是默认的 sdxl-lightning
    //     };

    //     // 第一步：使用 GenerateImageAction 批量生成图片
    //     GenerateImagesResponse batchResult = null;
    //     yield return StartCoroutine(_generateImageAction.GenerateImagesCoroutine(request, (result) =>
    //     {
    //         batchResult = result;
    //     }));

    //     // 检查生成是否成功
    //     if (batchResult == null || !batchResult.success || batchResult.images == null || batchResult.images.Count == 0)
    //     {
    //         Debug.LogError($"批量图片生成失败: {batchResult?.message}");
    //         yield break;
    //     }

    //     Debug.Log($"批量图片生成成功: {batchResult.images.Count} 张");

    //     // 第二步：随机选择一张图片进行下载
    //     int randomIndex = Random.Range(0, batchResult.images.Count);
    //     var randomImage = batchResult.images[randomIndex];
    //     Debug.Log($"随机选择第 {randomIndex + 1} 张图片进行下载: {randomImage.image_url}");

    //     // 第三步：使用 LoadTextureAction 下载选中的图片
    //     yield return StartCoroutine(_loadTextureAction.LoadAndApply(randomImage.image_url));

    //     // 第四步：应用到UI
    //     if (_loadTextureAction.HasTexture())
    //     {
    //         var sprite = _loadTextureAction.CreateSpriteFromCurrentTexture(100f);
    //         if (sprite != null)
    //         {
    //             targetImage.sprite = sprite;
    //             Debug.Log($"批量生成的随机图片已应用到UI: {_loadTextureAction.GetTextureInfo()}");
    //             Debug.Log($"该图片提示词: {randomImage.prompt}");
    //         }
    //         else
    //         {
    //             Debug.LogError("Failed to create sprite from batch generated texture");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("批量生成的图片下载失败");
    //     }
    // }

}