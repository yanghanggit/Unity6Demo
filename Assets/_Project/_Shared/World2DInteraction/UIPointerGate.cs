using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 供世界交互脚本（<see cref="WorldClickDetector"/> 等）查询"当前屏幕坐标是否落在某个
/// UI Toolkit 可交互 Panel 上"的全局注册表。
/// 场景里的 HUD Controller（如 <see cref="TopHUDController"/> / <see cref="BottomHUDController"/>）
/// 在 Panel 加载完成后把自己的 <see cref="IPanel"/> 注册进来，
/// 之后任何世界点击检测在真正做 Physics 拾取前，先用 <see cref="IsPointerOverUI"/> 排除掉
/// "点在 HUD 按钮上"的情况，避免 UI 点击穿透到场景对象。
/// </summary>
public static class UIPointerGate
{
    private static readonly List<IPanel> _panels = new();

    public static void Register(IPanel panel)
    {
        if (panel != null && !_panels.Contains(panel))
            _panels.Add(panel);
    }

    public static void Unregister(IPanel panel)
    {
        _panels.Remove(panel);
    }

    /// <summary>
    /// 给定屏幕坐标（像素，原点左下角，与 <c>Pointer.current.position</c> 一致），
    /// 判断是否命中已注册 Panel 中的某个可交互 UI 元素。
    /// </summary>
    public static bool IsPointerOverUI(Vector2 screenPosition)
    {
        for (int i = 0; i < _panels.Count; i++)
        {
            var panel = _panels[i];
            if (panel == null)
                continue;

            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, screenPosition);
            if (panel.Pick(panelPos) != null)
                return true;
        }

        return false;
    }
}
