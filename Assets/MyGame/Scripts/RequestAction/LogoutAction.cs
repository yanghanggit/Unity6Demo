using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// 用户登出操作，使用改进的 BaseRequestAction
/// </summary>
public class LogoutAction : BaseRequestAction
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本

    private string _url;
    private string _userName;
    private string _gameName;
    private RequestResult _requestResult = null;
    public RequestResult ReqResult => _requestResult;
    private LogoutResponse _responseData = null;
    public LogoutResponse ResponseData => _responseData;

    private void Setup(string url, string userName, string gameName)
    {
        _url = url;
        _userName = userName;
        _gameName = gameName;
        _requestResult = null;
        _responseData = null;

        Debug.Log($"LogoutAction initialized with URL: {_url}, UserName: {_userName}, GameName: {_gameName}");
    }

    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 用户登出（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine()
    {
        Debug.Log("Logout request started");

        //_lastRequestSuccess = false;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for logout");
            yield break;
        }

        // 创建请求数据
        var requestData = new LogoutRequest
        {
            user_name = _userName,
            game_name = _gameName
        };
        var jsonData = JsonConvert.SerializeObject(requestData);

        bool requestCompleted = false;
        //RequestResult result = null;

        // 发送请求
        yield return PostRequestCoroutine(_url, jsonData, (response) =>
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
                Debug.Log("Logout successful");
            }
            else
            {
                Debug.LogError("Failed to parse logout response");
            }
        }
        else
        {
            Debug.LogError($"Logout failed: {_requestResult?.error ?? "Unknown error"}");
        }
    }

    #endregion

    #region Async 版本（推荐用于 Unity 6）

    /// <summary>
    /// 用户登出（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync()
    {
        Debug.Log("Logout request async started");

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for logout");
            return false;
        }

        try
        {
            // 创建请求数据
            var requestData = new LogoutRequest
            {
                user_name = _userName,
                game_name = _gameName
            };
            var jsonData = JsonConvert.SerializeObject(requestData);

            // 发送请求
            _requestResult = await PostRequestAsync(_url, jsonData);

            // 处理结果
            if (_requestResult.isSuccess)
            {
                if (TryParseResponse(_requestResult.responseText))
                {
                    //_lastRequestSuccess = true;
                    Debug.Log("Logout successful");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse logout response");
                }
            }
            else
            {
                Debug.LogError($"Logout failed: {_requestResult.error}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during logout request: {ex.Message}");
        }

        return false;
    }

    #endregion

    #region 通用调用方法

    /// <summary>
    /// 统一的调用接口，根据配置选择协程或 Async 版本
    /// </summary>
    public IEnumerator Call(string url, string user, string game)
    {
        Setup(url, user, game);


        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Async logout call failed: {task.Exception?.GetBaseException().Message}");
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
    /// 尝试解析登出响应数据
    /// </summary>
    private bool TryParseResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("Logout response text is empty");
            return false;
        }

        try
        {
            var response = JsonConvert.DeserializeObject<LogoutResponse>(responseText);
            if (response == null)
            {
                Debug.LogError("LogoutAction response is null");
                return false;
            }

            Debug.Log($"LogoutAction.message = {response.message}");
            
            // 
            _responseData = response;
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse logout response: {ex.Message}");
            return false;
        }
    }

    #endregion
}
