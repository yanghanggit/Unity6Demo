using UnityEngine;
using Cysharp.Threading.Tasks;

public class DungeonCombatScene : MonoBehaviour, IUIEventListener, ICombatScene
{
    public static readonly string PreSceneName = "MainScene";
    public static readonly string NextSceneName = "DungeonCombatScene";

    public static string CachedStageName = string.Empty;
    public static string CachedDungeonName = string.Empty;

    [Header("UI Components")]

    [Header("Top Bar")]
    [SerializeField] private CombatTopBar _topBar; // 顶部UI控制器

    [Header("Initialization State")]
    [SerializeField] private GameObject _initializationState; // INITIALIZATION 状态的设置数据

    [Header("OnGoing State")]
    [SerializeField] private CombatOnGoingStatePanel _onGoingState; // ONGOING 状态的设置数据

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
        Debug.Assert(_topBar != null, "_topBar is null");
        Debug.Assert(_initializationState != null, "_initializationState is null");
        Debug.Assert(_onGoingState != null, "_onGoingState is null");
        Debug.Assert(_postCombatState != null, "_postCombatState is null");
        Debug.Assert(_settingPanel != null, "_settingPanel is null");
        Debug.Assert(_tasksStatusApi != null, "_tasksStatusApi is null");

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
        CombatState lastCombatState;

        if (!GameContext.Instance.IsLoggedIn)
        {
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
                    var response = await DungeonGamePlayManager.Instance.InitCombat();
                    if (response == null)
                    {
                        Debug.LogError("[DungeonCombatScene] Combat initialization failed, response data is null");
                        return;
                    }

                    Debug.Log("[DungeonCombatScene] Combat initialization succeeded, response data received taskid = " + response.task_id);
                    var taskRecord = await PollTaskStatus(response.task_id);
                    if (taskRecord == null)
                    {
                        Debug.LogError("[DungeonCombatScene] Failed to retrieve task status, taskRecord is null");
                        return;
                    }

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

    /// <summary>
    /// 轮询任务状态，直到任务完成并返回结果
    /// </summary>
    private async UniTask<TaskRecord> PollTaskStatus(string taskId)
    {
        return await _tasksStatusApi.PollTaskStatus(
            GameContext.Instance.TasksStatusUrl,
            taskId);
    }
}

