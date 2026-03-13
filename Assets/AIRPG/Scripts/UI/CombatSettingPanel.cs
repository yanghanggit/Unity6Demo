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

    /// <summary>
    /// 点击撤退按钮的处理逻辑，根据新的战斗状态切换 UI 显示和交互逻辑
    /// </summary>
    /// <returns></returns>
    public async UniTaskVoid ExecuteRetreatSync()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            await UniTask.Yield();
            SceneManager.LoadScene(MainSceneName);
            return;
        }

        var retreatResponse = await DungeonGamePlayManager.Instance.RetreatFromCombat();
        if (retreatResponse == null || string.IsNullOrEmpty(retreatResponse.task_id))
        {
            Debug.LogError("Retreat failed: no response data");
            return;
        }

        Debug.Log($"Retreat initiated successfully, task ID: {retreatResponse.task_id}");
        var taskRecord = await PollTaskStatus(retreatResponse.task_id);
        if (taskRecord == null)
        {
            Debug.LogError($"Failed to get task record for task ID: {retreatResponse.task_id}");
            return;
        }

        Debug.Log($"Retreat successful: {retreatResponse.message}");

        var exitResponse = await DungeonGamePlayManager.Instance.ExitDungeon();
        if (exitResponse == null)
        {
            Debug.LogWarning("Failed to exit dungeon, no messages returned");
            //_mainText.text = "退出地下城失败！";
            return;
        }


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
