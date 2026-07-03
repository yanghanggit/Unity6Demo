using UnityEngine;


// 场景的根部管理器，负责管理整个场景的生命周期和状态
public class MainHubController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private MainHubUIController _mainHubUIController; // 主界面UI控制器

    void Awake()
    {
        Debug.Assert(_mainHubUIController != null, "_mainHubUIController is null");
    }
}
