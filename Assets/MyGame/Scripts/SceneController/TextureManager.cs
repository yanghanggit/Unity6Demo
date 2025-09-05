using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 纹理管理器 - 方案一：单例资源管理器 + DontDestroyOnLoad
/// 用于在第一个场景加载所有需要的Texture，并在后续场景中提供统一的访问接口
/// 使用方式：spriteRenderer.sprite = TextureManager.Instance.GetSprite("texture_key");
/// </summary>
public class TextureManager : MonoBehaviour
{
    #region 单例模式

    public static TextureManager Instance { get; private set; }

    #endregion

    #region 配置字段

    [Header("预加载纹理配置")]
    [Tooltip("需要预加载的纹理数组")]
    [SerializeField] private Texture2D[] preloadTextures;

    [Tooltip("对应纹理的键值数组，与上面的纹理数组一一对应")]
    [SerializeField] private string[] textureKeys;

    [Header("Sprite创建参数")]
    [Tooltip("像素单位，影响Sprite的显示大小")]
    [SerializeField] private float pixelsPerUnit = 100f;

    [Tooltip("Sprite的锚点位置")]
    [SerializeField] private Vector2 spritePivot = new Vector2(0.5f, 0.5f);

    [Header("运行时配置")]
    [Tooltip("是否在启动时自动预加载所有纹理")]
    [SerializeField] private bool autoPreloadOnStart = true;

    [Tooltip("Resources文件夹路径，用于运行时加载纹理")]
    [SerializeField] private string resourcesTexturePath = "Textures";

    [Header("调试信息")]
    [Tooltip("是否显示调试日志")]
    [SerializeField] private bool enableDebugLog = true;

    #endregion

    #region 私有字段

    // Sprite缓存字典
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    // 是否已经初始化
    private bool isInitialized = false;

    #endregion

    #region Unity生命周期

    private void Awake()
    {
        // 单例模式处理
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (enableDebugLog)
                Debug.Log("TextureManager: Instance created and marked as DontDestroyOnLoad");

            if (autoPreloadOnStart)
                InitializeTextures();
        }
        else
        {
            if (enableDebugLog)
                Debug.Log("TextureManager: Duplicate instance destroyed");
            Destroy(gameObject);
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 初始化纹理管理器，预加载配置的纹理
    /// </summary>
    public void InitializeTextures()
    {
        if (isInitialized)
        {
            if (enableDebugLog)
                Debug.LogWarning("TextureManager: Already initialized");
            return;
        }

        LoadPreConfiguredTextures();
        isInitialized = true;

        if (enableDebugLog)
            Debug.Log($"TextureManager: Initialization completed. Total cached sprites: {spriteCache.Count}");
    }

    /// <summary>
    /// 获取Sprite
    /// 先从缓存中查找，如果没有则尝试从Resources加载
    /// </summary>
    /// <param name="key">纹理键值</param>
    /// <returns>对应的Sprite，如果找不到返回null</returns>
    public Sprite GetSprite(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            if (enableDebugLog)
                Debug.LogWarning("TextureManager: GetSprite called with null or empty key");
            return null;
        }

        // 先从缓存中查找
        if (spriteCache.TryGetValue(key, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        // 如果缓存中没有，尝试从Resources加载
        return LoadSpriteFromResources(key);
    }

    /// <summary>
    /// 检查是否存在指定的Sprite
    /// </summary>
    /// <param name="key">纹理键值</param>
    /// <returns>是否存在</returns>
    public bool HasSprite(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        return spriteCache.ContainsKey(key);
    }

    /// <summary>
    /// 运行时加载纹理并缓存为Sprite
    /// </summary>
    /// <param name="key">纹理键值</param>
    /// <param name="texture">纹理对象</param>
    /// <param name="customPixelsPerUnit">自定义像素单位，如果为负数则使用默认值</param>
    /// <param name="customPivot">自定义锚点，如果为null则使用默认值</param>
    /// <returns>创建的Sprite</returns>
    public Sprite LoadTexture(string key, Texture2D texture, float customPixelsPerUnit = -1f, Vector2? customPivot = null)
    {
        if (string.IsNullOrEmpty(key) || texture == null)
        {
            if (enableDebugLog)
                Debug.LogWarning("TextureManager: LoadTexture called with invalid parameters");
            return null;
        }

        float finalPixelsPerUnit = customPixelsPerUnit > 0 ? customPixelsPerUnit : pixelsPerUnit;
        Vector2 finalPivot = customPivot ?? spritePivot;

        Sprite sprite = CreateSpriteFromTexture(texture, key, finalPixelsPerUnit, finalPivot);

        if (sprite != null)
        {
            spriteCache[key] = sprite;
            if (enableDebugLog)
                Debug.Log($"TextureManager: Loaded and cached sprite '{key}'");
        }

        return sprite;
    }

    /// <summary>
    /// 移除缓存的Sprite
    /// </summary>
    /// <param name="key">纹理键值</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveSprite(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        if (spriteCache.TryGetValue(key, out Sprite sprite))
        {
            spriteCache.Remove(key);
            if (sprite != null)
                DestroyImmediate(sprite);

            if (enableDebugLog)
                Debug.Log($"TextureManager: Removed sprite '{key}' from cache");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 清空所有缓存的Sprite
    /// </summary>
    public void ClearCache()
    {
        foreach (var sprite in spriteCache.Values)
        {
            if (sprite != null)
                DestroyImmediate(sprite);
        }

        spriteCache.Clear();
        isInitialized = false;

        if (enableDebugLog)
            Debug.Log("TextureManager: Cache cleared");
    }

    /// <summary>
    /// 获取当前缓存的Sprite数量
    /// </summary>
    /// <returns>缓存数量</returns>
    public int GetCacheCount()
    {
        return spriteCache.Count;
    }

    /// <summary>
    /// 获取所有缓存的键值列表
    /// </summary>
    /// <returns>键值列表</returns>
    public List<string> GetAllKeys()
    {
        return new List<string>(spriteCache.Keys);
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 加载预配置的纹理
    /// </summary>
    private void LoadPreConfiguredTextures()
    {
        if (preloadTextures == null || textureKeys == null)
        {
            if (enableDebugLog)
                Debug.LogWarning("TextureManager: Preload textures or keys array is null");
            return;
        }

        int loadedCount = 0;
        int maxCount = Mathf.Min(preloadTextures.Length, textureKeys.Length);

        for (int i = 0; i < maxCount; i++)
        {
            if (preloadTextures[i] != null && !string.IsNullOrEmpty(textureKeys[i]))
            {
                Sprite sprite = CreateSpriteFromTexture(preloadTextures[i], textureKeys[i], pixelsPerUnit, spritePivot);
                if (sprite != null)
                {
                    spriteCache[textureKeys[i]] = sprite;
                    loadedCount++;
                }
            }
            else
            {
                if (enableDebugLog)
                    Debug.LogWarning($"TextureManager: Skipping invalid texture at index {i}");
            }
        }

        if (enableDebugLog)
            Debug.Log($"TextureManager: Preloaded {loadedCount} textures from configuration");
    }

    /// <summary>
    /// 从Resources文件夹加载Sprite
    /// </summary>
    /// <param name="key">纹理键值</param>
    /// <returns>加载的Sprite</returns>
    private Sprite LoadSpriteFromResources(string key)
    {
        string fullPath = string.IsNullOrEmpty(resourcesTexturePath) ? key : $"{resourcesTexturePath}/{key}";

        Texture2D texture = Resources.Load<Texture2D>(fullPath);
        if (texture != null)
        {
            Sprite sprite = CreateSpriteFromTexture(texture, key, pixelsPerUnit, spritePivot);
            if (sprite != null)
            {
                spriteCache[key] = sprite;
                if (enableDebugLog)
                    Debug.Log($"TextureManager: Loaded sprite '{key}' from Resources");
                return sprite;
            }
        }
        else
        {
            if (enableDebugLog)
                Debug.LogWarning($"TextureManager: Failed to load texture '{key}' from Resources path '{fullPath}'");
        }

        return null;
    }

    /// <summary>
    /// 从Texture2D创建Sprite
    /// </summary>
    /// <param name="texture">源纹理</param>
    /// <param name="spriteName">Sprite名称</param>
    /// <param name="pixelsPerUnit">像素单位</param>
    /// <param name="pivot">锚点</param>
    /// <returns>创建的Sprite</returns>
    private Sprite CreateSpriteFromTexture(Texture2D texture, string spriteName, float pixelsPerUnit, Vector2 pivot)
    {
        if (texture == null)
            return null;

        try
        {
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                pivot,
                pixelsPerUnit
            );

            sprite.name = spriteName;
            return sprite;
        }
        catch (System.Exception e)
        {
            if (enableDebugLog)
                Debug.LogError($"TextureManager: Failed to create sprite '{spriteName}': {e.Message}");
            return null;
        }
    }

    #endregion

    #region 编辑器辅助方法

#if UNITY_EDITOR

    /// <summary>
    /// 验证配置（仅在编辑器中可用）
    /// </summary>
    [ContextMenu("Validate Configuration")]
    private void ValidateConfiguration()
    {
        if (preloadTextures == null || textureKeys == null)
        {
            Debug.LogError("TextureManager: Preload textures or keys array is null");
            return;
        }

        if (preloadTextures.Length != textureKeys.Length)
        {
            Debug.LogWarning($"TextureManager: Texture array length ({preloadTextures.Length}) doesn't match keys array length ({textureKeys.Length})");
        }

        // 检查重复的键值
        HashSet<string> uniqueKeys = new HashSet<string>();
        List<string> duplicateKeys = new List<string>();

        for (int i = 0; i < textureKeys.Length; i++)
        {
            if (!string.IsNullOrEmpty(textureKeys[i]))
            {
                if (!uniqueKeys.Add(textureKeys[i]))
                {
                    duplicateKeys.Add(textureKeys[i]);
                }
            }
        }

        if (duplicateKeys.Count > 0)
        {
            Debug.LogError($"TextureManager: Found duplicate keys: {string.Join(", ", duplicateKeys)}");
        }
        else
        {
            Debug.Log("TextureManager: Configuration validation passed");
        }
    }

#endif

    #endregion
}
