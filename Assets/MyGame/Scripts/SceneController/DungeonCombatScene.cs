using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DungeonCombatScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string _preScene = "MainScene";
    [SerializeField] private string _nextScene = "DungeonCombatScene";

    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText;
    [SerializeField] private GameObject _allyAvatarContainer;  // 我方角色头像容器(左侧)
    [SerializeField] private GameObject _enemyAvatarContainer; // 敌方角色头像容器(右侧)
    [SerializeField] private GameObject _actorAvatarPrefab;    // 角色头像预制体
    [SerializeField] private StringGameEvent _onActorAvatarsRefreshEvent; // 角色头像刷新事件

    [Header("API Components")]
    [SerializeField] private TasksStatusApi _tasksStatusApi;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_allyAvatarContainer != null, "_allyAvatarContainer is null");
        Debug.Assert(_enemyAvatarContainer != null, "_enemyAvatarContainer is null");
        Debug.Assert(_actorAvatarPrefab != null, "_actorAvatarPrefab is null");
        Debug.Assert(_onActorAvatarsRefreshEvent != null, "_onActorAvatarsRefreshEvent is null");
        Debug.Assert(_tasksStatusApi != null, "_tasksStatusApi is null");

        // 检查是否已经连接服务器
        if (ApiEndpointsManager.GameRootResponse != null)
        {
            // 已经连接服务器，开始初始化战斗场景
            var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);
            _mainText.text = $"{GameContext.Instance.Dungeon.name} | {stageName} : Initializing combat scene...";

            StartCoroutine(ExecuteCombatInit());
        }
        else
        {
            // 没有连接服务器，基本是本地测试模式
            Debug.Log("DungeonCombatScene Start: RootResp is null, running in local test mode");
        }
    }

    public void OnClickViewDungeon()
    {
        Debug.Log("OnClickViewDungeon");
        StartCoroutine(RefreshDungeonStateDisplay());
    }

    public void OnClickViewActor()
    {
        Debug.Log("OnClickViewActor");
        StartCoroutine(ExecuteViewActorStats());
    }

    public void OnClickViewCards()
    {
        Debug.Log("OnClickViewCards");
        StartCoroutine(ExecuteViewActorCards());
    }

    public void OnClickDrawCards()
    {
        Debug.Log("OnClickDrawCards");
        StartCoroutine(ExecuteDrawCardsAndShowHands());
    }

    public void OnClickPlayCards()
    {
        Debug.Log("OnClickPlayCards");
        StartCoroutine(ExecutePlayCardsAndShowResult());
    }

    public void OnClickAdvanceNextDungeon()
    {
        Debug.Log("OnClickAdvanceNextDungeon");
        StartCoroutine(ExecuteAdvanceNextDungeon());
    }

    public void OnClickBackHome()
    {
        Debug.Log("OnClickBackHome");
        StartCoroutine(ExecuteBackHome());
    }

    /// <summary>
    /// 初始化战斗并刷新地下城状态
    /// 调用服务器 combat_init 接口开始战斗，成功后刷新并显示当前地下城状态
    /// </summary>
    private IEnumerator ExecuteCombatInit()
    {
        bool success = false;
        yield return DungeonGamePlayManager.Instance.CombatInit(
            (result, message, sessionMessages) =>
            {
                success = result;
                if (!result)
                {
                    _mainText.text = message;
                }
            });

        if (success)
        {
            yield return RefreshDungeonStateDisplay();
            InitializeActorAvatars();
        }
    }

    /// <summary>
    /// 执行抽卡操作并显示所有角色的手牌
    /// 调用服务器 draw_cards 接口，刷新角色数据后显示每个角色的手牌信息
    /// </summary>
    private IEnumerator ExecuteDrawCardsAndShowHands()
    {
        // 先刷新数据，检查是否有角色已经持有手牌
        bool anyActorHasHandCards = false;
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer(
            (result, message) =>
            {
                if (result)
                {
                    anyActorHasHandCards = AnyActorHasHandCards();
                }
            }
        );

        if (anyActorHasHandCards)
        {
            _mainText.text = "角色已有手牌，跳过抽卡操作。";
            yield break;
        }

        // 正式的抽卡操作
        // 步骤函数定义
        bool success = false;
        List<SessionMessage> sessionMessages = null;
        yield return DungeonGamePlayManager.Instance.DrawCards(
            (result, message, messages) =>
            {
                success = result;
                sessionMessages = messages;
                if (!result)
                {
                    _mainText.text = message;
                }
            });

        // Early return: 抽卡失败
        if (!success)
        {
            Debug.LogWarning("DrawCards action failed");
            yield break;
        }

        // 检查是否有战斗完成事件
        var combatCompleteEvents = GetCombatCompleteEvents(sessionMessages);
        if (combatCompleteEvents.Count > 0)
        {
            DisplayCombatCompleteEvents(combatCompleteEvents);

            yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

            RefreshActorAvatars();

            // 已经显示战斗完成结果，直接返回，不要再显示手牌信息！
            yield break;
        }

        // 刷新角色数据并显示手牌信息
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

        // 更新角色头像的死亡状态
        RefreshActorAvatars();

        // 显示所有角色的手牌信息
        DisplayAllActorsHands();
    }

    /// <summary>
    /// 检查是否有任意角色持有手牌
    /// 遍历所有角色实体,检查其手牌组件是否包含卡牌
    /// 用于在抽卡操作前判断是否需要跳过抽卡(避免重复抽卡)
    /// </summary>
    /// <returns>如果至少有一个角色持有手牌则返回 true,否则返回 false</returns>
    private bool AnyActorHasHandCards()
    {
        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;

        foreach (var actorEntity in actorEntitiesSerialization)
        {
            var handComponent = GameUtils.GetComponent<HandComponent>(actorEntity);
            if (handComponent != null && handComponent.cards.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 显示所有角色的手牌信息
    /// 遍历所有角色实体，提取手牌组件并格式化显示
    /// </summary>
    private void DisplayAllActorsHands()
    {
        var text = string.Empty;

        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;
        foreach (var actorEntity in actorEntitiesSerialization)
        {
            var handComponent = GameUtils.GetComponent<HandComponent>(actorEntity);
            if (handComponent == null)
            {
                //Debug.Log($"HandComponent is null for actor: {actorEntity.name}");
                continue;
            }

            text += GameUtils.FormatHandComponent(handComponent);
            text += "\n";
        }

        if (string.IsNullOrEmpty(text))
        {
            _mainText.text = "当前没有角色持有手牌信息。";
        }
        else
        {
            _mainText.text = "当前角色手牌信息：\n\n" + text;
        }
    }

    /// <summary>
    /// 执行打牌操作并轮询任务状态，完成后显示结果
    /// 调用服务器 play_cards 接口，获取任务ID后轮询查询任务状态
    /// 当任务完成时，刷新数据并显示战斗仲裁结果
    /// </summary>
    private IEnumerator ExecutePlayCardsAndShowResult()
    {
        bool success = false;
        string taskId = null;

        yield return DungeonGamePlayManager.Instance.PlayCards(
            (result, message, id) =>
            {
                success = result;
                taskId = id;
                if (result)
                {
                    Debug.Log($"PlayCards initiated successfully, task ID: {taskId}");
                    _mainText.text = "打牌请求已提交，正在处理中...";
                }
                else
                {
                    _mainText.text = message;
                }
            });

        if (!success || string.IsNullOrEmpty(taskId))
        {
            yield break;
        }

        // 轮询查询任务状态
        yield return PollTaskStatus(taskId);
    }

    /// <summary>
    /// 轮询查询任务状态直到完成或失败
    /// 委托 TasksStatusApi 执行轮询逻辑，成功完成后调用 ShowPlayCardsResult 显示战斗结果
    /// </summary>
    /// <param name="taskId">要查询的任务ID</param>
    /// <param name="pollInterval">轮询间隔时间（秒），默认2秒</param>
    /// <param name="maxAttempts">最大轮询次数，默认60次（即2分钟超时）</param>
    private IEnumerator PollTaskStatus(string taskId)
    {
        bool isSuccess = false;
        string message = "";
        TaskRecord taskRecord = null;

        // 调用 TasksStatusApi 的轮询方法，将轮询逻辑委托给API层处理
        yield return _tasksStatusApi.PollTaskStatus(
            GameContext.Instance.TasksStatusUrl,
            taskId,
            (success, msg, record) =>
            {
                isSuccess = success;
                message = msg;
                taskRecord = record;

                // 失败时立即更新UI显示错误信息
                if (!success)
                {
                    _mainText.text = msg;
                }
            }
        );

        // 任务未成功完成，直接返回（错误信息已在回调中显示）
        if (!isSuccess)
        {
            yield break;
        }

        // 任务成功完成，更新UI并显示结果
        _mainText.text = "打牌处理完成，正在加载结果...";
        yield return ShowPlayCardsResult();
    }

    /// <summary>
    /// 显示打牌结果
    /// 刷新地下城和角色数据，更新角色头像状态，显示最新回合的战斗仲裁信息
    /// (原 ExecutePlayCardsAndShowResult 的后半段逻辑)
    /// </summary>
    private IEnumerator ShowPlayCardsResult()
    {
        // 刷新地下城和角色数据
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

        // 更新角色头像的死亡状态
        RefreshActorAvatars();

        // 获取最新的地下城回合信息
        Round round = GameUtils.GetLastRound(GameContext.Instance.Dungeon);
        if (round == null)
        {
            Debug.LogWarning("No rounds found in dungeon after playing cards");
            _mainText.text = "打牌完成，但未找到回合信息";
            yield break;
        }

        // 显示最新的地下城战斗仲裁信息
        var formattedRoundInfo = GameUtils.FormatRoundInfo(round);
        if (!string.IsNullOrEmpty(formattedRoundInfo))
        {
            _mainText.text = formattedRoundInfo;
        }
        else
        {
            Debug.LogWarning("No combat arbitration info available in dungeon state");
            _mainText.text = "打牌完成，但未找到战斗仲裁信息";
        }
    }

    /// <summary>
    /// 从会话消息中提取战斗完成事件
    /// 查找并返回所有战斗完成事件的列表
    /// </summary>
    /// <param name="sessionMessages">会话消息列表</param>
    /// <returns>战斗完成事件列表，如果没有找到则返回空列表</returns>
    private List<CombatCompleteEvent> GetCombatCompleteEvents(List<SessionMessage> sessionMessages)
    {
        var completeEvents = new List<CombatCompleteEvent>();

        if (sessionMessages == null || sessionMessages.Count == 0)
        {
            return completeEvents;
        }

        foreach (var msg in sessionMessages)
        {
            var agentEvent = GameUtils.ParseAgentEvent(msg);
            if (agentEvent == null)
            {
                Debug.LogWarning("Failed to parse AgentEvent from session message");
                continue;
            }

            if (agentEvent is CombatCompleteEvent completeEvent)
            {
                completeEvents.Add(completeEvent);
            }
            else
            {
                Debug.Log($"Skipping non-combat complete event of type: {agentEvent.GetType().Name}");
            }
        }

        return completeEvents;
    }

    /// <summary>
    /// 显示战斗完成事件结果
    /// 格式化战斗完成事件列表并更新主文本显示
    /// </summary>
    /// <param name="events">战斗完成事件列表</param>
    private void DisplayCombatCompleteEvents(List<CombatCompleteEvent> events)
    {
        if (events == null || events.Count == 0)
        {
            Debug.LogWarning("No combat complete events to display");
            return;
        }

        var text = string.Empty;
        foreach (var evt in events)
        {
            text += $"Actor: {GameUtils.GetDisplayName(evt.actor)}\nSummary: \n{evt.summary}\n\n";
        }

        _mainText.text = "战斗完成！\n\n" + text;
    }


    /// <summary>
    /// 刷新并显示地下城状态
    /// 从服务器获取最新的地下城和角色数据，然后更新UI显示当前场景的角色分布和战斗信息
    /// </summary>
    private IEnumerator RefreshDungeonStateDisplay()
    {
        // 从服务器刷新地下城和角色数据
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

        // 更新角色头像的死亡状态
        RefreshActorAvatars();

        // 获取当前角色所在场景及该场景中的所有角色
        var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);
        Debug.Assert(stageName != "", "[GameStateSync] Current actor's stage name is empty");

        // 需要所有的角色名称列表！
        var actorsInStage = GameContext.Instance.GetActorsInStage(stageName);

        // 格式化并显示地下城状态（包括场景-角色映射和战斗序列信息）
        _mainText.text = GameUtils.FormatDungeonStateDisplay(GameContext.Instance.Dungeon, new Dictionary<string, List<string>> { { stageName, actorsInStage } });
    }

    /// <summary>
    /// 查看并显示所有角色的战斗属性
    /// 从服务器刷新数据后，获取所有角色的战斗属性组件并格式化显示
    /// </summary>
    private IEnumerator ExecuteViewActorStats()
    {
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

        var text = "";
        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;
        for (int i = 0; i < actorEntitiesSerialization.Count; i++)
        {
            var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(actorEntitiesSerialization[i]);
            if (combatStatsComponent == null)
            {
                Debug.Assert(false, "combatStatsComponent is null");
                continue;
            }
            text += GameUtils.FormatCombatStatsComponent(combatStatsComponent);
            text += "\n";
        }
        _mainText.text = text;
    }

    /// <summary>
    /// 查看并显示所有角色的手牌信息
    /// 从服务器刷新数据后,直接显示所有角色当前持有的手牌
    /// 用于在游戏过程中随时查看角色的手牌状态
    /// </summary>
    private IEnumerator ExecuteViewActorCards()
    {
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();
        DisplayAllActorsHands();
    }

    /// <summary>
    /// 前进到下一个地下城关卡
    /// 调用服务器 advance_next_dungeon 接口，成功后刷新并显示新的地下城状态
    /// </summary>
    private IEnumerator ExecuteAdvanceNextDungeon()
    {
        bool success = false;
        yield return DungeonGamePlayManager.Instance.AdvanceNextDungeon(
            (result, message, sessionMessages) =>
            {
                success = result;
                if (!result)
                {
                    _mainText.text = message;
                }
            });

        if (!success)
        {
            yield break;
        }

        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer(
            (result, message) =>
            {
                success = result;
                if (!result)
                {
                    _mainText.text = message;
                }
            }
        );

        if (!success)
        {
            yield break;
        }

        // 3. 切换到地下城场景
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_nextScene);
    }

    /// <summary>
    /// 返回主场景
    /// 调用服务器传送回家接口，成功后切换到主场景
    /// </summary>
    private IEnumerator ExecuteBackHome()
    {
        //Debug.Log("ExecuteBackHome");
        bool success = false;
        yield return DungeonGamePlayManager.Instance.TransHome(
            (result, message) =>
            {
                success = result;
                if (!result)
                {
                    _mainText.text = message;
                }
            });

        if (success)
        {
            yield return new WaitForSeconds(0);
            SceneManager.LoadScene(_preScene);
        }
    }

    /// <summary>
    /// 初始化战斗场景中的角色头像显示
    /// 获取当前场景中的所有角色，按阵营(盟友/敌人)分类后，分别在对应容器中创建头像UI
    /// 该方法在战斗场景初始化时调用一次，负责完整的头像系统设置
    /// </summary>
    private void InitializeActorAvatars()
    {
        var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);
        Debug.Assert(stageName != "", "[DungeonCombatScene] Current actor's stage name is empty");

        // 获取该场景中的所有角色名称
        var actorNames = GameContext.Instance.GetActorsInStage(stageName);

        List<EntitySerialization> allyEntities = new();
        List<EntitySerialization> enemyEntities = new();
        foreach (var actorName in actorNames)
        {
            var actorEntity = GameContext.Instance.GetActorEntitySerialization(actorName);
            if (actorEntity == null)
            {
                Debug.LogWarning($"Actor entity not found for: {actorName}");
                continue;
            }

            if (GameUtils.GetComponent<AllyComponent>(actorEntity) != null)
            {
                allyEntities.Add(actorEntity);
            }
            else if (GameUtils.GetComponent<EnemyComponent>(actorEntity) != null)
            {
                enemyEntities.Add(actorEntity);
            }
            else
            {
                Debug.LogWarning($"Actor {actorName} has neither AllyComponent nor EnemyComponent");
            }
        }

        // 创建并显示盟友头像
        PopulateContainerWithAvatars(_allyAvatarContainer, allyEntities);

        // 创建并显示敌人头像
        PopulateContainerWithAvatars(_enemyAvatarContainer, enemyEntities);

        // 刷新所有头像的死亡状态显示
        RefreshActorAvatars();
    }

    /// <summary>
    /// 在指定容器中填充角色头像
    /// 遍历角色实体列表，为每个角色实例化头像预制体并绑定数据
    /// 这是底层UI创建方法，由 InitializeActorAvatars 调用
    /// </summary>
    /// <param name="container">目标头像容器(盟友容器或敌人容器)</param>
    /// <param name="actorEntities">要创建头像的角色实体列表</param>
    private void PopulateContainerWithAvatars(GameObject container, List<EntitySerialization> actorEntities)
    {
        //清除container所有的孩子
        foreach (Transform child in container.transform)
        {
            DestroyImmediate(child.gameObject);
        }

        // 创建每个符合阵营条件的角色的头像
        foreach (var actorEntity in actorEntities)
        {
            // 创建头像
            GameObject actorAvatarInstance = Instantiate(_actorAvatarPrefab, container.transform);
            actorAvatarInstance.name = actorEntity.name;
        }
    }

    /// <summary>
    /// 更新所有已显示的角色头像的死亡状态
    /// 在战斗过程中调用，用于刷新角色的死亡状态显示
    /// </summary>
    private void RefreshActorAvatars()
    {
        // 获取当前场景的名字，给到
        _onActorAvatarsRefreshEvent.Raise(SceneManager.GetActiveScene().name);
    }
}

