---
sidebar_position: 14
---

# Entity 实体系统

Entity 模块位于 **Core 层** 的 `HN.Framework.Core` 程序集中，属于 Level.Logic 逻辑层的核心基础设施。具体位置见 [架构文档](/dev/architecture)。

## 概述

Entity 系统是游戏逻辑层面的**纯数据实体**，承载实体核心身份信息（EntityId 与 EntityDefId），与 Unity 层的 GameObject/EntityView 分离，确保逻辑层零 Unity 依赖。

### 核心类型

| 类型 | 说明 |
|------|------|
| `Entity` | 纯数据实体基类，MemoryPack 可序列化，实现 `IReference` 支持对象池复用 |
| `EntityManager` | 实体生命周期管理器，通过 EventBus 发布生成/销毁事件 |
| `EntitySpawnedEvent` | 实体生成事件（readonly struct） |
| `EntityDespawnedEvent` | 实体销毁事件（readonly struct） |

## 快速开始

### 1. 获取 GameWorld

EntityManager 由 GameWorld 持有：

```csharp
var world = GetGameWorld(); // 通过 GameWorldDriver 获取
```

### 2. 生成实体

```csharp
// Spawn 一个实体，entityDefId 指向配置表定义
var entity = world.EntityManager.Spawn(1001);

// 实体具有唯一 ID
Debug.Log($"Entity spawned: id={entity.EntityId}, defId={entity.EntityDefId}");
```

### 3. 查询实体

```csharp
Entity entity = world.EntityManager.GetEntity(entityId);
if (entity != null)
{
    // 实体存在
}
```

### 4. 销毁实体

```csharp
world.EntityManager.Despawn(entity.EntityId);
// Despawn 后 GetEntity 返回 null
```

### 5. 监听实体生命周期事件

```csharp
world.EventBus.Subscribe<EntitySpawnedEvent>(evt =>
{
    Debug.Log($"Entity spawned: {evt.EntityId}, type={evt.EntityDefId}");
});

world.EventBus.Subscribe<EntityDespawnedEvent>(evt =>
{
    Debug.Log($"Entity despawned: {evt.EntityId}");
});
```

## 数据模型

### Entity

```csharp
[MemoryPackable]
public partial class Entity : IReference
{
    [MemoryPackOrder(0)]
    public uint EntityId { get; private set; }   // 运行时唯一 ID（自增）

    [MemoryPackOrder(1)]
    public int EntityDefId { get; private set; }  // 配置表定义 ID

    public void Clear();  // 归还对象池前重置
    internal void Initialize(uint id, int defId); // 仅供 EntityManager 调用
}
```

### EntityManager API

```csharp
public sealed class EntityManager
{
    public Entity Spawn(int entityDefId);          // 生成实体
    public void Despawn(uint entityId);            // 销毁实体
    public Entity GetEntity(uint entityId);        // 查询实体
    public int EntityCount { get; }                // 当前活跃数
}
```

### Entity 事件

```csharp
public readonly struct EntitySpawnedEvent
{
    public readonly uint EntityId;
    public readonly int EntityDefId;
}

public readonly struct EntityDespawnedEvent
{
    public readonly uint EntityId;
}
```

## Entity 生命周期

```
EntityManager.Spawn(defId)
  ↓
ReferencePool.Acquire<Entity>()      ← 从引用池获取或创建新实例
  → Entity.Initialize(id, defId)      ← 设置数据
  → EventBus.Publish(SpawnedEvent)    ← 广播生成事件（Unity 层监听后创建 View）
  → 返回 Entity
  ↓
...游戏运行中...
  ↓
EntityManager.Despawn(entityId)
  → m_entities.Remove(id)
  → Entity.Clear()                    ← 重置为默认值
  → ReferencePool.Release(entity)     ← 归还引用池
  → EventBus.Publish(DespawnedEvent)  ← 广播销毁事件（Unity 层监听后销毁 View）
```

## 与 ViewFactory 配合

Entity 系统与 ViewFactory 通过 EventBus 桥接：

```csharp
// Unity 层监听 Entity 生成事件 → 创建 GameObject
world.EventBus.Subscribe<EntitySpawnedEvent>(evt =>
{
    string prefabAddress = GetPrefabAddress(evt.EntityDefId);
    // 异步加载并实例化，详见 ViewFactory 文档
});
```

## 注意事项

- **EntityId 自增**：EntityManager 内部使用 `uint` 自增计数器，每 Spawn 一个新实体分配递增 ID
- **线程安全**：EntityManager 非线程安全，仅在主线程使用
- **不参与 Tick**：EntityManager 是纯事件驱动的，不实现 ITickable
- **EntityDefId 映射**：EntityDefId 指向配置表定义，ViewFactory 通过此 ID 查找对应的预制体地址

## 网络权限与生命周期

Entity 系统与 C6.3 网络实体权限系统集成，提供端到端的网络实体生命周期管理。

### 所有权模型

| OwnerClientId | 含义 | 权限 |
|:------------:|------|------|
| -1 | 无所有者 | 仅 Server 可操作 |
| 0 | Server 所有 | Server 始终拥有 |
| >0 | 指定客户端所有 | 所有者 + Server 可操作 |

### 使用示例

```csharp
// 服务端生成一个由客户端 3 所有的实体
var entity = world.EntityManager.SpawnWithOwner(1001, 3);

// 检查权限（服务端）
if (world.EntityManager.HasAuthority(entity.EntityId, clientId))
{
    // 允许操作
}

// 转移所有权
world.EntityManager.TransferOwnership(entity.EntityId, newOwnerId);

// 移除所有权
world.EntityManager.RemoveOwnership(entity.EntityId);

// 查询客户端拥有的所有实体
var ownedEntities = world.EntityManager.GetOwnedEntities(clientId);
```

### 生命周期事件

```csharp
world.EventBus.Subscribe<EntitySpawnedEvent>(evt =>
{
    // Entity 已生成，Unity 层的 NetworkEntityLifecycleBridge
    // 会自动创建对应的 NetworkObject 并同步到所有客户端
});

world.EventBus.Subscribe<EntityOwnershipTransferredEvent>(evt =>
{
    // 所有权已变更，Unity 层会自动调用 GiveOwnership
});
```
