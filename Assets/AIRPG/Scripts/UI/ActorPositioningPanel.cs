using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ActorPositioningPanel : MonoBehaviour
{

    // 本游戏目前的最高敌人数量为3，最高探险队成员数量为3，因此总共需要6个定位对象来显示所有角色。
    public static readonly int MaxPositioningObjects = 6;

    [Header("UI Components")]
    [SerializeField] private ActorPositioningObject[] _positioningObjects;
    [SerializeField] private Button _playButton;

    void Start()
    {
        Debug.Assert(_positioningObjects != null && _positioningObjects.Length == MaxPositioningObjects, "Positioning objects array is not assigned in the inspector.");
        Debug.Assert(_playButton != null, "_playButton is not assigned in the inspector.");
    }

    /// <summary>
    /// 自主拉取战斗数据并刷新整个面板，包括角色站位布局和操作按钮文本。
    /// 未登录时自动回退到 mock 数据。
    /// </summary>
    public async UniTaskVoid RefreshCombatViewAsync()
    {
        List<EntitySerialization> sortedActorEntities;
        Combat combat;

        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("ActorPositioningPanel: Player is not logged in, using mock data to refresh combat view");
            sortedActorEntities = MockData.CreateActorData();
            combat = new Combat();
        }
        else
        {
            // 阶段1：并行获取战斗状态和场景-演员映射关系（两者互相独立）
            var (combatData, stagesState) = await UniTask.WhenAll(
                GameStateSync.Instance.GetCombat(),
                GameStateSync.Instance.GetStagesState()
            );

            if (combatData == null)
            {
                Debug.LogError("ActorPositioningPanel: Combat data is null, cannot refresh combat view");
                return;
            }

            if (stagesState == null)
            {
                Debug.LogError("ActorPositioningPanel: Stages state data is null, cannot determine current stage and actors");
                return;
            }

            // 阶段2：依据映射结果获取当前场景中的演员列表
            List<string> actorNamesInStage = new();
            foreach (var kvp in stagesState)
            {
                if (kvp.Value.Contains(GameContext.Instance.PlayerActorName))
                {
                    actorNamesInStage = kvp.Value;
                    break;
                }
            }

            var actorEntitiesInStage = await GameStateSync.Instance.GetEntities(actorNamesInStage);
            if (actorEntitiesInStage == null)
            {
                Debug.LogError("ActorPositioningPanel: Actor entities data is null, cannot refresh combat view");
                return;
            }

            combat = combatData;
            sortedActorEntities = GameUtils.SortActorsByCreationOrder(actorEntitiesInStage);
            Debug.Log($"Sorted actor entities by creation order: {string.Join(", ", sortedActorEntities.ConvertAll(e => e.name))}");
        }

        // 刷新站位界面显示
        var round = combat.rounds.Count > 0 ? combat.rounds[^1] : null;
        var actionOrder = round != null ? round.action_order : new List<string>();
        ArrangeActorsInSlots(sortedActorEntities, actionOrder);

        // 根据战斗状态更新按钮文本
        UpdatePlayButtonState(sortedActorEntities, combat);
    }

    /// <summary>
    /// 根据当前战斗状态更新操作按钮的文本显示，例如如果当前是玩家回合可以显示 "Play"，如果是敌人回合可以显示 "Next" 等等。
    /// </summary>
    /// <param name="sortedActorEntities"></param>
    /// <param name="combat"></param>
    private void UpdatePlayButtonState(List<EntitySerialization> sortedActorEntities, Combat combat)
    {
        // 更新按钮
        _playButton.GetComponentInChildren<TMP_Text>().text = combat.state.ToString(); // TODO: 根据实际情况设置按钮文本，比如如果当前是玩家回合可以显示 "Play"，如果是敌人回合可以显示 "Next" 等等。


        switch (combat.state)
        {
            case CombatState.ONGOING:
                {
                    var hasHandComponentEntities = GameUtils.FilterEntitiesByComponent<HandComponent>(sortedActorEntities);
                    var hasDeathComponentEntities = GameUtils.FilterEntitiesByComponent<DeathComponent>(sortedActorEntities);

                    // 计算活着的角色数量，就是 sortedActorEntities.Count 减去包含 DeathComponent 的实体数量
                    int aliveCount = sortedActorEntities.Count - hasDeathComponentEntities.Count;
                    if (aliveCount > 0 && hasHandComponentEntities.Count >= aliveCount)
                    {
                        // 所有活着的角色都有手牌了，可以执行行动了
                        _playButton.GetComponentInChildren<TMP_Text>().text = "演绎行动";
                    }
                    else
                    {
                        // 还有角色没有手牌，或者所有角色都死了，不能执行行动
                        _playButton.GetComponentInChildren<TMP_Text>().text = $"构建行动{hasHandComponentEntities.Count}/{aliveCount}";
                    }

                }
                break;

            case CombatState.COMPLETE:
                _playButton.GetComponentInChildren<TMP_Text>().text = combat.result == CombatResult.WIN ? "胜利" : "失败";
                break;

            case CombatState.POST_COMBAT:
                Debug.Log("战斗结束，进入战斗结算界面");
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 将角色实体按敌人（槽位 0~2）和探险队成员（槽位 3~5）分组，根据数量自动居中摆放到对应槽位。
    /// </summary>
    /// <param name="sortedActorEntities">已排序的角色实体列表。</param>
    /// <param name="actionOrder">当前回合的行动顺序列表，用于在槽位上显示行动指示器。</param>
    private void ArrangeActorsInSlots(List<EntitySerialization> sortedActorEntities, List<string> actionOrder)
    {
        // 先隐藏所有定位对象
        for (int i = 0; i < MaxPositioningObjects; i++)
        {
            _positioningObjects[i].gameObject.SetActive(false);
        }

        // 敌人占位 index 0~2，根据数量决定摆放位置
        var enemies = FilterEnemyEntities(sortedActorEntities);
        int[] enemyIndices = enemies.Count switch
        {
            1 => new[] { 1 },
            2 => new[] { 0, 2 },
            _ => new[] { 0, 1, 2 }
        };
        for (int i = 0; i < enemies.Count && i < enemyIndices.Length; i++)
        {
            _positioningObjects[enemyIndices[i]].gameObject.SetActive(true);
            _positioningObjects[enemyIndices[i]].RefreshView(enemies[i], actionOrder);
        }

        // 探险队成员占位 index 3~5，根据数量决定摆放位置
        var members = FilterExpeditionMemberEntities(sortedActorEntities);
        int[] memberIndices = members.Count switch
        {
            1 => new[] { 4 },
            2 => new[] { 3, 5 },
            _ => new[] { 3, 4, 5 }
        };
        for (int i = 0; i < members.Count && i < memberIndices.Length; i++)
        {
            _positioningObjects[memberIndices[i]].gameObject.SetActive(true);
            _positioningObjects[memberIndices[i]].RefreshView(members[i], actionOrder);
        }
    }

    /// <summary>
    /// 从角色实体列表中筛选出所有敌人实体（包含 <see cref="EnemyComponent"/> 的实体）。
    /// </summary>
    /// <param name="actorEntities">待筛选的角色实体列表。</param>
    /// <returns>仅包含敌人实体的列表。</returns>
    private List<EntitySerialization> FilterEnemyEntities(List<EntitySerialization> actorEntities)
    {
        List<EntitySerialization> enemyEntities = new();
        foreach (var entity in actorEntities)
        {
            var enemyComponent = GameUtils.GetComponent<EnemyComponent>(entity);
            if (enemyComponent != null)
            {
                enemyEntities.Add(entity);
            }
        }
        return enemyEntities;
    }

    /// <summary>
    /// 从角色实体列表中筛选出所有探险队成员实体（包含 <see cref="ExpeditionMemberComponent"/> 的实体）。
    /// </summary>
    /// <param name="actorEntities">待筛选的角色实体列表。</param>
    /// <returns>仅包含探险队成员实体的列表。</returns>
    private List<EntitySerialization> FilterExpeditionMemberEntities(List<EntitySerialization> actorEntities)
    {
        List<EntitySerialization> expeditionMemberEntities = new();
        foreach (var entity in actorEntities)
        {
            var expeditionMemberComponent = GameUtils.GetComponent<ExpeditionMemberComponent>(entity);
            if (expeditionMemberComponent != null)
            {
                expeditionMemberEntities.Add(entity);
            }
        }
        return expeditionMemberEntities;
    }
}
