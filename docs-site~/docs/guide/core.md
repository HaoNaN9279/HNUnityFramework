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

## DebugHub — 调试中枢

DebugHub 是框架的调试中枢，位于 `HN.Framework.Core.Driver.Common.Debug` 命名空间（Driver 层基础实现）和 `HN.Framework.Core.Capability.Debug` 命名空间（Capability 层增强实现）。

### 基础功能 (Driver 层)

- **通道注册** — 模块级日志通道注册与分级过滤
- **命令注册** — 调试命令的注册与按名查找
- **环形日志缓冲** — 最多保留 100 条结构化日志条目

### 与 ILogProvider 的关系

`ILogProvider` 是框架的日志输出抽象，`DebugHub` 在 Phase 2 中支持通过 `SetLogProvider` 方法将日志条目自动转发到 `ILogProvider`，实现调试日志与业务日志的统一输出。

```csharp
// GameWorld.LogProvider setter 自动调用 DebugHub.SetLogProvider
world.LogProvider = new UnityLogProvider();
```

#### Capability 层扩展 (v2)

Phase 2 在 `HN.Framework.Core.Capability.Debug` 命名空间新增了增强版 DebugHub，
继承自 Driver 层的基础实现，新增以下功能：

- **模块注册**：`RegisterModule(DebugModule)` — 批量注册模块的所有通道和命令
- **命令执行**：`ExecuteCommand(name, args)` — 按名称查找并执行调试命令（异常安全）
- **前缀搜索**：`SearchCommands(prefix)` — 按前缀匹配命令（用于控制台 Tab 自动补全）
- **ILogProvider 桥接**：`SetLogProvider(provider)` — 日志条目自动转发到 ILogProvider

```csharp
// 创建 DebugModule 并注册
var poolModule = new DebugModule("Pool",
    new ILogChannel[] { new PoolLogChannel() },
    new IDebugCommand[] { new PoolShowCommand(), new PoolClearCommand() });
world.DebugHub.RegisterModule(poolModule);

// 执行命令
world.DebugHub.ExecuteCommand("pool.show", new[] { "-v" });

// 搜索命令（前缀匹配）
var commands = world.DebugHub.SearchCommands("pool");

// 桥接到 ILogProvider（GameWorld setter 自动调用）
world.LogProvider = new UnityLogProvider();
```

## RuntimeDebugConsole — 运行时调试控制台

`RuntimeDebugConsole` 是一个 UGUI 调试终端组件，挂载到场景中的任意 GameObject 上即可激活。

### 功能

- **`~` 键切换** — 显示/隐藏控制台面板（屏幕下半部分）
- **命令执行** — 输入命令并回车，调用 `DebugHub.ExecuteCommand`
- **Tab 自动补全** — 按 Tab 键循环匹配已注册的命令
- **历史浏览** — ↑/↓ 键浏览命令历史（最多 50 条）
- **错误回显** — 未知命令显示红色错误提示
- **输出截断** — 最多保留 200 行输出

### 使用方式

```csharp
// 将组件挂载到场景中的 GameObject 上
// 运行时通过 GameWorldDriver 自动发现 DebugHub
var console = gameObject.AddComponent<RuntimeDebugConsole>();

// 也可手动注入 DebugHub（覆盖自动发现）
console.DebugHub = world.DebugHub;
```

> 控制台使用纯代码创建 UGUI 元素，无需 UXML/USS 或手动 Canvas 配置。

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

## HNRandom — 确定性伪随机数生成器

`HNRandom` 是基于 xorshift128+ 算法的确定��伪随机数生成器，位于 `HN.Framework.Core.Driver.Common.Math` 命名空间。

### 为什么需要 HNRandom？

与 `System.Random` 不同，`HNRandom` 的实现在所有 .NET 版本和平台上完全一致：

| 特性 | System.Random | HNRandom |
|------|:------------:|:--------:|
| 跨平台一致性 | ❌ 不保证 | ✅ 完全一致 |
| 种子控制 | ✅ | ✅ |
| 状态序列化 | ❌ | ✅ GetState/SetState |
| 性能 | 中等 | 快速（xorshift128+） |
| 统计质量 | 一般 | 优秀（通过 BigCrush） |

适用场景：回放系统、网络同步随机数、自动化测试、地图种子生成。

### 基本用法

```csharp
using HN.Framework.Core.Driver.Common.Math;

// 使用指定种子创建（相同种子 → 相同序列）
var rng = new HNRandom(12345UL);

// 随机整数
int value = rng.Next();                    // 0 ~ int.MaxValue
int dice = rng.Next(1, 7);                 // 1 ~ 6
int percentage = rng.Next(100);            // 0 ~ 99

// 随机浮点数
double d = rng.NextDouble();               // [0.0, 1.0)
float f = rng.NextFloat();                 // [0.0f, 1.0f)

// 原始 64 位随机数
ulong raw = rng.NextUInt64();
```

### 确定性验证

```csharp
var rng1 = new HNRandom(42UL);
var rng2 = new HNRandom(42UL);

for (int i = 0; i < 100; i++)
{
    Debug.Assert(rng1.Next() == rng2.Next()); // 始终相等
}
```

### 状态序列化

用于保存/恢复随机数生成器状态（如存档系统）：

```csharp
var rng = new HNRandom(seed);

// 消费一些随机数...
for (int i = 0; i < 100; i++) rng.Next();

// 保存状态
var (s0, s1) = rng.GetState();
// 将 s0, s1 序列化到存档...

// 从存档恢复
rng.SetState(s0, s1);
// 之后的随机数序列与保存时完全一致
```

### 线程安全

`HNRandom` 不是线程安全的。每个线程应使用独立实例。

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
