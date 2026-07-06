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

**Capability — 能力模块（接口定义）**

- **IAssetManager** — 资源管理器接口
- **ILogProvider** — 日志提供者接口（支持 LogLevel 分级 + Channel 通道）
- **INetworkManager** — 网络管理器接口
- **ISerializer** — 统一序列化接口，支持泛型和非泛型
- **MemoryPackFormatterProvider** — 格式化器注册适配器，框架友好封装
- **IStorageProvider** — 存储提供者接口
- **ProcedureManager** — 流程管理器，驱动游戏状态切换
- **ProcedureState** — 流程状态基类
- **ObjectPoolManager** — 对象池管理器，统一管理所有对象池

**Level.Logic — 逻辑层**

- **MVC**: `Controller` / `ControllerUnit` / `ControllerManager` / `Model` / `ModelUnit` / `IReadOnlyModel<T>` / `ReadOnlyModel<T>`
- **HFSM**: `HFSM` / `HFSMState` / `HFSMTransition` / `HFSMCompoundState`
- **Entity** 🚧 — 实体系统（待实现）
- **Sheet** 🚧 — 配置表运行时查询（待实现）

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

- **FishNetNetworkManager** 🚧 — FishNet 网络管理器（待实现）
- **FishNetMessageBus** 🚧 — FishNet 消息总线（待实现）
- **FishNetConnectionAdapter** 🚧 — FishNet 连接适配器（待实现）
- **FishNetSerializerAdapter** 🚧 — FishNet 序列化适配器骨架，将 MemoryPack 注入 FishNet（完整实现由 C6 模块负责）
- **UnityFormatters** — Unity 类型格式化器集合，含 16 种内置类型
- **UnityFormattersInitializer** — Unity 格式化器初始化器，提供 `RegisterAll()` 批量注册
- **SheetElementTypeAttribute** 🚧 — 配置表元素类型标记（待实现）

**Level.View — 视图层**

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

- **Sheet** — 配置表数据定义
- **SheetEditor** — 配置表编辑器
- **SheetImporter** — 配置表导入器
- **SheetImporterEditor** — 导入器编辑器
- **SheetFieldTypeAttribute** — 字段类型标记
- **SheetFieldTypeEditor** — 字段类型编辑器
- **SheetFieldTypeDrawer** — 字段类型绘图器

**ObjectPool — 对象池调试**

- **ObjectPoolViewerEditor** 🚧 — 对象池可视化调试器（待实现）

**AddressablesExtensions — Addressables 扩展**

- **AddressablesGroupsUpdater** — Addressables 分组更新器
- **AddressablesAssetsGroupPresets** — 分组预设配置
- **AddressablesAssetsGroupPresetsEditor** — 分组预设编辑器

**Utils — 编辑器工具**

- **HNUndoableObject** — 支持 Undo/Redo 的对象基类
- **HNDictionaryDrawer** — 字典属性绘图器

> 💡 **提示**：API 参考文档由 DocFX 从代码 XML 注释自动更新。如需修改 API 描述，请直接修改源代码中的 `<summary>` 注释，然后运行 `npm run docs:api` 重新生成。
