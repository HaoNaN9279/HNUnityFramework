---
slug: memorypack-serialization
status: awaiting-approval
intent: clear
pending-action: write .omo/plans/memorypack-serialization.md
approach: Vendor MemoryPack v1.21.4 source into separate asmdef (HN.Framework.Core.Vendor.MemoryPack), build framework wrapper layer per architecture docs (D2 + C1), add Unity type formatters, write TDD tests, run via Unity MCP, update docs-site.
---

# Draft: memorypack-serialization

## Components (topology ledger)
| id | outcome | status | evidence path |
|----|---------|--------|---------------|
| T1 | MemoryPack v1.21.4 source vendored, compiles in Unity | active | .omo/evidence/task-1-* |
| T2 | D2 Core wrapper (MemoryPackSerializer.cs) implemented | active | .omo/evidence/task-2-* |
| T3 | C1 Core ISerializer + formatters implemented | active | .omo/evidence/task-3-* |
| T4 | C1 Unity UnityFormatters + FishNetAdapter implemented | active | .omo/evidence/task-4-* |
| T5 | Core Tests (TDD) written and passing via Unity MCP | active | .omo/evidence/task-5-* |
| T6 | Unity Tests (TDD) written and passing via Unity MCP | active | .omo/evidence/task-6-* |
| T7 | docs-site updated (guide + API index + architecture) | active | .omo/evidence/task-7-* |
| F1-F4 | Final verification wave | deferred | .omo/evidence/final-* |

## Open assumptions (announced defaults)
| assumption | adopted default | rationale | reversible? |
|------------|----------------|-----------|-------------|
| MemoryPack version | v1.21.4 (2026-02-12) | Latest stable; compatible with Unity 2022.3+ per package.json | Yes |
| Generator delivery | Pre-built DLL from NuGet, not vendored source | Unity Roslyn analyzer system requires DLL format; Generator is a build tool, not runtime dependency | Yes |
| NuGet deps delivery | DLLs in vendor dir, precompiledReferences | System.Runtime.CompilerServices.Unsafe 6.0.0 + System.Collections.Immutable 6.0.0; avoids NuGetForUnity complexity | Yes |
| Vendor asmdef approach | Separate `HN.Framework.Core.Vendor.MemoryPack` asmdef | User chose Option B; clean separation of third-party source | No (user confirmed) |

## Findings (cited - path:lines)
- 架构~/最终架构.md:488-497 — D2 MemoryPack Serialization Core 规格（命名空间、文件、组成）
- 架构~/最终架构.md:631-652 — C1 MemoryPack Serialization Capability 规格（Core ISerializer + Unity UnityFormatters/FishNetAdapter）
- 架构~/最终架构.md:361-362 — 命名空间映射：C1 Core = `HN.Framework.Core.Capability.Serialization`，C1 Unity = `HN.Framework.Unity.Capability.Serialization`
- 架构~/最终架构.md:2062-2066 — 目录结构：Core/Driver/Common/Serialization/ 下 MemoryPackSerializer.cs
- 架构~/最终架构.md:2107-2109 — 目录结构：Core/Capability/Serialization/ 下 ISerializer.cs, MemoryPackFormatter.cs
- MemoryPack GitHub: v1.21.4, MIT license, netstandard2.1 target
- MemoryPack.Core: 依赖 System.Runtime.CompilerServices.Unsafe 6.0.0 + System.Collections.Immutable 6.0.0
- MemoryPack.Unity: 提供 UnityFormatters.cs (Vector2/3/4, Quaternion, Color, Bounds, Rect, Matrix4x4 等) + ProviderInitializer.cs
- Unity asmdef 支持 `"isRoslynAnalyzer": true` 配置 Source Generator DLL
- 现有序列化：Json.cs (708行手写JSON), JsonObject.cs (空基类), JsonData.cs (Unity ISerializationCallbackReceiver桥接)
- 现有测试：3个JSON测试文件共1741行 (JsonDataTests, JsonRoundtripTests, JsonEdgeCaseTests)
- FishNetSerializerAdapter.cs: 仅为6行空骨架 (Runtime/HN.Framework.Unity/Capability/Network/)
- Core.asmdef: `{"name":"HN.Framework.Core","noEngineReferences":true}` (仅4行JSON)
- Unity.asmdef: references Core, overrideReferences=false
- Core Tests.asmdef: references Core, overrideReferences=true, precompiledReferences=[nunit.framework.dll]

## Decisions (with rationale)
1. **Vendor asmdef name**: `HN.Framework.Core.Vendor.MemoryPack` — 遵循 Core 命名空间规范，明确标识为 Vendor 代码
2. **Generator 不作为源码 vendored**: Unity Roslyn analyzer 系统要求 DLL 格式；从 NuGet 提取 MemoryPack.Generator.dll 放置在 Vendor 目录，通过 asmdef `"isRoslynAnalyzer": true` 配置
3. **依赖 DLL 命名与放置**: `System.Runtime.CompilerServices.Unsafe.dll` + `System.Collections.Immutable.dll` 放 `Runtime/HN.Framework.Core/Vendor/MemoryPack/Dependencies/`
4. **UnityFormatters 归属**: 放 `Runtime/HN.Framework.Unity/Capability/Serialization/UnityFormatters.cs` — 属于 C1 Unity 层，不属于 Vendor（Vendor 仅包含不依赖 UnityEngine 的代码）
5. **不修改现有 Json 序列化器**: MemoryPack 是补充（二进制高性能），不是替代；Json.cs 继续服务其使用场景（Inspector 编辑、配置文件）
6. **TDD 策略**: 先写测试暴露需求 → 实现 → Unity MCP 验证。Core 层测试在 `HN.Framework.Core.Tests`，Unity 层在 `HN.Framework.Unity.Tests`

## Scope IN
- MemoryPack v1.21.4 Core 运行时源码（src/MemoryPack.Core/ 下所有 .cs 文件）
- MemoryPack.Generator v1.21.4 DLL（作为 Roslyn Analyzer）
- NuGet 依赖 DLLs
- D2: `MemoryPackSerializer.cs` 包装器（Core/Driver/Common/Serialization/）
- C1 Core: `ISerializer.cs` + `MemoryPackFormatter.cs`（Core/Capability/Serialization/）
- C1 Unity: `UnityFormatters.cs` + `FishNetSerializerAdapter.cs`（Unity/Capability/Serialization/）
- Core 层单元测试（TDD）
- Unity 层单元测试（TDD）
- docs-site 文档更新（guide/serialize.md, api/index.md, dev/architecture.md 状态更新）

## Scope OUT (Must NOT have)
- 不修改任何现有序列化代码（Json.cs, JsonObject.cs, JsonData.cs）
- 不修改 HFSM、MVC、ProcedureManager 等现有模块
- 不实现 C6 Network 的 FishNet 集成（仅定义序列化基础层 + FishNetSerializerAdapter 骨架）
- 不实现 C8 Storage 的存档序列化集成
- 不实现 L4 Sheet 的二进制配置加载
- 不使用 Git Submodule

## Open questions
（无 — 所有关键决策已通过用户确认或默认值覆盖）

## Approval gate
status: approved
review: Momus — [OKAY] (3 clarity fixes applied, 2 optional completeness gaps noted as non-blocking)
pending-action: ready for execution ($start-work)
