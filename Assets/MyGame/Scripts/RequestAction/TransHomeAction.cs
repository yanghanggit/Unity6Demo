using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// 转换到家园操作，使用改进的 BaseRequestAction
/// </summary>
public class TransHomeAction : BaseRequestAction
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本
    
    private bool _lastRequestSuccess = false;
    
    public bool LastRequestSuccess => _lastRequestSuccess;

    #region 协程版本（兼容现有代码）
    
    /// <summary>
    /// 转换到家园（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine()
    {
        Debug.Log("Trans to home request started");
        
        _lastRequestSuccess = false;
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for trans home");
            yield break;
        }
        
        // 创建请求数据
        var requestData = new DungeonTransHomeRequest 
        { 
            user_name = GameContext.Instance.UserName, 
            game_name = GameContext.Instance.GameName 
        };
        var jsonData = JsonConvert.SerializeObject(requestData);
        
        bool requestCompleted = false;
        RequestResult result = null;
        
        // 发送请求
        yield return PostRequestCoroutine(GameContext.Instance.DUNGEON_TRANS_HOME_URL, jsonData, (response) =>
        {
            result = response;
            requestCompleted = true;
        });
        
        // 等待请求完成
        yield return new WaitUntil(() => requestCompleted);
        
        // 处理结果
        if (result != null && result.isSuccess)
        {
            if (TryParseTransHomeResponse(result.responseText))
            {
                _lastRequestSuccess = true;
                Debug.Log("Trans to home successful");
            }
            else
            {
                Debug.LogError("Failed to parse trans home response");
            }
        }
        else
        {
            Debug.LogError($"Trans to home failed: {result?.error ?? "Unknown error"}");
        }
    }
    
    #endregion

    #region Async 版本（推荐用于 Unity 6）
    
    /// <summary>
    /// 转换到家园（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync()
    {
        Debug.Log("Trans to home request async started");
        
        _lastRequestSuccess = false;
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for trans home");
            return false;
        }
        
        try
        {
            // 创建请求数据
            var requestData = new DungeonTransHomeRequest 
            { 
                user_name = GameContext.Instance.UserName, 
                game_name = GameContext.Instance.GameName 
            };
            var jsonData = JsonConvert.SerializeObject(requestData);
            
            // 发送请求
            var result = await PostRequestAsync(GameContext.Instance.DUNGEON_TRANS_HOME_URL, jsonData);
            
            // 处理结果
            if (result.isSuccess)
            {
                if (TryParseTransHomeResponse(result.responseText))
                {
                    _lastRequestSuccess = true;
                    Debug.Log("Trans to home successful");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse trans home response");
                }
            }
            else
            {
                Debug.LogError($"Trans to home failed: {result.error}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during trans home request: {ex.Message}");
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
                Debug.LogError($"Async trans home call failed: {task.Exception?.GetBaseException().Message}");
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
    /// 尝试解析转换到家园响应数据
    /// </summary>
    private bool TryParseTransHomeResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("Trans home response text is empty");
            return false;
        }
        
        try
        {
            var response = JsonConvert.DeserializeObject<DungeonTransHomeResponse>(responseText);
            
            if (response == null)
            {
                Debug.LogError("TransHomeAction response is null");
                return false;
            }

            // 检查响应数据
            if (response.error != 0)
            {
                Debug.LogError($"TransHomeAction.error = {response.error}");
                Debug.LogError($"TransHomeAction.message = {response.message}");
                return false;
            }

            Debug.Log($"TransHomeAction.message = {response.message}");
            
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse trans home response: {ex.Message}");
            return false;
        }
    }
    
    #endregion
}
