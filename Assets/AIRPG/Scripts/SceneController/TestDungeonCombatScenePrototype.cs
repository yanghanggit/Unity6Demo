using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using Mosframe;

public class TestDungeonCombatScenePrototype : MonoBehaviour, IUIEventListener
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText; // 主文本显示对象
    [SerializeField] private TMP_Text _combatInfoText; // 战斗信息显示对象
    [SerializeField] private ActorOrderSlot[] _actorSlots; // 角色槽位数组
    [SerializeField] private DynamicScrollView _scrollView;     // 动态滚动视图

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onCardClickedEvent; // 卡牌点击事件
    [SerializeField] private UIEventGameEvent _onActorSlotClickedEvent; // 角色槽位点击事件

    // Mock 数据 - 用于测试
    private EntitySerialization[] _mockActorData;
    private CardElementData[] _mockCardElementData;
    private CardBuildData _mockCardBuildData; // 用于测试的卡牌构建数据

    private void CreateMockActorData()
    {
        // 创建一些 mock 角色数据用于测试
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

    private void CreateMockCardElementData()
    {
        // 创建 mock 卡牌要素数据，包含3种类型
        // 顺序：状态效果 -> 技能 -> 目标角色
        _mockCardElementData = new CardElementData[6];

        // 1-2. 两个状态效果（Status Effect）
        _mockCardElementData[0] = new CardElementData(new StatusEffect
        {
            name = "增益.攻击强化",
            category = "增益",
            manifestation = "我感到力量充盈全身，攻击力大幅提升",
            effect = "+5点攻击力"
        });

        _mockCardElementData[1] = new CardElementData(new StatusEffect
        {
            name = "减益.灼烧",
            category = "减益",
            manifestation = "身体被火焰灼烧，持续受到伤害",
            effect = "-3点生命值/回合"
        });

        // 3. 一个技能（Skill）
        _mockCardElementData[2] = new CardElementData(new Skill
        {
            name = "技能.火球术",
            description = "释放一颗炽热的火球攻击敌人，造成火焰伤害"
        });

        // 4-6. 三个目标角色（Target Actor）- 复用 _mockActorData
        _mockCardElementData[3] = new CardElementData(_mockActorData[0]); // 角色.猎人.石坚
        _mockCardElementData[4] = new CardElementData(_mockActorData[1]); // 角色.术士.云音
        _mockCardElementData[5] = new CardElementData(_mockActorData[2]); // 角色.常物.野猪
    }

    void Awake()
    {
        // 创建 mock 数据
        CreateMockActorData();
        CreateMockCardElementData();
        _mockCardBuildData = new CardBuildData(); //先创建一个空的卡牌构建数据对象，后续可以根据需要填充数据

        //
        Debug.Assert(_scrollView != null, "ScrollView component is not assigned in the inspector.");
        _scrollView.totalItemCount = 0; // 设置滚动视图的总项数，测试动态加载
        _scrollView.gameObject.SetActive(false); // 确保滚动视图对象被激活，能正确显示
    }

    void Start()
    {
        Debug.Assert(_combatInfoText != null, "_combatInfoText is null");
        Debug.Assert(_mainText != null, "Main Text component is not assigned in the inspector.");
        Debug.Assert(_actorSlots != null && _actorSlots.Length > 0, "Actor slots are not assigned in the inspector.");
        Debug.Assert(_scrollView != null, "ScrollView component is not assigned in the inspector.");
        Debug.Assert(_onCardClickedEvent != null, "_onCardClickedEvent is null");
        Debug.Assert(_onActorSlotClickedEvent != null, "_onActorSlotClickedEvent is null");
        Debug.Assert(_mockActorData != null && _mockActorData.Length > 0, "Mock actor data is not initialized");
        Debug.Assert(_mockCardElementData != null && _mockCardElementData.Length > 0, "Mock card element data is not initialized");
        Debug.Assert(SpriteCacheManager.Instance != null, "SpriteCacheManager instance is null");

        // 注册事件监听器
        _onCardClickedEvent.RegisterListener(this);
        _onActorSlotClickedEvent.RegisterListener(this);

        // 设置初始文本内容
        _combatInfoText.text = "场景.测试地下城关卡-第1局";

        // 显示卡牌构建数据的初始状态
        _mainText.text = GameUtils.FormatCardBuildData(_mockCardBuildData); 

        // 将 mock 卡牌要素数据添加到 CardElementCollection
        CardElementCollection.Clear(); // 清空之前的数据
        foreach (var element in _mockCardElementData)
        {
            CardElementCollection.AddElement(element);
        }
        Debug.Log($"[TestDungeonCombatScenePrototype] 已添加 {CardElementCollection.Count} 个卡牌要素到 CardElementCollection");

        //
        _scrollView.totalItemCount = CardElementCollection.Count; // 设置滚动视图的总项数，测试动态加载
        _scrollView.gameObject.SetActive(true); // 确保滚动视图对象被激活，能正确显示

        // 设置 mock 数据到 ActorOrderSlot
        for (int i = 0; i < _actorSlots.Length && i < _mockActorData.Length; i++)
        {
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

        switch (eventData.eventType)
        {
            case UIEventType.CardElementScrollViewItemClick:
                Debug.Log($"处理卡牌要素滚动视图项点击事件，目标: {eventData.targetId}, 索引: {eventData.index}");
                break;

            case UIEventType.ActorOrderSlotClick:
                Debug.Log($"处理角色槽位点击事件，目标: {eventData.targetId}, 索引: {eventData.index}");
                _mockCardBuildData = new CardBuildData
                {
                    owner = new EntitySerialization { name = eventData.targetId },
                };
                UpdateMainTextWithCardBuildData();
                break;
        }

    }

    /// <summary>
    /// 更新主文本显示，展示当前卡牌构建数据的状态
    /// </summary>
    private void UpdateMainTextWithCardBuildData()
    {
        _mainText.text = GameUtils.FormatCardBuildData(_mockCardBuildData);
    }
}
