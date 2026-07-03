---
sidebar_position: 6
---

# 资源管理

> ⚠️ **重要提示：框架原有的 `AssetManager` 调度类已弃用，所有代码已被注释。推荐直接使用 Unity Addressables 原生 API 进行资源管理。**

## 架构定位

资源管理系统属于 **CapabilityModule（通用能力层）**，采用接口定义 — 调度实现 — 平台适配三层分离设计：

| 层次 | 位置 | 说明 |
|------|------|------|
| **接口定义** | `HN.Framework.Core.Capability` — `IAssetManager` | 纯 C# 接口，无 Unity 依赖 |
| **调度实现** | `HN.Framework.Unity.Capability.Asset` — `AssetManager` | 资源加载 / 缓存 / 引用计数调度，实现 `IAssetManager` |
| **平台适配** | `HN.Framework.Unity.Driver.Platform` — `AddressablesOperator` 等 | 对接具体加载方式的操作器 |

### 架构关系

```
┌──────────────────────────────────────────────────────────────┐
│                    CapabilityModule                          │
│                                                              │
│  HN.Framework.Core.Capability                                │
│  ┌──────────────────────────────────────────────────────┐    │
│  │  IAssetManager (interface)                           │    │
│  │  · LoadSceneGroup / PreloadSceneGroup                │    │
│  │  · LoadAsset / ReleaseAsset / IsLoaded               │    │
│  │  · GetReferenceCount / GetGroupProgress              │    │
│  │  · Tick / LateTick                                   │    │
│  │  纯 C# 接口，无 UnityEngine 依赖                       │    │
│  └──────────────────────────────────────────────────────┘    │
│                          ▲                                    │
│                          │ implements                         │
│  HN.Framework.Unity.Capability.Asset                         │
│  ┌──────────────────────────────────────────────────────┐    │
│  │  AssetManager : IAssetManager, ITickable             │    │
│  │  资源加载 / AssetCache / 引用计数 / 自动卸载          │    │
│  │  内部持有 IAssetOperator 引用，由平台适配层注入         │    │
│  └──────────────────────────────────────────────────────┘    │
│                          │                                    │
│                          │ uses                               │
│  HN.Framework.Unity.Driver.Platform                          │
│  ┌──────────────────────────────────────────────────────┐    │
│  │  IAssetOperator 实现                                  │    │
│  │  ├── AddressablesOperator    ✅ 运行时唯一推荐路径     │    │
│  │  ├── AssetDatabaseOperator   🎯 Editor 环境专用        │    │
│  │  └── ResourcesOperator       ⚠️ 已标记 deprecated      │    │
│  └──────────────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────────────┘
```

### IAssetManager 接口

`IAssetManager` 定义于 `HN.Framework.Core.Capability` 命名空间，是资源管理系统的顶层抽象。它提供资源组标签加载、单资源加载、引用计数查询、加载进度追踪以及 Tick 生命周期管理。该接口完全独立于 Unity 引擎，可在纯 C# 逻辑层直接依赖。

### AssetManager 调度器

`AssetManager` 类位于 `HN.Framework.Unity.Capability.Asset` 命名空间，实现 `IAssetManager` 接口。它负责统一的加载调度、弱引用缓存（`AssetCacheItem`）、引用计数跟踪以及超时自动卸载。内部通过 `IAssetOperator` 接口委派实际的资源加载操作。

### 平台操作器

`AddressablesOperator`、`AssetDatabaseOperator`、`ResourcesOperator` 位于 `HN.Framework.Unity.Driver.Platform` 命名空间，是 `IAssetOperator` 接口的平台实现。三者均从 `GameWorldDriver` 的初始化流程中注入 `AssetManager`。其中 `AddressablesOperator` 是运行时资源加载的唯一推荐路径。

## 推荐方案：直接使用 Addressables

### 基本用法

```csharp
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// 异步加载资源
AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("MyPrefab");
handle.Completed += (op) =>
{
    if (op.Status == AsyncOperationStatus.Succeeded)
    {
        Instantiate(op.Result);
    }
};

// 释放资源
Addressables.Release(handle);
```

### 加载场景

```csharp
Addressables.LoadSceneAsync("MainScene");
```

### 按 Label 批量加载

```csharp
Addressables.LoadAssetsAsync<GameObject>("enemies", (obj) =>
{
    // 每个资源加载完成时回调
});
```

## 框架旧版设计（仅作参考）

虽然 `AssetManager` 已弃用，但其底层设计思路仍有参考价值：

### IAssetOperator 接口

```csharp
public interface IAssetOperator : IReference
{
    Object LoadAsset(string name);
    AsyncLoadHandle LoadAssetAsync<T>(string name) where T : Object;
    void ReleaseAsset(Object asset);
}
```

### 三种操作器实现

| 实现类 | 加载方式 | 编译条件 | 说明 |
|--------|---------|---------|------|
| `AssetDatabaseOperator` | `AssetDatabase.LoadAssetAtPath` | `UNITY_EDITOR` | Editor 专用，仅同步 |
| `AddressablesOperator` | `Addressables.LoadAssetAsync` | 全平台 | 仅异步 |
| `ResourcesOperator` | `Resources.Load` / `LoadAsync` | 全平台 | 同步+异步 |

### AsyncLoadHandle — 统一异步句柄

```csharp
public class AsyncLoadHandle
{
    public Object Target { get; }
    public bool IsDone { get; }
    public float PercentComplete { get; }
    public AsyncLoadStatus Status { get; }  // Progressing/Succeeded/Failed
    public Action CompletedEvent { get; set; }
}
```

### AssetCacheItem — 弱引用缓存

```csharp
public class AssetCacheItem : IAssetCacheItem
{
    public string AssetName { get; }
    public Object Asset { get; }    // 通过 WeakReference 获取
    public bool IsAlive { get; }    // 弱引用是否仍存活
}
```

## 编辑器工具

**HN Unity Framework → Update Addressable Groups**：根据预设配置自动创建/更新 Addressable Groups，将 `Assets/Project/RuntimeAssets` 下的资源按路径关键字分配到对应 Group。

## 迁移建议

如果之前使用了框架的 `AssetManager`：

1. 将 `AssetManager.Load("path")` 替换为 `Addressables.LoadAssetAsync<T>("key")`
2. 将 `AssetManager.Release(obj)` 替换为 `Addressables.Release(obj)`
3. 使用 Update Addressable Groups 工具管理资源分组
