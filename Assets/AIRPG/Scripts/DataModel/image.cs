

using System;

/// <summary>
/// 单张生成图片信息
/// </summary>
[Serializable]
public class GeneratedImage
{
    /// <summary>
    /// 图片文件名
    /// </summary>
    public string filename = "";

    /// <summary>
    /// 图片访问URL（相对路径）
    /// </summary>
    public string url = "";

    /// <summary>
    /// 生成图片使用的提示词
    /// </summary>
    public string prompt = "";

    /// <summary>
    /// 生成图片使用的模型名称
    /// </summary>
    public string model = "";

    /// <summary>
    /// 图片在服务器的本地存储路径
    /// </summary>
    public string local_path = "";
}