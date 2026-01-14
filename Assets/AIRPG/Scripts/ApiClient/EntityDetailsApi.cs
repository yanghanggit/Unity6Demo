using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// EntityDetailsApi API 客户端，用于获取角色详细信息
/// </summary>
public class EntityDetailsApi : BaseApiClient
{
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
    private EntitiesDetailsResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public EntitiesDetailsResponse RespData => _responseData;

    /// <summary>
    /// 调用获取实体详情 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="actors">实体名称列表</param>
    /// <returns>协程枚举器</returns>
    public IEnumerator Call(string url, List<string> actors)
    {
        if (actors == null || actors.Count == 0)
        {
            Debug.LogWarning("No entities provided for request");
            yield break;
        }

        // 记录请求信息
        Debug.Log("Starting EntityDetailsApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"Entities: {JsonConvert.SerializeObject(actors)}");

        var requestUrl = BuildRequestUrl(url, actors);
        Debug.Log("Starting EntityDetailsApi call...");
        Debug.Log($"Request URL: {requestUrl}");

        // 清除请求状态
        _responseData = null;
        _requestResult = null;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            yield break;
        }

        // 发送请求
        var task = GetRequestAsync(requestUrl);
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
            _responseData = JsonConvert.DeserializeObject<EntitiesDetailsResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                yield break;
            }

            //Debug.Log("Entity details loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建包含查询参数的实体请求 URL
    /// </summary>
    /// <param name="actors">实体名称列表</param>
    /// <returns>完整的请求 URL</returns>
    private string BuildRequestUrl(string baseUrl, List<string> actors)
    {
        var parameters = new List<KeyValuePair<string, string>>();
        foreach (var actor in actors)
        {
            parameters.Add(new KeyValuePair<string, string>("entities", actor));
        }
        return BuildUrlWithQueryParams(baseUrl, parameters);
    }

}
