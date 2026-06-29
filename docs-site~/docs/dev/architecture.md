---
sidebar_position: 2
---

# 项目架构

HNUnityFramework 采用模块化分层架构设计。

## 整体结构

```
HNUnityFramework/
├── Runtime/          # 运行时代码 (程序集: HN.Framework)
│   ├── Core/         # 框架核心（启动、全局设置、Tick 系统）
│   ├── MVC/          # MVC 模式实现（Model、View、Controller）
│   ├── AssetManager/ # 资源管理（Addressables/Resources/AssetDatabase）
│   ├── ObjectPool/   # 对象池系统
│   ├── ReferencePool/# 引用池系统
│   ├── HFSM/         # 层次有限状态机
│   ├── Procedure/    # 流程管理
│   ├── Serialize/    # 序列化 (程序集: HN.Serialize)
│   ├── Utils/        # 工具类 (命名空间: HN)
│   └── Sheet/        # 配置表
├── Editor/           # 编辑器扩展代码 (程序集: HN.Framework.Editor)
│   ├── Core/         # 编辑器核心（菜单、部署、设置面板）
│   ├── Sheet/        # 配置表编辑器
│   ├── ObjectPool/   # 对象池调试面板
│   ├── AddressablesExtensitions/ # Addressables 分组管理
│   └── Utils/        # 编辑器工具
└── ~docs-site/       # 文档站点 (Docusaurus)
```

## 核心设计原则

- **低耦合、高内聚**：各模块职责清晰，通过接口通信
- **可扩展**：基于接口设计，易于替换和扩展
- **零 GC 压力**：核心运行时使用引用池避免内存分配
- **编辑器友好**：提供丰富的编辑器工具和调试面板

## 框架生命周期

框架入口是 `HNUnityFramework`（抽象 MonoBehaviour），完整生命周期如下：

```
┌─────────────────────────────────────────────────────────┐
│                       Awake                             │
│  1. HNLogicTime.Initialize()     — 重置逻辑时间         │
│  2. ObjectPoolManager.Initialize() — 创建对象池管理器   │
│  3. ProcedureManager.Initialize()  — 创建流程管理器     │
│  4. ControllerManager.Initialize() — 创建控制器管理器   │
├─────────────────────────────────────────────────────────┤
│                       Start                             │
│  加载 HNUnityFrameworkGlobalSettings (via Addressables) │
│  计算 fixedLogicFrameTime (默认 1/60 ≈ 16.67ms)        │
├─────────────────────────────────────────────────────────┤
│                 每帧 Update / LateUpdate                 │
│  LogicTimeUpdate(tickFunc):                             │
│    计算帧耗时 → 截断(MaxFrameTime) → 累加              │
│    → 以 fixedLogicFrameTime 为步长循环执行:             │
│       a. HNLogicTime 推进 (Time, DeltaTime, FrameCount) │
│       b. Tick 链:                                       │
│          ObjectPoolManager → ProcedureManager →         │
│          ControllerManager                              │
│    → 步数保护(MaxStepsPerFrame)                         │
├─────────────────────────────────────────────────────────┤
│                     OnDestroy                            │
│  1. ControllerManager.Uninitialize()                     │
│  2. ProcedureManager.Shutdown() + Uninitialize()         │
│  3. ObjectPoolManager.Uninitialize()                     │
└─────────────────────────────────────────────────────────┘
```

## 固定步长逻辑帧

框架使用**独立于 Unity Time.timeScale 的固定步长逻辑帧循环**：

- 时间源：`Time.realtimeSinceStartupAsDouble`（不受 timeScale 影响）
- 逻辑时间：`HNLogicTime`（Time/DeltaTime/LogicFrameCount）
- 步长：由 `LogicRate` 决定（默认 60 Hz → ~16.67ms）
- 保护机制：`MaxFrameTime`（默认 0.1s 截断）+ `MaxStepsPerFrame`（默认 5 步上限）

## 模块依赖关系

```
                    ┌─────────────┐
                    │    Core     │ (框架入口 + 逻辑时间)
                    └──┬──┬──┬───┘
                       │  │  │
          ┌────────────┘  │  └────────────┐
          ▼               ▼               ▼
   ┌──────────┐   ┌──────────┐    ┌──────────┐
   │ObjectPool│   │Procedure │    │   MVC    │
   │ Manager  │   │ Manager  │    │ Manager  │
   └────┬─────┘   └────┬─────┘    └────┬─────┘
        │              │               │
        └──────────────┼───────────────┘
                       │
                       ▼
               ┌──────────────┐
               │ ReferencePool│ (被所有模块依赖)
               └──────────────┘

   HFSM ──→ ReferencePool (独立模块，自行管理生命周期)
   Serialize ──→ 独立模块 (命名空间 HN.Serialize)
   Utils ──→ 独立模块 (命名空间 HN)
```

## 对象池架构

```
ObjectPoolManager (单例)
  ├── Dictionary<string, PoolBase>
  ├── 工厂: CreateObjectPool<T> / CreateGameObjectPool<T>
  └── 生命周期: Tick → 遍历所有池 → 自动调节

PoolBase (抽象，ITickable + IReference)
  ├── ObjectPoolBase → ObjectPool<T> (PooledQueue 容器)
  └── GameObjectPoolBase → GameObjectPool (PooledQueue 容器)

PooledObjectBase → PooledObject<T> (被池化对象基类)
```

Tick 自动调节算法：

```
if count > MaxCount → Despawn 至 MaxLimitCount
if count < MinCount → Spawn 至 MinLimitCount
```

## 引用池架构

```
ReferencePool (静态，线程安全)
  └── Dictionary<Type, ReferenceCollection>
        └── ReferenceCollection (Queue<IReference>)
              ├── Acquire: 出队 | new
              └── Release: Clear() + 入队

IReference (接口)
  └── Clear() — 重置对象状态

PooledCollections (16 种)
  ├── PooledList<T>, PooledDictionary<K,V>, PooledQueue<T> ...
  └── PooledConcurrentQueue<T>, PooledConcurrentDictionary<K,V> ...
```
