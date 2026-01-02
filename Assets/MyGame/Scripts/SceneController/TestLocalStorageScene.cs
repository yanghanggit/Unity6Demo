using UnityEngine;
using TMPro;

public class TestLocalStorageScene : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText;
    [SerializeField] private string _testKey = "TestLocalStorageKey";

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");

        // 使用 PlayerPrefs 读取_testKey的值，如果是没有，就返回空字符串，并且生成一个随机的字符串存储在 _testKey中，然后显示在界面上
        // 如果已经有值了，就直接显示在界面上
        string value = PlayerPrefs.GetString(_testKey, string.Empty);
        if (string.IsNullOrEmpty(value))
        {
            value = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(_testKey, value);
            PlayerPrefs.Save(); // 确保数据被保存
        }
        _mainText.text = $"Stored value: {value}";
    }

    // WebGL 生命周期处理：当页面失去焦点时保存数据
    // 注意：关闭/刷新页面时这些方法可能来不及执行，所以重要数据应该在修改后立即保存
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            PlayerPrefs.Save(); // 页面暂停时保存
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            PlayerPrefs.Save(); // 页面失去焦点时保存
        }
    }
}
