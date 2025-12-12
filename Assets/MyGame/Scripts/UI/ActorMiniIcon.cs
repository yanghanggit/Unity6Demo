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
        if (_actorNameText != null)
        {
            _actorNameText.text = actorName;
        }

        // 设置角色头像
        if (_actorImage != null)
        {
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
    }

    /// <summary>
    /// 清空并隐藏图标
    /// </summary>
    public void Clear()
    {
        if (_actorNameText != null)
        {
            _actorNameText.text = string.Empty;
        }
        
        if (_actorImage != null)
        {
            _actorImage.sprite = null;
        }
        
        gameObject.SetActive(false);
    }
}
