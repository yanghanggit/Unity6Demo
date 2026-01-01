
using UnityEngine;

/// <summary>
/// 根响应管理器 - 静态单例类
/// 负责存储和管理从服务器获取的根响应数据(RootResponse)
/// 主要用于管理和验证API端点配置
/// </summary>
public static class RootResp
{
    /// <summary>
    /// 存储根响应数据的私有静态字段
    /// </summary>
    private static RootResponse _gameRootResponse;

    /// <summary>
    /// 存储图片服务根响应数据的私有静态字段
    /// 用于管理图片生成、列表查询等相关API端点
    /// </summary>
    private static ImageRootResponse _imageRootResponse;

    /// <summary>
    /// 获取当前存储的根响应对象
    /// </summary>
    /// <returns>返回RootResponse对象,如果未设置则返回null</returns>
    public static RootResponse GetGameRoot()
    {
        return _gameRootResponse;
    }

    /// <summary>
    /// 设置根响应对象并验证所有必需的API端点是否存在
    /// </summary>
    /// <param name="resp">从服务器获取的根响应对象</param>
    public static void SetGameRoot(RootResponse resp)
    {
        _gameRootResponse = resp;

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

    /// <summary>
    /// 获取当前存储的图片服务根响应对象
    /// </summary>
    /// <returns>返回ImageRootResponse对象,如果未设置则返回null</returns>
    public static ImageRootResponse GetImageRoot()
    {
        return _imageRootResponse;
    }

    /// <summary>
    /// 设置图片服务根响应对象并验证所有必需的API端点是否存在
    /// </summary>
    /// <param name="resp">从图片服务器获取的根响应对象</param>
    public static void SetImageRoot(ImageRootResponse resp)
    {
        _imageRootResponse = resp;

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