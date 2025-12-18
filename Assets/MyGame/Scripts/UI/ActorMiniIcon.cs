using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 角色迷你图标组件
/// 用于显示角色的小型图标,包括角色头像和名称
/// </summary>
public class ActorMiniIcon : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _actorImage;          // 角色头像图片
    [SerializeField] private TMP_Text _actorNameText;    // 角色名称文本
    [SerializeField] private GameObject _deathOverlay;    // 死亡覆盖标记（例如骷髅图标或X标记）
    
    [Header("Settings")]
    [SerializeField] private bool enableDeathDetection = false;  // 是否启用死亡检测（在需要的场景中打开）

    private string _actorName;  // 当前绑定的角色名称

    /// <summary>
    /// 获取当前绑定的角色名称
    /// </summary>
    public string ActorName => _actorName;

    void Start()
    {
        Debug.Assert(_actorImage != null, "_actorImage is null");
        Debug.Assert(_actorNameText != null, "_actorNameText is null");
        // _deathOverlay is optional
    }

    /// <summary>
    /// 绑定角色图标的显示内容
    /// </summary>
    /// <param name="actorName">角色名称</param>
    public void BindActor(string actorName)
    {
        if (string.IsNullOrEmpty(actorName))
        {
            Debug.LogWarning("ActorMiniIcon: Cannot bind actor with empty name");
            return;
        }

        _actorName = actorName;

        // 设置角色名称
        _actorNameText.text = GameUtils.GetDisplayName(actorName);

        // 设置角色头像
        // 优先尝试加载头像素材（键值格式：角色名_头像）
        var avatarSprite = TextureManager.Instance.GetSprite(actorName + "_头像");
        
        // 如果没有头像素材，降级使用全身图
        if (avatarSprite == null)
        {
            avatarSprite = TextureManager.Instance.GetSprite(actorName);
        }

        if (avatarSprite != null)
        {
            _actorImage.sprite = avatarSprite;
        }
        else
        {
            Debug.LogWarning($"Actor sprite not found for: {actorName}");
        }

        // 检测并设置死亡状态（仅在启用死亡检测时）
        if (enableDeathDetection)
        {
            bool isDead = false;
            var actorEntity = GameContext.Instance.GetActorEntitySerialization(actorName);
            if (actorEntity != null)
            {
                var deathComponent = GameUtils.GetComponent<DeathComponent>(actorEntity);
                isDead = deathComponent != null;
            }

            // 应用死亡状态UI效果
            if (_deathOverlay != null)
            {
                _deathOverlay.SetActive(isDead);
            }
            _actorImage.color = isDead ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
            _actorNameText.color = isDead ? Color.red : Color.black;
        }
    }
}
