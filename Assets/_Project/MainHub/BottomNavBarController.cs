using UnityEngine;
using TMPro;

public class BottomNavBarController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text[] _navTabButtonTexts; // 导航标签按钮文本数组，按顺序对应标签顺序

    void Awake()
    {
        Debug.Assert(_navTabButtonTexts != null && _navTabButtonTexts.Length == 5, "_navTabButtonTexts is null or empty");
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < _navTabButtonTexts.Length; i++)
        {
            _navTabButtonTexts[i].text = $"标签{i + 1}"; // TODO: 待替换为实际标签名
        }
    }

    public void OnNavTabButtonClicked(int tabIndex)
    {
        Debug.Log($"导航标签按钮 {tabIndex} 被点击");
    }
}
