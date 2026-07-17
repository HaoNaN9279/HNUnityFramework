---
sidebar_position: 1
---

# API 文档

HNUnityFramework 的完整 API 参考文档由 [DocFX](https://dotnet.github.io/docfx/) 从源代码的 XML 文档注释自动生成。

## 模块索引

### Runtime

#### HN.Framework.Core

纯 C# 逻辑层，无 Unity 依赖。

**Driver — 驱动层**

- **GameWorld** — 框架核心驱动器，管理生命周期与模块注册
- **HNLogicTime** — 逻辑时间系统，提供独立于渲染帧的计时
- **ITickable** — 可更新接口，所有需要逐帧更新的对象实现此接口
- **IReference** — 引用池对象接口
- **ReferencePool** — 引用池，零 GC 分配的对象复用
- **PoolBase** — 对象池基类
- **ObjectPool\<T\>** — 泛型对象池
- **PooledCollections** — `PooledList<T>`、`PooledDictionary<K,V>` 等 16 种池化集合
- **Json** — JSON 序列化/反序列化工具
- **JsonObject** — 动态 JSON 对象
- **MemoryPackSerializer** — 二进制序列化包装器，封装 MemoryPack API（DLL 引用 v1.21.4，NuGet）
- **LogLevel** — 日志等级枚举（Debug/Info/Warning/Error/Fatal）
- **ILogChannel** — 模块级日志通道接口（Name / Enabled）
- **LogEntry** — 结构化日志条目（Timestamp, Channel, Level, Message, Context）
- **IDebugHub** — 调试中枢接口（RegisterChannel / RegisterCommand / Log）
- **IDebugCommand** — 调试命令接口（Name / Description / Execute）
- **DebugHub** — 调试中枢实现类（通道/命令注册表 + 环形日志缓冲）
- **HNRandom** — 确定性伪随机数生成器，基于 xorshift128+ 算法，支持种子设置与状态序列化
- **HNFixedPoint** — 定点数类型别名（映射到 FixedMathSharp.Fixed64），提供与自研方案兼容的 API（FixedMathSharp 为 DLL 引用，自编译 .NET Standard 2.1）
- **FixedMathSharpFormatters** — FixedMathSharp 定点数类型自定义 MemoryPack 格式化器（含 Fixed64、Vector2d/3d/4d、FixedQuaternion、Fixed4x4、FixedBoundBox、FixedBoundSphere 共 8 个格式化器）
- **FormattersInitializer** — Core 层格式化器初始化器，提供 RegisterAll() 批量注册 FixedMathSharp 格式化器

**Capability — 能力模块（接口定义）**

- **IAssetManager** — 资源管理器接口
- **ILogProvider** — 日志提供者接口（支持 LogLevel 分级 + Channel 通道）
- **INetworkManager** — 网络管理器接口，定义 IsServer/IsClient/IsHost、StartServer/StartClient/StopConnection、连接事件
- **LocalClientId** — INetworkManager 接口新增属性，获取本地客户端连接 ID
- **ConnectedClientIds** — INetworkManager 接口新增属性，获取已连接客户端 ID 列表
- **SyncedModel\<T\>** — 同步数据模型，实现 IReadOnlyModel\<T\>，支持服务端写入、脏标记追踪、线程安全的值变更通知
- **SyncCollectionOperation** — 同步集合操作类型枚举（Add/Remove/Insert/Set/Clear）
- **SyncCollectionChange\<T\>** — 同步列表变更事件参数（Operation/Index/Item/OldItem）
- **SyncDictChange\<TKey, TValue\>** — 同步字典变更事件参数（Operation/Key/Value）
- **PredictionInputBase** — 预测输入基类（MemoryPackable + IReference）
- **PredictionReconcileData\<T\>** — 调和数据泛型结构体
- **IPredictedEntity** — 预测实体接口（Simulate/Reconcile/GetLastProcessedTick）
- **MessageBase** — 消息协议抽象基类，继承 IReference 支持 ReferencePool 池复用
- **ConnectionMessages** — 连接消息类型（ClientConnected / ClientDisconnected / ServerReady）
- **ISerializer** — 统一序列化接口，支持泛型和非泛型
- **MemoryPackFormatterProvider** — 格式化器注册适配器，框架友好封装
- **IStorageProvider** — 存储提供者接口
- **ProcedureManager** — 流程管理器，驱动游戏状态切换
- **ProcedureState** — 流程状态基类
- **ObjectPoolManager** — 对象池管理器，统一管理所有对象池
- **IEventBus** — 事件总线接口，提供 `Subscribe<T>` / `Unsubscribe<T>` / `Publish<T>` / `HasSubscribers<T>` 方法
- **EventBus** — 事件总线实现，线程安全，基于锁+快照模式
- **DebugHub** — Capability 层调试中枢，支持模块注册（RegisterModule）、命令执行（ExecuteCommand）、前缀搜索（SearchCommands）、ILogProvider 桥接（SetLogProvider）
- **IUIManager** — UI 管理器接口，定义 Push/Pop/Show/Hide/ShowDialog/ShowToast/StartGuide/StopGuide/PushAsync/ShowAsync 面板栈操作
- **UILayer** — UI 7 层层级枚举（Background/Scene/UI/Popup/Toast/Guide/System）
- **UIPanelState** — 面板状态枚举（Closed/Opening/Opened/Closing）
- **RedDotNode** — 红点树节点，支持父子聚合计数与变更事件
- **DialogResult** — 对话框结果枚举（None/Confirm/Cancel）
- **ToastConfig** — Toast 配置结构体（Duration/Layer）
- **GuideStep** — 引导步骤数据模型（7 个字段：StepId/TargetName/Description/HighlightOffsetX/Y/HighlightSizeWidth/Height）
- **DebugModule** — 调试模块，将关联的日志通道和调试命令打包为一个逻辑模块
- **DebugCommandRegistry** — 命令注册表查询层，支持精确查找和前缀搜索（Tab 自动补全）
- **IScriptEngine** — 脚本引擎抽象接口，定义脚本执行、全局注册和函数调用的基本契约
- **IScriptMod** — Mod 生命周期接口（OnLoad/OnEnable/OnDisable/OnUnload）
- **ModState** — Mod 状态枚举（NotLoaded/Loaded/Enabled/Disabled/Error）
- **ScriptModConfig** — Mod 配置数据模型（ModId/ModName/Version/ScriptPaths/Sandboxed）
- **ICameraManager** — 摄像机管理器接口，定义摄像机预设注册/激活/混合/振动等操作（基于 Cinemachine）
- **CameraPreset** — 摄像机预设数据模型（FOV/近远裁面/优先级/混合时间），实现 `IReference`
- **CameraShakeProfile** — 摄像机振动配置数据模型（振幅/频率/时长/衰减类型），实现 `IReference`
- **IHotUpdateEntry** — 热更新 DLL 入口接口，由热更程序集实现以注册模块到 GameWorld
- **C12 — 过场动画系统**：基于 Timeline 的过场动画框架，含自定义 Track（字幕/对话/事件/震动/游戏状态）、角色绑定解析器、优先级仲裁、输入阻塞/摄像机接管自动化。

**Level.Logic — 逻辑层**

- **MVC**: `Controller` / `ControllerUnit` / `ControllerManager` / `Model` / `ModelUnit` / `IReadOnlyModel<T>` / `ReadOnlyModel<T>`
- **HFSM**: `HFSM` / `HFSMState` / `HFSMTransition` / `HFSMCompoundState`
- **Entity** ✅ — 实体系统
  - `Entity` — 纯数据实体基类，MemoryPack 可序列化，实现 IReference
  - `EntityManager` — 实体生命周期管理器（Spawn/Despawn/GetEntity）
  - **EntityManager（扩展）** — 新增 SpawnWithOwner/HasAuthority/TransferOwnership/RemoveOwnership/GetOwnedEntities 方法
  - `EntitySpawnedEvent` — 实体生成事件
  - `EntityDespawnedEvent` — 实体销毁事件
- **EntityOwnershipTransferredEvent** — 实体所有权转移事件
- **Sheet** ✅ — 配置表运行时查询系统
  - `ISheetManager` — 配置表管理器，支持表注册和查询
  - `IConfigTable<TKey, TRow>` — 泛型配置表接口（Get/TryGet/GetAll/Count）
  - `ConfigTable<TKey, TRow>` — 默认实现，O(1) 字典查表
  - `AssetRef<T>` — MemoryPack 可序列化的资产引用，存 Addressables Label
  - `ConfigLoader` — MemoryPack 反序列化工具类
- **Combat（战斗数值体系）** ✅ — 基于 GAS 模式的六模块数值体系：
  - `AttributeType` / `AttributeTypeManager` / `AttributeSet<TId>` — 属性类型标识与管理、泛型属性集合、Modifier 修正系统（Add/Multiply/Override/MinCap/MaxCap）
  - `EffectSpec<TId>` / `EffectPipeline<TId>` — 效果规格定义与效果管线（Instant/Duration/Infinite）
  - `DamagePipeline<TId>` / `IDamageFormula<TId>` — 四阶段伤害管线（PreMigration→Calculate→PostMigration→ApplyDamage），可替换公式
  - `BuffSpec<TId>` / `BuffSystem<TId>` — Buff 规格与系统（Single/Multi/Refresh/Extend 四种堆叠规则）
  - `AbilitySpec<TId>` / `AbilitySystem<TId>` — 技能规格与系统（Cooldown/Cost/Tag 条件预检）
  - `PropagationSpec<TId>` / `IPropagationStrategy<TId>` / `IPropagationFilter<TId>` / `PropagationSystem<TId>` — 通用效果传播机制（三要素分离：Strategy+Filter+Effect，骨架执行器）
  - `IBuffBehaviour<TId>` / `BuffBehaviourContext<TId>` — Buff 自定义行为扩展接口（OnApply/OnTick/OnRemove/OnStackChanged 四生命周期），通过 `BuffSpec.BehaviourId` + 工厂委托集成到 BuffSystem
- **GameplayTag** ✅ — 层级标签系统
  - `GameplayTag` — 8 字节值类型（TableIndex + InstanceId），层级编码，零 GC
  - `GameplayTagManager` — 定义表管理，加载/冻结/查询
  - `GameplayTagContainer` — 标签容器，增删查 + 批量层级匹配
  - `TagQuery` — 嵌套布尔查询表达式（Any/All/Not），用于配置表条件匹配
- **Inventory（物品/背包/装备）** ✅ — 物品背包装备框架：
  - `ItemCategory` — 物品类别枚举（Weapon/Armor/Accessory/Consumable/Material/Quest/Key）
  - `EquipmentSlot` — 装备槽位枚举（[Flags]，Weapon/Head/Chest/Legs/Feet/Accessory1/Accessory2）
  - `ItemDef` — 物品配置表行定义（MemoryPackable struct，含 Id/Category/MaxStack/AllowedSlots/Durability/Buffs 等 14 字段）
  - `ItemBuffSpec` — 物品附加 Buff 规格定义，提供 `BuildBuffSpec<TId>()` 桥接到 L7 BuffSystem
  - `ItemInstance` — 运行时物品实例（MemoryPackable + IReference），含堆叠/耐久/OwnerEntityId/WorldEntityId/ExtraData
  - `ContainerType` — 容器类型枚举（Backpack/Warehouse/Equipment/Hotbar/Shop/Material）
  - `ContainerSlot` — 容器槽位数据（ItemInstance 引用 + StackCount）
  - `IContainer` / `Container` — 容器接口与实现（定长数组+自动堆叠+事件通知）
  - `IEquipment` / `Equipment` — 装备系统接口与实现（位掩码槽位校验+L7 Buff 生命周期管理）
  - `ItemFormatters` — ItemInstance 的 MemoryPack 自定义格式化器

#### HN.Framework.Unity

Unity 平台层，依赖 UnityEngine。

**Driver.Platform — 平台抽象层**

- **GameWorldDriver** — Unity 平台驱动器，桥接 GameWorld 与 Unity 生命周期
- **AddressablesOperator** — Addressables 资源加载器
- **ResourcesOperator** — Resources 资源加载器
- **AssetDatabaseOperator** — 编辑器 AssetDatabase 资源加载器
- **AssetManager** — 统一资源管理器，透明切换三种加载方式
- **AssetCacheItem** — 资源缓存项
- **AsyncLoadHandle** — 异步加载句柄
- **GameObjectPool** — GameObject 对象池
- **GameObjectPoolBase** — GameObject 对象池基类
- **PooledObject\<T\>** — 池化对象包装器（MonoBehaviour 适配）
- **UnityLogProvider** — Unity Debug 日志实现
- **UnityTimeProvider** — Unity Time 时间实现
- **UnityCoroutineProvider** — Unity 协程提供者
- **HNDictionary\<TKey, TValue\>** — 可序列化字典
- **SerializableDictionary\<K, V\>** — Unity 序列化字典
- **JsonData** — 可序列化 JSON 数据容器，实现 ISerializationCallbackReceiver
- **HNUnityFrameworkGlobalSettings** — 框架全局资源配置
- **HNRenderPipeline** 🚧 — 自定义渲染管线（待实现）
- **HNRenderPipelineAsset** 🚧 — 渲染管线资源（待实现）

**Capability — 能力模块（Unity 实现）**

- **FishNetNetworkManager** — FishNet 网络管理器实现，纯 C# 包装 FishNet.NetworkManager
- **NetworkEntityView** — 网络实体视图基类，继承 FishNet.NetworkBehaviour，提供 RegisterSyncedModel/UnregisterSyncedModel/ApplySyncValue 同步基础设施
- **SyncedEntityId** — NetworkEntityView 新增属性，同步的 Core 层 EntityId
- **IsOwnedByMe** — NetworkEntityView 新增属性，判断当前客户端是否拥有该实体
- **IsOwnedByServer** — NetworkEntityView 新增属性，判断服务器是否拥有该实体
- **FishNetSyncedList\<T\>** — 同步列表包装器，继承 FishNet SyncList\<T\>，提供框架统一的 OnCollectionChanged 事件
- **FishNetSyncedDictionary\<TKey, TValue\>** — 同步字典包装器，继承 FishNet SyncDictionary，提供框架统一的 OnCollectionChanged 事件
- **FishNetMessageBus** — 网络消息总线，封装 FishNet Broadcast 系统
- **FishNetConnectionAdapter** — 连接状态管理适配器
- **FishNetSerializerAdapter** — MemoryPack 注入 FishNet 的自定义序列化器
- **PredictedNetworkEntityView** — 支持客户端预测的网络实体视图基类
- **PredictionManagerAdapter** — FishNet PredictionManager 封装
- **LagCompensationAdapter** — FishNet ColliderRollback 封装
- **UnityFormatters** — Unity 类型格式化器集合，含 16 种内置类型
- **UnityFormattersInitializer** — Unity 格式化器初始化器，提供 `RegisterAll()` 批量注册
- **SheetManager** — ISheetManager 实现，支持二进制数据加载
- **ISheetRegistrar** — 由 Luban 生成的 Tables 实现，自动注册所有表
- **LubanTablesAdapter** — 泛型辅助类，LoadTables<TTables>
- **AssetRefExtensions** — AssetRef<T> 的 Addressables 加载扩展
- **RuntimeDebugConsole** — UGUI 运行时调试控制台（`~` 键切换，命令输入/自动补全/历史）
- **LuaModManager** — xLua Mod 脚本管理器，实现 IScriptEngine，提供沙箱隔离和 API 白名单机制（源码集成，C# 9.0）
- **HybridCLRAdapter** — HybridCLR 运行时适配器，负责加载热更新 DLL 和注册 AOT 补充元数据（源码集成，C# 9.0）
- **UIManager** — 7 层 Canvas + Addressables 异步加载 + Toast 队列管理 + 引导系统，实现 IUIManager + ITickable + IDisposable
- **UIPanel** — 面板基类（MonoBehaviour），生命周期 Closed→Opening→Opened→Closing 状态机，动画集成（OnEnterAnimation/OnExitAnimation）+ Pause/Resume 导航栈
- **UIAnimation** — LitMotion 封装静态工具类，提供 FadeIn/Out、SlideIn/Out、ScaleIn/Out 预设动画
- **UIDialog** — 模态弹窗基类，确认/取消回调 + 遮罩阻挡
- **UIToast** — 自动消失提示面板，FadeIn/Out 动画，排队机制
- **UIGuide** — 步骤驱动引导覆盖层，NextStep/PrevStep 导航
- **RedDotManager** — 红点树路径式注册管理器，Subscribe/Unsubscribe 监听
- **CameraManager** — 摄像机管理器，Cinemachine Brain 包装器，实现 ICameraManager + IDisposable
- **CameraHandle** — 运行时 VCam 引用管理，包装 CinemachineVirtualCameraBase，提供 Follow/LookAt 绑定和预设切换
- **CameraShake** — 摄像机振动控制器，通过 CinemachineBasicMultiChannelPerlin 实现 Perlin 噪声振动
- **LocaleManager** — 本地化管理器，实现 ILocaleProvider，管理多语言 StringTable、语言切换和 OnLocaleChanged 事件分发
- **ILocaleDataLoader** — 本地化数据加载接口，支持可注入的数据源（Addressables / Resources / 自定义）
- **AddressableStringTableLoader** — ILocaleDataLoader 的 Addressables 实现，加载 JSON 格式字符串表
- **LocaleSelector** — 语言选择器，管理可用语言列表和 PlayerPrefs 持久化偏好
- **TextLocalizer** — MonoBehaviour 组件，挂载到 TMP_Text 上自动响应语言切换更新文本
- **AssetLocalizer** — 静态工具类，按语言拼接路径加载本地化资源
- **NetworkEntityLifecycleBridge** — 网络实体生命周期桥接组件，将 EntityManager 事件桥接到 FishNet

**Level.View — 视图层**

- **UI** — UI 运行时子目录，包含 UIManager/UIPanel/UIAnimation/UIDialog/UIToast/UIGuide/RedDotManager
- **ViewFactory** 🚧 — 视图工厂基类（待实现）
- **EntityView** 🚧 — 实体视图基类（待实现）
- **PropertyBinder** — 属性绑定抽象类，提供 Bind/UnbindAll 方法
- **DefaultPropertyBinder** — PropertyBinder 默认实现，基于 Dictionary 管理绑定关系
- **ViewFactory** — 视图工厂，支持同步 CreateView/ReleaseView 和预制体缓存
- **EntityView** — 实体视图基类（MonoBehaviour），提供 OnSpawned/OnDespawned 生命周期
- **Inventory** — 物品背包装备 Unity 桥接层
  - `ContainerView` — MonoBehaviour，挂载到 Entity 的容器数据桥接组件
  - `EquipmentView` — MonoBehaviour，挂载到 Entity 的装备数据桥接组件

### Editor

#### HN.Framework.Editor

编辑器扩展与工具。

**Core — 编辑器核心**

- **FrameworkDeployer** — 框架部署工具
- **HNUnityFrameworkConstants** — 框架常量定义
- **HNUnityFrameworkGlobalSettingsProvider** — 全局设置提供者
- **HNUnityFrameworkEditorMenus** — 编辑器菜单扩展

**Sheet — 配置表工具**

- **SheetEditorWindow** — 统一配置表编辑器窗口（HNFramework/Sheet Editor 菜单）
- **TableModel/ColumnDef/RowData/CellData** — 统一数据模型
- **SchemaProvider** — Excel schema 解析器（字段名+类型标注→列定义）
- **ExcelSourceParser** — Excel (.xlsx) 文件解析器
- **ExcelSerializer** — 编辑后写回 .xlsx
- **SheetGrid** — UI Toolkit 网格视图（冻结表头/列宽拖拽/类型分发）
- **AssetRefCell** — 资产引用单元格（64×64 缩略图/拖拽替换/右键菜单）

**ObjectPool — 对象池调试**

- **ObjectPoolViewerEditor** 🚧 — 对象池可视化调试器（待实现）

**AddressablesExtensions — Addressables 扩展**

- **AddressablesGroupsUpdater** — Addressables 分组更新器
- **AddressablesAssetsGroupPresets** — 分组预设配置
- **AddressablesAssetsGroupPresetsEditor** — 分组预设编辑器

**Scripting — 热更新工具**

- **HybridCLRBuildProcessor** — HybridCLR 构建管线处理器，在 Unity 构建过程中自动处理 AOT 元数据生成和原生库拷贝
- **HybridCLRMetadataGenerator** — AOT 元数据生成器，封装 HybridCLR 的标准生成流程
- **HybridCLRBuildSettings** — HybridCLR 构建配置 ScriptableObject（AOT 元数据 / 热更程序集 / 构建选项）
- **HybridCLRNativeLibManager** — HybridCLR 原生库管理器，验证安装状态和拷贝原生库到构建输出

**Utils — 编辑器工具**

- **HNUndoableObject** — 支持 Undo/Redo 的对象基类
- **HNDictionaryDrawer** — 字典属性绘图器

> 💡 **提示**：API 参考文档由 DocFX 从代码 XML 注释自动更新。如需修改 API 描述，请直接修改源代码中的 `<summary>` 注释，然后运行 `npm run docs:api` 重新生成。
