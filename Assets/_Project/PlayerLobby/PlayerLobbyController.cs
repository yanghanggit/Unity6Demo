// using Cysharp.Threading.Tasks;
// using Newtonsoft.Json.Linq;
using UnityEngine;
using TMPro;

public class PlayerLobbyController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _playerIdText;
    [SerializeField] private TMP_Text _gameNameText;


    /// 私有字段
    private string _randomPlayerId = string.Empty;
    private const string _gameName = "Game1";

    void Awake()
    {
        Debug.Assert(_playerIdText != null, "_playerIdText is null");
        Debug.Assert(_gameNameText != null, "_gameNameText is null");
    }

    void Start()
    {
        // 初始化玩家ID并显示在UI上
        if (string.IsNullOrEmpty(_randomPlayerId))
        {
            System.DateTime now = System.DateTime.Now;
            string timestamp = now.ToString("yyyyMMddHHmmss");
            _randomPlayerId = "unity-player-" + timestamp;
        }

        // 显示玩家ID和游戏名在UI上
        _playerIdText.text = $"玩家ID: {_randomPlayerId}";
        _gameNameText.text = $"游戏名: {_gameName}";
    }

    /// <summary>
    /// 点击新游戏按钮的回调
    /// </summary>
    public void OnClickNewGame()
    {
        Debug.Log($"点击新游戏按钮，玩家ID: {_randomPlayerId}, 游戏名: {_gameName}");
        // 这里可以添加进入新游戏的逻辑，例如加载新场景或初始化
    }
}

