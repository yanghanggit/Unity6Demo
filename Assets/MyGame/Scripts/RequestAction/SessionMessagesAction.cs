using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// 查看家园操作，使用改进的 BaseRequestAction
/// </summary>
public class SessionMessagesAction : BaseRequestAction
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本

    private SessionMessageResponse _responseData;

    public SessionMessageResponse ResponseData => _responseData;



    private string _url;

    private string _userName;

    private string _gameName;

    private int _lastSequenceId;


    #region 协程版本（兼容现有代码）

    public void Setup(string url, string userName, string gameName, int lastSequenceId)
    {
        _url = url;
        _userName = userName;
        _gameName = gameName;
        _lastSequenceId = lastSequenceId;
        _responseData = null;

        Debug.Log($"WerewolfGameActorDetailsAction initialized with URL: {_url}, UserName: {_userName}, GameName: {_gameName}, LastSequenceId: {_lastSequenceId}");
        Debug.Log($"Full URL: {FullUrl}");
    }

    private string BuildUrl(int lastSequenceId)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("last_sequence_id", lastSequenceId.ToString())
        };
        return BuildUrlWithQueryParams(_url, parameters);
    }

    public string FullUrl => BuildUrl(_lastSequenceId);

    /// <summary>
    /// 查看家园（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine()
    {
        Debug.Log("View home request started");

        //_lastRequestSuccess = false;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for view home");
            yield break;
        }

        bool requestCompleted = false;
        RequestResult result = null;

        // 发送请求
        yield return GetRequestCoroutine(FullUrl, (response) =>
        {
            result = response;
            requestCompleted = true;
        });

        // 等待请求完成
        yield return new WaitUntil(() => requestCompleted);

        // 处理结果
        if (result != null && result.isSuccess)
        {
            if (TryParseViewHomeResponse(result.responseText))
            {
                //_lastRequestSuccess = true;
                Debug.Log("View home successful");
            }
            else
            {
                Debug.LogError("Failed to parse view home response");
            }
        }
        else
        {
            Debug.LogError($"View home failed: {result?.error ?? "Unknown error"}");
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
            var result = await GetRequestAsync(FullUrl);

            // 处理结果
            if (result.isSuccess)
            {
                if (TryParseViewHomeResponse(result.responseText))
                {
                    //_lastRequestSuccess = true;
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
                Debug.LogError($"View home failed: {result.error}");
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
    public IEnumerator Call()
    {
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
    private bool TryParseViewHomeResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("View home response text is empty");
            return false;
        }

        try
        {
            var response = JsonConvert.DeserializeObject<SessionMessageResponse>(responseText);

            if (response == null)
            {
                Debug.LogError("ViewHomeAction response is null");
                return false;
            }

            //
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
