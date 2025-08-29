using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System;

/**
 * LoadTextureAction.cs
 * 
 * 专门用于加载远程纹理的 MonoBehaviour
 * 基于 BaseRequestAction 的设计模式
 * 支持协程和 async/await 两种方式
 */
public class LoadTextureAction : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private float requestTimeout = 30f; // 请求超时时间（秒）
    [SerializeField] private int maxRetryAttempts = 3; // 最大重试次数
    [SerializeField] private float retryDelay = 1f; // 重试延迟（秒）
    [SerializeField] private TextureWrapMode wrapMode = TextureWrapMode.Clamp;
    [SerializeField] private FilterMode filterMode = FilterMode.Bilinear;

    // 加载结果
    private Texture2D _currentTexture;
    public Texture2D CurrentTexture => _currentTexture;

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
        TextureLoadResult result = null;

        yield return StartCoroutine(LoadTextureCoroutine(url, (loadResult) =>
        {
            result = loadResult;
        }));

        if (result != null && result.isSuccess)
        {
            // 清理之前的纹理
            ClearCurrentTexture();

            // 设置新纹理
            _currentTexture = result.texture;
            _currentTexture.wrapMode = wrapMode;
            _currentTexture.filterMode = filterMode;

            Debug.Log($"Texture loaded successfully: {url} ({_currentTexture.width}x{_currentTexture.height})");
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
        var result = await LoadTextureAsync(url);

        if (result.isSuccess)
        {
            // 清理之前的纹理
            ClearCurrentTexture();

            // 设置新纹理
            _currentTexture = result.texture;
            _currentTexture.wrapMode = wrapMode;
            _currentTexture.filterMode = filterMode;

            Debug.Log($"Texture loaded successfully: {url} ({_currentTexture.width}x{_currentTexture.height})");
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
            Destroy(_currentTexture);
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

    #endregion

    #region Unity 生命周期

    void OnDestroy()
    {
        ClearCurrentTexture();
    }

    #endregion
}
