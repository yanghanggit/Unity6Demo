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
    /// 根据当前角色数据更新构筑按钮的状态
    /// </summary>
    public void SetupForActor(EntitySerialization actorEntity)
    {
        Debug.Assert(actorEntity != null, "Current actor data is null");
        if (!GameContext.Instance.IsLoggedIn)
        {
            // mock 一个 HandComponent 数据
            var mockHandComponent = new HandComponent
            {
                name = "HandComponent",
                round = 1,
                cards = new List<Card>
                {
                    new() { name = "卡牌.普通攻击", action = "对目标造成普通攻击伤害", targets = new List<string> { actorEntity.name } },
                    new() { name = "卡牌.防御姿态", action = "进入防御姿态，减少受到的伤害", targets = new List<string> { actorEntity.name } },
                    new() { name = "卡牌.蓄力", action = "蓄积力量，下一次攻击伤害大幅提升", targets = new List<string> { actorEntity.name } }
                }
            };

            _infoText.text = GameUtils.FormatHandComponent(mockHandComponent);
            return;
        }

        // 根据角色数据获取 HandComponent 组件，并格式化显示在界面上
        var handComponent = GameUtils.GetComponent<HandComponent>(actorEntity);
        if (handComponent != null)
        {
            _infoText.text = GameUtils.FormatHandComponent(handComponent);
        }
        else
        {
            Debug.LogWarning($"HandComponent not found for actor: {actorEntity.name}");
            _infoText.text = "(从游戏数据加载手牌信息中...)";
            SetupForActorAsync(actorEntity).Forget();
        }
    }

    /// <summary>
    /// 根据当前角色数据异步加载并更新界面显示，适用于需要从资源或网络加载数据的情况。
    /// 目前游戏设计中敌人的手牌数据是直接包含在角色数据中的，因此这个方法暂时没有实际的异步操作，但保留这个接口以便未来扩展。
    /// </summary>
    /// <param name="actorEntity"></param>
    /// <returns></returns>
    private async UniTaskVoid SetupForActorAsync(EntitySerialization actorEntity)
    {
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

        // 刷新角色数据以获取最新的手牌信息
        var actorEntities = await GameStateSync.Instance.GetEntities(new List<string> { actorEntity.name });
        if (actorEntities == null)
        {
            Debug.LogError($"Failed to refresh actor entities from server for actor: {actorEntity.name}");
            _infoText.text = "(加载手牌信息失败)";
            return;
        }

        var handComponent = GameUtils.GetComponent<HandComponent>(actorEntities[0]);
        if (handComponent != null)
        {
            _infoText.text = GameUtils.FormatHandComponent(handComponent);
        }
        else
        {
            Debug.LogWarning($"HandComponent still not found for actor: {actorEntity.name} after refresh");
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
