using UnityEngine;
using TMPro;

public class PlayerProfilePanelController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _text;

    void Awake()
    {
        Debug.Assert(_text != null, "_text is null");
    }

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

        if (GameManager.Instance.Session != null)
        {
            var session = GameManager.Instance.Session;
            _text.text = $"玩家ID: {session.PlayerSession.name}\n" +
                         $"游戏名: {session.GameName}\n" +
                         $"角色名: {session.ActorName}\n" +
                         $"事件序列号: {session.PlayerSession.event_sequence}";
        }
        else
        {
            _text.text = "未登录";
        }
    }

    /// <summary>
    /// 隐藏玩家信息面板
    /// </summary>
    public void HidePanel()
    {
        gameObject.SetActive(false); // 隐藏玩家信息面板
    }
}
