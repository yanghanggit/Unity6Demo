using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// DungeonCombatRetreat API 客户端，用于处理战斗撤退请求
/// </summary>
public class DungeonCombatRetreatApi : BaseApiClient
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
    private DungeonCombatRetreatResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public DungeonCombatRetreatResponse RespData => _responseData;

    /// <summary>
    /// 调用战斗撤退 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <returns>异步任务</returns>
    public async UniTask Call(string url, string userName, string gameName)
    {
        // 记录请求信息
        Debug.Log("Starting DungeonCombatRetreatApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"UserName: {userName}");
        Debug.Log($"GameName: {gameName}");

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
        var requestData = new DungeonCombatRetreatRequest
        {
            user_name = userName,
            game_name = gameName
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
            _responseData = JsonConvert.DeserializeObject<DungeonCombatRetreatResponse>(_requestResult.responseText);
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
