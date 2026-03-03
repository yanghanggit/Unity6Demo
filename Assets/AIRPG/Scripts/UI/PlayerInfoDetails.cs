using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Newtonsoft.Json;


public class PlayerInfoDetails : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _playerImage;
    [SerializeField] private TMP_Text _playerInfoText;

    public event Action OnCloseButtonClickedCallback;

    void Start()
    {
        Debug.Assert(_playerImage != null, "_playerImage is null");
        Debug.Assert(_playerInfoText != null, "_playerInfoText is null");

        // 初始化为空
        _playerImage.sprite = null;
        _playerInfoText.text = "";

        // 刷新内容
        RefreshPlayerDetails();
    }

    private void RefreshPlayerDetails()
    {
        // 获取玩家实体
        var playerActorEntitySerialization = GameContext.Instance.GetActorEntity(GameContext.Instance.PlayerActorName);
        //Debug.Assert(playerActorEntitySerialization != null, "Player actor entity serialization is null for actor name: " + GameContext.Instance.PlayerActor);
        if (playerActorEntitySerialization == null)
        {
            Debug.LogError("Player actor entity serialization is null for actor name: " + GameContext.Instance.PlayerActorName);
            return;
        }

        var actorSprite = SpriteCacheManager.Instance.GetSprite(playerActorEntitySerialization.name);
        Debug.Assert(actorSprite != null, "Player actor sprite is null for entity: " + playerActorEntitySerialization.name);
        _playerImage.sprite = actorSprite;
        _playerImage.gameObject.SetActive(false); // 先隐藏图片，避免空白显示

        // 获取 CombatStatsComponent
        var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(playerActorEntitySerialization);
        Debug.Assert(combatStatsComponent != null, "CombatStatsComponent is null for player actor: " + playerActorEntitySerialization.name);
        Debug.Assert(combatStatsComponent.stats != null, "CombatStatsComponent.stats is null for player actor: " + playerActorEntitySerialization.name);

        //InventoryComponent
        var inventoryComponent = GameUtils.GetComponent<InventoryComponent>(playerActorEntitySerialization);
        Debug.Assert(inventoryComponent != null, "InventoryComponent is null for player actor: " + playerActorEntitySerialization.name);
        Debug.Assert(inventoryComponent.items != null, "InventoryComponent.items is null for player actor: " + playerActorEntitySerialization.name);

        // 格式化显示 CombatStatsComponent 的属性
        var stats = combatStatsComponent.stats;
        string statsText = $"玩家属性\n\n";
        statsText += $"当前HP: {stats.hp}\n";
        statsText += $"最大HP: {stats.max_hp}\n";
        statsText += $"攻击力: {stats.attack}\n";
        statsText += $"防御力: {stats.defense}\n";

        if (inventoryComponent.items.Count > 0)
        {
            statsText += $"\n物品列表:\n";
            foreach (var item in inventoryComponent.items)
            {
                // 这里直接添加 item 的json 反序列化后的字符串表示
                string itemJson = JsonConvert.SerializeObject(item, Formatting.Indented);
                statsText += $"{itemJson}\n";
            }
        }
        else
        {
            statsText += "\n物品列表: 无\n";
        }

        // 获取并显示 SkillBookComponent
        var skillBookComponent = GameUtils.GetComponent<SkillBookComponent>(playerActorEntitySerialization);
        if (skillBookComponent != null && skillBookComponent.skills != null && skillBookComponent.skills.Count > 0)
        {
            statsText += $"\n{GameUtils.FormatSkillBookComponent(skillBookComponent)}";
        }
        else
        {
            statsText += "\n技能列表: 无\n";
        }

        _playerInfoText.text = statsText;
    }

    public void OnClickClosePlayerInfoDetails()
    {
        Debug.Log("Player info details clicked!");
        OnCloseButtonClickedCallback?.Invoke();
    }
}
