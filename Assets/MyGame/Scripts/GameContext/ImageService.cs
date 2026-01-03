using UnityEngine;

/// <summary>
/// 图片服务类
/// 提供图片服务器相关的API端点访问，包括图片生成和静态图片获取
/// </summary>
public static partial class ImageService
{
    /// <summary>
    /// 获取生成图片API的URL地址
    /// </summary>
    public static string GenerateImageApiUrl
    {
        get
        {
            return ApiEndpointsManager.ImageApiBaseUrl.TrimEnd('/') + ApiEndpointsManager.ImageRootResponse.endpoints["generate"];
        }
    }

    /// <summary>
    /// 获取静态图片API的URL地址
    /// </summary>
    public static string StaticImagesApiUrl
    {
        get
        {
            return ApiEndpointsManager.ImageApiBaseUrl.TrimEnd('/') + ApiEndpointsManager.ImageRootResponse.endpoints["static_images"];
        }
    }

    /// <summary>
    /// 生成图片资源的唯一标识Key
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名称</param>
    /// <param name="entityName">实体类型（如：actor/stage/item等）</param>
    /// <param name="imageDescription">描述信息（将被Base64编码）</param>
    /// <returns>格式为 "{userName}/{gameName}/{entityName}/{base64EncodedDescription}" 的key字符串</returns>
    public static string GenerateImageKey(string userName, string gameName, string entityName, string imageDescription)
    {
        Debug.Assert(!string.IsNullOrEmpty(userName), "userName is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(gameName), "gameName is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(entityName), "entityName is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(imageDescription), "imageDescription is null or empty");

        // 将description编码为Base64字符串
        byte[] descriptionBytes = System.Text.Encoding.UTF8.GetBytes(imageDescription);
        string base64EncodedDescription = System.Convert.ToBase64String(descriptionBytes);

        // 生成并返回key
        return $"{userName}/{gameName}/{entityName}/{base64EncodedDescription}";
    }

    /// <summary>
    /// 设置图片URL映射
    /// 将远程图片URL存储到本地，使用生成的key作为索引
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名称</param>
    /// <param name="entityName">实体类型（如：actor/stage/item等）</param>
    /// <param name="imageDescription">图片描述信息</param>
    /// <param name="imageUrl">远程图片URL</param>
    public static void SetImageUrl(string userName, string gameName, string entityName, string imageDescription, string imageUrl)
    {
        Debug.Assert(!string.IsNullOrEmpty(imageUrl), "imageUrl is null or empty");
        string key = GenerateImageKey(userName, gameName, entityName, imageDescription);
        PlayerPrefs.SetString(key, imageUrl);
        PlayerPrefs.Save(); // 立即保存，确保数据持久化
    }

    /// <summary>
    /// 获取图片URL映射
    /// 根据key从本地获取存储的远程图片URL
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名称</param>
    /// <param name="entityName">实体类型（如：actor/stage/item等）</param>
    /// <param name="imageDescription">图片描述信息</param>
    /// <returns>存储的远程图片URL，如果不存在则返回空字符串</returns>
    public static string GetImageUrl(string userName, string gameName, string entityName, string imageDescription)
    {
        string key = GenerateImageKey(userName, gameName, entityName, imageDescription);
        return PlayerPrefs.GetString(key, string.Empty);
    }

    /// <summary>
    /// 检查指定图片URL映射是否存在
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名称</param>
    /// <param name="entityName">实体类型（如：actor/stage/item等）</param>
    /// <param name="imageDescription">图片描述信息</param>
    /// <returns>如果存在映射返回true，否则返回false</returns>
    public static bool HasImageUrl(string userName, string gameName, string entityName, string imageDescription)
    {
        string key = GenerateImageKey(userName, gameName, entityName, imageDescription);
        return PlayerPrefs.HasKey(key);
    }

    /// <summary>
    /// 删除图片URL映射（通过key）
    /// 当远程图片URL失效时，移除本地存储的映射关系
    /// </summary>
    /// <param name="key">图片资源的唯一标识Key</param>
    public static void RemoveImageUrl(string key)
    {
        Debug.Assert(!string.IsNullOrEmpty(key), "key is null or empty");
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save(); // 立即保存，确保删除持久化
    }

    /// <summary>
    /// 包装外观描述为优化的图片生成提示词
    /// 生成Markdown格式的结构化提示词，包含任务说明、外观信息和生成规则
    /// </summary>
    /// <param name="actorName">角色名称</param>
    /// <param name="appearancePrompt">原始外观描述</param>
    /// <returns>Markdown格式的完整提示词</returns>
    public static string WrapAppearancePromptForGeneration(string actorName, string appearancePrompt)
    {
        Debug.Assert(!string.IsNullOrEmpty(actorName), "actorName is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(appearancePrompt), "appearancePrompt is null or empty");

        return $@"# 生成图片任务！角色人物的立绘

## 外观信息

{appearancePrompt.Trim()}

## 生成规则

- 不要背景，用纯黑色背景
- 角色人物要 日式卡通 风格
- 正面视角，面向观众";
    }

}

