using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using TMPro;

public class LoginScene : MonoBehaviour
{
    public static readonly string NextSceneName = "MainScene";

    public static readonly string GameName = "Game1";

    [Header("UI Components")]
    [SerializeField] private TMP_Text _userName;
    [SerializeField] private TMP_Text _bluePrints;

    // 内部使用的玩家标识符
    private string _playerId;

    public string PlayerId
    {
        get
        {
            if (string.IsNullOrEmpty(_playerId))
            {
                System.DateTime now = System.DateTime.Now;
                string timestamp = now.ToString("yyyyMMddHHmmss");
                _playerId = "unity-player-" + timestamp;
            }
            return _playerId;
        }
    }

    void Start()
    {
        Debug.Assert(_userName != null, "_userNameText is null");
        Debug.Assert(_bluePrints != null, "_bluePrintsText is null");

        // 初始化玩家ID并显示在UI上
        _userName.text = PlayerId;
        _bluePrints.text = string.Empty;

        // 预加载蓝图数据，确保后续场景可以快速访问
        GetBlueprints().Forget();
    }

    /// <summary>
    /// 点击开始游戏按钮的回调
    /// </summary>
    public void OnStartGameClicked()
    {
        StartGameFlow(_playerId, GameName).Forget();
    }

    /// <summary>
    /// 获取蓝图列表并显示在UI上
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid GetBlueprints()
    {
        if (string.IsNullOrEmpty(GameContext.BaseUrl))
        {
            Debug.LogWarning("BaseUrl is not set, cannot retrieve blueprints");
            _bluePrints.text = "Base URL not configured";
            return;
        }

        var blueprints = await SessionManager.Instance.GetBlueprints();
        if (blueprints != null)
        {
            Debug.Log($"Successfully retrieved {blueprints.Count} blueprints");
        }
        else
        {
            Debug.LogError("Failed to retrieve blueprints");
        }

        // 遍历 blueprints 输出每个蓝图的name，然后赋值给 _gameName.text 显示在UI上
        if (blueprints != null && blueprints.Count > 0)
        {
            string blueprintNames = string.Join(", ", blueprints.ConvertAll(b => b.name));
            Debug.Log($"Blueprint names: {blueprintNames}");
            _bluePrints.text = blueprintNames;

            // assert GameName 一定在蓝图列表中
            bool gameNameExists = blueprints.Exists(b => b.name == GameName);
            Debug.Assert(gameNameExists, $"GameName '{GameName}' does not exist in blueprints list");
        }
        else
        {
            Debug.LogWarning("No blueprints available to display");
            _bluePrints.text = "No blueprints available";
        }
    }

    /// <summary>
    /// 执行登录并开始游戏的完整流程：登录 -> 开始游戎 -> 同步状态 -> 加载场景
    /// </summary>
    private async UniTaskVoid StartGameFlow(string userName, string gameName)
    {
        bool isLoginSuccessful = await SessionManager.Instance.Login(userName, gameName);
        if (!isLoginSuccessful)
        {
            Debug.LogError("[SessionManager] LoginAndStart failed at Login step");
            return;
        }

        // 2. 新建游戏
        bool isStartSuccessful = await SessionManager.Instance.NewGame(userName, gameName);
        if (!isStartSuccessful)
        {
            Debug.LogError("[SessionManager] LoginAndStart failed at NewGame step");
            return;
        }

        // 3. 同步游戏状态
        await UniTask.Yield();

        // 3. 切换场景
        SceneManager.LoadScene(NextSceneName);
    }
}
