---
sidebar_position: 10
---

# Procedure 流程管理

Procedure 模块提供事件驱动的游戏顶层流程管理，适用于管理游戏的不同阶段（启动、主菜单、游戏中、结算等）。

## 概述

Procedure 的核心理念：将游戏的不同阶段抽象为"流程状态"，通过事件订阅模式驱动状态切换。与 HFSM 的区别：

| 特性 | Procedure | HFSM |
|------|-----------|------|
| 驱动方式 | 事件订阅（Enter/Tick/LateTick/Exit） | 虚方法重写（OnEnter/Update/OnExit） |
| 转换方式 | 手动调用 ChangeState | 自动条件评估 + 转换 |
| 适用场景 | 游戏顶层流程（粗粒度） | 具体游戏逻辑（细粒度） |
| 层级支持 | 无 | 支持（CompositeState 嵌套） |

## 核心类型

| 类型 | 说明 |
|------|------|
| `IProcedureState` / `ProcedureState` | 流程状态基类，通过事件驱动 |
| `IProcedureManager` / `ProcedureManager` | GameWorld 持有的流程管理器，通过 `world.ProcedureManager` 访问 |

## ProcedureState 事件

每个流程状态提供四个事件：

```csharp
public class ProcedureState : IProcedureState
{
    public event Action EnterEvent;     // 进入状态时触发
    public event Action TickEvent;      // 每帧更新时触发
    public event Action LateTickEvent;  // 每帧 LateUpdate 时触发
    public event Action ExitEvent;      // 退出状态时触发

    public string Name { get; }
}
```

## 基本使用

### 创建流程状态

```csharp
using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver;
using UnityEngine;

public class GameStartProcedure : ProcedureState
{
    private GameWorld world;

    public override void Initialize(string name)
    {
        base.Initialize(name);
        EnterEvent += OnEnter;
        TickEvent += OnTick;
    }

    /// <summary>
    /// 设置 GameWorld 引用，注册后由调用方注入
    /// </summary>
    public void SetWorld(GameWorld gameWorld) => world = gameWorld;

    private void OnEnter()
    {
        Debug.Log($"进入流程: {Name}");
        // 加载初始资源、显示 Loading 界面等
    }

    private void OnTick()
    {
        // 检查资源加载进度
        if (loadingProgress >= 1.0f)
        {
            world.ProcedureManager.ChangeState("MainMenu");
        }
    }

    public override void Clear()
    {
        // 必须取消订阅事件，防止内存泄漏
        EnterEvent -= OnEnter;
        TickEvent -= OnTick;
        base.Clear();
    }
}
```

### 注册和启动

```csharp
// 注册流程状态
var gameStart = world.ProcedureManager.AddState<GameStartProcedure>("GameStart");
gameStart.SetWorld(world);

world.ProcedureManager.AddState<MainMenuProcedure>("MainMenu");
world.ProcedureManager.AddState<GamePlayProcedure>("GamePlay");

// 启动流程
world.ProcedureManager.Start("GameStart");

// 每帧驱动（由 GameWorld 自动调用）
// world.ProcedureManager.Tick();
// world.ProcedureManager.LateTick();
```

### 状态切换

```csharp
// 通过名称切换
world.ProcedureManager.ChangeState("MainMenu");

// 通过引用切换
var gamePlay = world.ProcedureManager.AddState<GamePlayProcedure>("GamePlay");
world.ProcedureManager.ChangeState(gamePlay);
```

切换流程：

```
当前状态.OnExit() → currentState = 新状态 → 新状态.OnEnter()
```

### 关闭流程

```csharp
world.ProcedureManager.Shutdown();
// 触发当前状态的 ExitEvent → currentState = null
```

## 完整生命周期

```
注册:   AddState<T>(name) → ProcedureState.Initialize(name)
        添加状态到内部字典

启动:   Start(name) → currentState.OnEnter()
        设置当前状态并触发进入事件

每帧:   Tick() → currentState.InvokeTickEvent()
        LateTick() → currentState.InvokeLateTickEvent()

切换:   ChangeState(newName)
        当前状态.ExitEvent → currentState = 新状态 → 新状态.EnterEvent

关闭:   Shutdown() → currentState.ExitEvent → currentState = null

清理:   ClearAll() → 遍历释放所有状态到 ReferencePool
```

## 通过管理器统一访问

```csharp
// 获取当前状态
ProcedureState current = world.ProcedureManager.CurrentState;

// 获取状态数量
int count = world.ProcedureManager.ProcedureStateCount;

// 动态移除状态
world.ProcedureManager.RemoveState("OldState");
```

## 注意事项

- `Clear()` 中**必须取消订阅所有事件**，否则会导致内存泄漏
- ProcedureState 通过 `ReferencePool.Acquire<T>()` 创建，不要直接 new
- Procedure 没有 HFSM 的"条件自动转换"——所有切换需要手动调用 `ChangeState`
- Procedure 适合顶层流程管理（几个互斥的大阶段），HFSM 适合有复杂状态切换的局部逻辑
- `GameWorldDriver.Awake()` 中自动创建 `GameWorld` 并初始化所有内置模块（包括 `ProcedureManager`）
