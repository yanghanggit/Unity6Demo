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

    void Start()
    {
        Debug.Assert(_actorImage != null, "_actorImage is null");
        Debug.Assert(_overlayButton != null, "_overlayButton is null");
        Debug.Assert(_actorNameText != null, "_actorNameText is null");
        Debug.Assert(_onActorSlotClickedEvent != null, "_onActorSlotClickedEvent is null");
        // 获取角色实体序列化数据
        // 先移除listener，确保不会重复添加
        _overlayButton.onClick.RemoveListener(OnClick);
        _overlayButton.onClick.AddListener(OnClick);

        _actorNameText.text = gameObject.name; // 默认显示对象名称，后续可以根据实际数据设置
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
}
