using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class EnemyHandPanel : MonoBehaviour
{

    [Header("UI Components")]
    [SerializeField] private TMP_Text _infoText; // 战斗信息显示对象


    [SerializeField] private TasksStatusApi _tasksStatusApi;

    void Start()
    {
        Debug.Assert(_infoText != null, "_infoText is null");
    }

    /// <summary>
    /// 根据当前角色数据更新敌人手牌面板显示。
    /// 同步快路径：未登录时使用 mock 数据直接显示。
    /// 异步流程：先向服务器请求一次最新角色数据并尝试获取 HandComponent；
    /// 若仍缺失，则发起 DrawCards → 轮询任务完成 → 再次刷新角色数据。
    /// </summary>
    /// <param name="actorEntityName">目标角色的实体名称</param>
    public async UniTaskVoid SetupForActorAsync(string actorEntityName)
    {
        Debug.Assert(!string.IsNullOrEmpty(actorEntityName), "Actor entity name is null or empty");

        // 未登录时使用 mock 手牌数据直接展示
        if (!GameContext.Instance.IsLoggedIn)
        {
            var mockHandComponent = new HandComponent
            {
                name = "HandComponent",
                round = 1,
                cards = new List<Card>
                {
                    new() { name = "卡牌.普通攻击", action = "对目标造成普通攻击伤害", targets = new List<string> { actorEntityName } },
                    new() { name = "卡牌.防御姿态", action = "进入防御姿态，减少受到的伤害", targets = new List<string> { actorEntityName } },
                    new() { name = "卡牌.蓄力", action = "蓄积力量，下一次攻击伤害大幅提升", targets = new List<string> { actorEntityName } }
                }
            };
            _infoText.text = GameUtils.FormatHandComponent(mockHandComponent);
            return;
        }

        _infoText.text = "(从游戏数据加载手牌信息中...)";

        // 先从服务器拉取一次最新角色数据，尝试获取 HandComponent
        var actorEntities = await GameStateSync.Instance.GetEntities(new List<string> { actorEntityName });
        if (actorEntities != null)
        {
            var fetchedHandComponent = GameUtils.GetComponent<HandComponent>(actorEntities[0]);
            if (fetchedHandComponent != null)
            {
                _infoText.text = GameUtils.FormatHandComponent(fetchedHandComponent);
                return;
            }
        }

        _infoText.text = "(未获取到手牌信息，正在请求服务器抽卡...)";

        // 仍未获取到 HandComponent，发起 DrawCards 请求
        Debug.LogWarning($"HandComponent not found for actor: {actorEntityName}, fetching via DrawCards API");

        string taskId = await DungeonGamePlayManager.Instance.DrawCards(new List<AllyDrawCardAction>(), true);
        if (string.IsNullOrEmpty(taskId))
        {
            Debug.LogError("DrawCards API call failed, no task ID returned");
            _infoText.text = "(加载手牌信息失败)";
            return;
        }

        Debug.Log($"DrawCards initiated successfully, task ID: {taskId}");
        var taskRecord = await PollTaskStatus(taskId);
        if (taskRecord == null)
        {
            Debug.LogError($"Failed to get task record for task ID: {taskId}");
            _infoText.text = "(加载手牌信息失败)";
            return;
        }

        // DrawCards 完成后再次刷新角色数据以获取最新手牌
        var refreshedEntities = await GameStateSync.Instance.GetEntities(new List<string> { actorEntityName });
        if (refreshedEntities == null)
        {
            Debug.LogError($"Failed to refresh actor entities from server for actor: {actorEntityName}");
            _infoText.text = "(加载手牌信息失败)";
            return;
        }

        var refreshedHandComponent = GameUtils.GetComponent<HandComponent>(refreshedEntities[0]);
        if (refreshedHandComponent != null)
        {
            _infoText.text = GameUtils.FormatHandComponent(refreshedHandComponent);
        }
        else
        {
            Debug.LogWarning($"HandComponent still not found for actor: {actorEntityName} after DrawCards");
            _infoText.text = "(加载手牌信息失败)";
        }
    }

    /// <summary>
    /// 轮询服务器获取指定任务的执行状态，直到任务完成并返回结果。这个方法用于等待异步操作完成，例如等待抽卡操作完成后获取最新的角色数据。
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
