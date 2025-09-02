using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public partial class GameContext
{
    // 临时写死。
    private Dictionary<string, string> _imagePath = new Dictionary<string, string>
    {
        { "角色.战士.卡恩", $"{Application.dataPath}/MyGame/Assets/warrior.png" },
        { "角色.法师.奥露娜", $"{Application.dataPath}/MyGame/Assets/wizard.png" }
    };

    public Dictionary<string, string> ImagePath
    {
        get { return _imagePath; }
        set { _imagePath = value; }
    }
}