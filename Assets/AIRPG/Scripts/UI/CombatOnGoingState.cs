using System.Collections.Generic;
using UnityEngine;

public class CombatOnGoingState : MonoBehaviour
{
    [Header("UI Components")]

    [SerializeField] private ActionOrderPanel _actionOrderPanel; // 行动顺序面板控制器

    private List<EntitySerialization> _mockActorData;

    void Awake()
    {
        // 创建 mock 数据
        _mockActorData = MockData.CreateActorData();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Assert(_actionOrderPanel != null, "_actionOrderPanel is null");
    }

    public void OnShow()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("CombatOnGoingState: Player is not logged in, using mock data to display action order panel");
            _actionOrderPanel.SetActionOrder(_mockActorData);
            return;
        }

    }
}
