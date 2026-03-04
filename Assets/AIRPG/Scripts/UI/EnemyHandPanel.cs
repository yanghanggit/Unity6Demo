using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyHandPanel : MonoBehaviour
{

    [Header("UI Components")]
    [SerializeField] private TMP_Text _infoText; // 战斗信息显示对象

    private EntitySerialization _currentActor;

    public EntitySerialization CurrentActor
    {
        get => _currentActor;
        set
        {
            Debug.Assert(value != null, "CurrentActor cannot be null");
            _currentActor = value;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        //_infoText.text = $"Enemy: {GameUtils.GetDisplayName(actorEntity.name)}";

        HandComponent handComponent = null;
        if (GameContext.Instance.IsLoggedIn)
        {
            handComponent = GameUtils.GetComponent<HandComponent>(actorEntity);
            if (handComponent == null)
            {
                Debug.LogWarning($"EnemyHandPanel: HandComponent not found for actor: {actorEntity.name}");
                return;
            }
        }
        else
        {
            // mock 一个 HandComponent 数据
            handComponent = new HandComponent
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
        }

        _infoText.text = GameUtils.FormatHandComponent(handComponent);

    }
}
