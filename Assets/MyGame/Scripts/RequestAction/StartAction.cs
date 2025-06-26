using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// 开始游戏操作，使用改进的 BaseRequestAction
/// </summary>
public class StartAction : BaseRequestAction
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本
    
    private bool _lastRequestSuccess = false;
    
    public bool LastRequestSuccess => _lastRequestSuccess;

    #region 协程版本（兼容现有代码）
    
    /// <summary>
    /// 开始游戏（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine(string actorName)
    {
        Debug.Log($"Start game request for actor: {actorName}");
        
        _lastRequestSuccess = false;
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for start game");
            yield break;
        }
        
        // 创建请求数据
        var requestData = new StartRequest 
        { 
            user_name = GameContext.Instance.UserName, 
            game_name = GameContext.Instance.GameName, 
            actor_name = actorName 
        };
        var jsonData = JsonConvert.SerializeObject(requestData);
        
        bool requestCompleted = false;
        RequestResult result = null;
        
        // 发送请求
        yield return PostRequestCoroutine(GameContext.Instance.START_URL, jsonData, (response) =>
        {
            result = response;
            requestCompleted = true;
        });
        
        // 等待请求完成
        yield return new WaitUntil(() => requestCompleted);
        
        // 处理结果
        if (result != null && result.isSuccess)
        {
            if (TryParseStartResponse(result.responseText, actorName))
            {
                _lastRequestSuccess = true;
                Debug.Log("Start game successful");
            }
            else
            {
                Debug.LogError("Failed to parse start game response");
            }
        }
        else
        {
            Debug.LogError($"Start game failed: {result?.error ?? "Unknown error"}");
        }
    }
    
    #endregion

    #region Async 版本（推荐用于 Unity 6）
    
    /// <summary>
    /// 开始游戏（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync(string actorName)
    {
        Debug.Log($"Start game request async for actor: {actorName}");
        
        _lastRequestSuccess = false;
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for start game");
            return false;
        }
        
        try
        {
            // 创建请求数据
            var requestData = new StartRequest 
            { 
                user_name = GameContext.Instance.UserName, 
                game_name = GameContext.Instance.GameName, 
                actor_name = actorName 
            };
            var jsonData = JsonConvert.SerializeObject(requestData);
            
            // 发送请求
            var result = await PostRequestAsync(GameContext.Instance.START_URL, jsonData);
            
            // 处理结果
            if (result.isSuccess)
            {
                if (TryParseStartResponse(result.responseText, actorName))
                {
                    _lastRequestSuccess = true;
                    Debug.Log("Start game successful");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse start game response");
                }
            }
            else
            {
                Debug.LogError($"Start game failed: {result.error}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during start game request: {ex.Message}");
        }
        
        return false;
    }
    
    #endregion

    #region 通用调用方法
    
    /// <summary>
    /// 统一的调用接口，根据配置选择协程或 Async 版本
    /// </summary>
    public IEnumerator Call(string actorName)
    {
        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync(actorName);
            yield return new WaitUntil(() => task.IsCompleted);
            
            if (task.IsFaulted)
            {
                Debug.LogError($"Async start game call failed: {task.Exception?.GetBaseException().Message}");
            }
        }
        else
        {
            // 使用协程版本
            yield return CallCoroutine(actorName);
        }
    }
    
    #endregion

    #region 私有方法
    
    /// <summary>
    /// 尝试解析开始游戏响应数据
    /// </summary>
    private bool TryParseStartResponse(string responseText, string actorName)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("Start game response text is empty");
            return false;
        }
        
        try
        {
            var response = JsonConvert.DeserializeObject<StartResponse>(responseText);
            
            if (response == null)
            {
                Debug.LogError("StartAction response is null");
                return false;
            }

            // 检查响应数据
            if (response.error != 0)
            {
                Debug.LogError($"StartAction.error = {response.error}");
                Debug.LogError($"StartAction.message = {response.message}");
                return false;
            }

            Debug.Log($"StartAction.message = {response.message}");

            // 设置角色名称
            GameContext.Instance.ActorName = actorName;
            
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse start game response: {ex.Message}");
            return false;
        }
    }
    
    #endregion
}
