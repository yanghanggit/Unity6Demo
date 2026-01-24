/// <summary>
/// 游戏API端点管理器
/// 提供游戏服务器相关的API端点访问
/// </summary>
public static class GameApiEndpointsManager
{
    /// <summary>
    /// 游戏API基础URL
    /// </summary>
    private static string _baseUrl;

    /// <summary>
    /// 获取或设置游戏API基础URL
    /// </summary>
    public static string BaseUrl
    {
        get { return _baseUrl; }
        set { _baseUrl = value; }
    }

    public static readonly string Login = "/api/login/v1/";
    public static readonly string Logout = "/api/logout/v1/";
    public static readonly string StartGame = "/api/start/v1/";
    public static readonly string HomePlayerAction = "/api/home/player_action/v1/";
    public static readonly string HomeAdvance = "/api/home/advance/v1/";
    public static readonly string HomeTransDungeon = "/api/home/trans_dungeon/v1/";
    public static readonly string DungeonProgress = "/api/dungeon/progress/v1/";
    public static readonly string DungeonCombatDrawCards = "/api/dungeon/combat/draw_cards/v1/";
    public static readonly string DungeonCombatPlayCards = "/api/dungeon/combat/play_cards/v1/";
    public static readonly string DungeonTransHome = "/api/dungeon/trans_home/v1/";
    public static readonly string DungeonState = "/api/dungeons/v1/";
    public static readonly string SessionMessages = "/api/session_messages/v1/";
    public static readonly string EntityDetails = "/api/entities/v1/";
    public static readonly string StagesState = "/api/stages/v1/";
    public static readonly string TasksTrigger = "/api/tasks/v1/trigger";
    public static readonly string TasksStatus = "/api/tasks/v1/status";
}

/// <summary>
/// 图片API端点管理器
/// 提供图片服务器相关的API端点访问
/// </summary>
public static class ImageApiEndpointsManager
{
    /// <summary> 
    /// 图片服务API基础URL
    /// </summary>
    private static string _baseUrl;

    /// <summary>
    /// 获取或设置图片服务API基础URL
    /// </summary>
    public static string BaseUrl
    {
        get { return _baseUrl; }
        set { _baseUrl = value; }
    }

    /// <summary>
    /// 图片生成API端点
    /// </summary>
    public static readonly string ImageGenerate = "/api/generate/v1";
}