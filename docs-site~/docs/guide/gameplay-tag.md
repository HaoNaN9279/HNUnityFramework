---
sidebar_position: 23
---

# GameplayTag 层级标签系统

GameplayTag 模块位于 **Core 层** 的 `HN.Framework.Core` 程序集中，属于 Level 层共享基础设施。具体位置见 [架构文档](/dev/architecture)。

## 概述

GameplayTag 是轻量级的层级标签系统，跨模块用于状态标记、条件匹配和权限控制。

- 无层级深度限制（通过索引表 + ParentIndex 链实现）
- 零字符串运行时开销
- 支持层级匹配（`Matches` 亲子关系检查）
- 支持嵌套布尔查询表达式（`TagQuery`：Any/All/Not）

### 核心类型

| 类型 | 说明 |
|------|------|
| `GameplayTag` | 8 字节值类型（TableIndex + InstanceId），零 GC |
| `GameplayTagManager` | 定义表管理，加载标签定义，冻结后不可变更 |
| `GameplayTagContainer` | 标签容器，支持增删查和批量层级匹配 |
| `TagQuery` | 嵌套布尔查询表达式，用于配置表条件匹配 |

## 快速开始

### 1. 定义标签

在配置表中定义层级标签（Luban 配置表）：

```csharp
// 标签定义数据结构（由 GameplayTagDefinition 承载）
// FullName: "State.Combat.Stunned"
// ParentIndex: 指向 State.Combat 的索引
// Depth: 3
```

### 2. 初始化管理器

```csharp
var world = GetGameWorld();  // 通过 GameWorldDriver 获取
var tagManager = world.GameplayTagManager;

// 从配置表加载定义
tagManager.LoadFromDefinitions(tagDefinitions);
tagManager.Freeze();  // 冻结后不可变更
```

### 3. 获取标签

```csharp
// 按完整名称查找
var stunned = tagManager.GetTag("State.Combat.Stunned");
var state = tagManager.GetTag("State");

// 尝试查找
if (tagManager.TryGetTag("Effect.Damage", out var damage))
{
    // 使用 damage 标签
}
```

### 4. 层级匹配

```csharp
// 层级匹配：检查标签是否是其他标签的后代
bool isStunned = stunned.Matches(state);  // true
bool isState = state.Matches(stunned);    // false（逆方向不成立）
```

### 5. 使用容器

```csharp
var container = new GameplayTagContainer();
container.AddTag(stunned);
container.AddTag(damage);

bool hasState = container.HasTag(state);     // true（层级匹配）
bool hasDamage = container.HasTagExact(damage); // true（严格匹配）
```

### 6. 使用查询表达式

```csharp
// 构建查询：Any(All(Stunned, Combat), Not(Damage))
var query = TagQuery.Any(
    TagQuery.All(tagStunned, tagCombat),
    TagQuery.Not(tagDamage)
);

var result = query.Matches(container);
```

## 配置表集成

GameplayTag 定义数据来源于 Luban 配置表，通过 L4 Sheet 系统运行时加载：

```csharp
// 使用 ConfigLoader 加载标签定义
var definitions = ConfigLoader.LoadFromBytes<GameplayTagDefinition[]>(bytes);
world.GameplayTagManager.LoadFromDefinitions(definitions);
world.GameplayTagManager.Freeze();
```

## 最佳实践

- 在游戏启动时一次性加载所有标签定义，Freeze 后不可修改
- `GameplayTag.Matches()` 和 `HasTag()` 是层级匹配（父标签匹配子标签）
- `MatchesExact()` 和 `HasTagExact()` 是严格匹配（仅 TableIndex 相等）
- TagQuery 在配置表中定义，运行时只需评估，不涉及字符串构造
- 容器中的层级匹配通过遍历所有存储标签检查亲子关系实现
