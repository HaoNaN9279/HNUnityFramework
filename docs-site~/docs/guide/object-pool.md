---
sidebar_position: 7
---

# 对象池

ObjectPool 模块提供高效的对象复用机制，支持 C# 普通对象池和 Unity GameObject 对象池。

## 架构概览

```
ObjectPoolManager (单例管理器)
  ├── Dictionary<string, PoolBase> — 管理所有池
  ├── 工厂方法：CreateObjectPool / CreateGameObjectPool
  └── 生命周期：Tick / LateTick 驱动

PoolBase (抽象基类，实现 ITickable + IReference)
  ├── ObjectPoolBase → ObjectPool<T> (C# 对象池)
  └── GameObjectPoolBase → GameObjectPool (GameObject 池)

PooledObjectBase (被池化对象的基类)
  └── PooledObject<T> (泛型池化对象)
```

## 核心类型

| 类型 | 说明 |
|------|------|
| `ObjectPoolManager` | 全局单例管理器，创建和管理所有对象池 |
| `PoolBase` | 池抽象基类，定义 Name/MaxCount/MinCount 等属性 |
| `ObjectPool<T>` | C# 对象池泛型抽象类 |
| `GameObjectPool` | GameObject 池抽象类 |
| `PooledObject<T>` | 被池化对象的泛型基类 |

## 池的配置参数

每个池都支持以下配置（通过 Initialize 重载设置）：

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `name` | (必填) | 池名称，全局唯一标识 |
| `initialCount` | `0` | 初始预创建数量 |
| `tickFrequency` | `0` | Tick 频率，0=每帧；N=每 N 帧 |
| `maxCount` | `int.MaxValue` | 普通上限，超过时 Tick 销毁 1 个 |
| `maxLimitCount` | `int.MaxValue` | 硬上限，超过时 Tick 销毁到 MaxCount |
| `minCount` | `0` | 普通下限，低于时 Tick 创建 1 个 |
| `minLimitCount` | `0` | 硬下限，低于时 Tick 创建到 MinCount |

## C# 对象池

适用于频繁创建销毁的非 UnityEngine.Object 类型。

### 创建 PooledObject 子类

```csharp
using HN.Framework;

public class MyPooledData : PooledObjectBase
{
    public int Value;

    public override void Clear()
    {
        Value = 0;
    }
}
```

### 创建 ObjectPool 子类

```csharp
public class MyDataPool : ObjectPool<MyPooledData>
{
    public override void Spawn()
    {
        // 创建新对象并入队
        var obj = new MyPooledData();
        objects.Enqueue(obj);
    }

    public override void Despawn()
    {
        // 出队并清理
        var obj = objects.Dequeue() as MyPooledData;
        obj.Clear();
    }
}
```

### 使用

```csharp
// 通过管理器创建
ObjectPoolManager.Initialize();
var pool = ObjectPoolManager.CreateObjectPool<MyDataPool>("MyData", initialCount: 10);

// 获取和归还
var data = pool.Acquire();
data.Value = 42;
// ... 使用 ...
pool.Release(data);
```

## GameObject 对象池

适用于频繁实例化/销毁的 GameObjects（子弹、特效等）。

### 创建 GameObjectPool 子类

```csharp
public class BulletPool : GameObjectPool
{
    public override void OnAcquire(GameObject obj)
    {
        // 取出时的额外处理（在 SetActive(true) 之前调用）
        obj.GetComponent<Bullet>().Reset();
    }

    public override void OnRelease(GameObject obj)
    {
        // 回收时的额外处理（在 SetActive(false) 之后调用）
        obj.GetComponent<Bullet>().StopEffects();
    }
}
```

### 使用

```csharp
// 通过管理器创建
var prototype = Resources.Load<GameObject>("Bullet");
var pool = ObjectPoolManager.CreateGameObjectPool<BulletPool>(
    prototype, "BulletPool", initialCount: 20);

// 获取和归还
var bullet = pool.Acquire(targetParent);  // 可指定父 Transform
bullet.transform.position = firePoint.position;

// 回收时自动 SetActive(false) + 移回池根节点
pool.Release(bullet);
```

## 自动调节机制

池的 Tick 方法会根据配置阈值自动调节对象数量：

```
if count > maxCount && count < maxLimitCount → Despawn 1 个
if count > maxLimitCount                     → Despawn 到 maxLimitCount
if count < minCount && count > minLimitCount → Spawn 1 个
if count < minLimitCount                     → Spawn 到 minLimitCount
```

频率由 `tickFrequency` 控制（基于 `HNLogicTime.LogicFrameCount` 取模）。

## 编辑器调试面板

运行时选中场景中的 `[ObjectPoolManager]` GameObject，Inspector 中会显示 `ObjectPoolViewer` 组件，实时展示所有池的状态：

```
Object Pools:
  MyDataPool[MyPooledData]  0:0|10--15--50|100
  ^名称    ^类型         ^MinLimit|Min--Current--Max|MaxLimit

GameObject Pools:
  BulletPool[GameObject]    5:10|10--25--100|200
```

## 注意事项

- `ObjectPool<T>` 和 `GameObjectPool` 都是抽象类，必须继承并实现 `Spawn`/`Despawn` 或 `OnAcquire`/`OnRelease`
- 池实例通过 `ReferencePool` 创建和回收（`CreateXxxPool` 内部调用 `ReferencePool.Acquire<T>()`）
- 移除池时调用 `ObjectPoolManager.RemoveObjectPool(name)`，会自动归还到引用池
- GameObject 池自动管理层级——回收时移回池根节点 `[{name}]`，取出时可指定父节点
