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
        var playerActorEntitySerialization = GameContext.Instance.GetActorEntitySerialization(GameContext.Instance.PlayerActor);
        Debug.Assert(playerActorEntitySerialization != null, "Player actor entity serialization is null for actor name: " + GameContext.Instance.PlayerActor);
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
            statsText += $"当前HP: {stats.hp}\n";
            statsText += $"最大HP: {stats.max_hp}\n";
            statsText += $"攻击力: {stats.attack}\n";
            statsText += $"防御力: {stats.defense}\n";
            
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
