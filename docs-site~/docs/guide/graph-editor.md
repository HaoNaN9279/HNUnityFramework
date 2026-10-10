# E2 Graph Editor

Graph Editor 位于 **Editor 层**，命名空间 `HN.Framework.Editor.Graph`，目录 `Editor/Graph/`。基于 Unity GraphView 构建，与 [L5 图数据模型](./graph.md) 双向同步，是动画状态机、行为树、决策树、效用曲线与 AI 策略编辑器的通用底层。

## 概述

- **双视图同步**：可视化操作（移动 / 连线 / 删除 / 编组）实时回写到 `GraphData`
- **节点类型注册**：实现 `IGraphNodeFactory` 并标注 `[GraphNodeType("路径")]`，`TypeCache` 自动发现
- **可继承**：领域编辑器继承 `GraphEditorWindow`，仅需设置显示名与分类过滤
- **序列化**：`GraphAsset`（`.asset`，MemoryPack 二进制）或 JSON（Newtonsoft）

### 核心类型

| 类型 | 说明 |
|------|------|
| `GraphEditorWindow` | 图编辑器窗口基类（工具栏 + 打开/保存/校验） |
| `GraphEditorView` | `GraphView` 子类，负责数据加载/回写 |
| `GraphNodeView` | 节点可视化元素 |
| `IGraphNodeFactory` / `GraphNodeTypeAttribute` / `GraphNodeRegistry` | 节点类型工厂与注册中心 |
| `GraphNodeSearchProvider` | 节点搜索提供器 |
| `GraphEdgeConnectorListener` | 边连接器监听器（可选扩展） |
| `GraphAsset` / `GraphSerializationUtility` | 序列化 |

## 使用

E2 只提供通用编辑器框架，不含领域编辑器；领域编辑器由对应领域自行实现（见下方「继承编辑器」）。

在编辑器窗口中，右键空白处（或空格）打开节点搜索窗口创建节点（节点来源为已注册的 `IGraphNodeFactory`），拖拽端口连线。

## 注册自定义节点类型

实现 `IGraphNodeFactory` 并标注 `[GraphNodeType]`：

```csharp
using HN.Framework.Core.Level.Logic.Graph;
using HN.Framework.Editor.Graph;

[GraphNodeType("Combat/Skill")]
public sealed class SkillNodeFactory : IGraphNodeFactory
{
    public string TypeId => "Combat.Skill";
    public string DisplayName => "Skill";

    public GraphNode Create(string id, GraphVector2 position)
    {
        var node = new GraphNode(id, TypeId, position);
        node.AddPort(new GraphPort("In", GraphPortDirection.Input, "Flow"));
        node.AddPort(new GraphPort("Out", GraphPortDirection.Output, "Flow"));
        return node;
    }
}
```

`TypeCache` 会自动发现该工厂；路径首段（`Combat`）作为领域分类。

## 继承编辑器

```csharp
using UnityEditor;
using HN.Framework.Editor.Graph;

public sealed class CombatGraphEditor : GraphEditorWindow
{
    [MenuItem(HNUnityFrameworkConstants.FRAMEWORK_NAME + "/Graph/Combat Graph")]
    public static void ShowWindow() => Open<CombatGraphEditor>("Combat Graph");

    protected override string GraphDisplayName => "Combat Graph";
    protected override string CategoryFilter => "Combat"; // 只显示 Combat/* 节点
}
```

## 序列化

保存为 `.asset`（`GraphAsset`，MemoryPack 二进制负载，支持领域派生图类型），或通过工具栏导出 / 导入 JSON：

```csharp
// 代码内使用
GraphSerializationUtility.SaveAsset(graph, "Assets/Graphs/player.asset");
GraphSerializationUtility.ExportJson(graph, "D:/player-graph.json");
GraphData imported = GraphSerializationUtility.ImportJson("D:/player-graph.json");
```

## 相关文档

- 数据模型：[L5 图数据模型](./graph.md)
- 架构说明：[Editor 工具层](../dev/architecture.md)
