# VSCode 查看 Unity 日志配置说明

## 📝 概述

本配置允许你在 VSCode 中实时查看 Unity Editor 的日志输出，无需频繁切换到 Unity Editor 查看 Console。

## 🎯 功能特点

- ✅ 无需修改任何代码
- ✅ 实时监控 Unity 日志输出
- ✅ 包含完整的堆栈跟踪信息
- ✅ 显示文件名和行号
- ✅ 一键启动/停止

## 📁 修改内容

### 新增文件

- `.vscode/tasks.json` - VSCode 任务配置文件

### 配置说明

文件路径：`.vscode/tasks.json`

提供了 3 个实用任务：

1. **监控Unity日志** - 实时追踪所有日志输出（推荐）
2. **查看Unity日志(最后100行)** - 快速查看最近的日志
3. **清空并监控Unity日志** - 清空终端后开始监控

## 🚀 使用方法

### 方法一：通过命令面板（推荐）

1. 在 Unity Editor 中运行你的游戏
2. 在 VSCode 中按 **`Cmd+Shift+P`** (Mac) 或 **`Ctrl+Shift+P`** (Windows/Linux)
3. 输入 **`Tasks: Run Task`**
4. 选择 **`监控Unity日志`**
5. 日志会在 VSCode 终端中实时显示

### 方法二：通过菜单

1. 点击 VSCode 菜单栏：**Terminal** → **Run Task...**
2. 选择 **`监控Unity日志`**

### 方法三：快捷键（可选配置）

你可以在 VSCode 的 `keybindings.json` 中添加快捷键：

```json
{
  "key": "cmd+shift+u",
  "command": "workbench.action.tasks.runTask",
  "args": "监控Unity日志"
}
```

## ⏹️ 停止监控

在 VSCode 终端中按 **`Ctrl+C`** 即可停止日志监控。

## 📊 日志内容示例

```bash
Actor: 角色.法师.奥露娜, Events Count: 2
UnityEngine.Debug:Log (object)
GameStateSync/<FetchSessionMessagesFromServer>d__17:MoveNext () 
(at Assets/MyGame/Scripts/UI/GameStateSync.cs:370)

(Filename: Assets/MyGame/Scripts/UI/GameStateSync.cs Line: 370)
```

## 📍 日志文件位置

Unity Editor 的日志文件位置：

- **当前会话**: `~/Library/Logs/Unity/Editor.log`
- **上一次会话**: `~/Library/Logs/Unity/Editor-prev.log`

## 💡 使用技巧

### 1. 过滤特定内容

在终端中使用 grep 过滤：

```bash
tail -f ~/Library/Logs/Unity/Editor.log | grep "Error"
```

### 2. 只看自己的代码日志

```bash
tail -f ~/Library/Logs/Unity/Editor.log | grep "Assets/MyGame"
```

### 3. 保存日志到文件

```bash
tail -f ~/Library/Logs/Unity/Editor.log > unity_debug.log
```

## 🔧 自定义配置

你可以在 `.vscode/tasks.json` 中添加更多自定义任务，例如：

```json
{
  "label": "监控Unity错误日志",
  "type": "shell",
  "command": "tail -f ~/Library/Logs/Unity/Editor.log | grep -i error",
  "problemMatcher": [],
  "presentation": {
    "reveal": "always",
    "panel": "dedicated"
  },
  "isBackground": true
}
```

## ❓ 常见问题

### Q: 看不到最新的日志？

**A**: 确保 Unity Editor 正在运行，并且已经有日志输出。

### Q: 日志太多看不过来？

**A**: 使用 `grep` 命令过滤特定内容，或者使用 "查看Unity日志(最后100行)" 任务。

### Q: 如何查看之前的日志？

**A**: 使用 `less ~/Library/Logs/Unity/Editor.log` 命令浏览完整日志文件。

### Q: 终端显示乱码？

**A**: 确保你的终端字符编码设置为 UTF-8。

## 🎉 总结

现在你可以在 VSCode 中愉快地查看 Unity 日志了！无需频繁切换窗口，开发效率大大提升。

---

**配置日期**: 2025年12月3日  
**Unity版本**: Unity 6  
**系统要求**: macOS (其他系统需要调整日志文件路径)
