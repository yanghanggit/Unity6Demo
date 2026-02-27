using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// BaseApiClient - API 请求基类
/// 
/// 为所有具体的 API 操作类（如 LoginAction、StartAction 等）提供统一的 HTTP 请求能力。
/// 针对 Unity 6 和 WebGL 平台进行了优化。
/// 
/// 主要特性：
/// - 支持 GET/POST 请求
/// - 提供 async/await（UniTask）调用方式
/// - 自动资源管理（using 语句）
/// - 完善的错误处理和超时控制
/// - 支持 CancellationToken 取消
/// - WebGL 平台兼容性处理
/// - 统一的请求头设置
/// 
/// 使用方式：
/// 1. 继承此类创建具体的 API 操作类
/// 2. 在子类中调用 GetRequestAsync/PostRequestAsync（返回 UniTask&lt;RequestResult&gt;）
/// 3. 处理返回的 RequestResult 结构
/// 
/// 作者:Unity6Demo Team
/// 日期:2025-01-23
/// </summary>
public abstract class BaseApiClient : MonoBehaviour
{
    /// <summary>
    /// 请求结果属性 - 子类必须实现
    /// 用于获取最近一次请求的结果
    /// </summary>
    public abstract RequestResult ReqResult { get; }


    [Header("请求配置")]
    [SerializeField]
    [Tooltip("HTTP 请求超时时间（秒），默认 30 秒")]
    private float requestTimeout = 30.0f;


    /// <summary>
    /// HTTP 请求结果封装类
    /// 统一封装请求的成功状态、响应内容、状态码和错误信息
    /// </summary>
    [Serializable]
    public class RequestResult
    {
        /// <summary>请求是否成功</summary>
        public bool isSuccess;

        /// <summary>响应文本内容（JSON 字符串）</summary>
        public string responseText;

        /// <summary>HTTP 响应状态码（如 200, 404, 500 等）</summary>
        public long responseCode;

        /// <summary>错误信息（仅在失败时有值）</summary>
        public string error;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="success">请求是否成功</param>
        /// <param name="response">响应文本</param>
        /// <param name="code">HTTP 状态码</param>
        /// <param name="errorMsg">错误信息</param>
        public RequestResult(bool success, string response = "", long code = 0, string errorMsg = "")
        {
            isSuccess = success;
            responseText = response;
            responseCode = code;
            error = errorMsg;
        }
    }

    /// <summary>
/// 发送 GET 请求（UniTask 版本）
/// 使用 UniTask 直接 await UnityWebRequest，零分配，主线程友好
/// </summary>
/// <param name="url">请求的完整 URL</param>
/// <param name="ct">取消令牌，场景销毁时可传入 this.GetCancellationTokenOnDestroy()</param>
/// <returns>包含请求结果的 UniTask</returns>
    public async UniTask<RequestResult> GetRequestAsync(string url, CancellationToken ct = default)
    {
        using (var request = UnityWebRequest.Get(url))
        {
            request.timeout = (int)requestTimeout;
            SetCommonHeaders(request);

            Debug.Log($"GET Request Async: {url}");

            try
            {
                await request.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"GET request cancelled: {url}");
                return new RequestResult(false, "", 0, "Request cancelled");
            }

            return ProcessResponse(request);
        }
    }

    /// <summary>
/// 发送 POST 请求（UniTask 版本）
/// 使用 UniTask 直接 await UnityWebRequest，零分配，主线程友好
/// </summary>
/// <param name="url">请求的完整 URL</param>
/// <param name="jsonData">要发送的 JSON 数据字符串</param>
/// <param name="ct">取消令牌，场景销毁时可传入 this.GetCancellationTokenOnDestroy()</param>
/// <returns>包含请求结果的 UniTask</returns>
    public async UniTask<RequestResult> PostRequestAsync(string url, string jsonData, CancellationToken ct = default)
    {
        using (var request = CreatePostRequest(url, jsonData))
        {
            Debug.Log($"POST Request Async: {url}\nData: {jsonData}");

            try
            {
                await request.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"POST request cancelled: {url}");
                return new RequestResult(false, "", 0, "Request cancelled");
            }

            return ProcessResponse(request);
        }
    }

    /// <summary>
    /// 创建 POST 请求对象
    /// 封装 UnityWebRequest 的创建逻辑，设置请求体、下载处理器和超时
    /// </summary>
    /// <param name="url">请求的完整 URL</param>
    /// <param name="jsonData">要发送的 JSON 数据字符串</param>
    /// <returns>配置好的 UnityWebRequest 对象</returns>
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
    /// 为所有请求添加标准的 HTTP 头部，包括 Content-Type 和 User-Agent
    /// WebGL 平台会跳过某些可能被浏览器阻止的头部
    /// </summary>
    /// <param name="request">要设置头部的 UnityWebRequest 对象</param>
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
    /// 处理 HTTP 响应
    /// 统一处理各种错误类型（连接错误、协议错误、数据处理错误）并记录日志
    /// </summary>
    /// <param name="request">已完成的 UnityWebRequest 对象</param>
    /// <returns>封装好的 RequestResult 对象</returns>
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

    /// <summary>
    /// 构建带查询参数的 URL
    /// 将参数列表拼接到基础 URL 后面，自动进行 URL 编码
    /// 例如：BuildUrlWithQueryParams("http://api.com/users", [{"name", "张三"}]) 
    ///       返回 "http://api.com/users?name=%E5%BC%A0%E4%B8%89"
    /// </summary>
    /// <param name="baseUrl">基础 URL（不含查询参数）</param>
    /// <param name="parameters">查询参数列表</param>
    /// <returns>拼接好的完整 URL</returns>
    public static string BuildUrlWithQueryParams(string baseUrl, List<KeyValuePair<string, string>> parameters)
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
    /// 在 WebGL 平台中始终返回 true（因为浏览器环境本身就需要网络）
    /// 在其他平台检查实际的网络连接状态
    /// </summary>
    /// <returns>true 表示网络可用，false 表示无网络连接</returns>
    public static bool IsNetworkReachable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 中总是假设有网络连接
        return true;
#else
        return Application.internetReachability != NetworkReachability.NotReachable;
#endif
    }

    


}
