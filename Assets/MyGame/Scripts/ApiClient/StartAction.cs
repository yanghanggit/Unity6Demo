using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// 开始游戏操作，使用改进的 BaseRequestAction
/// </summary>
public class StartAction : BaseApiClient
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本

    private string _url;
    private string _userName;
    private string _gameName;
    private string _actorName;
    private RequestResult _requestResult = null;
    public RequestResult ReqResult => _requestResult;
    private StartResponse _responseData = null;
    public StartResponse ResponseData => _responseData;

    void Setup(string url, string userName, string gameName, string actorName)
    {
        _url = url;
        _userName = userName;
        _gameName = gameName;
        _actorName = actorName;
        _requestResult = null;
        _responseData = null;

        Debug.Log($"StartAction initialized with URL: {_url}, UserName: {_userName}, GameName: {_gameName}, ActorName: {_actorName}");
    }




    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 开始游戏（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine()
    {
        //Debug.Log($"Start game request for actor: {actorName}");

        //_lastRequestSuccess = false;

        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for start game");
            yield break;
        }

        // 创建请求数据
        var requestData = new StartRequest
        {
            user_name = _userName,
            game_name = _gameName,
            actor_name = _actorName
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
                Debug.Log("Start game successful");
            }
            else
            {
                Debug.LogError("Failed to parse start game response");
            }
        }
        else
        {
            Debug.LogError($"Start game failed: {_requestResult?.error ?? "Unknown error"}");
        }
    }

    #endregion

    #region Async 版本（推荐用于 Unity 6）

    /// <summary>
    /// 开始游戏（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync()
    {
        // Debug.Log($"Start game request async for actor: {actorName}");

        //_lastRequestSuccess = false;

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
                user_name = _userName,
                game_name = _gameName,
                actor_name = _actorName
            };
            var jsonData = JsonConvert.SerializeObject(requestData);

            // 发送请求
            _requestResult = await PostRequestAsync(_url, jsonData);

            // 处理结果
            if (_requestResult.isSuccess)
            {
                if (TryParseResponse(_requestResult.responseText))
                {
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
                Debug.LogError($"Start game failed: {_requestResult.error}");
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
    public IEnumerator Call(string url, string user, string game, string actor)
    {
        Setup(url, user, game, actor);

        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Async start game call failed: {task.Exception?.GetBaseException().Message}");
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
    /// 尝试解析开始游戏响应数据
    /// </summary>
    private bool TryParseResponse(string responseText)
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

            Debug.Log($"StartAction.message = {response.message}");

            _responseData = response;
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
