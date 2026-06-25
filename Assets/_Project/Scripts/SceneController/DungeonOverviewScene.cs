// using UnityEngine;
// using UnityEngine.SceneManagement;
// using Cysharp.Threading.Tasks;
// using System.Collections.Generic;


// /// <summary>
// /// 地下城概览场景控制器
// /// 负责显示地下城的宏观信息，包括关卡列表和怪物预览
// /// 提供进入地下城和返回主场景的功能
// /// </summary>
// public class DungeonOverviewScene : MonoBehaviour, IUIEventListener
// {
//     public static readonly string PreSceneName = "MainScene";
//     public static readonly string NextSceneName = "DungeonCombatScene";
//     public static List<Dungeon> CachedDungeonOverviews = new();

//     [Header("UI Components")]
//     [SerializeField] private DungeonOverviewListPanel _dungeonOverviewListPanel;
//     [SerializeField] private DungeonOverviewDetailPanel _dungeonOverviewDetailPanel;

//     [Header("API Components")]
//     [SerializeField] private TasksStatusApi _tasksStatusApi; // 轮询任务状态的 API 组件

//     [Header("Events")]
//     [SerializeField] private UIEventGameEvent _onDungeonOverviewItemClickedEvent; // 地下城概览列表项被点击事件, 这个事件自己不可以再听了，是发送端，不能再监听了，否则会死循环。

//     /// 当前选中的地下城名称
//     private string _selectedDungeonName = string.Empty;


//     /// <summary>
//     /// 场景初始化
//     /// 验证必要组件并加载地下城概览数据
//     /// </summary>
//     void Start()
//     {
//         Debug.Assert(_dungeonOverviewListPanel != null, "_dungeonOverviewListPanel is null");
//         Debug.Assert(_dungeonOverviewDetailPanel != null, "_dungeonOverviewDetailPanel is null");
//         Debug.Assert(_tasksStatusApi != null, "_tasksStatusApi is null");
//         Debug.Assert(_onDungeonOverviewItemClickedEvent != null, "_onDungeonOverviewItemClickedEvent is null");

//         // 注册事件监听器
//         _onDungeonOverviewItemClickedEvent.RegisterListener(this);

//         // 更新显示
//         _dungeonOverviewListPanel.gameObject.SetActive(true);
//         _dungeonOverviewDetailPanel.gameObject.SetActive(false);

//         // 列出来地下城概览数据，实际项目中应该从服务器获取
//         RefreshDungeonList().Forget();
//     }

//     /// <summary>
//     /// 场景销毁时的清理工作
//     /// </summary>
//     void OnDestroy()
//     {
//         if (_onDungeonOverviewItemClickedEvent != null)
//         {
//             _onDungeonOverviewItemClickedEvent.UnregisterListener(this);
//         }
//     }

//     /// <summary>
//     /// UI按钮回调：返回主场景
//     /// 触发返回流程
//     /// </summary>
//     public void OnClickBack()
//     {
//         Debug.Log("Back button clicked");
//         NavigateToMainScene().Forget();
//     }


//     /// <summary>
//     /// UI按钮回调：新建地下城
//     /// </summary>
//     public void OnClickGenerateDungeon()
//     {
//         Debug.Log("OnClickGenerateDungeon");
//         // 这里可以添加新建地下城的逻辑，例如弹出新建地下城的UI，或者直接调用API创建新的地下城
//         GenerateDungeon().Forget();
//     }

//     /// <summary>
//     /// UI按钮回调：进入地下城
//     /// </summary>
//     public void OnClickEnterDungeon()
//     {
//         Debug.Log("Enter Dungeon button clicked");
//         TransitionToDungeonCombat().Forget();
//     }

//     /// <summary>
//     /// IUIEventListener 接口实现
//     /// 处理所有UI事件的统一入口
//     /// </summary>
//     public void OnEventRaised(UIEventData eventData)
//     {
//         Debug.Log($"[TestDungeonCombatScenePrototype] {eventData}");
//         Debug.Log($"OnEventRaised: {eventData.eventType}, TargetId: {eventData.targetId}, Index: {eventData.index}, ExtraData: {eventData.extraData}");

//         switch (eventData.eventType)
//         {

//             case UIEventType.DungeonOverviewItemClicked:
//                 {
//                     var clickedDungeonName = eventData.targetId;
//                     Debug.Log($"Dungeon overview item clicked: {clickedDungeonName}, Index: {eventData.index}");

//                     if (!string.IsNullOrEmpty(clickedDungeonName))
//                     {
//                         _selectedDungeonName = clickedDungeonName;
//                         // 显示地下城详情面板
//                         _dungeonOverviewDetailPanel.gameObject.SetActive(true);
//                         // 这里可以根据 clickedDungeonName 来刷新详情面板的显示内容
//                         _dungeonOverviewDetailPanel.OnRefreshView(clickedDungeonName).Forget();
//                     }
//                     else
//                     {
//                         Debug.LogWarning("Clicked dungeon name is null or empty");
//                     }
//                 }
//                 break;

//             default:
//                 Debug.LogWarning($"Unknown event type: {eventData.eventType}, no handler implemented");
//                 break;
//         }
//     }

//     /// <summary>
//     /// 切换到地下城战斗场景
//     /// 调用传送API，验证响应后加载 DungeonCombatScene
//     /// </summary>
//     private async UniTaskVoid TransitionToDungeonCombat()
//     {
//         if (!GameContext.Instance.IsLoggedIn)
//         {
//             Debug.LogWarning("Player is not logged in, cannot load dungeon overview");
//             return;
//         }

//         var homeEnterDungeonResponse = await HomeGamePlayManager.Instance.HomeEnterDungeon(_selectedDungeonName);
//         if (homeEnterDungeonResponse == null)
//         {
//             Debug.LogError("Failed to transition into dungeon");
//             return;
//         }

//         var (dungeon, stagesState) = await UniTask.WhenAll(
//             GameStateSync.Instance.GetDungeon(),
//             GameStateSync.Instance.GetStagesState()
//         );

//         if (dungeon == null)
//         {
//             Debug.LogError("Failed to refresh dungeon data");
//             return;
//         }

//         if (stagesState == null)
//         {
//             Debug.LogError("[HomeScene] Failed to get stages state from server");
//             return;
//         }

//         var targetStageName = string.Empty;
//         foreach (var kvp in stagesState)
//         {
//             if (kvp.Value.Contains(GameContext.Instance.PlayerActorName))
//             {
//                 targetStageName = kvp.Key;
//                 break;
//             }
//         }

//         await UniTask.Yield();

//         // 进入地下城后默认进入第一个关卡
//         //DungeonCombatScene.CachedStageName = targetStageName;
//         DungeonCombatScene.CachedDungeon = dungeon;

//         // 切换到地下城战斗场景
//         SceneManager.LoadScene(NextSceneName);
//     }

//     /// <summary>
//     /// 从服务器拉取地下城列表并刷新概览 UI
//     /// </summary>
//     private async UniTaskVoid RefreshDungeonList()
//     {
//         if (!GameContext.Instance.IsLoggedIn)
//         {
//             Debug.LogWarning("Player is not logged in, cannot load dungeon overview");

//             CachedDungeonOverviews.Clear();
//             for (int i = 1; i <= 10; i++)
//             {
//                 CachedDungeonOverviews.Add(new Dungeon
//                 {
//                     name = $"Dungeon {i}"
//                 });
//             }

//             _dungeonOverviewListPanel.RefreshScrollView();
//             return;
//         }

//         var dungeons = await HomeGamePlayManager.Instance.ListDungeons();
//         if (dungeons == null)
//         {
//             Debug.LogError("Failed to retrieve dungeon list");
//             CachedDungeonOverviews.Clear();
//             _dungeonOverviewListPanel.RefreshScrollView();
//             return;
//         }

//         CachedDungeonOverviews = dungeons;
//         _dungeonOverviewListPanel.RefreshScrollView();
//     }

//     /// <summary>
//     /// 返回主场景
//     /// </summary>
//     private async UniTaskVoid NavigateToMainScene()
//     {
//         if (GameContext.Instance.IsLoggedIn)
//         {
//             Debug.Log("Returning to MainScene");
//             await UniTask.Yield();
//             SceneManager.LoadScene(PreSceneName);
//         }
//         else
//         {
//             Debug.LogWarning("Game is not set up. Staying in CampScene.");
//         }
//     }


//     private async UniTaskVoid GenerateDungeon()
//     {
//         if (!GameContext.Instance.IsLoggedIn)
//         {
//             Debug.LogWarning("Player is not logged in, cannot create new dungeon");
//             return;
//         }

//         // 这里可以添加新建地下城的逻辑，例如弹出新建地下城的UI，或者直接调用API创建新的地下城
//         Debug.Log("NewDungeon logic is not implemented yet");

//         var generateDungeonResponse = await HomeGamePlayManager.Instance.GenerateDungeon();
//         if (generateDungeonResponse == null || string.IsNullOrEmpty(generateDungeonResponse.task_id))
//         {
//             Debug.LogError("Retreat failed: no response data");
//             return;
//         }

//         Debug.Log($"Retreat initiated successfully, task ID: {generateDungeonResponse.task_id}");
//         var taskRecord = await PollTaskStatus(generateDungeonResponse.task_id);
//         if (taskRecord == null)
//         {
//             Debug.LogError($"Failed to get task record for task ID: {generateDungeonResponse.task_id}");
//             return;
//         }

//         Debug.Log($"Retreat successful: {generateDungeonResponse.message}");

//         var listDungeons = await HomeGamePlayManager.Instance.ListDungeons();
//         if (listDungeons == null)
//         {
//             Debug.LogError("Failed to retrieve dungeon list");
//             return;
//         }

//         Debug.Log($"Successfully retrieved dungeon list with {listDungeons.Count} dungeons");
//         RefreshDungeonList().Forget();
//     }

//     /// <summary>
//     /// 轮询任务状态
//     /// </summary>
//     /// <param name="taskId">任务ID</param>
//     /// <returns>任务记录</returns>
//     private async UniTask<TaskRecord> PollTaskStatus(string taskId)
//     {
//         return await _tasksStatusApi.PollTaskStatus(
//             GameContext.Instance.TasksStatusUrl,
//             taskId);
//     }
// }
