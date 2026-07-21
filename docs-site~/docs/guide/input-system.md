---
sidebar_position: 14
---

# 输入系统 (C14)

Input System 模块提供基于 Unity Input System Package 的框架级封装，支持事件驱动的输入动作注册/注销、基于优先级的输入屏蔽、多设备自动检测、触屏虚拟控件等能力。

## 概述

输入系统按依赖拆分为 Core 和 Unity 两层，遵循与其它 Capability 模块一致的分层设计：

| 类型 | 层 | 命名空间 | 说明 |
|------|-----|----------|------|
| `IInputManager` | Core | `HN.Framework.Core.Capability.Input` | 输入管理器接口，定义注册/注销/ActionMap 管理 |
| `IInputBlocker` | Core | `HN.Framework.Core.Capability.Input` | 基于优先级的输入屏蔽接口 |
| `InputAction` | Core | `HN.Framework.Core.Capability.Input` | 输入动作数据模型，实现 `IReference` 可复用 |
| `InputContext` | Core | `HN.Framework.Core.Capability.Input` | 输入事件上下文的只读结构体 |
| `InputTypes` | Core | `HN.Framework.Core.Capability.Input` | `InputActionType` 和 `InputPhase` 枚举定义 |
| `InputManager` | Unity | `HN.Framework.Unity.Capability.Input` | 封装 `UnityEngine.InputSystem`，注入 `GameWorld` |
| `InputBlocker` | Unity | `HN.Framework.Unity.Capability.Input` | 优先级屏蔽栈实现 |
| `DeviceDetector` | Unity | `HN.Framework.Unity.Capability.Input` | 自动检测活跃输入设备并触发回调 |
| `TouchInputAdapter` | Unity | `HN.Framework.Unity.Capability.Input` | 创建 `OnScreenStick` / `OnScreenButton` 虚拟控件 |
| `InputActionAssetLoader` | Unity | `HN.Framework.Unity.Capability.Input` | `.inputactions` JSON 加载和解析工具 |

## 事件驱动架构

与轮询式输入不同，C14 输入系统完全基于事件驱动。`InputManager` 内部通过 `Application.quitting` 和 `domain reload` 处理生命周期，不需要每帧调用 Tick。

```
用户操作 → UnityEngine.InputSystem
         → InputManager (started / performed / canceled 事件)
         → 优先级检查 (InputBlocker)
         → 派发到已注册的回调
```

## 基本使用

### 创建 InputManager

```csharp
using HN.Framework.Unity.Capability.Input;
using UnityEngine.InputSystem;

// 从 .inputactions 资源创建
var asset = Resources.Load<InputActionAsset>("GameInput");
var inputManager = new InputManager(asset);

// 或通过 GameWorld 获取（如果已注册 Capability 模块）
// IInputManager input = world.InputManager;
```

### 注册和注销输入动作

```csharp
// 注册动作
inputManager.RegisterAction("Move", OnMove);
inputManager.RegisterAction("Jump", OnJump);
inputManager.RegisterAction("Fire", OnFire);

// 事件回调
private void OnMove(InputContext ctx)
{
    if (ctx.Phase == InputPhase.Performed)
    {
        Vector2 move = (Vector2)ctx.Value;
        // 处理移动逻辑
    }
}

private void OnJump(InputContext ctx)
{
    if (ctx.Phase == InputPhase.Started)
    {
        // 触发跳跃
    }
}

// 注销动作
inputManager.UnregisterAction("Move", OnMove);
```

### 管理 ActionMap

```csharp
// 启用/禁用整组输入映射
inputManager.EnableActionMap("Gameplay");
inputManager.DisableActionMap("UI");
inputManager.EnableActionMap("Menu");
```

### 输入屏蔽（优先级栈）

```csharp
IInputBlocker blocker = inputManager.Blocker;

// UI 打开时屏蔽低优先级游戏输入
blocker.Push("UIPanel", priority: 10);
blocker.Push("ModalDialog", priority: 20);

// 关闭 UI 后移除屏蔽
blocker.Pop("ModalDialog");
blocker.Pop("UIPanel");

// 清除所有屏蔽
blocker.Clear();
```

### 设备检测

```csharp
var detector = new DeviceDetector();

// 当前活跃设备方案
string scheme = detector.CurrentControlScheme;

// 设备切换时响应
detector.OnControlSchemeChanged += scheme =>
{
    Debug.Log($"输入设备切换为: {scheme}");
    inputManager.SetControlScheme(scheme);
};

// 手动触发检测
detector.Detect();
```

### 触屏虚拟控件

```csharp
var touchAdapter = new TouchInputAdapter();

// 创建虚拟摇杆
var stick = touchAdapter.CreateVirtualStick("Gameplay/Move", canvas.transform as RectTransform);

// 创建虚拟按钮
var button = touchAdapter.CreateVirtualButton("Gameplay/Jump", canvas.transform as RectTransform);

// 销毁所有虚拟控件
touchAdapter.DestroyAll();
```

### 加载 InputActionAsset

```csharp
// 从 JSON 字符串加载
TextAsset jsonAsset = Resources.Load<TextAsset>("Input/GameInput");
InputActionAsset asset = InputActionAssetLoader.LoadFromJson(jsonAsset.text);

// 使用完毕后释放
InputActionAssetLoader.UnloadAsset(asset);
```

## 完整示例

```csharp
using HN.Framework.Unity.Capability.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputInstaller : MonoBehaviour
{
    private InputManager inputManager;
    private DeviceDetector deviceDetector;
    private TouchInputAdapter touchAdapter;

    private void Awake()
    {
        // 加载资源
        var asset = Resources.Load<InputActionAsset>("GameInput");

        // 初始化组件
        inputManager = new InputManager(asset);
        deviceDetector = new DeviceDetector();
        touchAdapter = new TouchInputAdapter();

        // 设备切换联动
        deviceDetector.OnControlSchemeChanged += scheme =>
        {
            inputManager.SetControlScheme(scheme);
            if (scheme == "Touch")
                SetupTouchControls();
        };

        // 注册游戏动作
        inputManager.RegisterAction("Move", OnMove);
        inputManager.RegisterAction("Jump", OnJump);
        inputManager.RegisterAction("Fire", OnFire);
    }

    private void SetupTouchControls()
    {
        var canvas = FindObjectOfType<Canvas>();
        touchAdapter.CreateVirtualStick("Gameplay/Move", canvas.transform as RectTransform);
        touchAdapter.CreateVirtualButton("Gameplay/Jump", canvas.transform as RectTransform);
    }

    private void OnMove(InputContext ctx)
    {
        // 移动逻辑
    }

    private void OnJump(InputContext ctx) { }
    private void OnFire(InputContext ctx) { }

    private void OnDestroy()
    {
        inputManager?.Dispose();
        deviceDetector?.Dispose();
        touchAdapter?.DestroyAll();
    }
}
```

## 生命周期

```
创建:     new InputManager(asset) → BuildActionMapIndex()
注册:     RegisterAction(name, callback) → 绑定 started/performed/canceled
输入:     InputSystem 触发事件 → InputManager 派发 → InputBlocker 过滤 → 回调
注销:     UnregisterAction(name, callback) → 无其余回调时解绑事件
销毁:     Dispose() → 解绑全部事件 → 禁用所有 ActionMap → 清理字典
```

## 注意事项

- **事件驱动模型**：不需要每帧轮询，所有输入通过回调推送。如果一定要在 Update 中读取按键状态，请直接使用 Unity InputSystem 的 API。
- **域重载处理**：`InputManager` 实现了 `IDisposable`，`OnDestroy` 或场景切换时务必调用 `Dispose()`。
- **去重注册**：`RegisterAction` 对同一回调自动去重，多次注册不会产生重复绑定。
- **屏蔽优先级**：`InputBlocker` 的后入优先（LIFO）仅对同优先级有效；跨优先级时始终以高优先级为准。
- **V1 限制**：`SetControlScheme` 为占位实现，`InputActionAssetLoader.LoadFromAsset`（Addressables）暂未实现，`InputDebugger` 可视化面板尚待开发。
- **InputAction 复用**：数据模型实现了 `IReference`，可通过 `ReferencePool` 获取/回收。

## 架构参考

- 最终架构文档：[C14 输入系统](/dev/architecture/#c14--输入系统)
- 通用能力层：[CapabilityModule](/guide/capability)
