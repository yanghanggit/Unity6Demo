using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonOverviewDetailPanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _enterDungeonButton; // 进入地下城的按钮


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Assert(_enterDungeonButton != null, "_enterDungeonButton is null");
        //_enterDungeonButton.onClick.AddListener(OnClickEnterDungeon);

    }

    public async UniTaskVoid OnRefreshView(string dungeonNname)
    {
        Debug.Log($"DungeonOverviewDetailPanel refreshing view for dungeon: {dungeonNname}");

        await UniTask.Yield(); // 等待一帧，确保 UI 已经更新

        _enterDungeonButton.GetComponentInChildren<TMP_Text>().text = $"Enter {dungeonNname}";

    }
}
