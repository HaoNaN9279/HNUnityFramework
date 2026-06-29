---
sidebar_position: 1
---

# API 文档

HNUnityFramework 的完整 API 参考文档由 [DocFX](https://dotnet.github.io/docfx/) 从源代码的 XML 文档注释自动生成。

## 模块索引

### Runtime

- **Core** — 框架核心：`HNUnityFramework`、`HNLogicTime`、`HNUnityFrameworkGlobalSettings`、`ITickable`
- **MVC** — MVC 模式：`ControllerManager`、`Controller`、`Model`、`ControllerUnit`、`ModelUnit`
- **AssetManager** — 资源管理：`IAssetOperator`、`AsyncLoadHandle`、`AssetDatabaseOperator`、`AddressablesOperator`、`ResourcesOperator`、`AssetCacheItem`  
  ⚠️ `AssetManager` 调度类已弃用，推荐直接使用 Unity Addressables 原生 API
- **ObjectPool** — 对象池：`ObjectPoolManager`、`ObjectPool<T>`、`GameObjectPool`、`PoolBase`、`ObjectPoolBase`、`GameObjectPoolBase`、`PooledObject<T>`
- **ReferencePool** — 引用池：`ReferencePool`、`IReference`、`PooledList<T>`、`PooledDictionary<K,V>` 等 16 种池化集合
- **HFSM** — 层次状态机：`HFSM`、`HFSMState`、`HFSMTransition`、`HFSMCompoundState<T>`、`HFSMEntryState`、`HFSMExitState`
- **Procedure** — 流程管理：`ProcedureManager`、`ProcedureState`
- **Serialize** — 序列化：`Json`、`JsonData`、`JsonObject`
- **Utils** — 工具类：`HNDictionary<TKey,TValue>`、`SerializableDictionary<K,V>`

### Editor

- **Core** — 编辑器核心：`HNUnityFrameworkGlobalSettingsProvider`、`FrameworkDeployer`、`HNUnityFrameworkEditorMenus`
- **ObjectPool** — 对象池调试：`ObjectPoolViewer`、`ObjectPoolViewerEditor`
- **Addressables** — Addressables 扩展：`AddressablesGroupsUpdater`、`AddressablesAssetsGroupPresets`
- **Utils** — 编辑器工具：`HNUndoableObject`、`SerializableDictionaryDrawer`

> 💡 **提示**：API 参考文档由 DocFX 从代码 XML 注释自动更新。如需修改 API 描述，请直接修改源代码中的 `<summary>` 注释，然后运行 `npm run docs:api` 重新生成。
