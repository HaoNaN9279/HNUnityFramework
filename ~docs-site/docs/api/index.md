---
sidebar_position: 1
---

# API 文档

HNUnityFramework 的完整 API 参考文档由 [DocFX](https://dotnet.github.io/docfx/) 从源代码的 XML 文档注释自动生成。

## 模块索引

### Runtime

- **Core** — 框架核心：`HNUnityFramework`、`HNLogicTime`、`ITickable`
- **MVC** — MVC 模式：`Controller`、`Model`、`ControllerUnit`、`ModelUnit`、`ControllerManager`
- **AssetManager** — 资源管理：`AssetManager`、`AsyncLoadHandle`、`IAssetOperator`
- **ObjectPool** — 对象池：`ObjectPoolManager`、`ObjectPool<T>`、`GameObjectPool`、`PoolBase`
- **ReferencePool** — 引用池：`ReferencePool`、`IReference`、`PooledCollections`
- **HFSM** — 层次状态机：`HFSM`、`HFSMState`、`HFSMTransition`、`HFSMCompoundState`
- **Procedure** — 流程管理：`ProcedureManager`、`ProcedureState`
- **Serialize** — 序列化：`Serialize`、`JsonData`、`JsonObject`
- **Utils** — 工具类：`HNDictionary`、`SerializableDictionary`

### Editor

- **Core** — 编辑器核心：`HNUnityFrameworkGlobalSettingsProvider`、`FrameworkDeployer`
- **Sheet** — 配置表编辑器：`SheetImporter`、`SheetEditor`
- **ObjectPool** — 对象池调试：`ObjectPoolViewerEditor`
- **Addressables** — Addressables 扩展：`AddressablesGroupsUpdater`

> 💡 **提示**：API 参考文档由 DocFX 从代码 XML 注释自动更新。如需修改 API 描述，请直接修改源代码中的 `<summary>` 注释。
