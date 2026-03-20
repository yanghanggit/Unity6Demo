using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// HomeEnterDungeon API 客户端，用于处理进入地下城请求
/// </summary>
public class HomeEnterDungeonApi : BaseApiClient
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
    private HomeEnterDungeonResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public HomeEnterDungeonResponse RespData => _responseData;

    /// <summary>
    /// 调用进入地下城 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="user">用户名</param>
    /// <param name="game">游戏名</param>
    /// <param name="dungeon">地下城名</param>
    /// <returns>异步任务</returns>
    public async UniTask Call(string url, string user, string game, string dungeon)
    {
        // 记录请求信息
        Debug.Log("Starting HomeEnterDungeonApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"User: {user}");
        Debug.Log($"Game: {game}");
        Debug.Log($"Dungeon: {dungeon}");

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
        var requestData = new HomeEnterDungeonRequest
        {
            user_name = user,
            game_name = game,
            dungeon_name = dungeon
        };
        var jsonData = JsonConvert.SerializeObject(requestData);

        // 发送请求
        _requestResult = await PostRequestAsync(url, jsonData);

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
            _responseData = JsonConvert.DeserializeObject<HomeEnterDungeonResponse>(_requestResult.responseText);

            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return;
            }

            Debug.Log($"Enter dungeon successful. Message: {_responseData.message}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

}
