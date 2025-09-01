using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorImage : MonoBehaviour
{
    public LoadTextureAction _loadTextureAction; // 纹理加载组件
    public GenerateImageAction _generateImageAction; // 图片生成组件

    [Header("图片生成配置")]
    [SerializeField] private string defaultPrompt = "a cat~"; // 默认提示词

    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Assert(_loadTextureAction != null, "_loadTextureAction is null");
        Debug.Assert(_generateImageAction != null, "_generateImageAction is null");

        // 保持 SpriteRenderer 的原始设置，不强制修改 drawMode 和 size
        Debug.Log($"SpriteRenderer 初始设置: Size=({_spriteRenderer.size.x}, {_spriteRenderer.size.y}), DrawMode={_spriteRenderer.drawMode}");
    }

    /// <summary>
    /// 新增：点击生成按钮 - 单张图片生成
    /// </summary>
    public void UpdateImage()
    {
        StartCoroutine(GenerateImageAndApply());
    }


    /// <summary>
    /// 生成单张图片并应用到UI
    /// 流程：GenerateImageAction生成 -> LoadTextureAction下载 -> 应用到Image
    /// </summary>
    private IEnumerator GenerateImageAndApply()
    {
        //Debug.Log($"开始生成单张图片: {defaultPrompt}，使用模型: {customModelName}");

        // 创建自定义请求，指定模型（使用新的数据结构）
        var request = new GenerateImagesRequest
        {
            prompts = new List<string> { defaultPrompt },
            //model_name = customModelName // 使用本页指定的模型而不是默认的 sdxl-lightning
        };

        // 第一步：使用 GenerateImageAction 生成图片
        GenerateImagesResponse generateResult = null;
        yield return StartCoroutine(_generateImageAction.GenerateImagesCoroutine(request, (result) =>
        {
            generateResult = result;
        }));

        // 检查生成是否成功
        if (generateResult == null || !generateResult.success || generateResult.images == null || generateResult.images.Count == 0)
        {
            Debug.LogError($"图片生成失败: {generateResult?.message}");
            yield break;
        }

        // 获取第一张生成的图片
        var firstImage = generateResult.images[0];
        Debug.Log($"图片生成成功: {firstImage.image_url}");

        // 第二步：使用 LoadTextureAction 下载生成的图片
        yield return StartCoroutine(_loadTextureAction.LoadAndApply(firstImage.image_url));

        // 第三步：应用到UI
        if (_loadTextureAction.HasTexture())
        {
            // 使用固定的 pixelsPerUnit，让 Sliced 模式自动处理填满
            var sprite = _loadTextureAction.CreateSpriteFromCurrentTexture();
            if (sprite != null)
            {
                _spriteRenderer.sprite = sprite;
                Debug.Log($"生成的图片已应用到UI: {_loadTextureAction.GetTextureInfo()}");
                Debug.Log($"SpriteRenderer 设置: Size=({_spriteRenderer.size.x}, {_spriteRenderer.size.y}), DrawMode={_spriteRenderer.drawMode}");
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
}