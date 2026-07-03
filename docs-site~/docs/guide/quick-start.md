---
sidebar_position: 3
---

# 快速入门

本文档将带你快速上手 HNUnityFramework，涵盖框架初始化、流程管理和资源加载。

## 1. 创建框架入口

框架的核心入口是 `GameWorldDriver`（继承自 `MonoBehaviour`）。你需要创建自己的 GameEntry 类继承它，并重写 `OnRegisterGameModules` 方法来注册游戏专属模块：

```csharp
using HN.Framework.Unity.Driver.Platform;
using HN.Framework.Core.Driver;
using UnityEngine;

public class GameEntry : GameWorldDriver
{
    protected override void OnRegisterGameModules(GameWorld world)
    {
        // 在此注册游戏特定的 Controller、Procedure 等模块
        Debug.Log("GameEntry 初始化完成");

        // 例如注册游戏 Controller
        // world.ControllerManager.RegisterController(new BattleController(world));
    }
}
```

将 `GameEntry` 脚本挂载到场景中的任意 GameObject 上即可。

> `GameWorldDriver.Awake` 会自动创建 `GameWorld` 实例，注入平台适配层（日志、资源管理等），然后调用 `OnRegisterGameModules` 让你注册游戏专属模块，最后调用 `GameWorld.Initialize()`。无需手动初始化任何管理器。

## 2. 框架生命周期

框架启动后按以下顺序执行：

```
GameWorldDriver.Awake → 创建 GameWorld 实例
                      → 注入 UnityLogProvider、AssetManager 等平台实现
                      → OnRegisterGameModules(GameWorld world)   ← 你在此注册游戏模块
                      → GameWorld.Initialize()
                      →   ProcedureManager.Initialize() → ControllerManager.Initialize()

每帧 Update       → GameWorld.Tick()
每帧 LateUpdate   → GameWorld.LateTick()
```

`GameWorld.Tick()` 内部按顺序驱动各管理器：

```
PoolManager.Tick() → ProcedureManager.Tick() → ControllerManager.Tick() → AssetManager?.Tick()
```

框架使用**固定步长逻辑帧**循环，独立于 Unity 的 `Time.timeScale`。所有管理器由 `GameWorld` 统一驱动，无需手动调用初始化或关闭方法。

## 3. 创建第一个 Procedure 流程

Procedure 是框架推荐的游戏顶层流程管理方式。创建第一个流程状态：

```csharp
using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver;
using UnityEngine;

public class GameStartProcedure : ProcedureState
{
    /// <summary>
    /// 所属的 GameWorld 实例，注册时通过 AddState 返回值注入
    /// </summary>
    public GameWorld World { get; set; }

    public override void Initialize(string name)
    {
        base.Initialize(name);
        EnterEvent += OnEnter;
        TickEvent += OnTick;
    }

    private void OnEnter()
    {
        Debug.Log("进入游戏启动流程");
    }

    private void OnTick()
    {
        // 加载完成后切换到主菜单
        if (/* 加载完成条件 */)
        {
            World.ProcedureManager.ChangeState("MainMenu");
        }
    }

    public override void Clear()
    {
        EnterEvent -= OnEnter;
        TickEvent -= OnTick;
        base.Clear();
    }
}
```

在 `OnRegisterGameModules` 中注册并启动流程：

```csharp
protected override void OnRegisterGameModules(GameWorld world)
{
    // 注册流程状态，AddState<T> 返回创建的实例以便注入 World
    var startup = world.ProcedureManager.AddState<GameStartProcedure>("GameStart");
    startup.World = world;

    world.ProcedureManager.AddState<MainMenuProcedure>("MainMenu");

    // 启动流程
    world.ProcedureManager.Start("GameStart");
}
```

> 所有 Procedure 相关 API 均通过 `world.ProcedureManager` 实例调用，不再使用静态方法。

## 4. 资源加载

框架推荐直接使用 Unity Addressables 原生 API：

```csharp
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// 异步加载
var handle = Addressables.LoadAssetAsync<GameObject>("MyPrefab");
handle.Completed += (op) =>
{
    if (op.Status == AsyncOperationStatus.Succeeded)
    {
        Instantiate(op.Result);
    }
};
```

> ⚠️ 框架原有的 `AssetManager` 类已弃用，推荐直接使用 Addressables。详见 [资源管理](./asset-manager.md)。

## 5. 使用对象池

对象池由 `GameWorld` 自动创建，通过 `world.PoolManager` 访问：

```csharp
// 创建对象池
var pool = world.PoolManager.CreateObjectPool<MyObjectPool>("MyPool");

// 获取和归还
var obj = pool.Acquire();
pool.Release(obj);
```

详细用法见 [对象池](./object-pool.md)。

## 下一步

- 了解各模块的详细用法：[Core](./core.md) | [MVC](./mvc.md) | [对象池](./object-pool.md) | [HFSM](./hfsm.md)
- 查看 [架构文档](/dev/architecture) 理解框架设计
- 查阅 [API 文档](/api/) 获取完整 API 参考
{/* OMO_INTERNAL_INITIATOR */}
