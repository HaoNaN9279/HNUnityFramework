# event-bus-module - Work Plan

## TL;DR (For humans)

**What you'll get:** 一个完整的事件总线模块，允许任何模块通过订阅/发布事件实现零耦合通信，所有事件处理都在纯 C# 层完成，无需 Unity 依赖。

**Why this approach:** EventBus 实例由 GameWorld 持有（跟随生命周期，消除静态全局状态），使用锁保护订阅操作 + 快照机制保证线程安全发布。每个 Publish 调用对 handler 列表做一次快照拷贝以避免死锁（调用用户代码时不在锁内），这个 GC 开销是可接受的，因为事件总线面向低频业务事件而非逐帧高频触发。

**What it will NOT do:** 不做 Unity 层事件类型定义，不做 DebugHub 集成（留给 C2），不做事件对象池化（调用者负责），不做编辑器调试面板。

**Effort:** Short（~2h，5个文件变更）
**Risk:** Low — 纯 C# 代码，无外部依赖，完全可隔离测试
**Decisions to sanity-check:** Publish 异常策略（catch+log+continue），HasSubscribers<T> 是否必要，重复订阅是否允许

Your next move: approve to begin execution (`$start-work`), or choose to run a high-accuracy dual Momus review first. Full execution detail follows below.

---

> TL;DR (machine): Short / Low / 5 files (3 new + 2 modified) + 17 unit tests + 3 docs pages

## Scope
### Must have
- `IEventBus` 接口定义于 `HN.Framework.Core.Capability.Event`，4 个方法：`Subscribe<T>`, `Unsubscribe<T>`, `Publish<T>`, `HasSubscribers<T>`
- `EventBus` 实现于同一命名空间，使用 lock+snapshot 线程安全模式
- GameWorld 构造函数中创建 EventBus，暴露为 `public EventBus EventBus { get; }`
- 所有公共 API 有完整 XML 文档注释
- 17 个单元测试（TDD 顺序），覆盖订阅/发布/取消/类型隔离/线程安全/异常处理/空值守卫
- docs-site 三处更新：API index 补充条目、capability 指南标 ✅、architecture 状态表更新

### Must NOT have (guardrails, anti-slop, scope boundaries)
- 不定义任何 Unity 层事件类型或 UnityEngine 引用
- 不修改 GameWorldDriver（EventBus 纯 C#，无需平台注入）
- 不做 DebugHub 集成（即使 EventBus 暴露了 `HasSubscribers<T>` 以备未来使用）
- 不做事件对象池化基础设施
- 不做匿名委托检测或运行时警告
- 不引入不可变集合或第三方依赖
- 不在 Publish 回调中对 Subscribe/Unsubscribe 做运行时拦截

## Verification strategy
> Zero human intervention - all verification is agent-executed.
- Test decision: TDD + NUnit (EditMode)
- Framework: NUnit via Unity Test Framework, assembly `HN.Framework.Core.Tests`
- Evidence: `.omo/evidence/task-<N>-event-bus-module.log` (Unity console output from `run_tests`)

## Execution strategy
### Parallel execution waves
- **Wave 1** (TDD): IEventBus interface → EventBusTests (17 cases) → EventBus implementation → GameWorld integration → run tests
- **Wave 2** (Docs): docs-site updates (3 files, all independent → parallel)

### Dependency matrix
| Todo | Depends on | Blocks | Can parallelize with |
| --- | --- | --- | --- |
| 1. IEventBus interface | — | 2, 3 | — |
| 2. EventBusTests | 1 | 3, 4 | — |
| 3. EventBus implementation | 1, 2 | 4, 6 | — |
| 4. GameWorld integration | 1, 3 | 6 | — |
| 5. docs-site updates | — | — | (parallel with Wave 1) |
| 6. Final verification wave | ALL | — | — |

## Todos
> Implementation + Test = ONE todo. Never separate.
<!-- APPEND TASK BATCHES BELOW THIS LINE WITH edit/apply_patch - never rewrite the headers above. -->
- [x] 1. Create IEventBus interface
  What to do / Must NOT do: Create file `Runtime/HN.Framework.Core/Capability/Event/IEventBus.cs` with namespace `HN.Framework.Core.Capability.Event`, define 4 methods: `void Subscribe<T>(Action<T> handler)`, `void Unsubscribe<T>(Action<T> handler)`, `void Publish<T>(T eventData)`, `bool HasSubscribers<T>()`. All methods have XML doc comments. NO implementation — interface only. Must NOT add any other methods, properties, or events — the API surface is exactly these 4.
  Parallelization: Wave 1 | Blocked by: — | Blocks: Todos 2, 3, 4
  References (executor has NO interview context - be exhaustive): 架构~/最终架构.md:879-884 (IEventBus spec), 架构~/最终架构.md:384 (namespace spec), Debug/IDebugHub.cs (interface pattern — XML doc on every member, `#nullable enable`)
  Acceptance criteria (agent-executable): File exists at correct path, namespace is `HN.Framework.Core.Capability.Event`, contains exactly 4 method declarations with XML doc comments, no implementation.
  QA scenarios (name the exact tool + invocation): Happy: `glob **/IEventBus.cs` returns file, `read` confirms 4 methods + namespace. Failure: N/A (can't fail if file is correct). Evidence .omo/evidence/task-1-event-bus-module.log
  Commit: Y | `feat(Core.Event): add IEventBus interface`

- [x] 2. Write EventBusTests (TDD — write tests BEFORE implementation)
  What to do / Must NOT do: Create file `Tests/HN.Framework.Core.Tests/Event/EventBusTests.cs` with namespace `HN.Framework.Core.Tests.Event`. Write all 17 test cases using NUnit `[TestFixture]`, `[SetUp]`, `[TearDown]`, `[Test]` pattern. Use inline test doubles (no external test class files). Test event types must be `readonly struct` (matching architecture requirement). Must NOT reference any Unity types. Must NOT use lambda subscriptions in test handler registration (use instance methods per dev注意事项). Test order: basic → type safety → edge cases → error handling → concurrency.
  Parallelization: Wave 1 | Blocked by: Todo 1 | Blocks: Todo 3
  References (executor has NO interview context - be exhaustive): Tests/HN.Framework.Core.Tests/Debug/DebugHubTests.cs (exact test pattern: TestFixture, SetUp, TearDown, inline test doubles, Assert用法), Tests/HN.Framework.Core.Tests/HN.Framework.Core.Tests.asmdef (asmdef config), .omo/drafts/event-bus-module.md (17 test cases enumerated), 架构~/事件总线核心作用与开发注意事项.md:49-52 (readonly struct requirement)
  
  Test cases to implement (exactly, with these names):
  1. `Subscribe_Publish_HandlerCalled` — subscribe handler, publish event, assert handler received correct data
  2. `Subscribe_Unsubscribe_HandlerNotCalled` — subscribe, then unsubscribe, publish, handler NOT called
  3. `MultipleSubscribers_AllCalled` — 3 handlers subscribe same event, publish once, all 3 called
  4. `Publish_ReadonlyStruct_DataReceived` — publish a readonly struct event, verify field values in handler
  5. `MultipleInstances_IsolatedState` — create 2 EventBus instances, subscribe to each, verify no cross-contamination
  6. `Subscribe_DifferentTypes_TypeIsolation` — subscribe to type A and type B, publish A, only A handler called
  7. `Publish_ZeroSubscribers_NoError` — publish event with no subscribers, no exception
  8. `Subscribe_DuplicateHandler_CalledMultipleTimes` — subscribe same handler twice, publish once, handler called twice
  9. `Subscribe_NullHandler_ThrowsArgNull` — subscribe(null) throws ArgumentNullException
  10. `Unsubscribe_NullHandler_ThrowsArgNull` — unsubscribe(null) throws ArgumentNullException
  11. `Publish_ValueTypeDefault_Allowed` — publish default(int event), handler receives 0
  12. `Publish_HandlerThrows_ContinuesToOtherHandlers` — handler1 normal, handler2 throws, handler3 still called
  13. `Unsubscribe_NotSubscribed_NoError` — unsubscribe a handler that was never subscribed, no exception
  14. `HasSubscribers_WhenSubscribed_ReturnsTrue` — after subscribe, HasSubscribers returns true
  15. `HasSubscribers_WhenUnsubscribed_ReturnsFalse` — after unsubscribe last handler, HasSubscribers returns false
  16. `ThreadSafe_PublishFromBackgroundTask` — create Task that publishes, assert handler called from background context
  17. `Publish_ConcurrentSubscribe_NoException` — on a separate thread, subscribe while main thread publishes, no crash

  Acceptance criteria (agent-executable): All 17 tests compile (they will fail initially — this is TDD, failure is expected at this stage). No compilation errors. Test file has correct namespace and asmdef references.
  QA scenarios (name the exact tool + invocation): Happy: `vibe_unityMCP_run_tests(mode="EditMode", test_names=["HN.Framework.Core.Tests.Event.EventBusTests.*"])` — tests should fail (not crash) because EventBus not yet implemented. Failure: compilation errors → fix and retry. Evidence .omo/evidence/task-2-event-bus-module.log
  Commit: Y | `test(Core.Event): add EventBus unit tests (TDD)`

- [x] 3. Implement EventBus class
  What to do / Must NOT do: Create file `Runtime/HN.Framework.Core/Capability/Event/EventBus.cs` implementing IEventBus. Internal data structure: `Dictionary<Type, List<Delegate>>` + `readonly object _lock`. Subscribe/Unsubscribe protected by `lock(_lock)`. Subscribe: if type key doesn't exist, create new List<Delegate>; add handler delegate; throw ArgumentNullException if handler is null. Unsubscribe: if type key exists, remove handler delegate; throw ArgumentNullException if handler is null; silently no-op if handler not found. Publish: `lock → var snapshot = new List<Delegate>(handlerList); lock release → foreach snapshot invoke handlers`. Each handler invocation wrapped in try-catch; on exception, `System.Diagnostics.Debug.WriteLine` the exception then continue. HasSubscribers: check if type key exists and list is non-empty. Must NOT hold lock during handler invocation (snapshot pattern). Must NOT use ConcurrentDictionary or ImmutableList. Must have XML doc comments on all public members. Must use `#nullable enable`.
  Parallelization: Wave 1 | Blocked by: Todos 1, 2 | Blocks: Todo 4, 6
  References (executor has NO interview context - be exhaustive): .omo/drafts/event-bus-module.md (all decisions), Debug/DebugHub.cs:43-48 (constructor pattern, readonly fields), Capability/Pool/ObjectPoolManager.cs (sealed class pattern), ProcedureManager.cs (exception catch → Debug.WriteLine → continue pattern), 架构~/事件总线核心作用与开发注意事项.md:65-67 (thread safety requirements)
  Acceptance criteria (agent-executable): File exists, implements IEventBus, all 17 tests pass when run via `vibe_unityMCP_run_tests(mode="EditMode", assembly_names=["HN.Framework.Core.Tests"])`. No test failures, no test errors.
  QA scenarios (name the exact tool + invocation): Happy: Unity test runner shows 17/17 passed. Failure: any test fails → fix EventBus implementation and re-run. Evidence .omo/evidence/task-3-event-bus-module.log
  Commit: Y | `feat(Core.Event): implement EventBus with lock+snapshot thread safety`

- [x] 4. Integrate EventBus into GameWorld
  What to do / Must NOT do: Modify `Runtime/HN.Framework.Core/Driver/GameWorld/GameWorld.cs`. Add `using HN.Framework.Core.Capability.Event;`. Add `public EventBus EventBus { get; }` property alongside PoolManager/ProcedureManager/ControllerManager (line ~12). In constructor, add `EventBus = new EventBus();` (line ~24). Must NOT add EventBus to Tick() or LateTick() — it does not implement ITickable. Must NOT change any existing GameWorld behavior.
  Parallelization: Wave 1 | Blocked by: Todos 1, 3 | Blocks: —
  References (executor has NO interview context - be exhaustive): GameWorld.cs:11-13 (exact pattern for built-in modules: `public ObjectPoolManager PoolManager { get; }`), GameWorld.cs:22-26 (constructor pattern), 架构~/事件总线核心作用与开发注意事项.md:33 (EventBus held by GameWorld)
  Acceptance criteria (agent-executable): GameWorld.cs compiles without errors. `world.EventBus` is accessible, non-null after construction. Existing tests (DebugHubTests, MemoryPackSerializerTests) still pass.
  QA scenarios (name the exact tool + invocation): Happy: `vibe_unityMCP_run_tests(mode="EditMode", assembly_names=["HN.Framework.Core.Tests"])` — EventBus tests still pass + no regression in other tests. Failure: compilation error or regression → fix. Evidence .omo/evidence/task-4-event-bus-module.log
  Commit: Y | `feat(Core.Driver): add EventBus to GameWorld`

- [x] 5. Update docs-site documentation
  What to do / Must NOT do: Update 3 files in docs-site~/docs/. All edits are small, focused additions:
  
  **5a: `docs/api/index.md`** — In the "Capability — 能力模块（接口定义）" section, add after `ObjectPoolManager`:
  ```
  - **IEventBus** — 事件总线接口，提供 `Subscribe<T>` / `Unsubscribe<T>` / `Publish<T>` / `HasSubscribers<T>` 方法
  - **EventBus** — 事件总线实现，线程安全，基于锁+快照模式
  ```
  
  **5b: `docs/guide/capability.md`** — Change line 17 from:
  ```
  | `IEventBus` | 🚧 | 事件总线接口。待实现，支持模块间解耦通信 |
  ```
  To:
  ```
  | `IEventBus` | ✅ | 事件总线接口。支持 Subscribe/Unsubscribe/Publish/HasSubscribers，线程安全 |
  ```
  
  **5c: `docs/dev/architecture.md`** — In module status table, change line 218 from:
  ```
  | S6 | IEventBus / EventBus | 🚧 Stub | |
  ```
  To:
  ```
  | S6 | IEventBus / EventBus | ✅ | 线程安全事件总线（lock+snapshot），由 GameWorld 持有 |
  ```
  
  Must NOT create new files. Must NOT restructure existing sections.
  Parallelization: Wave 2 | Blocked by: — (independent of Wave 1) | Blocks: —
  References (executor has NO interview context - be exhaustive): docs-site~/docs/api/index.md:38-47 (Capability section structure), docs-site~/docs/guide/capability.md:17 (IEventBus row), docs-site~/docs/dev/architecture.md:218 (S6 row)
  Acceptance criteria (agent-executable): All 3 files contain the updated text. No broken markdown syntax.
  QA scenarios (name the exact tool + invocation): Happy: `grep` for "IEventBus" and "EventBus" in each file returns the new lines. Failure: N/A. Evidence .omo/evidence/task-5-event-bus-module.log
  Commit: Y | `docs: update docs-site for EventBus module`

## Final verification wave
> Runs in parallel after ALL todos. ALL must APPROVE. Surface results and wait for the user's explicit okay before declaring complete.
- [x] F1. Plan compliance audit — Verify all 5 todos completed, all files exist at correct paths, all tests pass, no scope creep. Evidence: `.omo/evidence/f1-event-bus-module.log`
- [x] F2. Code quality review — Verify XML doc comments on all public API, no Unity dependencies in Core assembly, proper `#nullable enable`, consistent naming with existing codebase. Evidence: `.omo/evidence/f2-event-bus-module.log`
- [x] F3. Real manual QA — Run full EditMode test suite via `vibe_unityMCP_run_tests(mode="EditMode")`, confirm zero failures (including pre-existing tests). Evidence: `.omo/evidence/f3-event-bus-module.log`
- [x] F4. Scope fidelity — Confirm no files outside scope were modified, no Unity-layer event types created, no GameWorldDriver changes. Evidence: `.omo/evidence/f4-event-bus-module.log`

## Commit strategy
- 5 atomic commits, one per todo
- Commit order: 1 → 2 → 3 → 4 → 5 (matches dependency chain)
- Final verification wave commits separately if fixes needed
- Commit messages follow project convention: `type(scope): description`

## Success criteria
- [x] IEventBus interface defined with correct namespace and 4 methods
- [x] EventBus implemented with lock+snapshot thread safety
- [x] 17/17 unit tests passing
- [x] GameWorld exposes EventBus as instance property
- [x] No regression in existing test suites
- [x] docs-site updated with EventBus entries
- [x] All XML doc comments present on public API
