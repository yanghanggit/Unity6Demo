using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// 改进的 URL 配置获取操作，使用新的 ImprovedRequestAction
/// </summary>
public class RootAction : BaseRequestAction
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本

    private RootResponse _responseData;

    public RootResponse ResponseData => _responseData;

    private RequestResult _requestResult = null;

    public RequestResult ReqResult => _requestResult;

    private string _url;

    public void Setup(string url)
    {
        _url = url;
        _requestResult = null;
        _responseData = null;

        Debug.Log($"RootAction initialized with URL: {_url}");
    }

    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 获取 URL 配置（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine()
    {
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            yield break;
        }

        bool requestCompleted = false;

        // 发送请求
        yield return GetRequestCoroutine(_url, (response) =>
        {
            _requestResult = response;
            requestCompleted = true;
        });

        // 等待请求完成
        yield return new WaitUntil(() => requestCompleted);

        // 处理结果
        if (_requestResult != null && _requestResult.isSuccess)
        {
            if (TryParseResponse(_requestResult.responseText))
            {
                Debug.Log("URL Configuration loaded successfully");
            }
            else
            {
                Debug.LogError("Failed to parse URL configuration response");
            }
        }
        else
        {
            Debug.LogError($"Failed to get URL configuration: {_requestResult?.error ?? "Unknown error"}");
        }
    }

    #endregion

    #region Async 版本（推荐用于 Unity 6）

    /// <summary>
    /// 获取 URL 配置（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync()
    {
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return false;
        }

        try
        {
            // 发送请求
            _requestResult = await GetRequestAsync(_url);

            // 处理结果
            if (_requestResult.isSuccess)
            {
                if (TryParseResponse(_requestResult.responseText))
                {
                    Debug.Log("URL Configuration loaded successfully");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse URL configuration response");
                }
            }
            else
            {
                Debug.LogError($"Failed to get URL configuration: {_requestResult.error}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during URL configuration request: {ex.Message}");
        }

        return false;
    }

    #endregion

    #region 通用调用方法

    /// <summary>
    /// 统一的调用接口，根据配置选择协程或 Async 版本
    /// </summary>
    public IEnumerator Call(string rootUrl)
    {
        // 初始化
        Setup(rootUrl);

        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Async call failed: {task.Exception?.GetBaseException().Message}");
            }
        }
        else
        {
            // 使用协程版本
            yield return CallCoroutine();
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 尝试解析响应数据
    /// </summary>
    private bool TryParseResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("Response text is empty");
            return false;
        }

        try
        {
            _responseData = JsonConvert.DeserializeObject<RootResponse>(responseText);

            if (_responseData == null)
            {
                Debug.LogError("Deserialized URL configuration is null");
                return false;
            }

            // 验证必要字段
            if (string.IsNullOrEmpty(_responseData.version))
            {
                Debug.LogError("API version is missing in URL configuration");
                return false;
            }

            Debug.Log($"URL Configuration parsed successfully. API Version: {_responseData.version}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse URL configuration: {ex.Message}");
            return false;
        }
    }

    #endregion
}
