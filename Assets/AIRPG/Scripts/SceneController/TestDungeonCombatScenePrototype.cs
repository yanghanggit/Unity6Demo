using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class TestDungeonCombatScenePrototype : MonoBehaviour, IUIEventListener
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText; // 主文本显示对象
    [SerializeField] private TMP_Text _combatInfoText; // 战斗信息显示对象
    [SerializeField] private ActorOrderSlot[] _actorSlots; // 角色槽位数组

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onCardClickedEvent; // 卡牌点击事件
    [SerializeField] private UIEventGameEvent _onActorSlotClickedEvent; // 角色槽位点击事件

    // Mock 数据 - 用于测试
    private EntitySerialization[] _mockActorData;

    void Awake()
    {
        // 初始化 mock 数据
        _mockActorData = new EntitySerialization[5];

        // 1. 角色.猎人.石坚
        _mockActorData[0] = new EntitySerialization
        {
            name = "角色.猎人.石坚",
            components = new List<ComponentSerialization>()
        };

        // 2. 角色.术士.云音
        _mockActorData[1] = new EntitySerialization
        {
            name = "角色.术士.云音",
            components = new List<ComponentSerialization>()
        };

        // 3. 角色.常物.野猪
        _mockActorData[2] = new EntitySerialization
        {
            name = "角色.常物.野猪",
            components = new List<ComponentSerialization>()
        };

        // 4. 角色.精怪.山魈
        _mockActorData[3] = new EntitySerialization
        {
            name = "角色.精怪.山魈",
            components = new List<ComponentSerialization>()
        };

        // 5. 角色.大妖.山中虎
        _mockActorData[4] = new EntitySerialization
        {
            name = "角色.大妖.山中虎",
            components = new List<ComponentSerialization>()
        };
    }

    void Start()
    {
        Debug.Assert(_combatInfoText != null, "_combatInfoText is null");
        Debug.Assert(_mainText != null, "Main Text component is not assigned in the inspector.");
        Debug.Assert(_actorSlots != null && _actorSlots.Length > 0, "Actor slots are not assigned in the inspector.");
        Debug.Assert(_onCardClickedEvent != null, "_onCardClickedEvent is null");
        Debug.Assert(_onActorSlotClickedEvent != null, "_onActorSlotClickedEvent is null");
        Debug.Assert(_mockActorData != null && _mockActorData.Length > 0, "Mock actor data is not initialized");
        Debug.Assert(SpriteCacheManager.Instance != null, "SpriteCacheManager instance is null");


        // 设置初始文本内容
        _combatInfoText.text = "场景.测试地下城关卡-第1局";
        _mainText.text = "这是一个测试";

        // 注册事件监听器
        _onCardClickedEvent.RegisterListener(this);
        _onActorSlotClickedEvent.RegisterListener(this);

        // 设置 mock 数据到 ActorOrderSlot
        for (int i = 0; i < _actorSlots.Length && i < _mockActorData.Length; i++)
        {
            // Debug.Assert(_actorSlots[i].GetComponent<Image>().sprite == null, $"Actor slot at index {i} already has a sprite assigned in the inspector, which may interfere with testing");
            // _actorSlots[i].GetComponent<Image>().sprite = null; // 初始时没有头像显示，后续根据数据设置
            _actorSlots[i].SetActorData(_mockActorData[i]);
        }
    }

    void OnDestroy()
    {
        // 确保在对象销毁时取消注册事件监听器，避免内存泄漏或错误调用
        if (_onCardClickedEvent != null)
        {
            _onCardClickedEvent.UnregisterListener(this);
        }

        if (_onActorSlotClickedEvent != null)
        {
            _onActorSlotClickedEvent.UnregisterListener(this);
        }
    }

    /// <summary>
    /// 点击 Setting 按钮
    /// </summary>
    public void OnClickSetting()
    {
        Debug.Log("Setting button clicked");
    }

    /// <summary>
    /// 点击 Info 按钮
    /// </summary>
    public void OnClickInfo()
    {
        Debug.Log("Info button clicked");
        // TODO: 显示信息面板
    }

    /// <summary>
    /// 点击 Run 按钮
    /// </summary>
    public void OnClickRun()
    {
        Debug.Log("Run button clicked");
        // TODO: 执行逃跑操作
    }

    /// <summary>
    /// IUIEventListener 接口实现
    /// 处理所有UI事件的统一入口
    /// </summary>
    public void OnEventRaised(UIEventData eventData)
    {
        Debug.Log($"[TestDungeonCombatScenePrototype] {eventData}");
        Debug.Log($"OnEventRaised: {eventData.eventType}, TargetId: {eventData.targetId}, Index: {eventData.index}, ExtraData: {eventData.extraData}");
        _mainText.text = $"事件: {eventData.eventType}\n目标: {eventData.targetId}\n索引: {eventData.index}\n额外: {eventData.extraData}";
    }
}
