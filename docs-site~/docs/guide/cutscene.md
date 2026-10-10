# 过场动画系统

## 概述

C12 过场动画系统基于 Unity Timeline 构建，提供完整的过场动画播放、管理和编辑器支持。核心创新有两个：

1. **`CutsceneBindingResolver`**（自定义 `IExposedPropertyTable`）：通过角色名而非具体 GameObject 引用进行 Track 绑定，实现 TimelineAsset 与场景解耦。
2. **`CutsceneActorRegistry`（`ICutsceneActorRegistry` 的默认实现，实例注册表）**：CutsceneActor 在 OnEnable/OnDisable 生命周期中自动注册/注销，实现 O(1) 字典查找替代传统 `GameObject.Find` / `FindObjectsByType` 的 O(n) 全场景扫描。注册表实例由 `CutsceneManager` 持有，并通过构造函数注入给 `CutsceneBindingResolver`——不使用全局静态状态，便于测试隔离与实现替换。

## 快速开始

### 1. 准备工作

确保 Timeline 资源（.playable）已通过 Addressables 可寻址。

### 2. 标记演员

在场景中为每个过场角色添加 `CutsceneActor` 组件：
```
选择 GameObject → Add Component → HN Framework/Cutscene/CutsceneActor
```
在 Inspector 中设置 `Actor Role` 名称。组件启用时经 `GameWorldDriver` → `CutsceneManager.ActorRegistry` 解析出注册表实例并注册，禁用时从同一实例注销。

> **提示**：推荐始终为过场相关的 GameObject 添加 `CutsceneActor` 组件，以获得 O(1) 查找性能。未添加组件的对象仍可通过 fallback 机制解析（`GameObject.Find` 等），但性能较低。

### 3. 配置绑定

```csharp
var bindingMap = new CutsceneBindingMap(BindingResolveMode.ScenePath);
bindingMap.AddBinding("Hero", "/Game/Characters/Hero");
bindingMap.AddBinding("NPC1", "/Game/Characters/Merchant");
```

支持四种解析模式：
- `ScenePath` — 优先通过注入的 `ICutsceneActorRegistry.TryResolveByRole()` O(1) 查找，未命中时回退到 `GameObject.Find`
- `Tag` — 优先通过注入的 `ICutsceneActorRegistry.TryResolveByTag()` O(1) 查找，未命中时回退到 `FindWithTag`
- `EntityId` — 优先通过注入的 `ICutsceneActorRegistry.TryResolveByEntity()` O(1) 查找，未命中时回退到 `FindObjectsByType`
- `ActorComponent` — 优先通过注入的 `ICutsceneActorRegistry.TryResolveByTag()` O(1) 查找，未命中时回退到 `FindObjectsByType`

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

## 绑定解析原理

### 注册表机制

注册表以**接口 + 实例 + 注入**组织（不再使用全局静态状态）：

- `ICutsceneActorRegistry` — 注册表接口（`Register`/`Unregister`/`TryResolveByRole`/`TryResolveByTag`/`TryResolveByEntity`/`Clear`）。
- `CutsceneActorRegistry` — 默认实现（实例，实现 `IDisposable`），提供 O(1) 字典查找。
- `CutsceneManager` 创建并持有注册表实例（`ActorRegistry` 属性），创建 `CutscenePlayer` 时经构造函数注入给 `CutsceneBindingResolver`。也可向 `CutsceneManager` 构造函数传入自定义 `ICutsceneActorRegistry` 以替换实现。

```
CutsceneActor.OnEnable()
  → 经 GameWorldDriver → CutsceneManager.ActorRegistry 解析出注册表实例
  → registry.Register(this)
       ├── _roleMap[ActorRole] = gameObject
       ├── _tagMap[ActorTag] = gameObject
       └── _entityMap[EntityId] = gameObject

CutsceneActor.OnDisable() → registry.Unregister(this)
                                 └── 仅当值匹配时才移除

场景卸载                →  SceneManager.sceneUnloaded 回调
                                 └── 批量清理该场景相关注册项
```

### 解析流程

```
CutsceneBindingResolver.GetReferenceValue(id)
  → ParsePropertyName("Hero:12345") → "Hero"
  → bindingMap.TryGetBinding("Hero") → targetId
  → ResolveBinding("Hero", targetId):
      ├── ScenePath:    Registry→role → Registry→targetId → GameObject.Find (fallback)
      ├── Tag:          Registry→tag  → FindWithTag (fallback)
      ├── EntityId:     Registry→entity → FindObjectsByType (fallback)
      └── ActorComponent: Registry→tag → FindObjectsByType (fallback)
```

### 性能对比

| 模式 | 优化前 | 优化后（Registry 命中） |
|------|--------|------------------------|
| ScenePath | `GameObject.Find` O(n) | `Dictionary.TryGetValue` **O(1)** |
| Tag | `FindWithTag` O(1) | `Dictionary.TryGetValue` **O(1)** |
| EntityId | `FindObjectsByType` O(n) + GC | `Dictionary.TryGetValue` **O(1)** |
| ActorComponent | `FindObjectsByType` O(n) + GC | `Dictionary.TryGetValue` **O(1)** |
