using UnityEngine;

/*
MainHubUI  (Canvas根节点)
├── TopHUDBar
│   ├── PlayerAvatarImage
│   └── ResourcesPanel
│       ├── ResourceItem_1
│       │   ├── ResourceNameText   → "资源1"
│       │   └── ResourceValueText  → "999/999"
│       ├── ResourceItem_2
│       └── ResourceItem_3
├── TabContentPanel                ← 主内容区（Tab页内容在此切换）
│   ├── Tab_1_Panel
│   ├── Tab_2_Panel
│   ├── Tab_3_Panel
│   ├── Tab_4_Panel
│   └── Tab_5_Panel
└── BottomNavBar
    ├── NavTabButton_1
    ├── NavTabButton_2
    ├── NavTabButton_3
    ├── NavTabButton_4
    └── NavTabButton_5

ResourceItem_1/2/3 建议做成预制体（Prefab），等资源类型确定后改为 GoldItem、GemItem 等语义名
NavTabButton 比 NavButton 更准确，体现它是 Tab 切换入口而非普通跳转
TabContentPanel 下的子 Panel 等功能确认后可改为 ShopPanel、BagPanel 之类的具体名称
对应控制器脚本建议命名为 MainHubController.cs，放在 Assets/_Project/MainHub/ 目录下（与 PlayerLobby/ 并列）
*/

public class MainHubController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
