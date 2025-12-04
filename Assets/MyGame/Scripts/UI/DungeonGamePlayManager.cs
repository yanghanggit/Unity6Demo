using System.Collections;
using UnityEngine;
using System;

/// <summary>
/// Dungeon游戏玩法管理器
/// 单例模式，封装所有Dungeon相关的游戏操作（POST请求）
/// 负责战斗初始化、抽卡、打牌、地下城推进、传送回家等写操作
/// 仅依赖 SessionManager.FetchSessionMessages 获取会话消息
/// </summary>
public class DungeonGamePlayManager : MonoBehaviour
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static DungeonGamePlayManager Instance { get; private set; }

    /// <summary>
    /// Dungeon游戏玩法API接口
    /// </summary>
    [SerializeField] private DungeonGamePlayApi _dungeonGamePlayApi;

    /// <summary>
    /// 传送回家API接口
    /// </summary>
    [SerializeField] private TransHomeApi _transHomeApi;

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
        Debug.Assert(_dungeonGamePlayApi != null, "_dungeonGamePlayApi is null");
        Debug.Assert(_transHomeApi != null, "_transHomeApi is null");
    }

    /// <summary>
    /// 初始化战斗
    /// 调用 combat_init 端点开始战斗
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator CombatInit(Action<bool> onComplete = null)
    {
        if (_dungeonGamePlayApi == null)
        {
            Debug.LogError("[DungeonGamePlayManager] DungeonGamePlayApi is not initialized");
            onComplete?.Invoke(false);
            yield break;
        }

        // 调用 combat_init 端点
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "combat_init");

        // 检查API调用是否成功
        if (_dungeonGamePlayApi.RespData == null)
        {
            Debug.LogError("[DungeonGamePlayManager] combat_init request failed");
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
                    Debug.Log($"[DungeonGamePlayManager] Fetched {sessionMessages.Count} session messages after combat init");
                }
            }
        );

        // 检查消息获取是否成功
        if (!fetchSuccess)
        {
            Debug.LogError("[DungeonGamePlayManager] Failed to fetch session messages after combat init");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Log("[DungeonGamePlayManager] CombatInit completed successfully");
        onComplete?.Invoke(true);
    }

    /// <summary>
    /// 抽卡
    /// 调用 draw_cards 端点执行抽卡操作
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator DrawCards(Action<bool> onComplete = null)
    {
        if (_dungeonGamePlayApi == null)
        {
            Debug.LogError("[DungeonGamePlayManager] DungeonGamePlayApi is not initialized");
            onComplete?.Invoke(false);
            yield break;
        }

        // 调用 draw_cards 端点
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "draw_cards");

        // 检查API调用是否成功
        if (_dungeonGamePlayApi.RespData == null)
        {
            Debug.LogError("[DungeonGamePlayManager] draw_cards request failed");
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
                    Debug.Log($"[DungeonGamePlayManager] Fetched {sessionMessages.Count} session messages after draw cards");
                }
            }
        );

        // 检查消息获取是否成功
        if (!fetchSuccess)
        {
            Debug.LogError("[DungeonGamePlayManager] Failed to fetch session messages after draw cards");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Log("[DungeonGamePlayManager] DrawCards completed successfully");
        onComplete?.Invoke(true);
    }

    /// <summary>
    /// 打牌
    /// 调用 play_cards 端点执行打牌操作
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator PlayCards(Action<bool> onComplete = null)
    {
        if (_dungeonGamePlayApi == null)
        {
            Debug.LogError("[DungeonGamePlayManager] DungeonGamePlayApi is not initialized");
            onComplete?.Invoke(false);
            yield break;
        }

        // 调用 play_cards 端点
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "play_cards");

        // 检查API调用是否成功
        if (_dungeonGamePlayApi.RespData == null)
        {
            Debug.LogError("[DungeonGamePlayManager] play_cards request failed");
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
                    Debug.Log($"[DungeonGamePlayManager] Fetched {sessionMessages.Count} session messages after play cards");
                }
            }
        );

        // 检查消息获取是否成功
        if (!fetchSuccess)
        {
            Debug.LogError("[DungeonGamePlayManager] Failed to fetch session messages after play cards");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Log("[DungeonGamePlayManager] PlayCards completed successfully");
        onComplete?.Invoke(true);
    }

    /// <summary>
    /// 前进到下一个地下城
    /// 调用 advance_next_dungeon 端点推进地下城进度
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator AdvanceNextDungeon(Action<bool> onComplete = null)
    {
        if (_dungeonGamePlayApi == null)
        {
            Debug.LogError("[DungeonGamePlayManager] DungeonGamePlayApi is not initialized");
            onComplete?.Invoke(false);
            yield break;
        }

        // 调用 advance_next_dungeon 端点
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "advance_next_dungeon");

        // 检查API调用是否成功
        if (_dungeonGamePlayApi.RespData == null)
        {
            Debug.LogError("[DungeonGamePlayManager] advance_next_dungeon request failed");
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
                    Debug.Log($"[DungeonGamePlayManager] Fetched {sessionMessages.Count} session messages after advance dungeon");
                }
            }
        );

        // 检查消息获取是否成功
        if (!fetchSuccess)
        {
            Debug.LogError("[DungeonGamePlayManager] Failed to fetch session messages after advance dungeon");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Log("[DungeonGamePlayManager] AdvanceNextDungeon completed successfully");
        onComplete?.Invoke(true);
    }

    /// <summary>
    /// 传送回家
    /// 调用传送回家端点，返回主场景
    /// 自动获取最新会话消息
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator TransHome(Action<bool> onComplete = null)
    {
        if (_transHomeApi == null)
        {
            Debug.LogError("[DungeonGamePlayManager] TransHomeApi is not initialized");
            onComplete?.Invoke(false);
            yield break;
        }

        // 调用传送回家端点
        yield return _transHomeApi.Call(
            GameContext.Instance.DungeonTransHomeUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName);

        // 检查API调用是否成功
        if (_transHomeApi.RespData == null)
        {
            Debug.LogError("[DungeonGamePlayManager] TransHome request failed");
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
                    Debug.Log($"[DungeonGamePlayManager] Fetched {sessionMessages.Count} session messages after trans home");
                }
            }
        );

        // 检查消息获取是否成功
        if (!fetchSuccess)
        {
            Debug.LogError("[DungeonGamePlayManager] Failed to fetch session messages after trans home");
            onComplete?.Invoke(false);
            yield break;
        }

        Debug.Log("[DungeonGamePlayManager] TransHome completed successfully");
        onComplete?.Invoke(true);
    }
}
