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
- **MemoryPackSerializer** — 二进制序列化包装器，封装 vendored MemoryPack API
- **LogLevel** — 日志等级枚举（Debug/Info/Warning/Error/Fatal）
- **ILogChannel** — 模块级日志通道接口（Name / Enabled）
- **LogEntry** — 结构化日志条目（Timestamp, Channel, Level, Message, Context）
- **IDebugHub** — 调试中枢接口（RegisterChannel / RegisterCommand / Log）
- **IDebugCommand** — 调试命令接口（Name / Description / Execute）
- **DebugHub** — 调试中枢实现类（通道/命令注册表 + 环形日志缓冲）
- **HNRandom** — 确定性伪随机数生成器，基于 xorshift128+ 算法，支持种子设置与状态序列化

**Capability — 能力模块（接口定义）**

- **IAssetManager** — 资源管理器接口
- **ILogProvider** — 日志提供者接口（支持 LogLevel 分级 + Channel 通道）
- **INetworkManager** — 网络管理器接口，定义 IsServer/IsClient/IsHost、StartServer/StartClient/StopConnection、连接事件
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
- **IHotUpdateEntry** — 热更新 DLL 入口接口，由热更程序集实现以注册模块到 GameWorld

**Level.Logic — 逻辑层**

- **MVC**: `Controller` / `ControllerUnit` / `ControllerManager` / `Model` / `ModelUnit` / `IReadOnlyModel<T>` / `ReadOnlyModel<T>`
- **HFSM**: `HFSM` / `HFSMState` / `HFSMTransition` / `HFSMCompoundState`
- **Entity** 🚧 — 实体系统（待实现）
- **Sheet** ✅ — 配置表运行时查询系统
  - `ISheetManager` — 配置表管理器，支持表注册和查询
  - `IConfigTable<TKey, TRow>` — 泛型配置表接口（Get/TryGet/GetAll/Count）
  - `ConfigTable<TKey, TRow>` — 默认实现，O(1) 字典查表
  - `AssetRef<T>` — MemoryPack 可序列化的资产引用，存 Addressables Label
  - `ConfigLoader` — MemoryPack 反序列化工具类

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
- **NetworkEntityView** — 网络实体视图基类，继承 FishNet.NetworkBehaviour
- **FishNetMessageBus** — 网络消息总线，封装 FishNet Broadcast 系统
- **FishNetConnectionAdapter** — 连接状态管理适配器
- **FishNetSerializerAdapter** — MemoryPack 注入 FishNet 的自定义序列化器
- **UnityFormatters** — Unity 类型格式化器集合，含 16 种内置类型
- **UnityFormattersInitializer** — Unity 格式化器初始化器，提供 `RegisterAll()` 批量注册
- **SheetManager** — ISheetManager 实现，支持二进制数据加载
- **ISheetRegistrar** — 由 Luban 生成的 Tables 实现，自动注册所有表
- **LubanTablesAdapter** — 泛型辅助类，LoadTables<TTables>
- **AssetRefExtensions** — AssetRef<T> 的 Addressables 加载扩展
- **RuntimeDebugConsole** — UGUI 运行时调试控制台（`~` 键切换，命令输入/自动补全/历史）
- **LuaModManager** — xLua Mod 脚本管理器，实现 IScriptEngine，提供沙箱隔离和 API 白名单机制
- **HybridCLRAdapter** — HybridCLR 运行时适配器，负责加载热更新 DLL 和注册 AOT 补充元数据
- **UIManager** — 7 层 Canvas + Addressables 异步加载 + Toast 队列管理 + 引导系统，实现 IUIManager + ITickable + IDisposable
- **UIPanel** — 面板基类（MonoBehaviour），生命周期 Closed→Opening→Opened→Closing 状态机，动画集成（OnEnterAnimation/OnExitAnimation）+ Pause/Resume 导航栈
- **UIAnimation** — LitMotion 封装静态工具类，提供 FadeIn/Out、SlideIn/Out、ScaleIn/Out 预设动画
- **UIDialog** — 模态弹窗基类，确认/取消回调 + 遮罩阻挡
- **UIToast** — 自动消失提示面板，FadeIn/Out 动画，排队机制
- **UIGuide** — 步骤驱动引导覆盖层，NextStep/PrevStep 导航
- **RedDotManager** — 红点树路径式注册管理器，Subscribe/Unsubscribe 监听

**Level.View — 视图层**

- **UI** — UI 运行时子目录，包含 UIManager/UIPanel/UIAnimation/UIDialog/UIToast/UIGuide/RedDotManager
- **ViewFactory** 🚧 — 视图工厂基类（待实现）
- **EntityView** 🚧 — 实体视图基类（待实现）
- **PropertyBinder** — 属性绑定抽象类，提供 Bind/UnbindAll 方法

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
