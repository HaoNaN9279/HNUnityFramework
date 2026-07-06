# mvc-review-fixes - Work Plan

## TL;DR (For humans)
<!-- Fill this LAST, after the detailed plan below is written, so it summarizes the REAL plan. -->
<!-- Plain English for a non-engineer: NO file paths, NO todo numbers, NO wave/agent/tool names. -->

**What you'll get:** MVC 框架的 9 项代码审查问题全部修复：GameWorld 初始化 Bug 修正、Controller/Model 增加生命周期钩子、新增数据绑定接口、代码风格统一、8 个单元测试、文档全量同步。

**Why this approach:** 所有修复按依赖关系分组为三波并行执行。数据绑定采用 IReadOnlyModel + PropertyBinder 方案，避免 ModelManager 带来的全局状态问题——Model 数据私有，View 通过只读通道订阅变化。

**What it will NOT do:** 不创建 ModelManager，不实现具体 UI 控件绑定，不修改框架公共接口契约，不引入新依赖。

**Effort:** Medium
**Risk:** Low — 所有变更是框架内部重构，不影响上游仓库；Metis 审查已通过，关键问题已修复入计划
**Decisions to sanity-check:** IReadOnlyModel 放在 Core 层是否合适（纯 C#，无 Unity 依赖）；命名统一为 plain camelCase（AGENTS.md），ControllerManager 的 `m_controllers` → `controllers`，Controller/Model 已正确无需修改（因为它们由构造函数内部创建）

Your next move: 审批通过后执行（`$start-work`），或者先运行高精度 Momus 双审。完整执行详情见下。

---

> TL;DR (machine): Medium effort, Low risk — fix P0 Initialize bug + add IReadOnlyModel/PropertyBinder + refactor Controller/Model hooks/naming + write tests + sync docs

## Scope
### Must have
- GameWorld.Initialize() 调用 Initialize() 而非 Tick()
- IReadOnlyModel<T> 接口 + ReadOnlyModel<T> 实现
- PropertyBinder.Bind<T>() 绑定 API
- Controller/Model 的 OnInitialize/OnClear 虚方法钩子
- Controller/Model 中 foreach → for 统一
- ControllerManager 中 List → PooledList，`m_controllers` → `controllers`（AGENTS.md camelCase）
- code-standard.md 命名规范与 AGENTS.md 对齐（去掉 `m_` 前缀）
- MVC 单元测试 8 个用例全部通过
- docs-site 和架构文档同步

### Must NOT have (guardrails, anti-slop, scope boundaries)
- 不创建 ModelManager
- 不实现具体 Unity UI 绑定
- 不修改 ITickable/IReference 接口
- 不创建新程序集
- 不修改 asmdef 文件（现有依赖已满足）

## Verification strategy
> Zero human intervention - all verification is agent-executed.
- Test decision: tests-after + NUnit (Unity Test Framework EditMode)
- Framework: NUnit via `vibe_unityMCP_run_tests(mode="EditMode")`
- Evidence: .omo/evidence/task-<N>-mvc-review-fixes.md

## Execution strategy
### Parallel execution waves
- Wave 1 (6 tasks parallel): All framework fixes — GameWorld fix, IReadOnlyModel, Controller refactor, Model refactor, ControllerManager refactor, PropertyBinder
- Wave 2 (1 task): MVC unit tests — depends on all Wave 1
- Wave 3 (2 tasks parallel): Documentation sync — depends on Wave 1 (can run parallel with Wave 2)

### Dependency matrix
| Todo | Depends on | Blocks | Can parallelize with |
| --- | --- | --- | --- |
| 1 | none | 7 | 2,3,4,5,6 |
| 2 | none | 6,7 | 1,3,4,5 |
| 3 | none | 7 | 1,2,4,5,6 |
| 4 | none | 7 | 1,2,3,5,6 |
| 5 | none | 7 | 1,2,3,4,6 |
| 6 | 2 | 7 | 1,3,4,5 |
| 7 | 1,2,3,4,5,6 | none | — |
| 8 | 2,3,4,6 | none | 9 |
| 9 | 1,2,6 | none | 8 |

## Todos
> Implementation + Test = ONE todo. Never separate.
<!-- APPEND TASK BATCHES BELOW THIS LINE WITH edit/apply_patch - never rewrite the headers above. -->

### Wave 1 — Framework Fixes (all parallel, no cross-dependencies)

- [x] 1. Fix GameWorld.Initialize() — call Initialize() not Tick()
  What to do: In `GameWorld.Initialize()`, replace `ProcedureManager.Tick()` with `ProcedureManager.Initialize()` and `ControllerManager.Tick()` with `ControllerManager.Initialize()`.
  Must NOT do: Do NOT change GameWorld.Tick() or GameWorld.LateTick() — they are correct. Do NOT add Initialize() calls that don't exist on those managers (ProcedureManager has Initialize(), ControllerManager has Initialize()).
  Parallelization: Wave 1 | Blocked by: none | Blocks: 7
  References:
    - `Runtime/HN.Framework.Core/Driver/GameWorld/GameWorld.cs:28-32` — the buggy code
    - `Runtime/HN.Framework.Core/Level/Logic/MVC/ControllerManager.cs:50-56` — ControllerManager.Initialize()
    - `Runtime/HN.Framework.Core/Capability/Procedure/ProcedureManager.cs:90-93` — ProcedureManager.Initialize()
    - `Runtime/HN.Framework.Unity/Driver/Platform/GameWorldDriver.cs:29` — caller: `World.Initialize()`
  Acceptance criteria:
    1. `GameWorld.Initialize()` calls `ProcedureManager.Initialize()` NOT `ProcedureManager.Tick()`
    2. `GameWorld.Initialize()` calls `ControllerManager.Initialize()` NOT `ControllerManager.Tick()`
    3. `GameWorld.Tick()` still calls `ProcedureManager.Tick()` and `ControllerManager.Tick()` (unchanged)
    4. `GameWorld.LateTick()` still calls `ProcedureManager.LateTick()` and `ControllerManager.LateTick()` (unchanged)
  QA scenarios:
    - Happy: Create mock Controller subclass that tracks Initialize() calls. Register with world.ControllerManager, call world.Initialize(), assert mock.InitializeWasCalled is true (verifies ControllerManager.Initialize() was called via GameWorld.Initialize())
    - Failure (pre-fix): Before the fix, creating a GameWorld and calling Initialize() would NOT trigger ControllerManager.Initialize() — controllers registered before Initialize() would never have their Initialize() called. After fix, they will.
    - Note: ProcedureManager.Initialize() is currently a no-op (empty method). The fix changes behavior correctly — during init we call Initialize() (setup hook), not Tick() (per-frame update). Verification for ProcedureManager is indirect via behavioral tests.
    - Evidence: `.omo/evidence/task-1-mvc-review-fixes.md`
  Commit: Y | `fix(core): GameWorld.Initialize calls Initialize() instead of Tick()`

- [x] 2. Create IReadOnlyModel<T> interface
  What to do: Create new file `Runtime/HN.Framework.Core/Level/Logic/MVC/IReadOnlyModel.cs` with:
    - `IReadOnlyModel<T>` interface: property `T Value { get; }` + event `Action<T> OnValueChanged`
    - `ReadOnlyModel<T>` sealed class implementing `IReadOnlyModel<T>` with backing field, setter `SetValue(T)`, and event invocation
    - Both types with full XML doc comments
  Must NOT do: Do NOT put this in Unity layer. Do NOT make Model itself implement IReadOnlyModel — this is a separate wrapper. Do NOT add any Unity dependencies.
  Parallelization: Wave 1 | Blocked by: none | Blocks: 6, 7
  References:
    - `Runtime/HN.Framework.Core/Level/Logic/MVC/` — target directory
    - `Runtime/HN.Framework.Core/HN.Framework.Core.asmdef:3` — `noEngineReferences: true`
  Acceptance criteria:
    1. `IReadOnlyModel<T>` interface exists with `Value` property and `OnValueChanged` event
    2. `ReadOnlyModel<T>` sealed class exists, implements `IReadOnlyModel<T>`
    3. `ReadOnlyModel<T>.SetValue(T)` triggers `OnValueChanged` only when value actually changes
    4. Full XML doc comments on all public members
    5. Compilation passes (no assembly reference issues — pure C#)
  QA scenarios:
    - Happy: Create ReadOnlyModel<int>(42), read Value=42, SetValue(100), OnValueChanged fires with new value
    - Failure: SetValue(42) when current is 42 → OnValueChanged does NOT fire (no unnecessary notifications)
    - Evidence: `.omo/evidence/task-2-mvc-review-fixes.md`
  Commit: Y | `feat(core): add IReadOnlyModel<T> for read-only data binding`

- [x] 3. Refactor Controller.cs — virtual hooks + foreach→for
  What to do: In `Controller.cs`:
    1. Add `protected virtual void OnInitialize()` and `protected virtual void OnClear()` — call OnInitialize() at end of Initialize(), call OnClear() at start of Clear()
    2. Change foreach to for in Initialize() and OnFirstFrame() (match Tick/LateTick pattern)
    3. NOTE: `controllerUnits` field naming is already correct camelCase per AGENTS.md — do NOT rename
  Must NOT do: Do NOT change the public API surface (IController, AddUnit, RemoveUnit, property names). Do NOT rename `controllerUnits`.
  Parallelization: Wave 1 | Blocked by: none | Blocks: 7
  References:
    - `Runtime/HN.Framework.Core/Level/Logic/MVC/Controller.cs` — full file
    - `docs-site~/docs/dev/code-standard.md:33` — `m_` prefix rule
    - `Runtime/HN.Framework.Core/Driver/Common/Pool/ReferencePool/PooledCollections.cs:10` — PooledList<T> constraint
  Acceptance criteria:
    1. `OnInitialize()` is `protected virtual`, called at end of `Initialize()`
    2. `OnClear()` is `protected virtual`, called at start of `Clear()`
    3. Initialize() and OnFirstFrame() use `for` loops (not `foreach`)
    4. Field `controllerUnits` remains unchanged (already camelCase per AGENTS.md)
    5. All XML doc comments present
  QA scenarios:
    - Happy: Subclass overrides OnInitialize, registers with ControllerManager.Initialize(), OnInitialize fires after units are initialized
    - Failure: Subclass overrides OnClear, removes controller, OnClear fires before units are cleared
    - Evidence: `.omo/evidence/task-3-mvc-review-fixes.md`
  Commit: Y | `refactor(core): add virtual hooks and unify naming in Controller`

- [x] 4. Refactor Model.cs — virtual hooks + foreach→for
  What to do: In `Model.cs`:
    1. Add `protected virtual void OnInitialize()` and `protected virtual void OnClear()` — same pattern as Controller
    2. Change foreach to for in Initialize() and OnFirstFrame()
    3. NOTE: `modelUnits` field naming is already correct camelCase per AGENTS.md — do NOT rename
  Must NOT do: Same constraints as task 3.
  Parallelization: Wave 1 | Blocked by: none | Blocks: 7
  References:
    - `Runtime/HN.Framework.Core/Level/Logic/MVC/Model.cs` — full file
    - Task 3 for pattern consistency
  Acceptance criteria: Same pattern as task 3 but for Model.
  QA scenarios: Same pattern as task 3 but for Model subclasses.
  Evidence: `.omo/evidence/task-4-mvc-review-fixes.md`
  Commit: Y | `refactor(core): add virtual hooks in Model`

- [x] 5. Refactor ControllerManager.cs — PooledList + naming
  What to do: In `ControllerManager.cs`:
    1. Replace `private List<Controller> m_controllers = new List<Controller>()` with `private PooledList<Controller> controllers = ReferencePool.Acquire<PooledList<Controller>>()`
    2. Rename all references: `m_controllers` → `controllers` (plain camelCase per AGENTS.md)
    3. In `Uninitialize()`: after clearing controllers, release via `ReferencePool.Release(controllers)` then immediately re-acquire fresh: `controllers = ReferencePool.Acquire<PooledList<Controller>>()`
    4. NOTE: The `using HN.Framework.Core.Driver.Common.Pool.ReferencePool;` import already exists — verify, do not re-add
    5. NOTE: Do NOT change foreach loops in Initialize/OnFirstFrame/Uninitialize — one-shot lifecycle methods
  Must NOT do: Do NOT change RegisterController/UnregisterController logic. Do NOT change Tick/LateTick. Do NOT leave controllers as a dangling reference after Uninitialize().
  Parallelization: Wave 1 | Blocked by: none | Blocks: 7
  References:
    - `Runtime/HN.Framework.Core/Level/Logic/MVC/ControllerManager.cs` — full file
    - `Runtime/HN.Framework.Core/Driver/Common/Pool/ReferencePool/PooledCollections.cs:10` — PooledList<T> where T : class
    - `Runtime/HN.Framework.Core/Driver/Common/Pool/ReferencePool/ReferencePool.cs:19` — Acquire<T>
  Acceptance criteria:
    1. `m_controllers` is `PooledList<Controller>` initialized via `ReferencePool.Acquire`
    2. `Uninitialize()` releases the list back to ReferencePool after clearing controllers
    3. RegisterController and UnregisterController still work correctly with PooledList (List API compatible)
    4. Controller access via `IReadOnlyList<Controller>` still works
  QA scenarios:
    - Happy: Register 3 controllers, verify they appear in Controllers list, Uninitialize releases list to pool
    - Failure: Unregister a controller that was never registered → throws InvalidOperationException (existing behavior preserved)
    - Evidence: `.omo/evidence/task-5-mvc-review-fixes.md`
  Commit: Y | `refactor(core): use PooledList in ControllerManager`

- [x] 6. Implement PropertyBinder binding API
  What to do: In `Runtime/HN.Framework.Unity/Level/View/Binding/PropertyBinder.cs`:
    1. Add `using HN.Framework.Core.Level.Logic;` import
    2. Add abstract method: `public abstract void Bind<T>(IReadOnlyModel<T> source, Action<T> onValueChanged);`
    3. Add `public abstract void UnbindAll();` for cleanup
    4. Add XML doc comments
  Also update `Runtime/HN.Framework.Unity/Level/View/ViewFactory.cs`:
    1. Add virtual method: `public virtual EntityView CreateView(string prefabAddress) { return null; }` as placeholder
  Also update `Runtime/HN.Framework.Unity/Level/View/EntityView.cs`:
    1. Add `protected PropertyBinder binder;` field
    2. Add `public virtual void BindData(PropertyBinder binder) { this.binder = binder; }`
  Must NOT do: Do NOT add concrete Unity UI bindings (Text, Image, etc.) — that's game-specific. Do NOT add Unity dependencies to Core layer.
  Parallelization: Wave 1 | Blocked by: 2 | Blocks: 7
  References:
    - `Runtime/HN.Framework.Unity/Level/View/Binding/PropertyBinder.cs` — current empty stub
    - `Runtime/HN.Framework.Unity/Level/View/ViewFactory.cs` — current empty stub
    - `Runtime/HN.Framework.Unity/Level/View/EntityView.cs` — current empty stub
    - `Runtime/HN.Framework.Unity/HN.Framework.Unity.asmdef:5` — references HN.Framework.Core
    - Task 2 for IReadOnlyModel<T>
  Acceptance criteria:
    1. PropertyBinder has `Bind<T>` and `UnbindAll` abstract methods with XML docs
    2. ViewFactory has `CreateView(string)` virtual method
    3. EntityView has `BindData(PropertyBinder)` method
    4. All files compile (Unity layer references Core layer for IReadOnlyModel<T>)
  QA scenarios:
    - Happy: Create mock binder subclass, call Bind<int>(readOnlyModel, callback), verify type-safe binding contract
    - Failure: Call UnbindAll after multiple Bind calls, verify cleanup
    - Evidence: `.omo/evidence/task-6-mvc-review-fixes.md`
  Commit: Y | `feat(unity): implement PropertyBinder binding API and View base classes`

### Wave 2 — Tests (depends on all Wave 1)

- [x] 7. Write MVC unit tests
  What to do: Create `Tests/HN.Framework.Unity.Tests/MVCTests.cs` with TestFixture covering:
    1. **ControllerManager lifecycle**: Register controller → Initialize → Tick → Unregister → verify OnInitialize/OnClear hooks called
    2. **GameWorld Initialize fix**: Create GameWorld, register mock controller via ControllerManager, call World.Initialize(), assert controller.Initialize() was called (indirect verification that ControllerManager.Initialize() runs)
    3. **IReadOnlyModel<T>**: Create, SetValue, verify OnValueChanged fires; SetValue same value, verify no redundant fire
    4. **Model lifecycle**: Create Model subclass with Unit, verify Initialize → Tick → Clear flow
    5. **ControllerUnit pool integration**: AddUnit, RemoveUnit, verify unit returned to ReferencePool (re-acquire and check reference equality)
    6. **ControllerManager PooledList lifecycle**: Register 3 controllers, Uninitialize, re-register, verify Controllers list works after Uninitialize (fresh PooledList after re-acquire)
    7. **PropertyBinder contract**: Create mock Binder subclass, test Bind/UnbindAll
    8. **Controller virtual hooks order**: Subclass overrides OnInitialize, verifies it fires AFTER units.Initialize() but BEFORE Tick
  Must NOT do: Do NOT create new test assembly. Do NOT write tests that require Unity PlayMode. Do NOT write runtime tests for foreach→for (that's static analysis — verified by code review in F2). Use `[SetUp]` with `ReferencePool.ClearAll()` to prevent cross-test pool contamination (follow PoolTests.cs pattern).
  Parallelization: Wave 2 | Blocked by: 1, 2, 3, 4, 5, 6 | Blocks: none
  References:
    - `Tests/HN.Framework.Unity.Tests/ProcedureTests.cs` — test pattern reference
    - `Tests/HN.Framework.Unity.Tests/PoolTests.cs` — test pattern for ReferencePool
    - `Tests/HN.Framework.Unity.Tests/HN.Framework.Unity.Tests.asmdef` — already references Core
    - All refactored files from tasks 1-6
  Acceptance criteria:
    1. All 8 test cases compile and pass
    2. Test coverage: ControllerManager lifecycle, GameWorld.Initialize, IReadOnlyModel, Model lifecycle, ReferencePool integration, PropertyBinder
    3. Test naming follows existing pattern (`[TestFixture]`, `[Test]`, `[SetUp]`)
    4. Run tests via `vibe_unityMCP_run_tests` with mode=EditMode, all pass
  QA scenarios:
    - Happy: Run all MVCTests, 8/8 pass
    - Failure: Before P0 fix, ControllerManager.Initialize not called → test reveals bug
    - Evidence: `.omo/evidence/task-7-mvc-review-fixes.md`
  Commit: Y | `test: add MVC unit tests for lifecycle, binding, and GameWorld fix`

### Wave 3 — Documentation Sync (depends on Wave 1, can parallelize with Wave 2)

- [x] 8. Update MVC guide and View docs
  What to do: Update `docs-site~/docs/guide/mvc.md`:
    1. Add IReadOnlyModel<T> section after "核心类型" table
    2. Update lifecycle flow to include OnInitialize/OnClear hooks
    3. Add code example showing Controller exposing IReadOnlyModel property
    4. Add code example showing PropertyBinder.Bind usage from View
    5. Add note: "ModelManager 不需要 — 通过 IReadOnlyModel + PropertyBinder 实现数据绑定"
    6. Fix sample code: Controller no longer needs to override Initialize/OnFirstFrame/Tick/LateTick/Clear to delegate to Model — delegation is automatic via units; but if Controller holds Model separately, OnInitialize can initialize it
  Update `docs-site~/docs/guide/view-factory.md`:
    1. Remove 🚧 Stub from description of PropertyBinder
    2. Add Bind<T> API to PropertyBinder row in table
    3. Update status note: PropertyBinder 绑定 API 已实现
  Must NOT do: Do NOT remove existing content — only add/update. Do NOT change sidebar_position.
  Parallelization: Wave 3 | Blocked by: 2, 3, 4, 6 | Blocks: none
  References:
    - `docs-site~/docs/guide/mvc.md` — full file (186 lines)
    - `docs-site~/docs/guide/view-factory.md` — full file (25 lines)
    - Tasks 2, 3, 4, 6 for accurate API descriptions
  Acceptance criteria:
    1. mvc.md has IReadOnlyModel section with code example
    2. mvc.md lifecycle flow updated with OnInitialize/OnClear
    3. mvc.md has PropertyBinder binding example
    4. view-factory.md PropertyBinder row shows Bind<T> API
    5. view-factory.md no longer says 🚧 Stub for PropertyBinder
  QA scenarios: Read updated files, verify all new content is accurate and consistent with actual API
  Evidence: `.omo/evidence/task-8-mvc-review-fixes.md`
  Commit: Y | `docs: update MVC guide with IReadOnlyModel and PropertyBinder binding`

- [x] 9. Update architecture and supporting docs
  What to do: Update all remaining docs:
    1. `docs-site~/docs/guide/quick-start.md`: Update GameWorld lifecycle flow — `GameWorld.Initialize()` now calls `ControllerManager.Initialize()` + `ProcedureManager.Initialize()` (not `Tick()`). Update the flow diagram on lines 40-47.
    2. `docs-site~/docs/api/index.md`: Add `IReadOnlyModel<T>` and `ReadOnlyModel<T>` under Core.Level.Logic section. Update PropertyBinder description from 🚧 to "属性绑定抽象类，提供 Bind/UnbindAll 方法".
    3. `docs-site~/docs/dev/code-standard.md`: Remove `m_` prefix from 私有字段 convention (line 33-34). The AGENTS.md says "私有字段: camelCase" without `m_` prefix. Update lines 33-34: remove "实例私有字段 | camelCase + `m_` 前缀" → change to "实例私有字段 | camelCase". Also remove `s_` static prefix rule if not used by existing code.
    4. `架构~/最终架构.md`: Update line 270-275 MVC directory listing to include `IReadOnlyModel.cs`. Update line 356 PropertyBinder from 🚧 Stub to ✅ (binding API implemented). Update GameWorld.Initialize() description (line 463) if it mentions Tick(). Update Appendix B (line 642) — remove ⚠️ annotation for MVC namespace (already correct in code). 
    5. `docs-site~/docs/dev/architecture.md`: Update module status table line 218 — MVC status stays ✅. Update line 221 View layer — PropertyBinder status from 🚧 to partial ✅.
  Must NOT do: Do NOT change the overall doc structure. Do NOT add new pages.
  Parallelization: Wave 3 | Blocked by: 1, 2, 6 | Blocks: none
  References:
    - `docs-site~/docs/guide/quick-start.md:40-53` — lifecycle flow
    - `docs-site~/docs/api/index.md:42` — MVC API listing
    - `docs-site~/docs/dev/code-standard.md:33` — naming rules
    - `架构~/最终架构.md:270-275` — MVC dir listing, `:355-356` — PropertyBinder
    - `docs-site~/docs/dev/architecture.md:218-221` — module status
    - Tasks 1, 2, 6 for accurate descriptions
  Acceptance criteria:
    1. quick-start.md GameWorld flow shows correct Initialize behavior
    2. api/index.md lists IReadOnlyModel<T> and ReadOnlyModel<T>
    3. 架构~/最终架构.md lists IReadOnlyModel.cs in MVC directory
    4. 架构~/最终架构.md PropertyBinder status updated from 🚧
    5. docs-site architecture.md View module status updated for PropertyBinder
  QA scenarios: Read each updated file, verify no contradictions with actual code
  Evidence: `.omo/evidence/task-9-mvc-review-fixes.md`
  Commit: Y | `docs: sync architecture and guide docs with MVC review fixes`

## Final verification wave
> Runs in parallel after ALL todos. ALL must APPROVE. Surface results and wait for the user's explicit okay before declaring complete.
- [x] F1. Plan compliance audit: Verify all 9 todos completed, each acceptance criterion met. Run `git diff --stat` to confirm all expected files touched. Check no unexpected files modified.
- [x] F2. Code quality review: Run `ast-grep` to verify no `foreach` remains in Controller/Model Initialize/OnFirstFrame. Verify `m_` prefix consistency across all changed files. Verify all new public APIs have XML doc comments.
- [x] F3. Real manual QA: Run `vibe_unityMCP_run_tests(mode="EditMode", test_names=["MVCTests"])` — ALL 8 tests must pass. Run existing test suites to verify no regressions.
- [x] F4. Scope fidelity: Diff against "Must NOT have" checklist — verify no ModelManager was created, no new asmdef, no Unity UI bindings, no ITickable/IReference changes.

## Commit strategy
- All changes on a single feature branch: `refactor/mvc-review-fixes`
- Commit per task: 9 atomic commits, each self-contained and compilable
- Merge via `rebase and merge` to main (keep history clean)
- If any task fails: `git revert <commit-hash>` for that task only
- Test commit (task 7) is the final code commit; docs commits (8, 9) follow

## Success criteria
