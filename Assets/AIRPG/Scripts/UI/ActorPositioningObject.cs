//using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActorPositioningObject : MonoBehaviour
{
    [Header("UI Components")]

    [SerializeField] private Image _image; // 角色图片显示对象
    [SerializeField] private TMP_Text _nameText; // 角色名称显示对象
    [SerializeField] private TMP_Text _statsText; // 角色属性显示对象
    [SerializeField] private Button _button; // 角色点击按钮

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onActorPositioningClickedEvent;

    /// 角色数据缓存字段，避免每次点击时都需要从UI组件中获取数据，提升性能和代码清晰度
    private EntitySerialization _actorEntity; // 角色数据

    /// <summary>
    /// 获取或设置缓存的角色数据
    /// </summary>
    public EntitySerialization ActorEntity
    {
        get => _actorEntity;
        set
        {
            Debug.Assert(value != null, "ActorEntity cannot be set to null.");
            _actorEntity = value;
        }
    }

    void Start()
    {
        Debug.Assert(_image != null, "_image is not assigned in the inspector.");
        Debug.Assert(_nameText != null, "_nameText is not assigned in the inspector.");
        Debug.Assert(_button != null, "button is not assigned in the inspector.");
        Debug.Assert(_statsText != null, "_statsText is not assigned in the inspector.");
        Debug.Assert(_onActorPositioningClickedEvent != null, "_onActorPositioningClickedEvent is not assigned in the inspector.");
    }

    public void OnClick()
    {
        if (_actorEntity == null)
        {
            Debug.LogError("ActorEntity is not set. Cannot raise event.");
            return;
        }

        var eventData = new UIEventData(
            UIEventType.ActorPositioningClicked,
            _actorEntity.name
        );

        // 触发事件，通知系统哪个卡牌要素被点击了
        Debug.Assert(_onActorPositioningClickedEvent != null, "_onCardElementClickedEvent is null");
        _onActorPositioningClickedEvent.Raise(eventData);
    }

    public void RefreshView()
    {
        if (_actorEntity == null)
        {
            Debug.LogError("ActorEntity is not set. Cannot refresh view.");
            return;
        }

        _nameText.text = GameUtils.GetDisplayName(_actorEntity.name); // 显示角色名称

        //
        _nameText.text += $" | 1"; // 显示角色名称和行动顺序（从1开始）

        //
        var handComponent = GameUtils.GetComponent<HandComponent>(_actorEntity);
        if (handComponent != null)
        {
            _nameText.text += $" | 手牌"; // 显示角色名称和行动顺序（从1开始）和手牌数
        }

        // 尝试从缓存中获取角色图片，如果没有找到则显示默认图片或保持不变
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

        // 显示角色属性信息，目前仅显示生命值，后续可以根据需要添加更多属性显示
        var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(_actorEntity);
        Debug.Assert(combatStatsComponent != null, "CombatStatsComponent is missing from ActorEntity.");
        _statsText.text = $"{combatStatsComponent.stats.hp}/{combatStatsComponent.stats.max_hp}";
    }
}
