using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionOrderObject : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _actorImage; // 角色头像显示对象
    [SerializeField] private Button _button; // 用于点击的按钮组件
    [SerializeField] private TMP_Text _actorNameText; // 角色名字显示对象
    [SerializeField] private UIEventGameEvent _onActionOrderClickedEvent; // 角色槽位点击事件

    private EntitySerialization _actorEntitySerialization; // 角色实体的序列化数据

    void Start()
    {
        Debug.Assert(_actorImage != null, "_actorImage is null");
        Debug.Assert(_button != null, "_button is null");
        Debug.Assert(_actorNameText != null, "_actorNameText is null");
        Debug.Assert(_onActionOrderClickedEvent != null, "_onActionOrderClickedEvent is null");

        // 先移除listener，确保不会重复添加
        _button.onClick.RemoveListener(OnClick);
        _button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// 点击 Setting 按钮
    /// </summary>
    public void OnClick()
    {
        Debug.Log("ActionOrderObject button clicked" + gameObject.name);
        Debug.Assert(_actorEntitySerialization != null, "Actor entity serialization data is null");

        // 创建并发送结构化的事件数据
        var eventData = new UIEventData(
            UIEventType.ActionOrderClick, // 事件类型
            _actorEntitySerialization.name, // 传递角色名称
            -1,
            ""
        );

        // 触发事件，通知系统哪个角色槽位被点击了
        _onActionOrderClickedEvent.Raise(eventData);
    }

    /// <summary>
    /// 根据传入的角色实体序列化数据设置UI显示
    /// </summary>
    /// <param name="actorEntitySerialization"></param>
    public void SetData(EntitySerialization actorEntitySerialization)
    {
        // 根据传入的角色实体序列化数据设置UI显示
        _actorEntitySerialization = actorEntitySerialization;
    }

    public void RefreshUI()
    {
        Debug.Assert(_actorEntitySerialization != null, "Actor entity serialization data is null");
        // 根据 _actorEntitySerialization 中的数据刷新UI显示
        // 例如，如果有头像组件，可以从组件数据中获取头像资源并设置到 _actorImage 上
        // 根据实体数据设置UI显示，例如角色名字和头像
        string displayName = GameUtils.GetDisplayName(_actorEntitySerialization.name);
        _actorNameText.text = displayName;

        // 尝试从 CombatStatsComponent 获取角色的HP信息并显示在名字旁边
        var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(_actorEntitySerialization);
        if (combatStatsComponent != null)
        {
            _actorNameText.text = $"{displayName}\n{combatStatsComponent.stats.hp}/{combatStatsComponent.stats.max_hp}";
        }

        // 从 SpriteCacheManager 获取角色头像并设置到 _actorImage 上
        var stageSprite = SpriteCacheManager.Instance.GetSprite(_actorEntitySerialization.name);
        if (stageSprite != null)
        {
            _actorImage.sprite = stageSprite;
        }
        else
        {
            Debug.LogWarning("Stage sprite not found for: " + _actorEntitySerialization.name);
        }
    }
}
