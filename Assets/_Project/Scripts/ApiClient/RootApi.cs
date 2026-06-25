using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;

/// <summary>
/// Root API 客户端，用于获取服务器根配置信息
/// </summary>
public class RootApi : BaseApiClient
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
    /// 响应数据 - 使用 JToken 存储 JSON 响应，支持灵活访问和嵌套对象
    /// </summary>
    private JToken _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public JToken RespData => _responseData;

    /// <summary>
    /// 调用 Root API 获取配置信息
    /// </summary>
    /// <param name="rootUrl">根 URL 地址</param>
    /// <returns>协程枚举器</returns>
    public async UniTask<JToken> Call(string rootUrl)
    {
        // 记录请求信息
        Debug.Log("Starting RootApi call...");
        Debug.Log($"URL: {rootUrl}");

        // 清除请求状态
        _requestResult = null;
        _responseData = null;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return null;
        }

        // 发送请求
        _requestResult = await GetRequestAsync(rootUrl);

        // 处理请求结果
        if (!_requestResult.isSuccess)
        {
            Debug.LogError($"Request failed: {_requestResult.error}");
            return null;
        }

        // 解析响应数据
        if (string.IsNullOrEmpty(_requestResult.responseText))
        {
            Debug.LogError("Response text is empty");
            return null;
        }

        try
        {
            _responseData = JToken.Parse(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return null;
            }

            Debug.Log("RootApi call successful, response data parsed");
            return _responseData;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
            return null;
        }
    }

}
