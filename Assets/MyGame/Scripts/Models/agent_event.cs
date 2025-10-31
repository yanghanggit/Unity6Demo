/// <summary>
/// 代理事件头部枚举
/// </summary>
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

/// <summary>
/// 代理事件基类
/// </summary>
[System.Serializable]
public class AgentEvent
{
    public int head = (int)EventHead.NONE;
    public string message = "";
}

/// <summary>
/// 说话事件
/// </summary>
[System.Serializable]
public sealed class SpeakEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.SPEAK_EVENT;
    public string actor = "";
    public string target = "";
    public string content = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 耳语事件
/// </summary>
[System.Serializable]
public sealed class WhisperEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.WHISPER_EVENT;
    public string actor = "";
    public string target = "";
    public string content = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 宣布事件
/// </summary>
[System.Serializable]
public sealed class AnnounceEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.ANNOUNCE_EVENT;
    public string actor = "";
    public string stage = "";
    public string content = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 心灵语音事件
/// </summary>
[System.Serializable]
public sealed class MindEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.MIND_VOICE_EVENT;
    public string actor = "";
    public string content = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 战斗开始事件
/// </summary>
[System.Serializable]
public sealed class CombatKickOffEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.COMBAT_KICK_OFF_EVENT;
    public string actor = "";
    public string description = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 战斗完成事件
/// </summary>
[System.Serializable]
public sealed class CombatCompleteEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.COMBAT_COMPLETE_EVENT;
    public string actor = "";
    public string summary = "";
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

[System.Serializable]
public sealed class DiscussionEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.DISCUSSION_EVENT;
    public string actor = "";
    public string stage = "";
    public string content = "";
}
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

[System.Serializable]
public sealed class NightActionEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.NIGHT_ACTION_EVENT;
    public string actor = "";
} 

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

[System.Serializable]
public sealed class VoteEvent : AgentEvent
{
    public string actor = "";
    public string target = "";
}