using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CombatSettingPanel : MonoBehaviour
{
    public static readonly string MainSceneName = "MainScene";

    [Header("API Components")]
    [SerializeField] private TasksStatusApi _tasksStatusApi; // 轮询任务状态的 API 组件

    void Start()
    {
        Debug.Assert(_tasksStatusApi != null, "_tasksStatusApi is null");
    }

    /// <summary>
    /// 点击 Setting 按钮
    /// </summary>
    public void OnClickRetreat()
    {
        Debug.Log("[CombatSettingPanel] Retreat button clicked");
        ExecuteRetreatSync().Forget();
    }

    public async UniTaskVoid ExecuteRetreatSync()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            await UniTask.Yield();
            SceneManager.LoadScene(MainSceneName);
            return;
        }

        var response = await DungeonGamePlayManager.Instance.RetreatFromDungeon();
        if (response == null || string.IsNullOrEmpty(response.task_id))
        {
            Debug.LogError("Retreat failed: no response data");
            return;
        }

        Debug.Log($"DrawCards initiated successfully, task ID: {response.task_id}");
        var taskRecord = await PollTaskStatus(response.task_id);
        if (taskRecord == null)
        {
            Debug.LogError($"Failed to get task record for task ID: {response.task_id}");
            return;
        }

        Debug.Log($"Retreat successful: {response.message}");

        await UniTask.Yield();
        SceneManager.LoadScene(MainSceneName);
    }

    /// <summary>
    /// 轮训
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    private async UniTask<TaskRecord> PollTaskStatus(string taskId)
    {
        return await _tasksStatusApi.PollTaskStatus(
            GameContext.Instance.TasksStatusUrl,
            taskId);
    }
}
