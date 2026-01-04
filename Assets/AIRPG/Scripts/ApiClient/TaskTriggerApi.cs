using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

/// <summary>
/// Task Trigger API 客户端，用于触发异步任务执行
/// </summary>
public class TaskTriggerApi : BaseApiClient
{
    /// <summary>
    /// 请求 URL
    /// </summary>
    private string _url;

    /// <summary>
    /// 请求结果
    /// </summary>
    private RequestResult _requestResult;

    /// <summary>
    /// 获取请求结果
    /// </summary>
    public override RequestResult ReqResult => _requestResult;

    /// <summary>
    /// 响应数据
    /// </summary>
    private TaskTriggerResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public TaskTriggerResponse RespData => _responseData;

    /// <summary>
    /// 初始化任务触发请求
    /// </summary>
    /// <param name="url">请求 URL</param>
    private void Initialize(string url)
    {
        _url = url;
        _requestResult = null;
        _responseData = null;
    }

    /// <summary>
    /// 调用任务触发 API
    /// </summary>
    /// <param name="url">任务触发接口 URL</param>
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
        var task = PostRequestAsync(_url, null);
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
        try
        {
            _responseData = JsonConvert.DeserializeObject<TaskTriggerResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                yield break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

}
