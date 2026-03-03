/// <summary>
/// 游戏状态同步操作的错误码
/// 用于编排型同步方法的返回值，供外部调用方精准定位失败断点
/// </summary>
public enum GameSyncError
{
    /// <summary>成功</summary>
    None = 0,

    /// <summary>无法获取 StageActorMapping</summary>
    FetchMappingFailed,

    /// <summary>无法获取 StageDetails</summary>
    FetchStageDetailsFailed,

    /// <summary>无法获取 ActorDetails</summary>
    FetchActorDetailsFailed,

    /// <summary>无法获取 Dungeon</summary>
    FetchDungeonFailed,

    /// <summary>无法获取指定 Stage 内的 Actors</summary>
    FetchActorsInStageFailed,
}
