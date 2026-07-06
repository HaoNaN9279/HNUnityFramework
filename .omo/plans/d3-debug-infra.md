# d3-debug-infra - Work Plan

## TL;DR (For humans)
<!-- Fill this LAST, after the detailed plan below is written -->

**What you'll get:** 框架的结构化日志基础和调试中枢 — LogLevel 分级、模块级日志通道开关、IDebugHub 命令注册系统，以及 ILogProvider 从"纯字符串日志"升级为支持分级/通道/上下文的完整日志接口。

**Why this approach:** 这是架构文档定义的最高优先级（P0）阻塞项。D3 完成后，C2 Debug 系统、E3 Debug Hub Editor、C5 完整日志系统才有地基。纯 C# 数据模型零外部依赖，风险极低。

**What it will NOT do:** 不包含运行时 UGUI 调试控制台（C2）、不包含 Editor 端的 LogViewer/DebugHubWindow（E3）、不包含调试命令的注册表自动发现机制 — 这些是 C2 的职责。

**Effort:** Quick（1-2 天）
**Risk:** Low — 纯数据模型 + 接口扩展，零外部依赖，不改变现有行为
**Decisions to sanity-check:** DebugHub 与 ILogProvider 职责完全分离 — DebugHub 只存储+事件，不做输出；ILogProvider 是唯一输出通道。LogLevel 在 Driver/D3 层，被 Capability/C5 的 ILogProvider 引用（这是正确的架构依赖方向）。ILogProvider 保留 `Log(string)` 向后兼容。

Your next move: 批准后即可 `$start-work` 开始执行。完整执行细节见下。

---

> TL;DR (machine): Quick, Low risk, 7 deliverables incl docs, 7 todos TDD

## Design decisions (architecture clarifications)
> From Metis gap analysis — resolving F1-F5.

**D1. DebugHub 与 ILogProvider 职责分离（关键）**
- **DebugHub**: 纯基础设施层 — 通道/命令注册表 + 环形日志缓冲（最多 100 条）+ `OnLog` 事件。其 `Log()` 方法**仅存储 LogEntry + 触发事件**，**不做输出**到 Unity Debug。
- **ILogProvider**: 唯一日志输出通道 — 负责将日志路由到实际输出（UnityEngine.Debug / 文件 / 网络）。
- **关系**: 两者**无耦合**。模块通过 `world.LogProvider.Log()` 输出日志；通过 `world.DebugHub.Log()` 写入结构化调试记录（供未来 C2 控制台消费 `OnLog` 事件）。
- **为什么**: 架构文档 §三.3.1 依赖规则：CapabilityModule → DriverLayer 单向依赖。LogLevel 在 Driver/D3 层，ILogProvider 在 Capability/C5 层引用 Driver 类型是正确的依赖方向。

**D2. Nullable 引用类型策略**
- 所有**新建** Core 文件启用 `#nullable enable`（.NET Standard 2.1 + C# 8 支持）
- `object? Context` 使用可空注解
- 已有文件（ILogProvider.cs, GameWorld.cs）暂不修改 nullable 上下文（与现有代码风格一致），新方法签名中 `object? context = null` 用 `#nullable enable` 包围或用注释标注

**D3. LogEntry 相等性**
- 使用默认 readonly struct 的值相等性（逐字段比较）
- `object Context` 按引用比较（两个不同日志事件即使 Context 值相同也不应被视为同一事件）
- 不实现 `IEquatable<LogEntry>`（当前阶段不需要字典 key 用途）

**D4. UnityLogProvider 测试策略**
- 使用 `LogAssert.Expect(LogType, string)` 验证正确的 Debug.Log 调用
- 示例: `LogAssert.Expect(LogType.Error, "[Error][test] msg"); provider.Log(LogLevel.Error, "test", "msg");`
### Must have
- [x] `LogLevel` enum（Debug/Info/Warning/Error/Fatal）
- [x] `ILogChannel` 接口（Name, Enabled）
- [x] `LogEntry` readonly struct（Timestamp, Channel, Level, Message, Context）
- [x] `IDebugHub` 接口（RegisterChannel, RegisterCommand, Log）
- [x] `IDebugCommand` 接口（Name, Description, Execute）
- [x] `DebugHub` 基础实现类（Dictionary 存储，无线程安全要求）
- [x] `ILogProvider` 接口扩展（保留 Log(string)，新增结构化 Log 方法）
- [x] `UnityLogProvider` 适配新 ILogProvider
- [x] `GameWorld` 持有 DebugHub 实例
- [x] 所有新增/修改代码的 TDD 单元测试
- [x] 完整 XML 文档注释
- [x] 文档站更新（api/index.md + guide/core.md + dev/architecture.md）

### Must NOT have (guardrails, anti-slop, scope boundaries)
- [ ] ❌ C2 Debug 系统（DebugModule、命令注册表、自动发现、RuntimeDebugConsole）
- [ ] ❌ E3 Debug Hub Editor（LogViewer、DebugHubWindow、RuntimeStateInspector）
- [ ] ❌ UnityLogProvider 的彩色格式化输出 — 仅基础 `[Level][Channel] message` 前缀
- [ ] ❌ 任何 Editor 程序集修改
- [ ] ❌ GameWorldDriver.cs 修改 — 集成仅在 GameWorld 层
- [ ] ❌ 线程安全（当前阶段不需要）
- [ ] ❌ 日志持久化/文件输出
- [ ] ❌ MemoryPack 依赖

## Verification strategy
> Zero human intervention - all verification is agent-executed.
- Test decision: TDD + NUnit (Unity Test Framework)
- Evidence: .omo/evidence/task-<N>-d3-debug-infra.log
- Full suite: `vibe_unityMCP_run_tests` EditMode → 全部通过

## Execution strategy
### Parallel execution waves
- **Wave 1**: Todo 1 — D3 数据模型（基础层，所有其他 todo 的依赖）
- **Wave 2**: Todo 2 + Todo 3（并行）— DebugHub 实现 + ILogProvider 扩展
- **Wave 3**: Todo 4 — UnityLogProvider 更新（依赖 Todo 3）
- **Wave 4**: Todo 5 — GameWorld 集成（依赖 Todo 2 + Todo 4）
- **Wave 5**: Todo 6 — 全量测试验证
- **Wave 6**: Todo 7 — 文档站更新（依赖 Todo 6）

### Dependency matrix
| Todo | Depends on | Blocks | Can parallelize with |
| --- | --- | --- | --- |
| 1 | — | 2, 3, 4, 5 | — |
| 2 | 1 | 5 | 3 |
| 3 | 1 | 4 | 2 |
| 4 | 3 | 5 | — |
| 5 | 2, 4 | 6 | — |
| 6 | 1, 2, 3, 4, 5 | 7 | — |
| 7 | 6 | — | — |

## Todos
> Implementation + Test = ONE todo. Never separate.
<!-- APPEND TASK BATCHES BELOW THIS LINE WITH edit/apply_patch - never rewrite the headers above. -->
- [x] 1. D3 数据模型类型（LogLevel, LogEntry, ILogChannel, IDebugCommand, IDebugHub）
  What to do / Must NOT do:
    - 在 `Runtime/HN.Framework.Core/Driver/Common/Debug/` 下创建 5 个 .cs 文件
    - TDD: 先在 `Tests/HN.Framework.Core.Tests/Debug/` 下写测试，再创建类型文件
    - LogLevel: 简单 enum，5 个值。测试验证所有值存在且正确
    - LogEntry: readonly struct，5 个属性全部只读。测试验证构造 + 不可变性
    - ILogChannel: 接口，string Name { get; }, bool Enabled { get; set; }
    - IDebugCommand: 接口，string Name { get; }, string Description { get; }, void Execute(string[] args)
    - IDebugHub: 接口，void RegisterChannel(ILogChannel), void RegisterCommand(IDebugCommand), void Log(LogLevel, string channel, string message, object? context = null)
    - 所有公共 API 必须有完整 XML 文档注释（`<summary>` 最低要求）
    - MUST NOT: 引用任何 Unity 类型，不在 Core 层做任何实现类（除 DebugHub 外）
    - MUST NOT: 创建 Editor 或 UI 相关代码
    - MUST NOT: LogLevel 添加额外枚举值
  Parallelization: Wave 1 | Blocked by: — | Blocks: 2, 3, 4, 5
  References (executor has NO interview context - be exhaustive):
    - 架构定义: 架构~/最终架构.md:568-609（D3 完整接口定义）
    - 命名空间: 架构~/最终架构.md:437-438（`HN.Framework.Core.Driver.Common.Debug`）
    - 目录结构: 架构~/最终架构.md:1613-1618
    - 接口模式参考: `Runtime/HN.Framework.Core/Driver/Common/Interfaces/ITickable.cs:1-18`
    - 枚举模式参考: 无现有枚举，参考 C# 规范命名 PascalCase
    - 测试规范: AGENTS.md:70-94（asmdef overrideReferences, NUnit, [TestFixture], [SetUp]/[TearDown]）
    - 测试 asmdef: `Tests/HN.Framework.Core.Tests/HN.Framework.Core.Tests.asmdef`（需参照其结构）
    - 现有测试目录结构: Tests/HN.Framework.Core.Tests/（目前空白，仅有 asmdef）
  Acceptance criteria (agent-executable):
    - `Tests/HN.Framework.Core.Tests/Debug/LogLevelTests.cs` 编译通过（LogLevel 枚举值测试）
    - `Tests/HN.Framework.Core.Tests/Debug/LogEntryTests.cs` 编译通过（构造 + 不可变性测试）
    - 5 个类型文件存在于 `Runtime/HN.Framework.Core/Driver/Common/Debug/`
    - 每个公共 API 有 `<summary>` XML 文档注释
    - 所有文件 namespace 为 `HN.Framework.Core.Driver.Common.Debug`
    - Core 程序集能成功编译（无 UnityEngine 引用错误）
  QA scenarios (name the exact tool + invocation):
    - happy: 创建测试 asmdef（若不存在），运行 `vibe_unityMCP_run_tests(mode="EditMode", assembly_names=["HN.Framework.Core.Tests"])` → 全部通过
    - failure: 检查编译 — 确保 Core 程序集无 UnityEngine 依赖（`noEngineReferences: true`），如果引用 UnityEngine 类型则编译失败
    - Evidence: .omo/evidence/task-1-d3-debug-infra.log
  Commit: Y | `feat(core): add D3 debug infrastructure data models (LogLevel, LogEntry, ILogChannel, IDebugCommand, IDebugHub)`

- [x] 2. DebugHub 基础实现
  What to do / Must NOT do:
    - 在 `Runtime/HN.Framework.Core/Driver/Common/Debug/` 下创建 `DebugHub.cs`
    - 实现 IDebugHub 接口，内部用 `Dictionary<string, ILogChannel>` 和 `Dictionary<string, IDebugCommand>` 存储
    - `RegisterChannel`: 添加到 channels 字典。重复名称：静默覆盖（记录 LogLevel.Warning 日志到自身？不——当前无日志路由，改为直接覆盖 + 不抛异常）
    - `RegisterCommand`: 添加到 commands 字典。重复名称覆盖。
    - `Log(LogLevel, string channel, string message, object? context)`: 构造 LogEntry → 写入环形缓冲（最多 100 条）→ 触发 `OnLog` 事件。**不做日志输出**。
    - **设计决策（Metis F1）**: DebugHub **不做日志输出**。其 `Log()` 方法仅：构造 LogEntry → 写入环形缓冲（最多 100 条）→ 触发 `OnLog` 事件。日志输出由 ILogProvider 独立负责。两者职责完全分离，无耦合。DebugHub 不持有 ILogProvider 引用，不持有 GameWorld 引用。
    - MUST NOT: 不做线程安全
    - MUST NOT: 不做命令执行引擎（那是 C2 的职责）
    - MUST NOT: 不依赖 GameWorld 引用（Core 层不可反向依赖）
  Parallelization: Wave 2 | Blocked by: 1 | Blocks: 5 | Can parallelize with: 3
  References:
    - IDebugHub 接口定义: `Runtime/HN.Framework.Core/Driver/Common/Debug/IDebugHub.cs`
    - 环形缓冲模式参考: `Runtime/HN.Framework.Core/Driver/Common/Pool/ReferencePool/PooledCollections.cs`（PooledQueue）
    - ObjectPoolManager 作为 Core 层实现参考: `Runtime/HN.Framework.Core/Capability/Pool/ObjectPoolManager.cs`
    - GameWorld 持有内置模块模式: `Runtime/HN.Framework.Core/Driver/GameWorld/GameWorld.cs:21-25`
  Acceptance criteria:
    - `DebugHub` 类实现 `IDebugHub` 接口
    - `RegisterChannel` 可注册并查询（通过 `IReadOnlyDictionary<string, ILogChannel> Channels` 暴露）
    - `RegisterCommand` 可注册并查询（通过 `IReadOnlyDictionary<string, IDebugCommand> Commands` 暴露）
    - `Log` 方法调用后 `RecentEntries` 包含该 LogEntry
    - `OnLog` 事件在每次 Log 调用时触发
    - `RecentEntries` 最多保留 100 条（超过后丢弃最旧的）
    - 测试文件: `Tests/HN.Framework.Core.Tests/Debug/DebugHubTests.cs`
  QA scenarios:
    - happy: 运行 `vibe_unityMCP_run_tests(mode="EditMode", test_names=["HN.Framework.Core.Tests.Debug.DebugHubTests"])` → 全部通过
    - failure: 检查 DebugHub 不持有对 GameWorld 或任何 Unity 类型的引用 → grep 确认
    - Evidence: .omo/evidence/task-2-d3-debug-infra.log
  Commit: Y | `feat(core): implement DebugHub with channel/command registry and log ring buffer`

- [x] 3. ILogProvider 接口扩展
  What to do / Must NOT do:
    - 修改 `Runtime/HN.Framework.Core/Capability/Log/ILogProvider.cs`
    - 保留 `void Log(string message)` 方法（向后兼容）
    - 新增 `void Log(LogLevel level, string channel, string message, object context = null)` 方法（注意: `object` 非 `object?`，因 ILogProvider.cs 未启用 `#nullable enable`，与现有代码风格一致）
    - 添加必要的 `using HN.Framework.Core.Driver.Common.Debug;`（引用 LogLevel — 这是正确的架构依赖: Capability → Driver 单向依赖）
    - **架构说明**: LogLevel 在 Driver/D3，ILogProvider 在 Capability/C5。Capability 依赖 Driver 是框架设计的正确方向（架构.md §三.3.1）
    - MUST NOT: 删除或修改 `Log(string)` 的签名
    - MUST NOT: 给 `Log(string)` 添加默认实现（保持纯接口，不做 default implementation）
  Parallelization: Wave 2 | Blocked by: 1 | Blocks: 4 | Can parallelize with: 2
  References:
    - 当前 ILogProvider: `Runtime/HN.Framework.Core/Capability/Log/ILogProvider.cs:1-7`
    - 架构扩展方向: 架构~/最终架构.md:785-788
    - LogLevel 定义: `Runtime/HN.Framework.Core/Driver/Common/Debug/LogLevel.cs`（Todo 1 产出）
  Acceptance criteria:
    - `ILogProvider` 有两个方法: `Log(string)` 和 `Log(LogLevel, string, string, object)`
    - 核心程序集编译通过（UnityLogProvider 实现类会有编译错误，留待 Todo 4 修复）
    - 测试文件: `Tests/HN.Framework.Core.Tests/Debug/LogProviderContractTests.cs` — 验证接口包含两个方法签名（通过反射或编译时验证）
  QA scenarios:
    - happy: `vibe_unityMCP_run_tests(mode="EditMode", assembly_names=["HN.Framework.Core.Tests"])` → 全部通过
    - failure: 验证 `Log(string)` 未被删除 → grep `ILogProvider.cs` 确认 `void Log(string message)` 存在
    - Evidence: .omo/evidence/task-3-d3-debug-infra.log
  Commit: Y | `feat(core): extend ILogProvider with structured logging (LogLevel, channel, context)`

- [x] 4. UnityLogProvider 实现更新
  What to do / Must NOT do:
    - 修改 `Runtime/HN.Framework.Unity/Driver/Platform/Log/UnityLogProvider.cs`
    - 实现新 `ILogProvider.Log(LogLevel level, ...)` 方法
    - 新增的重载按 `[Level][Channel] message` 格式路由到 `UnityEngine.Debug.Log`（非 Error 级别）/ `Debug.LogWarning`/ `Debug.LogError`
    - 格式化规则：
      - Debug/Info → `Debug.Log($"[{level}][{channel}] {message}")`
      - Warning → `Debug.LogWarning($"[{level}][{channel}] {message}")`
      - Error/Fatal → `Debug.LogError($"[{level}][{channel}] {message}")`
    - `context` 参数暂时忽略（不格式化到输出，后续 C2 阶段再处理结构上下文）
    - MUST NOT: 修改 `Log(string)` 的行为（仍路由到 `Debug.Log(message)`）
    - MUST NOT: 添加 using 到 Core 的 Debug 命名空间之外的额外依赖
  Parallelization: Wave 3 | Blocked by: 3 | Blocks: 5
  References:
    - 当前 UnityLogProvider: `Runtime/HN.Framework.Unity/Driver/Platform/Log/UnityLogProvider.cs:1-13`
    - ILogProvider 接口: `Runtime/HN.Framework.Core/Capability/Log/ILogProvider.cs`（Todo 3 产出）
    - UnityLogProvider 架构规划: 架构~/最终架构.md:650-653
    - Unity 层测试规范: AGENTS.md:70-94
    - Unity 测试 asmdef: `Tests/HN.Framework.Unity.Tests/HN.Framework.Unity.Tests.asmdef`
  Acceptance criteria:
    - `UnityLogProvider` 实现两个 `ILogProvider` 方法
    - `Log(LogLevel.Debug, "test", "hello")` 调用 `Debug.Log("[Debug][test] hello")`
    - `Log(LogLevel.Warning, "test", "warn")` 调用 `Debug.LogWarning("[Warning][test] warn")`
    - `Log(LogLevel.Error, "test", "err")` 调用 `Debug.LogError("[Error][test] err")`
    - `Log("plain")` 仍调用 `Debug.Log("plain")`
    - 测试文件: `Tests/HN.Framework.Unity.Tests/Log/UnityLogProviderTests.cs` — 使用 `LogAssert.Expect` 验证正确级别的日志输出
  QA scenarios:
    - happy: 运行 `vibe_unityMCP_run_tests(mode="EditMode", test_names=["HN.Framework.Unity.Tests.Log.UnityLogProviderTests"])` → 全部通过
    - failure: 验证 `Log(string)` 未被修改 → 测试确认仍输出到 `Debug.Log`
    - Evidence: .omo/evidence/task-4-d3-debug-infra.log
  Commit: Y | `feat(unity): update UnityLogProvider for structured logging with LogLevel routing`

- [x] 5. GameWorld 集成 DebugHub
  What to do / Must NOT do:
    - 修改 `Runtime/HN.Framework.Core/Driver/GameWorld/GameWorld.cs`
    - 在构造函数中创建 `DebugHub` 实例: `DebugHub = new DebugHub();`
    - 添加 `public DebugHub DebugHub { get; }` 属性
    - 添加 `using HN.Framework.Core.Driver.Common.Debug;`
    - MUST NOT: 修改 `GameWorldDriver.cs`
    - MUST NOT: 修改 Tick/LateTick 逻辑
    - MUST NOT: 改动任何现有属性的行为
    - 将 `ILogProvider LogProvider` 改为 nullable 以允许注入前为 null（当前非 nullable 需要先注入，留待后续改进，本次仅加 DebugHub）
  Parallelization: Wave 4 | Blocked by: 2, 4
  References:
    - 当前 GameWorld: `Runtime/HN.Framework.Core/Driver/GameWorld/GameWorld.cs:1-49`
    - 架构规划: 架构~/最终架构.md:341-367（GameWorld 持有 DebugHub）
    - DebugHub 类: `Runtime/HN.Framework.Core/Driver/Common/Debug/DebugHub.cs`（Todo 2 产出）
  Acceptance criteria:
    - `GameWorld` 构造函数中创建 `DebugHub` 实例
    - `world.DebugHub` 属性可访问，非 null
    - `world.DebugHub` 是 `DebugHub` 类型（不是接口）
    - 现有测试全部通过（回归验证）
    - 测试文件（可合并到现有测试或新建）: 验证 GameWorld.DebugHub 在构造后非 null
  QA scenarios:
    - happy: 运行 `vibe_unityMCP_run_tests(mode="EditMode")` → 全部通过（包括已有的 AssetManager/Pool/HFSM/Procedure/MVC 测试）
    - failure: 检查现有测试是否有因 DebugHub 引入而失败的 → 逐一排查
    - Evidence: .omo/evidence/task-5-d3-debug-infra.log
  Commit: Y | `feat(core): integrate DebugHub into GameWorld`

- [x] 6. 全量测试验证与修复
  What to do / Must NOT do:
    - 运行全量 EditMode 测试
    - 修复所有测试失败
    - 确认所有 5 个 todo 的测试文件全部通过
    - 确认回归（现有 AssetManager/Pool/HFSM/Procedure/MVC 测试不受影响）
    - 检查 Console 无编译错误/警告
    - MUST NOT: 跳过任何模块的测试
    - MUST NOT: 新增功能 — 本 todo 仅做验证和修复
  Parallelization: Wave 5 | Blocked by: 1, 2, 3, 4, 5
  References:
    - 所有前面 todo 的产出文件
    - MCP 测试运行: `vibe_unityMCP_run_tests`
    - MCP Console 读取: `vibe_unityMCP_read_console`
  Acceptance criteria:
    - `vibe_unityMCP_run_tests(mode="EditMode")` → 全部通过，0 失败
    - 新增测试覆盖: LogLevel, LogEntry, DebugHub, ILogProvider, UnityLogProvider, GameWorld
    - Console 无 Error 级别消息
    - 现有测试无回归
  QA scenarios:
    - happy: `vibe_unityMCP_run_tests(mode="EditMode")` → All tests passed, 0 failed
    - failure: 如有失败 → 读取 Console 定位错误 → 修复 → 重跑全部测试直到全绿
    - Evidence: .omo/evidence/task-6-d3-debug-infra.log
  Commit: Y | `test: add full D3 debug infrastructure test suite, verify no regression`

- [x] 7. 文档站更新
  What to do / Must NOT do:
    - 更新 3 个文件，反映 D3 Debug 基础设施已完成
    - **`docs-site~/docs/api/index.md`**: 在 "Driver — 驱动层" 下，`HNLogicTime` 之后新增 D3 条目：
      ```
      - **LogLevel** — 日志等级枚举（Debug/Info/Warning/Error/Fatal）
      - **ILogChannel** — 模块级日志通道接口（Name / Enabled）
      - **LogEntry** — 结构化日志条目（Timestamp, Channel, Level, Message, Context）
      - **IDebugHub** — 调试中枢接口（RegisterChannel / RegisterCommand / Log）
      - **IDebugCommand** — 调试命令接口（Name / Description / Execute）
      - **DebugHub** — 调试中枢实现类（通道/命令注册表 + 环形日志缓冲）
      ```
      同时更新 ILogProvider 条目标注已扩展：` - **ILogProvider** — 日志提供者接口（支持 LogLevel 分级 + Channel 通道）`
    - **`docs-site~/docs/guide/core.md`**: 在 "GameWorld" 节之后、"GameWorldDriver" 之前新增 "## DebugHub — 调试中枢" 节：
      ```
      ## DebugHub — 调试中枢

      `DebugHub` 是 D3 Debug 基础设施的核心实现，由 `GameWorld` 在构造函数中
      自动创建，通过 `world.DebugHub` 属性访问。

      ### 职责

      - **通道注册**：`RegisterChannel(ILogChannel)` — 模块注册日志通道，支持运行时开关
      - **命令注册**：`RegisterCommand(IDebugCommand)` — 注册调试命令
      - **结构化日志**：`Log(LogLevel, channel, message, context)` — 写入环形缓冲（最多 100 条）
      - **事件通知**：`OnLog` 事件 — 每次 Log 调用时触发

      ### 与 ILogProvider 的关系

      `DebugHub` 与 `ILogProvider` 职责完全分离：
      - `DebugHub` — 结构化日志基础设施（存储 + 事件），不做输出
      - `ILogProvider` — 唯一日志输出通道（UnityEngine.Debug / 文件 / 网络）

      ```csharp
      // 注册模块日志通道
      world.DebugHub.RegisterChannel(new LogChannel("pool", enabled: true));

      // 记录结构化日志（写入调试缓冲 + 触发 OnLog 事件）
      world.DebugHub.Log(LogLevel.Warning, "pool", "池容量接近上限");

      // 通过 ILogProvider 输出日志
      world.LogProvider.Log(LogLevel.Info, "pool", "对象池初始化完成");
      ```
      ```
    - **`docs-site~/docs/dev/architecture.md`**: 找到 D3 行，将状态标记从 📋 改为 ✅。搜索 `D3` 或 `Debug 基础设施` 定位。
    - MUST NOT: 新增文档文件（仅修改已有文件）
    - MUST NOT: 修改非 D3 相关的文档内容
  Parallelization: Wave 6 | Blocked by: 6
  References:
    - API 索引: `docs-site~\docs\api\index.md:17-28`（Driver 节插入位置）
    - Core 指南: `docs-site~\docs\guide\core.md:21-43`（GameWorld 节之后插入）
    - 架构文档: `docs-site~\docs\dev\architecture.md`（搜索 D3 状态标记）
    - 架构定义: 架构~\最终架构.md:568-609
  Acceptance criteria:
    - `docs/api/index.md` 的 Driver 节包含 LogLevel, ILogChannel, LogEntry, IDebugHub, IDebugCommand, DebugHub 条目
    - `docs/api/index.md` 的 ILogProvider 条目已更新为 "支持 LogLevel 分级 + Channel 通道"
    - `docs/guide/core.md` 包含 "DebugHub — 调试中枢" 节，包含职责、示例代码、与 ILogProvider 关系
    - `docs/dev/architecture.md` 中 D3 状态已从 📋 更新为 ✅
  QA scenarios:
    - happy: grep 确认 3 个文件均包含预期内容 — `grep "DebugHub" docs-site~/docs/api/index.md` → 有匹配；`grep "DebugHub" docs-site~/docs/guide/core.md` → 有匹配；`grep "✅" docs-site~/docs/dev/architecture.md` → D3 行标记为 ✅
    - Evidence: .omo/evidence/task-7-d3-debug-infra.log
  Commit: Y | `docs: update api index, core guide, and architecture docs for D3 debug infrastructure`

## Final verification wave
> Runs in parallel after ALL todos. ALL must APPROVE. Surface results and wait for the user's explicit okay before declaring complete.
- [x] F1. Plan compliance audit — 确认所有 Must have 项已交付，所有 Must NOT have 未被违反
- [x] F2. Code quality review — 检查 XML 文档注释完整性、命名规范、namespace 正确性
- [x] F3. Real manual QA — 通过 Unity MCP 运行全量 EditMode 测试，验证 Console 无错误
- [x] F4. Scope fidelity — diff 检查：确认只修改了计划内的文件，无越界改动

## Commit strategy
分 7 次提交，每次对应一个 Todo（TDD 测试 + 实现 + QA 通过后提交）：
1. `feat(core): add D3 debug infrastructure data models`
2. `feat(core): implement DebugHub with channel/command registry and log ring buffer`
3. `feat(core): extend ILogProvider with structured logging`
4. `feat(unity): update UnityLogProvider for structured logging with LogLevel routing`
5. `feat(core): integrate DebugHub into GameWorld`
6. `test: add full D3 debug infrastructure test suite, verify no regression`
7. `docs: update api index, core guide, and architecture docs for D3 debug infrastructure`

## Success criteria
- ✅ 5 个 D3 数据模型类型文件存在于 `Core/Driver/Common/Debug/`
- ✅ DebugHub 基础实现，支持通道/命令注册和日志缓冲
- ✅ ILogProvider 支持结构化日志（LogLevel + Channel + Context）
- ✅ UnityLogProvider 适配新接口，按级别路由到正确的 UnityEngine.Debug 方法
- ✅ GameWorld 持有 DebugHub 实例
- ✅ 6 个测试文件全部通过 EditMode 测试
- ✅ 现有测试零回归
- ✅ 所有公共 API 有完整 XML 文档注释
- ✅ 文档站 3 个文件已更新（api/index.md, guide/core.md, dev/architecture.md）
