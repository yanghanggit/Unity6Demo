using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// 查看角色操作，使用改进的 BaseRequestAction
/// </summary>
public class ActorDetailsAction : BaseApiClient
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本
    private string _url;
    private List<string> _actors;
    private ActorDetailsResponse _responseData;
    public ActorDetailsResponse ResponseData => _responseData;
    private RequestResult _requestResult = null;
    public RequestResult ReqResult => _requestResult;

    public void Setup(string url, List<string> actors)
    {
        _url = url;
        _actors = actors;
        _responseData = null;
        _requestResult = null;
        
        Debug.Log($"ActorDetailsAction setup with URL: {_url} and {actors?.Count ?? 0} actors");
        for (int i = 0; i < actors.Count; i++)
        {
            Debug.Log($"Actor {i}: {actors[i]}");
        }
    }


    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 查看角色（协程版本）
    /// </summary>
    private IEnumerator CallCoroutine()
    {
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for view actor");
            yield break;
        }

        // 构建完整URL
        string fullUrl = BuildUrl(_actors);

        bool requestCompleted = false;

        // 发送请求
        yield return GetRequestCoroutine(fullUrl, (response) =>
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
                //_lastRequestSuccess = true;
                Debug.Log("View actor successful");
            }
            else
            {
                Debug.LogError("Failed to parse view actor response");
            }
        }
        else
        {
            Debug.LogError($"View actor failed: {_requestResult?.error ?? "Unknown error"}");
        }
    }

    #endregion

    #region Async 版本（推荐用于 Unity 6）

    /// <summary>
    /// 查看角色（Async 版本）
    /// </summary>
    private async Task<bool> CallAsync()
    {
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for view actor");
            return false;
        }

        try
        {
            // 构建完整URL
            string fullUrl = BuildUrl(_actors);
            Debug.Log($"View actor request URL: {fullUrl}");

            // 发送请求
            _requestResult = await GetRequestAsync(fullUrl);

            // 处理结果
            if (_requestResult.isSuccess)
            {
                if (TryParseResponse(_requestResult.responseText))
                {
                    Debug.Log("View actor successful");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse view actor response");
                }
            }
            else
            {
                Debug.LogError($"View actor failed: {_requestResult.error}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during view actor request: {ex.Message}");
        }

        return false;
    }

    #endregion

    #region 通用调用方法

    /// <summary>
    /// 统一的调用接口，根据配置选择协程或 Async 版本
    /// </summary>
    public IEnumerator Call(string url, List<string> actors)
    {

        Setup(url, actors);

        if (actors == null || actors.Count == 0)
        {
            Debug.LogWarning("No actors provided for view actor request");
            yield break;
        }

        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Async view actor call failed: {task.Exception?.GetBaseException().Message}");
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
    /// 构建角色请求URL
    /// </summary>
    private string BuildUrl(List<string> actors)
    {
        var parameters = new List<KeyValuePair<string, string>>();
        foreach (var actor in actors)
        {
            parameters.Add(new KeyValuePair<string, string>("actors", actor));
        }
        return BuildUrlWithQueryParams(_url, parameters);
    }

    /// <summary>
    /// 尝试解析查看角色响应数据
    /// </summary>
    private bool TryParseResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("View actor response text is empty");
            return false;
        }

        try
        {
            var response = JsonConvert.DeserializeObject<ActorDetailsResponse>(responseText);
            if (response == null)
            {
                Debug.LogError("ViewActorAction response is null");
                return false;
            }

            _responseData = response;
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse view actor response: {ex.Message}");
            return false;
        }
    }

    #endregion
}
