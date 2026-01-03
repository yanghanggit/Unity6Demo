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
        var imageDisplayController = _headIconButton.GetComponent<ActorPortraitController>();
        if (imageDisplayController == null)
        {
            imageDisplayController = _headIconButton.gameObject.AddComponent<ActorPortraitController>();
        }


        // 设置 ActorName，解耦对 GameContext 的直接依赖
        imageDisplayController.ActorName = actorName;

        // 设置 PortraitPrompt
        var actorEntitySerialization = GameContext.Instance.GetActorEntitySerialization(actorName);
        var appearanceComponent = GameUtils.GetComponent<AppearanceComponent>(actorEntitySerialization);
        Debug.Assert(appearanceComponent != null, "[ImageDisplayController] AppearanceComponent is null for player actor: " + actorEntitySerialization.name);
        var portraitPrompt = ImageService.WrapActorPortraitPromptForGeneration(appearanceComponent.name, appearanceComponent.appearance);
        imageDisplayController.Prompt = portraitPrompt;

        // 设置 ImageUrlStorageKey 和 SpriteCacheKey, 先简单一些，使用相同的key
        var imageUrlStorageKey = ImageService.GenerateImageUrlStorageKey(
                GameContext.Instance.UserName,
                GameContext.Instance.GameName,
                actorName,
                portraitPrompt
            );

        imageDisplayController.ImageUrlStorageKey = imageUrlStorageKey;
        imageDisplayController.SpriteCacheKey = imageUrlStorageKey;
    }

    public void OnHeadIconClicked()
    {
        Debug.Log("Head icon clicked!");
        OnHeadIconClickedCallback?.Invoke();
    }
}


