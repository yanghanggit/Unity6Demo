using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

/// <summary>
/// StagesState API 客户端，用于获取关卡状态信息
/// </summary>
public class StagesStateApi : BaseApiClient
{
    /// <summary>
    /// 请求 URL
    /// </summary>
    private string _url;

    /// <summary>
    /// 响应数据
    /// </summary>
    private StagesStateResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public StagesStateResponse RespData => _responseData;

    /// <summary>
    /// 请求结果
    /// </summary>
    private RequestResult _requestResult;

    /// <summary>
    /// 获取请求结果
    /// </summary>
    public RequestResult ReqResult => _requestResult;


    /// <summary>
    /// 初始化关卡状态请求
    /// </summary>
    /// <param name="url">请求 URL</param>
    private void Initialize(string url)
    {
        _url = url;
        _responseData = null;
        _requestResult = null;
        Debug.Log($"StagesStateApi initialized with URL: {_url}");
    }


    /// <summary>
    /// 调用获取关卡状态 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <returns>协程枚举器</returns>
    public IEnumerator Call(string url)
    {
        Initialize(url);

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
            _responseData = JsonConvert.DeserializeObject<StagesStateResponse>(_requestResult.responseText);

            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                yield break;
            }

            Debug.Log("Stages state loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

}
