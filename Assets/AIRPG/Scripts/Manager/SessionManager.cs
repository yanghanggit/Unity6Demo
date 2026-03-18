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
    /// 新建游戏 API 接口
    /// </summary>
    [SerializeField] private NewGameApi _newGameApi;

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
            Debug.LogWarning("[SessionManager] Duplicate instance detected, destroying the new one.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Assert(_loginApi != null, "_loginApi is null");
        Debug.Assert(_newGameApi != null, "_newGameApi is null");
        Debug.Assert(_logoutApi != null, "_logoutApi is null");
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
    /// 新建游戏
    /// 调用 /api/game/new/v1/ 端点创建并初始化游戏会话
    /// 自动保存角色名到 GameContext
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="gameName">游戏名</param>
    /// <returns>是否成功</returns>
    public async UniTask<bool> NewGame(string userName, string gameName)
    {
        // 调用 NewGameApi
        await _newGameApi.Call(GameContext.Instance.NewGameUrl, userName, gameName);

        // 检查结果
        if (_newGameApi.ReqResult == null || !_newGameApi.ReqResult.isSuccess)
        {
            Debug.LogError("[SessionManager] NewGame request failed");
            return false;
        }

        // 保存角色信息到 GameContext
        GameContext.Instance.PlayerActorName = _newGameApi.RespData.blueprint.player_actor;
        GameContext.Instance.PlayerOnlyStageName = _newGameApi.RespData.blueprint.player_only_stage;

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
    // public async UniTask<bool> LoginAndStart(string userName, string gameName)
    // {
    //     // 1. 登录
    //     bool isLoginSuccessful = await Login(userName, gameName);
    //     if (!isLoginSuccessful)
    //     {
    //         Debug.LogError("[SessionManager] LoginAndStart failed at Login step");
    //         return false;
    //     }

    //     // 2. 开始游戏
    //     bool isStartSuccessful = await StartGame(userName, gameName);
    //     if (!isStartSuccessful)
    //     {
    //         Debug.LogError("[SessionManager] LoginAndStart failed at StartGame step");
    //         return false;
    //     }

    //     return true;
    // }
}
