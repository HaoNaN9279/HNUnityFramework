---
sidebar_position: 15
---

# 视图工厂和 EntityView

ViewModule 位于架构的 Level.View 层，为 Unity 依赖的视图层基础设施。提供 Entity 的视觉表示创建、数据绑定和生命周期管理。

## 架构位置

```
Level/
└── View/
    ├── ViewFactory.cs              # 工厂（已实现）
    ├── EntityView.cs               # 实体视图基类（已实现）
    └── Binding/
        ├── PropertyBinder.cs       # ✅ 抽象绑定器
        └── DefaultPropertyBinder.cs # ✅ 默认实现
```

> 所有类型依赖 UnityEngine，不可在 Core 层使用。

## 核心类型

| 类型 | 说明 |
|------|------|
| `EntityView` | 实体视图基类（MonoBehaviour），场景中可视化表示的根。提供 `OnSpawned`/`OnDespawned` 生命周期 |
| `ViewFactory` | 视图工厂类，从预制体创建/回收 EntityView |
| `PropertyBinder` | 属性绑定器抽象类，`Bind<T>()` / `UnbindAll()` |
| `DefaultPropertyBinder` | PropertyBinder 的默认实现，基于 Dictionary 管理绑定关系 |

## EntityView

### 生命周期

```
ViewFactory.CreateView()
  → Instantiate(prefab)           ← 实例化 GameObject
  → GetComponent<EntityView>()    ← 获取 EntityView 组件
  → EntityView.Initialize(id, defId)
      → Set EntityId, EntityDefId
      → Create DefaultPropertyBinder
      → virtual OnSpawned()       ← 子类重写此方法
  ↓
...游戏运行中...
  ↓
ViewFactory.ReleaseView()
  → EntityView.Deinitialize()
      → virtual OnDespawned()      ← 子类重写此方法
      → Binder.UnbindAll()
      → Reset EntityId, EntityDefId
  → Destroy(gameObject)
```

### 属性

```csharp
public class EntityView : MonoBehaviour
{
    public uint EntityId { get; private set; }
    public int EntityDefId { get; private set; }
    protected PropertyBinder Binder { get; private set; }
}
```

### 虚方法

```csharp
protected virtual void OnSpawned() { }    // 视图生成时调用
protected virtual void OnDespawned() { }  // 视图回收时调用
```

### 数据绑定

通过 PropertyBinder 将 IReadOnlyModel 绑定到 View 更新回调：

```csharp
public class PlayerView : EntityView
{
    [SerializeField] private Text healthText;

    protected override void OnSpawned()
    {
        // Binder 在 Initialize 中自动创建
    }

    public void BindHealth(IReadOnlyModel<int> healthModel)
    {
        Binder.Bind(healthModel, value =>
        {
            healthText.text = $"HP: {value}";
        });
    }
}
```

## ViewFactory

### API

```csharp
public class ViewFactory
{
    // 注册 EntityDefId → 预制体地址的映射
    void RegisterPrefabMapping(int entityDefId, string prefabAddress);

    // 缓存预制体
    void CachePrefab(string address, GameObject prefab);
    GameObject GetCachedPrefab(string address);

    // 从已加载的预制体创建视图
    EntityView CreateView(GameObject prefab, Vector3 position,
                          Quaternion rotation, int entityDefId,
                          Transform parent = null);

    // 释放并销毁视图
    void ReleaseView(EntityView view);
}
```

### 用法

```csharp
// 初始化工厂
var factory = new ViewFactory();

// 注册实体类型 → 预制体映射
factory.RegisterPrefabMapping(1001, "Assets/Prefabs/Player.prefab");

// 缓存已加载的预制体
var prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Player.prefab").WaitForCompletion();
factory.CachePrefab("Assets/Prefabs/Player.prefab", prefab);

// 创建视图
var view = factory.CreateView(prefab, spawnPos, spawnRot, 1001);

// 释放视图
factory.ReleaseView(view);
```

### 配合 EntityManager

```csharp
// 在 Unity 层监听 Entity 生成事件
world.EventBus.Subscribe<EntitySpawnedEvent>(evt =>
{
    string address = GetPrefabAddress(evt.EntityDefId);
    var prefab = factory.GetCachedPrefab(address);
    if (prefab != null)
    {
        var view = factory.CreateView(prefab, Vector3.zero, Quaternion.identity, evt.EntityDefId);
        // 将 EntityView 与 Controller 绑定...
    }
});

world.EventBus.Subscribe<EntityDespawnedEvent>(evt =>
{
    // 查找对应的 EntityView 并释放
    // factory.ReleaseView(view);
});
```

## DefaultPropertyBinder

`DefaultPropertyBinder` 是 `PropertyBinder` 的默认实现，在 `EntityView.Initialize()` 时自动创建：

```csharp
public sealed class DefaultPropertyBinder : PropertyBinder
{
    public override void Bind<T>(IReadOnlyModel<T> source, Action<T> onValueChanged);
    public override void UnbindAll();
}
```

行为：
- `Bind<T>()`：立即订阅 `source.OnValueChanged` + 立即回调 `onValueChanged(source.Value)` 初始化
- `UnbindAll()`：批量取消所有订阅并清空映射表
- EntityView.Deinitialize() 自动调用 UnbindAll()，无需手动清理

## 注意事项

- 预制体上**必须挂载 EntityView 组件**（或其子类），否则 CreateView 返回 null
- ViewFactory 的异步加载 `CreateViewAsync` 暂未实现，待 `IAssetManager` 扩展后补完
- EntityView 的 `EntityId` 为 0 表示尚未与 Entity 关联（默认分配）
- 如需自定义 Binder 行为，在 `OnSpawned()` 中替换 Binder 实例