using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// 地下城游戏玩法操作，使用改进的 BaseRequestAction
/// </summary>
public class DungeonGamePlayAction : BaseRequestAction
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本
    
    private bool _lastRequestSuccess = false;
    private DungeonGamePlayResponse _response;
    
    public bool LastRequestSuccess => _lastRequestSuccess;
    

    public void ResetStatus()
    {
        Debug.Log(this.GetType().Name + ":ResetStatus");
        _lastRequestSuccess = false;
        _response = null;
    }

    #region 协程版本（兼容现有代码）
    
    /// <summary>
    /// 地下城游戏玩法（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine(string userInputTag, Dictionary<string, string> data = null)
    {
        Debug.Log($"Dungeon gameplay request for tag: {userInputTag}");
        
        if (data == null)
        {
            data = new Dictionary<string, string>();
        }
        
        // 重置请求状态
        ResetStatus();
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for dungeon gameplay");
            yield break;
        }
        
        // 创建请求数据
        var requestData = new DungeonGamePlayRequest 
        { 
            user_name = GameContext.Instance.UserName, 
            game_name = GameContext.Instance.GameName, 
            user_input = new DungeonGamePlayUserInput { tag = userInputTag, data = data } 
        };
        var jsonData = JsonConvert.SerializeObject(requestData);
        
        bool requestCompleted = false;
        RequestResult result = null;
        
        // 发送请求
        yield return PostRequestCoroutine(GameContext.Instance.DUNGEON_GAMEPLAY_URL, jsonData, (response) =>
        {
            result = response;
            requestCompleted = true;
        });
        
        // 等待请求完成
        yield return new WaitUntil(() => requestCompleted);
        
        // 处理结果
        if (result != null && result.isSuccess)
        {
            if (TryParseDungeonGamePlayResponse(result.responseText))
            {
                _lastRequestSuccess = true;
                Debug.Log("Dungeon gameplay successful");
            }
            else
            {
                Debug.LogError("Failed to parse dungeon gameplay response");
            }
        }
        else
        {
            Debug.LogError($"Dungeon gameplay failed: {result?.error ?? "Unknown error"}");
        }
    }
    
    #endregion

    #region Async 版本（推荐用于 Unity 6）
    
    /// <summary>
    /// 地下城游戏玩法（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync(string userInputTag, Dictionary<string, string> data = null)
    {
        Debug.Log($"Dungeon gameplay request async for tag: {userInputTag}");
        
        if (data == null)
        {
            data = new Dictionary<string, string>();
        }
        
        // 重置请求状态
        ResetStatus();
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for dungeon gameplay");
            return false;
        }
        
        try
        {
            // 创建请求数据
            var requestData = new DungeonGamePlayRequest 
            { 
                user_name = GameContext.Instance.UserName, 
                game_name = GameContext.Instance.GameName, 
                user_input = new DungeonGamePlayUserInput { tag = userInputTag, data = data } 
            };
            var jsonData = JsonConvert.SerializeObject(requestData);
            
            // 发送请求
            var result = await PostRequestAsync(GameContext.Instance.DUNGEON_GAMEPLAY_URL, jsonData);
            
            // 处理结果
            if (result.isSuccess)
            {
                if (TryParseDungeonGamePlayResponse(result.responseText))
                {
                    _lastRequestSuccess = true;
                    Debug.Log("Dungeon gameplay successful");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse dungeon gameplay response");
                }
            }
            else
            {
                Debug.LogError($"Dungeon gameplay failed: {result.error}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during dungeon gameplay request: {ex.Message}");
        }
        
        return false;
    }
    
    #endregion

    #region 通用调用方法
    
    /// <summary>
    /// 统一的调用接口，根据配置选择协程或 Async 版本
    /// </summary>
    public IEnumerator Call(string userInputTag, Dictionary<string, string> data = null)
    {
        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync(userInputTag, data);
            yield return new WaitUntil(() => task.IsCompleted);
            
            if (task.IsFaulted)
            {
                Debug.LogError($"Async dungeon gameplay call failed: {task.Exception?.GetBaseException().Message}");
            }
        }
        else
        {
            // 使用协程版本
            yield return CallCoroutine(userInputTag, data);
        }
    }
    
    #endregion

    #region 私有方法
    
    /// <summary>
    /// 尝试解析地下城游戏玩法响应数据
    /// </summary>
    private bool TryParseDungeonGamePlayResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("Dungeon gameplay response text is empty");
            return false;
        }
        
        try
        {
            _response = JsonConvert.DeserializeObject<DungeonGamePlayResponse>(responseText);
            
            if (_response == null)
            {
                Debug.LogError("DungeonGamePlayAction response is null");
                return false;
            }

            // 设置游戏状态
            GameContext.Instance.ProcessClientMessages(_response.client_messages);
            
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse dungeon gameplay response: {ex.Message}");
            return false;
        }
    }
    
    #endregion
}
