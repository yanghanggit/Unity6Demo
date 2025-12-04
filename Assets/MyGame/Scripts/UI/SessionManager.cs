using System.Collections;
using UnityEngine;
using System;

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
        Debug.Assert(_loginApi != null, "_loginApi is null");
        Debug.Assert(_startApi != null, "_startApi is null");
        Debug.Assert(_logoutApi != null, "_logoutApi is null");
    }

    /// <summary>
    /// 登录游戏
    /// 调用 /login 端点进行用户认证
    /// 自动保存用户名和游戏名到 GameContext
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator Login(string userName, string gameName, Action<bool> onComplete = null)
    {
        if (_loginApi == null)
        {
            Debug.LogError("[SessionManager] LoginApi is not initialized");
            onComplete?.Invoke(false);
            yield break;
        }

        // 调用 LoginApi
        yield return _loginApi.Call(GameContext.Instance.LoginUrl, userName, gameName);

        // 检查结果
        if (_loginApi.ReqResult == null || !_loginApi.ReqResult.isSuccess)
        {
            Debug.LogError("[SessionManager] Login request failed");
            onComplete?.Invoke(false);
            yield break;
        }

        // 保存基本信息到 GameContext
        GameContext.Instance.UserName = userName;
        GameContext.Instance.GameName = gameName;
        GameContext.Instance.ActorName = ""; // 尚未分配角色

        Debug.Log($"[SessionManager] Login completed successfully: {userName}");
        onComplete?.Invoke(true);
    }

    /// <summary>
    /// 开始游戏
    /// 调用 /start 端点分配角色并初始化游戏
    /// 自动保存角色名到 GameContext
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="actorName">角色名</param>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator StartGame(string userName, string gameName, string actorName, Action<bool> onComplete = null)
    {
        if (_startApi == null)
        {
            Debug.LogError("[SessionManager] StartApi is not initialized");
            onComplete?.Invoke(false);
            yield break;
        }

        // 调用 StartApi
        yield return _startApi.Call(GameContext.Instance.StartUrl, userName, gameName, actorName);

        // 检查结果
        if (_startApi.ReqResult == null || !_startApi.ReqResult.isSuccess)
        {
            Debug.LogError("[SessionManager] Start game request failed");
            onComplete?.Invoke(false);
            yield break;
        }

        // 保存角色信息到 GameContext
        GameContext.Instance.ActorName = actorName;

        Debug.Log($"[SessionManager] StartGame completed successfully: {actorName}");
        onComplete?.Invoke(true);
    }

    /// <summary>
    /// 登出游戏
    /// 调用 /logout 端点并清理游戏上下文
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator Logout(Action<bool> onComplete = null)
    {
        if (_logoutApi == null)
        {
            Debug.LogError("[SessionManager] LogoutApi is not initialized");
            onComplete?.Invoke(false);
            yield break;
        }

        // 调用 LogoutApi
        yield return _logoutApi.Call(
            GameContext.Instance.LogoutUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName
        );

        // 检查结果
        if (_logoutApi.RespData == null)
        {
            Debug.LogError("[SessionManager] Logout request failed");
            onComplete?.Invoke(false);
            yield break;
        }

        // 清理游戏上下文
        GameContext.ClearInstance();

        Debug.Log("[SessionManager] Logout completed successfully");
        onComplete?.Invoke(true);
    }

    /// <summary>
    /// 完整的登录并开始游戏流程（组合方法）
    /// 依次执行：登录 → 开始游戏
    /// 注意：不包含游戏状态刷新，调用方需要自行处理
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="actorName">角色名</param>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator LoginAndStart(string userName, string gameName, string actorName, Action<bool> onComplete = null)
    {
        // 1. 登录
        bool loginSuccess = false;
        yield return Login(userName, gameName, (success) => loginSuccess = success);

        if (!loginSuccess)
        {
            Debug.LogError("[SessionManager] LoginAndStart failed at Login step");
            onComplete?.Invoke(false);
            yield break;
        }

        // 2. 开始游戏
        bool startSuccess = false;
        yield return StartGame(userName, gameName, actorName, (success) => startSuccess = success);

        if (!startSuccess)
        {
            Debug.LogError("[SessionManager] LoginAndStart failed at StartGame step");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Log("[SessionManager] LoginAndStart completed successfully");
        onComplete?.Invoke(true);
    }
}
