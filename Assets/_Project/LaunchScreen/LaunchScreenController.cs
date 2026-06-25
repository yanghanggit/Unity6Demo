using UnityEngine;
using UnityEngine.UI;

public class LaunchScreenController : MonoBehaviour
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
        //LoadLoginScene().Forget();
        Debug.Log("Button clicked, proceeding to the next scene...");
    }
}

