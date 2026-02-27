using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Tasks Status API 客户端，用于查询任务执行状态
/// </summary>
public class TasksStatusApi : BaseApiClient
{
    /// <summary>
    /// 轮询间隔时间（秒）
    /// </summary>
    [SerializeField] private float _pollInterval = 2.0f;

    /// <summary>
    /// 最大轮询次数
    /// </summary>
    [SerializeField] private int _maxAttempts = 60;

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
    /// 调用任务状态查询 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="taskIds">任务ID列表</param>
    /// <returns>协程枚举器</returns>
    public async UniTask Call(string url, List<string> taskIds)
    {
        // 记录请求信息
        Debug.Log("Starting TasksStatusApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"TaskIds: {JsonConvert.SerializeObject(taskIds)}");

        if (taskIds == null || taskIds.Count == 0)
        {
            Debug.LogWarning("No task IDs provided for request");
            return;
        }

        // 构建请求 URL
        var requestUrl = BuildRequestUrl(url, taskIds);
        Debug.Log($"Request URL: {requestUrl}");


        // 清除
        _responseData = null;
        _requestResult = null;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return;
        }

        // 发送请求
        _requestResult = await GetRequestAsync(requestUrl);

        // 处理请求结果
        if (!_requestResult.isSuccess)
        {
            Debug.LogError($"Request failed: {_requestResult.error}");
            return;
        }

        // 解析响应数据
        if (string.IsNullOrEmpty(_requestResult.responseText))
        {
            Debug.LogError("Response text is empty");
            return;
        }

        try
        {
            _responseData = JsonConvert.DeserializeObject<TasksStatusResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return;
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
    private string BuildRequestUrl(string baseUrl, List<string> taskIds)
    {
        var parameters = new List<KeyValuePair<string, string>>();
        foreach (var taskId in taskIds)
        {
            parameters.Add(new KeyValuePair<string, string>("task_ids", taskId));
        }
        return BuildUrlWithQueryParams(baseUrl, parameters);
    }

    /// <summary>
    /// 根据任务ID获取对应的任务记录
    /// </summary>
    /// <param name="taskId">要查找的任务ID</param>
    /// <returns>匹配的任务记录，如果未找到则返回null</returns>
    public TaskRecord GetTaskRecord(string taskId)
    {
        if (_responseData == null || _responseData.tasks == null)
        {
            return null;
        }

        return _responseData.tasks.Find(t => t.task_id == taskId);
    }

    /// <summary>
    /// 轮询查询任务状态直到完成、失败或超时
    /// 封装了完整的轮询逻辑，包括重试、状态检查和错误处理
    /// </summary>
    /// <param name="url">任务状态查询URL</param>
    /// <param name="taskId">要查询的任务ID</param>
    /// <param name="pollInterval">轮询间隔时间（秒），如果为null则使用成员变量_pollInterval的值</param>
    /// <param name="maxAttempts">最大轮询次数，如果为null则使用成员变量_maxAttempts的值</param>
    /// <returns>任务记录，失败或超时时返回 null</returns>
    public async UniTask<TaskRecord> PollTaskStatus(
        string url,
        string taskId,
        float? pollInterval = null,
        int? maxAttempts = null)
    {
        if (string.IsNullOrEmpty(taskId))
        {
            Debug.LogError("[TasksStatusApi] PollTaskStatus: taskId is null or empty");
            return null;
        }

        float actualPollInterval = pollInterval ?? _pollInterval;
        int actualMaxAttempts = maxAttempts ?? _maxAttempts;

        int attempts = 0;

        while (attempts < actualMaxAttempts)
        {
            attempts++;

            // 等待一段时间再查询
            await UniTask.Delay((int)(actualPollInterval * 1000));

            // 查询任务状态
            List<string> taskIds = new() { taskId };
            await Call(url, taskIds);

            if (_requestResult == null || !_requestResult.isSuccess)
            {
                Debug.LogWarning($"Failed to query task status (attempt {attempts}/{actualMaxAttempts})");
                continue;
            }

            var taskRecord = GetTaskRecord(taskId);
            if (taskRecord == null)
            {
                Debug.LogWarning($"Task record not found for task ID: {taskId} (attempt {attempts}/{actualMaxAttempts})");
                continue;
            }

            Debug.Log($"Task {taskId} status: {taskRecord.status} (attempt {attempts}/{actualMaxAttempts})");

            if (taskRecord.status == TaskStatus.COMPLETED)
            {
                Debug.Log($"Task {taskId} completed successfully");
                return taskRecord;
            }
            else if (taskRecord.status == TaskStatus.FAILED)
            {
                string errorMsg = string.IsNullOrEmpty(taskRecord.error)
                    ? "任务执行失败"
                    : $"任务执行失败: {taskRecord.error}";
                Debug.LogError(errorMsg);
                return null;
            }
            // RUNNING: continue polling
        }

        Debug.LogError($"Task {taskId} polling timeout after {actualMaxAttempts} attempts");
        return null;
    }

}
