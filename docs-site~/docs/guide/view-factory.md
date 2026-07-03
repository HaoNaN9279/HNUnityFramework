---
sidebar_position: 15
---
# 视图工厂
ViewModule 位于架构的 Level.View 层，为 Unity 依赖的视图层基础设施。PropertyBinder 绑定 API 已实现，ViewFactory 与 EntityView 仍为 🚧 Stub 状态。
## 核心类型

| 类型 | 说明 |
|------|------|
| `ViewFactory` | 抽象视图工厂基类，定义视图创建和回收接口 |
| `EntityView` | 实体视图基类（MonoBehaviour），场景中可视化表示的根 |
| `PropertyBinder` | 属性绑定器，实现 Model 到视图字段的自动同步。提供 `Bind<T>()` / `UnbindAll()` 抽象方法 |

## 架构位置

```
Level/
└── View/
    ├── ViewFactory.cs          # 🚧 Stub
    ├── EntityView.cs           # 🚧 Stub
    └── Binding/
        └── PropertyBinder.cs   # 已实现
```

> 所有类型依赖 UnityEngine，不可在 Core 层使用。
