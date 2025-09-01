using System;
using System.Collections.Generic;

[Serializable]
public class GenerateImageRequest
{
    /// <summary>
    /// 图片生成请求模型
    /// </summary>
    public string prompt;
    public string model_name = "sdxl-lightning";
    public string negative_prompt = "worst quality, low quality, blurry";
    public int width = 768;
    public int height = 768;
    public int num_inference_steps = 4;
    public float guidance_scale = 7.5f;

    public GenerateImageRequest(string prompt)
    {
        this.prompt = prompt;
    }
}

[Serializable]
public class GenerateBatchImagesRequest
{
    /// <summary>
    /// 批量图片生成请求模型
    /// </summary>
    public List<string> prompts; // 多个提示词
    public string model_name = "sdxl-lightning";
    public string negative_prompt = "worst quality, low quality, blurry";
    public int width = 768;
    public int height = 768;
    public int num_inference_steps = 4;
    public float guidance_scale = 7.5f;

    public GenerateBatchImagesRequest(List<string> prompts)
    {
        this.prompts = prompts;
    }
}

[Serializable]
public class ImageInfo
{
    /// <summary>
    /// 单张图片信息模型
    /// </summary>
    public string prompt;
    public string filename;
    public string image_url;
    public string local_path;
}

[Serializable]
public class GenerateImageResponse
{
    /// <summary>
    /// 图片生成响应模型
    /// </summary>
    public bool success;
    public string message;
    public string image_url;
    public string local_path;
    public string filename;
}

[Serializable]
public class GenerateBatchImagesResponse
{
    /// <summary>
    /// 批量图片生成响应模型
    /// </summary>
    public bool success;
    public string message;
    public int total_count;
    public List<ImageInfo> images;
}

[Serializable]
public class ImageListResponse
{
    public List<string> images;
    public int total_count;
    public string base_url;
}