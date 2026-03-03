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
        _playerInfoText.text = $"{GameContext.Instance.UserName}\n{GameUtils.GetDisplayName(GameContext.Instance.PlayerActorName)}";

        // 获取角色实体序列化数据
        var actorEntitySerialization = GameContext.Instance.GetActorEntity(GameContext.Instance.PlayerActorName);
        Debug.Assert(actorEntitySerialization != null, "Actor entity serialization is null for actor: " + GameContext.Instance.PlayerActorName);

        // 默认的显示逻辑
        // 显示头像
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(GameContext.Instance.PlayerActorName);
        Debug.Assert(cachedSprite != null, "Cached sprite is null for actor: " + GameContext.Instance.PlayerActorName);
        if (cachedSprite != null)
        {
            // 直接使用缓存的头像
            _headIconButton.GetComponent<Image>().sprite = cachedSprite;
        }
    }

    public void OnHeadIconClicked()
    {
        Debug.Log("Head icon clicked!");
        OnHeadIconClickedCallback?.Invoke();
    }
}


