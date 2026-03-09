using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PlayerTopBar : MonoBehaviour
{

    [Header("UI Components")]
    [SerializeField] private Button _iconButton;
    [SerializeField] private TMP_Text _infoText;

    void Start()
    {
        Debug.Assert(_iconButton != null, "_headIconButton is null");
        Debug.Assert(_infoText != null, "_playerInfoText is null");
        Debug.Assert(SpriteCacheManager.Instance != null, "SpriteCacheManager instance is null");

        if (!GameContext.Instance.IsLoggedIn)
        {
            _infoText.text = $"{MockData.MockUserName}\n{GameUtils.GetDisplayName(MockData.MockActorName)}";
            // 更新角色头像显示
            var cachedSprite = SpriteCacheManager.Instance.GetSprite(MockData.MockActorName);
            if (cachedSprite != null)
            {
                _iconButton.GetComponent<Image>().sprite = cachedSprite;
            }
            else
            {
                Debug.LogWarning($"DungeonCombatScene: Background sprite not found for stage: {MockData.MockActorName}");
                _iconButton.GetComponent<Image>().sprite = null;
            }
        }
        else
        {
            RefreshView();
        }
    }

    private void RefreshView()
    {
        // 显示玩家文字信息
        _infoText.text = $"{GameContext.Instance.UserName}\n{GameUtils.GetDisplayName(GameContext.Instance.PlayerActorName)}";

        // 显示头像
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(GameContext.Instance.PlayerActorName);
        Debug.Assert(cachedSprite != null, "Cached sprite is null for actor: " + GameContext.Instance.PlayerActorName);
        if (cachedSprite != null)
        {
            // 直接使用缓存的头像
            _iconButton.GetComponent<Image>().sprite = cachedSprite;
        }
        else
        {
            Debug.LogWarning($"Player sprite not found for: {GameContext.Instance.PlayerActorName}");
            _iconButton.GetComponent<Image>().sprite = null; // 或者设置为一个默认的占位图
        }
    }
}


