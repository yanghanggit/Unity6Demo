// 对应 Python models/agent_event.py

public enum EventHead
{
    NONE = 0,
    SPEAK_EVENT = 1,
    WHISPER_EVENT = 2,
    ANNOUNCE_EVENT = 3,
    MIND_EVENT = 4,
    QUERY_EVENT = 5,
    TRANS_STAGE_EVENT = 6,
    COMBAT_INITIATION_EVENT = 7,
    COMBAT_ARBITRATION_EVENT = 8,
    COMBAT_ARCHIVE_EVENT = 9,
    APPEARANCE_UPDATE_EVENT = 10,
}

public class AgentEvent
{
    public int head = (int)EventHead.NONE;
    public string message = "";
}

public sealed class SpeakEvent : AgentEvent
{
    public string actor = "";
    public string target = "";
    public string content = "";
}

public sealed class WhisperEvent : AgentEvent
{
    public string actor = "";
    public string target = "";
    public string content = "";
}

public sealed class AnnounceEvent : AgentEvent
{
    public string actor = "";
    public string stage = "";
    public string content = "";
}

public sealed class MindEvent : AgentEvent
{
    public string actor = "";
    public string content = "";
}

public sealed class TransStageEvent : AgentEvent
{
    public string actor = "";
    public string from_stage = "";
    public string to_stage = "";
}

public sealed class CombatInitiationEvent : AgentEvent
{
    public string actor = "";
}

public sealed class CombatArbitrationEvent : AgentEvent
{
    public string stage = "";
    public string combat_log = "";
    public string narrative = "";
}

public sealed class AppearanceUpdateEvent : AgentEvent
{
    public string actor = "";
    public string target = "";
    public string appearance = "";
}
