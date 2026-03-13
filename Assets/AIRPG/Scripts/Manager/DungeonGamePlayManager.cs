using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>
/// Dungeon游戏玩法管理器
/// 单例模式，封装所有Dungeon相关的游戏操作（POST请求）
/// 负责战斗初始化、抽卡、打牌、地下城推进、传送回家等写操作
/// 不负责会话消息同步，由调用方根据需要自行处理
/// </summary>
public class DungeonGamePlayManager : MonoBehaviour
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static DungeonGamePlayManager Instance { get; private set; }

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
            Debug.LogWarning("[DungeonGamePlayManager] Duplicate instance detected, destroying the new one.");
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
    /// 初始化战斗
    /// 调用 combat_init 端点开始战斗
    /// </summary>
    /// <returns>战斗初始化响应，失败时返回 null</returns>
    public async UniTask<DungeonCombatInitResponse> InitCombat()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[DungeonGamePlayManager] Player is not logged in, skip InitCombat");
            return null;
        }

        var api = CreateApi<DungeonCombatInitApi>();
        try
        {
            await api.Call(
                GameContext.Instance.DungeonCombatInitUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName);

            if (api.ReqResult == null)
            {
                Debug.LogError("[DungeonGamePlayManager] CombatInit: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[DungeonGamePlayManager] CombatInit failed: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "DungeonCombatInitApi response data is null");
            Debug.Log("[DungeonGamePlayManager] CombatInit completed successfully");
            return api.RespData;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 抽卡
    /// 调用地下城战斗盟友抽牌端点执行抽卡操作
    /// </summary>
    /// <returns>盟友抽牌响应，失败时返回 null</returns>
    public async UniTask<DungeonCombatDrawAllyCardsResponse> DrawAllyCards(List<AllyDrawCardAction> specifiedActions)
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[DungeonGamePlayManager] Player is not logged in, skip DrawCards");
            return null;
        }

        var api = CreateApi<DungeonCombatDrawAllyCardsApi>();
        try
        {
            await api.Call(
                GameContext.Instance.DungeonCombatDrawAllyCardsUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName,
                specifiedActions);

            if (api.ReqResult == null)
            {
                Debug.LogError("[DungeonGamePlayManager] DrawCards: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[DungeonGamePlayManager] DrawCards failed: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "DungeonCombatDrawCardsApi response data is null");
            if (string.IsNullOrEmpty(api.RespData.task_id) ||
                api.RespData.status != TaskStatus.RUNNING)
            {
                Debug.LogError("[DungeonGamePlayManager] DrawCards: response data is invalid");
                return null;
            }

            Debug.Log("[DungeonGamePlayManager] DrawCards completed successfully");
            return api.RespData;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 抽敌人牌
    /// 调用地下城战斗敌方抽牌端点执行抽卡操作
    /// </summary>
    /// <returns>敌方抽牌响应，失败时返回 null</returns>
    public async UniTask<DungeonCombatDrawEnemyCardsResponse> DrawEnemyCards()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[DungeonGamePlayManager] Player is not logged in, skip DrawEnemyCards");
            return null;
        }

        var api = CreateApi<DungeonCombatDrawEnemyCardsApi>();
        try
        {
            await api.Call(
                GameContext.Instance.DungeonCombatDrawEnemyCardsUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName);

            if (api.ReqResult == null)
            {
                Debug.LogError("[DungeonGamePlayManager] DrawEnemyCards: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[DungeonGamePlayManager] DrawEnemyCards failed: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "DungeonCombatDrawEnemyCardsApi response data is null");
            if (string.IsNullOrEmpty(api.RespData.task_id) ||
                api.RespData.status != TaskStatus.RUNNING)
            {
                Debug.LogError("[DungeonGamePlayManager] DrawEnemyCards: response data is invalid");
                return null;
            }

            Debug.Log("[DungeonGamePlayManager] DrawEnemyCards completed successfully");
            return api.RespData;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 打牌
    /// 调用 play_cards 端点执行打牌操作
    /// </summary>
    /// <returns>任务 ID 字符串，失败时返回 null</returns>
    public async UniTask<DungeonCombatPlayCardsResponse> PlayCards()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[DungeonGamePlayManager] Player is not logged in, skip PlayCards");
            return null;
        }

        var api = CreateApi<DungeonCombatPlayCardsApi>();
        try
        {
            await api.Call(
                GameContext.Instance.DungeonCombatPlayCardsUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName);

            if (api.ReqResult == null)
            {
                Debug.LogError("[DungeonGamePlayManager] PlayCards: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[DungeonGamePlayManager] PlayCards failed: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "DungeonCombatPlayCardsApi response data is null");
            if (string.IsNullOrEmpty(api.RespData.task_id) ||
                api.RespData.status != TaskStatus.RUNNING)
            {
                Debug.LogError("[DungeonGamePlayManager] PlayCards: response data is invalid");
                return null;
            }

            Debug.Log("[DungeonGamePlayManager] PlayCards completed successfully");
            return api.RespData;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 前进到下一个地下城
    /// 调用 advance_next_dungeon 端点推进地下城进度
    /// </summary>
    /// <returns>地下城推进响应，失败时返回 null</returns>
    public async UniTask<DungeonAdvanceStageResponse> AdvanceStage()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[DungeonGamePlayManager] Player is not logged in, skip AdvanceStage");
            return null;
        }

        var api = CreateApi<DungeonAdvanceStageApi>();
        try
        {
            await api.Call(
                GameContext.Instance.DungeonAdvanceStageUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName);

            if (api.ReqResult == null)
            {
                Debug.LogError("[DungeonGamePlayManager] AdvanceNextDungeon: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[DungeonGamePlayManager] AdvanceNextDungeon failed: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "DungeonAdvanceStageApi response data is null");
            Debug.Log("[DungeonGamePlayManager] AdvanceNextDungeon completed successfully");
            return api.RespData;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 从地下城撤退
    /// 调用 retreat 端点从地下城撤退
    /// </summary>
    /// <returns>撤退响应，失败时返回 null</returns>
    public async UniTask<DungeonCombatRetreatResponse> RetreatFromDungeon()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[DungeonGamePlayManager] Player is not logged in, skip RetreatFromDungeon");
            return null;
        }

        var api = CreateApi<DungeonCombatRetreatApi>();
        try
        {
            await api.Call(
                GameContext.Instance.DungeonCombatRetreatUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName);

            if (api.ReqResult == null)
            {
                Debug.LogError("[DungeonGamePlayManager] RetreatFromDungeon: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[DungeonGamePlayManager] RetreatFromDungeon failed: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "DungeonCombatRetreatApi response data is null");
            Debug.Log("[DungeonGamePlayManager] RetreatFromDungeon completed successfully");
            return api.RespData;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }

    /// <summary>
    /// 退出地下城
    /// 调用退出地下城端点,返回主场景
    /// </summary>
    /// <returns>成功返回 true,失败返回 false</returns>
    public async UniTask<DungeonExitResponse> ExitDungeon()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[DungeonGamePlayManager] Player is not logged in, skip ExitDungeon");
            return null;
        }

        var api = CreateApi<DungeonExitApi>();
        try
        {
            await api.Call(
                GameContext.Instance.DungeonExitUrl,
                GameContext.Instance.UserName,
                GameContext.Instance.GameName);

            if (api.ReqResult == null)
            {
                Debug.LogError("[DungeonGamePlayManager] ExitDungeon: request result is null");
                return null;
            }

            if (!api.ReqResult.isSuccess)
            {
                Debug.LogError($"[DungeonGamePlayManager] ExitDungeon failed: {api.ReqResult.responseText}");
                return null;
            }

            Debug.Assert(api.RespData != null, "DungeonExitApi response data is null");
            Debug.Log("[DungeonGamePlayManager] ExitDungeon completed successfully");
            return api.RespData;
        }
        finally
        {
            Destroy(api.gameObject);
        }
    }
}
