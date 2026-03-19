using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// GetBlueprints API 客户端，用于获取所有蓝图配置
/// </summary>
public class GetBlueprintsApi : BaseApiClient
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
    private BlueprintsResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public BlueprintsResponse RespData => _responseData;

    /// <summary>
    /// 调用获取蓝图列表 API
    /// </summary>
    /// <param name="url">接口 URL</param>
    public async UniTask Call(string url)
    {
        Debug.Log("Starting GetBlueprintsApi call...");
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

        // 发送 GET 请求
        _requestResult = await GetRequestAsync(url);

        // 处理请求结果
        if (!_requestResult.isSuccess)
        {
            Debug.LogError($"Request failed: {_requestResult.error}");
            return;
        }

        // 解析响应数据
        try
        {
            _responseData = JsonConvert.DeserializeObject<BlueprintsResponse>(_requestResult.responseText);
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
