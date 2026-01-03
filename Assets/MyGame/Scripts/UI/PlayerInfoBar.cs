using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerInfoBar : MonoBehaviour
{

    [Header("UI Components")]
    [SerializeField] private Button _headIconButton;
    [SerializeField] private TMP_Text _playerInfoText;
    public event Action OnHeadIconClickedCallback;

    void Start()
    {
        Debug.Assert(_headIconButton != null, "_headIconButton is null");
        Debug.Assert(_playerInfoText != null, "_playerInfoText is null");

        // 显示玩家信息
        var playerName = GameContext.Instance.UserName;
        var actorName = GameContext.Instance.ActorName;
        _playerInfoText.text = $"{playerName}\n{GameUtils.GetDisplayName(actorName)}";
    }

    public void OnHeadIconClicked()
    {
        Debug.Log("Head icon clicked!");
        OnHeadIconClickedCallback?.Invoke();
    }
}


