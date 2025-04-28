using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Scriptable Objects/GameConfig")]
public class GameConfig : ScriptableObject
{
    public string GameName;
    public string ActorName;
    public string AllyName;
    public string LocalHost;
    public string LocalNet;
}
