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
    /// 当进入仲裁阶段时调用，根据当前游戏状态刷新仲裁面板显示内容
    /// </summary>
    public void OnArbitrationPhaseEntered()
    {
        // 进入仲裁阶段时隐藏关闭按钮，直到仲裁结果显示完成后再显示关闭按钮
        _closeButton.gameObject.SetActive(false);


        if (!GameContext.Instance.IsLoggedIn)
        {
            var mockRound = new Round
            {
                action_order = new List<string> { "Hero", "Goblin", "Mage" },
                combat_log = "Hero attacks Goblin for 30 damage. Goblin is defeated. Mage casts Fireball on Hero for 20 damage.",
                narrative = "The battle begins! The Hero strikes first, taking down the Goblin. The Mage retaliates with a fiery spell, scorching the Hero."
            };

            _closeButton.gameObject.SetActive(true); // 显示关闭按钮，允许玩家关闭仲裁面板
            _arbitrationText.text = GameUtils.FormatRoundInfo(mockRound);
        }
        else
        {
            _arbitrationText.text = "正在执行仲裁操作，请稍候...";
            ExecuteRoundAndDisplayResultAsync().Forget();
        }
    }

    /// <summary>
    ///  执行出牌操作，并轮询任务状态直到完成，最后刷新界面显示结果
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid ExecuteRoundAndDisplayResultAsync()
    {
        string taskId = await DungeonGamePlayManager.Instance.PlayCards();
        if (string.IsNullOrEmpty(taskId))
        {
            _arbitrationText.text = "Failed to initiate PlayCards action.";
            _closeButton.gameObject.SetActive(true); // 显示关闭按钮，允许玩家关闭仲裁面板
            return;
        }

        Debug.Log($"PlayCards initiated successfully, task ID: {taskId}");
        var taskRecord = await PollTaskStatus(taskId);
        if (taskRecord == null)
        {
            _arbitrationText.text = "Failed to retrieve task status.";
            _closeButton.gameObject.SetActive(true); // 显示关闭按钮，允许玩家关闭仲裁面板
            return;
        }

        var combat = await GameStateSync.Instance.GetCombat();
        if (combat == null)
        {
            _arbitrationText.text = "Failed to retrieve combat data.";
            _closeButton.gameObject.SetActive(true); // 显示关闭按钮，允许玩家关闭仲裁面板
            return;
        }

        var round = combat.rounds.Count > 0 ? combat.rounds[combat.rounds.Count - 1] : null;
        if (round == null)
        {
            _arbitrationText.text = "No round data available.";
            _closeButton.gameObject.SetActive(true); // 显示关闭按钮，允许玩家关闭仲裁面板
            return;
        }

        // 根据最新的回合数据刷新仲裁面板显示内容
        _closeButton.gameObject.SetActive(true); // 显示关闭按钮，允许玩家关闭仲裁面板
        _arbitrationText.text = GameUtils.FormatRoundInfo(round);

        // 同步发射一个事件，通知其他系统当前回合的战斗状态已经更新，确保站位面板等内容也能及时刷新显示
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
