---
sidebar_position: 6
---

# 资源管理

> ⚠️ **重要提示：框架原有的 `AssetManager` 调度类已弃用，所有代码已被注释。推荐直接使用 Unity Addressables 原生 API 进行资源管理。**

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
