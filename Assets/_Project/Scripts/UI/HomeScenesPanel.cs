// using System.Collections.Generic;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.UI;

// public class HomeScenesPanel : MonoBehaviour
// {
//     [Header("UI Components")]
//     [SerializeField] private LoopVerticalScrollRect _scrollView; // 动态滚动视图

//     void Start()
//     {
//         Debug.Assert(_scrollView != null, "_scrollView is null");

//         //
//         RereshViewAsync().Forget();
//     }

//     public async UniTaskVoid RereshViewAsync()
//     {
//         var ct = this.GetCancellationTokenOnDestroy();

//         if (!GameContext.Instance.IsLoggedIn)
//         {
//             _scrollView.gameObject.SetActive(false);
//             // _scrollView.gameObject.SetActive(true);
//             // _scrollView.totalCount = 5;
//             // _scrollView.RefillCells(); // 重建列表并回到顶部
//             return;
//         }

//         var (dungeon, stagesState) = await UniTask.WhenAll(
//               GameStateSync.Instance.GetDungeon().AttachExternalCancellation(ct),
//               GameStateSync.Instance.GetStagesState().AttachExternalCancellation(ct)
//           );

//         if (dungeon == null || stagesState == null)
//         {
//             Debug.LogError($"Failed to get dungeon or stagesState data from server. dungeon is null: {dungeon == null}, stagesState is null: {stagesState == null}");
//             return;
//         }

//         // 获取所有的 stagesState 的 value，组成一个list
//         var allActorNames = new List<string>();
//         foreach (var actorNames in stagesState.Values)
//         {
//             allActorNames.AddRange(actorNames);
//         }

//         var allActorEntities = await GameStateSync.Instance.GetEntities(allActorNames).AttachExternalCancellation(ct);

//         if (allActorEntities == null)
//         {
//             Debug.LogError("ActorPositioningPanel: Actor entities data is null, cannot refresh combat view");
//             return;
//         }

//         MainScene.HomeScenes.Clear();
//         foreach (var kvp in stagesState)
//         {
//             var stageName = kvp.Key;
//             if (stageName == GameContext.Instance.PlayerOnlyStageName)
//             {
//                 continue; // 跳过玩家专属场景
//             }

//             var actorNames = kvp.Value;
//             var sceneData = new HomeSceneData
//             {
//                 stageName = stageName,
//                 actorsOnStage = new List<EntitySerialization>()
//             };

//             for (int j = 0; j < actorNames.Count; j++)
//             {
//                 var actorName = actorNames[j];
//                 if (actorName == GameContext.Instance.PlayerActorName)
//                 {
//                     continue; // 跳过玩家自己
//                 }

//                 var actorEntity = allActorEntities.Find(e => e.name == actorName);
//                 if (actorEntity != null)
//                 {
//                     sceneData.actorsOnStage.Add(actorEntity);
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"Actor entity not found for actor name: {actorName} in stage: {stageName}");
//                 }
//             }

//             MainScene.HomeScenes.Add(sceneData);
//         }

//         // 确保至少有一个场景数据可供显示，如果没有则添加一个默认的空场景
//         MainScene.HomeScenes.Add(new HomeSceneData
//         {
//             stageName = HomeSceneData.DungeonOverviewSceneName, // 这个名字是本地定义的，服务器不识别
//             actorsOnStage = new List<EntitySerialization>(),
//         });
        
//         // 刷新滚动视图
//         _scrollView.gameObject.SetActive(true);
//         _scrollView.totalCount = MainScene.HomeScenes.Count;
//         _scrollView.RefillCells(); // 重建列表并回到顶部
//     }
// }
