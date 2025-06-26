using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// 用户登录操作，使用改进的 BaseRequestAction
/// </summary>
public class LoginAction : BaseRequestAction
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本
    
    private bool _lastRequestSuccess = false;
    
    public bool LastRequestSuccess => _lastRequestSuccess;

    #region 协程版本（兼容现有代码）
    
    /// <summary>
    /// 用户登录（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine(string user, string game)
    {
        Debug.Log($"Login request for user: {user}, game: {game}");
        
        _lastRequestSuccess = false;
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for login");
            yield break;
        }
        
        // 创建请求数据
        var requestData = new LoginRequest { user_name = user, game_name = game };
        var jsonData = JsonConvert.SerializeObject(requestData);
        
        bool requestCompleted = false;
        RequestResult result = null;
        
        // 发送请求
        yield return PostRequestCoroutine(GameContext.Instance.LOGIN_URL, jsonData, (response) =>
        {
            result = response;
            requestCompleted = true;
        });
        
        // 等待请求完成
        yield return new WaitUntil(() => requestCompleted);
        
        // 处理结果
        if (result != null && result.isSuccess)
        {
            if (TryParseLoginResponse(result.responseText, user, game))
            {
                _lastRequestSuccess = true;
                Debug.Log("Login successful");
            }
            else
            {
                Debug.LogError("Failed to parse login response");
            }
        }
        else
        {
            Debug.LogError($"Login failed: {result?.error ?? "Unknown error"}");
        }
    }
    
    #endregion

    #region Async 版本（推荐用于 Unity 6）
    
    /// <summary>
    /// 用户登录（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync(string user, string game)
    {
        Debug.Log($"Login request async for user: {user}, game: {game}");
        
        _lastRequestSuccess = false;
        
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for login");
            return false;
        }
        
        try
        {
            // 创建请求数据
            var requestData = new LoginRequest { user_name = user, game_name = game };
            var jsonData = JsonConvert.SerializeObject(requestData);
            
            // 发送请求
            var result = await PostRequestAsync(GameContext.Instance.LOGIN_URL, jsonData);
            
            // 处理结果
            if (result.isSuccess)
            {
                if (TryParseLoginResponse(result.responseText, user, game))
                {
                    _lastRequestSuccess = true;
                    Debug.Log("Login successful");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse login response");
                }
            }
            else
            {
                Debug.LogError($"Login failed: {result.error}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during login request: {ex.Message}");
        }
        
        return false;
    }
    
    #endregion

    #region 通用调用方法
    
    /// <summary>
    /// 统一的调用接口，根据配置选择协程或 Async 版本
    /// </summary>
    public IEnumerator Call(string user, string game)
    {
        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync(user, game);
            yield return new WaitUntil(() => task.IsCompleted);
            
            if (task.IsFaulted)
            {
                Debug.LogError($"Async login call failed: {task.Exception?.GetBaseException().Message}");
            }
        }
        else
        {
            // 使用协程版本
            yield return CallCoroutine(user, game);
        }
    }
    
    #endregion

    #region 私有方法
    
    /// <summary>
    /// 尝试解析登录响应数据
    /// </summary>
    private bool TryParseLoginResponse(string responseText, string user, string game)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("Login response text is empty");
            return false;
        }
        
        try
        {
            var response = JsonConvert.DeserializeObject<LoginResponse>(responseText);
            
            if (response == null)
            {
                Debug.LogError("LoginAction response is null");
                return false;
            }
            
            Debug.Log($"LoginAction.message = {response.message}");

            // 设置登录信息
            GameContext.Instance.UserName = user;
            GameContext.Instance.GameName = game;
            GameContext.Instance.ActorName = "";
            
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse login response: {ex.Message}");
            return false;
        }
    }
    
    #endregion
}
