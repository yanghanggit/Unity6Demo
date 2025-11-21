/**
 * 代理事件头部枚举（对应 Python IntEnum）
 */
public enum EventHead
{
    NONE = 0,
    SPEAK_EVENT = 1,
    WHISPER_EVENT = 2,
    ANNOUNCE_EVENT = 3,
    MIND_EVENT = 4,
    QUERY_EVENT = 5,
    TRANS_STAGE_EVENT = 6,
    COMBAT_KICK_OFF_EVENT = 7,
    COMBAT_COMPLETE_EVENT = 8,
    DISCUSSION_EVENT = 9,
    NIGHT_ACTION_EVENT = 10,
    VOTE_EVENT = 11
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
 * 代理事件基类
 */
[System.Serializable]
public class AgentEvent
{
    public int head = (int)EventHead.NONE;
    public string message = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
 * 说话事件
 */
[System.Serializable]
public sealed class SpeakEvent : AgentEvent
{
    //public new int head = (int)EventHead.SPEAK_EVENT;
    public string actor = "";
    public string target = "";
    public string content = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
 * 耳语事件
 */
[System.Serializable]
public sealed class WhisperEvent : AgentEvent
{
    //public new int head = (int)EventHead.WHISPER_EVENT;
    public string actor = "";
    public string target = "";
    public string content = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
 * 宣布事件
 */
[System.Serializable]
public sealed class AnnounceEvent : AgentEvent
{
    //public new int head = (int)EventHead.ANNOUNCE_EVENT;
    public string actor = "";
    public string stage = "";
    public string content = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
 * 心灵语音事件
 */
[System.Serializable]
public sealed class MindEvent : AgentEvent
{
    //public new int head = (int)EventHead.MIND_EVENT;
    public string actor = "";
    public string content = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
 * 转移场景事件
 */
[System.Serializable]
public sealed class TransStageEvent : AgentEvent
{
    //public new int head = (int)EventHead.TRANS_STAGE_EVENT;
    public string actor = "";
    public string from_stage = "";
    public string to_stage = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
 * 战斗开始事件
 */
[System.Serializable]
public sealed class CombatKickOffEvent : AgentEvent
{
    //public new int head = (int)EventHead.COMBAT_KICK_OFF_EVENT;
    public string actor = "";
    public string description = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
 * 战斗完成事件
 */
[System.Serializable]
public sealed class CombatCompleteEvent : AgentEvent
{
    //public new int head = (int)EventHead.COMBAT_COMPLETE_EVENT;
    public string actor = "";
    public string summary = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
 * 讨论事件
 */
[System.Serializable]
public sealed class DiscussionEvent : AgentEvent
{
    //public new int head = (int)EventHead.DISCUSSION_EVENT;
    public string actor = "";
    public string stage = "";
    public string content = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
 * 夜晚行动事件
 */
[System.Serializable]
public sealed class NightActionEvent : AgentEvent
{
    //public new int head = (int)EventHead.NIGHT_ACTION_EVENT;
    public string actor = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
 * 投票事件
 */
[System.Serializable]
public sealed class VoteEvent : AgentEvent
{
    //public new int head = (int)EventHead.VOTE_EVENT;
    public string actor = "";
    public string target = "";
}