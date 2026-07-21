# HNUnityFramework 架构 — 02：GameWorld 驱动模型与命名空间规范

> 本文档源自 `架构~/最终架构.md`，为拆分后的第三部分。
> 涵盖：GameWorld 双组件驱动模型、Tick 循环、命名空间映射。
> 最后更新：2026-07-14

---

## 五、GameWorld 驱动模型

### 5.1 静态单例消除

所有有状态的 Manager 从静态单例迁移为 GameWorld 持有的实例：

| 类 | 旧模式 | 新模式 |
|----|--------|--------|
| `ControllerManager` | `static ControllerManager Instance` | `world.ControllerManager`（实例） |
| `ObjectPoolManager` | `static ObjectPoolManager Instance` | `world.PoolManager`（实例） |
| `ProcedureManager` | `static ProcedureManager Instance` | `world.ProcedureManager`（实例） |
| `ReferencePool` | `static` 全部静态 | **保持不变** — 线程安全工具类 |
| `HNLogicTime` | `static` 全部静态 | **保持不变** — 纯数据类 |

### 5.2 GameWorld 双组件模式

> **为空策略**：GameWorld 构造函数创建的模块（PoolManager, ProcedureManager, ControllerManager, DebugHub, EntityManager, AISystem, GameplayTagManager, EventBus）永远非空。
> 所有外部注入模块（除 LogProvider 由 GameWorldDriver 保证非空外）均声明为可空类型（`?`），项目可按需注入或置空，最大化框架的适配范围。
> 详见 [03-驱动层设计.md](./03-驱动层设计.md) D1 节。

```
                 HN.Framework.Core（纯 C#）            HN.Framework.Unity（Unity 层）
                 ────────────────────────            ──────────────────────────────

                 ┌──────────────────────┐           ┌──────────────────────────┐
                 │     GameWorld        │◄──────────│    GameWorldDriver        │
                 │                      │  持有并驱动 │   (MonoBehaviour)         │
                 │  内置模块:            │           │                          │
                 │  · PoolManager       │           │  Awake() → new GameWorld │
                 │  · ProcedureManager  │           │          → 注入平台实现   │
                 │  · ControllerManager │           │          → World.Initialize│
                 │  · DebugHub          │           │                          │
                 │  · EntityManager     │           │  Update() → World.Tick() │
                 │                      │           │  LateUpdate() → LateTick │
                 │  注入服务:            │←─────────│                          │
                 │  · LogProvider       │  注入      │  OnRegisterGameModules() │
                 │  · AssetManager      │           │    → 虚方法，Scripts 重写 │
                 │  · NetworkManager    │           └──────────────────────────┘
                 │  · StorageProvider   │            ▲
                 │  · LocaleProvider    │            │ 继承
                 │  · AudioManager      │  ┌─────────┴───────────┐
                 │  · CutsceneManager   │  │     GameEntry        │ ← Scripts 仓库
                 │  · UIManager         │  │  (Scripts Repo)      │
                 └──────────────────────┘  │                      │
                                           │  OnRegisterGameModules│
                                           │    → 注册游戏特定模块 │
                                           └──────────────────────┘
```

### 5.3 Tick 循环顺序

```
Tick():
  (FrameSyncManager as ITickable)?.Tick()  → 帧同步（最优先，C6）
  PoolManager.Tick()                        → 池回收调度（C10）
  ProcedureManager.Tick()                   → 流程状态 Tick（C9）
  ControllerManager.Tick()                  → 所有 Controller + ControllerUnit（L1）
  AISystem.Tick()                           → 所有 AIAgent 决策 Tick（L10）
  AssetManager?.Tick()                      → 资源自动卸载 + 预加载消费（C4）
  (InputManager as ITickable)?.Tick()       → 输入（C14，事件驱动，ITickable 可选）
  (UIManager as ITickable)?.Tick()          → UI（C13）
  (PhysicsWorld as ITickable)?.Tick()       → 物理（C16）
  (CutsceneManager as ITickable)?.Tick()    → 过场动画（C12）
  QuestManager?.Tick()                      → 任务/成就超时检查（L8，项目注入）

LateTick():
  (FrameSyncManager as ITickable)?.LateTick()
  PoolManager.LateTick()
  ProcedureManager.LateTick()
  ControllerManager.LateTick()
  AISystem.LateTick()
  AssetManager?.LateTick()
  (InputManager as ITickable)?.LateTick()
  (UIManager as ITickable)?.LateTick()
  (PhysicsWorld as ITickable)?.LateTick()
  (CutsceneManager as ITickable)?.LateTick()
  QuestManager?.LateTick()
```

### 5.4 Scripts 仓库 GameEntry 示例

```csharp
// 继承 Framework 的 GameWorldDriver，重写 OnRegisterGameModules
public class GameEntry : GameWorldDriver
{
    protected override void OnRegisterGameModules(GameWorld world)
    {
        // 注册游戏特定的 Controller
        world.ControllerManager.RegisterController(new BattleController(world));

        // 注入游戏特定的服务实现
        world.NetworkManager = new GameNetworkService();
        world.StorageProvider = new GameStorageService();
    }
}
```

---

## 六、命名空间规范

### 6.1 完整命名空间映射

| 编号 | 核心内容 | 命名空间 | 程序集 |
|------|------|----------|:------:|
| D1 | GameWorld | `HN.Framework.Core.Driver` | Core |
| D2 | ITickable, IReference, HNLogicTime, AsyncLoadHandle | `HN.Framework.Core.Driver.Common` | Core |
| D2 | ReferencePool + 17 PooledCollections | `HN.Framework.Core.Driver.Common.Pool.ReferencePool` | Core |
| D2 | ObjectPool<T>, PoolBase, PooledObjectBase | `HN.Framework.Core.Driver.Common.Pool.ObjectPool` | Core |
| D2 | Json, JsonObject, MemoryPack Core | `HN.Framework.Core.Driver.Common.Serialization` | Core |
| D2 | HNRandom | `HN.Framework.Core.Driver.Common.Math` | Core |
| D2 | FixedMathSharp（第三方定点数库，Core/Vendor） | `HN.Framework.Core.Vendor.FixedMathSharp`（源码） + `HN.Framework.Core.Capability.Serialization`（自定义格式化器） | Core |
| D3 | LogLevel, ILogChannel, LogEntry, IDebugHub, IDebugCommand, DebugHub | `HN.Framework.Core.Driver.Common.Debug` | Core |
| D4 | GameWorldDriver | `HN.Framework.Unity.Driver.Platform` | Unity |
| D4 | AssetManager, AssetCacheItem, IAssetOperator | `HN.Framework.Unity.Capability.Asset` | Unity |
| D4 | AddressablesOperator, ResourcesOperator, AssetDatabaseOperator | `HN.Framework.Unity.Driver.Platform.Asset` | Unity |
| D4 | GameObjectPool, PooledObject<T> | `HN.Framework.Unity.Driver.Platform.ObjectPool` | Unity |
| D4 | HNDictionary, SerializableDictionary | `HN.Framework.Unity.Driver.Platform.DataStructures` | Unity |
| D4 | UnityLogProvider | `HN.Framework.Unity.Driver.Platform.Log` | Unity |
| D4 | UnityTimeProvider | `HN.Framework.Unity.Driver.Platform.Time` | Unity |
| D4 | UnityCoroutineProvider | `HN.Framework.Unity.Driver.Platform.Coroutine` | Unity |
| D4 | HNRenderPipeline, HNRenderPipelineAsset, ShaderLibrary | `HN.Framework.Unity.Driver.Platform.Rendering` | Unity |
| D4 | GlobalSettings | `HN.Framework.Unity.Driver.Platform.Settings` | Unity |
| C1 | ISerializer, MemoryPackFormatter (Core) | `HN.Framework.Core.Capability.Serialization` | Core |
| C1 | UnityFormatters, FishNetSerializerAdapter | `HN.Framework.Unity.Capability.Serialization` | Unity |
| C1 | FixedMathSharpFormatters | `HN.Framework.Core.Capability.Serialization` | Core |
| C2 | DebugHub, DebugModule, DebugCommandRegistry | `HN.Framework.Core.Capability.Debug` | Core |
| C2 | RuntimeDebugConsole | `HN.Framework.Unity.Capability.Debug` | Unity |
| C3 | Locale, StringTable, ILocaleProvider | `HN.Framework.Core.Capability.Localization` | Core |
| C4 | IAssetManager | `HN.Framework.Core.Capability` | Core |
| C5 | ILogProvider | `HN.Framework.Core.Capability` | Core |
| C6 | INetworkManager, IFrameSyncManager, LockstepManager, FrameInput, SyncedModel 等 | `HN.Framework.Core.Capability.Network` | Core |
| C6 | FishNetNetworkManager, FishNetMessageBus, FishNetConnectionAdapter, FishNetSerializerAdapter, LockstepNetworkDriver, NetworkEntityView, PredictedNetworkEntityView, PredictionManagerAdapter, LagCompensationAdapter | `HN.Framework.Unity.Capability.Network` | Unity |
| C7 | IEventBus + EventBus | `HN.Framework.Core.Capability.Event` | Core |
| C8 | IStorageProvider | `HN.Framework.Core.Capability` | Core |
| C9 | ProcedureManager, ProcedureState | `HN.Framework.Core.Capability` | Core |
| C10 | ObjectPoolManager | `HN.Framework.Core.Capability` | Core |
| C11 | AudioEvent, AudioBus, AudioBank, IAudioManager 等 | `HN.Framework.Core.Capability.Audio` | Core |
| C11 | AudioManager, AudioEmitter, miniaudio bridge | `HN.Framework.Unity.Capability.Audio` | Unity |
| C12 | CutsceneData, CutsceneRole, ICutsceneManager | `HN.Framework.Core.Capability.Cutscene` | Core |
| C12 | CutsceneManager, CutsceneActor, CutsceneBindingResolver + Tracks | `HN.Framework.Unity.Capability.Cutscene` | Unity |
| C13 | UILayer, UIPanelState, IUIManager, RedDotNode, GuideStep, DialogResult, ToastConfig | `HN.Framework.Core.Capability.UI` | Core |
| C13 | UIManager, UIPanel, UIDialog, UIToast, UIGuide, UIAnimation, RedDotManager | `HN.Framework.Unity.Level.View.UI` | Unity |
| C14 | IInputManager, InputAction, InputContext, IInputBlocker, InputTypes | `HN.Framework.Core.Capability.Input` | Core |
| C14 | InputManager, InputBlocker, DeviceDetector, TouchInputAdapter, InputActionAssetLoader | `HN.Framework.Unity.Capability.Input` | Unity |
| C15 | IScriptEngine, IScriptMod, IHotUpdateEntry, ModState, ScriptModConfig | `HN.Framework.Core.Capability.Scripting` | Core |
| C15 | HybridCLRAdapter, LuaModManager | `HN.Framework.Unity.Capability.Scripting` | Unity |
| C16 | IPhysicsWorld, IBody, Shape 定义, RaycastHit, CollisionEvent, PhysicsVector3, PhysicsTypes | `HN.Framework.Core.Capability.Physics` | Core |
| C16 | PhysXWorld, UnityBody | `HN.Framework.Unity.Capability.Physics` | Unity |
| C17 | ICameraManager, CameraPreset, CameraShakeProfile | `HN.Framework.Core.Capability.Camera` | Core |
| C17 | CameraManager, CameraHandle, CameraShake | `HN.Framework.Unity.Capability.Camera` | Unity |
| C18 | IEncryptionProvider, IChecksumValidator, ISecureStorage | `HN.Framework.Core.Capability.Security` | Core |
| C18 | AesEncryptionProvider, MemoryGuard, SecurePlayerPrefs | `HN.Framework.Unity.Capability.Security` | Unity |
| C19 | IWorldStreamingManager, WorldChunk, StreamingStrategy | `HN.Framework.Core.Capability.SceneStreaming` | Core |
| C19 | WorldStreamingManager, ChunkLoader, ChunkGrid | `HN.Framework.Unity.Capability.SceneStreaming` | Unity |
| C20 | IGameTimeManager, WeatherPreset | `HN.Framework.Core.Capability.TimeWeather` | Core |
| C20 | GameTimeManager, WeatherManager, TimeWeatherDriver | `HN.Framework.Unity.Capability.TimeWeather` | Unity |
| C21 | IPerformanceMonitor, ITelemetryReporter, TelemetryEvent | `HN.Framework.Core.Capability.Performance` | Core |
| C21 | FrameTimeMonitor, MemoryMonitor, RuntimeOverlay | `HN.Framework.Unity.Capability.Performance` | Unity |
| C21 | PerformanceProfilerWindow | `HN.Framework.Editor.Performance` | Editor |
| L1 | Controller, ControllerUnit, ControllerManager, Model, ModelUnit, IReadOnlyModel<T> | `HN.Framework.Core.Level.Logic` | Core |
| L2 | HFSM, HFSMState, HFSMTransition, HFSMCompoundState | `HN.Framework.Core.Level.Logic` | Core |
| L3 | Entity, EntityManager, EntityEvents | `HN.Framework.Core.Level.Logic.Entity` | Core |
| L4 | ISheetManager, IConfigTable, ConfigTable, ConfigLoader, AssetRef\<T\> | `HN.Framework.Core.Level.Logic.Sheet` | Core |
| L4 | SheetManager, ISheetRegistrar, AssetRefExtensions | `HN.Framework.Unity.Capability.Sheet` | Unity |
| L0 | GameplayTag, GameplayTagContainer, TagQuery | `HN.Framework.Core.Level` | Core |
| L5 | GraphData, GraphNode, GraphEdge, GraphPort, GraphGroup | `HN.Framework.Core.Level.Logic.Graph` | Core |
| L6 | AnimationGraph, AnimState, BlendSpace1D/2D, SkeletonMask, IKConfig 等 | `HN.Framework.Core.Level.Logic.Animation` | Core |
| L7 | AttributeDefinition, AbilityDefinition, EffectDefinition, Modifier, IAttributeSet, IAbilitySystem, IEffectPipeline, DamagePipeline, BuffSystem, EffectSpec | `HN.Framework.Core.Level.Logic.Combat` | Core |
| L8 | IQuestManager, QuestCondition (Single/And/Or/Not), QuestLine | `HN.Framework.Core.Level.Logic.Quest` | Core |
| L9 | IInventoryService, IEquipmentService, ItemStack, Slot, InvOpResult, ItemDefinition | `HN.Framework.Core.Level.Logic.Inventory` | Core |
| L10 | AIAgent, DecisionPipeline, KnowledgePool, Blackboard, IDecisionStrategy, StrategyRegistry, IActionCommand, BehaviorTreeStrategy, FSMStrategy, UtilityStrategy, GOAPStrategy, HTNStrategy, DecisionTreeStrategy, FuzzyLogicStrategy, ScriptedStrategy | `HN.Framework.Core.Level.Logic.AI` | Core |
| L10 | AISystem（GameWorld 内置 AI Agent 管理器） | `HN.Framework.Core.Level.Logic.AI` | Core |
| L10 | AIAgentComponent, VisionSensor, AudioSensor, NavMeshAgentAdapter, DefaultActionExecutor | `HN.Framework.Unity.Level.Logic.AI` | Unity |
| L10 | AIStrategyEditorWindow, BehaviorTreeEditor, DecisionTreeEditor, UtilityCurveEditor | `HN.Framework.Editor.Graph` | Editor |
| V1 | UIManager, UIPanel, UIDialog, UIToast, UIGuide, UIAnimation, RedDotManager | `HN.Framework.Unity.Level.View.UI` | Unity |
| V2 | ViewFactory, EntityView | `HN.Framework.Unity.Level.View` | Unity |
| V3 | PropertyBinder, DefaultPropertyBinder | `HN.Framework.Unity.Level.View.Binding` | Unity |
| V4 | AnimationPlayableRuntime | `HN.Framework.Unity.Level.View.Animation` | Unity |
| V5 | AudioEmitter, AudioListener | `HN.Framework.Unity.Level.View.Audio` | Unity |
| E1 | ShowInInspector, ReadOnly, Button, FoldoutGroup 等 + CustomEditorRenderer | `HN.Framework.Editor.EditorUI` | Editor |
| E2 | GraphEditorWindow, AnimationGraphEditor, BehaviorTreeEditor, DecisionTreeEditor, UtilityCurveEditor | `HN.Framework.Editor.Graph` | Editor |
| E3 | DebugHubWindow, LogViewer, RuntimeStateInspector | `HN.Framework.Editor.Debug` | Editor |
| E4 | LocalizationEditorWindow, AssetLocaleTagger | `HN.Framework.Editor.Localization` | Editor |
| E5 | AudioBankEditor, AudioEventEditor, AudioProfiler | `HN.Framework.Editor.Audio` | Editor |
| E6 | CutsceneEditorWindow | `HN.Framework.Editor.Cutscene` | Editor |
| E7 | SheetEditorWindow, TableModel, SchemaProvider, ExcelSourceParser, ExcelSerializer, SheetGrid, AssetRefCell | `HN.Framework.Editor.Sheet` | Editor |
| E8 | ObjectPoolViewerEditor, AddressablesAssetsGroupPresets, AddressablesGroupsUpdater, HNDictionaryDrawer, HNUndoableObject | `HN.Framework.Editor.*` | Editor |
| E9 | AssetValidator, RedundancyCleaner, BuildPipelineOrchestrator, VersionManager | `HN.Framework.Editor.BuildPipeline` | Editor |

> **核心原则**：`HN.Framework.Unity` 程序集的命名空间以 `HN.Framework.Unity` 开头，其余以 `HN.Framework.Core` 开头。Editor 以 `HN.Framework.Editor` 开头。

### 6.2 已知命名空间不一致（待修正）

| 文件 | 当前 Namespace | 应修正为 |
|------|---------------|----------|
| `Editor/Sheet/Sheet.cs` | `HN.Framework` | `HN.Framework.Editor` |

---

> 继续阅读：[03-驱动层设计.md](./03-驱动层设计.md)
