using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ArbitrationPanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _backgroundImage; // 背景图片对象
    [SerializeField] private TMP_Text _arbitrationText; // 仲裁面板文本对象

    private Round _lastRound;// 用于存储最新的回合信息

    public Round LastRound
    {
        get => _lastRound;
        set
        {
            _lastRound = value;
            if (!GameContext.Instance.IsLoggedIn)
            {
                _lastRound = new Round
                {
                    action_order = new List<string> { "Hero", "Goblin", "Mage" },
                    combat_log = "Hero attacks Goblin for 30 damage. Goblin is defeated. Mage casts Fireball on Hero for 20 damage.",
                    narrative = "The battle begins! The Hero strikes first, taking down the Goblin. The Mage retaliates with a fiery spell, scorching the Hero."
                };
            }

            Debug.Assert(_lastRound != null, "Round data is null in UpdateState");
            var formattedRoundInfo = GameUtils.FormatRoundInfo(_lastRound);
            _arbitrationText.text = formattedRoundInfo;
        }
    }

    void Start()
    {
        Debug.Assert(_backgroundImage != null, "_backgroundImage is null");
        Debug.Assert(_arbitrationText != null, "_arbitrationText is null");
    }

    public void OnClickCloseButton()
    {
        Debug.Log("Close Arbitration Panel Button Clicked");
        gameObject.SetActive(false);
    }
}
