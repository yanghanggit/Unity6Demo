// 对应 Python models/agent_event.py

public enum EventType
{
    NONE = 0,
    SPEAK = 1,
    WHISPER = 2,
    ANNOUNCE = 3,
    MIND = 4,
    QUERY = 5,
    TRANS_STAGE = 6,
    COMBAT_INITIATION = 7,
    COMBAT_ARBITRATION = 8,
    COMBAT_ARCHIVE = 9,
    APPEARANCE_UPDATE = 10,
}

public class AgentEvent
{
    public int type = (int)EventType.NONE;
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

public sealed class QueryEvent : AgentEvent
{
    public string actor = "";
    public string question = "";
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

public sealed class CombatArchiveEvent : AgentEvent
{
    public string actor = "";
    public string summary = "";
}

public sealed class AppearanceUpdateEvent : AgentEvent
{
    public string actor = "";
    public string appearance = "";
}
