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

    private EntitySerialization _cachedActorEntity; // 角色数据

    /// <summary>
    /// 获取或设置缓存的角色数据
    /// </summary>
    public EntitySerialization CachedActorEntity
    {
        get => _cachedActorEntity;
        set
        {
            Debug.Assert(value != null, "ActorEntity cannot be set to null.");
            _cachedActorEntity = value;
        }
    }


    /// <summary>
    /// 获取当前角色的最新数据，确保显示的信息是最新的。由于角色数据可能会在战斗过程中发生变化（例如生命值、状态等），因此每次刷新界面时都应该获取最新的数据来更新显示。
    /// </summary>
    public EntitySerialization UpdatedActorEntity
    {
        get
        {
            if (_cachedActorEntity == null)
            {
                Debug.LogError("ActorEntity is not set. Cannot retrieve UpdatedActorEntity.");
                return null;
            }

            // 从游戏上下文中获取当前角色的最新数据，如果没有找到则返回缓存的数据并输出警告信息
            var updatedEntity = GameContext.Instance.GetActorEntity(_cachedActorEntity.name);
            if (updatedEntity == null)
            {
                Debug.LogWarning($"Updated actor entity not found for name: {_cachedActorEntity.name}. Returning original ActorEntity.");
                return _cachedActorEntity;
            }

            // 返回最新的角色数据
            return updatedEntity;
        }
    }

    public int ActionOrderIndex
    {
        get
        {
            var lastRound = GameUtils.GetLastRound(GameContext.Instance.Dungeon);
            if (lastRound == null)
            {
                Debug.LogWarning("No rounds found in current dungeon. Defaulting action order to 0.");
                return 0;
            }

            var actionOrder = lastRound.action_order;
            if (actionOrder == null || actionOrder.Count == 0)
            {
                Debug.LogWarning("Action order is empty in the last round. Defaulting action order to 0.");
                return 0;
            }

            return actionOrder.IndexOf(_cachedActorEntity.name);
        }
    }

    void Start()
    {
        Debug.Assert(_image != null, "_image is not assigned in the inspector.");
        Debug.Assert(_nameText != null, "_nameText is not assigned in the inspector.");
        Debug.Assert(button != null, "button is not assigned in the inspector.");
        Debug.Assert(_statsText != null, "_statsText is not assigned in the inspector.");
    }

    public void RefreshView()
    {
        if (_cachedActorEntity == null)
        {
            Debug.LogError("ActorEntity is not set. Cannot refresh view.");
            return;
        }

        _nameText.text = GameUtils.GetDisplayName(_cachedActorEntity.name); // 显示角色名称

        //
        _nameText.text += $" | {ActionOrderIndex + 1}"; // 显示角色名称和行动顺序（从1开始）

        //
        var handComponent = GameUtils.GetComponent<HandComponent>(UpdatedActorEntity);
        if (handComponent != null)
        {
            _nameText.text += $" | 手牌"; // 显示角色名称和行动顺序（从1开始）和手牌数
        }

        // 尝试从缓存中获取角色图片，如果没有找到则显示默认图片或保持不变
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(_cachedActorEntity.name);
        if (cachedSprite != null)
        {
            _image.sprite = cachedSprite;
        }
        else
        {
            Debug.LogWarning($"DungeonCombatScene: Background sprite not found for stage: {_cachedActorEntity.name}");
            _image.sprite = null;
        }

        // 显示角色属性信息，目前仅显示生命值，后续可以根据需要添加更多属性显示
        var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(UpdatedActorEntity);
        Debug.Assert(combatStatsComponent != null, "CombatStatsComponent is missing from ActorEntity.");
        _statsText.text = $"{combatStatsComponent.stats.hp}/{combatStatsComponent.stats.max_hp}";
    }
}
