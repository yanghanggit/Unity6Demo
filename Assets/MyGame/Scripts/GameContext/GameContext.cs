/// <summary>
/// 游戏上下文管理类
/// 用于管理游戏的全局状态，包括用户信息、游戏信息和API端点配置
/// 采用线程安全的单例模式实现
/// </summary>
public partial class GameContext
{
    /// <summary>
    /// 单例实例
    /// </summary>
    private static GameContext _instance;

    /// <summary>
    /// 线程锁对象，用于确保单例模式的线程安全
    /// </summary>
    /// <summary>
    /// 线程锁对象,用于确保单例模式的线程安全
    /// </summary>
    private static readonly object _lockObj = new object();

    /// <summary>
    /// 获取GameContext的单例实例
    /// 使用双重检查锁定模式确保线程安全
    /// </summary>
    public static GameContext Instance
    {
        get
        {
            lock (_lockObj)
            {
                if (_instance == null)
                {
                    _instance = new GameContext();
                }
                return _instance;
            }
        }
    }

    /// <summary>
    /// 私有构造函数，防止外部实例化
    /// </summary>
    private GameContext()
    {
    }

    /// <summary>
    /// 用户名
    /// </summary>
    private string _userName = "";

    /// <summary>
    /// 游戏名称
    /// </summary>
    private string _gameName = "";

    /// <summary>
    /// 角色名称
    /// </summary>
    private string _actorName = "";

    /// <summary>
    /// 根响应对象，包含所有API端点配置
    /// </summary>
    private RootResponse _rootResponse = new RootResponse();

    /// <summary>
    /// 最后一次序列ID，用于追踪游戏事件的顺序
    /// </summary>
    private int _lastSequenceId = 0;

    /// <summary>
    /// 获取或设置最后一次序列ID
    /// </summary>
    public int LastSequenceId
    {
        get
        {
            return _lastSequenceId;
        }
        set
        {
            _lastSequenceId = value;
        }
    }

    /// <summary>
    /// 获取或设置用户名
    /// </summary>
    public string UserName
    {
        get
        {
            return _userName;
        }
        set
        {
            _userName = value;
        }
    }

    /// <summary>
    /// 获取或设置游戏名称
    /// </summary>
    public string GameName
    {
        get
        {
            return _gameName;
        }
        set
        {
            _gameName = value;
        }
    }

    /// <summary>
    /// 获取或设置角色名称
    /// </summary>
    public string ActorName
    {
        get
        {
            return _actorName;
        }
        set
        {
            _actorName = value;
        }
    }

    /// <summary>
    /// 获取或设置根响应对象
    /// 在设置时会验证所有必需的API端点是否存在
    /// </summary>
    public RootResponse Root
    {
        get
        {
            return _rootResponse;
        }

        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("_rootResponse is null");
                return;
            }
            _rootResponse = value;

            // 验证关键字段 - 确保所有必需的API端点都已配置
            UnityEngine.Debug.Assert(_rootResponse.endpoints != null, "endpoints is null");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("login"), "endpoints does not contain login");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("logout"), "endpoints does not contain logout");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("home_gameplay"), "endpoints does not contain home_gameplay");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("stages_state"), "endpoints does not contain stages_state");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("dungeon_state"), "endpoints does not contain dungeon_state");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("entity_details"), "endpoints does not contain entity_details");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("start"), "endpoints does not contain start");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("home_trans_dungeon"), "endpoints does not contain home_trans_dungeon");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("dungeon_gameplay"), "endpoints does not contain dungeon_gameplay");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("dungeon_trans_home"), "endpoints does not contain dungeon_trans_home");
        }
    }

    /// <summary>
    /// 获取登录API的URL地址
    /// </summary>
    public string LoginUrl
    {
        get
        {
            return _rootResponse.endpoints["login"];
        }
    }

    /// <summary>
    /// 获取登出API的URL地址
    /// </summary>
    public string LogoutUrl
    {
        get
        {
            return _rootResponse.endpoints["logout"];
        }
    }

    /// <summary>
    /// 获取主场景游戏玩法API的URL地址
    /// </summary>
    public string HomeGameplayUrl
    {
        get
        {
            return _rootResponse.endpoints["home_gameplay"];
        }
    }

    /// <summary>
    /// 获取关卡状态API的URL地址
    /// 根据用户名和游戏名称动态构建完整的URL
    /// </summary>
    public string StagesStateUrl
    {
        get
        {
            var baseUrl = _rootResponse.endpoints["stages_state"];
            return $"{baseUrl}{UserName}/{GameName}/state";
        }
    }

    /// <summary>
    /// 获取地牢状态API的URL地址
    /// 根据用户名和游戏名称动态构建完整的URL
    /// </summary>
    public string DungeonStateUrl
    {
        get
        {
            var baseUrl = _rootResponse.endpoints["dungeon_state"];
            return $"{baseUrl}{UserName}/{GameName}/state";
        }
    }

    /// <summary>
    /// 获取实体详情API的URL地址
    /// 根据用户名和游戏名称动态构建完整的URL
    /// </summary>
    public string EntityDetailsUrl
    {
        get
        {
            var baseUrl = _rootResponse.endpoints["entity_details"];
            return $"{baseUrl}{UserName}/{GameName}/details";
        }
    }

    /// <summary>
    /// 获取游戏开始API的URL地址
    /// </summary>
    public string StartUrl
    {
        get
        {
            return _rootResponse.endpoints["start"];
        }
    }

    /// <summary>
    /// 获取从主场景转换到地牢场景API的URL地址
    /// </summary>
    public string HomeTransDungeonUrl
    {
        get
        {
            return _rootResponse.endpoints["home_trans_dungeon"];
        }
    }

    /// <summary>
    /// 获取地牢游戏玩法API的URL地址
    /// </summary>
    public string DungeonGameplayUrl
    {
        get
        {
            return _rootResponse.endpoints["dungeon_gameplay"];
        }
    }

    /// <summary>
    /// 获取从地牢场景转换到主场景API的URL地址
    /// </summary>
    public string DungeonTransHomeUrl
    {
        get
        {
            return _rootResponse.endpoints["dungeon_trans_home"];
        }
    }

    /// <summary>
    /// 获取会话消息API的URL地址
    /// 根据用户名和游戏名称动态构建完整的URL，用于获取增量消息
    /// </summary>
    public string SessionMessagesUrl
    {
        get
        {
            var baseUrl = _rootResponse.endpoints["session_messages"];
            return $"{baseUrl}{UserName}/{GameName}/since";
        }
    }
}