using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// HomeGenerateDungeon API 客户端，用于触发家园生成地下城流程
/// </summary>
public class HomeGenerateDungeonApi : BaseApiClient
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
    private HomeGenerateDungeonResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public HomeGenerateDungeonResponse RespData => _responseData;

    /// <summary>
    /// 调用家园生成地下城 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="user">用户名</param>
    /// <param name="game">游戏名</param>
    /// <returns>异步任务</returns>
    public async UniTask Call(string url, string user, string game)
    {
        Debug.Log("Starting HomeGenerateDungeonApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"User: {user}");
        Debug.Log($"Game: {game}");

        _requestResult = null;
        _responseData = null;

        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return;
        }

        var requestData = new HomeGenerateDungeonRequest
        {
            user_name = user,
            game_name = game
        };
        var jsonData = JsonConvert.SerializeObject(requestData);

        _requestResult = await PostRequestAsync(url, jsonData);

        if (!_requestResult.isSuccess)
        {
            Debug.LogError($"Request failed: {_requestResult.error}");
            return;
        }

        if (string.IsNullOrEmpty(_requestResult.responseText))
        {
            Debug.LogError("Response text is empty");
            return;
        }

        try
        {
            _responseData = JsonConvert.DeserializeObject<HomeGenerateDungeonResponse>(_requestResult.responseText);

            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return;
            }

            Debug.Log($"Generate dungeon task started. task_id={_responseData.task_id}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }
}
