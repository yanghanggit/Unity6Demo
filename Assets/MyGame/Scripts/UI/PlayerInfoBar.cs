using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerInfoBar : MonoBehaviour
{

    [Header("UI Components")]
    [SerializeField] private Button _headIconButton;
    [SerializeField] private TMP_Text _playerInfoText;

    // 头像点击回调
    public event Action OnHeadIconClickedCallback;

    void Start()
    {
        Debug.Assert(_headIconButton != null, "_headIconButton is null");
        Debug.Assert(_playerInfoText != null, "_playerInfoText is null");

        // 先清除！
        _playerInfoText.text = "";

        // 后刷新！
        RefreshPlayerInfo();
    }

    private void RefreshPlayerInfo()
    {
        // 设置图片
        if (ApiEndpointsManager.ImageRootResponse == null)
        {
            var playerActor = GameContext.Instance.GetActorEntitySerialization(GameContext.Instance.ActorName);
            Debug.Assert(playerActor != null, "Player actor entity serialization is null for actor name: " + GameContext.Instance.ActorName);
            if (playerActor != null)
            {
                var actorSprite = SpriteManager.Instance.GetSprite(playerActor.name);
                Debug.Assert(actorSprite != null, "Player actor sprite is null for entity: " + playerActor.name);
                var buttonImage = _headIconButton.GetComponent<Image>();
                buttonImage.sprite = actorSprite;
            }
        }

        // 设置文本
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


