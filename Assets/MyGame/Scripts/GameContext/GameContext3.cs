using System.Collections.Generic;
//using Newtonsoft.Json;
using UnityEngine;

public partial class GameContext
{
    // 临时写死。
    public const string CampName = "场景.营地";
    public const string WarriorName = "角色.战士.卡恩";
    public const string WizardName = "角色.法师.奥露娜";

    public bool homeAdvanceDone = false;

    private Dictionary<string, string> _imagePath = new Dictionary<string, string>
    {
        { WarriorName, $"{Application.dataPath}/MyGame/Assets/warrior.png" },
        { WizardName, $"{Application.dataPath}/MyGame/Assets/wizard.png" }
    };

    public Dictionary<string, string> ImagePath
    {
        get { return _imagePath; }
        set { _imagePath = value; }
    }

    
}