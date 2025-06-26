using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

/**
 * ImprovedRequestAction.cs
 * 
 * 改进的 HTTP 请求处理类，针对 Unity 6 和 WebGL 优化
 * 
 * 主要改进：
 * - 资源管理（using 语句）
 * - 更好的错误处理
 * - 超时设置
 * - 重试机制
 * - 支持 async/await（Unity 6+）
 * - WebGL 兼容性
 */
public class ImprovedRequestAction : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private float requestTimeout = 30f; // 请求超时时间（秒）
    [SerializeField] private int maxRetryAttempts = 3; // 最大重试次数
    [SerializeField] private float retryDelay = 1f; // 重试延迟（秒）
    
    // 请求结果结构
    [Serializable]
    public class RequestResult
    {
        public bool isSuccess;
        public string responseText;
        public long responseCode;
        public string error;
        
        public RequestResult(bool success, string response = "", long code = 0, string errorMsg = "")
        {
            isSuccess = success;
            responseText = response;
            responseCode = code;
            error = errorMsg;
        }
    }

    #region 协程版本（兼容现有代码）
    
    /// <summary>
    /// 发送 GET 请求（协程版本）
    /// </summary>
    public IEnumerator GetRequestCoroutine(string url, System.Action<RequestResult> onComplete = null)
    {
        var result = new RequestResult(false);
        
        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                // 设置超时
                request.timeout = (int)requestTimeout;
                
                // 设置请求头
                SetCommonHeaders(request);
                
                Debug.Log($"GET Request (Attempt {attempt + 1}): {url}");
                
                // 发送请求
                yield return request.SendWebRequest();
                
                // 处理结果
                result = ProcessResponse(request);
                
                if (result.isSuccess)
                {
                    break; // 成功，跳出重试循环
                }
                
                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Request failed, retrying in {retryDelay} seconds...");
                    yield return new WaitForSeconds(retryDelay);
                }
            }
        }
        
        onComplete?.Invoke(result);
    }

    /// <summary>
    /// 发送 POST 请求（协程版本）
    /// </summary>
    public IEnumerator PostRequestCoroutine(string url, string jsonData, System.Action<RequestResult> onComplete = null)
    {
        var result = new RequestResult(false);
        
        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            using (var request = CreatePostRequest(url, jsonData))
            {
                Debug.Log($"POST Request (Attempt {attempt + 1}): {url}\nData: {jsonData}");
                
                // 发送请求
                yield return request.SendWebRequest();
                
                // 处理结果
                result = ProcessResponse(request);
                
                if (result.isSuccess)
                {
                    break; // 成功，跳出重试循环
                }
                
                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Request failed, retrying in {retryDelay} seconds...");
                    yield return new WaitForSeconds(retryDelay);
                }
            }
        }
        
        onComplete?.Invoke(result);
    }
    
    #endregion

    #region Async/Await 版本（Unity 6 推荐）
    
    /// <summary>
    /// 发送 GET 请求（Async 版本）
    /// </summary>
    public async Task<RequestResult> GetRequestAsync(string url)
    {
        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)requestTimeout;
                SetCommonHeaders(request);
                
                Debug.Log($"GET Request Async (Attempt {attempt + 1}): {url}");
                
                var operation = request.SendWebRequest();
                
                // 等待请求完成
                while (!operation.isDone)
                {
                    await Task.Yield(); // 让出控制权给Unity主线程
                }
                
                var result = ProcessResponse(request);
                
                if (result.isSuccess)
                {
                    return result;
                }
                
                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Request failed, retrying in {retryDelay} seconds...");
                    await Task.Delay((int)(retryDelay * 1000));
                }
            }
        }
        
        return new RequestResult(false, "", 0, "Max retry attempts reached");
    }

    /// <summary>
    /// 发送 POST 请求（Async 版本）
    /// </summary>
    public async Task<RequestResult> PostRequestAsync(string url, string jsonData)
    {
        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            using (var request = CreatePostRequest(url, jsonData))
            {
                Debug.Log($"POST Request Async (Attempt {attempt + 1}): {url}\nData: {jsonData}");
                
                var operation = request.SendWebRequest();
                
                // 等待请求完成
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                var result = ProcessResponse(request);
                
                if (result.isSuccess)
                {
                    return result;
                }
                
                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Request failed, retrying in {retryDelay} seconds...");
                    await Task.Delay((int)(retryDelay * 1000));
                }
            }
        }
        
        return new RequestResult(false, "", 0, "Max retry attempts reached");
    }
    
    #endregion

    #region 私有辅助方法
    
    /// <summary>
    /// 创建 POST 请求
    /// </summary>
    private UnityWebRequest CreatePostRequest(string url, string jsonData)
    {
        var request = new UnityWebRequest(url, "POST");
        
        if (!string.IsNullOrEmpty(jsonData))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        }
        
        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = (int)requestTimeout;
        
        SetCommonHeaders(request);
        
        return request;
    }
    
    /// <summary>
    /// 设置通用请求头
    /// </summary>
    private void SetCommonHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        
        // WebGL 特殊处理
        #if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 构建中避免某些可能被浏览器阻止的头部
        #else
        request.SetRequestHeader("User-Agent", $"Unity-{Application.unityVersion}");
        #endif
    }
    
    /// <summary>
    /// 处理响应
    /// </summary>
    private RequestResult ProcessResponse(UnityWebRequest request)
    {
        // 检查网络错误
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            string error = $"Connection Error: {request.error}";
            Debug.LogError(error);
            return new RequestResult(false, "", request.responseCode, error);
        }
        
        // 检查协议错误
        if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            string error = $"Protocol Error: {request.error} (Response Code: {request.responseCode})";
            Debug.LogError(error);
            return new RequestResult(false, request.downloadHandler?.text ?? "", request.responseCode, error);
        }
        
        // 检查数据处理错误
        if (request.result == UnityWebRequest.Result.DataProcessingError)
        {
            string error = $"Data Processing Error: {request.error}";
            Debug.LogError(error);
            return new RequestResult(false, "", request.responseCode, error);
        }
        
        // 成功
        string responseText = request.downloadHandler?.text ?? "";
        Debug.Log($"Request Success: {responseText}");
        return new RequestResult(true, responseText, request.responseCode);
    }
    
    #endregion

    #region 工具方法
    
    /// <summary>
    /// 构建带查询参数的 URL
    /// </summary>
    public static string BuildUrlWithQueryParams(string baseUrl, Dictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return baseUrl;
            
        var uriBuilder = new StringBuilder(baseUrl);
        uriBuilder.Append("?");
        
        bool first = true;
        foreach (var param in parameters)
        {
            if (!first)
                uriBuilder.Append("&");
                
            string encodedKey = UnityWebRequest.EscapeURL(param.Key);
            string encodedValue = UnityWebRequest.EscapeURL(param.Value ?? "");
            uriBuilder.Append($"{encodedKey}={encodedValue}");
            
            first = false;
        }
        
        return uriBuilder.ToString();
    }
    
    /// <summary>
    /// 检查网络可达性（WebGL 友好）
    /// </summary>
    public static bool IsNetworkReachable()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 中总是假设有网络连接
        return true;
        #else
        return Application.internetReachability != NetworkReachability.NotReachable;
        #endif
    }
    
    #endregion
}
