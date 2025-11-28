using System.Collections.Generic;

public partial class GameContext
{
    private Dictionary<string, List<string>> _mapping = new Dictionary<string, List<string>>();

    private List<EntitySerialization> _actorEntitiesSerialization = new List<EntitySerialization>();

    private List<EntitySerialization> _stageEntitiesSerialization = new List<EntitySerialization>();

    private Dungeon _dungeon = new Dungeon();


    public Dictionary<string, List<string>> Mapping
    {
        get
        {
            return _mapping;
        }

        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("Mapping is null");
                return;
            }
            _mapping = value;
        }
    }

    public List<string> AllActors
    {
        get
        {
            List<string> allActors = new List<string>();
            foreach (var kvp in _mapping)
            {
                allActors.AddRange(kvp.Value);
            }
            return allActors;
        }
    }

    public List<string> AllStages
    {
        get
        {
            return new List<string>(_mapping.Keys);
        }
    }

    public string GetActorStage(string actorName)
    {
        foreach (var kvp in _mapping)
        {
            if (kvp.Value.Contains(actorName))
            {
                return kvp.Key;
            }
        }
        return "";
    }

    public List<string> GetActorsInStage(string stageName)
    {
        if (_mapping.ContainsKey(stageName))
        {
            return _mapping[stageName];
        }
        return new List<string>();
    }

    public Dungeon Dungeon
    {
        get
        {
            return _dungeon;
        }

        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("Dungeon is null");
                return;
            }
            _dungeon = value;
        }
    }

    public List<EntitySerialization> ActorEntitiesSerialization
    {
        get
        {
            return _actorEntitiesSerialization;
        }
        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("ActorEntitiesSerialization is null");
                return;
            }
            _actorEntitiesSerialization = value;
        }
    }

    public List<EntitySerialization> StageEntitiesSerialization
    {
        get
        {
            return _stageEntitiesSerialization;
        }
        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("StageEntitiesSerialization is null");
                return;
            }
            _stageEntitiesSerialization = value;
        }
    }

    public EntitySerialization getActorEntitySerialization(string actorName)
    {
        foreach (var entitySerialization in _actorEntitiesSerialization)
        {
            if (entitySerialization.name == actorName)
            {
                return entitySerialization;
            }
        }
        return null;
    }

    public EntitySerialization getStageEntitySerialization(string stageName)
    {
        foreach (var entitySerialization in _stageEntitiesSerialization)
        {
            if (entitySerialization.name == stageName)
            {
                return entitySerialization;
            }
        }
        return null;
    }
}