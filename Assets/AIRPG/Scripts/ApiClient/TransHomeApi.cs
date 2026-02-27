using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// TransHome API 客户端，用于处理转换到家园请求
/// </summary>
public class TransHomeApi : BaseApiClient
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
    private DungeonTransHomeResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public DungeonTransHomeResponse RespData => _responseData;

    /// <summary>
    /// 调用转换家园 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="user">用户名</param>
    /// <param name="game">游戏名</param>
    /// <returns>协程枚举器</returns>
    public async UniTask Call(string url, string user, string game)
    {
        //Initialize(url, user, game);
        // 记录请求信息
        Debug.Log("Starting TransHomeApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"User: {user}");
        Debug.Log($"Game: {game}");

        // 清除请求状态
        _responseData = null;
        _requestResult = null;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return;
        }

        // 创建请求数据
        var requestData = new DungeonTransHomeRequest
        {
            user_name = user,
            game_name = game
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
            _responseData = JsonConvert.DeserializeObject<DungeonTransHomeResponse>(_requestResult.responseText);

            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return;
            }

            Debug.Log($"Trans to home successful. Message: {_responseData.message}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }
}
