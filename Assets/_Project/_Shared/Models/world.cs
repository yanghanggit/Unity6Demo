// 对应 Python models/world_state.py
using System.Collections.Generic;

public sealed class WorldState
{
    public int entity_counter = 0;
    public List<EntitySerialization> entities = new();
    public Dungeon dungeon = new();
    public Blueprint blueprint = new();
    public Dictionary<string, AgentMemory> agent_memories = new();
}
