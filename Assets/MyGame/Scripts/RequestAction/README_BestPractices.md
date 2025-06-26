# Unity 6 HTTP 请求最佳实践指南

## 概述

这份指南基于你当前的代码，提供了针对 Unity 6 和 WebGL 平台的 HTTP 请求最佳实践。

## 主要改进点

### 1. 资源管理
```csharp
// ❌ 旧版本 - 可能导致内存泄漏
UnityWebRequest request = UnityWebRequest.Get(url);
// ... 使用请求
// 忘记释放资源

// ✅ 新版本 - 自动资源管理
using (var request = UnityWebRequest.Get(url))
{
    // ... 使用请求
} // 自动释放资源
```

### 2. 错误处理
```csharp
// ❌ 旧版本 - 简单的错误检查
if (request.result == UnityWebRequest.Result.ConnectionError || 
    request.result == UnityWebRequest.Result.ProtocolError)
{
    Debug.LogError(request.error);
    yield break;
}

// ✅ 新版本 - 详细的错误分类
private RequestResult ProcessResponse(UnityWebRequest request)
{
    switch (request.result)
    {
        case UnityWebRequest.Result.ConnectionError:
            return new RequestResult(false, "", request.responseCode, $"Connection Error: {request.error}");
        case UnityWebRequest.Result.ProtocolError:
            return new RequestResult(false, request.downloadHandler?.text ?? "", request.responseCode, $"Protocol Error: {request.error}");
        case UnityWebRequest.Result.DataProcessingError:
            return new RequestResult(false, "", request.responseCode, $"Data Processing Error: {request.error}");
        case UnityWebRequest.Result.Success:
            return new RequestResult(true, request.downloadHandler?.text ?? "", request.responseCode);
        default:
            return new RequestResult(false, "", request.responseCode, "Unknown error");
    }
}
```

### 3. 超时和重试机制
```csharp
// ✅ 设置超时时间
request.timeout = 30; // 30秒超时

// ✅ 重试机制
for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
{
    // ... 发送请求
    if (result.isSuccess) break;
    
    if (attempt < maxRetryAttempts - 1)
    {
        await Task.Delay((int)(retryDelay * 1000));
    }
}
```

### 4. WebGL 兼容性
```csharp
// ✅ WebGL 特殊处理
private void SetCommonHeaders(UnityWebRequest request)
{
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Accept", "application/json");
    
    #if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL 中避免某些可能被浏览器阻止的头部
    #else
    request.SetRequestHeader("User-Agent", $"Unity-{Application.unityVersion}");
    #endif
}

// ✅ WebGL 网络检查
public static bool IsNetworkReachable()
{
    #if UNITY_WEBGL && !UNITY_EDITOR
    return true; // WebGL 中总是假设有网络
    #else
    return Application.internetReachability != NetworkReachability.NotReachable;
    #endif
}
```

## Unity 6 推荐使用 Async/Await

Unity 6 对 async/await 有更好的支持，推荐使用：

```csharp
// ✅ Unity 6 推荐写法
public async Task<RequestResult> GetDataAsync(string url)
{
    using (var request = UnityWebRequest.Get(url))
    {
        var operation = request.SendWebRequest();
        
        while (!operation.isDone)
        {
            await Task.Yield(); // 让出控制权给Unity主线程
        }
        
        return ProcessResponse(request);
    }
}
```

## 使用建议

### 1. 项目迁移策略
- 保留现有的协程接口以保证兼容性
- 新功能优先使用 async/await
- 逐步迁移关键功能到新的实现

### 2. WebGL 注意事项
- 避免设置可能被浏览器阻止的请求头
- 不要依赖 `Application.internetReachability`
- 考虑 CORS 限制

### 3. 性能优化
- 使用对象池管理请求对象（如果频繁请求）
- 合理设置超时时间
- 实现请求缓存机制

### 4. 调试和监控
- 添加详细的日志记录
- 实现请求统计和监控
- 在开发阶段启用更详细的错误信息

## 配置示例

在 Inspector 中配置请求参数：

```csharp
[Header("网络配置")]
[SerializeField] private float requestTimeout = 30f;
[SerializeField] private int maxRetryAttempts = 3;
[SerializeField] private float retryDelay = 1f;
[SerializeField] private bool useAsyncVersion = true;
```

## 错误处理最佳实践

1. **分类处理错误**：区分网络错误、服务器错误和数据处理错误
2. **用户友好的错误消息**：不要直接显示技术错误信息给用户
3. **错误重试策略**：网络错误可以重试，但认证错误不应该重试
4. **错误上报**：在生产环境中收集错误信息用于改进

## 测试建议

1. **网络环境测试**：测试不同网络条件（慢网络、断网等）
2. **WebGL 专项测试**：在真实的 Web 环境中测试
3. **错误场景测试**：模拟服务器错误、超时等情况
4. **性能测试**：监控内存使用和请求响应时间
