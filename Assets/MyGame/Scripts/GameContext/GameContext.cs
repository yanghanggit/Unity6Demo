using System.Diagnostics;


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
    private static readonly object _lockObj = new();

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
                _instance ??= new GameContext();
                return _instance;
            }
        }
    }

    /// <summary>
    /// 清除单例实例和所有静态数据
    /// 用于登出或重新初始化游戏状态
    /// </summary>
    public static void ClearInstance()
    {
        lock (_lockObj)
        {
            // 清空单例实例（这会导致实例字段如 UserName、GameName 等也被清除）
            _instance = null;
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
    private string _userName;

    /// <summary>
    /// 游戏名称
    /// </summary>
    private string _gameName;

    /// <summary>
    /// 角色名称
    /// </summary>
    private string _actorName;



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
            Debug.Assert(value >= 0, "LastSequenceId cannot be negative");
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
    /// 获取登录API的URL地址
    /// </summary>
    public string LoginUrl
    {
        get
        {

            return RootResp.Get().endpoints["login"];
        }
    }

    /// <summary>
    /// 获取登出API的URL地址
    /// </summary>
    public string LogoutUrl
    {
        get
        {
            return RootResp.Get().endpoints["logout"];
        }
    }

    /// <summary>
    /// 获取主场景游戏玩法API的URL地址
    /// </summary>
    public string HomeGamePlayUrl
    {
        get
        {
            return RootResp.Get().endpoints["home_gameplay"];
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
            var baseUrl = RootResp.Get().endpoints["stages_state"];
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
            var baseUrl = RootResp.Get().endpoints["dungeon_state"];
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
            var baseUrl = RootResp.Get().endpoints["entity_details"];
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
            return RootResp.Get().endpoints["start"];
        }
    }

    /// <summary>
    /// 获取从主场景转换到地牢场景API的URL地址
    /// </summary>
    public string HomeTransDungeonUrl
    {
        get
        {
            return RootResp.Get().endpoints["home_trans_dungeon"];
        }
    }

    /// <summary>
    /// 获取地牢游戏玩法API的URL地址
    /// </summary>
    public string DungeonGamePlayUrl
    {
        get
        {
            return RootResp.Get().endpoints["dungeon_gameplay"];
        }
    }

    /// <summary>
    /// 获取地牢战斗出牌API的URL地址
    /// </summary>
    public string DungeonCombatPlayCardsUrl
    {
        get
        {
            return RootResp.Get().endpoints["dungeon_combat_play_cards"];
        }
    }

    /// <summary>
    /// 获取从地牢场景转换到主场景API的URL地址
    /// </summary>
    public string DungeonTransHomeUrl
    {
        get
        {
            return RootResp.Get().endpoints["dungeon_trans_home"];
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
            var baseUrl = RootResp.Get().endpoints["session_messages"];
            return $"{baseUrl}{UserName}/{GameName}/since";
        }
    }

    /// <summary>
    /// 获取任务触发API的URL地址
    /// </summary>
    public string TasksTriggerUrl
    {
        get
        {
            return RootResp.Get().endpoints["tasks_trigger"];
        }
    }

    /// <summary>
    /// 获取任务状态API的URL地址模板
    /// 返回包含 {task_id} 占位符的URL模板，需要调用者替换为实际的任务ID
    /// </summary>
    public string TasksStatusUrl
    {
        get
        {
            var baseUrl = RootResp.Get().endpoints["tasks_status"];
            return baseUrl;
        }
    }

}