# AGENTS.md

> AI 编程助手和项目贡献者的开发指南。记录当前架构现状，包括已知不一致。

---

## 项目概述

**HNUnityFramework** 是一个轻量级、模块化的 Unity 游戏开发框架，分三层程序集组织：纯 C# 逻辑层 (Core)、Unity 平台层 (Unity)、编辑器工具层 (Editor)。

- **仓库**: `HaoNaN9279/HNUnityFramework`
- **目标平台**: Unity 2022.3+
- **语言**: C# (.NET Standard 2.1)

---

## 架构

### 三层程序集

```
┌─────────────────────────────────────────────────┐
│                 HN.Framework.Editor              │
│            (Editor-only tools, 17 files)         │
│  References: HN.Framework.Core + HN.Framework.Unity │
└────────────────────┬────────────────────────────┘
                     │ references
┌────────────────────▼────────────────────────────┐
│               HN.Framework.Unity                 │
│          (Unity platform layer, 25 files)         │
│          References: HN.Framework.Core           │
└────────────────────┬────────────────────────────┘
                     │ references
┌────────────────────▼────────────────────────────┐
│               HN.Framework.Core                  │
│           (Pure C# logic, 29 files)              │
│           noEngineReferences: true               │
└─────────────────────────────────────────────────┘
```

### asmdef 引用链

```
HN.Framework.Core       (Runtime/HN.Framework.Core/HN.Framework.Core.asmdef)
    └── noEngineReferences: true
        ↓ [references]
HN.Framework.Unity      (Runtime/HN.Framework.Unity/HN.Framework.Unity.asmdef)
    ├── rootNamespace: HN.Framework.Unity
    ├── references: ["HN.Framework.Core"]
    └── autoReferenced: true
        ↓ [references]
HN.Framework.Editor     (Editor/HN.Framework.Editor.asmdef)
    ├── rootNamespace: HN.Framework.Editor
    ├── references: [GUID Core, GUID Unity]
    └── includePlatforms: ["Editor"]
```

---

## 项目结构

```
HNUnityFramework/
├── Runtime/
│   ├── HN.Framework.Core/          # 纯 C# 逻辑层 (29 files)
│   │   ├── HN.Framework.Core.asmdef
│   │   ├── Capability/             # 原子能力层
│   │   │   ├── Asset/              # IAssetOperator
│   │   │   ├── Event/              # 🚧 Stub
│   │   │   ├── Log/                # ILogProvider
│   │   │   ├── Network/            # INetworkManager
│   │   │   ├── Pool/               # ObjectPoolManager
│   │   │   ├── Procedure/          # ProcedureManager, ProcedureState
│   │   │   └── Storage/            # IStorageProvider
│   │   ├── Driver/
│   │   │   ├── Common/             # 公共基础
│   │   │   │   ├── Interfaces/     # ITickable, IReference
│   │   │   │   ├── Pool/
│   │   │   │   │   ├── ObjectPool/      # PoolBase, ObjectPool<T>, PooledObjectBase
│   │   │   │   │   └── ReferencePool/   # ReferencePool, ReferenceCollection, PooledCollections
│   │   │   │   ├── Serialization/       # Json, JsonObject
│   │   │   │   ├── AsyncLoadHandle.cs
│   │   │   │   └── HNLogicTime.cs
│   │   │   └── GameWorld/          # GameWorld
│   │   └── Level/
│   │       └── Logic/              # ⚠️ Old namespace (HN.Framework.Level.Logic)
│   │           ├── HFSM/           # HFSM, HFSMState, HFSMCompoundState, HFSMTransition
│   │           └── MVC/            # ControllerManager, Controller, ControllerUnit, Model, ModelUnit
│   │
│   └── HN.Framework.Unity/         # Unity 平台层 (25 files)
│       ├── HN.Framework.Unity.asmdef
│       ├── Capability/
│       │   ├── Network/            # FishNetNetworkManager, FishNetConnectionAdapter, FishNetMessageBus, FishNetSerializerAdapter (🚧 Stub)
│       │   └── Sheet/              # SheetElementTypeAttribute (🚧 Stub)
│       ├── Driver/
│       │   └── Platform/
│       │       ├── Asset/          # AddressablesOperator, ResourcesOperator, AssetDatabaseOperator, AssetCacheItem
│       │       ├── Coroutine/      # UnityCoroutineProvider
│       │       ├── DataStructures/ # HNDictionary, SerializableDictionary
│       │       ├── Log/            # UnityLogProvider
│       │       ├── ObjectPool/     # GameObjectPool, GameObjectPoolBase, PooledObject
│       │       ├── Rendering/      # HNRenderPipeline, HNRenderPipelineAsset (🚧 Stub)
│       │       ├── Serialization/  # JsonData
│       │       ├── Settings/       # HNUnityFrameworkGlobalSettings
│       │       ├── Time/           # UnityTimeProvider
│       │       └── GameWorldDriver.cs
│       └── Level/
│           └── View/
│               ├── EntityView.cs, ViewFactory.cs   (🚧 Stub)
│               └── Binding/
│                   └── PropertyBinder.cs            (🚧 Stub)
│
├── Editor/                          # 编辑器工具层 (17 files)
│   ├── HN.Framework.Editor.asmdef
│   ├── Core/                        # FrameworkDeployer, Constants, EditorMenus, GlobalSettingsProvider
│   ├── Sheet/                       # Sheet, SheetEditor, SheetImporter, SheetImporterEditor, SheetFieldTypeAttribute, SheetFieldTypeDrawer, SheetFieldTypeEditor
│   ├── ObjectPool/                  # ObjectPoolViewerEditor
│   ├── AddressablesExtensions/      # AddressablesAssetsGroupPresets, PresetsEditor, GroupsUpdater (🚧 Stub)
│   ├── Utils/                       # HNUndoableObject
│   └── HNDictionaryDrawer.cs
│
├── ~docs-site/                      # 文档站点 (Docusaurus, Unity 忽略)
│   ├── docs/
│   │   ├── guide/
│   │   ├── dev/
│   │   └── api/
│   ├── docfx/
│   └── static/
│
└── .github/workflows/
```

---

## Namespace 现状

### HN.Framework.Core 程序集（29 文件）

| Namespace | 文件数 | 文件 |
|-----------|:------:|------|
| `HN.Framework.Core.Capability` | 6 | IAssetOperator, IStorageProvider, ProcedureState, ProcedureManager, ObjectPoolManager, ILogProvider |
| `HN.Framework.Core.Driver` | 1 | GameWorld |
| `HN.Framework.Core.Driver.Common` | 10 | AsyncLoadHandle, HNLogicTime, ReferencePool(3), PooledCollections, PooledObjectBase, PoolBase, ObjectPool, ITickable, IReference |
| `HN.Framework.Core.Driver.Common.Serialization` | 2 | Json, JsonObject |
| `HN.Framework.Level.Logic` ⚠️ | 9 | HFSM(4), MVC(5) — 旧 namespace, 应迁移至 `HN.Framework.Core.Level.Logic` |
| `HN.Framework.Capability.Core.Network` ⚠️ | 1 | INetworkManager — namespace 混杂, 应为 `HN.Framework.Core.Capability.Network` |

### HN.Framework.Unity 程序集（25 文件）

全部 25 个文件使用 `HN.Framework.Unity.*` 命名空间，一致。

### HN.Framework.Editor 程序集（17 文件）

| Namespace | 文件数 | 文件 |
|-----------|:------:|------|
| `HN.Framework.Editor` | 16 | FrameworkDeployer, Constants, EditorMenus, GlobalSettingsProvider, Sheet(7), ObjectPoolViewerEditor, Addressables(3), HNUndoableObject, HNDictionaryDrawer |
| `HN.Framework` ⚠️ | 1 | Sheet.cs — 缺少 `.Editor` 后缀 |

---

## 模块表

### Core 模块 (HN.Framework.Core — 纯 C# 逻辑层)

| 模块 | 路径 | 关键类型 | 状态 |
|------|------|----------|:----:|
| 事件系统 | Capability/Event | *(空)* | 🚧 Stub |
| 日志 | Capability/Log | ILogProvider | ✅ |
| 网络 | Capability/Network | INetworkManager | ⚠️ Namespace 不一致 |
| 对象池 | Capability/Pool | ObjectPoolManager | ✅ |
| 流程管理 | Capability/Procedure | ProcedureManager, ProcedureState | ✅ |
| 存储 | Capability/Storage | IStorageProvider | ✅ |
| 资源接口 | Capability/Asset | IAssetOperator | ✅ |
| 对象池系统 | Driver/Common/Pool/ObjectPool | PoolBase, ObjectPool\<T\>, PooledObjectBase | ✅ |
| 引用池 | Driver/Common/Pool/ReferencePool | ReferencePool, ReferenceCollection, PooledCollections | ✅ |
| 序列化 | Driver/Common/Serialization | Json, JsonObject | ✅ |
| 时间 | Driver/Common | HNLogicTime | ✅ |
| 异步 | Driver/Common | AsyncLoadHandle | ✅ |
| 接口 | Driver/Common/Interfaces | ITickable, IReference | ✅ |
| GameWorld | Driver/GameWorld | GameWorld | ✅ |
| HFSM | Level/Logic/HFSM | HFSM, HFSMState, HFSMCompoundState, HFSMTransition | ✅ |
| MVC | Level/Logic/MVC | ControllerManager, Controller, ControllerUnit, Model, ModelUnit | ✅ |

### Unity 模块 (HN.Framework.Unity — Unity 平台层)

| 模块 | 路径 | 关键类型 | 状态 |
|------|------|----------|:----:|
| 资源操作器 | Driver/Platform/Asset | AddressablesOperator, ResourcesOperator, AssetDatabaseOperator, AssetCacheItem | ✅ |
| 协程 | Driver/Platform/Coroutine | UnityCoroutineProvider | ✅ |
| 数据结构 | Driver/Platform/DataStructures | HNDictionary, SerializableDictionary | ✅ |
| 日志 | Driver/Platform/Log | UnityLogProvider | ✅ |
| 对象池 | Driver/Platform/ObjectPool | GameObjectPool, GameObjectPoolBase, PooledObject | ✅ |
| 渲染管线 | Driver/Platform/Rendering | HNRenderPipeline, HNRenderPipelineAsset, ShaderLibrary(4 hlsl) | 🚧 Stub |
| 序列化 | Driver/Platform/Serialization | JsonData | ✅ |
| 设置 | Driver/Platform/Settings | HNUnityFrameworkGlobalSettings | ✅ |
| 时间 | Driver/Platform/Time | UnityTimeProvider | ✅ |
| GameDriver | Driver/Platform | GameWorldDriver(MonoBehaviour) | ✅ |
| 网络 | Capability/Network | FishNetNetworkManager, FishNetConnectionAdapter, FishNetMessageBus, FishNetSerializerAdapter | 🚧 Stub |
| 配置表 | Capability/Sheet | SheetElementTypeAttribute | 🚧 Stub |
| 实体视图 | Level/View | EntityView(MonoBehaviour), ViewFactory | 🚧 Stub |
| 属性绑定 | Level/View/Binding | PropertyBinder | 🚧 Stub |

### Editor 模块 (HN.Framework.Editor)

| 模块 | 路径 | 关键类型 | 状态 |
|------|------|----------|:----:|
| 编辑器核心 | Core | FrameworkDeployer, HNUnityFrameworkConstants, HNUnityFrameworkEditorMenus, HNUnityFrameworkGlobalSettingsProvider | ✅ |
| 配置表 | Sheet | Sheet, SheetEditor, SheetImporter, SheetImporterEditor, SheetFieldTypeAttribute, SheetFieldTypeDrawer, SheetFieldTypeEditor | ✅ |
| 对象池调试 | ObjectPool | ObjectPoolViewerEditor | ✅ |
| Addressables | AddressablesExtensions | AddressablesAssetsGroupPresets, AddressablesAssetsGroupPresetsEditor, AddressablesGroupsUpdater | 🚧 Stub |
| 工具 | Utils | HNUndoableObject | ✅ |
| 字典绘制器 | (root) | HNDictionaryDrawer | ✅ |

---

## 关键设计模式

1. **Adapter 模式**: `Driver/Platform/*` 提供 Core 接口的 Unity 实现。IAssetOperator → AddressablesOperator/ResourcesOperator/AssetDatabaseOperator, ILogProvider → UnityLogProvider, 等。
2. **GameWorld 作为入口**: GameWorld（纯 C#）持有所有管理器实例；GameWorldDriver（MonoBehaviour）桥接 Unity 生命周期。
3. **多资源策略**: IAssetOperator 有 4 种可互换实现（Addressables、Resources、AssetDatabase、自定义）。
4. **MVVM 风格视图层**: EntityView → ViewFactory → PropertyBinder 模式，用于 ECS 到 GameObject 的映射。
5. **ScriptableObject 配置**: HNUnityFrameworkGlobalSettings 提供 Inspector 可视化配置。

---

## 编码规范

### 命名规范
- **命名空间**: 见上方 namespace 表，新代码统一使用 `HN.Framework.Core.*` / `HN.Framework.Unity.*` / `HN.Framework.Editor`
- **类/接口**: PascalCase，接口以 `I` 开头
- **方法**: PascalCase
- **私有字段**: camelCase

### 文档注释
所有公共 API 必须使用 XML 文档注释:

```csharp
/// <summary>
/// 对象池管理器，负责所有对象池的生命周期管理。
/// </summary>
public class ObjectPoolManager { }
```

### XML 注释标签规范
- `<summary>`: 类型或方法的简要描述（必填）
- `<typeparam>`: 泛型类型参数说明
- `<param>`: 方法参数说明
- `<returns>`: 返回值说明
- `<remarks>`: 补充说明

---

## 开发流程

### 代码变更流程

1. 从 `main` 分支创建功能分支
2. 编写代码，包含完整 XML 文档注释
3. 更新相关文档:
   - 影响公共 API → 更新 `~docs-site/docs/api/index.md`
   - 新增模块 → 在对应指南文档中添加说明
   - 架构变更 → 更新 `~docs-site/docs/dev/architecture.md`
4. 提交并创建 PR

### 工作流检查清单

每次 PR 合并前确认:

- [ ] 公共 API 有完整的 XML 文档注释?
- [ ] 新增模块已在 `~docs-site/docs/api/index.md` 中索引?
- [ ] 架构变更已更新 `~docs-site/docs/dev/architecture.md`?
- [ ] 本次变更是否需要更新 `AGENTS.md`?
- [ ] 本次变更是否需要更新 `README.md`?

---

## 提交规范

- `feat:` — 新功能
- `fix:` — Bug 修复
- `docs:` — 文档更新
- `refactor:` — 代码重构
- `chore:` — 构建/工具变更

---

## 占位说明

项目中标记为 🚧 Stub 的模块目录已创建，但只包含骨架类。这是有意的设计决策:

- Capability/Event: 目录已创建，尚无实现
- Driver/Platform/Rendering: HNRenderPipeline + HNRenderPipelineAsset 基础结构存在，ShaderLibrary 有 4 个 hlsl 文件
- Unity Capability/Network: FishNet 封装是初始版本，API 可能变更
- Unity Capability/Sheet: 只有 SheetElementTypeAttribute
- Unity Level/View: EntityView + ViewFactory + PropertyBinder 基础结构存在，尚无完整绑定系统
- Editor AddressablesExtensions: 分组预设和更新器是初始版本

### 已知遗留问题

1. `HN.Framework.Level.Logic` namespace — HFSM/MVC 模块尚未从旧 namespace 迁移到 `HN.Framework.Core.Level.Logic`。迁移时需更新 9 个文件。
2. `HN.Framework.Capability.Core.Network` — INetworkManager 使用混杂 namespace，应为 `HN.Framework.Core.Capability.Network`。
3. `Editor/Sheet/Sheet.cs` 使用 `HN.Framework` namespace，缺少 `.Editor` 后缀。不影响编译，但不一致。
