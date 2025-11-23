using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// 查看地下城操作，使用改进的 BaseRequestAction
/// </summary>
public class DungeonStateAction : BaseApiClient
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本

    private string _url;

    private DungeonStateResponse _responseData;

    public DungeonStateResponse ResponseData => _responseData;

    public void Setup(string url)
    {
        _url = url;
        _responseData = null;
        Debug.Log($"DungeonStateAction URL set to: {_url}");
    }


    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 查看地下城（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine()
    {
        Debug.Log("View dungeon request started");

        //_lastRequestSuccess = false;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for view dungeon");
            yield break;
        }

        bool requestCompleted = false;
        RequestResult result = null;

        // 发送请求
        yield return GetRequestCoroutine(_url, (response) =>
        {
            result = response;
            requestCompleted = true;
        });

        // 等待请求完成
        yield return new WaitUntil(() => requestCompleted);

        // 处理结果
        if (result != null && result.isSuccess)
        {
            if (TryParseResponse(result.responseText))
            {
                //_lastRequestSuccess = true;
                Debug.Log("View dungeon successful");
            }
            else
            {
                Debug.LogError("Failed to parse view dungeon response");
            }
        }
        else
        {
            Debug.LogError($"View dungeon failed: {result?.error ?? "Unknown error"}");
        }
    }

    #endregion

    #region Async 版本（推荐用于 Unity 6）

    /// <summary>
    /// 查看地下城（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync()
    {
        Debug.Log("View dungeon request async started");

        //_lastRequestSuccess = false;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for view dungeon");
            return false;
        }

        try
        {
            // 发送请求
            var result = await GetRequestAsync(_url);

            // 处理结果
            if (result.isSuccess)
            {
                if (TryParseResponse(result.responseText))
                {
                    // _lastRequestSuccess = true;
                    Debug.Log("View dungeon successful");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse view dungeon response");
                }
            }
            else
            {
                Debug.LogError($"View dungeon failed: {result.error}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during view dungeon request: {ex.Message}");
        }

        return false;
    }

    #endregion

    #region 通用调用方法

    /// <summary>
    /// 统一的调用接口，根据配置选择协程或 Async 版本
    /// </summary>
    public IEnumerator Call(string url)
    {
        Setup(url);

        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Async view dungeon call failed: {task.Exception?.GetBaseException().Message}");
            }
        }
        else
        {
            // 使用协程版本
            yield return CallCoroutine();
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 尝试解析查看地下城响应数据
    /// </summary>
    private bool TryParseResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("View dungeon response text is empty");
            return false;
        }

        try
        {
            var response = JsonConvert.DeserializeObject<DungeonStateResponse>(responseText);
            if (response == null)
            {
                Debug.LogError("ViewDungeonAction response is null");
                return false;
            }

            _responseData = response;
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse view dungeon response: {ex.Message}");
            return false;
        }
    }

    #endregion
}
