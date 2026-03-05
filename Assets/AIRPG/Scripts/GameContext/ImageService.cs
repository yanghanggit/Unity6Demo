//using UnityEngine;

/// <summary>
/// 图片服务类
/// 提供图片服务器相关的API端点访问，包括图片生成和静态图片获取
/// </summary>
public static partial class ImageService
{

    /// <summary>
    /// 获取生成图片API的URL地址
    /// </summary>    
    public static string BaseUrl
    {
        get; set;
    }
    /// <summary>
    /// 获取生成图片API的URL地址
    /// </summary>
    public static string GenerateImageApiUrl
    {
        get
        {
            return BaseUrl.TrimEnd('/') + "/api/generate/v1";
        }
    }
}

