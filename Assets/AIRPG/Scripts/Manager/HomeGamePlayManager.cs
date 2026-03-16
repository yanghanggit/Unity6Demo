using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>
/// Home游戏玩法管理器
/// 单例模式，封装所有Home相关的游戏操作（POST请求）
/// 负责推进游戏、场景切换、角色交互等写操作
/// 仅依赖 GameStateSync.GetSessionMessages 获取会话消息
/// </summary>
public class HomeGamePlayManager : MonoBehaviour
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static HomeGamePlayManager Instance { get; private set; }

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

    /// <summary>
    /// 创建一个独立的临时 API 实例，挂载于自身 Transform 下。
    /// 每次调用均产生隔离对象，避免并发时共享 ReqResult / RespData 导致竞态。
    /// 调用方负责在使用完毕后通过 finally 块 Destroy 该实例的 gameObject。
    /// </summary>
    private T CreateApi<T>() where T : BaseApiClient
    {
        var go = new GameObject(typeof(T).Name);
        go.transform.SetParent(transform);
        go.hideFlags = HideFlags.HideInHierarchy;
        return go.AddComponent<T>();
    }

    /// <summary>
    /// 推进游戏状态
    /// 调用 home_advance API 推进指定角色的行动
    /// 传入空列表则推进所有角色，服务器会自动处理
    /// </summary>
    /// <param name="actors">角色名称列表，传入空列表则推进所有角色</param>
    /// <returns>成功时返回 <see cref="HomeAdvanceResponse"/>，失败时返回 null</returns>
    public async UniTask<HomeAdvanceResponse> AdvanceGame(List<string> actors)
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[HomeGamePlayManager] Player is not logged in, skip AdvanceGame");
            return null;
        }

        var api = CreateApi<HomeAdvanceApi>();
        try
        {
            // 使用 HomeAdvance API 推进角色
            await api.Call(
                GameContext.Instance.HomeAdvanceUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName,
                actors);

            if (api.ReqResult == null)
            {
                Debug.LogError($"[HomeGamePlayManager] home_advance request failed for actors: [{string.Join(", ", actors)}]");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[HomeGamePlayManager] home_advance request failed: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "[HomeGamePlayManager] home_advance response data is null");
            return api.RespData;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 发送消息给指定角色
    /// 调用 /speak 端点向目标角色发送消息
    /// </summary>
    /// <param name="targetActorName">目标角色名称</param>
    /// <param name="messageContent">消息内容</param>
    /// <returns>成功时返回 <see cref="HomePlayerActionResponse"/>，失败时返回 null</returns>
    public async UniTask<HomePlayerActionResponse> SpeakToActor(string targetActorName, string messageContent)
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[HomeGamePlayManager] Player is not logged in, skip SpeakToActor");
            return null;
        }

        if (string.IsNullOrEmpty(targetActorName) || string.IsNullOrEmpty(messageContent))
        {
            Debug.LogError("[HomeGamePlayManager] Target actor name or message content is empty");
            return null;
        }

        var api = CreateApi<HomePlayerActionApi>();
        try
        {
            await api.Call(
                GameContext.Instance.HomePlayerActionUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName,
                HomePlayerActionType.SPEAK,
                new Dictionary<string, string>
                {
                    ["target"] = targetActorName,
                    ["content"] = messageContent
                });

            if (api.ReqResult == null)
            {
                Debug.LogError("[HomeGamePlayManager] /speak request failed");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[HomeGamePlayManager] /speak request failed: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "[HomeGamePlayManager] /speak response data is null");
            return api.RespData;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 切换场景
    /// 调用 /switch_stage 端点切换到目标场景
    /// </summary>
    /// <param name="stageName">目标场景名称</param>
    /// <returns>成功时返回 <see cref="HomePlayerActionResponse"/>，失败时返回 null</returns>
    public async UniTask<HomePlayerActionResponse> SwitchStage(string stageName)
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[HomeGamePlayManager] Player is not logged in, skip SwitchStage");
            return null;
        }

        if (string.IsNullOrEmpty(stageName))
        {
            Debug.LogError("[HomeGamePlayManager] Stage name is empty");
            return null;
        }

        var api = CreateApi<HomePlayerActionApi>();
        try
        {
            await api.Call(
                GameContext.Instance.HomePlayerActionUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName,
                HomePlayerActionType.SWITCH_STAGE,
                new Dictionary<string, string>
                {
                    ["stage_name"] = stageName
                });

            if (api.ReqResult == null)
            {
                Debug.LogError($"[HomeGamePlayManager] /switch_stage to {stageName} request failed");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[HomeGamePlayManager] /switch_stage to {stageName} request failed: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, $"[HomeGamePlayManager] /switch_stage to {stageName} response data is null");
            return api.RespData;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 进入地下城
    /// 调用进入地下城端点，返回地下城响应数据
    /// </summary>
    /// <returns>成功时返回 <see cref="HomeEnterDungeonResponse"/>，失败时返回 null</returns>
    public async UniTask<HomeEnterDungeonResponse> HomeEnterDungeon()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[HomeGamePlayManager] Player is not logged in, skip HomeEnterDungeon");
            return null;
        }

        var api = CreateApi<HomeEnterDungeonApi>();
        try
        {
            await api.Call(
                GameContext.Instance.HomeEnterDungeonUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName);

            if (api.ReqResult == null)
            {
                Debug.LogError("[HomeGamePlayManager] HomeEnterDungeon request failed");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[HomeGamePlayManager] HomeEnterDungeon request failed: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "[HomeGamePlayManager] HomeEnterDungeon response data is null");
            return api.RespData;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }
}
