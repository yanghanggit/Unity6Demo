using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActorPositioningObject : MonoBehaviour
{
    [Header("UI Components")]

    [SerializeField] private Image _image; // 角色图片显示对象
    [SerializeField] private TMP_Text _nameText; // 角色名称显示对象
    [SerializeField] private TMP_Text _statsText; // 角色属性显示对象

    public Button button; // 角色点击按钮

    private EntitySerialization _actorEntity; // 角色数据


    public EntitySerialization ActorEntity
    {
        get => _actorEntity;
        set
        {
            Debug.Assert(value != null, "ActorEntity cannot be set to null.");
            _actorEntity = value;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Assert(_image != null, "_image is not assigned in the inspector.");
        Debug.Assert(_nameText != null, "_nameText is not assigned in the inspector.");
        Debug.Assert(button != null, "button is not assigned in the inspector.");
        Debug.Assert(_statsText != null, "_statsText is not assigned in the inspector.");
    }

    public void RefreshView()
    {
        if (_actorEntity == null)
        {
            Debug.LogError("ActorEntity is not set. Cannot refresh view.");
            return;
        }

        //gameObject.GetComponentInChildren<TMP_Text>().text = _actorEntity.name; // 显示角色名称
        _nameText.text = GameUtils.GetDisplayName(_actorEntity.name); // 显示角色名称

        // 从1～5之间随机一个数字
        var randomHp = Random.Range(1, 6);
        _nameText.text += $" | {randomHp}"; // 显示当前HP

        var cachedSprite = SpriteCacheManager.Instance.GetSprite(_actorEntity.name);
        if (cachedSprite != null)
        {
            _image.sprite = cachedSprite;
        }
        else
        {
            Debug.LogWarning($"DungeonCombatScene: Background sprite not found for stage: {_actorEntity.name}");
            _image.sprite = null;
        }

        //
        var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(_actorEntity);
        Debug.Assert(combatStatsComponent != null, "CombatStatsComponent is missing from ActorEntity.");
        _statsText.text = $"{combatStatsComponent.stats.hp}/{combatStatsComponent.stats.max_hp}";
    }
}
