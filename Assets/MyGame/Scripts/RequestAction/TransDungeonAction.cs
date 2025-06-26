using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// 转换到地下城操作，使用改进的 BaseRequestAction
/// </summary>
public class TransDungeonAction : BaseRequestAction
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本
    
    private bool _lastRequestSuccess = false;
    
    public bool LastRequestSuccess => _lastRequestSuccess;

    #region 协程版本（兼容现有代码）
    
    /// <summary>
    /// 转换到地下城（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine()
    {
        Debug.Log("Trans to dungeon request started");
        
        _lastRequestSuccess = false;
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for trans dungeon");
            yield break;
        }
        
        // 创建请求数据
        var requestData = new HomeTransDungeonRequest 
        { 
            user_name = GameContext.Instance.UserName, 
            game_name = GameContext.Instance.GameName 
        };
        var jsonData = JsonConvert.SerializeObject(requestData);
        
        bool requestCompleted = false;
        RequestResult result = null;
        
        // 发送请求
        yield return PostRequestCoroutine(GameContext.Instance.HOME_TRANS_DUNGEON_URL, jsonData, (response) =>
        {
            result = response;
            requestCompleted = true;
        });
        
        // 等待请求完成
        yield return new WaitUntil(() => requestCompleted);
        
        // 处理结果
        if (result != null && result.isSuccess)
        {
            if (TryParseTransDungeonResponse(result.responseText))
            {
                _lastRequestSuccess = true;
                Debug.Log("Trans to dungeon successful");
            }
            else
            {
                Debug.LogError("Failed to parse trans dungeon response");
            }
        }
        else
        {
            Debug.LogError($"Trans to dungeon failed: {result?.error ?? "Unknown error"}");
        }
    }
    
    #endregion

    #region Async 版本（推荐用于 Unity 6）
    
    /// <summary>
    /// 转换到地下城（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync()
    {
        Debug.Log("Trans to dungeon request async started");
        
        _lastRequestSuccess = false;
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for trans dungeon");
            return false;
        }
        
        try
        {
            // 创建请求数据
            var requestData = new HomeTransDungeonRequest 
            { 
                user_name = GameContext.Instance.UserName, 
                game_name = GameContext.Instance.GameName 
            };
            var jsonData = JsonConvert.SerializeObject(requestData);
            
            // 发送请求
            var result = await PostRequestAsync(GameContext.Instance.HOME_TRANS_DUNGEON_URL, jsonData);
            
            // 处理结果
            if (result.isSuccess)
            {
                if (TryParseTransDungeonResponse(result.responseText))
                {
                    _lastRequestSuccess = true;
                    Debug.Log("Trans to dungeon successful");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse trans dungeon response");
                }
            }
            else
            {
                Debug.LogError($"Trans to dungeon failed: {result.error}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during trans dungeon request: {ex.Message}");
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
                Debug.LogError($"Async trans dungeon call failed: {task.Exception?.GetBaseException().Message}");
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
    /// 尝试解析转换到地下城响应数据
    /// </summary>
    private bool TryParseTransDungeonResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("Trans dungeon response text is empty");
            return false;
        }
        
        try
        {
            var response = JsonConvert.DeserializeObject<HomeTransDungeonResponse>(responseText);
            
            if (response == null)
            {
                Debug.LogError("TransDungeonAction response is null");
                return false;
            }
            
            Debug.Log($"TransDungeonAction.message = {response.message}");
            
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse trans dungeon response: {ex.Message}");
            return false;
        }
    }
    
    #endregion
}
