using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// SessionMessages API 客户端，用于获取会话消息
/// </summary>
public class SessionMessagesApi : BaseApiClient
{
    /// <summary>
    /// 基础请求 URL
    /// </summary>
    private string _url;

    /// <summary>
    /// 用户名
    /// </summary>
    private string _userName;

    /// <summary>
    /// 游戏名
    /// </summary>
    private string _gameName;

    /// <summary>
    /// 请求的上次序列 ID
    /// </summary>
    private int _requestLastSequenceId;

    /// <summary>
    /// 获取响应中的最后一条消息的序列 ID
    /// </summary>
    public int RespLastSequenceId
    {
        get
        {
            if (_responseData != null && _responseData.session_messages != null && _responseData.session_messages.Count > 0)
            {
                var lastMessage = _responseData.session_messages[^1];
                return lastMessage.sequence_id;
            }
            return -1;
        }
    }

    /// <summary>
    /// 包含查询参数的完整请求 URL
    /// </summary>
    private string _requestUrl;

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
    /// 初始化会话消息请求
    /// </summary>
    /// <param name="url">基础请求 URL</param>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="requestLastSequenceId">上次请求的序列 ID</param>
    private void Initialize(string url, string userName, string gameName, int requestLastSequenceId)
    {
        _url = url;
        _userName = userName;
        _gameName = gameName;
        _requestLastSequenceId = requestLastSequenceId;
        _requestResult = null;
        _responseData = null;
        _requestUrl = BuildRequestUrl(requestLastSequenceId);

        Debug.Log($"SessionMessagesApi initialized with URL: {_url}, UserName: {_userName}, GameName: {_gameName}, LastSequenceId: {_requestLastSequenceId}");
        Debug.Log($"Request URL: {_requestUrl}");
    }

    /// <summary>
    /// 构建包含查询参数的请求 URL
    /// </summary>
    /// <param name="lastSequenceId">上次请求的序列 ID</param>
    /// <returns>完整的请求 URL</returns>
    private string BuildRequestUrl(int lastSequenceId)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("last_sequence_id", lastSequenceId.ToString())
        };
        return BuildUrlWithQueryParams(_url, parameters);
    }

    /// <summary>
    /// 调用获取会话消息 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="requestLastSequenceId">上次请求的序列 ID</param>
    /// <returns>协程枚举器</returns>
    public IEnumerator Call(string url, string userName, string gameName, int requestLastSequenceId)
    {
        Initialize(url, userName, gameName, requestLastSequenceId);

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
            _responseData = JsonConvert.DeserializeObject<SessionMessageResponse>(_requestResult.responseText);

            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                yield break;
            }

            Debug.Log("Session messages loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

}
