---
sidebar_position: 9
---

# HFSM 层次状态机

HFSM（Hierarchical Finite State Machine）模块提供灵活的层次有限状态机实现，支持条件转换、状态嵌套和动态增删。

## 概述

HFSM 的核心概念：

- **状态**（State）：有 7 个生命周期回调（OnCreate/OnEnter/Update/OnExit/OnDestroy/Initialize/Clear）
- **转换**（Transition）：由 `Func<bool>` 条件委托驱动，条件满足时自动切换状态
- **复合状态**（CompoundState）：一个状态内部包含另一个完整的子状态机（HFSM），实现层级嵌套
- **入口/出口状态**：EntryState 和 ExitState 自动管理状态机生命周期

## 核心类型

| 类型 | 说明 |
|------|------|
| `HFSM` | 主状态机，管理 State 和 Transition 的 CRUD |
| `HFSMState` | 状态基类，提供 7 个生命周期虚方法 |
| `HFSMTransition` | 转换基类，由条件委托驱动 |
| `HFSMCompoundState<T>` | 复合状态，内部嵌套子状态机 |
| `HFSMEntryState` | 入口状态（自动创建，isAllowedTrans=true） |
| `HFSMExitState` | 出口状态（到达后自动 Shutdown） |

## 简单状态机

### 创建状态

```csharp
using HN.Framework;

public class IdleState : HFSMState
{
    public override void OnEnter()
    {
        Debug.Log("进入 Idle 状态");
    }

    public override void Update()
    {
        // 逐帧逻辑
    }

    public override void OnExit()
    {
        Debug.Log("退出 Idle 状态");
    }
}

public class RunState : HFSMState
{
    public override void OnEnter() { /* ... */ }
    public override void Update() { /* ... */ }
    public override void OnExit() { /* ... */ }
}
```

### 组装状态机

```csharp
// 创建状态机（通过 ReferencePool）
var fsm = ReferencePool.Acquire<HFSM>();
fsm.Initialize();

// 添加状态
var idleState = fsm.AddState<IdleState>("Idle");
var runState = fsm.AddState<RunState>("Run");

// 添加转换（Idle → Run，条件：速度 > 0）
fsm.AddTransition<HFSMTransition>(idleState, runState, () => speed > 0);

// 添加转换（Run → Idle，条件：速度 == 0）
fsm.AddTransition<HFSMTransition>(runState, idleState, () => speed <= 0);

// 启动状态机（自动从 EntryState 跳转到 Idle）
fsm.Start("Idle");

// 每帧更新
void Update()
{
    fsm.Update();
}
```

## 条件转换

转换的 `Eval()` 方法检查条件委托，返回 true 则执行切换：

```csharp
// 条件委托为 null 时 = 无条件转换（始终满足）
fsm.AddTransition<HFSMTransition>(stateA, stateB, null);

// 带条件的转换
fsm.AddTransition<HFSMTransition>(stateA, stateB, () => player.Health <= 0);

// 动态替换条件
var transition = fsm.AddTransition<HFSMTransition>(stateA, stateB, oldCondition);
transition.ChangeConditionFunc(newCondition);
```

转换在 `Update()` 主循环中评估：

```
每帧 Update:
  如果 currentState.IsAllowedTrans:
    遍历 currentState.OutputTransitions:
      如果 转换.Active && 转换.Eval() == true:
        currentState.OnExit()
        currentState = 转换.TargetState
        currentState.OnEnter()
        break
```

## 复合状态（层级嵌套）

复合状态允许一个状态内部包含完整的子状态机：

```csharp
public class CombatCompoundState : HFSMCompoundState<HFSM>
{
    public override void Initialize(string name)
    {
        base.Initialize(name);
        
        // 配置子状态机
        var attackState = SubStateMachine.AddState<AttackState>("Attack");
        var defendState = SubStateMachine.AddState<DefendState>("Defend");
        SubStateMachine.AddTransition<HFSMTransition>(attackState, defendState, 
            () => Input.GetKey(KeyCode.Space));
    }
}
```

复合状态的生命周期：

```
进入复合状态:
  OnEnter() → isAllowedTrans = false → 子状态机.Start()
  
每帧 Update:
  子状态机.Update()
  如果子状态机到达 ExitState → isAllowedTrans = true → 允许外部转换
  
退出复合状态:
  OnExit() → 子状态机.Shutdown()
```

## Update 主循环细节

框架内置了**死循环检测**——同一帧内同一状态被执行两次以上会报错并终止状态机。当当前状态为 `ExitState` 时，状态机会自动 `Shutdown()`。

`IsAllowedTrans` 是关键的"门控"机制——状态自行决定何时允许外部转换。EntryState 自动设为 true（以便立刻跳转），复合状态在进入时设为 false（等待子状态机完成）。

## 注意事项

- 所有 HFSM 对象（状态、转换、状态机）通过 `ReferencePool` 管理，创建时用 `Acquire`，不要直接 new
- `HFSMTransition.Initialize()` 自动双向绑定——将自身注册到 fromState 的 OutputTransitions 和 targetState 的 InputTransitions
- 移除状态时会自动清理关联的所有入/出转换
- 复合状态的子状态机也是通过 `ReferencePool` 创建，不要手动 new
- Update 主循环中同一帧只执行**一个**成功转换（找到第一个满足条件的即 break）
