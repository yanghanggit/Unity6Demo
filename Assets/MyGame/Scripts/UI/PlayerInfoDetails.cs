using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Newtonsoft.Json;


public class PlayerInfoDetails : MonoBehaviour
{

    [Header("UI Components")]
    [SerializeField] private Image _playerImage;
    [SerializeField] private TMP_Text _playerInfoText;

    public event Action OnCloseButtonClickedCallback;

    void Start()
    {
        Debug.Assert(_playerImage != null, "_playerImage is null");
        Debug.Assert(_playerInfoText != null, "_playerInfoText is null");

        // 初始化为空
        _playerImage.sprite = null;
        _playerInfoText.text = "";

        // 刷新内容
        RefreshPlayerDetails();
    }

    private void RefreshPlayerDetails()
    {
        // 设置图片
        var playerActorEntitySerialization = GameContext.Instance.GetActorEntitySerialization(GameContext.Instance.ActorName);
        Debug.Assert(playerActorEntitySerialization != null, "Player actor entity serialization is null for actor name: " + GameContext.Instance.ActorName);
        if (playerActorEntitySerialization == null)
        {
            return;
        }

        var actorSprite = TextureManager.Instance.GetSprite(playerActorEntitySerialization.name);
        Debug.Assert(actorSprite != null, "Player actor sprite is null for entity: " + playerActorEntitySerialization.name);
        //_playerImage.sprite = actorSprite;
        _playerImage.gameObject.SetActive(false); // 先隐藏图片，避免空白显示

        // 设置文本
        try
        {
            // 直接将 EntitySerialization 序列化为 JSON 字符串
            string jsonString = JsonConvert.SerializeObject(playerActorEntitySerialization);
            Debug.Log($"Actor JSON:\n{jsonString}");
            _playerInfoText.text = jsonString;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to serialize Actor to JSON: {ex.Message}");
        }
    }

    public void OnClickClosePlayerInfoDetails()
    {
        Debug.Log("Player info details clicked!");
        OnCloseButtonClickedCallback?.Invoke();
    }
}
