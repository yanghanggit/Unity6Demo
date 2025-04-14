using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Scriptable Objects/GameConfig")]
public class GameConfig : ScriptableObject
{
    public string UserName;
    public string GameName;
    public string ActorName;
    public string apiEndPoints;

   
}
