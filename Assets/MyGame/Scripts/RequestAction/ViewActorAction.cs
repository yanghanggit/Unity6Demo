using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// 查看角色操作，使用改进的 BaseRequestAction
/// </summary>
public class ViewActorAction : BaseRequestAction
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本
    
    private bool _lastRequestSuccess = false;
    
    public bool LastRequestSuccess => _lastRequestSuccess;

    #region 协程版本（兼容现有代码）
    
    /// <summary>
    /// 查看角色（协程版本）
    /// </summary>
    private IEnumerator CallCoroutine(List<string> actors)
    {
        Debug.Log($"View actor request started with {actors?.Count ?? 0} actors");
        Debug.Assert(actors != null && actors.Count > 0, "Actors list is null or empty");

        _lastRequestSuccess = false;
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for view actor");
            yield break;
        }
        
        // 构建完整URL
        string fullUrl = BuildActorUrl(actors);
        
        bool requestCompleted = false;
        RequestResult result = null;
        
        // 发送请求
        yield return GetRequestCoroutine(fullUrl, (response) =>
        {
            result = response;
            requestCompleted = true;
        });
        
        // 等待请求完成
        yield return new WaitUntil(() => requestCompleted);
        
        // 处理结果
        if (result != null && result.isSuccess)
        {
            if (TryParseViewActorResponse(result.responseText))
            {
                _lastRequestSuccess = true;
                Debug.Log("View actor successful");
            }
            else
            {
                Debug.LogError("Failed to parse view actor response");
            }
        }
        else
        {
            Debug.LogError($"View actor failed: {result?.error ?? "Unknown error"}");
        }
    }
    
    #endregion

    #region Async 版本（推荐用于 Unity 6）
    
    /// <summary>
    /// 查看角色（Async 版本）
    /// </summary>
    private async Task<bool> CallAsync(List<string> actors)
    {
        Debug.Log($"View actor request async started with {actors?.Count ?? 0} actors");
        Debug.Assert(actors != null && actors.Count > 0, "Actors list is null or empty");

        _lastRequestSuccess = false;
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for view actor");
            return false;
        }
        
        try
        {
            // 构建完整URL
            string fullUrl = BuildActorUrl(actors);
            Debug.Log($"View actor request URL: {fullUrl}");
            
            // 发送请求
            var result = await GetRequestAsync(fullUrl);
            
            // 处理结果
            if (result.isSuccess)
            {
                if (TryParseViewActorResponse(result.responseText))
                {
                    _lastRequestSuccess = true;
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
                Debug.LogError($"View actor failed: {result.error}");
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
    public IEnumerator Call(List<string> actors)
    {
        if (actors == null || actors.Count == 0)
        {
            Debug.LogWarning("No actors provided for view actor request");
            yield break;
        }

        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync(actors);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Async view actor call failed: {task.Exception?.GetBaseException().Message}");
            }
        }
        else
        {
            // 使用协程版本
            yield return CallCoroutine(actors);
        }
    }
    
    #endregion

    #region 私有方法
    
    /// <summary>
    /// 构建角色请求URL
    /// </summary>
    private string BuildActorUrl(List<string> actors)
    {
        var parameters = new List<KeyValuePair<string, string>>();
        foreach (var actor in actors)
        {
            parameters.Add(new KeyValuePair<string, string>("actors", actor));
        }
        return BuildUrlWithQueryParams(GameContext.Instance.VIEW_ACTOR_URL, parameters);
    }
    
    /// <summary>
    /// 尝试解析查看角色响应数据
    /// </summary>
    private bool TryParseViewActorResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("View actor response text is empty");
            return false;
        }
        
        try
        {
            var response = JsonConvert.DeserializeObject<ViewActorResponse>(responseText);
            
            if (response == null)
            {
                Debug.LogError("ViewActorAction response is null");
                return false;
            }

            // 更新游戏上下文中的角色快照
            GameContext.Instance.ActorSnapshots = response.actor_snapshots;
            GameContext.Instance.AgentShortTermMemories = response.agent_short_term_memories;
            
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
