using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// 全局 Mock 数据工厂，提供测试用角色数据的创建方法
/// </summary>
public static class MockData
{

    //var mockStageName = "场景.山林边缘";
    public static readonly string MockStageName = "场景.山林边缘";

    /// <summary>
    /// 创建并返回一组测试用角色数据
    /// </summary>
    public static List<EntitySerialization> CreateActorData()
    {
        var actors = new List<EntitySerialization>();

        // 1. 角色.猎人.石坚
        actors.Add(new EntitySerialization
        {
            name = "角色.猎人.石坚",
            components = new List<ComponentSerialization>()
        });

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
        actors[0].components.Add(new ComponentSerialization
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
        actors[0].components.Add(new ComponentSerialization
        {
            name = "CombatStatsComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(combatStats0))
        });

        var expeditionMember0 = new ExpeditionMemberComponent { name = "ExpeditionMemberComponent", dungeon_name = "" };
        actors[0].components.Add(new ComponentSerialization
        {
            name = "ExpeditionMemberComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(expeditionMember0))
        });

        // 2. 角色.术士.云音
        actors.Add(new EntitySerialization
        {
            name = "角色.术士.云音",
            components = new List<ComponentSerialization>()
        });

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
        actors[1].components.Add(new ComponentSerialization
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
        actors[1].components.Add(new ComponentSerialization
        {
            name = "CombatStatsComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(combatStats1))
        });

        var expeditionMember1 = new ExpeditionMemberComponent { name = "ExpeditionMemberComponent", dungeon_name = "" };
        actors[1].components.Add(new ComponentSerialization
        {
            name = "ExpeditionMemberComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(expeditionMember1))
        });

        // 3. 角色.常物.野猪
        actors.Add(new EntitySerialization
        {
            name = "角色.常物.野猪",
            components = new List<ComponentSerialization>()
        });

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
        actors[2].components.Add(new ComponentSerialization
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
        actors[2].components.Add(new ComponentSerialization
        {
            name = "CombatStatsComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(combatStats2))
        });

        var enemy2 = new EnemyComponent { name = "EnemyComponent" };
        actors[2].components.Add(new ComponentSerialization
        {
            name = "EnemyComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(enemy2))
        });

        // 4. 角色.精怪.山魈
        actors.Add(new EntitySerialization
        {
            name = "角色.精怪.山魈",
            components = new List<ComponentSerialization>()
        });

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
        actors[3].components.Add(new ComponentSerialization
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
        actors[3].components.Add(new ComponentSerialization
        {
            name = "CombatStatsComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(combatStats3))
        });

        var enemy3 = new EnemyComponent { name = "EnemyComponent" };
        actors[3].components.Add(new ComponentSerialization
        {
            name = "EnemyComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(enemy3))
        });

        // 5. 角色.大妖.山中虎
        actors.Add(new EntitySerialization
        {
            name = "角色.大妖.山中虎",
            components = new List<ComponentSerialization>()
        });

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
        actors[4].components.Add(new ComponentSerialization
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
        actors[4].components.Add(new ComponentSerialization
        {
            name = "CombatStatsComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(combatStats4))
        });

        var enemy4 = new EnemyComponent { name = "EnemyComponent" };
        actors[4].components.Add(new ComponentSerialization
        {
            name = "EnemyComponent",
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(enemy4))
        });

        return actors;
    }
}
