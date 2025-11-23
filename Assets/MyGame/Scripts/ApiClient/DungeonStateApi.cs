using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

/// <summary>
/// DungeonState API 客户端，用于获取地下城状态信息
/// </summary>
public class DungeonStateApi : BaseApiClient
{
    /// <summary>
    /// 请求 URL
    /// </summary>
    private string _url;

    /// <summary>
    /// 响应数据
    /// </summary>
    private DungeonStateResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public DungeonStateResponse RespData => _responseData;

    /// <summary>
    /// 初始化地下城状态请求
    /// </summary>
    /// <param name="url">请求 URL</param>
    private void Initialize(string url)
    {
        _url = url;
        _responseData = null;
        Debug.Log($"DungeonStateApi initialized with URL: {_url}");
    }

    /// <summary>
    /// 调用获取地下城状态 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <returns>协程枚举器</returns>
    public IEnumerator Call(string url)
    {
        Initialize(url);

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            yield break;
        }

        // 发送请求
        var task = GetRequestAsync(_url);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            Debug.LogError($"Request exception: {task.Exception?.GetBaseException().Message}");
            yield break;
        }

        var result = task.Result;

        // 处理请求结果
        if (!result.isSuccess)
        {
            Debug.LogError($"Request failed: {result.error}");
            yield break;
        }

        // 解析响应数据
        if (string.IsNullOrEmpty(result.responseText))
        {
            Debug.LogError("Response text is empty");
            yield break;
        }

        try
        {
            _responseData = JsonConvert.DeserializeObject<DungeonStateResponse>(result.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                yield break;
            }

            Debug.Log("Dungeon state loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

}
