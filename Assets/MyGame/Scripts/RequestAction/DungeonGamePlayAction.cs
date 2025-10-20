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
    [SerializeField] private bool useAsyncVersion = true; // 是否使用 async 版本

    private string _url;
    private string _userName;
    private string _gameName;
    private string _userInputTag;
    private Dictionary<string, string> _data;

    private RequestResult _requestResult = null;
    public RequestResult ReqResult => _requestResult;

    public DungeonGamePlayResponse _responseData = null;
    public DungeonGamePlayResponse ResponseData => _responseData;

    public void Setup(string url, string userName, string gameName, string userInputTag, Dictionary<string, string> data = null)
    {
        _url = url;
        _userName = userName;
        _gameName = gameName;
        _userInputTag = userInputTag;
        _data = data ?? new Dictionary<string, string>();
        _requestResult = null;
        _responseData = null;

        Debug.Log($"DungeonGamePlayAction initialized with URL: {_url}, UserName: {_userName}, GameName: {_gameName}, UserInputTag: {_userInputTag}");
        Debug.Log($"DungeonGamePlayAction initialized with Data: {JsonConvert.SerializeObject(_data)}");
    }

    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 地下城游戏玩法（协程版本）
    /// </summary>
    public IEnumerator CallCoroutine()
    {
        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available for dungeon gameplay");
            yield break;
        }

        // 创建请求数据
        var requestData = new DungeonGamePlayRequest
        {
            user_name = _userName,
            game_name = _gameName,
            user_input = new DungeonGamePlayUserInput { tag = _userInputTag, data = _data }
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
                //_lastRequestSuccess = true;
                Debug.Log("Dungeon gameplay successful");
            }
            else
            {
                Debug.LogError("Failed to parse dungeon gameplay response");
            }
        }
        else
        {
            Debug.LogError($"Dungeon gameplay failed: {_requestResult?.error ?? "Unknown error"}");
        }
    }

    #endregion

    #region Async 版本（推荐用于 Unity 6）

    /// <summary>
    /// 地下城游戏玩法（Async 版本）
    /// </summary>
    public async Task<bool> CallAsync()
    {
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
                user_name = _userName,
                game_name = _gameName,
                user_input = new DungeonGamePlayUserInput { tag = _userInputTag, data = _data }
            };
            var jsonData = JsonConvert.SerializeObject(requestData);

            // 发送请求
            _requestResult = await PostRequestAsync(_url, jsonData);

            // 处理结果
            if (_requestResult.isSuccess)
            {
                if (TryParseResponse(_requestResult.responseText))
                {
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
                Debug.LogError($"Dungeon gameplay failed: {_requestResult.error}");
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
    public IEnumerator Call(string url, string userName, string gameName, string userInputTag, Dictionary<string, string> data = null)
    {
        Setup(url, userName, gameName, userInputTag, data);

        if (useAsyncVersion)
        {
            // 使用 async 版本
            var task = CallAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Async dungeon gameplay call failed: {task.Exception?.GetBaseException().Message}");
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
    /// 尝试解析地下城游戏玩法响应数据
    /// </summary>
    private bool TryParseResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogError("Dungeon gameplay response text is empty");
            return false;
        }

        try
        {
            var response = JsonConvert.DeserializeObject<DungeonGamePlayResponse>(responseText);
            if (response == null)
            {
                Debug.LogError("DungeonGamePlayAction response is null");
                return false;
            }

            // 设置游戏状态
            _responseData = response;
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
