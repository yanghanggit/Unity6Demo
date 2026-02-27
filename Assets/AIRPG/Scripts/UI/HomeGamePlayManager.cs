using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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
    [SerializeField] private HomePlayerActionApi _homePlayerActionApi;

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
        Debug.Assert(_homePlayerActionApi != null, "_homeGamePlayApi is null");
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
    /// <summary>
    /// 推进游戏状态
    /// </summary>
    public async UniTask<bool> AdvanceGame(List<string> actors)
    {
        // 使用 HomeAdvance API 推进角色
        await _homeAdvanceApi.Call(
            GameContext.Instance.HomeAdvanceUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            actors);

        if (_homeAdvanceApi.ReqResult == null)
        {
            Debug.LogError($"[HomeGamePlayManager] home_advance request failed for actors: [{string.Join(", ", actors)}]");
            return false;
        }

        if (!_homeAdvanceApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[HomeGamePlayManager] home_advance request failed: {_homeAdvanceApi.ReqResult.responseText}");
            return false;
        }

        Debug.Assert(_homeAdvanceApi.RespData != null, "[HomeGamePlayManager] home_advance response data is null");

        // 从服务器获取并同步最新的会话消息
        var sessionMessages = await SessionManager.Instance.FetchSessionMessages();
        if (sessionMessages == null)
        {
            Debug.LogError("[HomeGamePlayManager] Failed to fetch session messages after advancing");
            return false;
        }

        Debug.Log($"[HomeGamePlayManager] Fetched {sessionMessages.Count} session messages after advancing");
        GameContext.Instance.CollectEventsByActor(sessionMessages);

        Debug.Log($"[HomeGamePlayManager] AdvanceGame completed successfully for actors: [{string.Join(", ", actors)}]");
        return true;
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
    /// <summary>
    /// 发送消息给指定角色
    /// </summary>
    public async UniTask<bool> SpeakToActor(string targetActorName, string messageContent)
    {
        if (string.IsNullOrEmpty(targetActorName) || string.IsNullOrEmpty(messageContent))
        {
            Debug.LogError("[HomeGamePlayManager] Target actor name or message content is empty");
            return false;
        }

        await _homePlayerActionApi.Call(
            GameContext.Instance.HomePlayerActionUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            HomePlayerActionType.SPEAK,
            new Dictionary<string, string>
            {
                ["target"] = targetActorName,
                ["content"] = messageContent
            });

        if (_homePlayerActionApi.ReqResult == null)
        {
            Debug.LogError("[HomeGamePlayManager] /speak request failed");
            return false;
        }

        if (!_homePlayerActionApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[HomeGamePlayManager] /speak request failed: {_homePlayerActionApi.ReqResult.responseText}");
            return false;
        }

        Debug.Assert(_homePlayerActionApi.RespData != null, "[HomeGamePlayManager] /speak response data is null");

        var sessionMessages = await SessionManager.Instance.FetchSessionMessages();
        if (sessionMessages == null)
        {
            Debug.LogError("[HomeGamePlayManager] Failed to fetch session messages after speaking");
            return false;
        }

        Debug.Log($"[HomeGamePlayManager] Fetched {sessionMessages.Count} session messages after speaking");
        GameContext.Instance.CollectEventsByActor(sessionMessages);

        Debug.Log($"[HomeGamePlayManager] SpeakToActor completed successfully: {targetActorName}");
        return true;
    }

    /// <summary>
    /// 切换场景
    /// 调用 /switch_stage 端点切换到目标场景
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="stageName">目标场景名称</param>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    /// <summary>
    /// 切换场景
    /// </summary>
    public async UniTask<bool> SwitchStage(string stageName)
    {
        if (string.IsNullOrEmpty(stageName))
        {
            Debug.LogError("[HomeGamePlayManager] Stage name is empty");
            return false;
        }

        await _homePlayerActionApi.Call(
            GameContext.Instance.HomePlayerActionUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            HomePlayerActionType.SWITCH_STAGE,
            new Dictionary<string, string>
            {
                ["stage_name"] = stageName
            });

        if (_homePlayerActionApi.ReqResult == null)
        {
            Debug.LogError($"[HomeGamePlayManager] /switch_stage to {stageName} request failed");
            return false;
        }

        if (!_homePlayerActionApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[HomeGamePlayManager] /switch_stage to {stageName} request failed: {_homePlayerActionApi.ReqResult.responseText}");
            return false;
        }

        Debug.Assert(_homePlayerActionApi.RespData != null, $"[HomeGamePlayManager] /switch_stage to {stageName} response data is null");

        var sessionMessages = await SessionManager.Instance.FetchSessionMessages();
        if (sessionMessages == null)
        {
            Debug.LogError($"[HomeGamePlayManager] Failed to fetch session messages after switching to {stageName}");
            return false;
        }

        Debug.Log($"[HomeGamePlayManager] Fetched {sessionMessages.Count} session messages after stage switch");
        GameContext.Instance.CollectEventsByActor(sessionMessages);

        Debug.Log($"[HomeGamePlayManager] SwitchStage completed successfully: {stageName}");
        return true;
    }

    /// <summary>
    /// 传送到地下城
    /// 调用传送地下城端点，进入地下城副本
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    /// <summary>
    /// 传送到地下城
    /// </summary>
    public async UniTask<bool> TransDungeon()
    {
        await _transDungeonApi.Call(
            GameContext.Instance.HomeTransDungeonUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName);

        if (_transDungeonApi.ReqResult == null)
        {
            Debug.LogError("[HomeGamePlayManager] TransDungeon request failed");
            return false;
        }

        if (!_transDungeonApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[HomeGamePlayManager] TransDungeon request failed: {_transDungeonApi.ReqResult.responseText}");
            return false;
        }

        Debug.Assert(_transDungeonApi.RespData != null, "[HomeGamePlayManager] TransDungeon response data is null");

        var sessionMessages = await SessionManager.Instance.FetchSessionMessages();
        if (sessionMessages == null)
        {
            Debug.LogError("[HomeGamePlayManager] Failed to fetch session messages after trans dungeon");
            return false;
        }

        Debug.Log($"[HomeGamePlayManager] Fetched {sessionMessages.Count} session messages after trans dungeon");
        GameContext.Instance.CollectEventsByActor(sessionMessages);

        Debug.Log("[HomeGamePlayManager] TransDungeon completed successfully");
        return true;
    }
}
