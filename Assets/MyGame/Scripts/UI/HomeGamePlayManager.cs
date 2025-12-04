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
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Assert(_homeGamePlayApi != null, "_homeGamePlayApi is null");
    }

    /// <summary>
    /// 推进游戏状态
    /// 调用 /advancing 端点推进场景中所有角色的行动
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator AdvanceGame(Action<bool> onComplete = null)
    {
        if (_homeGamePlayApi == null)
        {
            Debug.LogError("[HomeGamePlayManager] HomeGamePlayApi is not initialized");
            onComplete?.Invoke(false);
            yield break;
        }

        // 调用 HomeGameplay API 的 /advancing 端点
        yield return _homeGamePlayApi.Call(
            GameContext.Instance.HomeGamePlayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "/advancing");

        // 检查API调用是否成功
        if (_homeGamePlayApi.RespData == null)
        {
            Debug.LogError("[HomeGamePlayManager] /advancing request failed");
            onComplete?.Invoke(false);
            yield break;
        }

        // 从服务器获取并同步最新的会话消息
        bool fetchSuccess = false;
        yield return SessionManager.Instance.FetchSessionMessages(
            (success, sessionMessages) =>
            {
                fetchSuccess = success;
                if (success)
                {
                    Debug.Log($"[HomeGamePlayManager] Fetched {sessionMessages.Count} session messages after advancing");
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

        Debug.Log("[HomeGamePlayManager] AdvanceGame completed successfully");
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
        if (_homeGamePlayApi == null)
        {
            Debug.LogError("[HomeGamePlayManager] HomeGamePlayApi is not initialized");
            onComplete?.Invoke(false);
            yield break;
        }

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
        if (_homeGamePlayApi.RespData == null)
        {
            Debug.LogError("[HomeGamePlayManager] /speak request failed");
            onComplete?.Invoke(false);
            yield break;
        }

        // 从服务器获取并同步最新的会话消息
        bool fetchSuccess = false;
        yield return SessionManager.Instance.FetchSessionMessages(
            (success, sessionMessages) =>
            {
                fetchSuccess = success;
                if (success)
                {
                    Debug.Log($"[HomeGamePlayManager] Fetched {sessionMessages.Count} session messages after speaking");
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
        if (_homeGamePlayApi == null)
        {
            Debug.LogError("[HomeGamePlayManager] HomeGamePlayApi is not initialized");
            onComplete?.Invoke(false);
            yield break;
        }

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
        if (_homeGamePlayApi.RespData == null)
        {
            Debug.LogError($"[HomeGamePlayManager] /switch_stage to {stageName} request failed");
            onComplete?.Invoke(false);
            yield break;
        }

        // 从服务器获取并同步最新的会话消息
        bool fetchSuccess = false;
        yield return SessionManager.Instance.FetchSessionMessages(
            (success, sessionMessages) =>
            {
                fetchSuccess = success;
                if (success)
                {
                    Debug.Log($"[HomeGamePlayManager] Fetched {sessionMessages.Count} session messages after stage switch");
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
}
