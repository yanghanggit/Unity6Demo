using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class DungeonCombatScene2 : MonoBehaviour, IUIEventListener, ICombatScene
{
    public static readonly string PreSceneName = "MainScene";
    public static readonly string NextSceneName = "DungeonCombatScene2";

    public static string StageName = string.Empty;
    public static string DungeonName = string.Empty;

    [Header("UI Components")]

    [Header("Top Bar")]
    [SerializeField] private CombatTopBar _topBar; // 顶部UI控制器

    [Header("Initialization State")]
    [SerializeField] private GameObject _initializationState; // INITIALIZATION 状态的设置数据

    [Header("OnGoing State")]
    [SerializeField] private CombatOnGoingState _onGoingState; // ONGOING 状态的设置数据

    [Header("PostCombat State")]
    [SerializeField] private CombatPostCombatState _postCombatState; // 战斗后状态的设置数据

    [Header("Setting Panel")]
    [SerializeField] private GameObject _settingPanel; // 设置面板对象

    [Header("API Components")]
    [SerializeField] private TasksStatusApi _tasksStatusApi; // 轮询任务状态的 API 组件


    void Start()
    {
        // 断言检查，确保所有必要的组件和数据都已正确设置
        Debug.Assert(SpriteCacheManager.Instance != null, "SpriteCacheManager instance is null");
        Debug.Assert(_tasksStatusApi != null, "TasksStatusApi component is not assigned in the inspector.");
        Debug.Assert(_topBar != null, "_topBar is null");
        Debug.Assert(_initializationState != null, "_initializationState is null");
        Debug.Assert(_onGoingState != null, "_onGoingState is null");
        Debug.Assert(_postCombatState != null, "_postCombatState is null");
        Debug.Assert(_settingPanel != null, "_settingPanel is null");

        // 场景初始状态设置为隐藏所有状态对象和设置面板，确保场景初始状态是干净的
        _topBar.gameObject.SetActive(true);

        // 初始状态先隐藏主对象和底部对象，确保场景初始状态是隐藏的
        _initializationState.SetActive(true);

        // 先隐藏掉 _onGoingState
        _onGoingState.gameObject.SetActive(false);
        _onGoingState.CombatScene = this; // 将当前场景作为属性传递给状态对象

        // 初始状态先隐藏仲裁面板和战斗后面板
        _postCombatState.gameObject.SetActive(false);
        _postCombatState.CombatScene = this; // 将当前场景作为属性传递给状态对象

        //
        _settingPanel.SetActive(false);

        /// 场景异步初始化入口，根据当前战斗状态执行对应的初始化逻辑
        InitCombatSceneAsync().Forget();
    }

    /// <summary>
    /// 处理抽卡行动：根据当前 CardBuilder.Build 的数据决定执行敌人抽卡或盟友抽卡
    /// </summary>
    private void HandleDrawCardAction()
    {
        // 如果未登录，就不要处理这段提交代码。
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.Log("Simulating successful escape for non-logged-in user");
            return;
        }

        if (CardBuilder.Build.owner == null)
        {
            Debug.LogWarning("No actor selected, cannot execute escape action");
            return;
        }

        var enemyComponent = GameUtils.GetComponent<EnemyComponent>(CardBuilder.Build.owner);
        if (enemyComponent != null)
        {
            // 敌人直接执行抽卡行动，传入空的行动列表和启用敌人抽卡的标志
            ExecuteDrawCards(new List<AllyDrawCardAction>(), true).Forget();
            return;
        }

        // 目标角色、技能和状态效果都是必选的，缺一不可，否则无法执行抽卡行动
        if (CardBuilder.Build.targetActors == null || CardBuilder.Build.targetActors.Count == 0)
        {
            Debug.LogWarning("No target actors selected, cannot execute escape action");
            return;
        }

        if (CardBuilder.Build.skill == null || CardBuilder.Build.skill.name == "")
        {
            Debug.LogWarning("No skill selected, cannot execute escape action");
            return;
        }

        if (CardBuilder.Build.statusEffects == null || CardBuilder.Build.statusEffects.Count == 0)
        {
            Debug.LogWarning("No status effects selected, cannot execute escape action");
            return;
        }

        // 创建抽卡行动
        var allyDrawAction = new AllyDrawCardAction
        {
            entity_name = CardBuilder.Build.owner.name,
            skill_name = CardBuilder.Build.skill.name,
            target_names = CardBuilder.Build.targetActors != null ? CardBuilder.Build.targetActors.ConvertAll(actor => actor.name) : new List<string>(),
            status_effect_names = CardBuilder.Build.statusEffects != null ? CardBuilder.Build.statusEffects.ConvertAll(effect => effect.name) : new List<string>()
        };

        // 调用抽卡接口，传入构建的行动数据
        ExecuteDrawCards(new List<AllyDrawCardAction> { allyDrawAction }, false).Forget();
    }

    /// <summary>
    /// 处理出牌行动：所有存活演员均已有手牌后执行
    /// TODO: 实现出牌逻辑
    /// </summary>
    private void HandlePlayCardAction()
    {
        Debug.Log("[DungeonCombatScene] HandlePlayCardAction: 所有演员已有手牌，出牌逻辑待实现");
        // 我已经添加了 ExecutePlayCards 函数，请你在此调用点进行使用。

        ExecutePlayCards().Forget();
    }

    /// <summary>
    /// 刷新当前场景中所有演员数据，并检查是否所有存活演员都已完成抽卡
    /// 存活演员定义：没有 DeathComponent 组件的演员
    /// 抽卡完成定义：存活演员的 HandComponent.cards 不为空
    /// </summary>
    /// <returns>是否所有存活演员都已完成抽卡</returns>
    // private async UniTask<bool> RefreshAndCheckAllActorsDrawn()
    // {
    //     // 刷新玩家所在场景中所有演员的最新数据
    //     var actorEntities = await GameStateSync.Instance.RefreshActorsInStageFromServer(GameContext.Instance.PlayerActorName);
    //     if (actorEntities == null)
    //     {
    //         Debug.LogWarning("[DungeonCombatScene] RefreshAndCheckAllActorsDrawn: failed to refresh actors in stage");
    //         return false;
    //     }

    //     // 数据已是最新，获取当前回合的角色列表
    //     var actors = GetCurrentRoundActors();
    //     if (actors == null || actors.Count == 0)
    //     {
    //         Debug.LogWarning("[DungeonCombatScene] RefreshAndCheckAllActorsDrawn: no actors found in current round");
    //         return false;
    //     }

    //     // 检查所有存活演员（没有 DeathComponent）是否都已有手牌数据
    //     bool allDrawn = true;
    //     foreach (var actor in actors)
    //     {
    //         var deathComponent = GameUtils.GetComponent<DeathComponent>(actor);
    //         if (deathComponent != null)
    //         {
    //             // 已死亡，跳过检查
    //             continue;
    //         }

    //         var handComponent = GameUtils.GetComponent<HandComponent>(actor);
    //         if (handComponent == null || handComponent.cards == null || handComponent.cards.Count == 0)
    //         {
    //             allDrawn = false;
    //             break;
    //         }
    //     }

    //     if (allDrawn)
    //     {
    //         Debug.Log("[DungeonCombatScene] 所有存活演员均已完成抽卡");
    //     }
    //     else
    //     {
    //         Debug.Log("[DungeonCombatScene] 尚有存活演员未完成抽卡");
    //     }

    //     return allDrawn;
    // }

    /// <summary>
    /// IUIEventListener 接口实现
    /// 处理所有UI事件的统一入口
    /// </summary>
    public void OnEventRaised(UIEventData eventData)
    {

    }

    /// <summary>
    /// 场景异步初始化入口，在 Start 时调用。
    /// 已登录时从服务器刷新地下城数据，获取当前战斗状态（CombatState），
    /// 并根据状态分支（INITIALIZATION / ONGOING / COMPLETE / POST_COMBAT）执行对应的初始化逻辑。
    /// 未登录时跳过服务器请求，直接以模拟状态继续。
    /// </summary>
    private async UniTaskVoid InitCombatSceneAsync()
    {
        // 获取当前战斗状态，如果当前战斗对象不存在则默认设置为 NONE，并在日志中输出警告信息
        CombatState lastCombatState = CombatState.NONE;

        if (!GameContext.Instance.IsLoggedIn)
        {
            // 模拟未登录用户的战斗状态，这里直接设置为 ONGOING，后续可以根据需要调整为其他状态
            await UniTask.Delay(500);

            // 假设未登录用户没有战斗数据，直接设置为 NONE 状态，并在日志中输出相关信息
            lastCombatState = CombatState.ONGOING;
        }
        else
        {
            var combatState = await GameStateSync.Instance.GetCombat();
            if (combatState == null)
            {
                Debug.LogWarning("[DungeonCombatScene] No combat data found for current dungeon, defaulting to NONE state");
                return;
            }

            lastCombatState = combatState.state;
            Debug.Log($"[DungeonCombatScene] Last combat state: {lastCombatState}");
        }

        //
        switch (lastCombatState)
        {
            case CombatState.INITIALIZATION:
                {
                    Debug.Log("[DungeonCombatScene] Combat is in initialization state, showing initialization UI");
                    var messages = await DungeonGamePlayManager.Instance.CombatInit();
                    if (messages == null)
                    {
                        Debug.LogError("CombatInit failed, messages is null");
                        return;
                    }


                    Debug.Log("[DungeonCombatScene] Combat initialization completed, switching to ongoing state");
                    OnEnterOnGoingState();
                }
                break;

            case CombatState.ONGOING:
                Debug.Log("[DungeonCombatScene] Combat is ongoing, showing ongoing UI");
                OnEnterOnGoingState();
                break;

            case CombatState.COMPLETE:
                Debug.Log("[DungeonCombatScene] Combat is complete, showing post-combat UI");
                break;

            case CombatState.POST_COMBAT:
                Debug.Log("[DungeonCombatScene] Combat is in post-combat state, showing post-combat UI");
                break;

            default:
                Debug.LogWarning($"Unknown combat state: {lastCombatState}, skipping combat initialization");
                break;
        }
    }

    /// <summary>
    /// 执行抽卡操作并轮询任务状态，完成后显示手牌
    /// 调用服务器 draw_cards 接口，获取任务ID后轮询查询任务状态
    /// 当任务完成时，刷新数据并显示角色手牌信息
    /// </summary>
    private async UniTaskVoid ExecuteDrawCards(List<AllyDrawCardAction> specifiedActions, bool enableEnemyDraw)
    {
        string taskId = await DungeonGamePlayManager.Instance.DrawCards(specifiedActions, enableEnemyDraw);
        if (string.IsNullOrEmpty(taskId))
        {
            Debug.LogError("DrawCards API call failed, no task ID returned");
            return;
        }

        Debug.Log($"DrawCards initiated successfully, task ID: {taskId}");
        var taskRecord = await PollTaskStatus(taskId);
        if (taskRecord == null)
        {
            Debug.LogError($"Failed to get task record for task ID: {taskId}");
            return;
        }
    }

    /// <summary>
    /// 轮询查询任务状态直到完成或失败
    /// 委托 TasksStatusApi 执行轮询逻辑，完成后通过回调函数返回结果
    /// </summary>
    /// <param name="taskId">要查询的任务ID</param>
    /// <param name="onComplete">轮询完成后的回调函数，参数为(成功标志, 消息, 任务记录)</param>
    private async UniTask<TaskRecord> PollTaskStatus(string taskId)
    {
        return await _tasksStatusApi.PollTaskStatus(
            GameContext.Instance.TasksStatusUrl,
            taskId);
    }

    /// <summary>
    /// 执行出牌操作并轮询任务状态，完成后刷新数据并评估战斗状态
    /// 调用服务器 play_cards 接口，获取任务ID后轮询查询任务状态
    /// 当任务完成时，刷新数据并调用 CombatStatusEvaluation 接口评估当前战斗状态
    /// </summary>
    private async UniTaskVoid ExecutePlayCards()
    {

        string taskId = await DungeonGamePlayManager.Instance.PlayCards();
        if (string.IsNullOrEmpty(taskId))
        {
            return;
        }

        Debug.Log($"PlayCards initiated successfully, task ID: {taskId}");

        //ShowArbitrationPanel("出牌操作已提交，任务id: " + taskId + "，正在等待服务器处理...");
        var taskRecord = await PollTaskStatus(taskId);
        if (taskRecord == null)
        {
            return;
        }

        // var dungeon = await GameStateSync.Instance.GetDungeon();
        // if (dungeon == null)
        // {
        //     Debug.LogError("Failed to refresh dungeon data");
        //     return;
        // }

        // Debug.Log("PlayCards action completed and combat status evaluated");

        // // 获取最新的地下城回合信息
        // Round round = GameUtils.GetLastRound(GameContext.Instance.Dungeon);
        // Debug.Assert(round != null, "Round data is null after playing cards");

        // 异步跑着，评估战斗状态，完成后会刷新地下城状态并更新UI显示
        DungeonGamePlayManager.Instance.CombatStatusEvaluation().Forget();
    }

    /// <summary>
    /// 战斗后处理
    /// 调用服务器 post_combat 接口进行战斗后处理，成功后刷新地下城状态显示
    /// </summary>
    private async UniTaskVoid ExecutePostCombat()
    {

        var sessionMessages = await DungeonGamePlayManager.Instance.PostCombat();
        if (sessionMessages == null)
        {
            Debug.LogWarning("Failed to get session messages from post combat");
            //ShowPostCombatPanel("战斗后处理失败，无法获取事件信息");
            return;
        }

        // 然后逐个处理返回的 SessionMessage，特别是 CombatArchiveEvent
        var showText = "战斗后事件：\n\n";

        for (int i = 0; i < sessionMessages.Count; i++)
        {
            SessionMessage sessionMessage = sessionMessages[i];
            if (sessionMessage.message_type != (int)MessageType.AGENT_EVENT)
            {
                continue;
            }

            var agentEvent = GameUtils.ParseAgentEvent(sessionMessage);
            if (agentEvent == null)
            {
                Debug.LogWarning("Failed to parse agent event from session message");
                continue;
            }

            if (agentEvent.head == (int)EventHead.COMBAT_ARCHIVE_EVENT)
            {
                Debug.Log("Processing CombatArchiveEvent from post combat");
                if (agentEvent is CombatArchiveEvent combatArchiveEvent)
                {
                    showText += $"Actor: {combatArchiveEvent.actor}\nSummary: {combatArchiveEvent.summary}\n\n";
                }
            }
        }
    }

    /// <summary>
    /// 进入下一关处理
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid ExecuteAdvanceNext()
    {
        var messages = await DungeonGamePlayManager.Instance.AdvanceNextDungeon();
        if (messages == null)
        {
            Debug.LogWarning("Failed to advance to next dungeon, no messages returned");
            return;
        }

        // var syncErr = await GameStateSync.Instance.RefreshCombatStateFromServer();
        // if (syncErr != GameSyncError.None)
        // {
        //     Debug.LogError($"[DungeonCombatScene] Failed to refresh dungeon and actors data: {syncErr}");
        //     return;
        // }

        await UniTask.Yield();
        SceneManager.LoadScene(NextSceneName);
    }

    /// <summary>
    /// 显示仲裁面板并设置文本内容
    /// </summary>
    public void OnEnterOnGoingState()
    {
        // 切换到 OnGoing 状态，显示主对象并刷新显示内容
        _initializationState.SetActive(false);
        _postCombatState.gameObject.SetActive(false);

        // 切换到 OnGoing 状态，显示主对象并刷新显示内容
        _onGoingState.gameObject.SetActive(true);
        _onGoingState.OnEnter();
    }


    /// <summary>
    /// 进入战斗后状态，显示战斗后面板并刷新显示内容
    /// </summary>
    public void OnEnterPostCombatState()
    {
        // 先隐藏掉其他状态对象，确保只有战斗后状态对象是显示的
        _initializationState.SetActive(false);
        _onGoingState.gameObject.SetActive(false);

        // 切换到战斗后状态，显示主对象并刷新显示内容
        _postCombatState.gameObject.SetActive(true);
        _postCombatState.OnEnter();
    }

    /// <summary>
    /// 点击 Setting 按钮
    /// </summary>
    public void OnClickOpenSetting()
    {
        Debug.Log("[DungeonCombatTopBar] Setting button clicked");
        _settingPanel.SetActive(true);
    }

    /// <summary>
    /// 点击 Close Setting 按钮
    /// </summary>
    public void OnClickCloseSetting()
    {
        Debug.Log("[DungeonCombatScene] Close Setting button clicked");
        _settingPanel.SetActive(false);
    }
}

