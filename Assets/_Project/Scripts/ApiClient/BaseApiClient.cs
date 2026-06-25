using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;

/// <summary>
/// API 请求基类
/// 
/// 为所有具体的 API 客户端类（如 LoginApi、DungeonStateApi 等）提供统一的 HTTP 请求能力。
/// 
/// 主要功能：
/// - 支持 GET / POST 请求
/// - 基于 UniTask 的 async/await 调用方式
/// - 自动资源管理（using 语句）
/// - 完善的错误处理和超时控制
/// - 支持 CancellationToken 取消
/// - WebGL 平台兼容性处理
/// - 统一请求头设置
/// - 失败自动重试（可配置次数、间隔、指数退避）
/// 
/// 使用方式：
/// 1. 继承此类创建具体的 API 客户端类
/// 2. 在子类中调用 GetRequestAsync / PostRequestAsync，返回 UniTask&lt;RequestResult&gt;
/// 3. 处理返回的 RequestResult
/// </summary>
public abstract class BaseApiClient : MonoBehaviour
{
    /// <summary>
    /// 最近一次请求的结果，子类必须实现此属性
    /// </summary>
    public abstract RequestResult ReqResult { get; }


    [Header("请求配置")]
    [SerializeField]
    [Tooltip("HTTP 请求超时时间（秒），默认 30 秒")]
    private float requestTimeout = 30.0f;

    [SerializeField]
    [Tooltip("请求失败后的最大重试次数（不含首次请求），0 表示不重试")]
    private int maxRetryCount = 3;

    [SerializeField]
    [Tooltip("首次重试前的等待时间（秒）")]
    private float retryDelaySeconds = 1.0f;

    [SerializeField]
    [Tooltip("是否启用指数退避：每次重试等待时间翻倍（1s → 2s → 4s …）")]
    private bool useExponentialBackoff = true;


    /// <summary>
    /// 封装单次 HTTP 请求的返回结果，包括成功标志、响应内容、状态码和错误信息
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

        /// <summary>实际发生的重试次数（成功或最终失败时记录）</summary>
        public int retryCount;

        /// <summary>
        /// </summary>
        /// <param name="success">请求是否成功</param>
        /// <param name="response">响应文本</param>
        /// <param name="code">HTTP 状态码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <param name="retries">实际重试次数</param>
        public RequestResult(bool success, string response = "", long code = 0, string errorMsg = "", int retries = 0)
        {
            isSuccess = success;
            responseText = response;
            responseCode = code;
            error = errorMsg;
            retryCount = retries;
        }
    }

    /// <summary>
    /// 发送 GET 请求（含自动重试）
    /// </summary>
    /// <param name="url">请求的完整 URL</param>
    /// <param name="ct">取消令牌，可传入 this.GetCancellationTokenOnDestroy() 以在场景销毁时自动取消</param>
    /// <returns>请求结果</returns>
    public UniTask<RequestResult> GetRequestAsync(string url, CancellationToken ct = default)
    {
        return ExecuteWithRetryAsync(() =>
        {
            var request = UnityWebRequest.Get(url);
            request.timeout = (int)requestTimeout;
            SetCommonHeaders(request);
            Debug.Log($"GET Request Async: {url}");
            return (request, $"GET {url}");
        }, ct);
    }

    /// <summary>
    /// 发送 POST 请求（含自动重试）
    /// </summary>
    /// <param name="url">请求的完整 URL</param>
    /// <param name="jsonData">要发送的 JSON 数据字符串</param>
    /// <param name="ct">取消令牌，可传入 this.GetCancellationTokenOnDestroy() 以在场景销毁时自动取消</param>
    /// <returns>请求结果</returns>
    public UniTask<RequestResult> PostRequestAsync(string url, string jsonData, CancellationToken ct = default)
    {
        return ExecuteWithRetryAsync(() =>
        {
            var request = CreatePostRequest(url, jsonData);
            Debug.Log($"POST Request Async: {url}\nData: {jsonData}");
            return (request, $"POST {url}");
        }, ct);
    }

    /// <summary>
    /// 判断某次请求结果是否应该重试
    /// - ConnectionError / DataProcessingError：网络层问题，可重试
    /// - HTTP 5xx：服务端临时故障，可重试
    /// - HTTP 429（Too Many Requests）：限流，可重试
    /// - HTTP 4xx（非429）/ 用户取消：不重试
    /// </summary>
    private bool ShouldRetry(RequestResult result)
    {
        if (result.isSuccess) return false;
        if (result.error == "Request cancelled") return false;

        // ConnectionError / DataProcessingError 通过 responseCode == 0 且有错误信息判断
        // ProtocolError 则看 HTTP 状态码决定
        if (result.responseCode == 0) return true;          // 网络层错误
        if (result.responseCode == 429) return true;        // 限流
        if (result.responseCode >= 500) return true;        // 服务端错误

        return false;
    }

    /// <summary>
    /// 核心重试执行器：按配置自动重试请求
    /// </summary>
    /// <param name="requestFactory">
    /// 每次尝试时调用，返回一个全新的 UnityWebRequest 和用于日志的标签。
    /// 每次重试必须创建新的 request 对象，不可复用。
    /// </param>
    /// <param name="ct">取消令牌</param>
    private async UniTask<RequestResult> ExecuteWithRetryAsync(
        Func<(UnityWebRequest request, string label)> requestFactory,
        CancellationToken ct)
    {
        int attempt = 0;
        float delay = retryDelaySeconds;
        RequestResult lastResult = null;

        while (true)
        {
            var (request, label) = requestFactory();

            using (request)
            {
                try
                {
                    await request.SendWebRequest().ToUniTask(cancellationToken: ct);
                }
                catch (OperationCanceledException)
                {
                    Debug.LogWarning($"{label} cancelled.");
                    return new RequestResult(false, "", 0, "Request cancelled", attempt);
                }
                catch (UnityWebRequestException)
                {
                    // 4xx / 5xx：请求已完成但服务端返回错误状态码
                    // ToUniTask() 会对非 2xx 抛出此异常，交由 ProcessResponse 统一处理
                }

                lastResult = ProcessResponse(request);
                lastResult.retryCount = attempt;
            }

            if (lastResult.isSuccess) return lastResult;
            if (!ShouldRetry(lastResult) || attempt >= maxRetryCount) return lastResult;

            attempt++;
            Debug.LogWarning($"{requestFactory().label} failed (attempt {attempt}/{maxRetryCount}), "
                + $"retrying in {delay:F1}s... Error: {lastResult.error}");

            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct);

            if (useExponentialBackoff)
                delay *= 2f;
        }
    }

    /// <summary>
    /// 创建配置好的 POST 请求对象，设置请求体、下载处理器和超时
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
    /// 为请求设置通用 HTTP 头部（Content-Type、Accept、User-Agent）
    /// WebGL 平台下跳过 User-Agent，避免浏览器拦截
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
    /// 处理 HTTP 响应，将 UnityWebRequest 结果转换为 RequestResult
    /// 依次处理连接错误、协议错误、数据处理错误及成功四种分支
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
    /// 将键値对参数列表拼接到基础 URL 后面，自动进行 URL 编码
    /// <example>
    /// BuildUrlWithQueryParams("http://api.com/users", [("name", "张三")])
    /// 返回 "http://api.com/users?name=%E5%BC%A0%E4%B8%89"
    /// </example>
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
    /// 尝试从最近一次失败请求中提取 FastAPI 错误信息。
    /// 服务端以 HTTPException 响应时，响应体格式为 {"detail": "..."}。
    /// </summary>
    /// <param name="httpStatusCode">输出 HTTP 状态码，成功时为 0</param>
    /// <param name="detail">输出 detail 字段内容，无法解析时为 null</param>
    /// <returns>true 表示成功提取到 detail；false 表示请求成功或无可用信息</returns>
    public bool TryGetErrorDetail(out long httpStatusCode, out string detail)
    {
        httpStatusCode = 0;
        detail = null;

        var result = ReqResult;
        if (result == null || result.isSuccess)
            return false;

        httpStatusCode = result.responseCode;

        if (string.IsNullOrEmpty(result.responseText))
            return false;

        try
        {
            var json = JObject.Parse(result.responseText);
            detail = json["detail"]?.ToString();
            return detail != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查网络可达性
    /// WebGL 平台始终返回 true（浏览器环境本身需要网络），其他平台检查实际连接状态
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


