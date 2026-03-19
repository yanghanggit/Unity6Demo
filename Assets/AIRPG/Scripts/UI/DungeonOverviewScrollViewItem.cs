using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DungeonOverviewScrollViewItem : UIBehaviour, IScrollViewItem
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _title; // card名称文本

    public void OnUpdateItem(int index)
    {

        Debug.Assert(_title != null, "_title is not assigned in the inspector.");

        Debug.Log($"Updating Dungeon Overview ScrollView item at index: {index}");
        if (index < 0 || index >= DungeonOverviewScene.DungeonOverviews.Count)
        {
            Debug.LogError($"Index {index} is out of range for DungeonOverviews list");
            return;
        }
        _title.text = DungeonOverviewScene.DungeonOverviews[index].dungeonName;
    }
}
