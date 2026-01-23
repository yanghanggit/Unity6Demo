
using UnityEngine;

/// <summary>
/// API端点管理器 - 静态类
/// 负责存储和管理从服务器获取的API根响应数据(RootResponse和ImageRootResponse)
/// 包括游戏API和图片服务API的基础URL以及端点配置
/// 设置响应对象时会自动验证所有必需的API端点是否存在
/// </summary>
public static class ApiEndpointsManager
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

    // 直接写所有端点为静态只读字段，方便其他地方引用
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

    /// <summary> 
    /// 图片服务API基础URL
    /// </summary>
    private static string _imageApiBaseUrl;

    /// <summary>
    /// 获取或设置图片服务API基础URL
    /// </summary>
    public static string ImageApiBaseUrl
    {
        get { return _imageApiBaseUrl; }
        set { _imageApiBaseUrl = value; }
    }

    /// <summary>
    /// 图片服务API根响应数据的私有存储字段
    /// </summary>
    private static ImageRootResponse _imageRootResponse;

    /// <summary>
    /// 图片服务API根响应对象
    /// 包含图片服务相关的所有API端点配置(图片生成、列表查询、静态图片访问等)
    /// 设置时会自动验证所有必需的端点是否存在
    /// </summary>
    public static ImageRootResponse ImageRootResponse
    {
        get { return _imageRootResponse; }
        set
        {
            _imageRootResponse = value;

            // 验证所有必要的图片服务端点是否存在
            if (_imageRootResponse != null)
            {
                var endpoints = _imageRootResponse.endpoints;

                // 图片生成与管理相关端点
                Debug.Assert(endpoints.ContainsKey("generate"), "endpoints does not contain generate");
                Debug.Assert(endpoints.ContainsKey("images_list"), "endpoints does not contain images_list");
                Debug.Assert(endpoints.ContainsKey("static_images"), "endpoints does not contain static_images");
                Debug.Assert(endpoints.ContainsKey("docs"), "endpoints does not contain docs");
            }
        }
    }
}