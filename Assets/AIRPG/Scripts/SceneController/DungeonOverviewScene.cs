using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;


[System.Serializable]
public class DungeonOveriewData
{
    public string dungeonName;
}

/// <summary>
/// 地下城概览场景控制器
/// 负责显示地下城的宏观信息，包括关卡列表和怪物预览
/// 提供进入地下城和返回主场景的功能
/// </summary>
public class DungeonOverviewScene : MonoBehaviour, IUIEventListener
{
    public static readonly string PreSceneName = "MainScene";
    public static readonly string NextSceneName = "DungeonCombatScene";
    public static readonly List<DungeonOveriewData> DungeonOverviews = new();

    [Header("UI Components")]
    [SerializeField] private DungeonOverviewListPanel _dungeonOverviewListPanel;
    [SerializeField] private DungeonOverviewDetailPanel _dungeonOverviewDetailPanel;

    [Header("API Components")]
    [SerializeField] private TasksStatusApi _tasksStatusApi; // 轮询任务状态的 API 组件

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onDungeonOverviewItemClickedEvent; // 地下城概览列表项被点击事件, 这个事件自己不可以再听了，是发送端，不能再监听了，否则会死循环。


    /// <summary>
    /// 场景初始化
    /// 验证必要组件并加载地下城概览数据
    /// </summary>
    void Start()
    {
        Debug.Assert(_dungeonOverviewListPanel != null, "_dungeonOverviewListPanel is null");
        Debug.Assert(_dungeonOverviewDetailPanel != null, "_dungeonOverviewDetailPanel is null");
        Debug.Assert(_tasksStatusApi != null, "_tasksStatusApi is null");
        Debug.Assert(_onDungeonOverviewItemClickedEvent != null, "_onDungeonOverviewItemClickedEvent is null");

        // 注册事件监听器
        _onDungeonOverviewItemClickedEvent.RegisterListener(this);

        // 更新显示
        _dungeonOverviewDetailPanel.gameObject.SetActive(false);

        // 列出来地下城概览数据，实际项目中应该从服务器获取
        ListDungeons().Forget();
    }

    /// <summary>
    /// 场景销毁时的清理工作
    /// </summary>
    void OnDestroy()
    {
        if (_onDungeonOverviewItemClickedEvent != null)
        {
            _onDungeonOverviewItemClickedEvent.UnregisterListener(this);
        }
    }

    /// <summary>
    /// UI按钮回调：返回主场景
    /// 触发返回流程
    /// </summary>
    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        ReturnToMainScene().Forget();
    }


    /// <summary>
    /// UI按钮回调：新建地下城
    /// </summary>
    public void OnClickGenerateDungeon()
    {
        Debug.Log("OnClickGenerateDungeon");
        // 这里可以添加新建地下城的逻辑，例如弹出新建地下城的UI，或者直接调用API创建新的地下城
        ExecuteGenerateDungeon().Forget();
    }

    /// <summary>
    /// UI按钮回调：进入地下城
    /// </summary>
    public void OnClickEnterDungeon()
    {
        Debug.Log("Enter Dungeon button clicked");
        //EnterDungeon().Forget();
        _dungeonOverviewDetailPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// IUIEventListener 接口实现
    /// 处理所有UI事件的统一入口
    /// </summary>
    public void OnEventRaised(UIEventData eventData)
    {
        Debug.Log($"[TestDungeonCombatScenePrototype] {eventData}");
        Debug.Log($"OnEventRaised: {eventData.eventType}, TargetId: {eventData.targetId}, Index: {eventData.index}, ExtraData: {eventData.extraData}");

        switch (eventData.eventType)
        {

            case UIEventType.DungeonOverviewItemClicked:
                {
                    var clickedDungeonName = eventData.targetId;
                    Debug.Log($"Dungeon overview item clicked: {clickedDungeonName}, Index: {eventData.index}");

                    if (!string.IsNullOrEmpty(clickedDungeonName))
                    {
                        // 显示地下城详情面板
                        _dungeonOverviewDetailPanel.gameObject.SetActive(true);
                        // 这里可以根据 clickedDungeonName 来刷新详情面板的显示内容
                        _dungeonOverviewDetailPanel.OnRefreshView(clickedDungeonName);
                    }
                    else
                    {
                        Debug.LogWarning("Clicked dungeon name is null or empty");
                    }
                }
                break;

            default:
                Debug.LogWarning($"Unknown event type: {eventData.eventType}, no handler implemented");
                break;
        }
    }

    /// <summary>
    /// 执行进入地下城的流程
    /// 调用传送API，验证响应后切换场景
    /// </summary>
    private async UniTaskVoid EnterDungeon()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, cannot load dungeon overview");
            //_mainText.text = "Player is not logged in";
            return;
        }

        var dungeon = await GameStateSync.Instance.GetDungeon();
        if (dungeon == null)
        {
            Debug.LogError("Failed to refresh dungeon data");
            //_mainText.text = "Failed to load dungeon data";
            return;
        }

        var homeEnterDungeonResponse = await HomeGamePlayManager.Instance.HomeEnterDungeon();
        if (homeEnterDungeonResponse == null)
        {
            Debug.LogError("Failed to transition into dungeon");
            //_mainText.text = "Failed to enter dungeon";
            return;
        }

        var stagesState = await GameStateSync.Instance.GetStagesState();
        if (stagesState == null)
        {
            Debug.LogError("[HomeScene] Failed to get stages state from server");
            //_mainText.text = "Failed to get stage information";
            return;
        }

        var targetStageName = string.Empty;
        foreach (var kvp in stagesState)
        {
            if (kvp.Value.Contains(GameContext.Instance.PlayerActorName))
            {
                targetStageName = kvp.Key;
                break;
            }
        }

        await UniTask.Yield();

        // 进入地下城后默认进入第一个关卡
        DungeonCombatScene.CachedStageName = targetStageName;
        DungeonCombatScene.CachedDungeonName = dungeon.name;

        // 切换到地下城战斗场景
        SceneManager.LoadScene(NextSceneName);
    }

    /// <summary>
    /// 加载并显示地下城概览信息
    /// 从服务器刷新地下城数据，并格式化显示在UI上
    /// </summary>
    private async UniTaskVoid ListDungeons()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, cannot load dungeon overview");

            for (int i = 1; i <= 10; i++)
            {
                DungeonOverviews.Add(new DungeonOveriewData
                {
                    dungeonName = $"Dungeon {i}"
                });
            }

            _dungeonOverviewListPanel.RefreshScrollView();
            return;
        }

        var dungeons = await HomeGamePlayManager.Instance.ListDungeons();
        if (dungeons == null)
        {
            Debug.LogError("Failed to retrieve dungeon list");
            DungeonOverviews.Clear();
            _dungeonOverviewListPanel.RefreshScrollView();
            return;
        }

        DungeonOverviews.Clear();
        foreach (var dungeon in dungeons)
        {
            DungeonOverviews.Add(new DungeonOveriewData
            {
                dungeonName = dungeon.name
            });
        }

        _dungeonOverviewListPanel.RefreshScrollView();
    }

    /// <summary>
    /// 执行返回主场景的流程
    /// 验证游戏状态后切换到主场景
    /// </summary>
    private async UniTaskVoid ReturnToMainScene()
    {
        if (GameContext.Instance.IsLoggedIn)
        {
            Debug.Log("Returning to MainScene");
            await UniTask.Yield();
            SceneManager.LoadScene(PreSceneName);
        }
        else
        {
            Debug.LogWarning("Game is not set up. Staying in CampScene.");
        }
    }


    private async UniTaskVoid ExecuteGenerateDungeon()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, cannot create new dungeon");
            return;
        }

        // 这里可以添加新建地下城的逻辑，例如弹出新建地下城的UI，或者直接调用API创建新的地下城
        Debug.Log("NewDungeon logic is not implemented yet");

        var generateDungeonResponse = await HomeGamePlayManager.Instance.GenerateDungeon();
        if (generateDungeonResponse == null || string.IsNullOrEmpty(generateDungeonResponse.task_id))
        {
            Debug.LogError("Retreat failed: no response data");
            return;
        }

        Debug.Log($"Retreat initiated successfully, task ID: {generateDungeonResponse.task_id}");
        var taskRecord = await PollTaskStatus(generateDungeonResponse.task_id);
        if (taskRecord == null)
        {
            Debug.LogError($"Failed to get task record for task ID: {generateDungeonResponse.task_id}");
            return;
        }

        Debug.Log($"Retreat successful: {generateDungeonResponse.message}");

        var listDungeons = await HomeGamePlayManager.Instance.ListDungeons();
        if (listDungeons == null)
        {
            Debug.LogError("Failed to retrieve dungeon list");
            return;
        }

        Debug.Log($"Successfully retrieved dungeon list with {listDungeons.Count} dungeons");
        ListDungeons().Forget();
    }

    /// <summary>
    /// 轮询任务状态
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>任务记录</returns>
    private async UniTask<TaskRecord> PollTaskStatus(string taskId)
    {
        return await _tasksStatusApi.PollTaskStatus(
            GameContext.Instance.TasksStatusUrl,
            taskId);
    }
}
