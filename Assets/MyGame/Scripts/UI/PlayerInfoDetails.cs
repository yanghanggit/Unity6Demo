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
        var playerActorEntitySerialization = GameContext.Instance.GetActorEntitySerialization(GameContext.Instance.ActorName);
        Debug.Assert(playerActorEntitySerialization != null, "Player actor entity serialization is null for actor name: " + GameContext.Instance.ActorName);
        if (playerActorEntitySerialization == null)
        {
            return;
        }

        var actorSprite = SpriteCacheManager.Instance.GetSprite(playerActorEntitySerialization.name);
        Debug.Assert(actorSprite != null, "Player actor sprite is null for entity: " + playerActorEntitySerialization.name);
        //_playerImage.sprite = actorSprite;
        _playerImage.gameObject.SetActive(false); // 先隐藏图片，避免空白显示

        // 获取 CombatStatsComponent
        var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(playerActorEntitySerialization);
        if (combatStatsComponent != null && combatStatsComponent.stats != null)
        {
            // 格式化显示 CombatStatsComponent 的属性
            var stats = combatStatsComponent.stats;
            string statsText = $"玩家属性\n\n";
            statsText += $"经验值: {stats.experience}\n";
            statsText += $"当前等级: {stats.level}\n";
            statsText += $"当前HP: {stats.hp}\n";
            statsText += $"最大HP: {stats.max_hp}\n\n";
            
            statsText += $"基础属性:\n";
            statsText += $"基础最大HP: {stats.base_max_hp}\n";
            statsText += $"基础力量: {stats.base_strength}\n";
            statsText += $"基础敏捷: {stats.base_dexterity}\n";
            statsText += $"基础智慧: {stats.base_wisdom}\n\n";
            
            statsText += $"战斗属性:\n";
            statsText += $"力量: {stats.strength}\n";
            statsText += $"敏捷: {stats.dexterity}\n";
            statsText += $"智慧: {stats.wisdom}\n";
            statsText += $"物理攻击: {stats.physical_attack}\n";
            statsText += $"物理防御: {stats.physical_defense}\n";
            statsText += $"魔法攻击: {stats.magic_attack}\n\n";
            
            statsText += $"成长系数:\n";
            statsText += $"每级力量成长: {stats.strength_per_level}\n";
            statsText += $"每级敏捷成长: {stats.dexterity_per_level}\n";
            statsText += $"每级智慧成长: {stats.wisdom_per_level}\n";
            
            _playerInfoText.text = statsText;
        }
        else
        {
            _playerInfoText.text = "未找到玩家战斗属性数据";
            Debug.LogWarning("CombatStatsComponent not found for player");
        }
    }

    public void OnClickClosePlayerInfoDetails()
    {
        Debug.Log("Player info details clicked!");
        OnCloseButtonClickedCallback?.Invoke();
    }
}
