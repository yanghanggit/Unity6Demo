using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// 家园玩家行为 API 客户端，用于处理玩家在家园中的行为动作请求
/// </summary>
public class HomePlayerActionApi : BaseApiClient
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
    private HomePlayerActionResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public HomePlayerActionResponse RespData => _responseData;

    /// <summary>
    /// 调用家园玩家行为 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="action">玩家行为动作</param>
    /// <param name="arguments">行为参数字典</param>
    /// <returns>协程枚举器</returns>
    public IEnumerator Call(string url, string userName, string gameName, string action, Dictionary<string, string> arguments)
    {
        // 记录请求信息
        Debug.Log("Starting HomePlayerActionApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"UserName: {userName}");
        Debug.Log($"GameName: {gameName}");
        Debug.Log($"Action: {action}");
        Debug.Log($"Arguments: {JsonConvert.SerializeObject(arguments)}");

        // 初始化请求状态
        _requestResult = null;
        _responseData = null;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            yield break;
        }

        // 创建请求数据
        var requestData = new HomePlayerActionRequest
        {
            user_name = userName,
            game_name = gameName,
            action = action,
            arguments = arguments
        };
        var jsonData = JsonConvert.SerializeObject(requestData);

        // 发送请求
        var task = PostRequestAsync(url, jsonData);
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
            _responseData = JsonConvert.DeserializeObject<HomePlayerActionResponse>(_requestResult.responseText);

            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                yield break;
            }

            Debug.Log("Home player action request successful");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }
}
