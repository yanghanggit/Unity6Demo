

using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 表示战斗的状态 Phase
/// </summary>
public enum CombatState
{
    NONE = 0,
    INITIALIZATION = 1,  // 初始化，需要同步一些数据与状态
    ONGOING = 2,  // 运行中，不断进行战斗推理
    COMPLETE = 3,  // 结束，需要进行结算
    POST_COMBAT = 4  // 战斗等待进入新一轮战斗或者回家
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 表示战斗的状态
/// </summary>
public enum CombatResult
{
    NONE = 0,
    WIN = 1,  // 胜利
    LOSE = 2  // 失败
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 状态效果！
/// </summary>
[System.Serializable]
public sealed class StatusEffect
{
    public string name = "";        // 效果名称
    public string category = "";    // 分类：增益 | 减益 | 复合 | 条件触发 | 环境
    public string manifestation = "";// 表现：第一人称描述具体表现
    public string effect = "";      // 效果：数值影响（±X点
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 代表一张卡牌
/// </summary>
[System.Serializable]
public sealed class Card
{
    public string name = "";
    public string action = "";
    public CharacterStats stats = new();
    public List<string> targets = new();
    public List<StatusEffect> status_effects = new();
    public List<string> affixes = new();
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 表示一个回合
/// </summary>
[System.Serializable]
public sealed class Round
{
    public List<string> action_order = new(); // 行动顺序，按顺序记录角色名称
    public string combat_log = "";                         // 战斗计算日志
    public string narrative = "";                          // 叙事文本/演出描述
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 表示一个战斗
/// </summary>
[System.Serializable]
public sealed class Combat
{
    public string name = "";
    public CombatState state = CombatState.NONE;
    public CombatResult result = CombatResult.NONE;
    public List<Round> rounds = new();
    public bool retreated = false; // 是否已经撤退（如果玩家选择撤退则为true，反之为false）
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 地下城房间（关卡包装）
/// </summary>
[System.Serializable]
public sealed class DungeonRoom
{
    public Stage stage = new();
    public Combat combat = new();
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 地下城
/// </summary>
[System.Serializable]
public sealed class Dungeon
{
    public string name = "";
    public List<DungeonRoom> rooms = new();
    public string ecology = "";
    public int current_room_index = -1;
    public bool setup_entities = false;
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
