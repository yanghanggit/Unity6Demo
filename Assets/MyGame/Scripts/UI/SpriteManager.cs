using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 纹理管理器，使用单例模式 + DontDestroyOnLoad 保持全局存在
/// 使用：SpriteManager.Instance.GetSprite("texture_key")
/// </summary>
public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    [Header("预加载纹理配置")]
    [Tooltip("需要预加载的纹理数组")]
    [SerializeField] private Texture2D[] preloadTextures;

    [Tooltip("对应纹理的键值数组，与上面的纹理数组一一对应")]
    [SerializeField] private string[] textureKeys;

    [Header("Sprite创建参数")]
    [Tooltip("像素单位，影响Sprite的显示大小")]
    [SerializeField] private float pixelsPerUnit = 100f;

    [Tooltip("Sprite的锚点位置")]
    [SerializeField] private Vector2 spritePivot = new(0.5f, 0.5f);

    [Header("运行时配置")]
    [Tooltip("Resources文件夹路径，用于运行时加载纹理")]
    [SerializeField] private string resourcesTexturePath = "Textures";

    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("TextureManager: Instance created and marked as DontDestroyOnLoad");
            InitializeTextures();
        }
        else
        {
            Debug.LogWarning("[TextureManager] Duplicate instance detected, destroying the new one.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化纹理管理器
    /// </summary>
    private void InitializeTextures()
    {
        LoadPreConfiguredTextures();
        Debug.Log($"TextureManager: Initialization completed. Total cached sprites: {spriteCache.Count}");
    }

    /// <summary>
    /// 获取 Sprite，先查缓存，没有则从 Resources 加载
    /// </summary>
    public Sprite GetSprite(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("TextureManager: GetSprite called with null or empty key");
            return null;
        }

        if (spriteCache.TryGetValue(key, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        return LoadSpriteFromResources(key);
    }

    /// <summary>
    /// 检查 Sprite 是否存在
    /// </summary>
    public bool HasSprite(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        return spriteCache.ContainsKey(key);
    }

    /// <summary>
    /// 移除缓存的 Sprite
    /// </summary>
    public bool RemoveSprite(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        if (spriteCache.TryGetValue(key, out Sprite sprite))
        {
            spriteCache.Remove(key);
            if (sprite != null)
                DestroyImmediate(sprite);

            Debug.Log($"TextureManager: Removed sprite '{key}' from cache");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    public void ClearCache()
    {
        foreach (var sprite in spriteCache.Values)
        {
            if (sprite != null)
                DestroyImmediate(sprite);
        }

        spriteCache.Clear();
        Debug.Log("TextureManager: Cache cleared");
    }

    /// <summary>
    /// 加载预配置的纹理
    /// </summary>
    private void LoadPreConfiguredTextures()
    {
        if (preloadTextures == null || textureKeys == null)
        {
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
                Debug.LogWarning($"TextureManager: Skipping invalid texture at index {i}");
            }
        }

        Debug.Log($"TextureManager: Preloaded {loadedCount} textures from configuration");
    }

    /// <summary>
    /// 从 Resources 加载 Sprite
    /// </summary>
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
                Debug.Log($"TextureManager: Loaded sprite '{key}' from Resources");
                return sprite;
            }
        }
        else
        {
            Debug.LogWarning($"TextureManager: Failed to load texture '{key}' from Resources path '{fullPath}'");
        }

        return null;
    }

    /// <summary>
    /// 从 Texture2D 创建 Sprite
    /// </summary>
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
            Debug.LogError($"TextureManager: Failed to create sprite '{spriteName}': {e.Message}");
            return null;
        }
    }
}
