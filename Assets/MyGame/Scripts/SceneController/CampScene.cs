using UnityEngine;

/// <summary>
/// 营地场景控制器
/// </summary>
public class CampScene : BaseHomeSceneController
{
    protected override string StageName => "场景.安全屋";
    protected override string PreScene => "MainScene2";
    protected override string SceneDisplayName => "CampScene";
}