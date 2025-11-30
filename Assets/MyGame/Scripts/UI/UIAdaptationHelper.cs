// using UnityEngine;
// using UnityEngine.UI;

// /// <summary>
// /// UI适配辅助类，用于设置Canvas和按钮的屏幕适配
// /// </summary>
// public class UIAdaptationHelper : MonoBehaviour
// {
//     [Header("Canvas Settings")]
//     public Canvas targetCanvas;
    
//     [Header("Reference Resolution")]
//     public Vector2 referenceResolution = new Vector2(1920, 1080);
    
//     [Header("Match Settings")]
//     [Range(0f, 1f)]
//     public float matchWidthOrHeight = 0.5f; // 0为完全匹配宽度，1为完全匹配高度，0.5为平衡
    
//     [Header("Six Buttons to Adapt")]
//     public RectTransform button1; // 左上角
//     public RectTransform button2; // 左中
//     public RectTransform button3; // 左下
//     public RectTransform button4; // 右上角
//     public RectTransform button5; // 右中
//     public RectTransform button6; // 右下
    
//     void Start()
//     {
//         SetupCanvasScaler();
//         SetupButtonAnchors();
//     }
    
//     /// <summary>
//     /// 设置Canvas Scaler组件，确保UI能够适配不同屏幕比例
//     /// </summary>
//     void SetupCanvasScaler()
//     {
//         if (targetCanvas == null)
//         {
//             targetCanvas = GetComponent<Canvas>();
//         }
        
//         if (targetCanvas == null)
//         {
//             Debug.LogError("Canvas not found!");
//             return;
//         }
        
//         // 获取或添加CanvasScaler组件
//         CanvasScaler canvasScaler = targetCanvas.GetComponent<CanvasScaler>();
//         if (canvasScaler == null)
//         {
//             canvasScaler = targetCanvas.gameObject.AddComponent<CanvasScaler>();
//         }
        
//         // 设置缩放模式为"Scale With Screen Size"
//         canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        
//         // 设置参考分辨率
//         canvasScaler.referenceResolution = referenceResolution;
        
//         // 设置匹配模式
//         canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
//         canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        
//         Debug.Log($"Canvas Scaler设置完成: 参考分辨率{referenceResolution}, 匹配值{matchWidthOrHeight}");
//     }
    
//     /// <summary>
//     /// 设置6个按钮的锚点位置，确保它们在不同屏幕比例下保持正确的相对位置
//     /// </summary>
//     void SetupButtonAnchors()
//     {
//         // 按钮1 - 左上角 (图中的角色1)
//         SetButtonAnchor(button1, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100f, -100f));
        
//         // 按钮2 - 左中 (图中的角色2)  
//         SetButtonAnchor(button2, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(100f, 0f));
        
//         // 按钮3 - 左下 (图中的角色3)
//         SetButtonAnchor(button3, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(100f, 100f));
        
//         // 按钮4 - 右上角 (图中的角色4)
//         SetButtonAnchor(button4, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-100f, -100f));
        
//         // 按钮5 - 右中 (图中的角色5)
//         SetButtonAnchor(button5, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-100f, 0f));
        
//         // 按钮6 - 右下 (图中的角色6)
//         SetButtonAnchor(button6, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-100f, 100f));
        
//         Debug.Log("6个按钮的锚点设置完成");
//     }
    
//     /// <summary>
//     /// 设置单个按钮的锚点位置
//     /// </summary>
//     /// <param name="buttonRect">按钮的RectTransform</param>
//     /// <param name="anchorMin">锚点最小值</param>
//     /// <param name="anchorMax">锚点最大值</param>
//     /// <param name="anchoredPosition">锚点位置偏移</param>
//     void SetButtonAnchor(RectTransform buttonRect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
//     {
//         if (buttonRect == null)
//         {
//             Debug.LogWarning("Button RectTransform is null, skipping...");
//             return;
//         }
        
//         // 设置锚点
//         buttonRect.anchorMin = anchorMin;
//         buttonRect.anchorMax = anchorMax;
        
//         // 设置偏移位置
//         buttonRect.anchoredPosition = anchoredPosition;
        
//         // 设置中心点为(0.5, 0.5)，确保按钮以中心点为准进行定位
//         buttonRect.pivot = new Vector2(0.5f, 0.5f);
        
//         Debug.Log($"按钮 {buttonRect.name} 锚点设置为: Min{anchorMin}, Max{anchorMax}, Position{anchoredPosition}");
//     }
    
//     /// <summary>
//     /// 运行时调试方法：显示当前屏幕信息
//     /// </summary>
//     [ContextMenu("显示屏幕信息")]
//     void ShowScreenInfo()
//     {
//         Debug.Log($"当前屏幕分辨率: {Screen.width} x {Screen.height}");
//         Debug.Log($"屏幕宽高比: {(float)Screen.width / Screen.height:F2}");
        
//         if (targetCanvas != null)
//         {
//             CanvasScaler scaler = targetCanvas.GetComponent<CanvasScaler>();
//             if (scaler != null)
//             {
//                 Debug.Log($"Canvas参考分辨率: {scaler.referenceResolution}");
//                 Debug.Log($"参考宽高比: {scaler.referenceResolution.x / scaler.referenceResolution.y:F2}");
//                 Debug.Log($"匹配模式: {scaler.matchWidthOrHeight}");
//             }
//         }
//     }
// }