
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
    private static RootResponse _rootResp;

    /// <summary>
    /// 获取当前存储的根响应对象
    /// </summary>
    /// <returns>返回RootResponse对象,如果未设置则返回null</returns>
    public static RootResponse Get()
    {
        return _rootResp;
    }

    /// <summary>
    /// 设置根响应对象并验证所有必需的API端点是否存在
    /// </summary>
    /// <param name="resp">从服务器获取的根响应对象</param>
    /// <remarks>
    /// 该方法会验证以下关键端点:
    /// - login: 用户登录
    /// - logout: 用户登出
    ///  /// - start: 游戏开始
    /// - home_gameplay: 主界面游戏玩法
    /// - stages_state: 关卡状态
    /// - dungeon_state: 地牢状态
    /// - entity_details: 实体详情
    /// - home_trans_dungeon: 从主界面转换到地牢
    /// - dungeon_gameplay: 地牢游戏玩法
    /// - dungeon_trans_home: 从地牢转换到主界面
    /// </remarks>
    public static void Set(RootResponse resp)
    {
        _rootResp = resp;

        // 验证所有必要的端点是否存在
        if (_rootResp != null)
        {
            var endpoints = _rootResp.endpoints;

            // 用户认证与游戏创建相关端点
            Debug.Assert(endpoints.ContainsKey("login"), "endpoints does not contain login");
            Debug.Assert(endpoints.ContainsKey("logout"), "endpoints does not contain logout");
            Debug.Assert(endpoints.ContainsKey("start"), "endpoints does not contain start");

            // 游戏玩法相关端点
            Debug.Assert(endpoints.ContainsKey("home_gameplay"), "endpoints does not contain home_gameplay");
            Debug.Assert(endpoints.ContainsKey("dungeon_gameplay"), "endpoints does not contain dungeon_gameplay");

            // 状态查询相关端点
            Debug.Assert(endpoints.ContainsKey("stages_state"), "endpoints does not contain stages_state");
            Debug.Assert(endpoints.ContainsKey("dungeon_state"), "endpoints does not contain dungeon_state");
            Debug.Assert(endpoints.ContainsKey("entity_details"), "endpoints does not contain entity_details");

            // 场景转换相关端点
            Debug.Assert(endpoints.ContainsKey("home_trans_dungeon"), "endpoints does not contain home_trans_dungeon");
            Debug.Assert(endpoints.ContainsKey("dungeon_trans_home"), "endpoints does not contain dungeon_trans_home");
        }
    }
}