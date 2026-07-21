---
sidebar_position: 23
---

# 任务/成就系统（Quest）

## 概述

任务/成就系统（L8）提供了一套事件驱动的任务与成就生命周期管理方案，基于 C7 EventBus 和 C1 MemoryPack，包含五个核心子系统：

| 模块 | 说明 | 独立使用 |
|------|------|:-------:|
| **Quest** | 任务生命周期管理（QuestSystem + QuestInstance） | ✅ 可独立 |
| **Achievement** | 成就系统（AchievementSystem + AchievementInstance） | ✅ 可独立 |
| **Condition** | 条件引擎（Counter/State/MultiCounter 条件评估） | 依赖 Quest |
| **Reward** | 奖励系统（RewardProcessor + IRewardHandler） | ✅ 可独立 |
| **QuestChain** | 任务链（线性/分支推进） | 依赖 Quest |

所有子系统通过 `QuestManager<TId>` 统一整合，实现 `ITickable` 驱动计时逻辑，并通过 C7 EventBus 向游戏全局发布任务/成就状态变更事件。

## 命名空间

**Core 层**：`HN.Framework.Core.Level.Logic.Quest`
**Unity 层**：`HN.Framework.Unity.Level.View.Quest`

## 依赖

- **C7 EventBus**（必选）— 事件驱动条件评估 + 任务/成就状态事件发布
- **C1 MemoryPack**（必选）— QuestInstance / AchievementInstance / QuestDef 等数据序列化
- **L4 Sheet**（推荐）— 配置表加载（QuestDef / AchievementDef / ConditionDef 等）
- **L3 Entity**（可选）— 通过 `ownerId` 关联任务/成就拥有者
- **C4 IAssetManager**（可选）— 奖励中的物品资源加载

## Quest — 任务子系统

### 五态状态机

任务从创建到终结经历 5 种状态：

```
Locked → Active → Completed → Claimed
                   ↘ Failed
```

| 状态 | 说明 |
|------|------|
| `Locked` | 未解锁（前置任务未完成或等级不够） |
| `Active` | 进行中，条件追踪中 |
| `Completed` | 条件全部满足，等待领取奖励 |
| `Claimed` | 已领取奖励（终态） |
| `Failed` | 已失败（超时或外部触发，终态） |

### 任务定义

任务定义（`QuestDef`）是配置表行定义，由 L4 Sheet + Luban 加载，MemoryPack 可序列化：

```csharp
// 从配置表获取任务定义
QuestDef questDef = sheetManager.GetTable<int, QuestDef>("tb_quest").Get(1001);

// 任务定义包含：
// - Id: 1001
// - Name: "击败史莱姆"
// - Description: "击败 10 只史莱姆"
// - PrerequisiteQuestIds: [1000]（前置任务）
// - RequiredLevel: 5
// - TimeLimit: Fixed64?（限时任务，null 无限制）
// - ConditionGroupIds: [1]（条件组引用）
// - RewardGroupIds: [101]（奖励组引用）
// - IsRepeatable: false
```

### 接受任务

通过 `QuestManager.TryAcceptQuest` 接受任务，内部校验前置条件、等级、任务链约束和重复接取：

```csharp
QuestHandle handle = questManager.TryAcceptQuest(questId: 1001, ownerId: playerId);
if (handle.IsValid)
{
    Console.WriteLine($"任务已接受：handle={handle.Id}");
}
```

### 查询与进度更新

```csharp
// 通过句柄获取任务实例
QuestInstance? quest = questManager.GetQuest(handle);
if (quest != null)
{
    Console.WriteLine($"状态: {quest.State}, 进度: {quest.Progress?.Count ?? 0}");
}

// 手动推进进度（通常由 ConditionEvaluator 自动驱动）
questManager.UpdateQuestProgress(handle, "kills", delta: 1);
```

### 完成与领取

任务条件全部满足后，`QuestManager` 自动调用 `CompleteQuest` 将状态从 Active 转为 Completed。领取奖励需手动调用：

```csharp
// 完成任务（通常由 ConditionEvaluator 自动触发）
questManager.CompleteQuest(handle);

// 领取奖励
RewardDeliveryResult result = questManager.ClaimQuest(handle);
Console.WriteLine($"奖励发放: {(result.Success ? "成功" : "失败")}");
```

### 失败处理

```csharp
// 外部强制失败
questManager.FailQuest(handle);

// 超时由 QuestSystem.Tick 自动检测
// QuestDef.TimeLimit 非 null 时，每帧扣除 TimeRemaining
```

## Achievement — 成就子系统

### 四态状态机

成就从隐藏到领取经历 4 种状态：

```
Hidden → Revealed → Completed → Claimed
```

| 状态 | 说明 |
|------|------|
| `Hidden` | 隐藏成就，需条件触发后才揭示 |
| `Revealed` | 已揭示，条件追踪中 |
| `Completed` | 条件满足，等待领取奖励 |
| `Claimed` | 已领取奖励（终态） |

> **注意**：成就是一次性的，不支持重复完成，无 Failed 状态。

### 成就定义

```csharp
AchievementDef def = sheetManager.GetTable<int, AchievementDef>("tb_achievement").Get(2001);

// 成就定义包含：
// - Id: 2001
// - CategoryId: 1（分类）
// - Name: "百人斩"
// - Description: "击败 100 个敌人"
// - IsHidden: false（是否隐藏成就）
// - SortOrder: 10（排序权重）
// - PrerequisiteAchievementIds: null（前置成就）
// - ConditionGroupIds: [10]（条件组）
// - RewardGroupIds: [201]（奖励组）
```

### 完成与领取

```csharp
// 完成成就（隐藏成就自动 Reveal → Complete）
questManager.CompleteAchievement(achievementId: 2001, ownerId: playerId);

// 领取奖励
RewardDeliveryResult result = questManager.ClaimAchievement(2001, playerId);

// 查询
AchievementInstance? ach = questManager.GetAchievement(2001, playerId);
float progress = ach?.ProgressPercent ?? 0f;
```

## Condition — 条件引擎

### 条件类型

| 类型 | 说明 | 评估方式 |
|------|------|---------|
| `Counter` | 累计计数（击杀数、收集数等） | 事件驱动自动递增，TargetCount 比较 |
| `State` | 状态检查（等级、持有物品等） | 通过 stateChecker 委托外部评估 |
| `MultiCounter` | 多重计数（多个子计数器 AND） | 保留扩展 |

### 条件定义

条件参数存储在 `ConditionDef.Parameters` 键值对中：

```csharp
// Counter 条件示例
var counterDef = new ConditionDef
{
    Id = 1,
    Type = ConditionType.Counter,
    Parameters = new Dictionary<string, string>
    {
        { "EventTypeName", "MonsterKilled" },  // 监听的事件类型
        { "TargetCount", "10" }                 // 目标计数
    }
};

// State 条件示例
var stateDef = new ConditionDef
{
    Id = 2,
    Type = ConditionType.State,
    Parameters = new Dictionary<string, string>
    {
        { "StateKey", "PlayerLevel" },  // 状态键名
        { "ExpectedValue", "10" }       // 期望值
    }
};
```

### 条件组

条件组内所有条件为 AND 关系，多个条件组之间为互斥 OR 关系：

```csharp
var group = new ConditionGroupDef
{
    Id = 1,
    ConditionIds = new List<int> { 1, 2, 3 },  // 组内全部满足
    Description = "主线第一章完成条件"
};
```

`ConditionGroupDef` 是 MemoryPackable struct，由 Luban 生成。

### 组合条件

`CompositeCondition` 支持 And/Or/Not 三种逻辑运算符组合多个子条件：

```csharp
var composite = new CompositeCondition
{
    Id = 100,
    Operator = CompositeOperator.And,
    SubConditionIds = new List<int> { 1, 2 }
};
```

### 显式注册 Counter 事件类型

Counter 条件通过 EventBus + 事件类型名称定位。项目方需通过 `RegisterCounterEventType<T>` API 将游戏事件类型注册到条件系统：

```csharp
// 注册游戏事件到条件系统
questManager.RegisterCounterEventType<MonsterKilledEvent>("MonsterKilled");
questManager.RegisterCounterEventType<ItemCollectedEvent>("ItemCollected");

// 之后在游戏中发布事件即可自动驱动 Counter 条件
eventBus.Publish(new MonsterKilledEvent { MonsterId = 1 });

// 若不使用 EventBus，可通过 NotifyGameEvent 手动通知
questManager.NotifyGameEvent("MonsterKilled", ownerId: playerId);
```

## Reward — 奖励系统

### 奖励类型

| 类型 | 说明 | TargetId 含义 |
|------|------|--------------|
| `Item` | 物品奖励 | 物品 ID |
| `Currency` | 货币奖励 | 货币 ID |
| `Experience` | 经验奖励 | 经验类别 ID |
| `Attribute` | 属性奖励 | 属性 ID |
| `Unlock` | 解锁内容/功能 | 解锁内容 ID |
| `Custom` | 自定义类型 | 项目自定义 |

### 奖励定义与发放

```csharp
// 奖励配置
var rewardDef = new RewardDef
{
    Id = 101,
    Type = RewardType.Item,
    TargetId = 1001,   // 物品 ID
    Amount = 5,         // 数量
    Probability = 0.8f  // 80% 概率掉落
};

// 实现自定义处理器
public class ItemRewardHandler : IRewardHandler<uint>
{
    public RewardType HandledType => RewardType.Item;

    public bool CanHandle(RewardDef reward) => reward.Type == RewardType.Item;

    public RewardDeliveryResult Deliver(RewardDef reward, uint recipient)
    {
        // 发放物品到背包
        backpack.TryAddItem(reward.TargetId, reward.Amount);
        return RewardDeliveryResult.SuccessResult();
    }
}
```

### RewardProcessor

```csharp
var processor = new RewardProcessor<uint>();
processor.RegisterHandler(new ItemRewardHandler());
processor.RegisterHandler(new CurrencyRewardHandler());

// 批量发放奖励
var result = processor.ProcessRewards(
    rewardGroupIds: new List<int> { 101, 102 },
    getRewardsForGroup: groupId => sheetManager.GetRewards(groupId),
    recipient: playerId);
```

`QuestManager` 内部已集成 `RewardProcessor`，领取奖励时自动调用。

## QuestChain — 任务链

任务链将多个任务按顺序组织，支持线性推进和基于条件的分支选择：

### 三态推进

```
Locked → Active → Completed
```

### 任务链定义

```csharp
var chainDef = new QuestChainDef
{
    Id = 1,
    Name = "主线第一章",
    QuestIds = new List<int> { 1001, 1002, 1003 },
    BranchConditionIds = new List<int> { 0, 50, 0 },  // 仅第二个任务分支
    DefaultBranchIndex = 1
};
```

`QuestChainInstance` 实现 `IReference`，通过 ReferencePool 复用。`QuestManager.ClaimQuest()` 内部自动检查任务链推进——当前任务 Claimed 后自动 `TryAcceptQuest` 下一个任务。

## QuestManager — 统一管理器

`QuestManager<TId>` 是五大子系统的统一入口，实现 `IQuestManager<TId>` 和 `ITickable`：

```csharp
public sealed class QuestManager<TId> : IQuestManager<TId>, ITickable
    where TId : IEquatable<TId>
{
    // 构造函数注入 EventBus + stateChecker + storage
    public QuestManager(
        IEventBus eventBus,
        Func<string, string, bool>? stateChecker = null,
        IStorageProvider? storage = null);
}
```

### 初始化流程

```csharp
var questManager = new QuestManager<int>(eventBus, stateChecker, storage);
questManager.Initialize(
    questDefs: sheetManager.GetAll<QuestDef>(),
    achievementDefs: sheetManager.GetAll<AchievementDef>(),
    conditionGroups: sheetManager.GetAll<ConditionGroupDef>(),
    rewards: sheetManager.GetAll<RewardDef>(),
    questChains: sheetManager.GetAll<QuestChainDef>());

// 注册 Counter 事件类型
questManager.RegisterCounterEventType<MonsterKilledEvent>("MonsterKilled");
```

### EventBus 集成

`QuestManager` 通过 C7 EventBus 发布以下事件：

| 事件 | 触发时机 |
|------|---------|
| `QuestManagerInitializedEvent` | 所有子系统初始化完毕 |
| `QuestAcceptedEvent<TId>` | 任务被接受 |
| `QuestStateChangedEvent<TId>` | 任务状态变更 |
| `QuestProgressUpdatedEvent` | 任务进度更新 |
| `QuestCompletedEvent<TId>` | 任务完成 |
| `QuestClaimedEvent<TId>` | 任务奖励领取 |
| `QuestFailedEvent<TId>` | 任务失败 |
| `AchievementStateChangedEvent<TId>` | 成就状态变更 |
| `AchievementRevealedEvent<TId>` | 隐藏成就揭示 |
| `AchievementProgressUpdatedEvent` | 成就进度更新 |
| `AchievementClaimedEvent<TId>` | 成就奖励领取 |
| `RewardClaimedEvent<TId>` | 奖励发放 |
| `QuestChainStateChangedEvent<TId>` | 任务链状态变更 |

### ITickable 驱动

`QuestManager.Tick()` 每帧调用 `QuestSystem.Tick(deltaTime)`，处理限时任务的超时检测。

QuestManager 通过 `GameWorld.QuestManager` 属性注入到 GameWorld，GameWorld 在 Tick 循环末尾自动驱动：

```csharp
// 注入（在 GameWorldDriver.OnRegisterGameModules 或 GameEntry 中）
world.QuestManager = new QuestManager<int>(eventBus);

// GameWorld.Tick() 中自动驱动
// ...
QuestManager?.Tick();
```

QuestManager 内部使用 `HNLogicTime.DeltaTime` 确保与 GameWorld 其他模块保持相同的时间基准和暂停语义：

```csharp
// QuestManager.Tick() 内部实现
public void Tick()
{
    var deltaTime = Fixed64.FromDouble(HNLogicTime.DeltaTime);
    _questSystem.Tick(deltaTime);
}
```

## Unity 桥接 — QuestManagerBridge

`QuestManagerBridge` 遵循 View-Bridge 模式，挂载到 Entity 的 GameObject 上，桥接 Core 层 `IQuestManager<uint>`。
项目代码通过 `GameWorld.QuestManager` 注入 QuestManager 实例后，GameWorld 会自动驱动其 Tick。QuestManagerBridge 负责
资源加载和存档持久化桥接。

```csharp
// 创建桥接组件
var bridgeObj = new GameObject("QuestBridge");
var bridge = bridgeObj.AddComponent<QuestManagerBridge>();
bridge.Initialize(entityId: playerEntityId);
bridge.SetQuestManager(questManager);  // 由 GameWorld 或管理器注入

// UI 面板通过桥接获取管理器引用
IQuestManager<uint>? mgr = bridge.GetQuestManager();
QuestInstance? quest = mgr?.GetQuest(handle);
```

## 序列化

`QuestInstance` 和 `AchievementInstance` 使用 MemoryPack source generator 的 `[MemoryPackable]` attribute 标记为 `partial class`，无需自定义格式化器。`QuestDef`、`AchievementDef`、`ConditionDef`、`ConditionGroupDef`、`RewardDef`、`QuestChainDef` 均为 MemoryPackable partial struct。

```csharp
// 序列化任务实例
byte[] data = MemoryPackSerializer.Serialize(questInstance);

// 反序列化
QuestInstance restored = MemoryPackSerializer.Deserialize<QuestInstance>(data);
```

## 完整示例

```csharp
// 1. 创建 QuestManager
var eventBus = new EventBus();
Func<string, string, bool> stateChecker = (key, expected) =>
{
    return key switch
    {
        "PlayerLevel" => int.TryParse(expected, out var lvl) && playerLevel >= lvl,
        _ => false,
    };
};

var questManager = new QuestManager<int>(eventBus, stateChecker);

// 2. 初始化配置表
questManager.Initialize(questDefs, achievementDefs, conditionGroups, rewards, questChains);

// 3. 注册游戏事件
questManager.RegisterCounterEventType<MonsterKilledEvent>("MonsterKilled");
questManager.RegisterCounterEventType<ItemCollectedEvent>("ItemCollected");

// 4. 接受任务
QuestHandle handle = questManager.TryAcceptQuest(questId: 1001, ownerId: playerId);

// 5. 游戏逻辑中发布事件驱动条件（Counter 自动递增）
eventBus.Publish(new MonsterKilledEvent { MonsterId = 101, KillerId = playerId });

// 6. 条件满足后自动完成 → 领取奖励
var quest = questManager.GetQuest(handle);
if (quest?.State == QuestState.Completed)
{
    RewardDeliveryResult result = questManager.ClaimQuest(handle);
    Console.WriteLine($"奖励: {(result.Success ? "领取成功" : "领取失败")}");
}

// 7. 完成成就
questManager.CompleteAchievement(achievementId: 2001, ownerId: playerId);
questManager.ClaimAchievement(2001, playerId);

// 8. 查询统计
Console.WriteLine($"活跃任务: {questManager.ActiveQuestCount}");
Console.WriteLine($"已完成成就: {questManager.CompletedAchievementCount}");
```
