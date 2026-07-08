# Unity 资源销毁：Destroy vs DestroyImmediate

## 核心结论 🎯

| 对象类型 | 运行时推荐 | 编辑器 | 是否需要 `true` 参数 |
| --------- | ----------- | ------- | --------------------- |
| **GameObject** | `Destroy()` | `DestroyImmediate()` | ❌ 不需要 |
| **Component** | `Destroy()` | `DestroyImmediate()` | ❌ 不需要 |
| **动态 Texture2D** | `DestroyImmediate(texture, true)` | `DestroyImmediate(texture, true)` | ✅ 必须 `true` |
| **动态 Sprite** | `DestroyImmediate(sprite, true)` | `DestroyImmediate(sprite, true)` | ✅ 必须 `true` |
| **动态 Material** | `DestroyImmediate(material, true)` | `DestroyImmediate(material, true)` | ✅ 必须 `true` |

---

## 1. 基本概念

### `Destroy(object)`

- **延迟销毁**：在当前帧结束时销毁
- **安全性高**：不会打断当前执行流程
- **限制**：只能销毁场景对象（GameObject/Component），**不能销毁资源（Asset）**

### `DestroyImmediate(object, allowDestroyingAssets)`

- **立即销毁**：调用后立即销毁
- **危险性**：可能打断执行流程（如在 foreach 中销毁会出错）
- **灵活性**：第二个参数为 `true` 时可以销毁资源文件

---

## 2. 关键区别对比

| 特性 | `Destroy()` | `DestroyImmediate()` |
| ----- | ------------ | --------------------- |
| **销毁时机** | 帧结束时 | 立即 |
| **场景对象** | ✅ 支持 | ✅ 支持 |
| **资源文件** | ❌ 不支持 | ✅ 支持（需要 `true`） |
| **运行时安全性** | 高 | 中（需谨慎） |
| **遍历中使用** | ✅ 安全 | ❌ 可能出错 |
| **编辑器专用** | ❌ 否 | ⚠️ 推荐但非强制 |

---

## 3. 第二个参数 `allowDestroyingAssets` 详解

```csharp
DestroyImmediate(object, allowDestroyingAssets)
```

### `false` 或省略（默认）

- 只能销毁**场景实例**（Scene Objects）
- 不能销毁**资源文件**（Assets）

### `true`

- 可以销毁**资源文件**，包括：
  - 从 Resources 加载的资源
  - 运行时动态创建的资源（Texture2D、Sprite、Material 等）
  - 从 AssetBundle 加载的资源

---

## 4. 使用场景和最佳实践

### ✅ GameObject/Component（场景对象）

```csharp
// ✅ 正确：运行时销毁 GameObject
foreach (Transform child in container.transform)
{
    Destroy(child.gameObject);  // 延迟销毁，安全
}

// ✅ 正确：编辑器中销毁（如 Editor 脚本）
#if UNITY_EDITOR
DestroyImmediate(child.gameObject);
#endif
```

### ✅ 动态创建的资源（Texture/Sprite/Material）

```csharp
// ✅ 正确：销毁动态创建的 Sprite 和 Texture
var sprite = SpriteManager.Instance.GetSprite(key);
if (sprite != null)
{
    var texture = sprite.texture;
    DestroyImmediate(sprite, true);      // 必须用 true
    DestroyImmediate(texture, true);     // 必须用 true
}

// ❌ 错误：Destroy() 不能销毁资源
Destroy(sprite);   // 无效！内存泄漏！
```

---

## 5. 常见错误和解决方案

### ❌ 错误 1：用 `Destroy()` 销毁资源

```csharp
// ❌ 错误：资源不会被销毁，导致内存泄漏
Texture2D texture = new Texture2D(512, 512);
Destroy(texture);  // 无效！

// ✅ 正确：
DestroyImmediate(texture, true);
```

### ❌ 错误 2：只销毁 Sprite 不销毁 Texture

```csharp
// ❌ 错误：Texture 仍然占用内存
var sprite = Sprite.Create(texture, ...);
DestroyImmediate(sprite, true);  // Texture 没被销毁！

// ✅ 正确：
var texture = sprite.texture;
DestroyImmediate(sprite, true);
DestroyImmediate(texture, true);  // 两者都要销毁
```

### ❌ 错误 3：运行时用 `DestroyImmediate` 销毁 GameObject

```csharp
// ⚠️ 不推荐：可能导致问题（如在 Update 中调用）
DestroyImmediate(gameObject);

// ✅ 推荐：
Destroy(gameObject);  // 更安全
```

### ❌ 错误 4：在遍历中用 `DestroyImmediate`

```csharp
// ❌ 错误：会导致索引错误
foreach (Transform child in transform)
{
    DestroyImmediate(child.gameObject);  // 危险！
}

// ✅ 正确方法 1：用 Destroy
foreach (Transform child in transform)
{
    Destroy(child.gameObject);
}

// ✅ 正确方法 2：倒序销毁
for (int i = transform.childCount - 1; i >= 0; i--)
{
    DestroyImmediate(transform.GetChild(i).gameObject);
}
```

---

## 6. 内存管理关键要点

### Sprite 和 Texture 的关系

```csharp
// Sprite 只是 Texture 的引用包装
Sprite sprite = Sprite.Create(texture, rect, pivot);

// sprite.texture 指向原始 texture
// 销毁 Sprite 不会自动销毁 Texture！
```

### 完整的清理流程

```csharp
public void RemoveSprite(string key)
{
    if (spriteCache.TryGetValue(key, out Sprite sprite))
    {
        spriteCache.Remove(key);
        
        if (sprite != null)
        {
            // ⚠️ 关键顺序：先获取 texture 引用
            var texture = sprite.texture;
            
            // 1️⃣ 销毁 Sprite
            DestroyImmediate(sprite, true);
            
            // 2️⃣ 销毁 Texture
            if (texture != null)
            {
                DestroyImmediate(texture, true);
            }
        }
    }
}
```

---

## 7. 项目中的实际应用

### 本项目中的正确用法

#### ✅ SpriteManager.cs

```csharp
// 清空缓存：销毁所有 Sprite 和 Texture
public void ClearCache()
{
    foreach (var sprite in spriteCache.Values)
    {
        if (sprite != null)
        {
            var texture = sprite.texture;
            DestroyImmediate(sprite, true);    // ✅ 动态资源
            if (texture != null)
            {
                DestroyImmediate(texture, true); // ✅ 动态资源
            }
        }
    }
    spriteCache.Clear();
}
```

#### ✅ ImageDisplayController.cs

```csharp
// 清理旧的 Sprite
if (_targetImage.sprite != null)
{
    DestroyImmediate(_targetImage.sprite, true);  // ✅ 动态 Sprite
    _targetImage.sprite = null;
}
```

#### ✅ DungeonCombatScene.cs

```csharp
// 清除容器的所有子对象
foreach (Transform child in container.transform)
{
    Destroy(child.gameObject);  // ✅ GameObject，用 Destroy
}
```

---

## 8. 决策流程图

```text
需要销毁对象？
    ↓
    ├─ GameObject/Component？
    │   ↓
    │   ├─ 运行时 → 使用 Destroy(obj)
    │   └─ 编辑器 → 使用 DestroyImmediate(obj)
    │
    └─ 资源文件（Texture/Sprite/Material）？
        ↓
        └─ 动态创建的？
            ↓
            ├─ 是 → DestroyImmediate(obj, true)  // 运行时和编辑器都用
            └─ 否（从 Project 加载）→ Resources.UnloadAsset(obj) 或不销毁
```

---

## 9. Unity 官方建议

> **运行时（Runtime）：**
>
> - GameObject/Component：使用 `Destroy()`
> - 动态资源：使用 `DestroyImmediate(obj, true)`
>
> **编辑器脚本（Editor Scripts）：**
>
> - 所有情况都可以使用 `DestroyImmediate()`
> - 资源文件需要添加 `true` 参数

---

## 10. 快速参考

### 常用代码片段

```csharp
// 销毁 GameObject
Destroy(gameObject);

// 销毁 Component
Destroy(GetComponent<Collider>());

// 销毁动态 Texture
DestroyImmediate(texture, true);

// 销毁 Sprite（包括其 Texture）
var texture = sprite.texture;
DestroyImmediate(sprite, true);
DestroyImmediate(texture, true);

// 延迟销毁
Destroy(gameObject, 2f);  // 2秒后销毁

// 清空容器（安全方式）
foreach (Transform child in container.transform)
{
    Destroy(child.gameObject);
}
```

---

## 11. 总结

### 记住三个原则

1. **GameObject/Component**：运行时用 `Destroy()`，编辑器用 `DestroyImmediate()`
2. **动态资源**：必须用 `DestroyImmediate(obj, true)`
3. **Sprite + Texture**：两个都要销毁，否则内存泄漏

### 检查清单 ✓

- [ ] 销毁 GameObject 时使用了 `Destroy()`
- [ ] 销毁动态 Texture/Sprite 时使用了 `DestroyImmediate(..., true)`
- [ ] 销毁 Sprite 时同时销毁了其 `texture`
- [ ] 没有在 `foreach` 中使用 `DestroyImmediate`
- [ ] 资源管理器（如 SpriteManager）正确处理了资源生命周期

---

**创建日期：** 2026年1月3日  
**适用版本：** Unity 6 及以上  
**参考文档：** [Unity Scripting API - Object.Destroy](https://docs.unity3d.com/ScriptReference/Object.Destroy.html)
