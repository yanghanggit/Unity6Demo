using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionOrderObject : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _image; // 角色头像显示对象
    [SerializeField] private TMP_Text _nameText; // 角色名字显示对象

    void Start()
    {
        Debug.Assert(_image != null, "_image is null");
        Debug.Assert(_nameText != null, "_nameText is null");
    }

    /// <summary>
    /// 根据当前设置的角色实体数据，刷新UI显示
    /// </summary>
    public void RefreshUI(EntitySerialization actorEntity)
    {
        Debug.Assert(actorEntity != null, "Actor entity serialization data is null");

        // 根据角色名称获取对应的显示名称，并设置到UI组件上
        string displayName = GameUtils.GetDisplayName(actorEntity.name);
        _nameText.text = displayName;

        // 根据角色名称从缓存中获取对应的头像资源，并设置到UI组件上
        var stageSprite = SpriteCacheManager.Instance.GetSprite(actorEntity.name);
        if (stageSprite != null)
        {
            _image.sprite = stageSprite;
        }
        else
        {
            Debug.LogWarning("Stage sprite not found for: " + actorEntity.name);
        }
    }
}
