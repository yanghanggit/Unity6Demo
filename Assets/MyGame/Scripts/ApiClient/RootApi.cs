using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

/// <summary>
/// Root API 客户端，用于获取服务器根配置信息
/// </summary>
public class RootApi : BaseApiClient
{
    /// <summary>
    /// 响应数据
    /// </summary>
    private RootResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public RootResponse RespData => _responseData;

    /// <summary>
    /// 请求结果
    /// </summary>
    private RequestResult _requestResult;

    /// <summary>
    /// 获取请求结果
    /// </summary>
    public RequestResult ReqResult => _requestResult;

    /// <summary>
    /// 请求 URL
    /// </summary>
    private string _url;

    /// <summary>
    /// 初始化 API 请求
    /// </summary>
    /// <param name="url">请求的根 URL</param>
    private void Initialize(string url)
    {
        _url = url;
        _requestResult = null;
        _responseData = null;

        Debug.Log($"RootApi initialized with URL: {_url}");
    }


    /// <summary>
    /// 调用 Root API 获取配置信息
    /// </summary>
    /// <param name="rootUrl">根 URL 地址</param>
    /// <returns>协程枚举器</returns>
    public IEnumerator Call(string rootUrl)
    {
        // 初始化
        Initialize(rootUrl);

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            yield break;
        }

        // 发送请求
        var task = GetRequestAsync(_url);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            Debug.LogError($"Request exception: {task.Exception?.GetBaseException().Message}");
            yield break;
        }

        _requestResult = task.Result;

        // 处理请求结果
        if (!_requestResult.isSuccess)
        {
            Debug.LogError($"Request failed: {_requestResult.error}");
            yield break;
        }

        // 解析响应数据
        if (string.IsNullOrEmpty(_requestResult.responseText))
        {
            Debug.LogError("Response text is empty");
            yield break;
        }

        try
        {
            _responseData = JsonConvert.DeserializeObject<RootResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                yield break;
            }

            Debug.Log($"Root configuration loaded successfully. API Version: {_responseData.version}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

}
