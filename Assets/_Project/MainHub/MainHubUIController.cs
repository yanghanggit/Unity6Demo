using UnityEngine;

public class MainHubUIController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TopHUDBarController _topHUDBarController; // 顶部HUD栏控制器
    [SerializeField] private BottomNavBarController _bottomNavBarController; // 底


    void Awake()
    {
        Debug.Assert(_topHUDBarController != null, "_topHUDBarController is null");
        Debug.Assert(_bottomNavBarController != null, "_bottomNavBarController is null");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
