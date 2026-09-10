# 模型同步规范：C# ↔ Python

## 定位

- 权威源：`server-code/models/`（Python）。**单向同步 Python → C#**，目标是 `Assets/_Project/_Shared/Models/`。
- `server-code/models/` 是后端快照，真实后端更大（依赖 `..entitas` 等，本仓库不含）。

## 指导思想

1. **字段名逐字符一致**：C# 字段名 = Python 字段名（均 snake_case）。JSON 序列化依赖字段名精确匹配，禁止改写成 PascalCase。
2. **枚举值逐字符一致**：`[EnumMember(Value = "...")]` 的字符串与 Python StrEnum 的值完全相同（不同枚举大小写风格不统一，照抄）。
3. **只同步 DTO / 纯数据**：后端逻辑（validator、registry、factory、generation 等）不搬到 C#。
4. **方法名例外**：字段保持 snake_case，但 C# 方法名/参数用 PascalCase（仅 `utils` 这类函数映射场景）。
5. **来源可追溯**：每个 C# 文件顶部保留 `// 对应 Python models/xxx.py` 注释，作为对应关系的唯一依据（文件名本身不一定一一对应）。

## 步骤

1. 读 `server-code/models/` 与 `Assets/_Project/_Shared/Models/` 两侧，找出后端变更。
2. 只改 C# 侧数据定义。
3. 自检后提醒在 Unity 编译验证。

## 常用手段

- 对照两侧目录逐个 diff 字段 / 枚举 / 默认值。
- 对应关系以文件顶部注释为准，不靠猜文件名。
- 类型与默认值写法沿用项目现有 C# 文件里的惯用映射（如 `Dict[str, Any]` → `JObject`、集合 → `new()`），保持风格一致。
