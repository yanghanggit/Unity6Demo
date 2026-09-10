# WebGL 文本输入与中文输入法（IME）限制

## 结论（决策）

- **不解决。** 已选择方案：接受现状，让用户在输入法外打好字后 `Ctrl+V` 粘贴。
- 该问题**仅 WebGL（浏览器）存在**；打成原生 app（移动/桌面）正常。

## 现象

- WebGL 桌面浏览器（Chrome 等）：聚焦 Unity 文本输入框后**无法切换输入法 / 拼音候选框不弹 / 打出来是英文字母**。
- UGUI `InputField` 与 UI Toolkit `TextField` 表现完全一致。

## 根因

- WebGL 里 Unity 用隐藏的 `<textarea>` DOM 元素承接键盘输入，UGUI / UI Toolkit 共用这条 `WebGLInput` 管线。
- 桌面浏览器 IME 组合依赖 `compositionstart/update/end` 事件作用在"可见、可聚焦"的输入元素上，隐藏 textarea 无法稳定触发 → IME 失效。
- 与 UI 框架无关，C# / UXML / USS 层无法修复。

## 平台差异

| 平台 | 中文输入 |
| --- | --- |
| WebGL 桌面浏览器 | ❌ IME 失效（本问题） |
| WebGL 移动浏览器 | ⚠️ 软键盘由 OS 处理，IME 组合在 OS 层完成，通常正常；但需实测软键盘能否弹出（UI Toolkit 触发 TouchScreenKeyboard 是独立问题） |
| 原生 app（移动/桌面） | ✅ OS 原生 IME，正常 |

## 约束

- 不要尝试用 `WebGLInput.captureAllKeyboardInput`、`Input.imeCompositionMode` 修复 —— 无效。
- 粘贴（`Ctrl+V`）走剪贴板路径，不受 IME 限制，中文可正常进入。
- 若未来要彻底解决，唯一路径：HTML 覆盖层 `input` + `.jslib` 桥接（浏览器原生 IME 输入后回传 Unity）。
