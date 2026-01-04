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

        // 显示玩家文字信息
        _playerInfoText.text = $"{GameContext.Instance.UserName}\n{GameUtils.GetDisplayName(GameContext.Instance.ActorName)}";

        // 获取角色实体序列化数据
        var actorEntitySerialization = GameContext.Instance.GetActorEntitySerialization(GameContext.Instance.ActorName);
        Debug.Assert(actorEntitySerialization != null, "Actor entity serialization is null for actor: " + GameContext.Instance.ActorName);

        // 默认的显示逻辑
        // 显示头像
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(GameContext.Instance.ActorName + "_头像");
        //cachedSprite = null; // 强制测试头像生成逻辑
        if (cachedSprite != null)
        {
            // 直接使用缓存的头像
            _headIconButton.GetComponent<Image>().sprite = cachedSprite;
        }
        else
        {
            // 初始化头像控制器
            InitializeActorPortraitController(actorEntitySerialization);
        }
    }

    /// <summary>
    /// 初始化并配置 ActorPortraitController 组件
    /// </summary>
    /// <param name="actorEntitySerialization">角色实体序列化数据</param>
    private void InitializeActorPortraitController(EntitySerialization actorEntitySerialization)
    {
        // 给 _headIconButton 添加 ImageDisplayController 组件（如果还没有的话）
        var imageDisplayController = _headIconButton.GetComponent<ActorPortraitController>();
        if (imageDisplayController == null)
        {
            imageDisplayController = _headIconButton.gameObject.AddComponent<ActorPortraitController>();
        }

        // 设置 ActorName，解耦对 GameContext 的直接依赖
        imageDisplayController.ActorName = GameContext.Instance.ActorName;

        // 设置 PortraitPrompt
        var appearanceComponent = GameUtils.GetComponent<AppearanceComponent>(actorEntitySerialization);
        Debug.Assert(appearanceComponent != null, "[ImageDisplayController] AppearanceComponent is null for player actor: " + actorEntitySerialization.name);
        var portraitPrompt = ImageService.WrapActorPortraitPromptForGeneration(appearanceComponent.name, appearanceComponent.appearance);
        imageDisplayController.Prompt = portraitPrompt;

        // 设置 ImageUrlStorageKey 和 SpriteCacheKey, 先简单一些，使用相同的key
        var imageUrlStorageKey = ImageService.GenerateImageUrlStorageKey(
                GameContext.Instance.UserName,
                GameContext.Instance.GameName,
                GameContext.Instance.ActorName,
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


