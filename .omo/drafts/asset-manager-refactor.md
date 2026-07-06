---
slug: asset-manager-refactor
status: awaiting-approval
intent: clear
pending-action: write .omo/plans/asset-manager-refactor.md
approach: 4-wave incremental refactor — fix Core violations first (C1), build AssetManager (C2+C5), integrate GameWorld (C3+C4), then add unit tests (W4). Each wave is independently compilable.
---

# Draft: asset-manager-refactor

## Components (topology ledger)
| id | outcome | status | evidence path |
|----|---------|:------:|---------------|
| C1 | IAssetOperator + AsyncLoadHandle migrated to Unity assembly; Core gets pure-C# IAssetManager | active | `架构~/最终架构.md:151-152` (asmdef noEngineRefs constraint) |
| C2 | AssetManager : ITickable, IAssetManager — ref counting, scene groups, tick-driven cleanup, preload, progress | active | `~/最终架构.md:114` (资源管理=CapabilityModule) |
| C3 | GameWorld + GameWorldDriver updated to hold AssetManager | active | `GameWorld.cs:17` (AssetOperator property → Manager) |
| C4 | Operator auto-select: Editor→AssetDatabaseOperator, Runtime→AddressablesOperator | deferred (after C3) | `GameWorldDriver.cs:18` (hardcoded AddressablesOperator) |
| C5 | AssetCacheItem integrated into AssetManager; IReference empty impls cleaned | active | `AssetCacheItem.cs` (orphan, unused) |

## Open assumptions (announced defaults)
| assumption | adopted default | rationale | reversible? |
|------------|----------------|-----------|:-----------:|
| AssetManager scope | Full orchestration: ref count + scene groups + tick cleanup + preload + progress | User selected "全套编排" | yes — can strip features later |
| AssetCacheItem fate | Integrated into AssetManager as weak-ref cache | User selected "集成到 AssetManager" | yes |
| Test strategy | EditMode NUnit, unified tests dir | User selected "编写单元测试，统一放在一起" | yes |
| AsyncLoadHandle handling | Flat move to Unity assembly (no pure-C# base class abstraction) | Simplest fix; pure-C# base adds complexity without immediate benefit | yes — can extract later |
| IAssetOperator role | Remains as thin wrapper; AssetManager uses it internally | Preserves existing abstraction for operator swapping | yes |
| Naming | AssetManager (not ResourceManager) | Consistent with IAssetOperator naming | no — public API surface |

## Findings (cited - path:lines)

### Core assembly violations
- `IAssetOperator.cs:5` — `using UnityEngine;` + `using Object = UnityEngine.Object;` in Core assembly with `noEngineReferences: true`
- `AsyncLoadHandle.cs:4-9` — `using UnityEngine;` + Addressables types in Core assembly
- `HN.Framework.Core.asmdef:3` — `"noEngineReferences": true` confirms this is a compile-breaking violation
- These are the ONLY 2 files in Core with Unity references (grep confirmed)

### Orphan/invalid code
- `AssetCacheItem.cs:27-86` — fully implemented class with `IAssetCacheItem` interface, zero usages codebase-wide
- `IAssetOperator.cs` — implements `IReference` but `Clear()` is empty in all 3 operators
- `IAssetOperator.cs:2-4` — unused `System.Collections`, `System.Collections.Generic` imports
- `AsyncLoadHandle.cs:2-3` — unused `System.Collections`, `System.Collections.Generic` imports

### Pattern reference (target to match)
- `ObjectPoolManager.cs` — the gold standard: ITickable, held by GameWorld, manages PoolBase children via Dictionary, Tick forwarded to children
- `ProcedureManager.cs` — same pattern, confirms consistency expectation
- `ControllerManager.cs` — same pattern, confirms consistency expectation

### Addressables capabilities (do NOT reimplement)
- Reference counting: `LoadAssetAsync` +1, `Release` -1, auto AssetBundle unload at 0
- Dependency resolution: automatic
- Bundle caching: Unity Cache
- Content update: `CheckForCatalogUpdates` / `UpdateCatalogs`
- Download mgmt: `DownloadDependenciesAsync`

### Addressables gaps (framework MUST add)
- Priority loading queue — Addressables fires all loads concurrently
- Scene/label lifecycle — no automatic load-on-enter / unload-on-exit
- Reference tracking — no central registry of "what's loaded by whom"
- Composite progress — no aggregation across multiple handles
- Preload orchestration — no "load next scene's assets while current scene plays"
- Double-Release guard — Addressables throws on double Release
- Forget-to-Release detection — no leak detection

## Decisions (with rationale)
1. **AssetManager wraps IAssetOperator, not replaces it.** Rationale: preserves operator-swapping (Editor vs Runtime), keeps thin-wrapper layer intact.
2. **AssetManager is ITickable.** Rationale: matches ObjectPoolManager/ProcedureManager/ControllerManager pattern; enables tick-driven cache eviction and preload progress updates.
3. **AsyncLoadHandle moves to Unity as-is.** Rationale: simplest fix for Core violation; pure-C# base class is premature abstraction.
4. **IAssetManager interface defined in Core.** Rationale: framework architecture requires Core to define capability interfaces; concrete AssetManager in Unity.
5. **Tests in `Tests/` at repo root.** Rationale: user directive "所有模块的测试代码统一放在一起".

## Scope IN
- Move IAssetOperator.cs + AsyncLoadHandle.cs to Unity assembly
- Create IAssetManager.cs (Core) + AssetManager.cs (Unity)
- Modify GameWorld.cs + GameWorldDriver.cs
- Operator auto-select (Editor vs Runtime)
- Integrate AssetCacheItem into AssetManager
- Unit tests for AssetManager core logic
- Clean unused usings and IReference empty impls

## Scope OUT (Must NOT have)
- No changes to ObjectPoolManager / ProcedureManager / ControllerManager
- No changes to AddressablesExtensions editor tools (GroupPresets, GroupsUpdater)
- No changes to ResourcesOperator (keeps deprecated)
- No namespace fixes beyond the moved files (arch doc §5.2 tracked separately)
- No pure-C# AsyncLoadHandle base class abstraction
- No PlayMode tests (only EditMode NUnit)

## Open questions
(None — all resolved by user decisions above)

## Approval gate
status: approved
<!-- Approved by user: "继续" -->
