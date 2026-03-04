using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

/// <summary>
/// 角色迷你图标组件
/// 用于显示角色的小型图标,包括角色头像和名称
/// </summary>
public class ActorIcon : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _image;          // 角色头像图片
    [SerializeField] private TMP_Text _nameText;    // 角色名称文本

    public string ActorName
    {
        get; set;
    }

    void Start()
    {
        Debug.Assert(_image != null, "_image is null");
        Debug.Assert(_nameText != null, "_nameText is null");

        //
        RefreshView().Forget();
    }

    /// <summary>
    /// 刷新角色迷你图标的UI显示
    /// 从GameContext获取角色最新数据，更新名称、头像和死亡状态的显示
    /// </summary>
    private async UniTaskVoid RefreshView()
    {

        // 设置角色名称
        _nameText.text = GameUtils.GetDisplayName(ActorName);

        // 设置角色头像
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(ActorName);
        if (cachedSprite != null)
        {
            _image.sprite = cachedSprite;
        }
        else
        {
            Debug.LogWarning($"Actor sprite not found for: {ActorName}");
            _image.sprite = null; // 或者设置为一个默认的占位图
        }
    }
}
