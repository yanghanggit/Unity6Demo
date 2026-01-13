using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Home游戏玩法管理器
/// 单例模式，封装所有Home相关的游戏操作（POST请求）
/// 负责推进游戏、场景切换、角色交互等写操作
/// 仅依赖 SessionManager.FetchSessionMessages 获取会话消息
/// </summary>
public class HomeGamePlayManager : MonoBehaviour
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static HomeGamePlayManager Instance { get; private set; }

    /// <summary>
    /// Home游戏玩法API接口
    /// </summary>
    [SerializeField] private HomeGamePlayApi _homeGamePlayApi;

    /// <summary>
    /// 传送到地下城API接口
    /// </summary>
    [SerializeField] private TransDungeonApi _transDungeonApi;

    /// <summary>
    /// 家园推进API接口
    /// </summary>
    [SerializeField] private HomeAdvanceApi _homeAdvanceApi;

    private void Awake()
    {
        // 单例模式处理
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("[HomeGamePlayManager] Duplicate instance detected, destroying the new one.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Assert(_homeGamePlayApi != null, "_homeGamePlayApi is null");
        Debug.Assert(_transDungeonApi != null, "_transDungeonApi is null");
        Debug.Assert(_homeAdvanceApi != null, "_homeAdvanceApi is null");
    }

    /// <summary>
    /// 推进游戏状态
    /// 调用 home_advance API 推进指定角色的行动
    /// 传入空列表则推进所有角色，服务器会自动处理
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="actors">角色名称列表，传入空列表则推进所有角色</param>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator AdvanceGame(List<string> actors, Action<bool> onComplete = null)
    {
        // 使用 HomeAdvance API 推进角色
        yield return _homeAdvanceApi.Call(
            GameContext.Instance.HomeAdvanceUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            actors);

        // 检查API调用是否成功
        if (_homeAdvanceApi.ReqResult == null)
        {
            Debug.LogError($"[HomeGamePlayManager] home_advance request failed for actors: [{string.Join(", ", actors)}]");
            onComplete?.Invoke(false);
            yield break;
        }

        // 进一步检查响应结果的成功标志
        if (!_homeAdvanceApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[HomeGamePlayManager] home_advance request failed: {_homeAdvanceApi.ReqResult.responseText}");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Assert(_homeAdvanceApi.RespData != null, "[HomeGamePlayManager] home_advance response data is null");

        // 从服务器获取并同步最新的会话消息
        bool fetchSuccess = false;
        yield return SessionManager.Instance.FetchSessionMessages(
            (success, sessionMessages) =>
            {
                fetchSuccess = success;
                if (success)
                {
                    Debug.Log($"[HomeGamePlayManager] Fetched {sessionMessages.Count} session messages after advancing");
                    // 收集AgentEvents 事件到 GameContext
                    GameContext.Instance.CollectEventsByActor(sessionMessages);
                }
            }
        );

        // 检查消息获取是否成功
        if (!fetchSuccess)
        {
            Debug.LogError("[HomeGamePlayManager] Failed to fetch session messages after advancing");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Log($"[HomeGamePlayManager] AdvanceGame completed successfully for actors: [{string.Join(", ", actors)}]");
        onComplete?.Invoke(true);
    }

    /// <summary>
    /// 发送消息给指定角色
    /// 调用 /speak 端点向目标角色发送消息
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="targetActorName">目标角色名称</param>
    /// <param name="messageContent">消息内容</param>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator SpeakToActor(string targetActorName, string messageContent, Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(targetActorName) || string.IsNullOrEmpty(messageContent))
        {
            Debug.LogError("[HomeGamePlayManager] Target actor name or message content is empty");
            onComplete?.Invoke(false);
            yield break;
        }

        // 调用 /speak 端点
        yield return _homeGamePlayApi.Call(
            GameContext.Instance.HomeGamePlayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "/speak",
            new Dictionary<string, string>
            {
                ["target"] = targetActorName,
                ["content"] = messageContent
            });

        // 检查API调用是否成功
        if (_homeGamePlayApi.ReqResult == null)
        {
            Debug.LogError("[HomeGamePlayManager] /speak request failed");
            onComplete?.Invoke(false);
            yield break;
        }

        // 进一步检查响应结果的成功标志
        if (!_homeGamePlayApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[HomeGamePlayManager] /speak request failed: {_homeGamePlayApi.ReqResult.responseText}");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Assert(_homeGamePlayApi.RespData != null, "[HomeGamePlayManager] /speak response data is null");

        // 从服务器获取并同步最新的会话消息
        bool fetchSuccess = false;
        yield return SessionManager.Instance.FetchSessionMessages(
            (success, sessionMessages) =>
            {
                fetchSuccess = success;
                if (success)
                {
                    Debug.Log($"[HomeGamePlayManager] Fetched {sessionMessages.Count} session messages after speaking");
                    // 收集AgentEvents 事件到 GameContext
                    GameContext.Instance.CollectEventsByActor(sessionMessages);
                }
            }
        );

        // 检查消息获取是否成功
        if (!fetchSuccess)
        {
            Debug.LogError("[HomeGamePlayManager] Failed to fetch session messages after speaking");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Log($"[HomeGamePlayManager] SpeakToActor completed successfully: {targetActorName}");
        onComplete?.Invoke(true);
    }

    /// <summary>
    /// 切换场景
    /// 调用 /switch_stage 端点切换到目标场景
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="stageName">目标场景名称</param>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator SwitchStage(string stageName, Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(stageName))
        {
            Debug.LogError("[HomeGamePlayManager] Stage name is empty");
            onComplete?.Invoke(false);
            yield break;
        }

        // 调用 /switch_stage 端点
        yield return _homeGamePlayApi.Call(
            GameContext.Instance.HomeGamePlayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "/switch_stage",
            new Dictionary<string, string>
            {
                ["stage_name"] = stageName
            });

        // 检查API调用是否成功
        if (_homeGamePlayApi.ReqResult == null)
        {
            Debug.LogError($"[HomeGamePlayManager] /switch_stage to {stageName} request failed");
            onComplete?.Invoke(false);
            yield break;
        }

        // 进一步检查响应结果的成功标志
        if (!_homeGamePlayApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[HomeGamePlayManager] /switch_stage to {stageName} request failed: {_homeGamePlayApi.ReqResult.responseText}");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Assert(_homeGamePlayApi.RespData != null, $"[HomeGamePlayManager] /switch_stage to {stageName} response data is null");

        // 从服务器获取并同步最新的会话消息
        bool fetchSuccess = false;
        yield return SessionManager.Instance.FetchSessionMessages(
            (success, sessionMessages) =>
            {
                fetchSuccess = success;
                if (success)
                {
                    Debug.Log($"[HomeGamePlayManager] Fetched {sessionMessages.Count} session messages after stage switch");
                    // 收集AgentEvents 事件到 GameContext
                    GameContext.Instance.CollectEventsByActor(sessionMessages);
                }
            }
        );

        // 检查消息获取是否成功
        if (!fetchSuccess)
        {
            Debug.LogError($"[HomeGamePlayManager] Failed to fetch session messages after switching to {stageName}");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Log($"[HomeGamePlayManager] SwitchStage completed successfully: {stageName}");
        onComplete?.Invoke(true);
    }

    /// <summary>
    /// 传送到地下城
    /// 调用传送地下城端点，进入地下城副本
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator TransDungeon(Action<bool> onComplete = null)
    {
        // 调用传送地下城端点
        yield return _transDungeonApi.Call(
            GameContext.Instance.HomeTransDungeonUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName);

        // 检查API调用是否成功
        if (_transDungeonApi.ReqResult == null)
        {
            Debug.LogError("[HomeGamePlayManager] TransDungeon request failed");
            onComplete?.Invoke(false);
            yield break;
        }

        // 进一步检查响应结果的成功标志
        if (!_transDungeonApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[HomeGamePlayManager] TransDungeon request failed: {_transDungeonApi.ReqResult.responseText}");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Assert(_transDungeonApi.RespData != null, "[HomeGamePlayManager] TransDungeon response data is null");

        // 从服务器获取并同步最新的会话消息
        bool fetchSuccess = false;
        yield return SessionManager.Instance.FetchSessionMessages(
            (success, sessionMessages) =>
            {
                fetchSuccess = success;
                if (success)
                {
                    Debug.Log($"[HomeGamePlayManager] Fetched {sessionMessages.Count} session messages after trans dungeon");
                    // 收集AgentEvents 事件到 GameContext
                    GameContext.Instance.CollectEventsByActor(sessionMessages);
                }
            }
        );

        // 检查消息获取是否成功
        if (!fetchSuccess)
        {
            Debug.LogError("[HomeGamePlayManager] Failed to fetch session messages after trans dungeon");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Log("[HomeGamePlayManager] TransDungeon completed successfully");
        onComplete?.Invoke(true);
    }
}
