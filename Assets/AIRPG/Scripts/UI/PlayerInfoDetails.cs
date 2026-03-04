using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;


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
        //_playerImage.sprite = null;
        _playerInfoText.text = "";

        var actorSprite = SpriteCacheManager.Instance.GetSprite(GameContext.Instance.PlayerActorName);
        //Debug.Assert(actorSprite != null, "Player actor sprite is null for entity: " + entitySerialization.name);
        _playerImage.sprite = actorSprite;
        _playerImage.gameObject.SetActive(false); // 先隐藏图片，避免空白显示

        // 刷新内容
        RefreshPlayerDetails().Forget();
    }

    private async UniTaskVoid RefreshPlayerDetails()
    {
        var actorEntities = await GameStateSync.Instance.GetEntities(
            new List<string> { GameContext.Instance.PlayerActorName }
        );

        if (actorEntities == null || actorEntities.Count == 0)
        {
            Debug.LogError("PlayerInfoDetails: Player actor entity not found for name: " + GameContext.Instance.PlayerActorName);
            _playerInfoText.text = "玩家信息未找到";
            return;
        }

        //  var playerActorEntitySerialization = actorEntities[0];
        //  Debug.Assert(playerActorEntitySerialization != null, "Player actor entity serialization is null for actor name: " + GameContext.Instance.PlayerActorName);
        //  if (playerActorEntitySerialization == null)
        //  {
        //      Debug.LogError("Player actor entity serialization is null for actor name: " + GameContext.Instance.PlayerActorName);
        //      _playerInfoText.text = "玩家信息未找到";
        //      return;
        //  }

        //  // 获取角色头像
        //  var actorSprite = SpriteCacheManager.Instance.GetSprite(playerActorEntitySerialization.name);
        //  Debug.Assert(actorSprite != null, "Player actor sprite is null for entity: " + playerActorEntitySerialization.name);
        //  if (actorSprite == null)
        //  {
        //      Debug.LogWarning($"PlayerInfoDetails: Actor sprite not found for: {playerActorEntitySerialization.name}");
        //      _playerImage.sprite = null; // 或者设置为一个默认的占位图
        //  }
        //  else
        //  {
        //      _playerImage.sprite = actorSprite;
        //      _playerImage.gameObject.SetActive(true); // 显示图片
        //  }



        // 获取玩家实体
        var entitySerialization = actorEntities[0];
        //Debug.Assert(playerActorEntitySerialization != null, "Player actor entity serialization is null for actor name: " + GameContext.Instance.PlayerActor);
        // if (entitySerialization == null)
        // {
        //     Debug.LogError("Player actor entity serialization is null for actor name: " + GameContext.Instance.PlayerActorName);
        //     _playerInfoText.text = "玩家信息未找到";
        //     return;
        // }



        // 获取 CombatStatsComponent
        var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(entitySerialization);
        //Debug.Assert(combatStatsComponent != null, "CombatStatsComponent is null for player actor: " + entitySerialization.name);
        //Debug.Assert(combatStatsComponent.stats != null, "CombatStatsComponent.stats is null for player actor: " + entitySerialization.name);

        //InventoryComponent
        var inventoryComponent = GameUtils.GetComponent<InventoryComponent>(entitySerialization);
        //Debug.Assert(inventoryComponent != null, "InventoryComponent is null for player actor: " + entitySerialization.name);
        //Debug.Assert(inventoryComponent.items != null, "InventoryComponent.items is null for player actor: " + entitySerialization.name);

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
        var skillBookComponent = GameUtils.GetComponent<SkillBookComponent>(entitySerialization);
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
