using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

/// <summary>
/// DungeonCombatPlayCards API 客户端，用于处理地下城战斗打牌请求
/// </summary>
public class DungeonCombatPlayCardsApi : BaseApiClient
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
    public override RequestResult ReqResult => _requestResult;

    /// <summary>
    /// 响应数据
    /// </summary>
    private DungeonCombatPlayCardsResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public DungeonCombatPlayCardsResponse RespData => _responseData;

    /// <summary>
    /// 初始化地下城战斗打牌请求
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

        Debug.Log($"DungeonCombatPlayCardsApi initialized with URL: {_url}, UserName: {_userName}, GameName: {_gameName}");
    }

    /// <summary>
    /// 调用地下城战斗打牌 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <returns>协程枚举器</returns>
    public IEnumerator Call(string url, string userName, string gameName)
    {
        Initialize(url, userName, gameName);

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            yield break;
        }

        // 创建请求数据
        var requestData = new DungeonCombatPlayCardsRequest
        {
            user_name = _userName,
            game_name = _gameName
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
            _responseData = JsonConvert.DeserializeObject<DungeonCombatPlayCardsResponse>(_requestResult.responseText);
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

}
