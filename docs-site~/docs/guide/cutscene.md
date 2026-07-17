---
sidebar_position: 23
---

# 过场动画系统

## 概述

C12 过场动画系统基于 Unity Timeline 构建，提供完整的过场动画播放、管理和编辑器支持。核心创新是 `CutsceneBindingResolver`（自定义 `IExposedPropertyTable`），通过角色名而非具体 GameObject 引用进行 Track 绑定，实现 TimelineAsset 与场景解耦。

## 快速开始

### 1. 准备工作

确保 Timeline 资源（.playable）已通过 Addressables 可寻址。

### 2. 标记演员

在场景中为每个过场角色添加 `CutsceneActor` 组件：
```
选择 GameObject → Add Component → HN Framework/Cutscene/CutsceneActor
```
在 Inspector 中设置 `Actor Role` 名称。

### 3. 配置绑定

```csharp
var bindingMap = new CutsceneBindingMap(BindingResolveMode.ScenePath);
bindingMap.AddBinding("Hero", "/Game/Characters/Hero");
bindingMap.AddBinding("NPC1", "/Game/Characters/Merchant");
```

支持四种解析模式：
- `ScenePath` — 通过 `GameObject.Find` 查找
- `Tag` — 通过 Tag 查找
- `EntityId` — 通过 L3 Entity 系统查找
- `ActorComponent` — 通过 `CutsceneActor.ActorTag` 查找

### 4. 播放过场

```csharp
// 获取 CutsceneManager（通过 GameWorld 注入）
var cutsceneManager = world.CutsceneManager as CutsceneManager;

// 播放优先级过场
cutsceneManager.Play("Assets/Cutscenes/Intro", CutscenePriority.Critical);

// 排队播放
cutsceneManager.Enqueue("Assets/Cutscenes/Ending", CutscenePriority.Normal);
```

## 优先级系统

| 优先级 | 值 | 行为 |
|--------|-----|------|
| Critical | 100 | 立即停止所有过场并播放 |
| Important | 50 | 暂停低优先级过场 |
| Normal | 20 | 排队等待 |
| Background | 0 | 空闲时播放 |

播放时自动：
- ✅ Push `IInputBlocker` 阻塞输入
- ✅ 切换到过场摄像机（如已配置 `CutsceneCameraHook`）
- ✅ 完成时恢复摄像机和输入

## 自定义 Track

系统提供 5 种自定义 Timeline Track：

### SubtitleTrack
绑定到 `CutsceneSubtitleDisplay`，自动读取本地化字符串显示字幕。

### DialogueTrack
绑定到 `IEventBus`（通过 GameObject），触发对话事件。

### TimelineEventTrack
发布 `CutsceneEvent` 到 EventBus，可用于触发游戏逻辑。

### ShakeTrack
绑定到 `ICameraManager`（通过 GameObject），触发屏幕震动。

### GameStateTrack
绑定到 `GameObject`，控制组件启用/禁用和 Animator 参数。

## Editor 工具

通过 `HN Framework → Cutscene Editor` 打开编辑器窗口：
- 选择 Timeline Asset 预览
- 编辑角色绑定映射
- 导出绑定配置
