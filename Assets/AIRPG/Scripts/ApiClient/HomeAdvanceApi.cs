using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// HomeAdvance API 客户端，用于处理家园游戏推进玩法的请求
/// 负责发送家园系统的推进请求，包含用户名、游戏名和角色列表
/// </summary>
public class HomeAdvanceApi : BaseApiClient
{
    /// <summary>
    /// API 请求的目标 URL 地址
    /// </summary>
    private string _url;

    /// <summary>
    /// 用户名
    /// </summary>
    private string _userName;

    /// <summary>
    /// 游戏名称标识
    /// </summary>
    private string _gameName;

    /// <summary>
    /// 参与家园推进的角色列表
    /// </summary>
    private List<string> _actors;

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
    /// 初始化家园推进请求的参数
    /// </summary>
    /// <param name="url">API 请求的目标 URL</param>
    /// <param name="userName">发起请求的用户名</param>
    /// <param name="gameName">游戏名称标识</param>
    /// <param name="actors">参与推进的角色名称列表</param>
    private void Initialize(string url, string userName, string gameName, List<string> actors)
    {
        _url = url;
        _userName = userName;
        _gameName = gameName;
        _actors = actors;
        _requestResult = null;
        _responseData = null;

        Debug.Log($"HomeAdvanceApi initialized with URL: {_url}, UserName: {_userName}, GameName: {_gameName}, Actors: {string.Join(", ", _actors)}");
        //Debug.Log($"Request data: {JsonConvert.SerializeObject(_data)}");
    }



    /// <summary>
    /// 调用家园推进 API，发送推进请求并处理响应
    /// </summary>
    /// <param name="url">API 请求的目标 URL</param>
    /// <param name="userName">发起请求的用户名</param>
    /// <param name="gameName">游戏名称标识</param>
    /// <param name="actors">参与推进的角色名称列表</param>
    /// <returns>协程枚举器，用于异步执行请求</returns>
    public IEnumerator Call(string url, string userName, string gameName, List<string> actors)
    {
        Initialize(url, userName, gameName, actors);

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            yield break;
        }

        // 创建请求数据
        var requestData = new HomeAdvanceRequest
        {
            user_name = _userName,
            game_name = _gameName,
            actors = _actors
        };
        var jsonData = JsonConvert.SerializeObject(requestData);

        // 发送请求
        var task = PostRequestAsync(_url, jsonData);
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
            _responseData = JsonConvert.DeserializeObject<HomeAdvanceResponse>(_requestResult.responseText);

            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                yield break;
            }

            Debug.Log("Home gameplay successful");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }
}
