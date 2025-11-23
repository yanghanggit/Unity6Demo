using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// 用户登录操作，使用改进的 BaseRequestAction
/// </summary>
public class LoginAction : BaseApiClient
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本

    private string _url;
    private string _userName;
    private string _gameName;
    private RequestResult _requestResult = null;
    public RequestResult ReqResult => _requestResult;

    private LoginResponse _responseData = null;
    public LoginResponse ResponseData => _responseData;
  

    void Setup(string url, string userName, string gameName)
    {
        _url = url;
        _userName = userName;
        _gameName = gameName;
        _requestResult = null;
        _responseData = null;

        Debug.Log($"LoginAction initialized with URL: {_url}, UserName: {_userName}, GameName: {_gameName}");
    }


    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 用户登录（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine()
    {
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for login");
            yield break;
        }

        // 创建请求数据
        var requestData = new LoginRequest { user_name = _userName, game_name = _gameName };
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
                Debug.Log("Login successful");
            }
            else
            {
                Debug.LogError("Failed to parse login response");
            }
        }
        else
        {
            Debug.LogError($"Login failed: {_requestResult?.error ?? "Unknown error"}");
        }
    }

    #endregion

    #region Async 版本（推荐用于 Unity 6）

    /// <summary>
    /// 用户登录（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync()
    {
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for login");
            return false;
        }

        try
        {
            // 创建请求数据
            var requestData = new LoginRequest { user_name = _userName, game_name = _gameName };
            var jsonData = JsonConvert.SerializeObject(requestData);

            // 发送请求
            _requestResult = await PostRequestAsync(_url, jsonData);

            // 处理结果
            if (_requestResult.isSuccess)
            {
                if (TryParseResponse(_requestResult.responseText))
                {
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
                Debug.LogError($"Login failed: {_requestResult.error}");
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
                Debug.LogError($"Async login call failed: {task.Exception?.GetBaseException().Message}");
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
    /// 尝试解析登录响应数据
    /// </summary>
    private bool TryParseResponse(string responseText)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<LoginResponse>(responseText);
            if (response == null)
            {
                Debug.LogError("LoginAction response is null");
                return false;
            }

            Debug.Log($"LoginAction.message = {response.message}");

            _responseData = response;
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
