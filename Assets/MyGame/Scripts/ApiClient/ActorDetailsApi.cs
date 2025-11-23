using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// ActorDetails API 客户端，用于获取角色详细信息
/// </summary>
public class ActorDetailsApi : BaseApiClient
{
    /// <summary>
    /// 基础请求 URL
    /// </summary>
    private string _url;

    /// <summary>
    /// 角色名称列表
    /// </summary>
    private List<string> _actors;

    /// <summary>
    /// 包含查询参数的完整请求 URL
    /// </summary>
    private string _requestUrl;

    /// <summary>
    /// 响应数据
    /// </summary>
    private ActorDetailsResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public ActorDetailsResponse RespData => _responseData;

    /// <summary>
    /// 请求结果
    /// </summary>
    private RequestResult _requestResult;

    /// <summary>
    /// 获取请求结果
    /// </summary>
    public RequestResult ReqResult => _requestResult;

    /// <summary>
    /// 初始化角色详情请求
    /// </summary>
    /// <param name="url">基础请求 URL</param>
    /// <param name="actors">角色名称列表</param>
    private void Initialize(string url, List<string> actors)
    {
        _url = url;
        _actors = actors;
        _responseData = null;
        _requestResult = null;

        Debug.Log($"ActorDetailsApi initialized with URL: {_url} and {actors?.Count ?? 0} actors");
        for (int i = 0; i < actors.Count; i++)
        {
            Debug.Log($"Actor {i}: {actors[i]}");
        }
        _requestUrl = BuildRequestUrl(_actors);
    }

    /// <summary>
    /// 调用获取角色详情 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="actors">角色名称列表</param>
    /// <returns>协程枚举器</returns>
    public IEnumerator Call(string url, List<string> actors)
    {
        if (actors == null || actors.Count == 0)
        {
            Debug.LogWarning("No actors provided for request");
            yield break;
        }

        Initialize(url, actors);

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
            _responseData = JsonConvert.DeserializeObject<ActorDetailsResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                yield break;
            }

            Debug.Log("Actor details loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建包含查询参数的角色请求 URL
    /// </summary>
    /// <param name="actors">角色名称列表</param>
    /// <returns>完整的请求 URL</returns>
    private string BuildRequestUrl(List<string> actors)
    {
        var parameters = new List<KeyValuePair<string, string>>();
        foreach (var actor in actors)
        {
            parameters.Add(new KeyValuePair<string, string>("actors", actor));
        }
        return BuildUrlWithQueryParams(_url, parameters);
    }

}
