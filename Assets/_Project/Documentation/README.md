# Documentation 索引

本目录存放 Unity6Demo 的项目文档。本文是索引：agent 接手任务前先扫这里定位文档。

## 文档清单

| 文档 | 内容 |
| --- | --- |
| [模型同步规范-CSharp与Python.md](模型同步规范-CSharp与Python.md) | C# ↔ Python 模型同步的指导思想、步骤、常用手段 |
| [UnityProjectStructureNotes.md](UnityProjectStructureNotes.md) | 目录归属与命名规范 |
| [Unity工程结构与渲染配置说明.md](Unity工程结构与渲染配置说明.md) | URP 渲染配置、Settings 重要性、误删恢复 |
| [WebGL部署指南.md](WebGL部署指南.md) | WebGL gzip 部署（webgl-server.js） |
| [VSCode查看Unity日志配置说明.md](VSCode查看Unity日志配置说明.md) | .vscode/tasks.json 日志监控 |
| [Unity资源销毁-Destroy vs DestroyImmediate.md](Unity资源销毁-Destroy vs DestroyImmediate.md) | Destroy / DestroyImmediate 用法对照 |

## 写作约束（本目录所有文档共同遵守）

1. **不重复代码**：代码已表达的内容（字段、逻辑、流程、常量），文档不展开复述，直接指向文件路径，让 agent 自己读代码。
2. **只写决策与约束**：写"为什么这么做""必须遵守什么""例外是什么"；不写"是什么"的完整描述。
3. **信息密度高**：去掉过程叙述、套话、铺垫，一句话能说清就不用一段。
4. **可执行优先**：规则尽量写成 agent 能直接照做/自检的指令，而非论述。
