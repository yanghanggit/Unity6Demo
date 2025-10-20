using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// 查看家园操作，使用改进的 BaseRequestAction
/// </summary>
public class WerewolfGameStateAction : BaseRequestAction
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本

    // 响应数据
    private WerewolfGameStateResponse _responseData = null;

    private RequestResult _requestResult = null;

    public WerewolfGameStateResponse ResponseData => _responseData;

    public RequestResult ReqResult => _requestResult;

    private string _url;
    private string _userName;
    private string _gameName;

    public void Setup(string url, string userName, string gameName)
    {
        _url = url;
        _userName = userName;
        _gameName = gameName;
        _requestResult = null;
        _responseData = null;

        Debug.Log($"WerewolfGameStateAction initialized with URL: {_url}, UserName: {_userName}, GameName: {_gameName}");
    }

    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 查看家园（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine()
    {
        Debug.Log("View home request started");

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for view home");
            yield break;
        }

        bool requestCompleted = false;

        // 发送请求
        yield return GetRequestCoroutine(_url, (response) =>
        {
            _requestResult = response;
            requestCompleted = true;
        });

        // 等待请求完成
        yield return new WaitUntil(() => requestCompleted);

        // 处理结果
        if (_requestResult != null && _requestResult.isSuccess)
        {
            if (TryParseResponse(_requestResult.responseText))
            {
                Debug.Log("View home successful");
            }
            else
            {
                Debug.LogError("Failed to parse view home response");
            }
        }
        else
        {
            Debug.LogError($"View home failed: {_requestResult?.error ?? "Unknown error"}");
        }
    }

    #endregion

    #region Async 版本（推荐用于 Unity 6）

    /// <summary>
    /// 查看家园（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync()
    {
        Debug.Log("View home request async started");

        //_lastRequestSuccess = false;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for view home");
            return false;
        }

        try
        {
            // 发送请求
            _requestResult = await GetRequestAsync(_url);

            // 处理结果
            if (_requestResult.isSuccess)
            {
                if (TryParseResponse(_requestResult.responseText))
                {
                    Debug.Log("View home successful");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse view home response");
                }
            }
            else
            {
                Debug.LogError($"View home failed: {_requestResult.error}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during view home request: {ex.Message}");
        }

        return false;
    }

    #endregion

    #region 通用调用方法

    /// <summary>
    /// 统一的调用接口，根据配置选择协程或 Async 版本
    /// </summary>
    public IEnumerator Call(string url, string userName, string gameName)
    {
        Setup(url, userName, gameName);

        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Async view home call failed: {task.Exception?.GetBaseException().Message}");
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
    /// 尝试解析查看家园响应数据
    /// </summary>
    private bool TryParseResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("View home response text is empty");
            return false;
        }

        try
        {
            var response = JsonConvert.DeserializeObject<WerewolfGameStateResponse>(responseText);

            if (response == null)
            {
                Debug.LogError("ViewHomeAction response is null");
                return false;
            }

            // 赋值响应数据
            _responseData = response;
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse view home response: {ex.Message}");
            return false;
        }
    }

    #endregion
}
