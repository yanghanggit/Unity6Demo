using UnityEngine;
using TMPro;

public class TopHUDBarController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _resourcesText;
    [SerializeField] private TMP_Text _playerAvatarText;


    [Header("Player Profile Panel")]
    [SerializeField] private PlayerProfilePanelController _playerProfilePanelController;

    void Awake()
    {
        Debug.Assert(_resourcesText != null, "_resourcesText is null");
        Debug.Assert(_playerAvatarText != null, "_playerAvatarText is null");
        Debug.Assert(_playerProfilePanelController != null, "_playerProfilePanelController is null");
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _resourcesText.text = "资源: 999/999"; // TODO: 待替换为实际资源数据
        _playerAvatarText.text = "玩家头像";
        _playerProfilePanelController.HidePanel(); // 初始化时隐藏玩家信息面板
    }

    public void OnClickPlayerAvatar()
    {
        Debug.Log("点击了玩家头像");
        _playerProfilePanelController.ShowPanel(); // 显示玩家信息面板
    }
}
