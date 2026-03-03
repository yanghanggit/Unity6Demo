using TMPro;
using UnityEngine;

public class CombatPostCombatState : MonoBehaviour, ICombatState
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText; // 战斗后面板文本对象

    public ICombatScene CombatScene { get; set; }

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
    }

    /// <summary>
    /// 点击继续按钮的处理逻辑，根据新的战斗状态切换 UI 显示和交互逻辑
    /// </summary>
    public void OnClickAdvanceButton()
    {
        Debug.Log("Advance Button Clicked");
        // 在这里添加点击继续按钮后的逻辑，例如返回主界面或进入下一个关卡
    }
}
