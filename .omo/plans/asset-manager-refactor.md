# asset-manager-refactor - Work Plan

## TL;DR (For humans)
<!-- Fill LAST -->

**What you'll get:** 资源管理模块从"薄透传"升级为"厚编排"——新增 `AssetManager` 统一管理所有资产的加载、释放、场景分组和引用计数，支持预加载队列和加载进度聚合。修复 Core 程序集中 2 个文件的 Unity 依赖违规。集成已有的 `AssetCacheItem` 弱引用缓存。

**Why this approach:** 逐波增量重构——每波独立可编译，不破坏现有代码。先修违规（Core→Unity），再建 AssetManager，最后集成 GameWorld。AssetManager 参照 `ObjectPoolManager` 的 ITickable + GameWorld 组合模式，使资源模块与框架其他管理器保持一致。

**What it will NOT do:** 不重写 Addressables 的加载/缓存/引用计数逻辑，不修改对象池/流程管理/控制器等其他模块，不删除 ResourcesOperator（保持 deprecated 标记），不修改编辑器 Addressables 扩展工具。

**Effort:** Medium
**Risk:** Low — 迁移不改逻辑，新增类不破坏现有 API（IAssetOperator 仍可用）
**Decisions to sanity-check:** IAssetManager 接口签名（纯 C#，无 Unity 依赖）；AssetManager 内部使用 IAssetOperator 还是直接调 Addressables API

Your next move: approve this plan, then run `$start-work`. Full execution detail follows below.

---

> TL;DR (machine): Medium effort, Low risk — 4 waves, 15 todos: fix 2 Core violations → build AssetManager → integrate GameWorld → test

## Scope
### Must have
- Move `IAssetOperator.cs` + `AsyncLoadHandle.cs` from Core to Unity assembly
- Create `IAssetManager` (pure C# interface in Core) + `AssetManager` (Unity impl, ITickable)
- AssetManager: reference counting, scene/label groups, Tick auto-unload, preload queue, progress aggregation
- Integrate `AssetCacheItem` as AssetManager internal weak-ref cache
- Update GameWorld.cs + GameWorldDriver.cs to hold and drive AssetManager
- Operator auto-select: Editor → AssetDatabaseOperator, Runtime → AddressablesOperator
- Remove `IReference` from `IAssetOperator` + clean empty `Clear()` impls
- Clean unused usings in moved files
- NUnit EditMode tests for AssetManager logic, in `Tests/` at repo root

### Must NOT have (guardrails, anti-slop, scope boundaries)
- MUST NOT create a pure-C# AsyncLoadHandle base class — unnecessary abstraction
- MUST NOT modify ObjectPoolManager, ProcedureManager, ControllerManager
- MUST NOT modify AddressablesExtensions editor tools (GroupPresets, GroupsUpdater)
- MUST NOT touch ResourcesOperator (keep deprecated)
- MUST NOT change `IAssetOperator` method signatures (keep backward compat)
- MUST NOT introduce new external dependencies
- MUST NOT use `LoadSceneGroup` as a Unity scene-loading system — it operates on Addressables labels only, delegating actual scene loading to `Addressables.LoadSceneAsync` if needed
- MUST NOT implement priority-based preload queue — use simple FIFO
- MUST NOT delete `ResourcesOperator` — keep as fallback when Addressables unavailable
- All AssetManager Dictionary access is main-thread-only (Unity APIs are main-thread; no lock needed)
- AssetDatabaseOperator autocomplete type is wrapped in `#if UNITY_EDITOR` — auto-select must use compile-time `#if`, not runtime reflection
- When `HAS_ADDRESSABLES` is undefined, `AssetManager.SetOperator` should accept `ResourcesOperator` as fallback (no crash, log warning)

### Metis-identified constraints (folded in)
- C3/C4 no conflict: operator auto-select is compile-time `#if` in `GameWorldDriver.Awake()` — single decision point
- IAssetOperator removes `IReference` inheritance (todo 12) — no downstream `is IReference` checks found in codebase
- IAssetManager in Core returns `void` for LoadAsset (no Unity Object types) — progress via `Action<float>` callbacks
- Test asmdef defined in todo 13 — references `HN.Framework.Unity` + `nunit.framework`, `includePlatforms: ["Editor"]`
- AssetManager injection: `SetOperator(IAssetOperator)` setter allows mock injection for tests

## Verification strategy
> Zero human intervention - all verification is agent-executed.
- Test decision: tests-after — write implementation first, then tests (AssetManager is new code with no existing tests to lock)
- Test framework: NUnit (Unity Test Framework, EditMode — can run without PlayMode)
- Test location: `Tests/HN.Framework.Unity.Tests/`
- Evidence: .omo/evidence/task-<N>-asset-manager-refactor.txt (compile output + test results)

## Execution strategy
### Parallel execution waves
- **Wave 1** (4 todos): Assembly clean-up — all independent, can run in parallel
- **Wave 2** (5 todos): AssetManager build — sequential (each builds on previous)
- **Wave 3** (3 todos): GameWorld integration — depends on W2
- **Waves 1+2**: Can partially overlap (W2.1 can start after W1.4)

### Dependency matrix (by new flat task number)
| Todo | Depends on | Blocks | Wave |
|------|-----------|--------|:--:|
| 1 | — | 4, 5 | W1 |
| 2 | — | 4, 5 | W1 |
| 3 | — | 4 | W1 |
| 4 | 1, 2, 3 | 5 | W1 |
| 5 | 4 | 6, 7, 8, 9 | W2 |
| 6 | 5 | 7 | W2 |
| 7 | 6 | 8 | W2 |
| 8 | 7 | 9 | W2 |
| 9 | 8 | 10 | W2 |
| 10 | 9 | 11 | W3 |
| 11 | 10 | 12 | W3 |
| 12 | 11 | 13 | W3 |
| 13 | 12 | 14, 15 | W4 |
| 14 | 13 | — | W4 |
| 15 | 13 | — | W4 |

## Todos
> Implementation + Test = ONE todo. Never separate.

### Wave 1: Assembly clean-up

- [x] 1. [W1] Move IAssetOperator.cs to HN.Framework.Unity assembly
  What to do: Move file from `Runtime/HN.Framework.Core/Capability/Asset/IAssetOperator.cs` to `Runtime/HN.Framework.Unity/Capability/Asset/IAssetOperator.cs`. Change namespace from `HN.Framework.Core.Capability` to `HN.Framework.Unity.Capability.Asset`. Remove unused `using System.Collections;` and `using System.Collections.Generic;`. Remove `: IReference` from interface declaration (operators are singletons, never pooled). Remove `using HN.Framework.Core.Driver.Common;` (only needed for IReference which is being removed). Remove `using Object = UnityEngine.Object;` alias — use `UnityEngine.Object` directly.
  Must NOT do: Do not change the three method signatures (LoadAsset, LoadAssetAsync, ReleaseAsset). Do not change the XML doc comments. Do not move or touch the three operator implementations yet.
  Parallelization: Wave 1 | Blocked by: — | Blocks: 1.4, 2.1
  References: `Runtime/HN.Framework.Core/Capability/Asset/IAssetOperator.cs:1-37`, `Runtime/HN.Framework.Core/HN.Framework.Core.asmdef:3` (noEngineRefs: true)
  Acceptance criteria: Unity compilation succeeds. `using HN.Framework.Unity.Capability.Asset;` resolves in Unity assembly.
  QA scenarios: Happy — recreate file in Unity assembly, delete from Core, compile. Failure — grep `UnityEngine` in Core `Capability/Asset/` returns zero.
  Evidence: .omo/evidence/task-1-asset-manager-refactor.txt
  Commit: Y | refactor(core): move IAssetOperator to HN.Framework.Unity assembly

- [x] 2. [W1] Move AsyncLoadHandle.cs to HN.Framework.Unity assembly
  What to do: Move file from `Runtime/HN.Framework.Core/Driver/Common/AsyncLoadHandle.cs` to `Runtime/HN.Framework.Unity/Driver/Platform/Asset/AsyncLoadHandle.cs`. Change namespace from `HN.Framework.Core.Driver.Common` to `HN.Framework.Unity.Driver.Platform`. Remove unused `using System.Collections;` and `using System.Collections.Generic;`. Keep all `#if HAS_ADDRESSABLES` conditional compilation and all constructors.
  Must NOT do: Do not refactor into pure-C# base + Unity subclass. Do not change `AsyncLoadStatus` enum or `CompletedEvent`.
  Parallelization: Wave 1 | Blocked by: — | Blocks: 1.4, 2.1
  References: `Runtime/HN.Framework.Core/Driver/Common/AsyncLoadHandle.cs:1-147`
  Acceptance criteria: Unity compilation succeeds. `using HN.Framework.Unity.Driver.Platform;` resolves AsyncLoadHandle.
  QA scenarios: Happy — move file, update namespace, compile. Failure — grep `UnityEngine` in Core `Driver/Common/` returns zero.
  Evidence: .omo/evidence/task-2-asset-manager-refactor.txt
  Commit: Y | refactor(core): move AsyncLoadHandle to HN.Framework.Unity assembly

- [x] 3. [W1] Create IAssetManager.cs in Core (pure C# interface)
  What to do: Create `Runtime/HN.Framework.Core/Capability/Asset/IAssetManager.cs`. Define `IAssetManager` interface with members: `void LoadSceneGroup(string label, Action<float> onProgress = null);` `void UnloadSceneGroup(string label);` `void PreloadSceneGroup(string label, Action<float> onProgress = null);` `void LoadAsset(string key);` `void ReleaseAsset(string key);` `bool IsLoaded(string key);` `int GetReferenceCount(string key);` `float GetGroupProgress(string label);` `int GetLoadedAssetCount();`. Namespace: `HN.Framework.Core.Capability`. Full XML doc comments on every member. Imports only `System`.
  Must NOT do: No Unity types. No AsyncLoadHandle. No generics with Unity constraints.
  Parallelization: Wave 1 | Blocked by: — | Blocks: 1.4, 2.1
  References: `Runtime/HN.Framework.Core/Capability/Log/ILogProvider.cs:1-7` (pure C# capability interface pattern), `Runtime/HN.Framework.Core/Capability/Storage/IStorageProvider.cs:1-9`
  Acceptance criteria: Compiles in Core assembly (noEngineReferences: true). Zero Unity references via grep.
  QA scenarios: Happy — compile Core. Failure — grep `UnityEngine` in IAssetManager.cs returns zero.
  Evidence: .omo/evidence/task-3-asset-manager-refactor.txt
  Commit: Y | feat(core): add IAssetManager pure-C# interface

- [x] 4. [W1] Update all cross-references to moved types
  What to do: Update using statements in all files referencing old namespaces:
  - `AddressablesOperator.cs:11` → `using HN.Framework.Unity.Capability.Asset;`
  - `ResourcesOperator.cs:5-6` → same
  - `AssetDatabaseOperator.cs:5-6` → same
  - `GameWorld.cs:2,17` → ensure IAssetManager referenced; add `public IAssetManager? AssetManager { get; set; }` property (settable, null-safe for backward compat)
  - `GameWorldDriver.cs:18` → add `using HN.Framework.Unity.Capability.Asset;`
  Must NOT do: Do not change method bodies. Do not touch Editor assembly files.
  Parallelization: Wave 1 | Blocked by: 1.1, 1.2, 1.3 | Blocks: 2.1
  References: Each file listed above — see specific lines in prior todos
  Acceptance criteria: Core + Unity + Editor assemblies all compile. No CS0234 errors.
  QA scenarios: Happy — compile all assemblies. Failure — any CS0234 → grep for old namespace strings.
  Evidence: .omo/evidence/task-4-asset-manager-refactor.txt
  Commit: Y | refactor: update references to moved IAssetOperator and AsyncLoadHandle

### Wave 2: AssetManager implementation

- [x] 5. [W2] Create AssetManager.cs skeleton
  What to do: Create `Runtime/HN.Framework.Unity/Capability/Asset/AssetManager.cs`. `public sealed class AssetManager : ITickable, IAssetManager`. Private fields: `Dictionary<string, AssetLoadRecord> m_LoadRecords`, `Dictionary<string, HashSet<string>> m_SceneGroups`, `Dictionary<string, AssetCacheItem> m_AssetCache`, `Queue<PreloadRequest> m_PreloadQueue`, `float m_AutoUnloadDelay = 30f`, `IAssetOperator m_Operator`. Define internal struct `AssetLoadRecord` with: `string Key; object Asset; int RefCount; float LastAccessTime; List<string> SceneLabels;`. Define internal struct `PreloadRequest` with: `string Label; Action<float> OnProgress;`. Add `SetOperator(IAssetOperator op)`. Implement `Tick()` + `LateTick()` as empty. All IAssetManager methods as `throw new NotImplementedException()`.
  Must NOT do: No real logic yet — only skeleton. No GameWorld references.
  Parallelization: Wave 2 | Blocked by: 1.4 | Blocks: 2.2-2.5
  References: `Runtime/HN.Framework.Core/Capability/Pool/ObjectPoolManager.cs:1-317` (pattern), `Runtime/HN.Framework.Unity/Driver/Platform/Asset/AssetCacheItem.cs:1-86`
  Acceptance criteria: Compiles. All IAssetManager methods present as stubs. SetOperator stores reference.
  QA scenarios: Happy — compile, verify skeleton. Failure — missing interface members → compile error.
  Evidence: .omo/evidence/task-5-asset-manager-refactor.txt
  Commit: Y | feat(unity): add AssetManager skeleton

- [x] 6. [W2] Implement reference counting
  What to do: Implement LoadAsset, ReleaseAsset, IsLoaded, GetReferenceCount, GetLoadedAssetCount:
  - LoadAsset: check cache weak-ref → m_Operator.LoadAsset → wrap in AssetCacheItem → create/update AssetLoadRecord (increment RefCount, set LastAccessTime).
  - ReleaseAsset: find record → decrement RefCount → if 0, set LastAccessTime (countdown for Tick auto-unload). Key not found → log warning, no crash.
  - IsLoaded: `m_LoadRecords.ContainsKey(key) && RefCount > 0`.
  - GetReferenceCount: return RefCount or 0.
  - GetLoadedAssetCount: count records with RefCount > 0.
  Must NOT do: Do NOT call Addressables.Release() directly. Do NOT implement Tick-driven unload (that's 2.4).
  Parallelization: Wave 2 | Blocked by: 2.1 | Blocks: 2.3
  References: Your own AssetManager.cs from 2.1, `IAssetOperator.cs` (moved in 1.1), `AssetCacheItem.cs:46-50`
  Acceptance criteria: Compiles. All five methods have real logic. Load→refcount++, Release→refcount--, IsLoaded returns correct bool.
  QA scenarios: Happy — LoadAsset → IsLoaded=true → ReleaseAsset → refcount drops. Failure — LoadAsset with invalid key → graceful error. Double Release → log warning.
  Evidence: .omo/evidence/task-6-asset-manager-refactor.txt
  Commit: Y | feat(unity): implement AssetManager reference counting

- [x] 7. [W2] Implement scene group lifecycle
  What to do: Implement LoadSceneGroup, UnloadSceneGroup, PreloadSceneGroup, GetGroupProgress:
  - LoadSceneGroup: iterate records with matching label → LoadAsset each → invoke onProgress after each. Progress = loaded/total.
  - UnloadSceneGroup: ReleaseAsset each asset in group → remove group from m_SceneGroups.
  - PreloadSceneGroup: enqueue PreloadRequest to m_PreloadQueue.
  - GetGroupProgress: return loaded/total for label, 0 if untracked.
  Must NOT do: Do not hardcode label→key mapping. Groups registered manually via LoadSceneGroup.
  Parallelization: Wave 2 | Blocked by: 2.2 | Blocks: 2.4
  References: Your AssetManager.cs (2.2 implementation for LoadAsset/ReleaseAsset)
  Acceptance criteria: Compiles. LoadSceneGroup increments refcounts for group assets. UnloadSceneGroup decrements them.
  QA scenarios: Happy — LoadSceneGroup("L1") → loaded → GetGroupProgress("L1")=1.0 → UnloadSceneGroup → progress=0. Failure — UnloadSceneGroup on non-loaded group → no-op.
  Evidence: .omo/evidence/task-7-asset-manager-refactor.txt
  Commit: Y | feat(unity): implement AssetManager scene group lifecycle

- [x] 8. [W2] Implement Tick-driven operations
  What to do: Fill Tick() body:
  1. Auto-unload: iterate m_LoadRecords, for RefCount==0 and (Time.time - LastAccessTime) > m_AutoUnloadDelay → m_Operator.ReleaseAsset → remove record → remove from cache.
  2. Preload processing: dequeue 1 from m_PreloadQueue per Tick → call LoadSceneGroup.
  3. Cache eviction: iterate m_AssetCache, remove entries where !cacheItem.IsAlive.
  LateTick(): empty.
  Must NOT do: Do NOT call Resources.UnloadUnusedAssets(). Do NOT use coroutines.
  Parallelization: Wave 2 | Blocked by: 2.3 | Blocks: 2.5
  References: `ObjectPoolManager.cs:240-257` (Tick pattern), `AssetCacheItem.cs:73-78` (IsAlive)
  Acceptance criteria: Compiles. Tick processes expired assets. Preload queue consumed 1/Tick. Dead weak-refs removed.
  QA scenarios: Happy — Load+Release+wait→Tick evicts. 3 preloads→3 ticks→all consumed. Failure — RefCount>0 asset never auto-unloaded.
  Evidence: .omo/evidence/task-8-asset-manager-refactor.txt
  Commit: Y | feat(unity): implement AssetManager tick-driven cleanup

- [x] 9. [W2] Progress aggregation + AssetCacheItem integration
  What to do: Add `Dictionary<string, (int loaded, int total)> m_GroupProgress`. Update in LoadSceneGroup/UnloadSceneGroup. GetGroupProgress returns loaded/(float)total. Integrate AssetCacheItem: when loading, `ReferencePool.Acquire<AssetCacheItem>()` → `Initialize(key, asset)`. When evicting, `ReferencePool.Release(cacheItem)`. Add `Clear()` to AssetManager: release all, clear all dicts, return all cache items to pool. Expose `SetAutoUnloadDelay(float s)` and `int PendingPreloadCount` (for testability, use `internal`).
  Must NOT do: Do not modify AssetCacheItem.cs beyond needed for pooling (already has IReference).
  Parallelization: Wave 2 | Blocked by: 2.4 | Blocks: 3.1
  References: `AssetCacheItem.cs:27-86`, `ReferencePool.cs:18-21`
  Acceptance criteria: GetGroupProgress returns correct float. Cache items pooled via ReferencePool. Clear() resets all state.
  QA scenarios: Happy — LoadSceneGroup→progress 1.0. Clear()→count 0. Failure — Clear() twice→no crash.
  Evidence: .omo/evidence/task-9-asset-manager-refactor.txt
  Commit: Y | feat(unity): add progress aggregation and AssetCacheItem integration

### Wave 3: GameWorld integration

- [x] 10. [W3] Modify GameWorld.cs
  What to do: Edit `Runtime/HN.Framework.Core/Driver/GameWorld/GameWorld.cs`:
  1. Add `public IAssetManager? AssetManager { get; set; }` (settable, null-safe — Core cannot new() Unity type).
  2. In Tick(): add `AssetManager?.Tick();` after PoolManager.Tick().
  3. In LateTick(): add `AssetManager?.LateTick();` after PoolManager.LateTick().
  4. Keep existing `IAssetOperator AssetOperator` property unchanged (backward compat).
  Must NOT do: Do NOT `new AssetManager()` in Core. Do NOT remove existing AssetOperator property.
  Parallelization: Wave 3 | Blocked by: 2.5 | Blocks: 3.2
  References: `GameWorld.cs:17` (AssetOperator property), `GameWorld.cs:36-40` (Tick loop)
  Acceptance criteria: Core compiles. Tick/LateTick forward to AssetManager if not null. Existing AssetOperator untouched.
  QA scenarios: Happy — set AssetManager, Tick→AssetManager.Tick called. Failure — AssetManager null→Tick no-op.
  Evidence: .omo/evidence/task-10-asset-manager-refactor.txt
  Commit: Y | feat(core): add IAssetManager property to GameWorld

- [x] 11. [W3] Modify GameWorldDriver.cs + Operator auto-select
  What to do: Edit `Runtime/HN.Framework.Unity/Driver/Platform/GameWorldDriver.cs`:
  1. Add `using HN.Framework.Unity.Capability.Asset;`
  2. In Awake(), after `World = new GameWorld();`:
  ```csharp
  var assetManager = new AssetManager();
  #if UNITY_EDITOR
  assetManager.SetOperator(new AssetDatabaseOperator());
  #else
  assetManager.SetOperator(new AddressablesOperator());
  #endif
  World.AssetManager = assetManager;
  ```
  3. Keep existing `World.AssetOperator = new AddressablesOperator();` for backward compat.
  Must NOT do: Do not break existing World.AssetOperator access.
  Parallelization: Wave 3 | Blocked by: 3.1 | Blocks: 3.3
  References: `GameWorldDriver.cs:14-21` (Awake), `AssetDatabaseOperator.cs:1-56`, `AddressablesOperator.cs:1-70`
  Acceptance criteria: Unity compiles. Editor→AssetDatabaseOperator, Runtime→AddressablesOperator.
  QA scenarios: Happy — Editor uses AssetDatabaseOperator. Build for Runtime uses AddressablesOperator. Failure — AssetManager.SetOperator receives null→check #if.
  Evidence: .omo/evidence/task-11-asset-manager-refactor.txt
  Commit: Y | feat(unity): inject AssetManager with auto-select operator

- [x] 12. [W3] Clean dead code
  What to do:
  1. In moved `IAssetOperator.cs`: remove `: IReference` from interface, delete `Clear()` signature.
  2. In AddressablesOperator.cs: remove `Clear()` method (lines 65-68).
  3. In ResourcesOperator.cs: remove `Clear()` method (lines 51-54).
  4. In AssetDatabaseOperator.cs: remove `Clear()` method (lines 51-54).
  5. Remove `using HN.Framework.Core.Driver.Common;` from operator files if only used for IReference.
  Must NOT do: Do not remove IReference from IAssetCacheItem (it uses ReferencePool legitimately). Do not delete any file.
  Parallelization: Wave 3 | Blocked by: 3.2 | Blocks: 4.1
  References: IAssetOperator.cs:14, AddressablesOperator.cs:65-68, ResourcesOperator.cs:49-52, AssetDatabaseOperator.cs:48-51
  Acceptance criteria: Compiles. IAssetOperator no longer extends IReference. No Clear() on any operator.
  QA scenarios: Happy — verify interface declaration. Failure — call Clear() on IAssetOperator→compile error (expected).
  Evidence: .omo/evidence/task-12-asset-manager-refactor.txt
  Commit: Y | refactor(unity): remove IReference from IAssetOperator

### Wave 4: Unit tests

- [x] 13. [W4] Create test infrastructure
  What to do: Create `Tests/HN.Framework.Unity.Tests/` directory. Create asmdef: `{"name":"HN.Framework.Unity.Tests","references":["HN.Framework.Core","HN.Framework.Unity"],"includePlatforms":["Editor"],"overrideReferences":true,"precompiledReferences":["nunit.framework.dll"]}`. Create `AssetManagerTests.cs` with `[TestFixture]` class and one placeholder `[Test] public void Placeholder_Passes() => Assert.Pass();`. Create `Runtime/HN.Framework.Unity/AssemblyInfo.cs` with `[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("HN.Framework.Unity.Tests")]` so tests can access `internal` members like `PendingPreloadCount` and `SetAutoUnloadDelay`.
  Must NOT do: No PlayMode tests. No real test logic yet.
  Parallelization: Wave 4 | Blocked by: 3.3 | Blocks: 4.2, 4.3
  References: Unity EditMode test setup: https://docs.unity3d.com/Manual/testing-editortestsrunner.html
  Acceptance criteria: asmdef compiles. Test Runner discovers fixture. Placeholder passes.
  QA scenarios: Happy — Test Runner→HN.Framework.Unity.Tests→run→1 passed. Failure → asmdef error.
  Evidence: .omo/evidence/task-13-asset-manager-refactor.txt
  Commit: Y | test: add EditMode test infrastructure for AssetManager

- [x] 14. [W4] Reference counting tests
  What to do: Write 6 NUnit tests:
  - LoadAsset_IncrementsRefCount: Load→assert refcount==1, IsLoaded==true.
  - ReleaseAsset_DecrementsRefCount: Load+Release→assert refcount==0.
  - DoubleRelease_NoCrash: Load+Release+Release→no exception, refcount≥0.
  - Release_UnknownKey_NoCrash: Release nonexistent→no exception.
  - GetLoadedAssetCount_ReflectsState: Load 3→count==3, Release 2→count==1.
  - IsLoaded_FalseForUnknownKey: IsLoaded("nonexistent")→false.
  Use [SetUp] to create AssetManager with mock IAssetOperator (nested test class). Mock only needs: `LoadAsset` returns `new GameObject("test")` (call `HideFlags.HideAndDontSave` to avoid scene pollution, `DestroyImmediate` in teardown), `ReleaseAsset` is no-op, `LoadAssetAsync` throws NotSupportedException (sync tests only).
  Must NOT do: No Addressables dependency. No real asset files. No PlayMode.
  Parallelization: Wave 4 | Blocked by: 4.1 | Can parallelize with: 4.3
  References: Your AssetManager.cs from W2
  Acceptance criteria: 6 tests pass in EditMode Test Runner.
  QA scenarios: Happy — run→6 passed. Failure → any fail→check LoadAsset/ReleaseAsset logic.
  Evidence: .omo/evidence/task-14-asset-manager-refactor.txt
  Commit: Y | test: add reference counting unit tests

- [x] 15. [W4] Scene group + tick cleanup tests
  What to do: Write 6 additional tests:
  - LoadSceneGroup_TracksProgress: Load 3 assets label="L1"→GetGroupProgress("L1")>0.
  - UnloadSceneGroup_ClearsGroup: Load→Unload→progress==0, count==0.
  - PreloadSceneGroup_Enqueues: Preload→PendingPreloadCount==1.
  - Tick_ProcessesPreload: Preload→Tick→PendingPreloadCount==0.
  - Tick_EvictsExpired: Load+Release+set expiry 0→Tick→record removed.
  - Clear_ResetsAll: Load 2→Clear→count==0, all dicts empty.
  Add `internal int PendingPreloadCount` and `internal void SetAutoUnloadDelay(float s)` to AssetManager for testability.
  Must NOT do: No Unity Time.time dependency — use testable SetAutoUnloadDelay.
  Parallelization: Wave 4 | Blocked by: 4.1 | Can parallelize with: 4.2
  References: Same as 4.2
  Acceptance criteria: 12 total tests pass (4.2 + 4.3).
  QA scenarios: Happy → run all 12 pass. Failure → any fail→investigate.
  Evidence: .omo/evidence/task-15-asset-manager-refactor.txt
  Commit: Y | test: add scene group and tick cleanup tests

## Final verification wave
> Runs in parallel after ALL todos. ALL must APPROVE.

- [x] F1. Plan compliance: Verify every todo completed, all Scope IN files touched, nothing in Scope OUT modified. Evidence: .omo/evidence/F1-asset-manager-refactor.txt
- [x] F2. Code quality: Zero compilation warnings. All public API has XML doc comments. No dead usings. Evidence: .omo/evidence/F2-asset-manager-refactor.txt
- [x] F3. Manual QA: Unity Editor → Test Runner → 12/12 passing. GameWorldDriver.Awake creates AssetManager. Editor→AssetDatabaseOperator. Evidence: .omo/evidence/F3-asset-manager-refactor.txt
- [x] F4. Scope fidelity: grep `UnityEngine` in Core assembly→zero matches. Core file count−2. Evidence: .omo/evidence/F4-asset-manager-refactor.txt

## Commit strategy
- One commit per todo (Y in commit field)
- Format: `<type>(<scope>): <description>`
- Push after all W4 tests pass + F1-F4 verified

## Success criteria
1. Core assembly has zero `using UnityEngine;` or Unity type references
2. AssetManager follows ITickable + GameWorld pattern matching ObjectPoolManager
3. 12 NUnit tests pass in EditMode
4. Editor→AssetDatabaseOperator, Runtime→AddressablesOperator
5. Backward compat: `world.AssetOperator` still compiles
6. All operator Clear() removed, IAssetOperator no longer extends IReference
