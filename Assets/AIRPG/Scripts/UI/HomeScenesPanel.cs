using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;




public class HomeScenesPanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private LoopVerticalScrollRect _scrollView; // 动态滚动视图

    void Start()
    {
        Debug.Assert(_scrollView != null, "_scrollView is null");

        //
        RereshViewAsync().Forget();
    }

    public async UniTaskVoid RereshViewAsync()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            _scrollView.gameObject.SetActive(false);
            // _scrollView.gameObject.SetActive(true);
            // _scrollView.totalCount = 5;
            // _scrollView.RefillCells(); // 重建列表并回到顶部
            return;
        }

        var (dungeon, stagesState) = await UniTask.WhenAll(
              GameStateSync.Instance.GetDungeon(),
              GameStateSync.Instance.GetStagesState()
          );

        if (dungeon == null || stagesState == null)
        {
            Debug.LogError($"Failed to get dungeon or stagesState data from server. dungeon is null: {dungeon == null}, stagesState is null: {stagesState == null}");
            return;
        }

        // 获取所有的 stagesState 的 value，组成一个list
        var allActorNames = new List<string>();
        foreach (var actorNames in stagesState.Values)
        {
            allActorNames.AddRange(actorNames);
        }

        var allActorEntities = await GameStateSync.Instance.GetEntities(allActorNames);
        if (allActorEntities == null)
        {
            Debug.LogError("ActorPositioningPanel: Actor entities data is null, cannot refresh combat view");
            return;
        }

        MainScene.HomeSceneDataList.Clear();
        foreach (var kvp in stagesState)
        {
            var stageName = kvp.Key;
            if (stageName == GameContext.Instance.PlayerOnlyStageName)
            {
                continue; // 跳过玩家专属场景
            }

            var actorNames = kvp.Value;
            var sceneData = new HomeSceneData
            {
                stageName = stageName,
                actorsOnStage = new List<EntitySerialization>()
            };

            for (int j = 0; j < actorNames.Count; j++)
            {
                var actorName = actorNames[j];
                if (actorName == GameContext.Instance.PlayerActorName)
                {
                    continue; // 跳过玩家自己
                }

                var actorEntity = allActorEntities.Find(e => e.name == actorName);
                if (actorEntity != null)
                {
                    sceneData.actorsOnStage.Add(actorEntity);
                }
                else
                {
                    Debug.LogWarning($"Actor entity not found for actor name: {actorName} in stage: {stageName}");
                }
            }

            MainScene.HomeSceneDataList.Add(sceneData);
        }

        // 如果 dungeon name 不为空，
        if (!string.IsNullOrEmpty(dungeon.name))
        {
            // 将 dungeon name 添加到每个 HomeSceneData 的 dungeonName 字段中
            MainScene.HomeSceneDataList.Add(new HomeSceneData
            {
                stageName = dungeon.name,
                actorsOnStage = new List<EntitySerialization>(),
                dungeonName = dungeon.name,
            });
        }

        _scrollView.gameObject.SetActive(true);
        _scrollView.totalCount = MainScene.HomeSceneDataList.Count;
        _scrollView.RefillCells(); // 重建列表并回到顶部
    }

}
