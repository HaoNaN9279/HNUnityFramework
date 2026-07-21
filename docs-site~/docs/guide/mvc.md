---
sidebar_position: 5
---

# MVC 模块

MVC 模块提供了 Model-View-Controller 分层架构的实现。

## 架构概览

```
GameWorld
  └── ControllerManager (实例，注册中心)
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
| `ControllerManager` | GameWorld 持有的实例，Controller 的注册中心和帧驱动入口 |

## 数据绑定

### IReadOnlyModel\<T\>

`IReadOnlyModel<T>` 接口提供只读数据的访问与变化通知能力。View 层可通过它订阅数据变化，无需轮询或依赖 ModelManager。

```csharp
public interface IReadOnlyModel<T>
{
    T Value { get; }
    event Action<T> OnValueChanged;
}
```

框架提供默认实现 `ReadOnlyModel<T>`，通过 `SetValue()` 更新值，仅在值变化时触发通知：

```csharp
var health = new ReadOnlyModel<int>(100);
health.SetValue(80); // 仅当值变化时触发 OnValueChanged
```

### 在 Controller 中暴露可观察数据

```csharp
public class BattleController : Controller
{
    private BattleModel model;

    public IReadOnlyModel<int> Health => model.HealthModel;

    public BattleController(BattleModel model)
    {
        this.model = model;
    }

    protected override void OnInitialize()
    {
        model.Initialize();
    }

    protected override void OnClear()
    {
        model.Clear();
    }
}

public class BattleModel : Model
{
    public ReadOnlyModel<int> HealthModel { get; } = new ReadOnlyModel<int>(100);

    public void TakeDamage(int damage)
    {
        HealthModel.SetValue(HealthModel.Value - damage);
    }
}
```

### 在 View 中订阅数据

通过 `PropertyBinder` 将 IReadOnlyModel 绑定到 UI 更新回调。View 继承 `EntityView` 获得 `binder` 实例：

```csharp
using HN.Framework.Unity.Level.View;

public class BattleView : EntityView
{
    [SerializeField] private Text healthText;

    public void Bind(BattleController controller)
    {
        binder.Bind(controller.Health, value =>
        {
            healthText.text = $"HP: {value}";
        });
    }

    private void OnDestroy()
    {
        binder?.UnbindAll();
    }
}
```

> **数据绑定**：通过 IReadOnlyModel + PropertyBinder 实现 View 层的数据订阅，无需 ModelManager。

## 创建数据层

### 定义 ModelUnit

```csharp
using HN.Framework.Core.Level.Logic;

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

    // 单位通过 AddUnit 注册，生命周期由基类自动驱动
    // 可通过 OnInitialize / OnClear 钩子执行自定义逻辑
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

    // 单位通过 AddUnit 注册，基类自动驱动其生命周期
    // 使用 OnInitialize / OnClear 钩子管理 Model

    protected override void OnInitialize()
    {
        model.Initialize();
    }

    protected override void OnClear()
    {
        model.Clear();
    }
}
```

## 注册和生命周期

```csharp
// 获取 GameWorld 实例（通过构造函数或方法参数传入）
var world = GetGameWorld();

// 创建并注册
var playerModel = new PlayerModel();
var playerController = new PlayerController(playerModel);
world.ControllerManager.RegisterController(playerController);

// 初始化
world.ControllerManager.Initialize();

// 退出时注销（自动归还引用池）
world.ControllerManager.UnregisterController(playerController);
```

GameWorld 主循环自动驱动：

```csharp
// 每帧自动调用（由 GameWorld 驱动）
world.ControllerManager.Tick();
world.ControllerManager.LateTick();
```

## 生命周期流程

```
RegisterController
  → Controller.Initialize() → units.Initialize() → Controller.OnInitialize()
  → Model.Initialize() → modelUnits.Initialize() → Model.OnInitialize()

OnFirstFrame → Controller.OnFirstFrame() → Model.OnFirstFrame()
每帧 Tick → Controller.Tick() → Model.Tick() → 各 Unit.Tick()
每帧 LateTick → Controller.LateTick() → Model.LateTick()

退出 → Controller.Clear()
  → Controller.OnClear() → Model.Clear()
  → Model.OnClear() → modelUnits.Clear() → controllerUnits.Clear()
  → 归还到 ReferencePool
```

## 注意事项

- `ModelUnit` 和 `ControllerUnit` 的五个抽象方法必须全部实现
- View 层暂无内置实现，由开发者自行实现
- `Clear()` 中务必调用 `base.Clear()` 以归还引用池
- `AddUnit`/`RemoveUnit` 已内置去重检查
