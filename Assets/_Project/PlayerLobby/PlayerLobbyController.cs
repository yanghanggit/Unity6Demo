// using Cysharp.Threading.Tasks;
// using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _button;

    void Start()
    {
        Debug.Assert(_button != null, "_button is null");
    }

    /// <summary>
    /// 点击事件处理函数
    /// </summary>
    public void OnClick()
    {
        
    }
}

