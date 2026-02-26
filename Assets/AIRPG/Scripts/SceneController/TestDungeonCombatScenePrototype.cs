using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Mosframe;
using Newtonsoft.Json;

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
    [SerializeField] private UIEventGameEvent _onCardBuilderDataChangedEvent; // CardBuilder 数据变化事件

    // Mock 数据 - 用于测试
    private EntitySerialization[] _mockActorData;

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

        var skillBook0 = new SkillBookComponent
        {
            name = "SkillBookComponent",
            skills = new List<Skill>
            {
                new() { name = "技能.重击", description = "全力挥击武器，造成高额伤害" },
                new() { name = "技能.铁壁", description = "提升防御力，减少受到的伤害" },
                new() { name = "技能.猎人标记", description = "标记目标，增加后续攻击伤害" }
            }
        };
        _mockActorData[0].components.Add(new ComponentSerialization
        {
            name = "SkillBookComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(skillBook0))
        });

        var combatStats0 = new CombatStatsComponent
        {
            name = "CombatStatsComponent",
            stats = new CharacterStats
            {
                hp = 100,
                max_hp = 100,
                attack = 15,
                defense = 10
            },
            status_effects = new List<StatusEffect>
            {
                new() { name = "增益.战意高昂", category = "增益", manifestation = "斗志昂扬，战意盎然", effect = "攻击力+2" },
                new() { name = "增益.坚韧体质", category = "增益", manifestation = "体质强健，不易受伤", effect = "防御力+3" },
                new() { name = "增益.猎人本能", category = "增益", manifestation = "敏锐的本能感知", effect = "命中率+10%" }
            }
        };
        _mockActorData[0].components.Add(new ComponentSerialization
        {
            name = "CombatStatsComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(combatStats0))
        });

        // 2. 角色.术士.云音
        _mockActorData[1] = new EntitySerialization
        {
            name = "角色.术士.云音",
            components = new List<ComponentSerialization>()
        };

        var skillBook1 = new SkillBookComponent
        {
            name = "SkillBookComponent",
            skills = new List<Skill>
            {
                new() { name = "技能.火球术", description = "释放炽热火球，造成火焰伤害" },
                new() { name = "技能.冰封术", description = "冻结目标，降低其行动能力" },
                new() { name = "技能.闪电链", description = "释放闪电链条，攻击多个目标" }
            }
        };
        _mockActorData[1].components.Add(new ComponentSerialization
        {
            name = "SkillBookComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(skillBook1))
        });

        var combatStats1 = new CombatStatsComponent
        {
            name = "CombatStatsComponent",
            stats = new CharacterStats
            {
                hp = 80,
                max_hp = 80,
                attack = 20,
                defense = 5
            },
            status_effects = new List<StatusEffect>
            {
                new() { name = "增益.法力充盈", category = "增益", manifestation = "体内法力涌动", effect = "法术伤害+5" },
                new() { name = "增益.元素共鸣", category = "增益", manifestation = "与元素产生共鸣", effect = "技能效果+20%" },
                new() { name = "增益.术士专注", category = "增益", manifestation = "精神高度集中", effect = "施法速度+15%" }
            }
        };
        _mockActorData[1].components.Add(new ComponentSerialization
        {
            name = "CombatStatsComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(combatStats1))
        });

        // 3. 角色.常物.野猪
        _mockActorData[2] = new EntitySerialization
        {
            name = "角色.常物.野猪",
            components = new List<ComponentSerialization>()
        };

        var skillBook2 = new SkillBookComponent
        {
            name = "SkillBookComponent",
            skills = new List<Skill>
            {
                new() { name = "技能.野蛮冲撞", description = "野蛮冲锋，撞击敌人造成伤害" },
                new() { name = "技能.獠牙突刺", description = "用锋利獠牙刺穿敌人" },
                new() { name = "技能.兽性咆哮", description = "发出野性咆哮，震慑敌人" }
            }
        };
        _mockActorData[2].components.Add(new ComponentSerialization
        {
            name = "SkillBookComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(skillBook2))
        });

        var combatStats2 = new CombatStatsComponent
        {
            name = "CombatStatsComponent",
            stats = new CharacterStats
            {
                hp = 120,
                max_hp = 120,
                attack = 12,
                defense = 8
            },
            status_effects = new List<StatusEffect>
            {
                new() { name = "增益.野性狂暴", category = "增益", manifestation = "野性本能爆发", effect = "攻击速度+10%" },
                new() { name = "增益.厚皮", category = "增益", manifestation = "皮糙肉厚", effect = "受到伤害-15%" },
                new() { name = "增益.兽性直觉", category = "增益", manifestation = "敏锐的野兽直觉", effect = "闪避率+8%" }
            }
        };
        _mockActorData[2].components.Add(new ComponentSerialization
        {
            name = "CombatStatsComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(combatStats2))
        });

        // 4. 角色.精怪.山魈
        _mockActorData[3] = new EntitySerialization
        {
            name = "角色.精怪.山魈",
            components = new List<ComponentSerialization>()
        };

        var skillBook3 = new SkillBookComponent
        {
            name = "SkillBookComponent",
            skills = new List<Skill>
            {
                new() { name = "技能.妖雾迷踪", description = "释放妖雾，迷惑敌人方向" },
                new() { name = "技能.精怪诅咒", description = "施加诅咒，削弱敌人能力" },
                new() { name = "技能.藤蔓缠绕", description = "召唤藤蔓缠绕并束缚敌人" }
            }
        };
        _mockActorData[3].components.Add(new ComponentSerialization
        {
            name = "SkillBookComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(skillBook3))
        });

        var combatStats3 = new CombatStatsComponent
        {
            name = "CombatStatsComponent",
            stats = new CharacterStats
            {
                hp = 90,
                max_hp = 90,
                attack = 14,
                defense = 7
            },
            status_effects = new List<StatusEffect>
            {
                new() { name = "增益.妖气缭绕", category = "增益", manifestation = "周身妖气环绕", effect = "敌人命中率-10%" },
                new() { name = "增益.诡秘之力", category = "增益", manifestation = "诡异的力量涌动", effect = "技能伤害+3" },
                new() { name = "增益.精怪敏捷", category = "增益", manifestation = "身形飘忽不定", effect = "移动速度+20%" }
            }
        };
        _mockActorData[3].components.Add(new ComponentSerialization
        {
            name = "CombatStatsComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(combatStats3))
        });

        // 5. 角色.大妖.山中虎
        _mockActorData[4] = new EntitySerialization
        {
            name = "角色.大妖.山中虎",
            components = new List<ComponentSerialization>()
        };

        var skillBook4 = new SkillBookComponent
        {
            name = "SkillBookComponent",
            skills = new List<Skill>
            {
                new() { name = "技能.虎啸震山", description = "发出震撼山林的虎啸" },
                new() { name = "技能.利爪撕裂", description = "用锋利虎爪撕裂敌人" },
                new() { name = "技能.王者霸体", description = "展现王者气势，提升战斗力" }
            }
        };
        _mockActorData[4].components.Add(new ComponentSerialization
        {
            name = "SkillBookComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(skillBook4))
        });

        var combatStats4 = new CombatStatsComponent
        {
            name = "CombatStatsComponent",
            stats = new CharacterStats
            {
                hp = 150,
                max_hp = 150,
                attack = 18,
                defense = 12
            },
            status_effects = new List<StatusEffect>
            {
                new() { name = "增益.王者威压", category = "增益", manifestation = "散发王者气势", effect = "敌人攻击力-3" },
                new() { name = "增益.百兽之王", category = "增益", manifestation = "君临百兽的霸气", effect = "所有属性+5%" },
                new() { name = "增益.虎啸余威", category = "增益", manifestation = "虎啸威慑敌胆", effect = "敌人防御力-2" }
            }
        };
        _mockActorData[4].components.Add(new ComponentSerialization
        {
            name = "CombatStatsComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(combatStats4))
        });
    }

    /// <summary>
    /// 从选中角色加载卡牌要素数据
    /// 只负责维护 CardElements 部分，不处理 Build 数据
    /// </summary>
    /// <param name="selectedActor">选中的角色</param>
    private void LoadCardElementsFromActor(EntitySerialization selectedActor)
    {
        if (selectedActor == null)
        {
            Debug.LogWarning("[LoadCardElementsFromActor] selectedActor is null");
            return;
        }

        // 1. 添加角色的技能
        var skillBook = GameUtils.GetComponent<SkillBookComponent>(selectedActor);
        if (skillBook?.skills != null)
        {
            foreach (var skill in skillBook.skills)
            {
                CardBuilder.AddElement(new CardElementData(skill));
            }
            Debug.Log($"[LoadCardElementsFromActor] 添加了 {skillBook.skills.Count} 个技能");
        }

        // 2. 添加角色的状态效果
        var combatStats = GameUtils.GetComponent<CombatStatsComponent>(selectedActor);
        if (combatStats?.status_effects != null)
        {
            foreach (var effect in combatStats.status_effects)
            {
                CardBuilder.AddElement(new CardElementData(effect));
            }
            Debug.Log($"[LoadCardElementsFromActor] 添加了 {combatStats.status_effects.Count} 个状态效果");
        }

        // 3. 添加其他角色作为目标（排除自己）
        int targetCount = 0;
        foreach (var actor in _mockActorData)
        {
            if (actor.name != selectedActor.name)
            {
                CardBuilder.AddElement(new CardElementData(actor));
                targetCount++;
            }
        }
        Debug.Log($"[LoadCardElementsFromActor] 添加了 {targetCount} 个目标角色");

        // 更新滚动视图
        _scrollView.totalItemCount = CardBuilder.Count;
        _scrollView.scrollByItemIndex(0); // 滚动到顶部
        _scrollView.refresh();
        Debug.Log($"[LoadCardElementsFromActor] 总共加载 {CardBuilder.Count} 个卡牌要素");
    }

    void Awake()
    {
        // 创建 mock 数据
        CreateMockActorData();
    }

    void Start()
    {
        Debug.Assert(_combatInfoText != null, "_combatInfoText is null");
        Debug.Assert(_mainText != null, "Main Text component is not assigned in the inspector.");
        Debug.Assert(_actorSlots != null && _actorSlots.Length > 0, "Actor slots are not assigned in the inspector.");
        Debug.Assert(_scrollView != null, "ScrollView component is not assigned in the inspector.");
        Debug.Assert(_onCardClickedEvent != null, "_onCardClickedEvent is null");
        Debug.Assert(_onActorSlotClickedEvent != null, "_onActorSlotClickedEvent is null");
        Debug.Assert(_onCardBuilderDataChangedEvent != null, "_onCardBuilderDataChangedEvent is null");
        Debug.Assert(_mockActorData != null && _mockActorData.Length > 0, "Mock actor data is not initialized");
        Debug.Assert(SpriteCacheManager.Instance != null, "SpriteCacheManager instance is null");

        // 注册事件监听器
        _onCardClickedEvent.RegisterListener(this);
        _onActorSlotClickedEvent.RegisterListener(this);

        // 设置初始文本内容
        _combatInfoText.text = "场景.测试地下城关卡-第1局";

        // 默认加载第一个角色的卡牌要素
        CardBuilder.Clear();
        // 滚动到顶部
        UpdateMainTextWithCardBuildData();

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
        //_mainText.text = $"事件: {eventData.eventType}\n目标: {eventData.targetId}\n索引: {eventData.index}\n额外: {eventData.extraData}";

        switch (eventData.eventType)
        {
            case UIEventType.CardElementScrollViewItemClick:
                Debug.Log($"处理卡牌要素滚动视图项点击事件，目标: {eventData.targetId}, 索引: {eventData.index}");

                // 切换要素在 Build 中的状态（存在则删除，不存在则添加）
                if (!CardBuilder.TryToggleElementInBuild(eventData.index))
                {
                    Debug.LogWarning($"[TestDungeonCombatScenePrototype] TryToggleElementInBuild 失败，索引: {eventData.index}");
                    break;
                }

                // 更新主文本显示
                UpdateMainTextWithCardBuildData();

                // 派发 CardBuilder 数据已改变事件
                _onCardBuilderDataChangedEvent.Raise(new UIEventData(UIEventType.CardBuilderDataChanged));
                break;

            case UIEventType.ActorOrderSlotClick:
                HandleActorOrderSlotClick(eventData);
                break;

            default:
                Debug.LogWarning($"未处理的事件类型: {eventData.eventType}");
                break;
        }

    }

    /// <summary>
    /// 处理角色槽位点击事件
    /// 重置 Build 数据并动态加载选中角色的卡牌要素数据
    /// </summary>
    private void HandleActorOrderSlotClick(UIEventData eventData)
    {
        Debug.Log($"处理角色槽位点击事件，目标: {eventData.targetId}, 索引: {eventData.index}");

        // 从 mock 数据中查找对应的角色
        var selectedActor = System.Array.Find(_mockActorData, actor => actor.name == eventData.targetId);
        if (selectedActor != null)
        {
            // 清空并设置新的 Build 数据
            CardBuilder.Clear();
            CardBuilder.Build = new CardBuildData { owner = selectedActor };

            // 加载卡牌要素
            LoadCardElementsFromActor(selectedActor);
            UpdateMainTextWithCardBuildData();

            // 派发 CardBuilder 数据已改变事件
            _onCardBuilderDataChangedEvent.Raise(new UIEventData(UIEventType.CardBuilderDataChanged));
        }
        else
        {
            Debug.LogWarning($"未找到名为 {eventData.targetId} 的角色数据");
        }
    }

    /// <summary>
    /// 更新主文本显示，展示当前卡牌构建数据的状态
    /// </summary>
    private void UpdateMainTextWithCardBuildData()
    {
        Debug.Assert(CardBuilder.Build != null, "CardBuilder.Build is null");
        _mainText.text = GameUtils.FormatCardBuildData(CardBuilder.Build);
    }
}

