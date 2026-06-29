---
sidebar_position: 4
---

# Core 模块

Core 模块是框架的入口和心脏，负责框架初始化、全局配置和固定步长逻辑帧驱动。

## 概述

Core 模块包含以下核心类型：

| 类型 | 说明 |
|------|------|
| `HNUnityFramework` | 抽象 MonoBehaviour，框架入口，驱动生命周期 |
| `HNLogicTime` | 逻辑时间系统（独立于 Unity Time.timeScale） |
| `HNUnityFrameworkGlobalSettings` | ScriptableObject 全局配置 |
| `ITickable` | 可被帧循环驱动的接口契约 |

## HNUnityFramework — 框架入口

`HNUnityFramework` 是抽象类，你的项目必须创建一个子类并挂载到场景 GameObject 上。

### 生命周期虚方法

框架提供了四个可重写的虚方法，遵循模板方法模式：

```csharp
public class GameEntry : HNUnityFramework
{
    protected override void OnAwake()
    {
        base.OnAwake(); // 必须调用！初始化所有子系统
    }

    protected override void OnStart()
    {
        base.OnStart(); // 加载全局设置
    }

    protected override void OnUpdate()
    {
        base.OnUpdate(); // 驱动逻辑帧
    }

    protected override void OnLateUpdate()
    {
        base.OnLateUpdate();
    }
}
```

### 初始化顺序

在 `OnAwake()` 中，框架按以下顺序初始化子系统：

1. `HNLogicTime.Initialize()` — 重置逻辑时间
2. `ObjectPoolManager.Initialize()` — 创建对象池管理器
3. `ProcedureManager.Initialize()` — 创建流程管理器
4. `ControllerManager.Initialize()` — 创建控制器管理器

## 固定步长逻辑帧

框架的核心机制是**固定步长逻辑帧循环**，完全独立于 Unity 的 `Time.timeScale`。

### 工作原理

```
每帧 Update/LateUpdate:
  1. 计算当前帧真实耗时 frameTime
  2. 截断到 MaxFrameTime（默认 0.1s，防止长时间挂起）
  3. 累加到 accumulatedTime
  4. 以 fixedLogicFrameTime 为步长循环：
     a. 推进 HNLogicTime (Time += delta, LogicFrameCount++)
     b. 执行 Tick/LateTick
     c. 减少 accumulatedTime
     d. 步数达到 MaxStepsPerFrame 时停止（防止死循环）
```

### 配置参数

所有参数通过 `HNUnityFrameworkGlobalSettings` 配置：

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `LogicRateMode` | `Custom` | `Custom`：固定帧率模式；`NoLimit`：不限帧率 |
| `LogicRate` | `60` | 目标逻辑帧率（仅 Custom 模式） |
| `MaxStepsPerFrame` | `5` | 单帧最大逻辑步数，防止螺旋式追赶死循环 |
| `MaxFrameTime` | `0.1f` | 单帧最大时间预算（秒），超过则截断 |

### 编辑器配置

通过 Unity 菜单 **Edit → Project Settings → HN Unity Framework** 打开设置面板，可直接修改以上参数。

## HNLogicTime — 逻辑时间

```csharp
double currentTime = HNLogicTime.Time;          // 当前逻辑时间（秒）
double deltaTime = HNLogicTime.DeltaTime;        // 当前逻辑帧间隔
ulong frameCount = HNLogicTime.LogicFrameCount;  // 累计逻辑帧数
```

这些属性为 `internal set`，仅框架内部可写入，外部模块只读。

## ITickable 接口

```csharp
public interface ITickable
{
    void Tick();
    void LateTick();
}
```

所有需要帧驱动的模块均实现此接口，由框架主循环统一调度。

## 注意事项

- `HNUnityFramework` 是 **abstract class**，不能直接挂载，必须继承
- 重写 `OnAwake()` / `OnStart()` 时**必须调用 `base` 方法**
- 逻辑帧独立于 `Time.timeScale`，不受时间缩放影响
- 全局设置通过 Addressables 加载，确保 `HNUnityFrameworkGlobalSettings.asset` 已标记为 Addressable
