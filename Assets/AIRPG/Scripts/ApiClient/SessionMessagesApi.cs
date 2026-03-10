using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// SessionMessages API 客户端，用于获取会话消息
/// </summary>
public class SessionMessagesApi : BaseApiClient
{
    /// <summary>
    /// 请求结果
    /// </summary>
    private RequestResult _requestResult;

    /// <summary>
    /// 获取请求结果,目前故意不写，始终返回 null
    /// </summary>
    public override RequestResult ReqResult => _requestResult;

    /// <summary>
    /// 响应数据
    /// </summary>
    private SessionMessageResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public SessionMessageResponse RespData => _responseData;

    /// <summary>
    /// 构建包含查询参数的请求 URL
    /// </summary>
    /// <param name="lastSequenceId">上次请求的序列 ID</param>
    /// <returns>完整的请求 URL</returns>
    private string BuildRequestUrl(string url, int lastSequenceId)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("last_sequence_id", lastSequenceId.ToString())
        };
        return BuildUrlWithQueryParams(url, parameters);
    }

    /// <summary>
    /// 调用获取会话消息 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="requestLastSequenceId">上次请求的序列 ID</param>
    /// <returns>协程枚举器</returns>
    public async UniTask Call(string url, string userName, string gameName, int requestLastSequenceId)
    {
        // 记录请求信息
        Debug.Log("Starting SessionMessagesApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"UserName: {userName}");
        Debug.Log($"GameName: {gameName}");
        Debug.Log($"RequestLastSequenceId: {requestLastSequenceId}");
        var buildRequestUrl = BuildRequestUrl(url, requestLastSequenceId);
        Debug.Log($"Sending request to URL: {buildRequestUrl}");

        // 清除请求状态
        _requestResult = null;
        _responseData = null;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return;
        }

        // 发送请求
        _requestResult = await GetRequestAsync(buildRequestUrl);

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
            _responseData = JsonConvert.DeserializeObject<SessionMessageResponse>(_requestResult.responseText);

            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return;
            }

            Debug.Log("Session messages loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

}
