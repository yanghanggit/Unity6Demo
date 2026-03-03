using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionOrderObject : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _image; // 角色头像显示对象
    [SerializeField] private TMP_Text _nameText; // 角色名字显示对象

    private EntitySerialization _actorEntity; // 角色实体的序列化数据

    public EntitySerialization ActorEntity
    {
        get => _actorEntity;
        set
        {
            _actorEntity = value;
            RefreshUI();
        }
    }

    void Start()
    {
        Debug.Assert(_image != null, "_image is null");
        Debug.Assert(_nameText != null, "_nameText is null");
    }

    /// <summary>
    /// 根据当前设置的角色实体数据，刷新UI显示
    /// </summary>
    private void RefreshUI()
    {
        Debug.Assert(_actorEntity != null, "Actor entity serialization data is null");

        // 根据角色名称获取对应的显示名称，并设置到UI组件上
        string displayName = GameUtils.GetDisplayName(_actorEntity.name);
        _nameText.text = displayName;

        // 根据角色名称从缓存中获取对应的头像资源，并设置到UI组件上
        var stageSprite = SpriteCacheManager.Instance.GetSprite(_actorEntity.name);
        if (stageSprite != null)
        {
            _image.sprite = stageSprite;
        }
        else
        {
            Debug.LogWarning("Stage sprite not found for: " + _actorEntity.name);
        }
    }
}
