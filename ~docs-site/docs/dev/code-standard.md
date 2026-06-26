---
sidebar_position: 4
---

# 代码规范

> 📝 待补充：代码编写规范。

## 命名规范

{/* TODO: 添加命名规范 */}

## 注释规范

C# 代码必须使用 XML 文档注释（`<summary>` 标签），用于自动生成 API 文档。

```csharp
/// <summary>
/// 对象池管理器，负责所有对象池的生命周期管理。
/// </summary>
public class ObjectPoolManager
{
    /// <summary>
    /// 从池中获取指定类型的对象。
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <returns>池中的对象实例</returns>
    public T Get<T>() where T : class, new() { ... }
}
```

## 最佳实践

{/* TODO: 添加最佳实践 */}
