using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;


public class PlayerInfoPanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _mainText;

    void Start()
    {
        Debug.Assert(_image != null, "_image is null");
        Debug.Assert(_mainText != null, "_mainText is null");

        // 初始化为空
        _mainText.text = string.Empty;
        _image.gameObject.SetActive(false); // 先隐藏图片，避免空白显示

        // 刷新内容
        RefreshPlayerDetails().Forget();
    }

    private async UniTaskVoid RefreshPlayerDetails()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            _mainText.text = "玩家未登录";
            return;
        }

        var actorEntities = await GameStateSync.Instance.GetEntities(
            new List<string> { GameContext.Instance.PlayerActorName }
        );

        if (actorEntities == null || actorEntities.Count == 0)
        {
            Debug.LogError("PlayerInfoDetails: Player actor entity not found for name: " + GameContext.Instance.PlayerActorName);
            _mainText.text = "玩家信息未找到";
            return;
        }

        // 获取玩家实体
        var entitySerialization = actorEntities[0];

        // 获取 CombatStatsComponent
        var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(entitySerialization);

        //InventoryComponent
        var inventoryComponent = GameUtils.GetComponent<InventoryComponent>(entitySerialization);

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

        _mainText.text = statsText;
    }

    /// <summary>
    /// 点击关闭按钮的回调方法
     /// 隐藏玩家信息面板
    /// </summary>
    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}
