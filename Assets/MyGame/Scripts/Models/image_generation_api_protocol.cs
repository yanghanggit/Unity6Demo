using System;
using System.Collections.Generic;

// 图片生成请求模型 - 支持单张或批量生成
[Serializable]
public class GenerateImagesRequest
{
    public List<string> prompts = new List<string>();
    public string model_name = "sdxl-lightning";
    public string negative_prompt = "worst quality, low quality, blurry";
    public int width = 768;
    public int height = 768;
    public int num_inference_steps = 4;
    public float guidance_scale = 7.5f;
}

// 单张图片信息模型
[Serializable]
public class ImageInfo
{
    public string prompt = "";
    public string filename = "";
    public string image_url = "";
    public string local_path = "";
}

// 图片生成响应模型 - 支持单张或批量响应
[Serializable]
public class GenerateImagesResponse
{
    public bool success = false;
    public string message = "";
    public List<ImageInfo> images = new List<ImageInfo>();
}

// 图片列表响应模型
[Serializable]
public class ImageListResponse
{
    public List<string> images = new List<string>();
}


