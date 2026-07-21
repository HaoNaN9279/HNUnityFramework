---
sidebar_position: 25
---

# L10 AI 决策管线

L10 模块提供插件式 AI 决策管线框架，将感知、决策、执行三个阶段统一编排，支持 8 种内置策略并可扩展自定义策略。

## 概述

L10 决策管线的核心设计理念：

- **决策管线**（DecisionPipeline）：按四层优先级（战略 → 任务规划 → 行为执行 → 子状态）顺序评估策略，累加输出动作指令
- **插件式策略**（IDecisionStrategy）：所有策略遵循统一接口，通过注册中心（StrategyRegistry）管理，可按需组合
- **知识池**（KnowledgePool）：Blackboard（Agent 内部状态）、WorldStateCache（环境感知缓存）、PerceptionState（多通道感知数据）三类数据源，通过 IEvaluationContext 统一暴露给策略
- **动作指令**（IActionCommand）：管线产出的原子行为单元，通过 AIAgent 每帧调度执行

## 核心概念

### 核心类型一览

| 类型 | 说明 |
|------|------|
| `AIAgent` | AI 容器，聚合 KnowledgePool、DecisionPipeline、ActionExecutor，提供统一的感知→决策→执行循环 |
| `DecisionPipeline` | 四层决策管线编排器，按 Strategic → TaskPlanning → BehaviorExecution → SubState 顺序评估策略 |
| `StrategyRegistry` | 策略注册中心，支持按名称注册、查询和管理所有可用的 IDecisionStrategy 实例 |
| `StrategyContext` | IEvaluationContext 的实现，聚合 Blackboard、WorldStateCache、PerceptionState 为统一访问入口 |
| `IDecisionStrategy` | 策略接口，提供 Name / Priority / Evaluate(context) / Reset 等标准方法 |
| `IEvaluationContext` | 评估上下文接口，暴露 Blackboard、WorldState、Perception 属性 |
| `IActionCommand` | 动作指令接口，提供 Initialize / Execute / Cancel 生命周期和 Pending → Running → Completed 状态流转 |

### PipelineLayer 四层管线

| 层级 | 枚举值 | 说明 | 推荐策略 |
|------|--------|------|---------|
| 战略层 | `PipelineLayer.Strategic` | 高层目标选择与优先级 | Utility、GOAP |
| 任务规划层 | `PipelineLayer.TaskPlanning` | 任务分解与路径规划 | HTN |
| 行为执行层 | `PipelineLayer.BehaviorExecution` | 具体行为模式执行 | BehaviorTree、DecisionTree |
| 子状态层 | `PipelineLayer.SubState` | 细粒度状态驱动逻辑 | FSM |

### 知识池三类数据源

| 数据源 | 用途 | 关键方法 |
|--------|------|---------|
| `Blackboard` | Agent 内部键值存储 | `Set<T>(key, value)` / `Get<T>(key)` / `TryGet<T>(key, out value)` |
| `WorldStateCache` | 环境感知数据快照（支持过期） | `Set<T>(key, value, lifetime)` / `Get<T>(key)` / `HasKey(key)` |
| `PerceptionState` | 多通道感知聚合（可见/音频/威胁） | `AddVisibleTarget(...)` / `AddAudioTarget(...)` / `AddThreatTarget(...)` |

## 内置策略一览

| 策略名称 | 类名 | 适用场景 |
|---------|------|---------|
| 行为树 | `BehaviorTreeStrategy` | 复杂行为逻辑，需 Sequence/Selector/Parallel 组合、装饰器控制 |
| 有限状态机 | `FSMStrategy` | 明确状态转换逻辑，包装已有 HFSM 为管线策略 |
| 决策树 | `DecisionTreeStrategy` | 简单条件分支决策，深度优先遍历条件→动作 |
| 效用系统 | `UtilityStrategy` | 多因素权衡场景，通过曲线映射和加权评分选择最优行动 |
| 目标导向规划 | `GOAPStrategy` | 目标驱动的自主规划，A* 搜索最优动作序列 |
| 层次任务网络 | `HTNStrategy` | 任务有明确层次分解结构，自顶向下规划 |
| 模糊逻辑 | `FuzzyLogicStrategy` | 连续值推理场景，Mamdani 模糊推理 + 重心法解模糊化 |
| 脚本驱动 | `ScriptedStrategy` | 热更新需求，通过 Lua 等脚本语言实现决策逻辑 |

### 策略选择速查

- **简单条件判断** → DecisionTree
- **明确状态转换** → FSM
- **复杂行为模式** → BehaviorTree
- **多目标权衡** → Utility
- **自主规划** → GOAP 或 HTN
- **连续值推理** → FuzzyLogic
- **热更新需求** → Scripted

## 快速开始

### 创建 Agent 并注册策略

```csharp
using HN.Framework.Core.Level.Logic.AI;
using HN.Framework.Core.Level.Logic.AI.Strategies;
using HN.Framework.Core.Level.Logic.AI.ActionCommands;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

// 1. 创建 AIAgent
var agent = ReferencePool.Acquire<AIAgent>();
agent.Initialize("Warrior_01");

// 2. 创建并注册行为树策略
var btStrategy = new BehaviorTreeStrategy();
btStrategy.Initialize();

// 构建行为树
var root = btStrategy.AddSelector("Root");
var attackSeq = btStrategy.AddSequence("AttackSequence");
attackSeq.AddChild(btStrategy.AddConditional("EnemyNear",
    ctx => ctx.Perception.HasVisibleTargets));
attackSeq.AddChild(btStrategy.AddAction("Attack",
    ctx => ReferencePool.Acquire<AttackAction>()));
root.AddChild(attackSeq);
root.AddChild(btStrategy.AddAction("Idle",
    ctx => ReferencePool.Acquire<WaitAction>()));
btStrategy.SetRoot(root);

// 3. 注册策略并配置管线
agent.Registry.Register(btStrategy);
agent.Pipeline.SetLayer(PipelineLayer.BehaviorExecution, btStrategy);

// 4. 激活 Agent
agent.Activate();

// 5. 每帧调用（由 GameWorld.AISystem 统一驱动）
// 不再需要项目代码手动调用 agent.Tick()，
// GameWorld.Tick() → AISystem.Tick() 自动驱动所有已注册 Agent
// agent.Tick(HNLogicTime.DeltaTime) 内部确保与 GameWorld 时间同步
```

### 向知识池写入数据

```csharp
// 写入 Blackboard
agent.Blackboard.Set("health", 80f);
agent.Blackboard.Set("isAlive", true);

// 写入 WorldStateCache（带过期时间）
agent.WorldState.Set("gate_open", true, 10f); // 10 秒后过期

// 写入 PerceptionState
agent.Perception.AddVisibleTarget(enemyId, 0.9f, 15f,
    new Vector3Data(enemyX, enemyY, enemyZ));
agent.Perception.AddThreatTarget(enemyId, 0.7f, 15f,
    new Vector3Data(enemyX, enemyY, enemyZ));
```

## 策略详解

### BehaviorTreeStrategy — 行为树

行为树策略通过组合节点、装饰节点和动作节点构建决策树，每帧从根节点递归评估并返回单一动作指令。

#### 节点类型

| 节点 | 类型 | 说明 |
|------|------|------|
| `SequenceNode` | 组合节点 | 顺序执行所有子节点，任意子节点失败则整体失败 |
| `SelectorNode` | 组合节点 | 依次尝试子节点，任意子节点成功则整体成功 |
| `ParallelNode` | 组合节点 | 同时评估所有子节点，满足成功阈值时返回成功 |
| `InverterNode` | 装饰节点 | 反转子节点结果（Success ↔ Failure） |
| `RepeaterNode` | 装饰节点 | 重复执行子节点指定次数（-1 为无限重复） |
| `ConditionalDecorator` | 装饰节点 | 条件满足时评估子节点，否则直接返回 Failure |
| `ActionNode` | 叶子节点 | 通过 ActionFactory 生成 IActionCommand |

#### 代码示例

```csharp
var bt = new BehaviorTreeStrategy();
bt.Initialize();

// 构建"战斗行为"子树
var combatSelector = bt.AddSelector("CombatSelector");

// 撤退分支：血量小于 30% 时触发
var retreatSeq = bt.AddSequence("Retreat");
retreatSeq.AddChild(bt.AddConditional("LowHP",
    ctx => ctx.Blackboard.Get<float>("health") < 0.3f));
retreatSeq.AddChild(bt.AddAction("RetreatAction", ctx =>
{
    var cmd = ReferencePool.Acquire<MoveToAction>();
    cmd.TargetPosition = new Vector3Data(0, 0, 0);
    return cmd;
}));

// 攻击分支：敌人可见时触发
var attackSeq = bt.AddSequence("Attack");
attackSeq.AddChild(bt.AddConditional("EnemyVisible",
    ctx => ctx.Perception.HasVisibleTargets));
// 重复执行攻击直到敌人消失
var attackRepeater = bt.AddRepeater("KeepAttacking", -1);
attackRepeater.Child = bt.AddAction("DoAttack",
    ctx => ReferencePool.Acquire<AttackAction>());
attackSeq.AddChild(attackRepeater);

// 巡逻分支：无敌人时默认为巡逻
var patrolSeq = bt.AddSequence("Patrol");
patrolSeq.AddChild(bt.AddConditional("NoEnemy",
    ctx => !ctx.Perception.HasVisibleTargets));
patrolSeq.AddChild(bt.AddAction("PatrolAction", ctx =>
{
    var cmd = ReferencePool.Acquire<MoveToAction>();
    cmd.TargetPosition = new Vector3Data(10, 0, 10);
    return cmd;
}));

// 组装：按优先级编排选择节点
combatSelector.AddChild(retreatSeq); // 优先级最高：先判断撤退
combatSelector.AddChild(attackSeq);  // 其次：判断攻击
combatSelector.AddChild(patrolSeq);  // 兜底：巡逻

bt.SetRoot(combatSelector);
```

### FSMStrategy — 有限状态机

FSM 策略包装 L2 HFSM，为每个状态绑定动作工厂，通过条件转换驱动状态切换。

```csharp
var fsm = new FSMStrategy();
fsm.Initialize();

// 添加状态并绑定动作工厂
var idleState = fsm.AddState("Idle", ctx =>
{
    var cmd = ReferencePool.Acquire<WaitAction>();
    return cmd;
});

var patrolState = fsm.AddState("Patrol", ctx =>
{
    var cmd = ReferencePool.Acquire<MoveToAction>();
    cmd.TargetPosition = GetNextPatrolPoint();
    return cmd;
});

var combatState = fsm.AddState("Combat", ctx =>
{
    return ReferencePool.Acquire<AttackAction>();
});

// 添加条件转换
fsm.AddTransition(idleState, patrolState, () => patrolTimer > PatrolInterval);
fsm.AddTransition(patrolState, combatState, () => HasEnemyDetected());
fsm.AddTransition(combatState, patrolState, () => !HasEnemyDetected());

// 启动状态机
fsm.SetStartState(idleState);
```

### DecisionTreeStrategy — 决策树

基于条件分支的决策模型，深度优先遍历条件节点和动作节点。

```csharp
var dt = new DecisionTreeStrategy();
dt.Initialize();

// 条件节点：是否有敌人
var hasEnemy = dt.AddConditionNode("HasEnemy",
    ctx => ctx.Perception.HasVisibleTargets);

// 条件节点：血量是否安全
var isSafe = dt.AddConditionNode("IsHealthSafe",
    ctx => ctx.Blackboard.Get<float>("health") > 0.5f);

// 条件节点：是否在攻击范围内
var inRange = dt.AddConditionNode("InAttackRange",
    ctx => ctx.Perception.ClosestVisibleDistance < 3f);

// 动作节点
var attack = dt.AddActionNode("Attack", ctx => ReferencePool.Acquire<AttackAction>());
var retreat = dt.AddActionNode("Retreat", ctx =>
{
    var cmd = ReferencePool.Acquire<MoveToAction>();
    cmd.TargetPosition = GetSafePosition();
    return cmd;
});
var patrol = dt.AddActionNode("Patrol", ctx =>
{
    var cmd = ReferencePool.Acquire<MoveToAction>();
    cmd.TargetPosition = GetNextPatrolPoint();
    return cmd;
});

// 构建决策树
hasEnemy.TrueNode = isSafe;
hasEnemy.FalseNode = patrol;
isSafe.TrueNode = inRange;
isSafe.FalseNode = retreat;
inRange.TrueNode = attack;
inRange.FalseNode = patrol;

dt.SetRoot(hasEnemy);
```

### UtilityStrategy — 效用系统

通过多因素加权评分选择最优行动。每个行动包含多个 UtilityFactor，每个因素由评分函数、响应曲线和权重组成。

#### 响应曲线

| 曲线类型 | 枚举值 | 说明 |
|---------|--------|------|
| 线性 | `UtilityCurveType.Linear` | `y = x`，不改变输入形状 |
| S 形 | `UtilityCurveType.SShape` | `y = x² / (x² + (1-x)²)`，中间平缓两端陡峭 |
| 指数 | `UtilityCurveType.Exponential` | `y = x^power`，可通过 `"power"` 参数控制 |
| 反转 | `UtilityCurveType.Inverse` | `y = 1 - x`，高输入对应低效用 |
| 阶梯 | `UtilityCurveType.Step` | 超过阈值返回 1，否则返回 0，阈值通过 `"threshold"` 参数控制 |

#### 代码示例

```csharp
var utility = new UtilityStrategy();

// 攻击行动
var attackAction = new UtilityAction
{
    Name = "Attack",
    ActionFactory = ctx => ReferencePool.Acquire<AttackAction>(),
    Factors = new List<UtilityFactor>
    {
        new UtilityFactor
        {
            Name = "HealthRatio",
            Weight = 1.5f,
            Curve = new UtilityCurve(UtilityCurveType.Inverse),
            ScoreFunction = ctx => ctx.Blackboard.Get<float>("health_ratio")
        },
        new UtilityFactor
        {
            Name = "EnemyProximity",
            Weight = 1.0f,
            Curve = new UtilityCurve(UtilityCurveType.Linear),
            ScoreFunction = ctx => 1f - Math.Min(ctx.Perception.ClosestVisibleDistance / 50f, 1f)
        }
    }
};

// 治疗行动
var healAction = new UtilityAction
{
    Name = "Heal",
    ActionFactory = ctx => ReferencePool.Acquire<UseAbilityAction>(),
    Factors = new List<UtilityFactor>
    {
        new UtilityFactor
        {
            Name = "HealthUrgency",
            Weight = 2.0f,
            Curve = new UtilityCurve(UtilityCurveType.Inverse),
            ScoreFunction = ctx => ctx.Blackboard.Get<float>("health_ratio")
        }
    }
};

utility.AddAction(attackAction);
utility.AddAction(healAction);
utility.Initialize();
```

### GOAPStrategy — 目标导向行动规划

使用 A* 搜索从当前世界状态到达目标状态的最优动作序列。通过 GOAPGoal 定义目标，GOAPAction 定义动作（含前置条件、效果和成本）。

```csharp
var goap = new GOAPStrategy();

// 定义目标：消灭敌人
var killGoal = new GOAPGoal
{
    Name = "KillEnemy",
    Priority = 10,
    GoalConditions = new Dictionary<string, bool>
    {
        { "IsEnemyDead", true }
    }
};

// 定义动作：装备武器
var equipAction = new GOAPAction
{
    Name = "EquipWeapon",
    Cost = 1f,
    Preconditions = new Dictionary<string, bool> { { "HasWeaponInBag", true } },
    Effects = new Dictionary<string, bool> { { "HasWeapon", true } },
    ActionFactory = ctx =>
    {
        var cmd = ReferencePool.Acquire<UseAbilityAction>();
        cmd.AbilityId = 100;
        return cmd;
    }
};

// 定义动作：攻击
var attackAction = new GOAPAction
{
    Name = "Attack",
    Cost = 3f,
    Preconditions = new Dictionary<string, bool> { { "HasWeapon", true } },
    Effects = new Dictionary<string, bool> { { "IsEnemyDead", true } },
    ActionFactory = ctx => ReferencePool.Acquire<AttackAction>()
};

goap.AddGoal(killGoal);
goap.AddAction(equipAction);
goap.AddAction(attackAction);
goap.Initialize();
```

GOAP 的核心特性：

- **A* 规划**：启发函数为当前状态与目标条件的不匹配数，保证找到最优路径
- **跨帧执行**：规划生成的动作序列可在多帧间逐步执行，无需每帧重新规划
- **优先级目标**：多个目标按 Priority 降序逐一尝试，高优先级目标优先
- **动态重规划**：外部条件变化时可调用 `ForceReplan()` 触发重新规划

### HTNStrategy — 层次任务网络

通过 HTNDomain 注册任务和方法，HTNPlanner 自顶向下递归分解复合任务为原始任务序列。

```csharp
var domain = new HTNDomain();

// 注册原始任务
var moveToTask = new HTNTask
{
    Name = "MoveToEnemy",
    Type = HTNTaskType.Primitive,
    ActionFactory = ctx =>
    {
        var cmd = ReferencePool.Acquire<MoveToAction>();
        cmd.TargetPosition = GetNearestEnemyPosition(ctx);
        return cmd;
    }
};
var attackTask = new HTNTask
{
    Name = "AttackEnemy",
    Type = HTNTaskType.Primitive,
    ActionFactory = ctx => ReferencePool.Acquire<AttackAction>()
};
var fleeTask = new HTNTask
{
    Name = "Flee",
    Type = HTNTaskType.Primitive,
    ActionFactory = ctx =>
    {
        var cmd = ReferencePool.Acquire<MoveToAction>();
        cmd.TargetPosition = GetSafePosition();
        return cmd;
    }
};

domain.RegisterTask(moveToTask);
domain.RegisterTask(attackTask);
domain.RegisterTask(fleeTask);

// 注册复合任务
var combatTask = new HTNTask
{
    Name = "Combat",
    Type = HTNTaskType.Compound
};
domain.RegisterTask(combatTask);

// 注册分解方法
domain.RegisterMethod("Combat", new HTNMethod
{
    Name = "LowHPFlee",
    Condition = ctx => ctx.Blackboard.Get<float>("health") < 0.25f,
    Subtasks = new List<HTNTask> { fleeTask }
});
domain.RegisterMethod("Combat", new HTNMethod
{
    Name = "MeleeRush",
    Condition = ctx => ctx.Perception.ClosestVisibleDistance < 3f,
    Subtasks = new List<HTNTask> { attackTask }
});
domain.RegisterMethod("Combat", new HTNMethod
{
    Name = "ApproachAndAttack",
    Subtasks = new List<HTNTask> { moveToTask, attackTask }
});

var htn = new HTNStrategy
{
    Domain = domain,
    RootTaskName = "Combat"
};
htn.Initialize();
```

### FuzzyLogicStrategy — 模糊逻辑

通过 FuzzyVariable 定义模糊变量、FuzzySet 定义模糊集合、FuzzyRule 定义模糊规则，使用 Mamdani 推理 + 重心法解模糊化输出精确决策值。

```csharp
var fuzzy = new FuzzyLogicStrategy();

// 定义输入变量 "distance"（范围 0~100）
var distanceVar = new FuzzyVariable("distance", 0f, 100f);
distanceVar.AddSet(new FuzzySet("Near", MembershipFunctionType.Trapezoid,
    0f, 0f, 20f, 40f));
distanceVar.AddSet(new FuzzySet("Medium", MembershipFunctionType.Triangle,
    50f, 30f));
distanceVar.AddSet(new FuzzySet("Far", MembershipFunctionType.Trapezoid,
    60f, 80f, 100f, 100f));
fuzzy.AddInputVariable(distanceVar);

// 定义输出变量 "aggressiveness"（范围 0~1）
var outputVar = new FuzzyVariable("aggressiveness", 0f, 1f);
outputVar.AddSet(new FuzzySet("Passive", MembershipFunctionType.Trapezoid,
    0f, 0f, 0.2f, 0.35f));
outputVar.AddSet(new FuzzySet("Cautious", MembershipFunctionType.Triangle,
    0.5f, 0.25f));
outputVar.AddSet(new FuzzySet("Aggressive", MembershipFunctionType.Trapezoid,
    0.65f, 0.8f, 1f, 1f));
fuzzy.SetOutputVariable(outputVar);

// 添加模糊规则
fuzzy.AddRule(new FuzzyRule("R1",
    new List<(string, int)> { ("distance", 0) }, // IF distance is Near
    ("aggressiveness", 2)));                      // THEN aggressiveness is Aggressive
fuzzy.AddRule(new FuzzyRule("R2",
    new List<(string, int)> { ("distance", 2) }, // IF distance is Far
    ("aggressiveness", 0)));                      // THEN aggressiveness is Passive

// 配置输入提供者和输出映射
fuzzy.SetInputProvider("distance", ctx => ctx.Perception.ClosestVisibleDistance);
fuzzy.SetOutputMapper(value =>
{
    if (value > 0.6f)
    {
        return ReferencePool.Acquire<AttackAction>();
    }
    return ReferencePool.Acquire<WaitAction>();
});

fuzzy.Initialize();
```

隶属度函数类型：

- **Triangle**（三角）：参数 `[x0, d]`，峰值为 x0，基底从 x0-d 到 x0+d
- **Trapezoid**（梯形）：参数 `[x0, x1, x2, x3]`，平台段 [x1, x2] 恒为 1，两侧线性过渡

### ScriptedStrategy — 脚本驱动

桥接 C15 IScriptEngine，通过 Lua 等脚本语言实现决策逻辑，支持热更新。

```csharp
// 注入脚本引擎
var scriptEngine = GetScriptEngine();

// 创建脚本策略
var scripted = new ScriptedStrategy(
    scriptEngine,
    scriptModule: "ai_combat",
    decisionFuncName: "Evaluate",
    initScript: @"
        -- 初始化全局变量
        ATTACK_THRESHOLD = 10
        FLEE_THRESHOLD = 0.2
    ",
    contextGlobalName: "ai_context"
);

scripted.Initialize();
```

脚本端（Lua 示例）：

```lua
function Evaluate(context)
    local health = context.Blackboard:Get("health_ratio")
    local distance = context.Perception.ClosestVisibleDistance

    -- 血量太低 → 逃跑
    if health < FLEE_THRESHOLD then
        return FleeAction.new()
    end

    -- 距离近 → 攻击
    if distance < ATTACK_THRESHOLD then
        return AttackAction.new()
    end

    return WaitAction.new()
end
```

## 管线配置

DecisionPipeline 按四层顺序评估策略，每层配置一个策略实例。SetLayer 支持动态切换：

```csharp
// 配置多层管线
agent.Pipeline.SetLayer(PipelineLayer.Strategic, utilityStrategy);
agent.Pipeline.SetLayer(PipelineLayer.TaskPlanning, htnStrategy);
agent.Pipeline.SetLayer(PipelineLayer.BehaviorExecution, btStrategy);
agent.Pipeline.SetLayer(PipelineLayer.SubState, fsmStrategy);

// 动态启用/禁用某层
// 方式一：跳过该层
agent.Pipeline.SkipLayer(PipelineLayer.TaskPlanning);

// 方式二：通过策略的 IsEnabled 属性
utilityStrategy.IsEnabled = false;

// 运行时检查管线状态
var activeStrategies = agent.Pipeline.GetActiveStrategies();
bool hasEnabled = agent.Pipeline.HasAnyEnabled();
```

### 典型管线组合

| 场景 | Strategic | TaskPlanning | BehaviorExecution | SubState |
|------|-----------|-------------|-------------------|----------|
| 简单敌人 AI | — | — | BehaviorTree | FSM |
| 战术 AI | Utility | HTN | BehaviorTree | — |
| BOSS AI | — | HTN | BehaviorTree | FSM |
| 脚本热更新 | — | — | Scripted | — |
| 规划型 AI | GOAP | — | — | FSM |

## Unity 集成

### AIAgent 生命周期

L10 Core 层的 AIAgent 为纯 C# 类，Unity 层通过 `AIAgentComponent` 创建并注册到 `GameWorld.AISystem`，由 GameWorld 统一驱动 Tick：

```csharp
using HN.Framework.Core.Level.Logic.AI;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;
using HN.Framework.Unity.Driver.Platform;

public class AIAgentComponent : MonoBehaviour
{
    private AIAgent _agent;

    private void Awake()
    {
        _agent = ReferencePool.Acquire<AIAgent>();
        _agent.Initialize(gameObject.name);

        // 配置策略和管线
        ConfigurePipeline();

        // 注册到 GameWorld.AISystem，由 GameWorld 统一驱动 Tick
        // 替代原来的 MonoBehaviour.Update() 自驱动模式
        var driver = FindFirstObjectByType<GameWorldDriver>();
        driver?.World?.AISystem?.Register(_agent);
    }

    private void Start()
    {
        _agent.Activate();
    }

    // NOTE: 不再需要 Update() 方法。
    // Agent.Tick() 现由 GameWorld.AISystem 每帧驱动，
    // 使用 HNLogicTime.DeltaTime 确保时间同步和暂停语义。

    private void LateUpdate()
    {
        // 在渲染帧末尾填充感知数据，供下一逻辑帧的 AISystem.Tick() 使用
        _agent.ClearPerception();
        UpdatePerception();
    }

    private void UpdatePerception()
    {
        // 填充感知数据示例
        foreach (var enemy in DetectEnemies())
        {
            _agent.Perception.AddVisibleTarget(
                enemy.GetInstanceID(),
                confidence: 0.9f,
                distance: Vector3.Distance(transform.position, enemy.position),
                new Vector3Data(enemy.position.x, enemy.position.y, enemy.position.z)
            );
        }

        // 同步到 WorldState
        _agent.WorldState.Set("player_alive", IsPlayerAlive());
        _agent.Blackboard.Set("health_ratio", currentHealth / maxHealth);
    }

    private void OnDestroy()
    {
        // 从 AISystem 注销，停止 GameWorld 对其的驱动
        var driver = FindFirstObjectByType<GameWorldDriver>();
        driver?.World?.AISystem?.Unregister(_agent);
        ReferencePool.Release(_agent);
        _agent = null;
    }

    private void ConfigurePipeline()
    {
        // 示例：配置行为树 + FSM 双层管线
        var bt = CreateBehaviorTree();
        var fsm = CreateFSM();
        _agent.Registry.Register(bt);
        _agent.Registry.Register(fsm);
        _agent.Pipeline.SetLayer(PipelineLayer.BehaviorExecution, bt);
        _agent.Pipeline.SetLayer(PipelineLayer.SubState, fsm);
    }

    private BehaviorTreeStrategy CreateBehaviorTree()
    {
        // ... 同前面示例
        return new BehaviorTreeStrategy();
    }

    private FSMStrategy CreateFSM()
    {
        // ... 同前面示例
        return new FSMStrategy();
    }

    private Collider[] DetectEnemies()
    {
        return Physics.OverlapSphere(transform.position, 20f, enemyLayerMask);
    }

    private bool IsPlayerAlive()
    {
        return playerHealth > 0;
    }
}
```

### 动作指令的 Unity 层执行

Core 层动作指令（如 MoveToAction、AttackAction）通过 Unity 层适配器转换为实际的 GameObject 操作：

```csharp
// Core 层的纯数据指令
public class MoveToAction : IActionCommand
{
    public Vector3Data TargetPosition;

    public void Execute()
    {
        Status = ActionCommandStatus.Running;
        // 数据设置完毕，由 Unity 层适配器读取并执行
    }
}

// Unity 层适配器（示例）
public class AIActionExecutor : MonoBehaviour
{
    private NavMeshAgent _navAgent;
    private Animator _animator;

    public void ExecuteCommand(IActionCommand command)
    {
        switch (command)
        {
            case MoveToAction move:
                _navAgent.SetDestination(new Vector3(
                    move.TargetPosition.X,
                    move.TargetPosition.Y,
                    move.TargetPosition.Z));
                _animator.SetFloat("Speed", 1f);
                break;

            case AttackAction attack:
                _animator.SetTrigger("Attack");
                break;

            case WaitAction wait:
                _animator.SetFloat("Speed", 0f);
                break;
        }
    }
}
```

## 最佳实践

### 1. 策略选择原则

- 从简单开始：先用 DecisionTree / FSM 快速验证，确认需要时再升级到 BehaviorTree / Utility
- 分层组合：高优先级策略在 Strategic 层做宏观决策，低优先级策略在 BehaviorExecution 层做微观行为
- 避免过拟合：不要为每个角落案例添加规则，保持策略泛化能力

### 2. 知识池使用规范

- **Blackboard 存 Agent 状态**（血量、弹药、当前目标），不存环境快照
- **WorldState 存环境快照**（门是否打开、某个开关状态），支持过期时间避免使用过期数据
- **PerceptionState 每帧清空重填**：在 Tick 之前调用 `ClearPerception()` 确保数据新鲜度
- 不要在策略的 Evaluate 中修改知识池数据——保持评估只读

### 3. 管线性能优化

- 跳过不需要的层：未分配策略的层不会产生开销，但也可以用 `SkipLayer` 显式跳过
- 控制 MaxActionsPerTick：避免单帧产生过多动作指令，默认 5 已足够大多数场景
- 减少 Evaluate 内部分配：策略的 Evaluate 每帧调用，避免在内部创建大量临时对象

### 4. 引用池管理

- AIAgent 通过 `ReferencePool.Acquire<AIAgent>()` 创建，`ReferencePool.Release()` 释放
- BehaviorTreeStrategy 的节点、FSMStrategy 的 HFSM 均通过 ReferencePool 管理
- 动作指令（IActionCommand）建议也从引用池获取，减少 GC 压力
- 在 Agent 的 OnDestroy 中确保所有引用归还

### 5. GOAP 规划注意事项

- 动作和条件数量控制在合理范围（建议 < 50 个动作），避免 A* 搜索爆炸
- 为同类动作设置有区分度的 Cost 值，引导规划器选择最优路径
- 世界状态大幅变化时调用 `ForceReplan()` 放弃当前计划重新规划

### 6. 模糊逻辑调参建议

- 先定义变量值域，再划分模糊集合（通常 3~7 个集合覆盖完整值域）
- 相邻集合应有适当重叠（约 25%~50%），确保过渡平滑
- 规则数量：一般 5~20 条足够覆盖常见场景，避免规则爆炸
- 解模糊化采样点数默认 200，值域较大时可适当调大

### 7. 调试技巧

```csharp
// 在策略 Evaluate 中打印当前决策
public IReadOnlyList<IActionCommand> Evaluate(IEvaluationContext context)
{
    var commands = baseEvaluate(context);
    Debug.Log($"[{Name}] 决策完成，输出 {commands?.Count ?? 0} 条指令");
    return commands;
}

// 检查管线活跃策略
foreach (var s in agent.Pipeline.GetActiveStrategies())
{
    Debug.Log($"活跃策略: {s.Name}, 优先级: {s.Priority}");
}

// 查看知识池状态
Debug.Log($"黑板键数: {agent.Blackboard.Count}");
Debug.Log($"可见目标: {agent.Perception.VisibleTargets.Count}");
```
