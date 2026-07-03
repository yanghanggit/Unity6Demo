using UnityEngine;

public class PlayerProfilePanelController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 关闭玩家信息面板
    /// </summary>
    public void OnClickCloseButton()
    {
        Debug.Log("点击了关闭按钮");
        HidePanel(); // 隐藏玩家信息面板
    }

    /// <summary>
    /// 显示玩家信息面板
    /// </summary>
    public void ShowPanel()
    {
        gameObject.SetActive(true); // 显示玩家信息面板
    }

    /// <summary>
    /// 隐藏玩家信息面板
    /// </summary>
    public void HidePanel()
    {
        gameObject.SetActive(false); // 隐藏玩家信息面板
    }
}
