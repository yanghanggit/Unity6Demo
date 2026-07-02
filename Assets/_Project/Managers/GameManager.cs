// 全局游戏管理器。
// 正常流程：在（Bootstrap 场景）中放一个带有此组件以及 GameServerClientHolder 组件的 GameObject。
// 从任意场景 Play：RuntimeInitializeOnLoadMethod 会在任何场景加载前自动创建此 GameObject，
// 配置从 Resources/ServerConfig.asset 读取，与 Inspector 路径完全一致。
// 其他脚本通过 GameManager.Instance.ServerClient 访问服务器客户端。

using Newtonsoft.Json.Linq;
using UnityEngine;

[RequireComponent(typeof(GameServerClientHolder))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 同一 GameObject 上挂载的 GameServerClientHolder 组件
    [SerializeField] private GameServerClientHolder _serverClientHolder;

    /// <summary>
    /// 服务器 API 客户端，等价于 GameServerClientHolder.Instance.Client。
    /// </summary>
    public GameServerClient ServerClient => _serverClientHolder.Client;

    // ── 当前登录会话 ──────────────────────────────────────────────────────────

    /// <summary>
    /// 玩家会话数据（不可变）。null 表示未登录。
    /// </summary>
    public class PlayerSession
    {
        public string PlayerId { get; }
        public string GameName { get; }

        public PlayerSession(string playerId, string gameName)
        {
            PlayerId = playerId;
            GameName = gameName;
        }
    }

    /// <summary>
    /// 当前玩家会话数据（不可变）。null 表示未登录。
    /// </summary>
    public PlayerSession Session { get; private set; }
    // ── 游戏启动状态 ──────────────────────────────────────────────────────────
    /// <summary>
    /// 服务器信息。null 表示尚未连通服务器。
    /// </summary>
    public JObject ServerInfo { get; set; }
    public bool IsServerConnected => ServerInfo != null;

    /// <summary>
    /// 设置当前玩家会话数据（不可变）。
    /// </summary>
    public void SetSession(string playerId, string gameName)
    {
        Session = new PlayerSession(playerId, gameName);
    }

    /// <summary>
    /// 清除当前玩家会话数据（不可变）。
    /// </summary>
    public void ClearSession()
    {
        Session = null;
    }

    // 从任意场景按 Play 时，自动创建 GameManager（若尚未存在）。
    // 在所有场景 Awake 之前执行，Build 中同样生效。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoBootstrap()
    {
        if (Instance != null) return; // MainScene 正常启动时已存在，跳过
        var go = new GameObject("[GameManager]");
        go.AddComponent<GameServerClientHolder>(); // Awake 里自动加载 Resources/ServerConfig
        go.AddComponent<GameManager>();            // Awake 里设置 Instance + DontDestroyOnLoad
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 如果 Inspector 未赋值，尝试从同一 GameObject 上自动获取
        if (_serverClientHolder == null)
            _serverClientHolder = GetComponent<GameServerClientHolder>();

        Debug.Assert(_serverClientHolder != null, "[GameManager] 未找到 GameServerClientHolder 组件，请在同一 GameObject 上添加该组件。");
        if (_serverClientHolder == null)
            Debug.LogError("[GameManager] 未找到 GameServerClientHolder 组件，请在同一 GameObject 上添加该组件。");
    }
}
