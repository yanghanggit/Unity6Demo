using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActorOrderSlot : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _actorImage; // 角色头像显示对象
    [SerializeField] private Button _overlayButton; // 用于点击的按钮组件
    [SerializeField] private TMP_Text _actorNameText; // 角色名字显示对象
    [SerializeField] private UIEventGameEvent _onActorSlotClickedEvent; // 角色槽位点击事件

    private EntitySerialization _actorEntitySerialization; // 角色实体的序列化数据

    void Start()
    {
        Debug.Assert(_actorImage != null, "_actorImage is null");
        Debug.Assert(_overlayButton != null, "_overlayButton is null");
        Debug.Assert(_actorNameText != null, "_actorNameText is null");
        Debug.Assert(_onActorSlotClickedEvent != null, "_onActorSlotClickedEvent is null");
    
        // 先移除listener，确保不会重复添加
        _overlayButton.onClick.RemoveListener(OnClick);
        _overlayButton.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// 点击 Setting 按钮
    /// </summary>
    public void OnClick()
    {
        Debug.Log("ActorOrderSlot button clicked" + gameObject.name);
        // 创建并发送结构化的事件数据
        var eventData = new UIEventData(
            UIEventType.CombatSceneUI_ActorSlotClick, // 事件类型
            _actorNameText.text,
            -1,
            gameObject.name
        );
        _onActorSlotClickedEvent.Raise(eventData);
    }

    public void SetActorData(EntitySerialization actorEntitySerialization)
    {
        // 根据传入的角色实体序列化数据设置UI显示
        _actorEntitySerialization = actorEntitySerialization;

        // 从实体数据中提取显示名称并设置到UI
        RefreshUI();
    }

    private void RefreshUI()
    {
        Debug.Assert(_actorEntitySerialization != null, "Actor entity serialization data is null");
        // 根据 _actorEntitySerialization 中的数据刷新UI显示
        // 例如，如果有头像组件，可以从组件数据中获取头像资源并设置到 _actorImage 上
        // 根据实体数据设置UI显示，例如角色名字和头像
        string displayName = GameUtils.GetDisplayName(_actorEntitySerialization.name);
        _actorNameText.text = displayName;

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
