using UnityEngine;
using TMPro;

/// <summary>
/// 地牢战斗场景顶部UI控制器
/// </summary>
public class CombatTopBar : MonoBehaviour
{

    [Header("UI Components")]
    [SerializeField] private TMP_Text _infoText; // 战斗信息显示对象

    [Header("Setting Panel")]
    [SerializeField] private GameObject _settingPanel; // 设置面板对象

    void Start()
    {
        // 断言检查，确保所有必要的组件和数据都已正确设置
        Debug.Assert(_infoText != null, "_infoText is null");
        Debug.Assert(_settingPanel != null, "_settingPanel is null");

        _settingPanel.SetActive(false); // 初始时隐藏设置面板

        // 暂时不做过多的逻辑处理，主要负责显示当前地下城和关卡信息
        RefreshView();
    }

    /// <summary>
    /// 点击 Setting 按钮
    /// </summary>
    public void OnClickOpenSetting()
    {
        Debug.Log("[DungeonCombatTopBar] Setting button clicked");
        _settingPanel.SetActive(true);
    }

    /// <summary>
    /// 点击 Close Setting 按钮
    /// </summary>
    public void OnClickCloseSetting()
    {
        Debug.Log("[DungeonCombatScene] Close Setting button clicked");
        _settingPanel.SetActive(false);
    }

    /// <summary>
    /// 更新战斗信息文本，显示当前地下城和关卡信息
    /// TODO: 待修改 名称！
    /// </summary>
    public void RefreshView()
    {
        if (!GameContext.Instance.IsLoggedIn || GameContext.Instance.Dungeon == null)
        {
            Debug.LogWarning("DungeonCombatScene: Player is not logged in, cannot update combat info text");
            _infoText.text = "未登录 | 无地下城信息";
            return;
        }

        // 基础的名字
        var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.PlayerActorName);
        _infoText.text = $"{GameContext.Instance.Dungeon.name} | {stageName}";

        // 后缀添加回合数等战斗相关信息
        Combat currentCombat = GameUtils.GetLastCombat(GameContext.Instance.Dungeon);
        if (currentCombat != null)
        {
            var rounds = currentCombat.rounds != null ? currentCombat.rounds.Count : 0;
            _infoText.text += $" | 回合数: {rounds}";
        }
        else
        {
            _infoText.text += " | 无战斗数据";
            Debug.LogWarning("DungeonCombatScene: No combat data found for current dungeon");
        }
    }

}

