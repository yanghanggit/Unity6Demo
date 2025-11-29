using UnityEngine;

/// <summary>
/// 餐馆场景控制器
/// </summary>
public class RestaurantScene : BaseHomeSceneController
{
    protected override string StageName => "场景.餐馆";
    protected override string PreScene => "MainScene2";
    protected override string SceneDisplayName => "RestaurantScene";
}
