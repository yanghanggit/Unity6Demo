using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

/// <summary>
/// 游戏会话管理器
/// 单例模式，封装游戏会话的完整生命周期管理
/// 负责登录、开始游戏、登出等会话级别的操作
/// 保持纯粹的会话管理职责，不处理游戏状态同步等业务逻辑
/// </summary>
public class SessionManager : MonoBehaviour
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static SessionManager Instance { get; private set; }

    /// <summary>
    /// 登录 API 接口
    /// </summary>
    [SerializeField] private LoginApi _loginApi;

    /// <summary>
    /// 开始游戏 API 接口
    /// </summary>
    [SerializeField] private StartApi _startApi;

    /// <summary>
    /// 登出 API 接口
    /// </summary>
    [SerializeField] private LogoutApi _logoutApi;

    /// <summary>
    /// 会话消息API接口
    /// 用于从服务器获取游戏会话消息列表，支持基于序列ID的增量拉取
    /// </summary>
    [SerializeField] private SessionMessagesApi _sessionMessagesApi;

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
            Debug.LogWarning("[SessionManager] Duplicate instance detected, destroying the new one.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Assert(_loginApi != null, "_loginApi is null");
        Debug.Assert(_startApi != null, "_startApi is null");
        Debug.Assert(_logoutApi != null, "_logoutApi is null");
        Debug.Assert(_sessionMessagesApi != null, "_sessionMessagesApi is null");
    }

    /// <summary>
    /// 登录游戏
    /// <summary>
    /// 登录
    /// 调用 /login 端点进行用户认证
    /// 自动保存用户名和游戏名到 GameContext
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <returns>是否成功</returns>
    public async UniTask<bool> Login(string userName, string gameName)
    {
        await _loginApi.Call(GameContext.Instance.LoginUrl, userName, gameName);

        if (_loginApi.ReqResult == null || !_loginApi.ReqResult.isSuccess)
        {
            Debug.LogError("[SessionManager] Login request failed");
            return false;
        }

        GameContext.Instance.UserName = userName;
        GameContext.Instance.GameName = gameName;
        GameContext.Instance.PlayerActorName = string.Empty;

        return true;
    }

    /// <summary>
    /// 开始游戏
    /// 调用 /start 端点分配角色并初始化游戏
    /// 自动保存角色名到 GameContext
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <returns>是否成功</returns>
    public async UniTask<bool> StartGame(string userName, string gameName)
    {
        // 调用 StartApi
        await _startApi.Call(GameContext.Instance.StartUrl, userName, gameName);

        // 检查结果
        if (_startApi.ReqResult == null || !_startApi.ReqResult.isSuccess)
        {
            Debug.LogError("[SessionManager] Start game request failed");
            return false;
        }

        // 保存角色信息到 GameContext
        GameContext.Instance.PlayerActorName = _startApi.RespData.blueprint.player_actor;
        GameContext.Instance.PlayerOnlyStageName = _startApi.RespData.blueprint.player_only_stage;

        return true;
    }

    /// <summary>
    /// 登出游戏
    /// 调用 /logout 端点并清理游戏上下文
    /// </summary>
    /// <returns>是否成功</returns>
    public async UniTask<bool> Logout()
    {
        // 调用 LogoutApi
        await _logoutApi.Call(
            GameContext.Instance.LogoutUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName
        );

        // 检查结果
        if (_logoutApi.ReqResult == null)
        {
            Debug.LogError("[SessionManager] Logout request failed: request result is null");
            return false;
        }

        if (!_logoutApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[SessionManager] Logout request failed: {_logoutApi.ReqResult.responseText}");
            return false;
        }

        Debug.Assert(_logoutApi.RespData != null, "[SessionManager] Logout response data is null");

        // 清理游戏上下文
        GameContext.ClearInstance();

        return true;
    }

    /// <summary>
    /// 完整的登录并开始游戏流程（组合方法）
    /// 依次执行：登录 → 开始游戏
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <returns>是否成功</returns>
    public async UniTask<bool> LoginAndStart(string userName, string gameName)
    {
        // 1. 登录
        bool isLoginSuccessful = await Login(userName, gameName);
        if (!isLoginSuccessful)
        {
            Debug.LogError("[SessionManager] LoginAndStart failed at Login step");
            return false;
        }

        // 2. 开始游戏
        bool isStartSuccessful = await StartGame(userName, gameName);
        if (!isStartSuccessful)
        {
            Debug.LogError("[SessionManager] LoginAndStart failed at StartGame step");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 从服务器获取会话消息
    /// 获取最新的会话消息列表并更新序列ID
    /// </summary>
    /// <returns>会话消息列表，失败时返回 null</returns>
    public async UniTask<List<SessionMessage>> FetchSessionMessages()
    {
        if (string.IsNullOrEmpty(GameContext.Instance.UserName) ||
            string.IsNullOrEmpty(GameContext.Instance.GameName))
        {
            Debug.LogError("[SessionManager] UserName or GameName is not set in GameContext");
            return null;
        }

        // 获取会话消息
        await _sessionMessagesApi.Call(GameContext.Instance.SessionMessagesUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            GameContext.Instance.LastSequenceId);

        if (_sessionMessagesApi.ReqResult == null)
        {
            Debug.LogError("[SessionManager] Failed to fetch session messages from server: request result is null");
            return null;
        }

        if (!_sessionMessagesApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[SessionManager] Failed to fetch session messages from server: {_sessionMessagesApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_sessionMessagesApi.RespData != null, "[SessionManager] SessionMessagesApi response data is null");

        // 更新最后一个序列ID
        if (_sessionMessagesApi.RespLastSequenceId >= 0)
        {
            GameContext.Instance.LastSequenceId = _sessionMessagesApi.RespLastSequenceId;
        }

        return new List<SessionMessage>(_sessionMessagesApi.RespData.session_messages);
    }
}
