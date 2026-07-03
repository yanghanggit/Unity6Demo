// 对应 Python models/player_session.py
using System.Collections.Generic;

public sealed class PlayerSession
{
    public string name = "";
    public string actor = "";
    public string game = "";
    public List<SessionMessage> session_messages = new();
    public int event_sequence = 0;
}
