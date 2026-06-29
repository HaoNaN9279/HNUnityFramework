---
sidebar_position: 3
---

# 快速入门

本文档将带你快速上手 HNUnityFramework，涵盖框架初始化、流程管理和资源加载。

## 1. 创建框架入口

HNUnityFramework 的核心是一个抽象类 `HNUnityFramework`（继承自 `MonoBehaviour`）。你需要创建自己的 GameEntry 类继承它：

```csharp
using HN.Framework;
using UnityEngine;

public class GameEntry : HNUnityFramework
{
    protected override void OnAwake()
    {
        base.OnAwake(); // 初始化 ObjectPoolManager、ProcedureManager、ControllerManager
        Debug.Log("GameEntry 初始化完成");
    }

    protected override void OnStart()
    {
        base.OnStart(); // 加载全局设置
        // 在这里启动你的第一个流程
    }
}
```

将 `GameEntry` 脚本挂载到场景中的任意 GameObject 上即可。

## 2. 框架生命周期

框架启动后按以下顺序执行：

```
Awake → HNLogicTime.Initialize()
      → ObjectPoolManager.Initialize()
      → ProcedureManager.Initialize()
      → ControllerManager.Initialize()

Start → 加载 HNUnityFrameworkGlobalSettings（通过 Addressables）
      → 计算 fixedLogicFrameTime

每帧 Update  → LogicTimeUpdate(Tick) → 依次 Tick 各管理器
每帧 LateUpdate → LogicTimeUpdate(LateTick) → 依次 LateTick 各管理器

OnDestroy → ControllerManager.Uninitialize()
          → ProcedureManager.Shutdown()
          → ObjectPoolManager.Uninitialize()
```

框架使用**固定步长逻辑帧**循环，独立于 Unity 的 `Time.timeScale`。默认以 60Hz 的逻辑帧率运行。

## 3. 创建第一个 Procedure 流程

Procedure 是框架推荐的游戏顶层流程管理方式。创建第一个流程状态：

```csharp
using HN.Framework;
using UnityEngine;

public class GameStartProcedure : ProcedureState
{
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
            ProcedureManager.ChangeProcedureState("MainMenu");
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

注册并启动流程：

```csharp
protected override void OnStart()
{
    base.OnStart();
    
    // 注册流程状态
    ProcedureManager.AddProcedureState<GameStartProcedure>("GameStart");
    ProcedureManager.AddProcedureState<MainMenuProcedure>("MainMenu");
    
    // 启动流程
    ProcedureManager.StartProcedure("GameStart");
}
```

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

```csharp
// 初始化管理器
ObjectPoolManager.Initialize();

// 创建对象池
var pool = ObjectPoolManager.CreateObjectPool<MyObjectPool>("MyPool");

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
