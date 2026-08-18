# Unity 项目结构与命名规范笔记

> 整理自 2026-06-25 的讨论，供日后复盘参考。

---

## 一、Assets/ 目录的三类资源归属

| 来源 | 推荐位置 | 说明 |
| ------ | ---------- | ------ |
| **项目自身资源** | `Assets/AIRPG/` | 自由组织 |
| **Unity 官方包**（TextMesh Pro、URP 等） | `Packages/`（通过 Package Manager） | 不应手动放在 Assets/；旧式导入才在 `Assets/TextMesh Pro/` |
| **第三方/外部包**（WebGLSupport 等） | `Assets/ThirdParty/` 或 `Assets/Plugins/` | 隔离存放，不修改内部结构 |

---

## 二、`Plugins/` 与 `ThirdParty/` 的本质区别

| | `Plugins/` | `ThirdParty/`（自定义名称） |
| ------ | ------------ | -------------------------- |
| **性质** | Unity 引擎保留的**特殊目录** | 团队自定义的**约定目录** |
| **用途** | 存放 Native 二进制插件：`.dll`、`.so`、`.dylib`、`.aar`、`.jslib` | 存放纯 C# 的第三方资源包 |
| **编译顺序** | 第一轮编译（先于普通脚本） | 普通顺序 |
| **平台识别** | `Plugins/Android/`、`Plugins/iOS/`、`Plugins/WebGL/` 自动识别 | 无特殊处理 |

> **判断依据**：包里有 `.jslib`/`.dll`/`.aar` 等 Native 文件 → `Plugins/`；纯 C# + 资源 → `ThirdParty/`

---

## 三、特殊文件夹的处理原则

**核心规则：Unity 特殊文件夹按名称在任意层级生效，不必放在根目录。**

```text
Assets/AIRPG/Editor/          ← 生效 ✅
Assets/ThirdParty/Foo/Editor/ ← 同样生效 ✅
```

### 推荐做法：特殊文件夹跟随各自包

```text
Assets/
├── AIRPG/
│   ├── Editor/          ← 项目的 Editor 脚本
│   ├── Resources/       ← 项目的 Resources
│   └── Documentation/   ← 项目的文档
└── ThirdParty/
    └── SomePlugin/
        ├── Editor/      ← 插件的 Editor 脚本
        └── Resources/   ← 插件的 Resources
```

根目录的 `Editor/`、`Resources/` 只留真正**全局共享**的内容。

### `Tests/` 必须配合 `.asmdef`

```text
AIRPG/
└── Tests/
    ├── EditMode/
    │   ├── AIRPG.Tests.EditMode.asmdef  ← 必须有，否则测试代码进入正式包
    │   └── MyTest.cs
    └── PlayMode/
        ├── AIRPG.Tests.PlayMode.asmdef
        └── MyTest.cs
```

---

## 四、命名规范速查

| 资产类型 | 风格 | 示例 |
| ---------- | ------ | ------ |
| **Scene** | PascalCase | `BattleStage_Forest.unity` |
| **MonoBehaviour** | PascalCase，无冗余后缀 | `PlayerController.cs` |
| **ScriptableObject 类** | PascalCase + 语义后缀 | `CharacterData.cs`、`WeaponConfig.cs` |
| **ScriptableObject 实例** | 实例名 + 类名 | `Warrior_CharacterData.asset` |
| **精灵图片** | 小写下划线 + 类别前缀 | `chr_warrior_idle.png`、`ui_btn_confirm.png` |
| **Sprite Atlas** | PascalCase | `UI_Icons.spriteatlas` |

> Unity 要求：**文件名必须与类名完全一致**（MonoBehaviour / ScriptableObject）

---

## 五、按功能组织 vs 按类型组织

### 旧方式（按类型）—— 不推荐用于中大型项目

```text
AIRPG/
├── Scripts/   ← 所有脚本堆在一起
├── Scenes/    ← 所有场景
└── Prefabs/   ← 所有预制体
```

### 新方式（按功能）—— 现代 Unity 推荐

```text
AIRPG/
├── Battle/                  ← 战斗模块（Scene + Script + Prefab + Sprite 聚合）
│   ├── BattleScene.unity
│   ├── BattleManager.cs
│   ├── ActionOrderObject.cs
│   └── Prefabs/
├── Town/                    ← 城镇模块
│   ├── TownScene.unity
│   └── NPCController.cs
├── MainMenu/                ← 主菜单模块
└── _Shared/                 ← 跨模块共享（下划线前缀置顶）
    ├── Scripts/
    │   ├── GameUtils.cs
    │   └── SpriteCacheManager.cs
    └── ScriptableObjects/
```

### `_Shared/` 的判断标准

> 一个文件**被 2 个及以上功能模块引用**，才移入 `_Shared/`。

### 参考项目

Unity 官方示例（Boss Room、Dragon Crashers）均采用按功能分组方式。

---

## 六、本项目对照检查

| 当前状态 | 建议 |
| ---------- | ------ |
| `Assets/TextMesh Pro/`（旧式导入） | 条件允许时改为 Package Manager 引用 |
| `Assets/WebGLSupport/` | 移至 `Assets/ThirdParty/WebGLSupport/`（若含 `.jslib` 则内部建 `Plugins/WebGL/`） |
| `Assets/AIRPG/Scripts/UI/ActionOrderObject.cs` | 按功能重组后可移至 `Assets/AIRPG/Battle/UI/ActionOrderObject.cs` |
| `Assets/Editor/`、`Assets/Resources/` 若存在 | 评估是否真正全局共享，否则下沉到各功能模块 |

## 补充

AIRPG/
├── Battle/                      # 战斗功能模块
│   ├── BattleScene.unity
│   ├── BattleManager.cs
│   ├── ActionOrderObject.cs     ← 你的文件就在这里
│   ├── TurnSystem.cs
│   ├── Prefabs/
│   │   ├── EnemyPrefab.prefab
│   │   └── SkillEffect.prefab
│   └── Sprites/
│       └── chr_warrior_idle.png
│
├── Town/                        # 城镇功能模块
│   ├── TownScene.unity
│   ├── NPCController.cs
│   ├── ShopSystem.cs
│   └── Prefabs/
│
├── MainMenu/                    # 主菜单模块
│   ├── MainMenuScene.unity
│   ├── MainMenuUI.cs
│   └── Sprites/
│
└── _Shared/                     # 跨模块共享（加下划线置顶）
    ├── Scripts/
    │   ├── GameUtils.cs
    │   └── SpriteCacheManager.cs
    ├── ScriptableObjects/
    │   └── CharacterData.asset
    └── Prefabs/
