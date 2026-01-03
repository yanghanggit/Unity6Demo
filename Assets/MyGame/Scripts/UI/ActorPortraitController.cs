using UnityEngine;

/// <summary>
/// Actor 头像控制器
/// 用于生成并显示 Actor 的 AI 头像图片
/// 继承自 CachedRemoteImageController，专注于 Actor 特定的业务逻辑
/// </summary>
public class ActorPortraitController : CachedRemoteImageController
{
    [Header("Actor Settings")]
    public string EntityName { get; set; }
    public string Prompt { get; set; }
    public string ImageUrlStorageKey { get; set; }
    public string SpriteCacheKey { get; set; }

    void Start()
    {
        // 参数验证
        Debug.Assert(!string.IsNullOrEmpty(EntityName), "[ActorPortraitController] EntityName is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(Prompt), "[ActorPortraitController] Prompt is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(ImageUrlStorageKey), "[ActorPortraitController] ImageUrlStorageKey is null or empty");
        Debug.Assert(!string.IsNullOrEmpty(SpriteCacheKey), "[ActorPortraitController] SpriteCacheKey is null or empty");
        Debug.Assert(ImageUrlStorageKey == SpriteCacheKey, "[ActorPortraitController] For simplicity, ImageUrlStorageKey should equal SpriteCacheKey in current implementation");

        // 调用基类方法启动图片加载流程
        StartImageLoadingProcess(Prompt, ImageUrlStorageKey, SpriteCacheKey);
    }
}



