---
sidebar_position: 2
---

# 项目架构

HNUnityFramework 采用三层驱动架构（DriverLayer → CapabilityModule → Level），
配合四仓库菱形依赖解耦与三程序集分离设计。

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
| **CapabilityModule** | 服务接口定义 + 通用实现（ProcedureManager、ObjectPoolManager） | 游戏特定实现（GameNetworkService 等）→ Scripts |
| **Level.LogicModule** | MVC 框架、HFSM、Entity 基类、Sheet 属性 | 具体玩法逻辑（Task/Battle/Shop）→ Scripts |
| **Level.ViewModule** | ViewFactory 基类、EntityView 基类、PropertyBinder | 视图代码 → Scripts；Prefab 装配 → Design |

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
   │                      │           │     → World.Initialize   │
   │  · LogProvider       │←─────────│  Update()->World.Tick()  │
   │  · AssetOperator     │  注入      │  LateUpdate()->LateTick  │
   │  · NetworkManager    │           │                          │
   └──────────────────────┘           └──────────────────────────┘
                                       ▲
                                       │ 继承
                              ┌────────┴───────────┐
                              │     GameEntry        │ ← Scripts 仓库
                              │  (Scripts Repo)      │
                              │  OnRegisterGameModules│
                              │   → 注册游戏特定模块   │
                              └──────────────────────┘
```

**GameWorld**（纯 C#）：创建并持有所有模块，驱动 Tick 循环，暴露平台接口供外部注入。

**GameWorldDriver**（MonoBehaviour）：在 Awake 中创建 GameWorld，注入平台实现，调用虚方法 `OnRegisterGameModules` 让 Scripts 仓库注册游戏特定模块，在 Unity 生命周期中驱动 Tick。

## 关键设计决策

### ReferencePool 和 HNLogicTime 为何保留静态
- `ReferencePool`：线程安全的零 GC 工具类，类比 .NET 的 `ArrayPool<T>.Shared`，无业务状态
- `HNLogicTime`：纯数据类，仅提供 Time/DeltaTime/LogicFrameCount，不依赖外部服务

### ObjectPool 拆分边界
- Core 层：`PoolBase` 抽象、`ObjectPool<T>` 泛型池、`PooledObjectBase`、`ObjectPoolManager`
- Unity 层：`GameObjectPoolBase`、`GameObjectPool`、`PooledObject<T>`（管理 UnityEngine.Object）
- `ObjectPoolManager` 通过 `PoolBase` 接口管理所有池，Unity 侧的 GameObjectPool 由 GameWorldDriver 创建注册

### FishNet 隔离策略
- Core 层只定义 `INetworkManager` 接口和消息协议（纯 C#）
- Unity 层通过 `FishNetNetworkManager` 封装 FishNet Client/Server API
- FishNet 的 NetworkBehaviour/SyncVar/RPC 不使用，自定义消息协议替代

### Addressables 策略
- `AddressablesOperator` 为运行时主要路径
- `AssetDatabaseOperator` Editor only，用于快速迭代
- `ResourcesOperator` 标记 deprecated

### UNITY_SERVER 宏策略
- Core 层禁止使用，保证客户端/服务端代码一致
- Unity 层渲染相关代码用 `#if !UNITY_SERVER` 包裹

## 命名空间规范

| 架构层 | 命名空间 | 程序集 |
|--------|---------|--------|
| GameWorld | `HN.Framework.Core.Driver` | Core |
| 基础类库 | `HN.Framework.Core.Driver.Common` | Core |
| 池系统 | `HN.Framework.Core.Driver.Common.Pool.*` | Core |
| 平台抽象层 | `HN.Framework.Unity.Driver.Platform` | Unity |
| Capability（通用） | `HN.Framework.Core.Capability` | Core |
| Capability（Unity 实现） | `HN.Framework.Unity.Capability` | Unity |
| Level.LogicModule | `HN.Framework.Core.Level.Logic` | Core |
| Level.ViewModule | `HN.Framework.Unity.Level.View` | Unity |
| Editor | `HN.Framework.Editor` | Editor |

## 模块状态一览

| 架构层 | 模块 | 状态 | 说明 |
|--------|------|:----:|------|
| D1 | GameWorld | ✅ | 已从静态单例迁移 |
| D2 | 基础类库（Interfaces/HNLogicTime/Serialization） | ✅ | 手写 JSON 序列化器 + MemoryPack 二进制序列化包装器 |
| D2 | ReferencePool / PooledCollections | ✅ | 静态，不做迁移 |
| D2 | `ObjectPool<T>` / PoolBase / PooledObjectBase | ✅ | |
| D2 | IEventBus | 🚧 Stub | 接口定义 |
| D2 | HNRandom（确定性随机数） | ✅ | xorshift128+ 算法，支持种子设置与状态序列化 |
| D2 | HNFixedPoint（定点数） | 🚧 Stub | 可选模块，仅 lockstep 需要 |
| D3 | Debug 基础设施（LogLevel/ILogChannel/LogEntry） | ✅ | 日志等级枚举、模块级日志通道、结构化日志条目 |
| D3 | IDebugHub / IDebugCommand / DebugHub | ✅ | 调试中枢：通道/命令注册表 + 环形日志缓冲（100 条） |
| D4 | GameWorldDriver | ✅ | |
| D4 | AddressablesOperator / ResourcesOperator / AssetDatabaseOperator | ✅ | |
| D4 | GameObjectPool / `PooledObject<T>` | ✅ | |
| D4 | UnityLogProvider / UnityTimeProvider / UnityCoroutineProvider | ✅ | |
| D4 | HNRenderPipeline + ShaderLibrary | 🚧 Stub | |
| C1 | MemoryPack 序列化模块 | ✅ | 二进制序列化，含 Core 包装器 + Unity 类型格式化器（16种） |
| C2 | Debug 系统（Core + Unity） | ✅ | DebugHub（Capability 层）、DebugModule、DebugCommandRegistry、RuntimeDebugConsole |
| C15 | 热更新与脚本系统（HybridCLR + xLua Mod） | ✅ | HybridCLRAdapter + LuaModManager，含 AOT 元数据加载、热更 DLL 加载、Mod 生命周期管理、沙箱隔离 |
| C17 | 摄像机管理系统 | ✅ | ICameraManager + CameraManager(Cinemachine全量) + CameraHandle + CameraShake |
| S2 | ILogProvider / UnityLogProvider | ✅ | |
| S3 | IAssetOperator + 三种实现 | ✅ | |
| S5 | INetworkManager + FishNet 封装 | 🚧 Stub | |
| S6 | IEventBus / EventBus | ✅ | 线程安全事件总线（lock+snapshot），由 GameWorld 持有 |
| S7 | ProcedureManager / ProcedureState | ✅ | 静态单例已消除 |
| S9 | IStorageProvider | ✅ | 接口定义 |
| Level.Logic | MVC | ✅ | |
| Level.Logic | HFSM | ✅ | |
| Level.Logic | Entity 系统 | 🚧 Stub | |
| Level.Logic | **Sheet** | ✅ | 配置表运行时查询系统（ISheetManager + IConfigTable + AssetRef\<T\> + ConfigLoader） |
| Level.View | ViewFactory / EntityView | 🚧 Stub | |
| Level.View | PropertyBinder | partial ✅ | 属性绑定抽象类，提供 Bind/UnbindAll 方法 |
| C13 | UI 系统 (Core 接口) | ✅ Phase 1 | UILayer/UIPanelState/IUIManager/RedDotNode/DialogResult/ToastConfig/GuideStep |
| C13 | UI 系统 (Unity 实现) | ✅ Phase 2 | UIManager/UIPanel/UIAnimation/UIDialog/UIToast/UIGuide/RedDotManager |
| C13 | UI 系统 (扩展面板) | ✅ Phase 2 | UIDialog/UIToast/UIGuide/RedDotManager |
| V1 | UI 运行时 | ✅ Phase 2 | ✅ Phase 2（独立 Level.View.UI 目录 + Addressables + 动画集成） |
| E7 | Sheet Editor | ✅ | Phase 1（Excel 编辑 + AssetRefCell + SheetGrid） |

> ✅ = 已实现  🚧 Stub = 骨架已创建待完整实现

---

完整架构详情（含完整目录树、FishNet 封装策略、Luban + Sheet 协作模式、SRP 渲染管线分工、Art 仓库组织原则等）请参阅 [`架构~/最终架构.md`](pathname:///file/?path=D%3A%5Cworkspace%5Cwork%5CHNUnityFramework%5C%E6%9E%B6%E6%9E%84~%5C%E6%9C%80%E7%BB%88%E6%9E%B6%E6%9E%84.md)。
