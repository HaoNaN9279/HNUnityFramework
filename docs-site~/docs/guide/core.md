---
sidebar_position: 4
---

# Core 模块

Core 模块是框架的入口和心脏，负责框架初始化、全局配置、平台适配器注入和固定步长逻辑帧驱动。

## 概述

Core 模块包含以下核心类型：

| 类型 | 说明 |
|------|------|
| `GameWorld` | 纯 C# 驱动根节点，持有所有模块实例，驱动 Tick 循环 |
| `GameWorldDriver` | MonoBehaviour，桥接 Unity 生命周期与 GameWorld |
| `HNLogicTime` | 静态逻辑时间系统（独立于 Unity Time.timeScale） |
| `HNUnityFrameworkGlobalSettings` | ScriptableObject 全局配置 |
| `ITickable` | 可被帧循环驱动的接口契约 |

## GameWorld — 纯 C# 驱动根节点

`GameWorld` 是框架的核心枢纽，位于 `HN.Framework.Core` 程序集，不依赖 UnityEngine。

### 职责

- 在构造函数中创建框架内置模块实例
- 通过属性暴露模块引用供外部访问
- 接收外部注入的平台适配实现
- 驱动 Tick / LateTick 循环，级联到所有模块

```csharp
// 框架内置模块
world.PoolManager          // 对象池管理器
world.ProcedureManager     // 流程状态机管理器
world.ControllerManager    // 控制器管理器

// 平台适配接口（由 GameWorldDriver 注入）
world.LogProvider          // 日志接口
world.AssetManager         // 资源管理器
world.NetworkManager       // 网络管理器
world.StorageProvider      // 存储服务接口
```

## DebugHub — 调试中枢

`DebugHub` 是 D3 Debug 基础设施的核心实现，由 `GameWorld` 在构造函数中
自动创建，通过 `world.DebugHub` 属性访问。

### 职责

- **通道注册**：`RegisterChannel(ILogChannel)` — 模块注册日志通道，支持运行时开关
- **命令注册**：`RegisterCommand(IDebugCommand)` — 注册调试命令
- **结构化日志**：`Log(LogLevel, channel, message, context)` — 写入环形缓冲（最多 100 条）
- **事件通知**：`OnLog` 事件 — 每次 Log 调用时触发

### 与 ILogProvider 的关系

`DebugHub` 与 `ILogProvider` 职责完全分离：
- `DebugHub` — 结构化日志基础设施（存储 + 事件），不做输出
- `ILogProvider` — 唯一日志输出通道（UnityEngine.Debug / 文件 / 网络）

```csharp
// 注册模块日志通道
world.DebugHub.RegisterChannel(new LogChannel("pool", enabled: true));

// 记录结构化日志（写入调试缓冲 + 触发 OnLog 事件）
world.DebugHub.Log(LogLevel.Warning, "pool", "池容量接近上限");

// 通过 ILogProvider 输出日志
world.LogProvider.Log(LogLevel.Info, "pool", "对象池初始化完成");
```

### 注入模式

`GameWorld` 不负责创建平台相关的实现，而是通过属性注入：

```csharp
var world = new GameWorld();
world.LogProvider = new UnityLogProvider();     // 注入 Unity 日志实现
world.AssetManager = new AssetManager();        // 注入资源管理器

// 完成注入后再初始化
world.Initialize();
```

这种模式保证了 `HN.Framework.Core` 程序集保持纯 C#，零 UnityEngine 引用。

### 内部 Tick 调度

```csharp
public void Tick()
{
    PoolManager.Tick();
    ProcedureManager.Tick();
    ControllerManager.Tick();
    AssetManager?.Tick();
}

public void LateTick()
{
    PoolManager.LateTick();
    ProcedureManager.LateTick();
    ControllerManager.LateTick();
    AssetManager?.LateTick();
}
```

## GameWorldDriver — Unity 生命周期桥接

`GameWorldDriver` 是一个 MonoBehaviour，位于 `HN.Framework.Unity` 程序集，负责：

- 在 `Awake()` 中创建 `GameWorld` 实例
- 注入 Unity 平台实现（日志、资源加载器等）
- 调用 `OnRegisterGameModules` 虚方法（用户扩展点）
- 将 Unity 的 `Update` / `LateUpdate` 转发为 `GameWorld.Tick` / `LateTick`

### 标准用法

你的项目必须创建一个 `GameEntry` 类继承 `GameWorldDriver`，并挂载到场景中的 GameObject 上。

```csharp
using HN.Framework.Unity.Driver.Platform;
using HN.Framework.Core.Driver;

public class GameEntry : GameWorldDriver
{
    protected override void OnRegisterGameModules(GameWorld world)
    {
        // 注册游戏特定的 Controller
        world.ControllerManager.RegisterController(new BattleController(world));

        // 注入游戏特定的服务实现
        world.NetworkManager = new GameNetworkService();
        world.StorageProvider = new GameStorageService();

        // 注：LogProvider 和 AssetManager 已在基类 Awake 中自动注入
    }
}
```

### 生命周期

```
Awake()
  ├── new GameWorld()              ← 创建纯 C# 驱动根节点
  ├── world.LogProvider 注入       ← UnityLogProvider
  ├── world.AssetManager 注入      ← AssetDatabaseOperator / AddressablesOperator
  ├── OnRegisterGameModules(world) ← 子类注册游戏特定模块
  └── world.Initialize()          ← 初始化所有模块

Update()    → world.Tick()
LateUpdate() → world.LateTick()
```

## 固定步长逻辑帧

框架的核心机制是**固定步长逻辑帧循环**，完全独立于 Unity 的 `Time.timeScale`。

### 工作原理

```
每帧 Update:
  1. 计算当前帧真实耗时 frameTime（Time.unscaledDeltaTime）
  2. 截断到 MaxFrameTime（默认 0.1s，防止长时间挂起后的螺旋追赶）
  3. 累加到 accumulatedTime
  4. 以 fixedLogicFrameTime (= 1 / LogicRate) 为步长循环：
     a. 推进 HNLogicTime (Time += delta, LogicFrameCount++)
     b. 执行 GameWorld.Tick()
     c. accumulatedTime -= fixedLogicFrameTime
     d. 步数达到 MaxStepsPerFrame 时强制停止（防止死循环）
```

### 配置参数

所有参数通过 `HNUnityFrameworkGlobalSettings` 配置：

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `LogicRateMode` | `Custom` | `Custom`：固定帧率模式；`NoLimit`：不限帧率 |
| `LogicRate` | `60` | 目标逻辑帧率（仅 Custom 模式生效） |
| `MaxStepsPerFrame` | `5` | 单帧最大逻辑步数，防止螺旋式追赶死循环 |
| `MaxFrameTime` | `0.1f` | 单帧最大时间预算（秒），超过则截断 |

### 编辑器配置

通过 Unity 菜单 **Edit → Project Settings → HN Unity Framework** 打开设置面板，可直接修改以上参数。

## HNLogicTime — 逻辑时间

`HNLogicTime` 是纯静态的纯数据类，提供逻辑层统一的时间视图。

```csharp
// 逻辑时间 API（只读，仅框架内部可写入）
double currentTime = HNLogicTime.Time;            // 当前逻辑时间（秒）
double deltaTime  = HNLogicTime.DeltaTime;        // 当前逻辑帧间隔（秒）
ulong  frameCount = HNLogicTime.LogicFrameCount;  // 累计逻辑帧数
```

### 为什么是 static？

`HNLogicTime` 是纯数据容器，不持有任何业务状态，不依赖外部服务，也不被任何模块管理。保留静态设计，类似于 `ReferencePool`，属于线程安全的工具类模式。

## HNUnityFrameworkGlobalSettings — 全局配置

`HNUnityFrameworkGlobalSettings` 是一个 ScriptableObject，集中管理框架运行时的全局参数。

### 引用路径

默认存储在 `Assets/Project/RuntimeAssets/Core/HNUnityFrameworkGlobalSettings.asset`，通过 Addressables 加载。

### 配置参数

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `LogicRateMode` | `LogicRateMode` 枚举 | `Custom` | 逻辑帧率模式：`Custom` 固定帧率，`NoLimit` 不限帧率 |
| `LogicRate` | `int` | `60` | 目标逻辑帧率（仅 Custom 模式） |
| `MaxStepsPerFrame` | `int` | `5` | 单帧最大逻辑步数保护 |
| `MaxFrameTime` | `float` | `0.1f` | 单帧最大时间预算（秒） |

```csharp
// 运行时读取
int logicRate = settings.LogicRate;
int maxSteps  = settings.MaxStepsPerFrame;
float maxTime = settings.MaxFrameTime;
```

## ITickable 接口

所有需要帧驱动的模块均实现此接口，由 `GameWorld.Tick` / `LateTick` 统一调度。

```csharp
public interface ITickable
{
    void Tick();
    void LateTick();
}
```

框架内置的 `ObjectPoolManager`、`ProcedureManager`、`ControllerManager` 均已实现此接口，并在 `GameWorld.Tick()` 中自动调用。

## 迁移说明

旧版本中框架入口为 `HNUnityFramework` 抽象类（MonoBehaviour），用户通过继承它并重写 `OnAwake` / `OnStart` 等虚方法来接入。该设计已废弃，被 `GameWorld` + `GameWorldDriver` 双组件模式取代：

| 旧模式 | 新模式 |
|--------|--------|
| `HNUnityFramework` 抽象类（继承） | `GameWorldDriver`（继承）+ `GameWorld`（纯 C# 组合） |
| 子类重写 `OnAwake` / `OnStart` / `OnUpdate` / `OnLateUpdate` | 子类重写 `OnRegisterGameModules(GameWorld world)` |
| 静态单例管理器 | 模块实例由 `GameWorld` 持有 |
| 管理器通过 `Initialize()` 手动初始化 | 管理器在 `GameWorld` 构造函数中自动创建 |

## 注意事项

- `GameWorldDriver` 是 **MonoBehaviour**，必须挂载到场景中的 GameObject 上
- 请勿直接修改 `GameWorld.cs` 的构造函数来注册游戏模块，应通过 `OnRegisterGameModules` 虚方法
- `LogProvider` 和 `AssetManager` 已在基类 `Awake()` 中完成注入，子类无需重复操作
- 逻辑帧独立于 `Time.timeScale`，不受时间缩放影响
- 全局设置通过 Addressables 加载，确保 `HNUnityFrameworkGlobalSettings.asset` 已标记为 Addressable
