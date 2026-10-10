---
sidebar_position: 41
---

# L5 图数据模型

图数据模型位于 **Core 层** 的 `HN.Framework.Core` 程序集，命名空间 `HN.Framework.Core.Level.Logic.Graph`，无 Unity 依赖。它是动画状态机图、行为树图、对话图等领域图的通用底层。

## 概述

- **纯数据**：只描述节点、边、编组，不含运行时求值逻辑
- **可序列化**：全部类型 `[MemoryPackable]` 源生成序列化，可直接落盘 / 网络传输
- **可扩展**：领域图继承 `GraphData` 追加类型化字段
- **可校验**：`Validate(out error)` 检查边端点、端口引用、编组引用与 Id 唯一性

### 核心类型

| 类型 | 说明 |
|------|------|
| `GraphData` | 图根对象：Id / Name + Nodes / Edges / Groups，提供增删查与校验 |
| `GraphNode` | 节点：Id / Type / Title / Position / Ports / Properties |
| `GraphEdge` | 边：`SourceNodeId.SourcePort → TargetNodeId.TargetPort` |
| `GraphPort` | 端口：Name / Direction（Input / Output）/ Type |
| `GraphGroup` | 节点编组：Id / Title / NodeIds / Position / Size（编辑器视觉分组） |
| `GraphVector2` | 二维坐标值类型（X / Y），替代 Core 层不可用的 `UnityEngine.Vector2` |
| `GraphPortDirection` | 端口方向枚举（Input / Output） |

## 快速开始

### 1. 构建图

```csharp
using HN.Framework.Core.Level.Logic.Graph;

var graph = new GraphData("player-fsm", "Player FSM");

var idle = new GraphNode("idle", "State", new GraphVector2(100, 20));
idle.AddPort(new GraphPort("In", GraphPortDirection.Input, "Flow"));
idle.AddPort(new GraphPort("Out", GraphPortDirection.Output, "Flow"));
idle.SetProperty("clip", "Idle");

var run = new GraphNode("run", "State", new GraphVector2(300, 20));
run.AddPort(new GraphPort("In", GraphPortDirection.Input, "Flow"));
run.AddPort(new GraphPort("Out", GraphPortDirection.Output, "Flow"));

graph.AddNode(idle);
graph.AddNode(run);
graph.AddEdge(new GraphEdge("idle", "Out", "run", "In", "e1"));
```

### 2. 查询与修改

```csharp
var node = graph.GetNode("idle");                 // 按 Id 查找节点
var incoming = graph.GetEdgesToNode("run");       // 入边
var outgoing = graph.GetEdgesFromNode("idle");    // 出边

node!.RemovePort("Out", GraphPortDirection.Output);
graph.RemoveNode("idle");                          // 级联删除相连边并从编组移除
```

### 3. 校验与序列化

```csharp
if (!graph.Validate(out var error))
{
    Debug.LogError(error);
}

// MemoryPack 序列化/反序列化
byte[] data = MemoryPackSerializer.Serialize(graph);
GraphData restored = MemoryPackSerializer.Deserialize<GraphData>(data)!;
```

> 领域图对象（继承 `GraphData`）同样可直接用 `MemoryPackSerializer` 序列化。

## 编组

```csharp
var group = new GraphGroup("locomotion", "Locomotion");
group.AddNode("idle");
group.AddNode("run");
group.Position = new GraphVector2(80, 0);
group.Size = new GraphVector2(400, 120);
graph.AddGroup(group);
```

## 领域扩展

`GraphData`、`GraphNode` 均为可继承的非密封类型。后续领域图（如 L6 `AnimationGraph`）继承 `GraphData` 并追加类型化数据，通用编辑能力（E2 Graph Editor）基于本层构建。

## 相关文档

- 架构说明：[06 - 关卡层](/dev/architecture)
- API 参考：[API 索引](/api/)
