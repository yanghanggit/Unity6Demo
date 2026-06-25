using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// 地下城战斗敌方抽牌 API 客户端
/// 用于处理地下城战斗中的敌方抽卡请求
/// </summary>
public class DungeonCombatDrawEnemyCardsApi : BaseApiClient
{
    /// <summary>
    /// API 请求结果
    /// </summary>
    private RequestResult _requestResult;

    /// <summary>
    /// 获取 API 请求结果
    /// </summary>
    public override RequestResult ReqResult => _requestResult;

    /// <summary>
    /// API 响应数据
    /// </summary>
    private DungeonCombatDrawEnemyCardsResponse _responseData;

    /// <summary>
    /// 获取 API 响应数据
    /// </summary>
    public DungeonCombatDrawEnemyCardsResponse RespData => _responseData;

    /// <summary>
    /// 调用地下城战斗抽牌 API
    /// </summary>
    /// <param name="url">API 请求地址</param>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名称</param>
    /// <returns>异步任务</returns>
    public async UniTask Call(string url, string userName, string gameName)
    {
        Debug.Log("Starting DungeonCombatDrawEnemyCardsApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"UserName: {userName}");
        Debug.Log($"GameName: {gameName}");

        // 清除之前的请求结果和响应数据
        _requestResult = null;
        _responseData = null;

        // 检查网络连接状态
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return;
        }

        // 创建请求数据对象并序列化为 JSON
        var requestData = new DungeonCombatDrawEnemyCardsRequest
        {
            user_name = userName,
            game_name = gameName,
        };
        var jsonData = JsonConvert.SerializeObject(requestData);

        // 发送 POST 请求并等待完成
        _requestResult = await PostRequestAsync(url, jsonData);

        // 检查请求是否成功
        if (!_requestResult.isSuccess)
        {
            Debug.LogError($"Request failed: {_requestResult.error}");
            return;
        }

        // 验证并解析响应数据
        if (string.IsNullOrEmpty(_requestResult.responseText))
        {
            Debug.LogError("Response text is empty");
            return;
        }

        try
        {
            _responseData = JsonConvert.DeserializeObject<DungeonCombatDrawEnemyCardsResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }

}
