---
sidebar_position: 8
---

# 引用池

ReferencePool 模块提供零 GC 压力的引用对象复用机制，是框架内存管理的基石。

## 概述

引用池的设计目标：**避免频繁的 new/GC 分配**。所有实现 `IReference` 接口的对象都可以通过 `ReferencePool` 获取和回收。

## 核心类型

| 类型 | 说明 |
|------|------|
| `IReference` | 接口，定义 `Clear()` 方法，用于重置对象状态 |
| `ReferencePool` | 静态类，提供 Acquire/Release/Add/Remove/ClearAll 等操作 |
| `ReferenceCollection` | 内部类，按类型管理 Queue（嵌套在 ReferencePool 中） |
| `PooledList<T>` 等 | 16 种可池化的集合类型 |

## IReference 接口

```csharp
public interface IReference
{
    void Clear();  // 归还前重置对象状态
}
```

实现此接口的类即可被引用池管理：

```csharp
public class MyData : IReference
{
    public int Value;
    public string Name;

    public void Clear()
    {
        Value = 0;
        Name = null;
    }
}
```

## 基本使用

### 获取和归还

```csharp
// 从池中获取（池空则 new）
var data = ReferencePool.Acquire<MyData>();
data.Value = 42;

// ... 使用 data ...

// 归还（自动调用 Clear()）
ReferencePool.Release(data);
```

### 预填充

```csharp
// 预先创建 100 个实例到池中
ReferencePool.Add<MyData>(100);
```

### 手动管理池容量

```csharp
// 减少池中空闲对象数量
ReferencePool.Remove<MyData>(50);    // 移除 50 个
ReferencePool.RemoveAll<MyData>();   // 全部移除
ReferencePool.ClearAll();            // 清除所有类型的池
```

## 线程安全

`ReferencePool` 是线程安全的——所有内部操作（Acquire/Release/Add/Remove）均使用 `lock` 保护内部 `Queue<IReference>`。

## PooledCollections — 池化集合

框架提供了 16 种可直接池化的集合类型，均继承标准集合类并实现 `IReference`：

### 基础集合

| 类型 | 继承自 | 适用场景 |
|------|--------|---------|
| `PooledList<T>` | `List<T>` | 动态数组 |
| `PooledDictionary<K,V>` | `Dictionary<K,V>` | 键值对映射 |
| `PooledQueue<T>` | `Queue<T>` | FIFO 队列 |
| `PooledStack<T>` | `Stack<T>` | LIFO 栈 |
| `PooledHashSet<T>` | `HashSet<T>` | 唯一元素集合 |
| `PooledLinkedList<T>` | `LinkedList<T>` | 双向链表 |
| `PooledSortedList<K,V>` | `SortedList<K,V>` | 有序键值对 |
| `PooledSortedDictionary<K,V>` | `SortedDictionary<K,V>` | 有序字典 |

### 线程安全集合

| 类型 | 继承自 | 适用场景 |
|------|--------|---------|
| `PooledConcurrentQueue<T>` | `ConcurrentQueue<T>` | 线程安全 FIFO |
| `PooledConcurrentStack<T>` | `ConcurrentStack<T>` | 线程安全 LIFO |
| `PooledConcurrentBag<T>` | `ConcurrentBag<T>` | 线程安全无序集合 |
| `PooledConcurrentDictionary<K,V>` | `ConcurrentDictionary<K,V>` | 线程安全字典 |
| `PooledConcurrentSet<T>` | `ConcurrentDictionary<T,byte>` | 线程安全 Set |
| `PooledConcurrentHashSet<T>` | `ConcurrentDictionary<T,byte>` | 线程安全 HashSet |
| `PooledConcurrentLinkedList<T>` | `ConcurrentBag<T>` | 线程安全链表 |
| `PooledConcurrentSortedList<K,V>` | `ConcurrentDictionary<K,V>` | 线程安全有序列表 |

### 使用示例

```csharp
// 获取池化集合
var list = ReferencePool.Acquire<PooledList<MyData>>();
list.Add(new MyData { Value = 1 });
list.Add(new MyData { Value = 2 });

// 使用完毕后归还（自动调用 Clear，清空集合内容）
ReferencePool.Release(list);
```

## 在框架中的应用

引用池是框架内部大量使用的机制：

- **MVC 模块**：ModelUnit 和 ControllerUnit 通过引用池创建/回收
- **对象池模块**：ObjectPool 实例通过 `ReferencePool.Acquire<T>()` 创建
- **HFSM 模块**：State 和 Transition 通过引用池管理
- **Procedure 模块**：ProcedureState 通过引用池创建
- **AssetManager**：Operator 和 CacheItem 通过引用池管理

## 注意事项

- 实现 `IReference` 的类**必须有无参构造函数**（`new()` 约束）
- `Clear()` 中务必重置所有字段，避免归还后残留数据影响下次使用
- `Release()` 前确保外部已无对该对象的引用，否则会导致数据错乱
- 线程安全集合适用于多线程场景，但多数游戏逻辑在主线程运行，普通池化集合即足够
