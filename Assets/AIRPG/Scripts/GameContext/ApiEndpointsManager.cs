
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
    private static string _gameApiBaseUrl;

    /// <summary>
    /// 获取或设置游戏API基础URL
    /// </summary>
    public static string GameApiBaseUrl
    {
        get { return _gameApiBaseUrl; }
        set { _gameApiBaseUrl = value; }
    }

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
    /// 游戏API根响应数据的私有存储字段
    /// </summary>
    private static RootResponse _gameRootResponse;

    /// <summary>
    /// 图片服务API根响应数据的私有存储字段
    /// </summary>
    private static ImageRootResponse _imageRootResponse;

    /// <summary>
    /// 游戏API根响应对象
    /// 包含游戏相关的所有API端点配置(登录、登出、游戏玩法、状态查询、场景转换等)
    /// 设置时会自动验证所有必需的端点是否存在
    /// </summary>
    public static RootResponse GameRootResponse
    {
        get { return _gameRootResponse; }
        set
        {
            _gameRootResponse = value;

            // 验证所有必要的端点是否存在
            if (_gameRootResponse != null)
            {
                var endpoints = _gameRootResponse.endpoints;

                // 用户认证与游戏创建相关端点
                Debug.Assert(endpoints.ContainsKey("login"), "endpoints does not contain login");
                Debug.Assert(endpoints.ContainsKey("logout"), "endpoints does not contain logout");
                Debug.Assert(endpoints.ContainsKey("start"), "endpoints does not contain start");

                // 游戏玩法相关端点
                Debug.Assert(endpoints.ContainsKey("home_gameplay"), "endpoints does not contain home_gameplay");
                Debug.Assert(endpoints.ContainsKey("home_advance"), "endpoints does not contain home_advance");
                Debug.Assert(endpoints.ContainsKey("dungeon_gameplay"), "endpoints does not contain dungeon_gameplay");
                Debug.Assert(endpoints.ContainsKey("dungeon_combat_play_cards"), "endpoints does not contain dungeon_combat_play_cards");

                // 状态查询相关端点
                Debug.Assert(endpoints.ContainsKey("stages_state"), "endpoints does not contain stages_state");
                Debug.Assert(endpoints.ContainsKey("dungeon_state"), "endpoints does not contain dungeon_state");
                Debug.Assert(endpoints.ContainsKey("entity_details"), "endpoints does not contain entity_details");

                // 场景转换相关端点
                Debug.Assert(endpoints.ContainsKey("home_trans_dungeon"), "endpoints does not contain home_trans_dungeon");
                Debug.Assert(endpoints.ContainsKey("dungeon_trans_home"), "endpoints does not contain dungeon_trans_home");

                // 测试后台任务相关端点
                Debug.Assert(endpoints.ContainsKey("tasks_trigger"), "endpoints does not contain tasks_trigger");
                Debug.Assert(endpoints.ContainsKey("tasks_status"), "endpoints does not contain tasks_status");

            }
        }
    }

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