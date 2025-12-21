using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Tasks Status API 客户端，用于查询任务执行状态
/// </summary>
public class TasksStatusApi : BaseApiClient
{
    /// <summary>
    /// 基础请求 URL
    /// </summary>
    private string _url;

    /// <summary>
    /// 任务ID列表
    /// </summary>
    private List<string> _taskIds;

    /// <summary>
    /// 包含查询参数的完整请求 URL
    /// </summary>
    private string _requestUrl;

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
    private TasksStatusResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public TasksStatusResponse RespData => _responseData;

    /// <summary>
    /// 初始化任务状态查询请求
    /// </summary>
    /// <param name="url">基础请求 URL</param>
    /// <param name="taskIds">任务ID列表</param>
    private void Initialize(string url, List<string> taskIds)
    {
        _url = url;
        _taskIds = taskIds;
        _responseData = null;
        _requestResult = null;
        _requestUrl = BuildRequestUrl(_taskIds);
    }

    /// <summary>
    /// 调用任务状态查询 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="taskIds">任务ID列表</param>
    /// <returns>协程枚举器</returns>
    public IEnumerator Call(string url, List<string> taskIds)
    {
        if (taskIds == null || taskIds.Count == 0)
        {
            Debug.LogWarning("No task IDs provided for request");
            yield break;
        }

        Initialize(url, taskIds);

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            yield break;
        }

        // 发送请求
        var task = GetRequestAsync(_requestUrl);
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
            _responseData = JsonConvert.DeserializeObject<TasksStatusResponse>(_requestResult.responseText);
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

    /// <summary>
    /// 构建包含查询参数的任务状态请求 URL
    /// </summary>
    /// <param name="taskIds">任务ID列表</param>
    /// <returns>完整的请求 URL</returns>
    private string BuildRequestUrl(List<string> taskIds)
    {
        var parameters = new List<KeyValuePair<string, string>>();
        foreach (var taskId in taskIds)
        {
            parameters.Add(new KeyValuePair<string, string>("task_ids", taskId));
        }
        return BuildUrlWithQueryParams(_url, parameters);
    }

}
