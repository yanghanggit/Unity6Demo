using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// HomeAdvance API 客户端，用于处理家园游戏推进玩法的请求
/// 负责发送家园系统的推进请求，包含用户名、游戏名和角色列表
/// </summary>
public class HomeAdvanceApi : BaseApiClient
{
    /// <summary>
    /// HTTP 请求的结果对象，包含响应状态和数据
    /// </summary>
    private RequestResult _requestResult;

    /// <summary>
    /// 获取请求结果
    /// </summary>
    public override RequestResult ReqResult => _requestResult;

    /// <summary>
    /// 解析后的家园推进响应数据
    /// </summary>
    private HomeAdvanceResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public HomeAdvanceResponse RespData => _responseData;

    /// <summary>
    /// 调用家园推进 API，发送推进请求并处理响应
    /// </summary>
    /// <param name="url">API 请求的目标 URL</param>
    /// <param name="userName">发起请求的用户名</param>
    /// <param name="gameName">游戏名称标识</param>
    /// <param name="actors">参与推进的角色名称列表</param>
    /// <returns>协程枚举器，用于异步执行请求</returns>
    public async UniTask Call(string url, string userName, string gameName, List<string> actors)
    {
        // 记录请求信息
        Debug.Log("Starting HomeAdvanceApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"UserName: {userName}");
        Debug.Log($"GameName: {gameName}");
        Debug.Log($"Actors: {JsonConvert.SerializeObject(actors)}");

        // 清除请求状态
        _requestResult = null;
        _responseData = null;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return;
        }

        // 创建请求数据
        var requestData = new HomeAdvanceRequest
        {
            user_name = userName,
            game_name = gameName,
            actors = actors
        };
        var jsonData = JsonConvert.SerializeObject(requestData);

        // 发送请求
        _requestResult = await PostRequestAsync(url, jsonData);
        if (!_requestResult.isSuccess)
        {
            Debug.LogError($"Request failed: {_requestResult.error}");
            return;
        }

        try
        {
            _responseData = JsonConvert.DeserializeObject<HomeAdvanceResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return;
            }

            Debug.Log("Home gameplay successful");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }
}
