---
sidebar_position: 2
---

# 项目架构

HNUnityFramework 采用三层驱动架构（DriverLayer → CapabilityModule → Level），
配合四仓库菱形依赖解耦与三程序集分离设计。

> 完整架构详情（含模块设计、命名空间映射、设计决策等）请参阅 [`架构~/` 目录下的模块化文档](../../../架构~/README.md)。

## 三层架构全景

框架从底到顶分为驱动层、通用能力层、关卡层三层：

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Level (关卡层)                               │
│  ┌─────────────────────────┐   ┌─────────────────────────────────┐  │
│  │     ViewModule           │<─>│         LogicModule             │  │
│  │   (视图层, 纯表现)        │   │     (数据/逻辑层, 纯逻辑)        │  │
│  │  3D场景  Pawn  UI         │   │  任务 Entity 场景 战斗 成就     │  │
│  │  相机   特效  天气  过场   │   │  商城 剧情 导航 技能 背包      │  │
│  │  音频   输入               │   │  物品 行为树 关卡状态          │  │
│  └─────────────────────────┘   └─────────────────────────────────┘  │
└──────────────────────────────┬─┬───────────────────────────────────┘
                               │ │  双向通信
┌──────────────────────────────┼─┼───────────────────────────────────┐
│                   CapabilityModule (通用能力层)                       │
│                               │ │                                   │
│  UI系统  日志系统  资源管理    音频系统  网络系统  事件系统             │
│  关卡重置  空间查询  存储系统  任务调度器  Procedure  对象池           │
└──────────────────────────────┬─────────────────────────────────────┘
                               │  单向依赖
┌──────────────────────────────┼─────────────────────────────────────┐
│                     DriverLayer (驱动层)                              │
│                               ▼                                     │
│  GameWorld     基础类库        游戏引擎         平台抽象层             │
│  全局驱动根节点  算法/工具      Unity           Unity 适配            │
└─────────────────────────────────────────────────────────────────────┘
```

**依赖规则**：
- `Level` ↔ `CapabilityModule`：双向通信
- `CapabilityModule` → `DriverLayer`：单向依赖
- `DriverLayer` 自底向上，GameWorld 在最上层驱动所有模块

### 框架提供 vs 上层仓库提供

| 架构层 | 框架提供 | 上层仓库提供 |
|--------|---------|-------------|
| **DriverLayer** | GameWorld、基础类库（HNLogicTime、ReferencePool、ITickable 等）、平台抽象层 | — |
| **CapabilityModule** | 服务接口定义 + 通用实现（ProcedureManager、ObjectPoolManager、DebugHub、EventBus 等） | 游戏特定实现（GameNetworkService 等）→ Scripts |
| **Level.LogicModule** | MVC 框架、HFSM、Entity 基类、Sheet/Config 系统 | 具体玩法逻辑（Task/Battle/Shop）→ Scripts |
| **Level.ViewModule** | ViewFactory、EntityView、PropertyBinder、UI 系统全套 | 视图代码 → Scripts；Prefab 装配 → Design |

## 四仓库菱形依赖

项目由四个独立仓库组成，框架在最底层，形成单向菱形依赖：

```
                        ┌─────────────────┐
                        │   策划设计内容    │  ← 最上层
                        │   (Design Repo)  │
                        │  零代码，纯装配   │
                        └───────┬─┬───────┘
                                │ │
                     ┌──────────┘ └──────────┐
                     │                       │
                     ▼                       ▼
         ┌──────────────────┐    ┌──────────────────┐
         │   项目脚本         │    │   纯美术资源       │
         │  (Scripts Repo)   │    │   (Art Repo)      │
         │  玩法逻辑 + 视图   │    │  零代码，按实体组织 │
         └────────┬─────────┘    └────────┬──────────┘
                  │                       │
                  └───────────┬───────────┘
                              │
                 ┌────────────────────────┐
                 │   HNUnityFramework      │  ← 最底层
                 │   (Framework Repo)      │
                 │   HN.Framework.Core    │  ← 纯 C# 逻辑层
                 │   HN.Framework.Unity   │  ← Unity 平台层
                 │   HN.Framework.Editor  │  ← 编辑器工具层
                 └────────────────────────┘
```

**解耦原则**：
- 程序写代码 → 不需要美术资源与策划 Prefab，只需 Framework
- 美术做资源 → 不需要程序代码与策划场景，只需 Framework（Shader 依赖）
- 策划搭场景 → 不需要修改代码与源美术资产，引用 Art + Scripts 装配

## 三程序集依赖关系

按 Unity 依赖边界拆分为三个程序集：

| 程序集 | 架构层 | 外部依赖 | noEngineRefs | 命名空间 |
|--------|--------|---------|:------------:|---------|
| `HN.Framework.Core` | DriverLayer + CapabilityModule（纯 C# 部分）+ Level.Logic 基础设施 | 无（仅 .NET Standard 2.1） | ✅ true | `HN.Framework.Core.*` |
| `HN.Framework.Unity` | 平台抽象层 + Unity 依赖实现 + Level.View 基础设施 | UnityEngine + Addressables + FishNet | ❌ | `HN.Framework.Unity.*` |
| `HN.Framework.Editor` | 编辑器工具层 | UnityEditor | ❌ | `HN.Framework.Editor` |

```
┌─────────────────────────┐
│   HN.Framework.Editor   │  Editor only
└───────────┬─────────────┘
            │ 引用
    ┌───────┴───────────────────────┐
    ▼                               ▼
┌──────────────────┐  ┌─────────────────────────────────┐
│ HN.Framework     │  │    HN.Framework.Unity            │
│  .Core            │  │                                 │
│  (noEngineRefs)   │  │  + UnityEngine + Addressables   │
│                   │  │  + FishNet (封装)               │
│  GameWorld        │◄─┤  (引用 HN.Framework.Core)       │
│  Capability 接口  │  │                                 │
│  Level.Logic 基础 │  └─────────────────────────────────┘
└──────────────────┘
```

**跨仓库依赖规则**：
- Scripts → Framework：asmdef `references` 显式引用 Core 和 Unity 的 GUID
- `HN.Framework.Core` → FishNet：**禁止**，纯 C# 层只定义 `INetworkManager`
- `HN.Framework.Unity` → FishNet：唯一在 Framework 内引用 FishNet 的位置
- Framework → 上层：**禁止**，asmdef 绝不包含上层仓库 GUID

## GameWorld 双组件驱动模型

所有有状态的 Manager 从静态单例改为 GameWorld 持有的实例。`ReferencePool` 和 `HNLogicTime` 因其工具类性质保持不变。

```
   HN.Framework.Core（纯 C#）            HN.Framework.Unity（Unity 层）
   ────────────────────────            ──────────────────────────────

   ┌──────────────────────┐           ┌──────────────────────────┐
   │     GameWorld        │◄──────────│    GameWorldDriver        │
   │                      │  持有并驱动 │   (MonoBehaviour)         │
   │  · PoolManager       │           │                          │
   │  · ProcedureManager  │           │  Awake() → new GameWorld │
   │  · ControllerManager │           │     → 注入平台实现        │
   │  · EntityManager     │           │     → World.Initialize   │
   │  · DebugHub          │           │                          │
   │                      │←─────────│  Update()->World.Tick()  │
   │  · LogProvider       │  注入      │  LateUpdate()->LateTick  │
   │  · AssetOperator     │           │                          │
   │  · NetworkManager    │           └──────────────────────────┘
   └──────────────────────┘            ▲
                                       │ 继承
                              ┌────────┴───────────┐
                              │     GameEntry        │ ← Scripts 仓库
                              │  (Scripts Repo)      │
                              │  OnRegisterGameModules│
                              │   → 注册游戏特定模块   │
                              └──────────────────────┘
```

## 关键设计决策

### ReferencePool 和 HNLogicTime 为何保留静态
- `ReferencePool`：线程安全的零 GC 工具类，类比 .NET 的 `ArrayPool<T>.Shared`，无业务状态
- `HNLogicTime`：纯数据类，仅提供 Time/DeltaTime/LogicFrameCount，不依赖外部服务

### ObjectPool 拆分边界
- Core 层：`PoolBase` 抽象、`ObjectPool<T>` 泛型池、`PooledObjectBase`、`ObjectPoolManager`
- Unity 层：`GameObjectPoolBase`、`GameObjectPool`、`PooledObject<T>`（管理 UnityEngine.Object）

### FishNet 隔离策略
- Core 层只定义 `INetworkManager` 接口和帧同步/预测数据模型
- Unity 层通过 `FishNetNetworkManager` 封装 FishNet Client/Server API
- 最大程度复用 FishNet 内置能力（SyncVar/RPC/Spawn/Prediction），不自建同步协议

### Addressables 策略
- `AddressablesOperator` 为运行时主要路径
- `AssetDatabaseOperator` Editor only，用于快速迭代
- `ResourcesOperator` 标记 deprecated

### UNITY_SERVER 宏策略
- Core 层禁止使用，保证客户端/服务端代码一致
- Unity 层渲染相关代码用 `#if !UNITY_SERVER` 包裹

## 模块状态一览

| 架构层 | 模块 | 状态 | 说明 |
|--------|------|:----:|------|
| D1 | GameWorld | ✅ | 非静态单例，持有所有模块 |
| D2 | 基础类库（Interfaces/HNLogicTime/Serialization） | ✅ | Json 序列化器 + MemoryPack 包装器 |
| D2 | ReferencePool / PooledCollections | ✅ | 静态工具类，17 种池化集合 |
| D2 | ObjectPool<T> / PoolBase | ✅ | 纯 C# 泛型对象池 |
| D2 | IEventBus / EventBus | ✅ | 线程安全事件总线 |
| D2 | HNRandom | ✅ | xorshift128+ 确定性随机数 |
| D2 | FixedMathSharp | ✅ | 定点数库（vendored DLL），仅 lockstep 需要 |
| D3 | Debug 基础设施（LogLevel/ILogChannel/LogEntry） | ✅ | 结构化日志数据模型 |
| D3 | DebugHub / IDebugCommand | ✅ | 调试中枢：通道/命令注册表 + 环形缓冲区 |
| D4 | GameWorldDriver | ✅ | MonoBehaviour 驱动 |
| D4 | AddressablesOperator / ResourcesOperator / AssetDatabaseOperator | ✅ | 三种资源加载路径 |
| D4 | AssetManager + AssetCacheItem | ✅ | 资源管理 + 弱引用缓存 |
| D4 | GameObjectPool / PooledObject | ✅ | Unity 对象池 |
| D4 | UnityLogProvider / UnityTimeProvider / UnityCoroutineProvider | ✅ | 平台适配 |
| D4 | HNRenderPipeline + ShaderLibrary | ✅ | 渲染管线 + Shader 库 |
| D4 | GlobalSettings | ✅ | ScriptableObject 全局配置 |
| C1 | MemoryPack 序列化模块 | ✅ | 二进制序列化 + Unity 格式化器 |
| C2 | Debug 系统（Core + Unity） | ✅ | DebugHub + RuntimeDebugConsole + InputDebugger |
| C3 | 本地化系统 | ✅ | Core + Unity 层全部已完成（LocaleManager / TextLocalizer / LocaleSelector / AssetLocalizer） |
| C4 | IAssetManager + AssetManager | ✅ | 资源管理核心 |
| C5 | ILogProvider / UnityLogProvider | ✅ | 日志接口（待扩展） |
| C6 | 网络系统（FishNet 封装） | ✅ | INetworkManager + 帧同步 + 预测与校验 + 实体权限与生命周期 |
| C7 | EventBus | ✅ | 线程安全事件总线 |
| C8 | IStorageProvider | ✅ | 存储接口 |
| C9 | ProcedureManager | ✅ | 流程状态机 |
| C10 | ObjectPoolManager | ✅ | 对象池管理器 |
| C11 | 音频系统 | 📋 | 规划中 |
| C12 | 过场动画系统 | 📋 | 规划中 |
| C13 | UI 系统 | ✅ Phase 2 | 7 层 Canvas + UIPanel/UIDialog/UIToast/UIGuide/RedDotManager |
| C14 | 输入系统 | ✅ | Unity InputSystem 框架级封装 |
| C15 | 热更新与脚本系统（HybridCLR + xLua） | ✅ | Core 接口 + Unity 实现 + Editor 工具 |
| C16 | 物理系统抽象层 | ✅ | IPhysicsWorld/IBody + PhysXWorld |
| C17 | 摄像机管理系统 | ✅ | ICameraManager + CameraManager（Cinemachine） |
| C18 | 数据安全 | 📋 | 规划中 |
| C19 | 场景流送 | 📋 | 规划中 |
| C20 | 昼夜天气 | 📋 | 规划中 |
| C21 | 性能监控 | 📋 | 规划中 |
| L1 | MVC 框架 | ✅ | Controller/Model/ControllerManager/IReadOnlyModel |
| L2 | HFSM | ✅ | 完整层次状态机（复合状态/死循环检测） |
| L3 | Entity 系统 | ✅ | Entity + EntityManager + EntityView + ViewFactory |
| L4 | Sheet/Config 系统 | ✅ | ISheetManager + ConfigTable + AssetRef + SheetManager |
| L5 | 图数据模型 | 📋 | 规划中 |
| L6 | 动画数据模型 | 📋 | 规划中 |
| L7 | 战斗数值体系 | ✅ | Attribute+Effect+Buff+Ability 实现 |
| L8 | 任务成就系统 | 📋 | 规划中 |
| L9 | 物品背包装备 | 📋 | 规划中 |
| L10 | AI 行为框架 | 📋 | 规划中 |
| V1 | UI 运行时 | ✅ Phase 2 | 7 层 Canvas + Addressables + 动画 + Toast + 引导 |
| V2 | ViewFactory + EntityView | ✅ | 实体视图工厂 |
| V3 | PropertyBinder | ✅ | IReadOnlyModel → UI 绑定 |
| V4 | 动画 Playable 运行时 | 📋 | 规划中 |
| V5 | 音频播放组件 | 📋 | 规划中 |
| E1 | Editor UI 扩展 | 📋 | 规划中 |
| E2 | Graph Editor | 📋 | 规划中 |
| E3 | Debug Hub Editor | 📋 | 规划中 |
| E4 | Localization Editor | 📋 | 规划中 |
| E5 | Audio Editor | 📋 | 规划中 |
| E6 | Cutscene Editor | 📋 | 规划中 |
| E7 | Sheet Editor | ✅ | Excel 编辑 + AssetRefCell + SheetGrid |
| E8 | ObjectPoolViewer | 📋 | 空目录 |
| E8 | AddressablesExtensions | ✅ | Group 预设管理 |
| E8 | HNDictionaryDrawer / HNUndoableObject | ✅ | 工具类 |
| E9 | BuildPipeline | 📋 | 规划中 |

> ✅ = 已实现  📋 = 规划中

---

完整架构详情（含完整模块设计、命名空间映射、Vendor 策略、Luban 协作模式、SRP 渲染管线分工、Art 仓库组织原则等）请参阅 [`架构~/` 目录下的模块化文档](../../../架构~/README.md)。
