# memorypack-serialization - Work Plan

## TL;DR (For humans)

**What you'll get:** 一个完整的 MemoryPack 高性能二进制序列化模块，包含 vendor 的 MemoryPack v1.21.4 源码（独立 asmdef）、框架包装层（MemoryPackSerializer / ISerializer）、Unity 类型格式化器（Vector3/Quaternion 等），以及完整的 TDD 测试覆盖。

**Why this approach:** MemoryPack.Core 源码放入独立的 `HN.Framework.Core.Vendor.MemoryPack` asmdef（你选的隔离方案），Generator 作为预编译 DLL 引入（Unity Roslyn Analyzer 机制需要 DLL 格式），框架包装层遵循架构文档的 D2+C1 两层设计，TDD 策略确保每个模块都有测试验证。

**What it will NOT do:** 不修改现有 JSON 序列化器，不集成 FishNet/Storage/Sheet（仅定义接口和适配器骨架），不使用 Git Submodule。

**Effort:** Medium
**Risk:** Low — MemoryPack 是成熟库，集成路径明确，依赖项少
**Decisions to sanity-check:** Vendor asmdef 独立隔离（你的选择），Generator 用预编译 DLL（Unity 技术约束）

Your next move: 批准后即可开始执行（`$start-work`）。完整执行细节如下。

---

> TL;DR (machine): Medium effort, Low risk. Vendor MemoryPack v1.21.4 source into independent asmdef, build D2+C1 wrapper layer per architecture docs, TDD tests via Unity MCP, update docs.

## Scope
### Must have
- [x] MemoryPack v1.21.4 Core 运行时源码 vendored 到 `Runtime/HN.Framework.Core/Vendor/MemoryPack/`，独立 asmdef 编译通过
- [x] MemoryPack.Generator v1.21.4 DLL 作为 Roslyn Analyzer 配置
- [x] NuGet 依赖 DLLs（Unsafe + Immutable）作为 precompiledReferences
- [x] D2: `MemoryPackSerializer.cs` 框架包装器（`HN.Framework.Core.Driver.Common.Serialization`）
- [x] C1 Core: `ISerializer.cs` + `MemoryPackFormatter.cs`（`HN.Framework.Core.Capability.Serialization`）
- [x] C1 Unity: `UnityFormatters.cs` + `FishNetSerializerAdapter.cs` update（`HN.Framework.Unity.Capability.Serialization`）
- [x] Core 层 TDD 测试（`HN.Framework.Core.Tests/Serialization/`）
- [x] Unity 层 TDD 测试（`HN.Framework.Unity.Tests/Serialization/`）
- [x] 全部 EditMode 测试通过 Unity MCP 验证
- [x] docs-site 文档同步更新

### Must NOT have (guardrails, anti-slop, scope boundaries)
- ❌ 不修改 `Json.cs`, `JsonObject.cs`, `JsonData.cs` 及其他现有代码
- ❌ 不实现 FishNet 网络消息序列化的完整集成（仅骨架适配器）
- ❌ 不使用 Git Submodule
- ❌ 不修改 Core.asmdef 的 `noEngineReferences: true` 约束
- ❌ 不在 Core asmdef 中直接引用 UnityEngine 类型

## Verification strategy
> Zero human intervention - all verification is agent-executed.
- Test decision: **TDD** — 先写测试暴露需求，再实现功能，最后 Unity MCP 验证
- Framework: NUnit + Unity Test Framework (EditMode)
- Evidence: .omo/evidence/task-<N>-memorypack-serialization.<ext>

## Execution strategy
### Parallel execution waves
> Target 5-8 todos per wave. Fewer than 3 (except the final) means you under-split.

**Wave 1** (并行): T1 (Vendor MemoryPack) → 无依赖，可独立执行
**Wave 2** (并行, 依赖 T1): T2 (D2 wrapper), T3 (C1 Core), T4 (C1 Unity) — 三个模块代码开发
**Wave 3** (并行, 依赖 T2+T3+T4): T5 (Core Tests), T6 (Unity Tests) — TDD 测试 + Unity MCP 验证
**Wave 4** (并行, 依赖 T5+T6): T7 (docs update)
**Wave 5** (并行, 依赖 ALL): F1-F4 (最终验证波)

### Dependency matrix
| Todo | Depends on | Blocks | Can parallelize with |
| --- | --- | --- | --- |
| T1 | — | T2, T3, T4 | — |
| T2 | T1 | T5 | T3, T4 |
| T3 | T1 | T5 | T2, T4 |
| T4 | T1 | T6 | T2, T3 |
| T5 | T2, T3 | T7 | T6 |
| T6 | T4 | T7 | T5 |
| T7 | T5, T6 | F1-F4 | — |
| F1-F4 | T7 | — | 互相并行 |

## Todos
> Implementation + Test = ONE todo. Never separate.

- [x] 1. Vendor MemoryPack v1.21.4 源码 + 依赖配置
  What to do / Must NOT do:
  - 从 GitHub release 下载 MemoryPack v1.21.4 源码 ZIP (https://github.com/Cysharp/MemoryPack/archive/refs/tags/1.21.4.zip)
  - 提取 `src/MemoryPack.Core/` 下所有 .cs 文件（不包括 .csproj、bin/、obj/），放入 `Runtime/HN.Framework.Core/Vendor/MemoryPack/MemoryPack.Core/`
  - 创建 asmdef: `Runtime/HN.Framework.Core/Vendor/MemoryPack/HN.Framework.Core.Vendor.MemoryPack.asmdef`
  - asmdef 配置：`"name": "HN.Framework.Core.Vendor.MemoryPack"`, `"rootNamespace": "MemoryPack"`, `"noEngineReferences": true`, `"allowUnsafeCode": true` (MemoryPack 内部使用 unsafe code 优化)
  - 从 NuGet 下载 `MemoryPack.Generator.1.21.4.nupkg`，解压出 `analyzers/dotnet/cs/MemoryPack.Generator.dll`，放入 `Vendor/MemoryPack/Analyzers/`
  - 从 NuGet 下载 `System.Runtime.CompilerServices.Unsafe.6.0.0.nupkg`，解压出 `lib/netstandard2.0/System.Runtime.CompilerServices.Unsafe.dll`，放入 `Vendor/MemoryPack/Dependencies/`
  - 从 NuGet 下载 `System.Collections.Immutable.6.0.0.nupkg`，解压出 `lib/netstandard2.0/System.Collections.Immutable.dll`（需检查是否有 netstandard2.0 目标），放入 `Vendor/MemoryPack/Dependencies/`
  - Vendor asmdef 的 `precompiledReferences` 添加 DLL 文件名：`"System.Runtime.CompilerServices.Unsafe.dll"`, `"System.Collections.Immutable.dll"`
  - 在 `HN.Framework.Core.asmdef` 中新增 `"references"` 数组字段（当前只有 `name` + `noEngineReferences`），添加 vendor asmdef 的 GUID
  - 配置 MemoryPack.Generator.dll 作为 Roslyn Analyzer（在 `HN.Framework.Core.asmdef` 中配置 `"roslynAnalyzers"` 或通过 `.csproj`/`csc.rsp` 方式）
  - 验证：Unity 编译通过，`[MemoryPackable]` 属性可用
  - Must NOT：不要用 Git Submodule，不要修改 MemoryPack 源码，不要覆盖现有文件，不要修改 Core.asmdef 的 `noEngineReferences: true`
  Parallelization: Wave 1 | Blocked by: — | Blocks: T2, T3, T4
  References (executor has NO interview context - be exhaustive):
  - MemoryPack v1.21.4 release: https://github.com/Cysharp/MemoryPack/releases/tag/1.21.4
  - MemoryPack.Core 源码路径：`src/MemoryPack.Core/` (包含 Formatters/ 子目录)
  - NuGet 包下载：https://www.nuget.org/packages/MemoryPack.Generator/1.21.4
  - NuGet 包下载：https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/6.0.0
  - NuGet 包下载：https://www.nuget.org/packages/System.Collections.Immutable/6.0.0
  - 架构~/最终架构.md:2062-2066 (D2 目录结构), 架构~/最终架构.md:2044-2047 (Core asmdef 约束)
  Acceptance criteria (agent-executable): Unity Editor 编译无错误，可通过 `vibe_unityMCP_refresh_unity(compile="request", wait_for_ready=true)` 验证
  QA scenarios (name the exact tool + invocation): 编译验证：`vibe_unityMCP_refresh_unity(mode="force", scope="all", compile="request", wait_for_ready=true)` → 确认无编译错误。Evidence .omo/evidence/task-1-memorypack-serialization.log
  Commit: Y | feat(vendor): add MemoryPack v1.21.4 vendored source with analyzer

- [x] 2. 实现 D2 MemoryPackSerializer 框架包装器
  What to do / Must NOT do:
  - 创建 `Runtime/HN.Framework.Core/Driver/Common/Serialization/MemoryPackSerializer.cs`
  - 命名空间：`HN.Framework.Core.Driver.Common.Serialization`
  - 内容：`MemoryPackSerializer` 静态工具类，封装底层 MemoryPack API：
    - `byte[] Serialize<T>(T value)` — 序列化到字节数组
    - `T Deserialize<T>(byte[] data)` — 从字节数组反序列化
    - `void Serialize<T>(Stream stream, T value)` — 流式序列化
    - `T Deserialize<T>(Stream stream)` — 流式反序列化
    - 支持 `MemoryPackSerializerOptions` 参数传递
  - 所有方法添加完整 XML 文档注释（`<summary>`, `<param>`, `<returns>`, `<typeparam>`）
  - 包装调用 `global::MemoryPack.MemoryPackSerializer.Serialize/Deserialize`（使用 global:: 避免命名冲突）
  - Must NOT：不实现自己的序列化逻辑，纯委托给底层 MemoryPack；不引入 Unity 依赖
  Parallelization: Wave 2 | Blocked by: T1 | Blocks: T5
  References:
  - 架构~/最终架构.md:488-497 (D2 MemoryPack Serialization Core 规格)
  - 架构~/最终架构.md:2062-2066 (目录结构)
  - 命名空间规范：`HN.Framework.Core.Driver.Common.Serialization`
  - 现有 Json.cs 风格参考：Runtime/HN.Framework.Core/Driver/Common/Serialization/Json.cs (708行, 静态类 + 工具方法)
  - MemoryPack API: `MemoryPack.MemoryPackSerializer.Serialize<T>(in T?)` → `byte[]`, `MemoryPack.MemoryPackSerializer.Deserialize<T>(ReadOnlySpan<byte>)` → `T?`
  Acceptance criteria: 代码编译通过，API 签名与架构文档一致
  QA scenarios: 编译验证 `vibe_unityMCP_refresh_unity(compile="request")`，然后 `vibe_unityMCP_execute_code(code="return typeof(HN.Framework.Core.Driver.Common.Serialization.MemoryPackSerializer).GetMethods().Select(m => m.Name).ToArray();")` 确认方法存在。Evidence .omo/evidence/task-2-memorypack-serialization.log
  Commit: Y | feat(core): add D2 MemoryPackSerializer wrapper

- [x] 3. 实现 C1 Core 序列化接口与格式化器
  What to do / Must NOT do:
  - 创建目录 `Runtime/HN.Framework.Core/Capability/Serialization/`
  - 创建 `ISerializer.cs`：
    - 命名空间：`HN.Framework.Core.Capability.Serialization`
    - 接口定义：`byte[] Serialize<T>(T obj)`, `T Deserialize<T>(byte[] data)`, `byte[] Serialize(Type type, object obj)`, `object Deserialize(Type type, byte[] data)`
  - 创建 `MemoryPackFormatter.cs`：
    - 命名空间：`HN.Framework.Core.Capability.Serialization`
    - 内容：`IMemoryPackFormatter<T>` 接口（如果 MemoryPack 未自带）+ 自定义格式化器注册基类
    - 提供 `MemoryPackFormatterProvider` 静态类用于注册/查找格式化器
  - 所有公共 API 添加完整 XML 文档注释
  - Must NOT：不在此处实现具体类型的格式化器（留给 Unity/Vendor 层）
  Parallelization: Wave 2 | Blocked by: T1 | Blocks: T5
  References:
  - 架构~/最终架构.md:631-652 (C1 MemoryPack Serialization 规格)
  - 架构~/最终架构.md:374-375 (命名空间映射)
  - 架构~/最终架构.md:2107-2109 (目录结构)
  - MemoryPack 内置接口：`MemoryPack.IMemoryPackFormatter<T>`, `MemoryPack.MemoryPackFormatterProvider`
  Acceptance criteria: 编译通过，ISerializer 接口定义完整
  QA scenarios: `vibe_unityMCP_refresh_unity(compile="request")` 确认无编译错误。Evidence .omo/evidence/task-3-memorypack-serialization.log
  Commit: Y | feat(core): add C1 ISerializer interface and MemoryPackFormatter

- [x] 4. 实现 C1 Unity 类型格式化器 + FishNetSerializerAdapter
  What to do / Must NOT do:
  - 创建目录 `Runtime/HN.Framework.Unity/Capability/Serialization/`
  - 创建 `UnityFormatters.cs`：
    - 命名空间：`HN.Framework.Unity.Capability.Serialization`
    - 为以下 Unity 类型实现 `MemoryPack.IMemoryPackFormatter<T>`：
      - `Vector2`, `Vector3`, `Vector4`
      - `Vector2Int`, `Vector3Int`
      - `Quaternion`
      - `Color`, `Color32`
      - `Bounds`, `BoundsInt`
      - `Rect`, `RectInt`
      - `Matrix4x4`
      - `LayerMask`
      - `AnimationCurve`（序列化关键帧数据）
      - `Gradient`（序列化颜色键 + alpha 键）
    - 每个 Formatter 继承 `MemoryPack.MemoryPackFormatter<T>` 基类（命名空间 `MemoryPack`，不是 `MemoryPack.Formatters`）
    - 格式：每个组件依次写入/读取（如 Vector3: x, y, z 三个 float）
    - 添加 `UnityFormattersInitializer` 静态类，提供 `RegisterAll()` 方法注册所有格式化器到 `MemoryPackFormatterProvider`
  - 更新 `FishNetSerializerAdapter.cs`（现有6行空骨架）：
    - 路径：`Runtime/HN.Framework.Unity/Capability/Network/FishNetSerializerAdapter.cs`
    - 命名空间：修正为 `HN.Framework.Unity.Capability.Network`（已经是正确命名空间）
    - 添加 `RegisterMemoryPackSerializer()` 静态方法骨架（具体实现留给 C6 Network 模块）
    - 添加 XML 文档注释说明集成方式
  - Must NOT：不在此处实现完整的 FishNet 集成逻辑（留给 C6）
  Parallelization: Wave 2 | Blocked by: T1 | Blocks: T6
  References:
  - 架构~/最终架构.md:646-648 (UnityFormatters 内容)
  - 架构~/最终架构.md:375 (命名空间映射)
  - 架构~/最终架构.md:793-796 (FishNet Custom Serializer 集成描述)
  - MemoryPack.Unity 参考包：`src/MemoryPack.Unity/Assets/MemoryPack.Unity/Runtime/UnityFormatters.cs` (vendored source 内)
  - MemoryPack.Unity 参考包：`src/MemoryPack.Unity/Assets/MemoryPack.Unity/Runtime/ProviderInitializer.cs`
  - 现有 FishNetSerializerAdapter.cs：Runtime/HN.Framework.Unity/Capability/Network/FishNetSerializerAdapter.cs (6行空类)
  - MemoryPack 格式化器基类：`MemoryPack.MemoryPackFormatter<T>`
  Acceptance criteria: 编译通过，格式化器注册方法存在
  QA scenarios: `vibe_unityMCP_refresh_unity(compile="request")` 确认无编译错误。Evidence .omo/evidence/task-4-memorypack-serialization.log
  Commit: Y | feat(unity): add C1 UnityFormatters and update FishNetSerializerAdapter

- [x] 5. Core 层 TDD 测试 + Unity MCP 验证
  What to do / Must NOT do:
  - 创建目录 `Tests/HN.Framework.Core.Tests/Serialization/`
  - 创建 `MemoryPackSerializerTests.cs`（TDD，先写测试后补充实现）：
    - 命名空间：`HN.Framework.Core.Tests.Serialization`
    - 测试类用 `[TestFixture]` 标记
    - 测试用例（使用 `[MemoryPackable] partial` 测试类）：
      1. `Serialize_Int32_RoundtripSuccess` — int 序列化/反序列化往返
      2. `Serialize_String_RoundtripSuccess` — string 往返
      3. `Serialize_NullString_HandlesCorrectly` — null string
      4. `Serialize_ComplexObject_RoundtripSuccess` — 嵌套对象往返（含 List/Dictionary）
      5. `Serialize_EmptyArray_RoundtripSuccess` — 空数组
      6. `Serialize_LargeData_PerformanceAcceptable` — 大数据量（10000 elements）性能在合理范围
      7. `Deserialize_InvalidData_ThrowsException` — 损坏数据抛出异常
      8. `Serialize_ToStream_RoundtripSuccess` — MemoryStream 往返
      9. `Deserialize_Overwrite_RoundtripSuccess` — In-place 反序列化（覆盖已有实例）
    - 使用 NUnit 断言：`Assert.That()`, `Assert.AreEqual()`, `Assert.Throws()`
    - `[SetUp]`/`[TearDown]` 清理测试数据
  - 创建 `MemoryPackFormatterTests.cs`：
    - 测试自定义格式化器注册和查找
    - 测试 ISerializer 接口的非泛型序列化
  - 测试 asmdef：`HN.Framework.Core.Tests.asmdef` 需要确保 `references` 包含 `HN.Framework.Core.Vendor.MemoryPack` 的 GUID
  - 验证命令：`vibe_unityMCP_run_tests(mode="EditMode", assembly_names=["HN.Framework.Core.Tests"])`
  - Must NOT：不引用 `UnityEngine` 命名空间（Core 测试纯 C#）
  Parallelization: Wave 3 | Blocked by: T2, T3 | Blocks: T7
  References:
  - AGENTS.md 测试规范：`[TestFixture]`, `[Test]`, `[SetUp]`/`[TearDown]`, NUnit 断言, 命名模式 `方法名_场景_预期结果`
  - 现有测试参考：Tests/HN.Framework.Core.Tests/Debug/DebugHubTests.cs (查看风格)
  - Core Tests asmdef：Tests/HN.Framework.Core.Tests/HN.Framework.Core.Tests.asmdef (overrideReferences=true, precompiledReferences=["nunit.framework.dll"], defineConstraints=["UNITY_INCLUDE_TESTS"], includePlatforms=["Editor"])
  - MemoryPack 测试模式：`[MemoryPackable] public partial class TestData { ... }`
  Acceptance criteria (agent-executable): 所有测试通过 `vibe_unityMCP_run_tests(mode="EditMode", assembly_names=["HN.Framework.Core.Tests"])` → `vibe_unityMCP_get_test_job(job_id)` 确认 Passed
  QA scenarios:
    - Happy: 运行 Core 测试，全部通过
    - Failure: 确认损坏数据场景正确抛出异常
    Evidence .omo/evidence/task-5-memorypack-serialization.log
  Commit: Y | test(core): add TDD tests for MemoryPackSerializer and formatters

- [x] 6. Unity 层 TDD 测试 + Unity MCP 验证
  What to do / Must NOT do:
  - 创建目录（若不存在）`Tests/HN.Framework.Unity.Tests/Serialization/`
  - 创建 `UnityFormattersTests.cs`：
    - 命名空间：`HN.Framework.Unity.Tests.Serialization`
    - 测试 Unity 类型格式化器往返：
      1. `Serialize_Vector3_RoundtripSuccess` — Vector3(1.5f, 2.5f, 3.5f) 往返
      2. `Serialize_Quaternion_RoundtripSuccess` — Quaternion 往返
      3. `Serialize_Color_RoundtripSuccess` — Color 往返（含 alpha）
      4. `Serialize_Matrix4x4_RoundtripSuccess` — Matrix4x4 往返
      5. `Serialize_Vector3_NegativeValues` — 负值 Vector3
      6. `Serialize_AllUnityTypes_CompositeObject` — 包含多种 Unity 类型的复合对象
      7. `UnityFormattersInitializer_RegisterAll_DoesNotThrow` — 注册不抛异常
    - 确保测试 asmdef (`HN.Framework.Unity.Tests.asmdef`) 的 `references` 包含 vendor asmdef GUID
  - 创建 `FishNetAdapterTests.cs`（可选，基础骨架测试）：
    - 验证 `FishNetSerializerAdapter` 类型存在，方法签名正确
  - 验证命令：`vibe_unityMCP_run_tests(mode="EditMode", assembly_names=["HN.Framework.Unity.Tests"])`
  - Must NOT：运行测试前确保不破坏现有 JSON 序列化测试
  Parallelization: Wave 3 | Blocked by: T4 | Blocks: T7
  References:
  - AGENTS.md 测试规范（同上）
  - 现有测试参考：Tests/HN.Framework.Unity.Tests/Serialization/JsonRoundtripTests.cs (607行，风格参考)
  - Unity Tests asmdef：Tests/HN.Framework.Unity.Tests/HN.Framework.Unity.Tests.asmdef
  - Unity 测试规范：临时 GameObject 设置 `HideFlags.HideAndDontSave` 并在 `[TearDown]` 中 `Object.DestroyImmediate`
  Acceptance criteria (agent-executable): 所有测试通过 `vibe_unityMCP_run_tests(mode="EditMode", assembly_names=["HN.Framework.Unity.Tests"])` → `vibe_unityMCP_get_test_job(job_id)` 确认 Passed；同时确认现有 JSON 测试无回归
  QA scenarios:
    - Happy: 运行 Unity 测试，全部通过（含新测试 + 现有 JSON 测试无回归）
    - Failure: 确认格式化器未注册时序列化抛出有意义的异常
    Evidence .omo/evidence/task-6-memorypack-serialization.log
  Commit: Y | test(unity): add TDD tests for UnityFormatters

- [x] 7. 同步更新 docs-site 文档
  What to do / Must NOT do:
  - 更新 `docs-site~/docs/guide/serialize.md`：
    - 在现有 JSON 序列化内容之后，添加 "## MemoryPack 二进制序列化" 新章节
    - 包含：概述（何时用 JSON vs MemoryPack）、MemoryPackSerializer API 使用示例、ISerializer 接口说明、Unity 类型格式化器列表、`[MemoryPackable]` 属性用法、注意事项（IL2CPP 兼容、性能对比）
    - 保留现有 JSON 文档完整不变
  - 更新 `docs-site~/docs/api/index.md`：
    - Core API 部分添加：
      - `MemoryPackSerializer` — 二进制序列化包装器（Driver/Common/Serialization）
      - `ISerializer` — 统一序列化接口（Capability/Serialization）
      - `IMemoryPackFormatter<T>` — 自定义格式化器接口（Capability/Serialization）
    - Unity API 部分添加：
      - `UnityFormatters` — Unity 类型格式化器集合（Capability/Serialization）
      - `FishNetSerializerAdapter` — FishNet 序列化适配器（Capability/Network，更新状态为 ✅）
    - MemoryPack vendored 源码不作为公开 API 列出（内部实现细节）
  - 更新 `docs-site~/docs/dev/architecture.md`：
    - 模块状态一览表：将 C1 MemoryPack Serialization 状态从 📋 更新为 ✅
    - D2 MemoryPack 序列化核心状态从 📋 更新为 ✅
    - 外部依赖清单：MemoryPack 状态从 📋 更新为 ✅（标注为 vendored source v1.21.4）
  - 更新 `架构~/最终架构.md`：
    - D2.4 MemoryPack Serialization Core 状态从 "📋 规划中" 更新为 "✅ 已实现"
    - C1 MemoryPack 序列化状态从 "📋 规划中" 更新为 "✅ 已实现"
    - 十五、模块实现状态矩阵：C1 和 D2 MemoryPack 相关行更新为 ✅
  - Must NOT：不重写现有文档结构，只做增量更新
  Parallelization: Wave 4 | Blocked by: T5, T6 | Blocks: F1-F4
  References:
  - docs-site~/docs/guide/serialize.md (151行，现有JSON文档)
  - docs-site~/docs/api/index.md (131行，现有API索引)
  - docs-site~/docs/dev/architecture.md (230行，现有架构文档)
  - 架构~/最终架构.md:488-497, 631-652, 1974-1975 (需更新的状态行)
  - AGENTS.md 文档更新规范
  Acceptance criteria (agent-executable): 所有 .md 文件格式正确，无断链，Docusaurus 可构建
  QA scenarios: 检查文档中 API 命名、命名空间、文件路径与实际代码一致。Evidence .omo/evidence/task-7-memorypack-serialization.log
  Commit: Y | docs: update serialization guide, API index, and architecture status for MemoryPack

## Final verification wave
> Runs in parallel after ALL todos. ALL must APPROVE. Surface results and wait for the user's explicit okay before declaring complete.
- [x] F1. Plan compliance audit — 对照架构文档检查：所有 D2+C1 文件是否创建、命名空间是否正确、asmdef 引用是否正确
- [x] F2. Code quality review — 检查 XML 文档注释完整性、命名规范一致性、代码风格与现有代码一致
- [x] F3. Real manual QA — 运行全量 EditMode 测试 (`vibe_unityMCP_run_tests(mode="EditMode")`) 确认零回归
- [x] F4. Scope fidelity — 确认未修改范围外的文件（Json.cs, JsonObject.cs, JsonData.cs, HFSM, MVC, ProcedureManager 等）

## Commit strategy
每个 todo 完成后独立提交（共 7 个 commits），使用 conventional commits 格式：
1. `feat(vendor): add MemoryPack v1.21.4 vendored source with analyzer`
2. `feat(core): add D2 MemoryPackSerializer wrapper`
3. `feat(core): add C1 ISerializer interface and MemoryPackFormatter`
4. `feat(unity): add C1 UnityFormatters and update FishNetSerializerAdapter`
5. `test(core): add TDD tests for MemoryPackSerializer and formatters`
6. `test(unity): add TDD tests for UnityFormatters`
7. `docs: update serialization guide, API index, and architecture status for MemoryPack`

## Success criteria
1. ✅ Unity Editor 编译无错误
2. ✅ 全部 Core Tests 通过（含新增 MemoryPack 测试）
3. ✅ 全部 Unity Tests 通过（含新增 UnityFormatters 测试 + 现有 JSON 测试无回归）
4. ✅ docs-site 文档已同步更新，Docusaurus 可构建
5. ✅ 架构文档状态已更新
6. ✅ 未使用 Git Submodule
