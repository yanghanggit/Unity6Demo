using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DungeonCombatScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string _preScene = "MainScene";
    [SerializeField] private string _nextScene = "DungeonCombatScene";

    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText; // 主文本显示对象
    [SerializeField] private GameObject _backgroundImage; // 背景图片对象

    [Header("API Components")]
    [SerializeField] private TasksStatusApi _tasksStatusApi;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_backgroundImage != null, "_backgroundImage is null");
        Debug.Assert(_tasksStatusApi != null, "_tasksStatusApi is null");

        // 检查是否已经连接服务器
        if (GameContext.Instance.IsLoggedIn)
        {
            // 已经连接服务器，开始初始化战斗场景
            var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.PlayerActor);
            _mainText.text = $"{GameContext.Instance.Dungeon.name} | {stageName} : Initializing combat scene...";

            // 刷新场景初始化
            ExecuteCombatInit().Forget();
        }
        else
        {
            // 没有连接服务器，基本是本地测试模式
            Debug.Log("DungeonCombatScene: Not logged in, running in local test mode");
            _mainText.text = "本地测试模式：未连接服务器，无法进行完整战斗操作。";
        }
    }

    public void OnClickViewDungeon()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Not logged in, cannot view dungeon");
            return;
        }
        RefreshDungeonStateDisplay().Forget();
    }

    public void OnClickViewActor()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Not logged in, cannot view actor");
            return;
        }

        //Debug.Log("OnClickViewActor");
        ExecuteViewActorStats().Forget();
    }

    public void OnClickViewCards()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Not logged in, cannot view cards");
            return;
        }

        //Debug.Log("OnClickViewCards");
        ExecuteViewActorCards().Forget();
    }

    public void OnClickDrawCards()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Not logged in, cannot draw cards");
            return;
        }

        //Debug.Log("OnClickDrawCards");
        List<AllyDrawCardAction> specifiedActions = GenerateAllyDrawCardActions();
        ExecuteDrawCardsAndShowHands(specifiedActions, true).Forget();
    }

    public void OnClickPlayCards()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Not logged in, cannot play cards");
            return;
        }

        //Debug.Log("OnClickPlayCards");
        ExecutePlayCardsAndShowResult().Forget();
    }

    public void OnClickAdvanceNextDungeon()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Not logged in, cannot advance to next dungeon");
            return;
        }

        //Debug.Log("OnClickAdvanceNextDungeon");

        // 检查当前战斗状态，决定执行哪个操作
        Combat currentCombat = GameUtils.GetLastCombat(GameContext.Instance.Dungeon);

        if (currentCombat != null && currentCombat.state == CombatState.COMPLETE)
        {
            ExecutePostCombat().Forget();
        }
        else
        {
            ExecuteAdvanceNextDungeon().Forget();
        }
    }

    public void OnClickRetreatFromDungeon()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Not logged in, cannot retreat from dungeon");
            return;
        }

        //Debug.Log("OnClickRetreatFromDungeon");
        ExecuteRetreatFromDungeon().Forget();
    }

    public void OnClickBackHome()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Not logged in, cannot go back home");
            return;
        }

        //Debug.Log("OnClickBackHome");
        ExecuteBackHome().Forget();
    }

    /// <summary>
    /// 点击角色头像显示该角色的战斗属性信息
    /// 根据点击的角色索引获取对应的角色实体，提取战斗属性组件并格式化显示
    /// </summary>
    public void OnClickCombatActor(GameObject gameObject)
    {
        Debug.Log($"OnClickCombatActor: Clicked on actor {gameObject.name}");
    }

    /// <summary>
    /// 点击战斗运行操作
    /// </summary>
    public void OnClickCombatRun()
    {
        Debug.Log("OnClickCombatRun: Run action triggered");
    }

    /// <summary> 
    /// 下一场战斗操作
    /// </summary>
    public void OnClickNextCombat()
    {
        Debug.Log("OnClickNextCombat: Next combat action triggered");
    }

    /// <summary>
    /// 初始化战斗并刷新地下城状态
    /// 调用服务器 combat_init 接口开始战斗，成功后刷新并显示当前地下城状态
    /// </summary>
    private async UniTaskVoid ExecuteCombatInit()
    {
        var messages = await DungeonGamePlayManager.Instance.CombatInit();
        if (messages != null)
        {
            await RefreshDungeonStateDisplay();
        }
    }

    /// <summary>
    /// 执行抽卡操作并轮询任务状态，完成后显示手牌
    /// 调用服务器 draw_cards 接口，获取任务ID后轮询查询任务状态
    /// 当任务完成时，刷新数据并显示角色手牌信息
    /// </summary>
    private async UniTaskVoid ExecuteDrawCardsAndShowHands(List<AllyDrawCardAction> specifiedActions, bool enable_enemy_draw)
    {
        string taskId = await DungeonGamePlayManager.Instance.DrawCards(specifiedActions, enable_enemy_draw);
        if (string.IsNullOrEmpty(taskId))
        {
            return;
        }

        Debug.Log($"DrawCards initiated successfully, task ID: {taskId}");
        _mainText.text = "请求已提交，正在处理中...";

        var taskRecord = await PollTaskStatus(taskId);
        if (taskRecord == null)
        {
            _mainText.text = "任务轮询彻底失败";
            return;
        }

        _mainText.text = "处理完成，正在加载结果...";
        ExecuteViewActorCards().Forget();
    }

    /// <summary>
    /// 显示所有角色的手牌信息
    /// 遍历所有角色实体，提取手牌组件并格式化显示
    /// </summary>
    private void DisplayAllActorsHands()
    {
        var text = string.Empty;

        var actorEntitiesSerialization = GameContext.Instance.ActorEntities;
        foreach (var actorEntity in actorEntitiesSerialization)
        {
            var handComponent = GameUtils.GetComponent<HandComponent>(actorEntity);
            if (handComponent == null)
            {
                continue;
            }

            text += GameUtils.FormatHandComponent(handComponent);
            text += "\n";
        }

        if (string.IsNullOrEmpty(text))
        {
            _mainText.text = "当前没有角色持有手牌信息。";
        }
        else
        {
            _mainText.text = "当前角色手牌信息：\n\n" + text;
        }
    }

    /// <summary>
    /// 执行打牌操作并轮询任务状态，完成后显示结果
    /// 调用服务器 play_cards 接口，获取任务ID后轮询查询任务状态
    /// 当任务完成时，刷新数据并显示战斗仲裁结果
    /// </summary>
    private async UniTaskVoid ExecutePlayCardsAndShowResult()
    {
        string taskId = await DungeonGamePlayManager.Instance.PlayCards();
        if (string.IsNullOrEmpty(taskId))
        {
            return;
        }

        Debug.Log($"PlayCards initiated successfully, task ID: {taskId}");
        _mainText.text = "打牌请求已提交，正在处理中...";

        var taskRecord = await PollTaskStatus(taskId);
        if (taskRecord == null)
        {
            return;
        }

        _mainText.text = "打牌处理完成，正在加载结果...";

        var dungeon = await GameStateSync.Instance.RefreshDungeonFromServer();
        if (dungeon == null)
        {
            Debug.LogError("Failed to refresh dungeon data");
            _mainText.text = "刷新地下城数据失败";
            return;
        }

        DisplayLastRoundInfo();
        await DungeonGamePlayManager.Instance.CombatStatusEvaluation();
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
    /// 显示最新的地下城回合信息
    /// 获取最后一个回合并格式化显示战斗仲裁信息，如果没有回合或信息则显示相应提示
    /// </summary>
    private void DisplayLastRoundInfo()
    {
        // 获取最新的地下城回合信息
        Round round = GameUtils.GetLastRound(GameContext.Instance.Dungeon);
        if (round == null)
        {
            Debug.LogWarning("No rounds found in dungeon after playing cards");
            _mainText.text = "打牌完成，但未找到回合信息";
            return;
        }

        // 显示最新的地下城战斗仲裁信息
        var formattedRoundInfo = GameUtils.FormatRoundInfo(round);
        if (!string.IsNullOrEmpty(formattedRoundInfo))
        {
            _mainText.text = formattedRoundInfo;
        }
        else
        {
            Debug.LogWarning("No combat arbitration info available in dungeon state");
            _mainText.text = "打牌完成，但未找到战斗仲裁信息";
        }
    }

    /// <summary>
    /// 刷新并显示地下城状态
    /// 从服务器获取最新的地下城和角色数据，然后更新UI显示当前场景的角色分布和战斗信息
    /// </summary>
    private async UniTask RefreshDungeonStateDisplay()
    {
        var refreshErr = await GameStateSync.Instance.RefreshDungeonAndActorsFromServer();
        if (refreshErr != GameSyncError.None)
        {
            Debug.LogError($"Failed to refresh dungeon and actors data: {refreshErr}");
            _mainText.text = "刷新地下城状态失败";
            return;
        }

        // 获取当前角色所在场景及该场景中的所有角色
        var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.PlayerActor);
        Debug.Assert(stageName != "", "[GameStateSync] Current actor's stage name is empty");

        //
        SetBackgroundImage(stageName);

        // 需要所有的角色名称列表！
        var actorsInStage = GameContext.Instance.GetActorsInStage(stageName);

        // 格式化并显示地下城状态（包括场景-角色映射和战斗序列信息）
        _mainText.text = GameUtils.FormatDungeonStateDisplay(GameContext.Instance.Dungeon, new Dictionary<string, List<string>> { { stageName, actorsInStage } });
    }

    /// <summary>
    /// 更新场景背景图片
    /// 根据当前角色所在场景从缓存中获取并更新背景图片，如果未找到则清空背景
    /// </summary>
    private void SetBackgroundImage(string stageName)
    {
        // 获取当前角色所在场景
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(stageName);
        if (cachedSprite != null)
        {
            _backgroundImage.GetComponent<Image>().sprite = cachedSprite;
        }
        else
        {
            Debug.LogWarning($"DungeonCombatScene: Background sprite not found for stage: {stageName}");
            _backgroundImage.GetComponent<Image>().sprite = null;
        }
    }

    /// <summary>
    /// 查看并显示所有角色的战斗属性
    /// 从服务器刷新数据后，获取所有角色的战斗属性组件并格式化显示
    /// </summary>
    private async UniTaskVoid ExecuteViewActorStats()
    {
        var refreshErr = await GameStateSync.Instance.RefreshDungeonAndActorsFromServer();
        if (refreshErr != GameSyncError.None)
        {
            Debug.LogError($"Failed to refresh dungeon and actors data: {refreshErr}");
            _mainText.text = "刷新角色数据失败";
            return;
        }

        var text = "";
        var actorEntitiesSerialization = GameContext.Instance.ActorEntities;
        for (int i = 0; i < actorEntitiesSerialization.Count; i++)
        {
            var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(actorEntitiesSerialization[i]);
            if (combatStatsComponent == null)
            {
                Debug.Assert(false, "combatStatsComponent is null");
                continue;
            }
            text += GameUtils.FormatCombatStatsComponent(combatStatsComponent);
            text += "\n";
        }
        _mainText.text = text;
    }

    /// <summary>
    /// 查看并显示所有角色的手牌信息
    /// 从服务器刷新数据后,直接显示所有角色当前持有的手牌
    /// 用于在游戏过程中随时查看角色的手牌状态
    /// </summary>
    private async UniTaskVoid ExecuteViewActorCards()
    {
        var refreshErr = await GameStateSync.Instance.RefreshDungeonAndActorsFromServer();
        if (refreshErr != GameSyncError.None)
        {
            Debug.LogError($"Failed to refresh dungeon and actors data: {refreshErr}");
            _mainText.text = "刷新数据失败";
            return;
        }

        DisplayAllActorsHands();
    }

    /// <summary>
    /// 战斗后处理
    /// 调用服务器 post_combat 接口进行战斗后处理，成功后刷新地下城状态显示
    /// </summary>
    private async UniTaskVoid ExecutePostCombat()
    {
        _mainText.text = "正在执行战斗后处理...";

        var responseSessionMessages = await DungeonGamePlayManager.Instance.PostCombat();
        if (responseSessionMessages == null)
        {
            return;
        }

        // 然后逐个处理返回的 SessionMessage，特别是 CombatArchiveEvent
        _mainText.text = "";
        for (int i = 0; i < responseSessionMessages.Count; i++)
        {
            SessionMessage sessionMessage = responseSessionMessages[i];
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
                    _mainText.text += "\n\n" + combatArchiveEvent.actor + ":" + combatArchiveEvent.summary;
                }
            }
        }

        // 最后需要获取最新个的数据，因为服务器推进了地下城状态
        var dungeon = await GameStateSync.Instance.RefreshDungeonFromServer();
        if (dungeon == null)
        {
            Debug.LogError("Failed to refresh dungeon data after post combat");
        }
        else
        {
            Debug.Log("Successfully refreshed dungeon data after post combat");
        }
    }

    /// <summary>
    /// 前进到下一个地下城关卡
    /// 调用服务器 advance_next_dungeon 接口，成功后刷新并显示新的地下城状态
    /// </summary>
    private async UniTaskVoid ExecuteAdvanceNextDungeon()
    {
        var messages = await DungeonGamePlayManager.Instance.AdvanceNextDungeon();
        if (messages == null)
        {
            return;
        }

        await UniTask.Yield();
        SceneManager.LoadScene(_nextScene);
    }

    /// <summary>
    /// 从地下城撤退
    /// 调用服务器撤退接口，成功后切换回主场景
    /// </summary>
    private async UniTaskVoid ExecuteRetreatFromDungeon()
    {
        _mainText.text = "正在从地下城撤退...";

        var messages = await DungeonGamePlayManager.Instance.RetreatFromDungeon();
        if (messages != null)
        {
            await UniTask.Yield();
            SceneManager.LoadScene(_preScene);
        }
    }

    /// <summary>
    /// 返回主场景
    /// 调用服务器传送回家接口，成功后切换到主场景
    /// </summary>
    private async UniTaskVoid ExecuteBackHome()
    {
        bool success = await DungeonGamePlayManager.Instance.TransHome();
        if (success)
        {
            await UniTask.Yield();
            SceneManager.LoadScene(_preScene);
        }
    }

    /// <summary>
    /// 为所有活着的盟友生成抽卡行动
    /// 每个盟友随机选择一个技能、一个敌人目标和一个状态效果
    /// </summary>
    /// <returns>盟友抽卡行动列表</returns>
    private List<AllyDrawCardAction> GenerateAllyDrawCardActions()
    {
        var actions = new List<AllyDrawCardAction>();
        var aliveAllies = GameContext.Instance.GetAliveExpeditionMembersInCurrentCombatStage();
        var aliveEnemies = GameContext.Instance.GetAliveEnemiesInCurrentCombatStage();

        // 如果没有敌人，无法生成攻击行动
        if (aliveEnemies.Count == 0)
        {
            Debug.LogWarning("No alive enemies found, cannot generate draw card actions");
            return actions;
        }

        foreach (var allyEntity in aliveAllies)
        {
            // 获取盟友的技能书组件
            var skillBookComponent = GameUtils.GetComponent<SkillBookComponent>(allyEntity);
            if (skillBookComponent == null || skillBookComponent.skills.Count == 0)
            {
                Debug.LogWarning($"Ally {allyEntity.name} has no skills, skipping");
                continue;
            }

            // 获取盟友的战斗属性组件
            var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(allyEntity);
            if (combatStatsComponent == null)
            {
                Debug.LogWarning($"Ally {allyEntity.name} has no combat stats component, skipping");
                continue;
            }

            // 随机选择一个技能
            var randomSkill = skillBookComponent.skills[Random.Range(0, skillBookComponent.skills.Count)];

            // 随机选择一个敌人作为目标
            var randomEnemy = aliveEnemies[Random.Range(0, aliveEnemies.Count)];

            // 随机选择一个状态效果（如果有）
            var statusEffectNames = new List<string>();
            if (combatStatsComponent.status_effects.Count > 0)
            {
                var randomStatusEffect = combatStatsComponent.status_effects[Random.Range(0, combatStatsComponent.status_effects.Count)];
                statusEffectNames.Add(randomStatusEffect.name);
            }

            // 创建抽卡行动
            var action = new AllyDrawCardAction
            {
                entity_name = allyEntity.name,
                skill_name = randomSkill.name,
                target_names = new List<string> { randomEnemy.name },
                status_effect_names = statusEffectNames
            };

            actions.Add(action);
            Debug.Log($"Generated draw card action: {allyEntity.name} uses {randomSkill.name} on {randomEnemy.name}");
        }

        return actions;
    }
}
