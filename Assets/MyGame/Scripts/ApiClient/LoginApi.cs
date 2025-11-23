using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

/// <summary>
/// Login API 客户端，用于处理用户登录请求
/// </summary>
public class LoginApi : BaseApiClient
{
    /// <summary>
    /// 请求 URL
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
    /// 请求结果
    /// </summary>
    private RequestResult _requestResult;

    /// <summary>
    /// 获取请求结果
    /// </summary>
    public RequestResult ReqResult => _requestResult;

    /// <summary>
    /// 响应数据
    /// </summary>
    private LoginResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public LoginResponse RespData => _responseData;

    /// <summary>
    /// 初始化登录请求
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    private void Initialize(string url, string userName, string gameName)
    {
        _url = url;
        _userName = userName;
        _gameName = gameName;
        _requestResult = null;
        _responseData = null;

        Debug.Log($"LoginApi initialized with URL: {_url}, UserName: {_userName}, GameName: {_gameName}");
    }

    /// <summary>
    /// 调用登录 API
    /// </summary>
    /// <param name="url">登录接口 URL</param>
    /// <param name="user">用户名</param>
    /// <param name="game">游戏名</param>
    /// <returns>协程枚举器</returns>
    public IEnumerator Call(string url, string user, string game)
    {
        Initialize(url, user, game);

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            yield break;
        }

        // 创建请求数据
        var requestData = new LoginRequest { user_name = _userName, game_name = _gameName };
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
        try
        {
            _responseData = JsonConvert.DeserializeObject<LoginResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                yield break;
            }

            Debug.Log($"Login successful. Message: {_responseData.message}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

}
