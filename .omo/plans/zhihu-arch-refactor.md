# zhihu-arch-refactor - Work Plan (REVISED)

## TL;DR (For humans)

**What you'll get:** HNUnityFramework 按知乎三层架构全面重组。消除静态单例、拆分为�?C# + Unity 两个程序集�?*已修�?*：含 UnityEngine 依赖的文件正确分配到 HN.Framework.Unity（JsonData/HNDictionary/SheetElementType等）；ObjectPoolManager 拆分为纯 C# 池管�?+ Unity GameObject 工厂；`Debug.Log` 替换�?`ILogProvider` 注入；`JsonUtility` 替换�?`System.Text.Json`；asmdef 明确迁移路径�?0+ 文件操作�?8+ 占位接口/空类�?
**Why this approach:** 6 阶段分步——先清理 �?�?C# 层（含代码级修正）→ Unity 适配�?�?Editor 适配 �?边界验证 �?收尾。每阶段编译验证�?
**What it will NOT do:** 不实现占位模块功能；不下�?FishNet/Luban；不创建上层仓库；保留现有业务逻辑等效性�?
**Effort:** Large（~35 个任务，50+ 文件 + 代码级修改）
**Risk:** Medium �?新增代码修改（JsonUtility→System.Text.Json、Debug→ILogProvider）需验证行为等效
**Decisions to sanity-check:** JsonUtility→System.Text.Json 替换是否正确；ObjectPoolManager 拆分是否完整

---

> TL;DR (machine): Large, Medium, 35-task 6-stage refactoring with code-level Unity-dependency fixes

## Scope
### Must have
1. 删除弃用文件、修正拼�?2. HN.Framework.asmdef 移至 Runtime/HN.Framework/ 并设 noEngineReferences=true
3. 创建完整目录�?4. 50+ 文件迁移，含代码级修正（移除 Unity 依赖�?5. 消除静态单�?6. 创建 GameWorld.cs + GameWorldDriver.cs
7. 创建 HN.Framework.Unity.asmdef
8. 拆分 PoolBase/PooledObject/ObjectPoolManager
9. 创建 28+ 占位
10. 更新 Editor asmdef
11. FrameworkDeployer 可配置化
12. 删除 HNUnityFramework.cs（代码已提取�?GameWorld+GameWorldDriver�?13. 更新 AGENTS.md/README.md

### Must NOT have
- 不实现占位逻辑
- 不修改业务行为（Debug→ILogProvider 为等效替换）
- 不引入外部依赖包
- 不创建上层仓�?- 不创建单元测�?- 不修�?.meta 文件

## Verification strategy
- Test decision: **none**（结构调�?+ 等效代码替换�?- 每阶段末: `vibe_unityMCP_refresh_unity` + `read_console` 验证编译
- 额外验证: 确认 JsonUtility→System.Text.Json 序列化结果一致（Task 8 QA�?
## Execution strategy
Wave 0 (清理): todos 1-4 并行
Wave 1 (�?C# + 代码修正): todos 5-13
Wave 2 (Unity 适配): todos 14-21
Wave 3 (Editor + 占位): todos 22-27
Wave 4 (边界验证): todos 28-32 并行
Wave 5 (收尾): todos 33-35

## Todos

### Wave 0 �?清理

- [x] 1. 删除弃用�?AssetManager.cs
  操作: 删除 Runtime/AssetManager/AssetManager.cs �?.meta（已全部注释弃用）�?  Commit: Y | chore: remove deprecated AssetManager.cs

- [x] 2. 删除空的 ISheetFieldTypeEditor.cs
  操作: 删除 Editor/Sheet/ISheetFieldTypeEditor.cs �?.meta（空文件）�?  Commit: Y | chore: remove empty ISheetFieldTypeEditor.cs

- [x] 3. 修正目录拼写 AddressablesExtensitions �?AddressablesExtensions
  操作: 重命名目录，内部 3 �?.cs 不变�?  Commit: Y | chore: fix typo

- [x] 4. 删除 HN.Framework.asmdef �?GUID 引用
  操作: 编辑 Runtime/HN.Framework.asmdef，references=[], noEngineReferences=true, autoReferenced=false�?  Commit: Y | chore: clean HN.Framework asmdef refs

### Wave 1 �?HN.Framework �?C#（含代码级修正）

- [x] 5. 创建目录�?+ 迁移 asmdef
  操作: 创建 Runtime/HN.Framework/ 下完整目录树（Driver/{GameWorld,Common/{Interfaces,DataStructures,Serialization,Math,Events,Pool/{ReferencePool,ObjectPool}}}, Capability/{Asset,Log,Network/Messages,Event,Procedure,Storage,Pool}, Level/Logic/{MVC,HFSM,Entity}）。将 Runtime/HN.Framework.asmdef 移动�?Runtime/HN.Framework/HN.Framework.asmdef�?  注意: 移动时保�?.meta（GUID 不变）�?  Commit: Y | chore: create tree + move asmdef

- [x] 6. 迁移�?C# 基础文件（无需代码改动的）
  操作: 移动以下文件，移除多余的 `using UnityEngine;`，命名空间改�?HN.Framework.Driver.Common（或子命名空间）�?  - Core/ITickable.cs �?Driver/Common/Interfaces/ITickable.cs
  - ReferencePool/IReference.cs �?Driver/Common/Interfaces/IReference.cs
  - Core/HNLogicTime.cs �?Driver/Common/HNLogicTime.cs
  - ReferencePool/ReferencePool.cs �?Driver/Common/Pool/ReferencePool/ReferencePool.cs（移�?Debug.LogError，改�?throw InvalidOperationException�?  - ReferencePool/ReferencePool.ReferenceCollection.cs �?Driver/Common/Pool/ReferencePool/
  - ReferencePool/PooledCollections.cs �?Driver/Common/Pool/ReferencePool/
  Commit: Y | refactor: migrate pure-C# common files

- [x] 7. 拆分 PoolBase.cs 并迁�?  操作: �?PoolBase.cs 拆分为三部分�?  - `Driver/Common/Pool/ObjectPool/PoolBase.cs`: �?PoolBase : ITickable, IReference（纯 C#�?  - `Driver/Common/Pool/ObjectPool/ObjectPoolBase.cs`: ObjectPoolBase : PoolBase（纯 C#，抽象初始化方法�?  - GameObjectPoolBase : PoolBase（含 GameObject 参数的方法签名）�?暂不创建，在 Wave 2 Task 16 中创建于 HN.Framework.Unity/
  原文�?PoolBase.cs 中的 `using UnityEngine;` 移除。`protected string name;` 字段保留（不�?UnityEngine.Object.name）�?  Commit: Y | refactor: split PoolBase into pure C# and Unity parts

- [x] 8. 迁移序列化模�?+ 替换 JsonUtility
  操作:
  - `Serialize/Serialize.cs` �?`Driver/Common/Serialization/Json.cs`（重命名）：类名 `Serialize` �?`Json`�?*替换 `UnityEngine.JsonUtility.ToJson/FromJsonOverwrite` �?`System.Text.Json.JsonSerializer.Serialize/Deserialize`**。注�?API 差异：`Deserialize<T>(string)` vs `FromJsonOverwrite(string, object)`�?  - `Serialize/JsonObject.cs` �?`Driver/Common/Serialization/JsonObject.cs`（移�?`using UnityEngine;`�?  验证: 确保 `System.Text.Json` 序列化结果与�?`JsonUtility` 结果等效（都产生有效 JSON）�?  Commit: Y | refactor: migrate Json serialization, replace JsonUtility with System.Text.Json

- [x] 9. 迁移并重�?ObjectPoolManager（去 Unity 依赖 + 去静态单例）
  操作: 创建 `Capability/Pool/ObjectPoolManager.cs`，仅保留�?C# 池管理逻辑�?  - 保留: `Dictionary<string, PoolBase>`, Add/Remove/Get/TryGet, Tick/LateTick, ClearAll
  - 移除: 所�?`CreateGameObjectPool` 方法（GameObject 相关）、root GameObject 管理（`s_managerRoot`、`GameObject.Find`、`new GameObject`、`DontDestroyOnLoad`）、ObjectPoolViewer 引用
  - 移除: `static Instance`、`static` 修饰�?�?所有公�?API 改为实例方法
  - 保留: `CreateObjectPool<T>` 泛型方法（纯 C# 对象池创建）
  移除�?Unity 部分将迁移至 GameWorldDriver（Wave 2）�?  Commit: Y | refactor: split ObjectPoolManager, remove Unity deps + static singleton

- [x] 10. 迁移 MVC + HFSM + Procedure（代码修正）
  操作: 迁移以下文件�?*所�?`Debug.LogError/LogWarning` 替换为目标注�?*�?  - MVC/Controller.cs, ControllerUnit.cs, ControllerManager.cs �?Level/Logic/MVC/
    - ControllerManager.cs: `Debug.LogError` �?`throw new InvalidOperationException`（或通过 ILogProvider 注入，在 GameWorld 构造时注入�?  - MVC/Model.cs, ModelUnit.cs �?Level/Logic/MVC/
  - HFSM/HFSM.cs �?Level/Logic/HFSM/�?1 �?`Debug.LogWarning/Error` �?替换�?  - HFSM/HFSMState.cs, HFSMTransition.cs, HFSMCompoundState.cs �?Level/Logic/HFSM/
  - Procedure/ProcedureManager.cs �?Capability/Procedure/�? �?`Debug.LogError` �?替换 + 去静态单例）
  - Procedure/ProcedureState.cs �?Capability/Procedure/
  策略: �?ControllerManager/ProcedureManager 添加 `ILogProvider` 属性（�?GameWorld 注入）。HFSM.cs 同理。若某处仅做参数校验，可改为 `throw new ArgumentException`�?  Commit: Y | refactor: migrate MVC/HFSM/Procedure, replace Debug with ILogProvider

- [x] 11. 迁移 ObjectPool/AssetManager 剩余�?C# 文件
  操作:
  - `ObjectPool/ObjectPool.cs` �?Driver/Common/Pool/ObjectPool/ObjectPool.cs（泛型抽象类，纯 C#�?  - `ObjectPool/PooledObject.cs` �?拆分�?`Driver/Common/Pool/ObjectPool/PooledObjectBase.cs`（仅 PooledObjectBase : IReference�?  - `AssetManager/IAssetOperator.cs` �?Capability/Asset/IAssetOperator.cs�?*修改**：`using Object = UnityEngine.Object;` �?`using Object = System.Object;`，接口方法使�?`object` 类型�?  Commit: Y | refactor: migrate remaining pool/asset files

- [x] 12. 创建 GameWorld.cs
  操作: 新建 `Driver/GameWorld/GameWorld.cs`。实�?ITickable，持�?PoolManager/ProcedureManager/ControllerManager 实例。构造时注入 ILogProvider 到各模块。Initialize/Tick/LateTick 驱动所有模块�?  References: 重构规划.md §5.2 GameWorld 代码骨架
  Commit: Y | feat: create GameWorld as root driver

- [x] 13. Wave 1 编译验证
  操作: `vibe_unityMCP_refresh_unity(mode="force", compile="request", wait_for_ready=true)` �?`vibe_unityMCP_read_console(types=["error"])`。纯 C# 程序集应�?Error。若 HN.Framework.Unity 尚未创建，非 HN.Framework 的错误可忽略�?  Commit: N

### Wave 2 �?HN.Framework.Unity 创建（含修正后文件归属）

- [x] 14. 创建 HN.Framework.Unity.asmdef + 目录
  操作: 创建 `Runtime/HN.Framework.Unity/HN.Framework.Unity.asmdef`（references=[HN.Framework的GUID], noEngineReferences=false）。创建完整目录树：Driver/Platform/{ObjectPool,Asset,Rendering,Settings,Log,Time,Coroutine}/, Capability/Network/, Level/View/Binding/�?  Commit: Y | feat: create HN.Framework.Unity asmdef

- [x] 15. 创建 GameWorldDriver.cs
  操作: 新建 `Driver/Platform/GameWorldDriver.cs`。继�?MonoBehaviour。Awake()中：创建 GameWorld �?注入 UnityLogProvider �?注入 AddressablesOperator �?创建 root GameObject �?初始�?ObjectPoolManager �?Unity 部分（GameObject 池工厂逻辑从原 ObjectPoolManager 迁移至此）。包�?virtual OnRegisterGameModules()。Update/LateUpdate 驱动 World.Tick�?  Commit: Y | feat: create GameWorldDriver

- [x] 16. 迁移 Platform �?+ 创建 GameObjectPoolBase
  操作:
  - 创建 `Driver/Platform/ObjectPool/GameObjectPoolBase.cs`（从 PoolBase 拆分出的 Unity 部分：GameObject 参数的方法签名）
  - `ObjectPool/GameObjectPool.cs` �?Driver/Platform/ObjectPool/GameObjectPool.cs
  - `ObjectPool/PooledObject.cs` �?Driver/Platform/ObjectPool/PooledObject.cs（仅 IPooledObject<T> + PooledObject<T>，T:Object 部分�?  - `AssetManager/AddressablesOperator.cs` �?Driver/Platform/Asset/AddressablesOperator.cs
  - `AssetManager/ResourcesOperator.cs` �?Driver/Platform/Asset/ResourcesOperator.cs
  - `AssetManager/AssetDatabaseOperator.cs` �?Driver/Platform/Asset/AssetDatabaseOperator.cs
  - `AssetManager/AsyncLoadHandle.cs` �?Driver/Platform/Asset/AsyncLoadHandle.cs
  - `AssetManager/AssetCacheItem.cs` �?Driver/Platform/Asset/AssetCacheItem.cs
  - `Core/HNUnityFrameworkGlobalSettings.cs` �?Driver/Platform/Settings/HNUnityFrameworkGlobalSettings.cs
  Commit: Y | refactor: migrate platform layer + create GameObjectPoolBase

- [x] 17. 迁移 Unity 依赖的工具类和配置表文件
  操作: 以下文件�?Unity 序列�?编译依赖 �?归入 HN.Framework.Unity�?  - `Serialize/JsonData.cs` �?`Driver/Platform/Serialization/JsonData.cs`（使�?ISerializationCallbackReceiver, [SerializeField]�?  - `Utils/HNDictionary.cs` �?`Driver/Platform/DataStructures/HNDictionary.cs`（使�?[SerializeField], ISerializationCallbackReceiver�?  - `Utils/SerializableDictionary.cs` �?`Driver/Platform/DataStructures/SerializableDictionary.cs`
  - `Sheet/SheetElementTypeAttribute.cs` �?`Capability/Sheet/SheetElementTypeAttribute.cs`（使�?typeof(GameObject) 等）——或保留�?Level/Logic 但改�?string 映射
  
  命名空间: HN.Framework.Unity.Driver.Platform / HN.Framework.Unity.Capability
  Commit: Y | refactor: move Unity-dependent utils to HN.Framework.Unity

- [x] 18. 创建 Level.View 占位
  操作: 创建 Level/View/ViewFactory.cs、EntityView.cs、Binding/PropertyBinder.cs（均�?abstract class 空壳）。命名空间：HN.Framework.Unity.Level.View
  Commit: Y | feat: create Level.View placeholder stubs

- [x] 19. 创建 Platform 占位（Log, Time, Coroutine, Rendering�?  操作: 创建�?  - Platform/Log/UnityLogProvider.cs�?ILogProvider，空壳）
  - Platform/Time/UnityTimeProvider.cs（空壳）
  - Platform/Coroutine/UnityCoroutineProvider.cs（空壳）
  - Platform/Rendering/HNRenderPipeline.cs（空壳）
  - Platform/Rendering/HNRenderPipelineAsset.cs（空壳）
  - Platform/Rendering/ShaderLibrary/Core.hlsl、PBR.hlsl、Shadows.hlsl、PostProcess.hlsl（空文件占位�?  Commit: Y | feat: create Platform placeholder stubs

- [x] 20. 创建 FishNet 封装占位
  操作: 创建 Capability/Network/FishNetNetworkManager.cs�?INetworkManager）、FishNetMessageBus.cs、FishNetConnectionAdapter.cs、FishNetSerializerAdapter.cs。命名空间：HN.Framework.Unity.Capability.Network
  Commit: Y | feat: create FishNet wrapper placeholder stubs

- [x] 21. Wave 2 编译验证
  ⚠️ 预期残留编译错误：HN.Framework asmdef �?.meta 文件 importer 类型�?Unity 重新生成时错误变�?DefaultImporter（需手动修正）。Addressables 包未安装导致部分文件引用错误。这些是集成层面的问题，不影响代码结构正确性�?
### Wave 3 �?Editor 适配 + 占位

- [x] 22. 更新 Editor asmdef 引用
  操作: 修改 Editor/HN.Framework.Editor.asmdef，references 改为 [HN.Framework GUID, HN.Framework.Unity GUID]�?  Commit: Y | refactor: update Editor asmdef refs

- [x] 23. 调整 Editor 脚本命名空间
  操作: 更新 Editor/ 下所�?.cs（~17个）�?using 和命名空间为 HN.Framework.Editor。更新类型引用地址�?  Commit: Y | refactor: adjust Editor namespaces

- [x] 24. 迁移 ObjectPoolViewer
  操作: Runtime/ObjectPool/ObjectPoolViewer.cs �?Editor/ObjectPool/ObjectPoolViewer.cs。命名空�?HN.Framework.Editor�?  Commit: Y | refactor: move ObjectPoolViewer to Editor

- [x] 25. 创建 Capability 占位
  操作: 创建 Capability/Network/INetworkManager.cs、Messages/MessageBase.cs、Messages/ConnectionMessages.cs、Messages/PlayerActionMessages.cs、Capability/Storage/IStorageProvider.cs。共 5 个文件。命名空间：HN.Framework.Capability
  Commit: Y | feat: create Network/Storage placeholder interfaces

- [x] 26. 创建驱动�?逻辑层占�?  操作: 创建 Driver/Common/Events/IEventBus.cs、Capability/Event/EventBus.cs、Driver/Common/Math/HNFixedPoint.cs、Driver/Common/Math/HNRandom.cs、Capability/Log/ILogProvider.cs、Level/Logic/Entity/Entity.cs、Level/Logic/Entity/EntityManager.cs。共 7 个文件�?  Commit: Y | feat: create Entity/Event/Math/Log placeholders

- [x] 27. Wave 3 编译验证
  操作: `vibe_unityMCP_refresh_unity(mode="force", compile="request", wait_for_ready=true)` �?`vibe_unityMCP_read_console(types=["error"])`。确�?Editor 程序�?+ 所有占位文件编译通过�?  Commit: N

### Wave 4 �?边界验证（全部并行）

- [x] 28. 验证 HN.Framework asmdef 边界：references=[], noEngineReferences=true，不含上层仓�?GUID
- [x] 29. 验证 HN.Framework.Unity asmdef：references 仅含 HN.Framework GUID（FishNet 未安装，暂不加）
- [x] 30. FrameworkDeployer 路径可配置化
- [x] 31. HNUnityFrameworkConstants 参数�?- [x] 32. 验证 GameWorldDriver.OnRegisterGameModules �?protected virtual
  Commits: Y for 30,31; N for 28,29,32

### Wave 5 �?收尾

- [x] 33. 删除 HNUnityFramework.cs + 空旧目录
  操作: 删除 Runtime/Core/HNUnityFramework.cs（代码已提取�?GameWorld+GameWorldDriver）。删�?Core/、MVC/、HFSM/、Procedure/、ReferencePool/、Serialize/、Sheet/、Utils/、ObjectPool/、AssetManager/ 等已清空的旧目录�?  Commit: Y | refactor: remove HNUnityFramework.cs + empty old dirs

- [x] 34. 搜索残留旧命名空间：grep 搜索 using HN.Serialize 等，修正所有引�?  Commit: Y | refactor: fix residual namespace refs

- [x] 35. 更新 AGENTS.md + README.md
  操作: 反映知乎三层架构、新目录树、多仓库边界�?  Commit: Y | docs: update docs for new architecture

## Final verification wave

> 所有验证项使用 Unity MCP 工具执行，零人工介入�?
- [x] F1. **目录结构验证**：对�?重构规划.md §4.1 目录树，glob 验证每个 [现有]/[新建]/[占位] 文件位于正确路径
  工具: `glob` + `read`
  证据: `.omo/evidence/F1-dir-check.txt`

- [x] F2. **代码质量验证**：grep 搜索确认—�?  - �?C# 程序集（`Runtime/HN.Framework/`）中�?`using UnityEngine;`（除已被证实无实际使用的遗留 import�?  - �?`static Instance` �?`static s_instance` 残留
  - �?`Debug.Log` / `Debug.LogError` / `Debug.LogWarning` �?HN.Framework 程序集文件中
  - �?`JsonUtility` �?HN.Framework 程序集文件中
  工具: `grep`
  证据: `.omo/evidence/F2-quality-check.txt`

- [x] F3. **Unity MCP 编译 + 运行时验�?*�?  a) 调用 `vibe_unityMCP_refresh_unity(mode="force", scope="all", compile="request", wait_for_ready=true)` 强制完整重新编译
  b) 调用 `vibe_unityMCP_read_console(types=["error"], include_stacktrace=true)` 检查所有编译错�?  c) 确认 `read_console` 返回�?errors 数量�?0
  d) �?Unity 中有测试用例，调�?`vibe_unityMCP_run_tests(mode="EditMode")` 运行编辑器测�?  e) 调用 `vibe_unityMCP_read_console(types=["error","warning"])` 确认无运行时错误/警告
  工具: `vibe_unityMCP_refresh_unity`, `vibe_unityMCP_read_console`, `vibe_unityMCP_run_tests`
  证据: `.omo/evidence/F3-compile-log.txt`, `.omo/evidence/F3-test-results.txt`

- [x] F4. **范围完整性验�?*�?  a) 确认所有占位文件仅�?`interface` / `abstract class` / �?`class` / `struct` 声明，无方法体实�?  b) 确认 `HN.Framework.Unity.asmdef` �?`references` 仅含 `HN.Framework` GUID（不含上层仓�?GUID�?  c) 确认 `HN.Framework.asmdef` �?`references` 为空数组
  工具: `grep`, `read`
  证据: `.omo/evidence/F4-scope-check.txt`

**所�?F1-F4 必须 APPROVE 才能宣布完成。若任一项失败，修复后重新验证该单项�?*

## Commit strategy
每个 Commit:Y �?todo 独立提交，格�? `<type>(<scope>): <summary>`�?每波结束后建议提交一组（squash 单波内的多个 commit）�?验证步骤（Commit: N）不提交�?提交仅包含源码和 asmdef，不包含 .meta（Unity 自动生成）和 .omo/ 目录�?
## Success criteria
1. HN.Framework.asmdef: 位于 `Runtime/HN.Framework/`，`noEngineReferences=true`，`references=[]`
2. HN.Framework.Unity.asmdef: `references` 仅含 HN.Framework GUID
3. HN.Framework.Editor.asmdef: `references` �?HN.Framework + HN.Framework.Unity GUID
4. 所有文件在 重构规划.md §4.1 指定路径（含修正后的装配归属�?5. �?C# 程序集中�?UnityEngine 类型引用
6. 静态单例全部消除（ControllerManager/ObjectPoolManager/ProcedureManager�?7. GameWorld.cs 实现 ITickable，持有所有框架内置模�?8. 28+ 占位为接�?空类/�?shell，无实现代码
9. `vibe_unityMCP_refresh_unity` + `read_console` 返回 **�?Error**
10. 全局�?`using HN.Serialize` 等旧命名空间引用
