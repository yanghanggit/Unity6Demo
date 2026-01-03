using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

/**
 * TextureLoader.cs - 简单的纹理加载器
 * 
 * 用法:
 * 1. 将此组件挂载到 GameObject 上
 * 2. 调用 StartCoroutine(LoadTexture(url)) 加载纹理
 * 3. 通过 Result 属性获取加载结果
 * 4. 通过 LoadedTexture 快捷访问加载的纹理
 */
public class TextureLoader : MonoBehaviour
{
    // 加载结果（包含成功状态、纹理、错误信息等）
    private TextureLoadResult _result;
    public TextureLoadResult Result => _result;

    // 快捷访问：直接获取加载的纹理（如果加载成功）
    public Texture2D LoadedTexture => _result?.Texture;

    // 纹理加载结果结构
    [Serializable]
    public class TextureLoadResult
    {
        public bool IsSuccess;
        public Texture2D Texture;
        public string Error;
        public long ResponseCode;

        public TextureLoadResult(bool success, Texture2D tex = null, string errorMsg = "", long code = 0)
        {
            IsSuccess = success;
            Texture = tex;
            Error = errorMsg;
            ResponseCode = code;
        }
    }

    /// <summary>
    /// 加载纹理（协程方法）
    /// </summary>
    /// <param name="url">纹理的 URL 地址</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator LoadTexture(string url)
    {
        // 重置加载结果（不销毁纹理，因为纹理可能已被 Sprite 使用，生命周期由 SpriteManager 管理）
        _result = null;

        using (var request = UnityWebRequestTexture.GetTexture(url, true))
        {
            SetCommonHeaders(request);
            Debug.Log($"[TextureLoader] Starting to load texture from: {url}");

            // 发送请求并等待响应
            yield return request.SendWebRequest();

            // 处理响应结果
            _result = ProcessTextureResponse(request);

            // 记录最终状态
            if (_result != null && _result.IsSuccess)
            {
                Debug.Log($"[TextureLoader] ✓ Texture loaded successfully: {url} ({_result.Texture.width}x{_result.Texture.height})");
            }
        }
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
        // 检查网络连接错误（如：无网络、DNS 解析失败等）
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            string error = $"Connection Error: {request.error}";
            Debug.LogError($"[TextureLoader] ✗ {error}");
            return new TextureLoadResult(false, null, error, request.responseCode);
        }

        // 检查协议错误（如：404、500 等 HTTP 状态码错误）
        if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            string error = $"Protocol Error: {request.error} (HTTP {request.responseCode})";
            Debug.LogError($"[TextureLoader] ✗ {error}");
            return new TextureLoadResult(false, null, error, request.responseCode);
        }

        // 检查数据处理错误（如：下载的数据无法解析为纹理）
        if (request.result == UnityWebRequest.Result.DataProcessingError)
        {
            string error = $"Data Processing Error: {request.error}";
            Debug.LogError($"[TextureLoader] ✗ {error}");
            return new TextureLoadResult(false, null, error, request.responseCode);
        }

        // 请求成功 - 提取纹理数据
        try
        {
            var texture = DownloadHandlerTexture.GetContent(request);
            if (texture != null)
            {
                // 纹理提取成功，返回结果（不在这里记录日志，由 LoadTexture 统一记录）
                return new TextureLoadResult(true, texture, "", request.responseCode);
            }
            else
            {
                // 响应成功但纹理为空（可能是无效的图片数据）
                string error = "Texture data is null or invalid";
                Debug.LogError($"[TextureLoader] ✗ {error}");
                return new TextureLoadResult(false, null, error, request.responseCode);
            }
        }
        catch (System.Exception ex)
        {
            // 提取纹理时发生异常
            string error = $"Exception while extracting texture: {ex.Message}";
            Debug.LogError($"[TextureLoader] ✗ {error}");
            return new TextureLoadResult(false, null, error, request.responseCode);
        }
    }
}