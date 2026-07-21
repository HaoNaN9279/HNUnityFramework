---
sidebar_position: 4
---

# 代码规范

本文档定义了 HNUnityFramework 项目的代码编写和注释规范。

## 命名规范

### 命名空间

| 命名空间 | 适用代码 |
|---------|---------|
| `HN.Framework.Core.Driver` | GameWorld, GameWorld 基础类库 |
| `HN.Framework.Core.Driver.Common` | ITickable, IReference, HNLogicTime, 序列化, 池系统 |
| `HN.Framework.Core.Capability` | Capability 接口定义 (IAssetManager, ILogProvider, INetworkManager, IStorageProvider, ProcedureManager, ObjectPoolManager) |
| `HN.Framework.Core.Level.Logic` | MVC, HFSM, Entity |
| `HN.Framework.Unity.Driver.Platform` | Unity 平台适配 (GameWorldDriver, AddressablesOperator, GameObjectPool, UnityLogProvider 等) |
| `HN.Framework.Unity.Capability` | FishNet 封装, Sheet 运行时 |
| `HN.Framework.Unity.Level.View` | ViewFactory, EntityView, PropertyBinder |
| `HN.Framework.Editor` | Editor 工具 |

### 标识符

| 元素 | 风格 | 示例 |
|------|------|------|
| 类 / 结构体 | PascalCase | `ObjectPoolManager`, `HFSMState` |
| 接口 | PascalCase + `I` 前缀 | `ITickable`, `IReference`, `IAssetOperator` |
| 方法 | PascalCase | `Initialize()`, `ClearAll()` |
| 公共属性 | PascalCase | `CurrentState`, `LogicFrameCount` |
| 私有字段 | camelCase | `currentState`, `instance` |
| 静态私有字段 | camelCase | `instance`, `managerRoot` |
| 实例私有字段 | camelCase | `objectPools`, `currentState` |
| 常量 | UPPER_SNAKE_CASE | `FRAMEWORK_NAME` |
| 参数 / 局部变量 | camelCase | `stateName`, `targetState` |

## 文件组织

- 一个文件包含一个主要类型
- 紧密相关的内部类型可放在同一文件（如 `ReferencePool` + `ReferenceCollection`）
- 接口和实现类可放在同一文件（如 `IHFSMState` + `HFSMState`）
- 文件名与主类型名一致

## XML 注释规范

所有公共 API **必须**使用 XML 文档注释，用于自动生成 API 文档。

### 必需标签

| 标签 | 用途 | 示例 |
|------|------|------|
| `<summary>` | 类型或成员的简要描述 | `<summary>对象池管理器。</summary>` |
| `<typeparam>` | 泛型类型参数说明 | `<typeparam name="T">池中对象类型。</typeparam>` |
| `<param>` | 方法参数说明 | `<param name="count">数量。</param>` |
| `<returns>` | 返回值说明 | `<returns>可用的对象实例。</returns>` |

### 可选标签

| 标签 | 用途 |
|------|------|
| `<remarks>` | 补充说明、使用示例、注意事项 |

### 示例

```csharp
/// <summary>
/// 对象池管理器，负责所有对象池的生命周期管理。
/// </summary>
/// <remarks>
/// 使用示例：
/// <code>
/// ObjectPoolManager.Initialize();
/// var pool = ObjectPoolManager.CreateObjectPool&lt;MyPool&gt;("MyPool");
/// </code>
/// </remarks>
public sealed class ObjectPoolManager : ITickable
{
    /// <summary>
    /// 初始化对象池管理器。
    /// </summary>
    /// <param name="managerRoot">管理器根节点，null 时自动创建。</param>
    public static void Initialize(GameObject managerRoot = null) { }

    /// <summary>
    /// 创建一个新的对象池。
    /// </summary>
    /// <typeparam name="T">对象池类型，必须继承 ObjectPoolBase。</typeparam>
    /// <param name="name">池名称，全局唯一标识。</param>
    /// <returns>创建的对象池实例。</returns>
    public static T CreateObjectPool<T>(string name) where T : ObjectPoolBase, new()
    {
        // ...
    }
}
```

## 泛型约束规范

合理使用泛型约束确保类型安全：

```csharp
// 必须有无参构造函数 + 引用类型 + IReference
public static T Acquire<T>() where T : class, IReference, new()

// 必须是 PooledObjectBase 子类 + 无参构造
public abstract class ObjectPool<T> where T : PooledObjectBase, new()

// 复合状态内部必须能创建子状态机
public class HFSMCompoundState<T> where T : HFSM, new()
```

## 最佳实践

- 优先使用现有类型和接口，避免重复造轮子
- 会被频繁创建/销毁的对象实现 `IReference`，通过 `ReferencePool` 管理
- 需要帧驱动的模块实现 `ITickable`，由框架主循环统一调度
- 公共 API 使用接口暴露，内部实现可自由替换
- 静态方法尽量无副作用，复杂逻辑放在实例方法中
