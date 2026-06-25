using System;
using System.Collections.Generic;

/// <summary>
/// 单张图片生成配置 - 对应一个完整的生成任务
/// </summary>
[Serializable]
public class ImageGenerationConfig
{
    /// <summary>
    /// 文本提示词（必需参数）
    /// </summary>
    public string prompt = "";

    /// <summary>
    /// 模型名称，为空时使用默认模型
    /// </summary>
    public string model = "";

    /// <summary>
    /// 负向提示词，描述不希望出现的内容
    /// </summary>
    public string negative_prompt = "worst quality, low quality, blurry";

    /// <summary>
    /// 推理步数，范围: 1-50，步数越多质量越好但速度越慢
    /// </summary>
    public int num_inference_steps = 4;

    /// <summary>
    /// 引导比例，范围: 1.0-20.0，控制生成结果与提示词的贴合度
    /// </summary>
    public float guidance_scale = 7.5f;

    /// <summary>
    /// 图片宽度，范围: 256-2048像素
    /// </summary>
    public int width = 1024;

    /// <summary>
    /// 图片高度，范围: 256-2048像素
    /// </summary>
    public int height = 1024;

    /// <summary>
    /// 宽高比 (如 '1:1', '16:9')，设置后优先级高于 width/height
    /// </summary>
    public string aspect_ratio = "";

    /// <summary>
    /// 调度器类型，控制生成算法
    /// </summary>
    public string scheduler = "K_EULER";

    /// <summary>
    /// 随机种子，用于复现相同的生成结果，-1表示未设置（随机）
    /// </summary>
    public int seed = -1;

    /// <summary>
    /// ideogram 模型专用参数，可选值: Auto/On/Off
    /// </summary>
    public string magic_prompt_option = "Auto";
}

/// <summary>
/// 图片生成请求模型 - 支持单张或批量生成（每个配置独立）
/// </summary>
[Serializable]
public class ImageGenerationRequest
{
    /// <summary>
    /// 图片生成配置列表，每个配置独立生成一张图片，至少需要一个配置
    /// </summary>
    public List<ImageGenerationConfig> configs = new();
}


/// <summary>
/// 图片生成响应模型 - 支持单张或批量响应
/// </summary>
[Serializable]
public class ImageGenerationResponse
{
    /// <summary>
    /// 生成的图片列表
    /// </summary>
    public List<GeneratedImage> images = new();

    /// <summary>
    /// 总耗时（秒）
    /// </summary>
    public float elapsed_time = 0f;
}