using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// DungeonProgress API 客户端，用于处理地下城进度请求
/// </summary>
public class DungeonProgressApi : BaseApiClient
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
    private DungeonProgressResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public DungeonProgressResponse RespData => _responseData;

    /// <summary>
    /// 调用地下城进度 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="action">操作类型</param>
    /// <returns>协程枚举器</returns>
    public async UniTask Call(string url, string userName, string gameName, string action)
    {
        // 记录请求信息
        Debug.Log("Starting DungeonProgressApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"UserName: {userName}");
        Debug.Log($"GameName: {gameName}");
        Debug.Log($"Action: {action}");

        //清除
        _requestResult = null;
        _responseData = null;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return;
        }

        // 创建请求数据
        var requestData = new DungeonProgressRequest
        {
            user_name = userName,
            game_name = gameName,
            action = action
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
            _responseData = JsonConvert.DeserializeObject<DungeonProgressResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

}
