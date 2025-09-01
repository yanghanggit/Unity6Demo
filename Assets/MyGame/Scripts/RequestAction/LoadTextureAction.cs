using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

/**
 * LoadTextureAction.cs
 * 
 * 专门用于加载远程纹理的 MonoBehaviour
 * 基于 BaseRequestAction 的设计模式
 * 支持协程和 async/await 两种方式
 * 包含内存缓存功能，避免重复下载相同URL的纹理
 */
public class LoadTextureAction : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private float requestTimeout = 30f; // 请求超时时间（秒）
    [SerializeField] private int maxRetryAttempts = 3; // 最大重试次数
    [SerializeField] private float retryDelay = 1f; // 重试延迟（秒）
    [SerializeField] private TextureWrapMode wrapMode = TextureWrapMode.Clamp;
    [SerializeField] private FilterMode filterMode = FilterMode.Bilinear;

    // 静态缓存系统 - 所有实例共享
    private static Dictionary<string, CachedTexture> _textureCache = new Dictionary<string, CachedTexture>();

    // 加载结果
    private Texture2D _currentTexture;
    public Texture2D CurrentTexture => _currentTexture;

    // 缓存项结构
    [Serializable]
    private class CachedTexture
    {
        public Texture2D texture;
        public DateTime lastAccessTime;
        public int accessCount;

        public CachedTexture(Texture2D tex)
        {
            texture = tex;
            lastAccessTime = DateTime.Now;
            accessCount = 1;
        }

        public void UpdateAccess()
        {
            lastAccessTime = DateTime.Now;
            accessCount++;
        }
    }

    // 纹理加载结果结构
    [Serializable]
    public class TextureLoadResult
    {
        public bool isSuccess;
        public Texture2D texture;
        public string error;
        public long responseCode;

        public TextureLoadResult(bool success, Texture2D tex = null, string errorMsg = "", long code = 0)
        {
            isSuccess = success;
            texture = tex;
            error = errorMsg;
            responseCode = code;
        }
    }

    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 加载并应用纹理（协程版本）
    /// </summary>
    public IEnumerator LoadAndApply(string url)
    {
        // 检查缓存
        if (TryGetFromCache(url, out Texture2D cachedTexture))
        {
            // 缓存命中，直接使用
            ApplyTexture(cachedTexture, url, true);
            yield break;
        }

        TextureLoadResult result = null;

        yield return StartCoroutine(LoadTextureCoroutine(url, (loadResult) =>
        {
            result = loadResult;
        }));

        if (result != null && result.isSuccess)
        {
            // 加入缓存
            AddToCache(url, result.texture);

            // 应用纹理
            ApplyTexture(result.texture, url, false);
        }
        else
        {
            Debug.LogError($"Failed to load texture: {url} - {result?.error}");
        }
    }

    /// <summary>
    /// 加载纹理（协程版本，带回调）
    /// </summary>
    public IEnumerator LoadTextureCoroutine(string url, System.Action<TextureLoadResult> onComplete = null)
    {
        var result = new TextureLoadResult(false);

        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            using (var request = UnityWebRequestTexture.GetTexture(url, true))
            {
                // 设置超时
                request.timeout = Mathf.CeilToInt(requestTimeout);

                // 设置请求头
                SetCommonHeaders(request);

                Debug.Log($"Loading Texture (Attempt {attempt + 1}): {url}");

                // 发送请求
                yield return request.SendWebRequest();

                // 处理结果
                result = ProcessTextureResponse(request);

                if (result.isSuccess)
                {
                    break; // 成功，跳出重试循环
                }

                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Texture load failed, retrying in {retryDelay} seconds...");
                    yield return new WaitForSeconds(retryDelay);
                }
            }
        }

        onComplete?.Invoke(result);
    }

    #endregion

    #region Async/Await 版本（Unity 6 推荐）

    /// <summary>
    /// 加载并应用纹理（Async 版本）
    /// </summary>
    public async Task<bool> LoadAndApplyAsync(string url)
    {
        // 检查缓存
        if (TryGetFromCache(url, out Texture2D cachedTexture))
        {
            // 缓存命中，直接使用
            ApplyTexture(cachedTexture, url, true);
            return true;
        }

        var result = await LoadTextureAsync(url);

        if (result.isSuccess)
        {
            // 加入缓存
            AddToCache(url, result.texture);

            // 应用纹理
            ApplyTexture(result.texture, url, false);
            return true;
        }
        else
        {
            Debug.LogError($"Failed to load texture: {url} - {result.error}");
            return false;
        }
    }

    /// <summary>
    /// 加载纹理（Async 版本）
    /// </summary>
    public async Task<TextureLoadResult> LoadTextureAsync(string url)
    {
        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            using (var request = UnityWebRequestTexture.GetTexture(url, true))
            {
                request.timeout = Mathf.CeilToInt(requestTimeout);
                SetCommonHeaders(request);

                Debug.Log($"Loading Texture Async (Attempt {attempt + 1}): {url}");

                var operation = request.SendWebRequest();

                // 等待请求完成
                while (!operation.isDone)
                {
                    await Task.Yield(); // 让出控制权给Unity主线程
                }

                var result = ProcessTextureResponse(request);

                if (result.isSuccess)
                {
                    return result;
                }

                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Texture load failed, retrying in {retryDelay} seconds...");
                    await Task.Delay((int)(retryDelay * 1000));
                }
            }
        }

        return new TextureLoadResult(false, null, "Max retry attempts reached");
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 尝试从缓存获取纹理
    /// </summary>
    private bool TryGetFromCache(string url, out Texture2D texture)
    {
        texture = null;

        if (string.IsNullOrEmpty(url))
            return false;

        if (_textureCache.TryGetValue(url, out CachedTexture cachedItem))
        {
            // 检查纹理是否仍然有效
            if (cachedItem.texture != null)
            {
                cachedItem.UpdateAccess();
                texture = cachedItem.texture;
                return true;
            }
            else
            {
                // 纹理已被销毁，从缓存中移除
                _textureCache.Remove(url);
            }
        }

        return false;
    }

    /// <summary>
    /// 添加纹理到缓存
    /// </summary>
    private void AddToCache(string url, Texture2D texture)
    {
        if (string.IsNullOrEmpty(url) || texture == null)
            return;

        // 添加到缓存
        if (_textureCache.ContainsKey(url))
        {
            // 更新现有缓存
            _textureCache[url].texture = texture;
            _textureCache[url].UpdateAccess();
        }
        else
        {
            // 添加新缓存项
            _textureCache[url] = new CachedTexture(texture);
        }
    }

    /// <summary>
    /// 应用纹理到当前对象
    /// </summary>
    private void ApplyTexture(Texture2D texture, string url, bool fromCache)
    {
        // 清理之前的纹理（但不要销毁缓存中的纹理）
        if (_currentTexture != null && !IsTextureInCache(_currentTexture))
        {
            Destroy(_currentTexture);
        }

        // 设置新纹理
        _currentTexture = texture;
        _currentTexture.wrapMode = wrapMode;
        _currentTexture.filterMode = filterMode;

        string source = fromCache ? "cache" : "network";
        Debug.Log($"Texture applied from {source}: {url} ({_currentTexture.width}x{_currentTexture.height})");
    }

    /// <summary>
    /// 检查纹理是否在缓存中
    /// </summary>
    private bool IsTextureInCache(Texture2D texture)
    {
        foreach (var cachedItem in _textureCache.Values)
        {
            if (cachedItem.texture == texture)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 设置通用请求头
    /// </summary>
    private void SetCommonHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("Accept", "image/*");

        // WebGL 特殊处理
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 构建中避免某些可能被浏览器阻止的头部
#else
        request.SetRequestHeader("User-Agent", $"Unity-{Application.unityVersion}");
#endif
    }

    /// <summary>
    /// 处理纹理响应
    /// </summary>
    private TextureLoadResult ProcessTextureResponse(UnityWebRequest request)
    {
        // 检查网络错误
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            string error = $"Connection Error: {request.error}";
            Debug.LogError(error);
            return new TextureLoadResult(false, null, error, request.responseCode);
        }

        // 检查协议错误
        if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            string error = $"Protocol Error: {request.error} (Response Code: {request.responseCode})";
            Debug.LogError(error);
            return new TextureLoadResult(false, null, error, request.responseCode);
        }

        // 检查数据处理错误
        if (request.result == UnityWebRequest.Result.DataProcessingError)
        {
            string error = $"Data Processing Error: {request.error}";
            Debug.LogError(error);
            return new TextureLoadResult(false, null, error, request.responseCode);
        }

        // 成功 - 获取纹理
        try
        {
            var texture = DownloadHandlerTexture.GetContent(request);
            if (texture != null)
            {
                Debug.Log($"Texture loaded successfully: {texture.width}x{texture.height}");
                return new TextureLoadResult(true, texture, "", request.responseCode);
            }
            else
            {
                string error = "Downloaded texture is null";
                Debug.LogError(error);
                return new TextureLoadResult(false, null, error, request.responseCode);
            }
        }
        catch (System.Exception ex)
        {
            string error = $"Exception while getting texture: {ex.Message}";
            Debug.LogError(error);
            return new TextureLoadResult(false, null, error, request.responseCode);
        }
    }

    /// <summary>
    /// 清理当前纹理
    /// </summary>
    private void ClearCurrentTexture()
    {
        if (_currentTexture != null)
        {
            // 只有当纹理不在缓存中时才销毁
            if (!IsTextureInCache(_currentTexture))
            {
                Destroy(_currentTexture);
            }
            _currentTexture = null;
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 创建Sprite从当前纹理
    /// </summary>
    public Sprite CreateSpriteFromCurrentTexture(float pixelsPerUnit = 100f)
    {
        if (_currentTexture == null)
        {
            Debug.LogWarning("No texture loaded to create sprite from");
            return null;
        }

        return Sprite.Create(_currentTexture,
                           new Rect(0, 0, _currentTexture.width, _currentTexture.height),
                           new Vector2(0.5f, 0.5f),
                           pixelsPerUnit);
    }

    /// <summary>
    /// 检查是否有已加载的纹理
    /// </summary>
    public bool HasTexture()
    {
        return _currentTexture != null;
    }

    /// <summary>
    /// 获取纹理信息
    /// </summary>
    public string GetTextureInfo()
    {
        if (_currentTexture == null)
            return "No texture loaded";

        return $"Texture: {_currentTexture.width}x{_currentTexture.height}, " +
               $"Format: {_currentTexture.format}, " +
               $"WrapMode: {_currentTexture.wrapMode}, " +
               $"FilterMode: {_currentTexture.filterMode}";
    }

    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    public string GetCacheStats()
    {
        int totalAccessCount = _textureCache.Values.Sum(item => item.accessCount);
        return $"Cache: {_textureCache.Count} items, Total access: {totalAccessCount}";
    }

    /// <summary>
    /// 清理所有缓存
    /// </summary>
    public static void ClearAllCache()
    {
        foreach (var cachedItem in _textureCache.Values)
        {
            if (cachedItem.texture != null)
            {
                Destroy(cachedItem.texture);
            }
        }

        _textureCache.Clear();

        Debug.Log("All texture cache cleared");
    }

    /// <summary>
    /// 清理指定URL的缓存
    /// </summary>
    public static void ClearCacheForUrl(string url)
    {
        if (_textureCache.TryGetValue(url, out CachedTexture cachedItem))
        {
            if (cachedItem.texture != null)
            {
                Destroy(cachedItem.texture);
            }
            _textureCache.Remove(url);

            Debug.Log($"Cache cleared for URL: {url}");
        }
    }

    /// <summary>
    /// 检查指定URL是否在缓存中
    /// </summary>
    public static bool IsUrlCached(string url)
    {
        return _textureCache.ContainsKey(url) && _textureCache[url].texture != null;
    }

    /// <summary>
    /// 获取缓存中所有URL列表
    /// </summary>
    public static List<string> GetCachedUrls()
    {
        return _textureCache.Keys.ToList();
    }

    #endregion

    #region Unity 生命周期

    void OnDestroy()
    {
        ClearCurrentTexture();
    }

    #endregion
}


