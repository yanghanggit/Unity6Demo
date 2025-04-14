//using System.Collections.Generic;
/**
 */
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

/**
* 
*/
[System.Serializable]
public class AgentEvent
{
    public int head = (int)AgentEventHead.NONE;

    public string message = "";
}

/**
* 
*/
[System.Serializable]
public class SpeakEvent : AgentEvent
{
    //public int head = AgentEventHead.SPEAK_EVENT;
    public string speaker = "";
    public string listener = "";
    public string dialogue = "";
}

/**
* 
*/
[System.Serializable]
public class WhisperEvent : AgentEvent
{
    //public int head = AgentEventHead.WHISPER_EVENT;
    public string speaker = "";
    public string listener = "";
    public string dialogue = "";
}

/**
* 
*/
[System.Serializable]
public class AnnounceEvent : AgentEvent
{
    //public int head = AgentEventHead.ANNOUNCE_EVENT;
    public string announcement_speaker = "";
    public string event_stage = "";
    public string announcement_message = "";
}

/**
* 
*/
[System.Serializable]
public class MindVoiceEvent : AgentEvent
{
    //public int head = AgentEventHead.MIND_VOICE_EVENT;
    public string speaker = "";
    public string dialogue = "";
}

/**
* 
*/
[System.Serializable]
public class CombatKickOffEvent : AgentEvent
{
    //public int head = AgentEventHead.COMBAT_KICK_OFF_EVENT;
    public string actor = "";
    public string description = "";
}

/**
* 
*/
[System.Serializable]
public class CombatCompleteEvent : AgentEvent
{
    //public int head = AgentEventHead.COMBAT_COMPLETE_EVENT;
    public string actor = "";
    public string summary = "";
}