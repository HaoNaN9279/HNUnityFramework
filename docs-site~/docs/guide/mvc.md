---
sidebar_position: 5
---

# MVC 模块

MVC 模块提供了 Model-View-Controller 分层架构的实现。

## 架构概览

```
ControllerManager (单例，全局注册中心)
  ├── Controller A (IController, 逻辑容器)
  │     ├── ControllerUnit (原子行为单元)
  │     └── ControllerUnit
  │     └── 持有 → Model A (IModel, 数据容器)
  │           ├── ModelUnit (原子数据单元)
  │           └── ModelUnit
  ├── Controller B
  └── ...
```

关键设计原则：
- **对称分层**：Controller ↔ Model 完全对称，ControllerUnit ↔ ModelUnit 完全对称
- **组合模式**：Controller/Model 是 Unit 的组合容器，生命周期事件向下委派
- **引用池管理**：所有 Unit 通过 `ReferencePool` 管理，减少 GC 压力
- **无 ModelManager**：Model 的生命周期由对应的 Controller 内部管理

## 核心类型

| 类型 | 说明 |
|------|------|
| `IModel` / `Model` | 数据容器抽象类，管理一组 ModelUnit |
| `IModelUnit` / `ModelUnit` | 原子数据单元抽象类 |
| `IController` / `Controller` | 逻辑容器抽象类，管理一组 ControllerUnit |
| `IControllerUnit` / `ControllerUnit` | 原子行为单元抽象类 |
| `ControllerManager` | 全局单例，Controller 的注册中心和帧驱动入口 |

## 创建数据层

### 定义 ModelUnit

```csharp
using HN.Framework;

public class PlayerDataUnit : ModelUnit
{
    public int Health;
    public float Speed;

    public override void Initialize()
    {
        Health = 100;
        Speed = 5f;
    }

    public override void OnFirstFrame() { }

    public override void Tick()
    {
        // 数据更新逻辑
    }

    public override void LateTick() { }

    public override void Clear()
    {
        Health = 0;
        Speed = 0;
    }
}
```

### 定义 Model

```csharp
public class PlayerModel : Model
{
    public PlayerModel()
    {
        AddUnit(new PlayerDataUnit());
    }

    public override void Initialize() { }
    public override void OnFirstFrame() { }
    public override void Tick() { }
    public override void LateTick() { }

    public override void Clear()
    {
        base.Clear(); // 归还 modelUnits 到引用池
    }
}
```

## 创建逻辑层

### 定义 ControllerUnit

```csharp
public class PlayerInputUnit : ControllerUnit
{
    public override void Initialize() { }
    public override void OnFirstFrame() { }

    public override void Tick()
    {
        if (Input.GetKey(KeyCode.W))
        {
            // 处理输入
        }
    }

    public override void LateTick() { }
    public override void Clear() { }
}
```

### 定义 Controller

```csharp
public class PlayerController : Controller
{
    private PlayerModel model;

    public PlayerController(PlayerModel model)
    {
        this.model = model;
        AddUnit(new PlayerInputUnit());
    }

    public override void Initialize() { model.Initialize(); }
    public override void OnFirstFrame() { model.OnFirstFrame(); }
    public override void Tick() { model.Tick(); }
    public override void LateTick() { model.LateTick(); }

    public override void Clear()
    {
        model.Clear();
        base.Clear();
    }
}
```

## 注册和生命周期

```csharp
// 创建并注册
var playerModel = new PlayerModel();
var playerController = new PlayerController(playerModel);
ControllerManager.RegisterController(playerController);

// 初始化
ControllerManager.Initialize();

// 退出时注销（自动归还引用池）
ControllerManager.UnregisterController(playerController);
```

框架主循环自动驱动：

```csharp
// 每帧自动调用（由 HNUnityFramework 驱动）
ControllerManager.TickControllerManager();
ControllerManager.LateTickControllerManager();
```

## 生命周期流程

```
RegisterController → Controller.Initialize() → Model.Initialize() → 各 Unit.Initialize()
OnFirstFrame → Controller.OnFirstFrame() → Model.OnFirstFrame()
每帧 Tick → Controller.Tick() + Model.Tick() → 各 Unit.Tick()
退出 → Controller.Clear() + Model.Clear() → 归还到 ReferencePool
```

## 注意事项

- `ModelUnit` 和 `ControllerUnit` 的五个抽象方法必须全部实现
- View 层暂无内置实现，由开发者自行实现
- `Clear()` 中务必调用 `base.Clear()` 以归还引用池
- `AddUnit`/`RemoveUnit` 已内置去重检查
