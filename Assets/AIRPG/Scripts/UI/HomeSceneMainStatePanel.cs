using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HomeSceneMainStatePanel : MonoBehaviour
{
    

    [Header("UI Components")]
    [SerializeField] private Image _background;                 // 顶部信息栏
    [SerializeField] private LoopHorizontalScrollRect _scrollView; // 动态滚动视图

    void Start()
    {
        Debug.Assert(_background != null, "Background Image component is not assigned in the inspector.");
        Debug.Assert(_scrollView != null, "ScrollView component is not assigned in the inspector.");
        Debug.Assert(SpriteCacheManager.Instance != null, "SpriteCacheManager instance is not available.");


        if (GameContext.Instance.IsLoggedIn)
        {
            var stageSprite = SpriteCacheManager.Instance.GetSprite(HomeScene.CachedHomeStageName);
            if (stageSprite != null)
            {
                _background.sprite = stageSprite;
            }
            else
            {
                Debug.LogWarning("Stage sprite not found for: " + HomeScene.CachedHomeStageName);
            }
        }
        else
        {
            var stageSprite = SpriteCacheManager.Instance.GetSprite(MockData.MockStageName);
            if (stageSprite != null)
            {
                _background.sprite = stageSprite;
            }
            else
            {
                Debug.LogWarning("Stage sprite not found for: " + MockData.MockStageName);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public async UniTaskVoid RefreshView()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.Log("[HomeScene] GameContext Root is null");
            HomeScene.ActorNamesOnStage.Clear();


            var mockData = MockData.CreateActorData();
            for (int i = 0; i < mockData.Count; i++)
            {
                HomeScene.ActorNamesOnStage.Add(mockData[i].name);
            }

            _scrollView.totalCount = HomeScene.ActorNamesOnStage.Count;
            _scrollView.RefillCells();
            return;
        }

        var stagesState = await GameStateSync.Instance.GetStagesState();
        if (stagesState == null)
        {
            Debug.LogError("[HomeScene] Failed to get stages state from server");
            HomeScene.ActorNamesOnStage.Clear();
            _scrollView.totalCount = HomeScene.ActorNamesOnStage.Count;
            _scrollView.RefillCells();
            return;
        }

        foreach (var kvp in stagesState)
        {
            if (kvp.Value.Contains(GameContext.Instance.PlayerActorName))
            {
                HomeScene.ActorNamesOnStage = kvp.Value;
                break;
            }
        }

        HomeScene.ActorNamesOnStage.Remove(GameContext.Instance.PlayerActorName); // 移除玩家角色自己
        Debug.Log($"Actors in current stage: {string.Join(", ", HomeScene.ActorNamesOnStage)}");
        _scrollView.totalCount = HomeScene.ActorNamesOnStage.Count;
        _scrollView.RefillCells();
    }
}
