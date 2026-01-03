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

        // 给 _headIconButton 添加 ImageDisplayController 组件（如果还没有的话）
        var imageDisplayController = _headIconButton.GetComponent<ImageDisplayController>();
        if (imageDisplayController == null)
        {
            imageDisplayController = _headIconButton.gameObject.AddComponent<ImageDisplayController>();
        }
        // 设置 ActorName，解耦对 GameContext 的直接依赖
        imageDisplayController.ActorName = actorName;
    }

    public void OnHeadIconClicked()
    {
        Debug.Log("Head icon clicked!");
        OnHeadIconClickedCallback?.Invoke();
    }
}


