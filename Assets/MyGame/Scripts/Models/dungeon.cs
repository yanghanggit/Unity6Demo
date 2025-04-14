


using System.Collections.Generic;


/**
* 战斗状态
*/
public enum CombatPhase
{
    NONE = 0,
    KICK_OFF = 1,  // 初始化，需要同步一些数据与状态
    ONGOING = 2,  // 运行中，不断进行战斗推理
    COMPLETE = 3,  // 结束，需要进行结算
    POST_WAIT = 4  // 战斗等待进入新一轮战斗或者回家
}

/**
 * 
 */
public enum CombatResult
{
    NONE = 0,
    HERO_WIN = 1,  // 胜利
    HERO_LOSE = 2  // 失败
}

/**
 * 
 */
[System.Serializable]
public class StatusEffect
{
    public string name = "";
    public string description = "";
    public int rounds = 0;
}

/**
 * 
 */
[System.Serializable]
public class Skill
{
    public string name = "";
    public string description = "";
    public string effect = "";
}

/**
 * 
 */
[System.Serializable]
public class Round
{
    public string tag = "";
    public List<string> round_turns = new List<string>();
    public string stage_environment = "";
    public Dictionary<string, string> select_report = new Dictionary<string, string>();
    public string stage_director_calculation = "";
    public string stage_director_performance = "";
    public Dictionary<string, string> feedback_report = new Dictionary<string, string>();
}

/**
 * 
 */
[System.Serializable]
public class Combat
{
    public string name = "";
    public CombatPhase phase = CombatPhase.NONE;
    public CombatResult result = CombatResult.NONE;
    public List<Round> rounds = new List<Round>();
    public Dictionary<string, string> summarize_report = new Dictionary<string, string>();
}

/**
 * 
 */
[System.Serializable]
public class Engagement
{
    public List<Combat> combats = new List<Combat>();
}

/**
 * 
 */
[System.Serializable]
public class Dungeon
{
    public string name = "";
    public List<Stage> levels = new List<Stage>();
    public Engagement engagement = new Engagement();
    public int position = -1;
}
