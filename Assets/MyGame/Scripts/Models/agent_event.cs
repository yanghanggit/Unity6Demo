/// <summary>
/// 代理事件头部枚举
/// </summary>
public enum AgentEventHead
{
    NONE = 0,
    SPEAK_EVENT = 1,
    WHISPER_EVENT = 2,
    ANNOUNCE_EVENT = 3,
    MIND_VOICE_EVENT = 4,
    COMBAT_KICK_OFF_EVENT = 5,
    COMBAT_COMPLETE_EVENT = 6,
}

/// <summary>
/// 代理事件基类
/// </summary>
[System.Serializable]
public class AgentEvent
{
    public int head = (int)AgentEventHead.NONE;
    public string message = "";
}

/// <summary>
/// 说话事件
/// </summary>
[System.Serializable]
public sealed class SpeakEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.SPEAK_EVENT;
    public string speaker = "";
    public string listener = "";
    public string dialogue = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 耳语事件
/// </summary>
[System.Serializable]
public sealed class WhisperEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.WHISPER_EVENT;
    public string speaker = "";
    public string listener = "";
    public string dialogue = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 宣布事件
/// </summary>
[System.Serializable]
public sealed class AnnounceEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.ANNOUNCE_EVENT;
    public string announcement_speaker = "";
    public string event_stage = "";
    public string announcement_message = "";
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// 心灵语音事件
/// </summary>
[System.Serializable]
public sealed class MindVoiceEvent : AgentEvent
{
    //public new int head = (int)AgentEventHead.MIND_VOICE_EVENT;
    public string speaker = "";
    public string dialogue = "";
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