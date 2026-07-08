---
sidebar_position: 20
---

# UI 系统使用指南

UI 系统提供基于 7 层 Canvas 的面板栈管理，支持面板生命周期状态机、预设动画和红点树数据模型。

## 7 层 Canvas 架构

UI 系统按照显示优先级分为 7 个层级，每个层级对应独立的 Canvas，通过 Sort Order 控制渲染顺序：

| 层级 | SortOrder | 用途 |
|------|:---------:|------|
| Background | 0 | 场景背景层，放置场景背景 UI |
| Scene | 100 | 3D 场景 UI，如 HUD、血条、标记 |
| UI | 200 | 主界面层，如主菜单、设置界面 |
| Popup | 300 | 弹窗层，模态对话框、确认框 |
| Toast | 400 | 提示层，自动消失的短暂提示 |
| Guide | 500 | 引导层，新手教程覆盖 |
| System | 600 | 系统层，加载画面、错误提示、断线重连 |

层级枚举定义：

```csharp
namespace HN.Framework.Core.Capability.UI
{
    public enum UILayer
    {
        Background = 0,
        Scene      = 100,
        UI         = 200,
        Popup      = 300,
        Toast      = 400,
        Guide      = 500,
        System     = 600
    }
}
```

## UIManager API

`UIManager` 是 UI 系统的核心管理器，实现 `IUIManager`、`ITickable` 和 `IDisposable` 接口。它通过 GameWorld 注入，由 GameWorldDriver 在 Awake 时创建。

```csharp
using HN.Framework.Core.Capability.UI;
using HN.Framework.Unity.Capability.UI;

// 通过 GameWorld 获取 UIManager
IUIManager ui = GameWorld.Instance.UIManager;

// Push — 打开面板并入栈（支持返回键导航）
ui.Push<LoginPanel>();
ui.Push<SettingPanel>();

// Pop — 关闭当前面板并返回上一面板
ui.Pop(); // 从 SettingPanel 返回到 LoginPanel

// Show — 显示面板（不改变栈结构）
ui.Show<HudPanel>();

// Hide — 隐藏面板（不改变栈结构）
ui.Hide<HudPanel>();
```

## UIPanel 生命周期

每个面板都有明确的五种状态，形成完整的状态机循环：

```
      ┌──────────┐
      │  Closed   │
      └────┬─────┘
           │ OnOpen
           ▼
      ┌──────────┐
      │  Opening  │  ← 播放入场动画（OnEnterAnimation）
      └────┬─────┘
           │ 动画完成
           ▼
      ┌──────────┐
      │  Opened   │  ← 面板正常工作
      └────┬─────┘
           │ OnClose
           ▼
      ┌──────────┐
      │  Closing  │  ← 播放出场动画（OnExitAnimation）
      └────┬─────┘
           │ 动画完成
           ▼
      ┌──────────┐
      │  Closed   │
      └──────────┘
```

自定义面板时，重写以下虚方法：

```csharp
using HN.Framework.Core.Capability.UI;
using HN.Framework.Unity.Capability.UI;

public class MyPanel : UIPanel
{
    public override UILayer Layer => UILayer.UI;

    // 面板打开时调用
    protected override void OnOpen()
    {
        // 初始化数据、绑定事件
    }

    // 面板关闭时调用
    protected override void OnClose()
    {
        // 清理数据、解绑事件
    }

    // 面板被暂停（上层面板覆盖）
    protected override void OnPause()
    {
        // 暂停动画、计时器等
    }

    // 面板恢复（上层面板关闭）
    protected override void OnResume()
    {
        // 恢复动画、计时器等
    }

    // 入场动画协程（返回动画持续时间）
    protected override float OnEnterAnimation()
    {
        // 默认实现：无动画，立即完成
        return 0f;
    }

    // 出场动画协程（返回动画持续时间）
    protected override float OnExitAnimation()
    {
        // 默认实现：无动画，立即完成
        return 0f;
    }
}
```

## UIAnimation 预设动画

`UIAnimation` 是一个封装 LitMotion 的静态工具类，提供常用 UI 转场动画：

```csharp
using HN.Framework.Unity.Capability.UI;
using UnityEngine;

public class AnimatedPanel : UIPanel
{
    public override UILayer Layer => UILayer.UI;

    [SerializeField] private RectTransform _content;

    // 淡入动画
    protected override float OnEnterAnimation()
    {
        UIAnimation.FadeIn(_content, duration: 0.3f);
        return 0.3f;
    }

    // 淡出动画
    protected override float OnExitAnimation()
    {
        UIAnimation.FadeOut(_content, duration: 0.2f);
        return 0.2f;
    }
}
```

### 可用动画

| 方法 | 说明 | 参数 |
|------|------|------|
| `FadeIn(target, duration)` | 透明度 0→1 | target: CanvasGroup/RectTransform, duration: float |
| `FadeOut(target, duration)` | 透明度 1→0 | target: CanvasGroup/RectTransform, duration: float |
| `SlideIn(target, direction, duration)` | 从指定方向滑入 | direction: Direction enum (Left/Right/Up/Down) |
| `SlideOut(target, direction, duration)` | 向指定方向滑出 | direction: Direction enum (Left/Right/Up/Down) |
| `ScaleIn(target, duration)` | 缩放 0→1 | target: RectTransform |
| `ScaleOut(target, duration)` | 缩放 1→0 | target: RectTransform |

动画基于 LitMotion 实现（零 GC 分配，Burst 兼容），底层使用 `LMotion.Create` API。

## RedDotNode 红点树

`RedDotNode` 是红点系统的数据模型，以树形结构组织，支持父子节点自动聚合计数：

```csharp
using HN.Framework.Core.Capability.UI;

// 创建根节点
var mailNode = new RedDotNode("mail");
var systemNode = new RedDotNode("system", parent: mailNode);
var teamNode = new RedDotNode("team", parent: mailNode);

// 设置叶子节点计数
systemNode.SetCount(3);  // mailNode.Count → 3（自动聚合）
teamNode.SetCount(1);    // mailNode.Count → 4

// 监听红点变化
mailNode.OnCountChanged += count =>
{
    Debug.Log($"Mail 红点: {count}");
};
```

数据结构：

```
RedDotNode 树状结构：
     mail (count: 4)
     ├── system (count: 3)
     └── team (count: 1)

- 父节点 Count = 所有子节点 Count 之和
- 叶子节点通过 SetCount() 设置显式值
- OnCountChanged 事件在节点或其子节点 Count 变化时触发
```

## 下一步

Phase 1 已完成 7 层 Canvas 栈管理器、面板生命周期和基础动画。Phase 2 计划实现：

- **UIDialog** — 模态弹框，阻挡下层交互，支持确认/取消回调
- **UIToast** — 自动消失的短暂提示，支持时长和动画配置
- **UIGuide** — 新手引导覆盖层，步骤驱动，高亮区域
- **RedDotManager** — 红点管理器运行时，支持路径注册和批量查询
- **Addressables 面板加载** — 支持 Addressables 异步加载面板 Prefab
