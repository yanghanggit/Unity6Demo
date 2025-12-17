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

    void Start()
    {
        Debug.Assert(_actorImage != null, "_actorImage is null");
        Debug.Assert(_actorNameText != null, "_actorNameText is null");
        Debug.Assert(_deathOverlay != null, "_deathOverlay is null");
    }

    /// <summary>
    /// 设置角色图标的显示内容
    /// </summary>
    /// <param name="actorName">角色名称</param>
    public void SetActor(string actorName)
    {
        if (string.IsNullOrEmpty(actorName))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // 设置角色名称
        _actorNameText.text = actorName;

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
    }

    /// <summary>
    /// 清空并隐藏图标
    /// </summary>
    public void Clear()
    {
        _actorNameText.text = string.Empty;
        _actorImage.sprite = null;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 设置角色的死亡状态显示
    /// </summary>
    /// <param name="isDead">是否死亡</param>
    public void SetDeathState(bool isDead)
    {
        _deathOverlay.SetActive(isDead);

        // 可选：死亡时将角色图片变灰
        _actorImage.color = isDead ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;

        // 可选：死亡时将名字变红
        _actorNameText.color = isDead ? Color.red : Color.black;
    }
}
