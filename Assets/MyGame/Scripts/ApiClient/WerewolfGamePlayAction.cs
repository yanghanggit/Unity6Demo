using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

public class WerewolfGamePlayAction : BaseApiClient
{
    [Header("配置")]
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本

    private WerewolfGamePlayResponse _responseData = null;

    private RequestResult _requestResult = null;

    public WerewolfGamePlayResponse ResponseData => _responseData;

    public override RequestResult ReqResult => _requestResult;

    private string _url;

    private string _userName;

    private string _gameName;

    private Dictionary<string, string> _data;

    public void Setup(string url, string userName, string gameName, Dictionary<string, string> data)
    {
        _url = url;
        _userName = userName;
        _gameName = gameName;
        _data = data;
        _requestResult = null;
        _responseData = null;

        Debug.Log($"WerewolfGamePlayAction initialized with URL: {_url}, UserName: {_userName}, GameName: {_gameName}, Data count: {_data.Count}");

        // 打印一下data
        foreach (var kvp in _data)
        {
            Debug.Log($"Data Key: {kvp.Key}, Value: {kvp.Value}");
        }
    }

    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 家园游戏玩法（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine()
    {
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for home gameplay");
            yield break;
        }

        // 创建请求数据
        var requestData = new WerewolfGamePlayRequest
        {
            user_name = _userName,
            game_name = _gameName,
            data = _data
        };
        var jsonData = JsonConvert.SerializeObject(requestData);

        bool requestCompleted = false;

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
                Debug.Log("Home gameplay successful");
            }
            else
            {
                Debug.LogError("Failed to parse home gameplay response");
            }
        }
        else
        {
            Debug.LogError($"Home gameplay failed: {_requestResult?.error ?? "Unknown error"}");
        }
    }

    #endregion

    #region Async 版本（推荐用于 Unity 6）

    /// <summary>
    /// 家园游戏玩法（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync()
    {
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for home gameplay");
            return false;
        }

        try
        {
            // 创建请求数据
            var requestData = new WerewolfGamePlayRequest
            {
                user_name = _userName,
                game_name = _gameName,
                data = _data
            };
            var jsonData = JsonConvert.SerializeObject(requestData);

            // 发送请求
            _requestResult = await PostRequestAsync(_url, jsonData);

            // 处理结果
            if (_requestResult.isSuccess)
            {
                if (TryParseResponse(_requestResult.responseText))
                {
                    Debug.Log("Home gameplay successful");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse home gameplay response");
                }
            }
            else
            {
                Debug.LogError($"Home gameplay failed: {_requestResult.error}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during home gameplay request: {ex.Message}");
        }

        return false;
    }

    #endregion

    #region 通用调用方法

    /// <summary>
    /// 统一的调用接口，根据配置选择协程或 Async 版本
    /// </summary>
    public IEnumerator Call(string url, string userName, string gameName, Dictionary<string, string> data)
    {
        Setup(url, userName, gameName, data);

        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Async home gameplay call failed: {task.Exception?.GetBaseException().Message}");
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
    /// 尝试解析家园游戏玩法响应数据
    /// </summary>
    private bool TryParseResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("Home gameplay response text is empty");
            return false;
        }

        try
        {
            var response = JsonConvert.DeserializeObject<WerewolfGamePlayResponse>(responseText);

            if (response == null)
            {
                Debug.LogError("HomeGamePlayAction response is null");
                return false;
            }

            // 设置游戏状态
            _responseData = response;
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse home gameplay response: {ex.Message}");
            return false;
        }
    }

    #endregion
}
