using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class ArbitrationPanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _arbitrationText; // 仲裁面板文本对象

    [Header("API Components")]
    [SerializeField] private TasksStatusApi _tasksStatusApi; // 轮询任务状态的 API 组件
    [SerializeField] private Button _closeButton; // 关闭仲裁面板的按钮

    void Start()
    {
        Debug.Assert(_arbitrationText != null, "_arbitrationText is null");
        Debug.Assert(_tasksStatusApi != null, "_tasksStatusApi is null");
        Debug.Assert(_closeButton != null, "_closeButton is null");
    }

    /// <summary>
    /// 进入仲裁阶段的完整处理流程：隐藏关闭按钮，显示加载文本。
    /// 未登录时直接展示 mock 回合数据；已登录时异步执行 PlayCards，
    /// 轮询任务完成后获取最新战斗回合数据并更新面板显示。
    /// </summary>
    public async UniTaskVoid EnterArbitrationPhaseAsync()
    {
        _closeButton.gameObject.SetActive(false);

        if (!GameContext.Instance.IsLoggedIn)
        {
            var mockRound = new Round
            {
                action_order = new List<string> { "Hero", "Goblin", "Mage" },
                combat_log = "Hero attacks Goblin for 30 damage. Goblin is defeated. Mage casts Fireball on Hero for 20 damage.",
                narrative = "The battle begins! The Hero strikes first, taking down the Goblin. The Mage retaliates with a fiery spell, scorching the Hero."
            };
            _arbitrationText.text = GameUtils.FormatRoundInfo(mockRound);
            _closeButton.gameObject.SetActive(true);
            return;
        }

        _arbitrationText.text = "正在执行仲裁操作，请稍候...";

        string taskId = await DungeonGamePlayManager.Instance.PlayCards();
        if (string.IsNullOrEmpty(taskId))
        {
            _arbitrationText.text = "Failed to initiate PlayCards action.";
            _closeButton.gameObject.SetActive(true);
            return;
        }

        Debug.Log($"PlayCards initiated successfully, task ID: {taskId}");
        var taskRecord = await PollTaskStatus(taskId);
        if (taskRecord == null)
        {
            _arbitrationText.text = "Failed to retrieve task status.";
            _closeButton.gameObject.SetActive(true);
            return;
        }

        var combat = await GameStateSync.Instance.GetCombat();
        if (combat == null)
        {
            _arbitrationText.text = "Failed to retrieve combat data.";
            _closeButton.gameObject.SetActive(true);
            return;
        }

        var round = combat.rounds.Count > 0 ? combat.rounds[^1] : null;
        if (round == null)
        {
            _arbitrationText.text = "No round data available.";
            _closeButton.gameObject.SetActive(true);
            return;
        }

        _arbitrationText.text = GameUtils.FormatRoundInfo(round);
        _closeButton.gameObject.SetActive(true);

        // 通知其他系统战斗状态已更新，确保站位面板等内容及时刷新
        DungeonGamePlayManager.Instance.CombatStatusEvaluation().Forget();
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
