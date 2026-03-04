using UnityEngine;
using TMPro;

/// <summary>
/// 地牢战斗场景顶部UI控制器
/// </summary>
public class CombatTopBar : MonoBehaviour
{

    [Header("UI Components")]
    [SerializeField] private TMP_Text _infoText; // 战斗信息显示对象

   

    void Start()
    {
        // 断言检查，确保所有必要的组件和数据都已正确设置
        Debug.Assert(_infoText != null, "_infoText is null");
        // Debug.Assert(_settingPanel != null, "_settingPanel is null");

        // // 初始状态下只显示基本的地下城和关卡信息，战斗相关信息将在战斗状态更新时刷新
        // _settingPanel.SetActive(false); // 初始时隐藏设置面板

        // 暂时不做过多的逻辑处理，主要负责显示当前地下城和关卡信息
        if (GameContext.Instance.Dungeon != null)
        {
            var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.PlayerActorName);
            _infoText.text = $"{GameContext.Instance.Dungeon.name} | {stageName}";
        }
        else
        {
            _infoText.text = string.Empty;
        }
    }

    

    /// <summary>
    /// 设置顶部信息文本，通常包含当前地下城、关卡、回合数等信息
    /// </summary>
    /// <param name="info"></param>
    public void SetInfoText(string info)
    {
        _infoText.text = info;
    }

}

