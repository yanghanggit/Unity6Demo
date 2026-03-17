using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// DungeonRoom API 客户端，用于获取当前地下城房间状态信息
/// </summary>
public class DungeonRoomApi : BaseApiClient
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
    private DungeonRoomResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public DungeonRoomResponse RespData => _responseData;

    /// <summary>
    /// 调用获取地下城房间状态 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    public async UniTask Call(string url)
    {
        Debug.Log("Starting DungeonRoomApi call...");
        Debug.Log($"URL: {url}");

        _requestResult = null;
        _responseData = null;

        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return;
        }

        _requestResult = await GetRequestAsync(url);

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
            _responseData = JsonConvert.DeserializeObject<DungeonRoomResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return;
            }

            Debug.Log("Dungeon room state loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }
}
