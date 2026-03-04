using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// DungeonCombat API 客户端，用于获取地下城战斗状态信息
/// </summary>
public class DungeonCombatApi : BaseApiClient
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
    private DungeonCombatResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public DungeonCombatResponse RespData => _responseData;


    /// <summary>
    /// 调用获取地下城战斗状态 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <returns>协程枚举器</returns>
    public async UniTask Call(string url)
    {
        // 记录请求信息
        Debug.Log("Starting DungeonCombatApi call...");
        Debug.Log($"URL: {url}");

        // 清除请求状态
        _requestResult = null;
        _responseData = null;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return;
        }

        // 发送请求
        _requestResult = await GetRequestAsync(url);

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
            _responseData = JsonConvert.DeserializeObject<DungeonCombatResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return;
            }

            Debug.Log("Dungeon combat state loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }
}
