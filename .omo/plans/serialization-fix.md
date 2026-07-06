# serialization-fix - Work Plan

## TL;DR (For humans)

**What you'll get:** 序列化模块 11 个已识别缺陷全部修复（资源泄漏、性能陷阱、静默吞异常、解析器增强），新增完整单元测试覆盖，文档中的 3 处错误陈述同步修正。

**Why this approach:** 最小化破坏性——Enum 改为整数序列化但保持向后兼容读取（旧文件仍可反序列化）。`JsonData.Obj` 使用脏标记缓存避免每次访问重新解析。Core 层修复严格保持 `noEngineRefs` 约束。

**What it will NOT do:** 不引入第三方 JSON 库，不添加 ISerializer 接口抽象（推迟到 MemoryPack 集成），不修改 Core/Unity 的 asmdef 引用，不扩大 JsonObject 空类的设计。

**Effort:** Medium（4 组件，~600 行代码变更 + ~400 行测试 + 文档）
**Risk:** Low — 集中在单模块，测试先行
**Decisions to sanity-check:** Enum 向后兼容读取策略（旧字符串格式 → 新整数输出）；`ReadFromDisk` 失败时 `Deserialize` 返回 false 而非 true

Your next move: approve the plan, then run `$start-work` to begin execution. Full execution detail follows below.

---

> TL;DR (machine): Medium effort, Low risk, 4 components (C1 Json.cs 7 fixes + C2 JsonData.cs 3 fixes + C3 Tests ~400 LOC + C4 Docs 3 updates), Enum breaking with backward-compat read, no third-party deps

## Scope
### Must have
- C1: Json.cs — fix WriteToDisk resource leak, ConvertJsonValue silent catch, add Unicode \uXXXX support (serialize + deserialize), add depth limit (64, both directions), Enum → integer serialization with backward-compat read, ReadFromDisk error handling, DeserializeFromString(typeName) null-safety
- C2: JsonData.cs — Obj getter dirty-flag cache, remove dead Assembly.Load code in SerializeToJson, remove redundant jsonText="" in OnBeforeSerialize
- C3: Tests — roundtrip tests (all primitive types including enum-int), edge cases (null, empty, nested depth, invalid JSON, missing file), JsonData cache/ISerializationCallbackReceiver behavior
- C4: Docs — fix serialize.md (3 incorrect statements: "JsonUtility-based", "no Dictionary support", "JsonUtility limits"), verify architecture docs

### Must NOT have (guardrails, anti-slop, scope boundaries)
- Must NOT introduce any third-party JSON library (Newtonsoft, TinyJson, Utf8Json, etc.)
- Must NOT add ISerializer interface abstraction
- Must NOT modify asmdef files (Core, Unity, Editor)
- Must NOT change JsonObject beyond XML doc comment
- Must NOT touch any other module (MVC, HFSM, Pool, Procedure, etc.)
- Must NOT break Core's noEngineRefs constraint (no UnityEngine, no Debug.Log)
- Must NOT change the public API signatures of Json.Serialize/Deserialize methods

## Verification strategy
> Zero human intervention - all verification is agent-executed.
- Test decision: tests-after (write tests, implement fixes, verify all pass)
- Framework: Unity Test Framework EditMode (NUnit, via existing HN.Framework.Unity.Tests.asmdef)
- Evidence: .omo/evidence/task-<N>-serialization-fix.log (test output), .omo/evidence/task-<N>-serialization-fix.diff (diff review)

## Execution strategy
### Parallel execution waves
- **Wave 1** (tests-first, no deps): 1 (Json roundtrip tests), 2 (Json edge cases), 3 (JsonData tests) — run in parallel, all expected to FAIL initially
- **Wave 2** (core fixes, depends on Wave 1): 4 (Json.cs fixes) — blocked by 1+2
- **Wave 3** (unity fixes, no deps on Wave 2): 5 (JsonData.cs fixes) — blocked by 3; 6 (Docs updates) — independent
- **Wave 4** (verify): F1-F4 final verification — blocked by all

### Dependency matrix
| Todo | Depends on | Blocks | Can parallelize with |
| --- | --- | --- | --- |
| 1 | — | 4 | 2, 3 |
| 2 | — | 4 | 1, 3 |
| 3 | — | 5 | 1, 2 |
| 4 | 1, 2 | F1-F4 | — |
| 5 | 3 | F1-F4 | 6 |
| 6 | — | F1-F4 | 5 |
| F1-F4 | 4, 5, 6 | — | all parallel |

## Todos
> Implementation + Test = ONE todo. Never separate.

- [x] 1. Write Json roundtrip tests (happy path)
  What to do: Create serialization roundtrip tests in Tests/HN.Framework.Unity.Tests/Serialization/JsonRoundtripTests.cs. Test: primitive types (int, float, double, string, bool, decimal, long, short, byte, uint, ulong, ushort, sbyte, char), enum (integer format), arrays, List<T>, Dictionary<K,V>, nested objects, null handling.
  Must NOT do: Don't test JsonData (that's todo 3), don't test file I/O (that's todo 2 edge cases).
  Parallelization: Wave 1 | Blocked by: — | Blocks: 4
  References: Runtime/HN.Framework.Core/Driver/Common/Serialization/Json.cs:1-621 (all public API), Tests/HN.Framework.Unity.Tests/ (existing pattern), Tests/HN.Framework.Unity.Tests/HN.Framework.Unity.Tests.asmdef:1-20 (test asmdef config)
  Acceptance criteria: All roundtrip tests pass (serialize→deserialize→equality check for all types). Enum test verifies integer output format (e.g. `0` not `"ValueName"`). Dictionary roundtrip verifies both keys and values survive.
  QA scenarios: Run via Unity Test Runner: `HN.Framework.Unity.Tests > JsonRoundtripTests`. Happy: all pass. Failure: any mismatch in roundtrip values. Evidence .omo/evidence/t1-serialization-fix.log
  Commit: N (part of todo 4 batch)

- [x] 2. Write Json edge case tests
  What to do: Create Tests/HN.Framework.Unity.Tests/Serialization/JsonEdgeCaseTests.cs. Test: empty JSON "{}", null strings, depth limit (65+ levels → exception), invalid JSON handling, missing file return empty, Unicode escape \uXXXX roundtrip, enum backward-compat (parse string "ValueName" → correct int value), DeserializeFromString(typeName) with invalid type name.
  Must NOT do: Don't duplicate roundtrip tests from todo 1.
  Parallelization: Wave 1 | Blocked by: — | Blocks: 4
  References: Json.cs:357-459 (ParseJsonObject), Json.cs:600-619 (EscapeJson), Json.cs:120-134 (DeserializeFromString typeName), Json.cs:20-35 (WriteToDisk), Json.cs:42-45 (ReadFromDisk)
  Acceptance criteria: Empty JSON → returns object with default values. Depth > 64 → throws or safely returns null. Invalid JSON → returns null, no crash. Missing file → ReadFromDisk returns "". Unicode \u0041 → roundtrips as "A". Enum "ValueName" → deserializes to MyEnum.Value1.
  QA scenarios: Run via Unity Test Runner. Happy: all edge cases handled gracefully. Failure: any unhandled exception or wrong return value. Evidence .omo/evidence/t2-serialization-fix.log
  Commit: N (part of todo 4 batch)

- [x] 3. Write JsonData behavior tests
  What to do: Create Tests/HN.Framework.Unity.Tests/Serialization/JsonDataTests.cs. Test: Obj getter cache (first call parses, second returns cached without re-parse), Serialize after modifying Obj reflects changes, OnBeforeSerialize/OnAfterDeserialize callback behavior, construction with valid/invalid type.
  Must NOT do: Don't test Json.cs internals (covered by todos 1+2).
  Parallelization: Wave 1 | Blocked by: — | Blocks: 5
  References: Runtime/HN.Framework.Unity/Driver/Platform/Serialization/JsonData.cs:1-120, Runtime/HN.Framework.Core/Driver/Common/Serialization/JsonObject.cs:1-10
  Acceptance criteria: AC-C2.1.1: first Obj access deserializes from jsonText. AC-C2.1.2: second Obj access returns cached (no re-parse). AC-C2.1.3: setting jsonText then accessing Obj returns updated data. AC-C2.1.4: OnAfterDeserialize triggers cache refresh. AC-C2.1.5: construction with null type doesn't crash.
  QA scenarios: Run via Unity Test Runner. Happy: cache behavior verified. Failure: cache staleness or unexpected re-parsing. Evidence .omo/evidence/t3-serialization-fix.log
  Commit: N (part of todo 5 batch)

- [x] 4. Fix Json.cs (7 issues in C1)
  What to do: Fix all C1 issues in Json.cs in one commit:
  1. WriteToDisk: replace lines 20-35 with `File.WriteAllText(path, text, Encoding.UTF8)` + null check
  2. ConvertJsonValue catch{}: add `System.Diagnostics.Debug.WriteLine($"JSON deserialization: failed to set field '{field.Name}' to '{strVal}': {ex.Message}")` inside catch
  3. Unicode: add `\uXXXX` unescape in ParseJsonObject key/value parsing + in SkipJsonValue string handling; add `\uXXXX` escape in EscapeJson for chars > 127
  4. Depth limit: add `const int MaxDepth = 64` + check in SerializeObject (if depth >= MaxDepth, write "null") + in ParseJsonObject/DeserializeObject (track depth, return null if exceeded)
  5. Enum: change SerializeObject line 226 from quoted string to `((int)obj).ToString()`; change ConvertJsonValue lines 539-540 to `return Enum.ToObject(targetType, int.Parse(strVal))` with fallback to `Enum.Parse(targetType, strVal.Trim('"'))` for backward compat
  6. ReadFromDisk: add `if (!File.Exists(path)) return string.Empty;` + try-catch returning string.Empty on exception
  7. DeserializeFromString(typeName): add null check for `type` after Type.GetType, return null if type is null
  Also fix Deserialize<T>(T obj, string path) to return false when ReadFromDisk returns empty string. Also fix Deserialize<T>(string path) to return default when file missing.
  Must NOT do: Don't change public API signatures. Don't add Unity dependencies. Don't touch JsonData.cs.
  Parallelization: Wave 2 | Blocked by: 1, 2 | Blocks: F1-F4
  References: Json.cs:20-35 (WriteToDisk), Json.cs:479-487 (catch{}), Json.cs:600-619 (EscapeJson), Json.cs:357-459 (ParseJsonObject/SkipJsonValue), Json.cs:187-245 (SerializeObject), Json.cs:221-227 (enum serialize), Json.cs:539-540 (enum deserialize), Json.cs:42-45 (ReadFromDisk), Json.cs:120-134 (DeserializeFromString typeName), Json.cs:85-94 (Deserialize path), Json.cs:142-150 (Deserialize<T> path)
  Acceptance criteria: WriteToDisk uses single File.WriteAllText call (no FS leak). ConvertJsonValue logs to Debug output on failure (verify in Unity console with debugger). Unicode \u0041 roundtrips correctly. Depth 65 returns null/safe value. Enum serialized as integer, old string-format enum still readable. ReadFromDisk returns "" on missing file, no crash. Deserialize returns false on missing file. DeserializeFromString(null type) returns null.
  QA scenarios: Run tests 1 + 2 via Unity Test Runner — all must pass. Additional: manually verify WriteToDisk no longer leaves file handles open (check via Process Explorer or after 1000 rapid writes). Evidence .omo/evidence/t4-serialization-fix.log
  Commit: Y | fix(serialization): fix resource leak, silent catch, unicode, depth limit, enum format, error handling

- [x] 5. Fix JsonData.cs (3 issues in C2)
  What to do: Fix all C2 issues in JsonData.cs:
  1. Obj cache: add `private string cachedJsonText;` field. In Obj getter: if `jsonText != cachedJsonText` → `DeserializeFromString(jsonText); cachedJsonText = jsonText;`. Return obj. In Serialize(): after serializing, set `cachedJsonText = jsonText`. In OnAfterDeserialize: set `cachedJsonText = null` to force re-parse on next access.
  2. Remove dead code: delete lines 85-87 (Assembly.Load + GetType + commented-out line) from SerializeToJson. Keep objTypeName null check at line 82-83 as initialization guard.
  3. Remove redundant assignment: delete `jsonText = "";` at line 111 in OnBeforeSerialize.
  Must NOT do: Don't change public API. Don't modify DeserializeFromString (its Assembly.Load is USED for CreateInstance). Don't touch Json.cs.
  Parallelization: Wave 3 | Blocked by: 3 | Blocks: F1-F4 | Can parallelize with: 6
  References: JsonData.cs:25-29 (Obj getter), JsonData.cs:77-88 (SerializeToJson dead code), JsonData.cs:109-113 (OnBeforeSerialize redundant), JsonData.cs:91-106 (DeserializeFromString - DON'T touch, Assembly.Load IS used)
  Acceptance criteria: AC-C2.1.1-5 all pass (from T3 tests). SerializeToJson no longer contains Assembly.Load. OnBeforeSerialize no longer sets jsonText = "" before Serialize().
  QA scenarios: Run tests 3 — all must pass. Additional: verify via debugger that second Obj access skips DeserializeFromString. Evidence .omo/evidence/t5-serialization-fix.log
  Commit: Y | fix(serialization): cache JsonData.Obj, remove dead reflection, fix redundant assignment

- [x] 6. Update documentation
  What to do: Fix 3 incorrect statements in serialize.md + verify architecture docs:
  1. serialize.md line 19: Replace "所有序列化基于 Unity 的 `JsonUtility`" → "Core 层 `Json` 类为纯 C# 手写 JSON 序列化器，不依赖 `JsonUtility`"
  2. serialize.md line 147-148: Replace "序列化基于 `JsonUtility`，只支持 `[Serializable]` 标记的类型" + "不支持字典、多维数组等复杂类型（Unity JsonUtility 限制）" → "支持基本类型、数组、`List<T>`、`Dictionary<K,V>`、嵌套对象；支持 `[Serializable]` 标记的类型"
  3. serialize.md line 148 "不支持字典" → actually it's already covered in #2
  4. serialize.md: Add note about Enum integer format: "枚举序列化为整数值（如 `0`、`1`），反序列化兼容旧的字符串格式"
  5. docs-site~/docs/dev/architecture.md: Verify serialization module status is correct (line 202: "基础类库（Interfaces/HNLogicTime/Serialization）" ✅). Add note if needed.
  6. 架构~/最终架构.md: Verify line 629 (D2 Serialization ✅). Add note if needed.
  Must NOT do: Don't rewrite entire serialize.md guide — only fix incorrect statements. Don't change code examples unless they're wrong.
  Parallelization: Wave 3 | Blocked by: — | Blocks: F1-F4 | Can parallelize with: 5
  References: docs-site~/docs/guide/serialize.md:19 (JsonUtility claim), serialize.md:147-148 (capability claims), serialize.md:145-151 (注意事项 section), docs-site~/docs/dev/architecture.md:197-223 (module status), 架构~/最终架构.md:617-648 (module status appendix)
  Acceptance criteria: serialize.md no longer mentions JsonUtility-backed. serialize.md correctly states Dictionary/Array/List support. serialize.md documents enum integer format. Architecture docs correctly reflect serialization module state.
  QA scenarios: Read serialize.md — verify no mention of "JsonUtility" in technical description. Verify Dictionary support is mentioned. Evidence .omo/evidence/t6-serialization-fix.diff
  Commit: Y | docs(serialization): correct serialize.md claims, update architecture status

## Final verification wave
> Runs in parallel after ALL todos. ALL must APPROVE. Surface results and wait for the user's explicit okay before declaring complete.

- [x] F1. Plan compliance audit — verify all 11 issues resolved: grep for old patterns (FileStream+CreateText in same method, silent `catch{}`, `jsonText = ""` before Serialize(), dead Assembly.Load in SerializeToJson); confirm all fixes present. Evidence: .omo/evidence/f1-serialization-fix.log
- [x] F2. Code quality review — verify no new warnings, XML doc comments intact, no regression in public API. Evidence: .omo/evidence/f2-serialization-fix.log
- [x] F3. Real QA — run FULL test suite (`HN.Framework.Unity.Tests`) via Unity Test Runner. All existing + new tests must pass. Evidence: .omo/evidence/f3-serialization-fix.log
- [x] F4. Scope fidelity — verify no unintended changes: git diff limited to Json.cs, JsonData.cs, JsonObject.cs (doc comment only), new test files, serialize.md, architecture docs. No asmdef changes, no other module changes. Evidence: .omo/evidence/f4-serialization-fix.diff

## Commit strategy
- 4 (Json.cs fixes) + 1/2 tests: single commit — `fix(serialization): fix resource leak, silent catch, unicode, depth limit, enum format, error handling`
- 5 (JsonData.cs fixes) + 3 tests: single commit — `fix(serialization): cache JsonData.Obj, remove dead reflection, fix redundant assignment`
- 6 (Docs): single commit — `docs(serialization): correct serialize.md claims, update architecture status`

## Success criteria
1. All 11 identified issues resolved (verified by F1 audit)
2. All existing tests pass (F3) — no regression
3. All new tests pass (1+2+3) — 100% pass rate
4. Documentation consistent with actual implementation (F4)
5. Zero new compiler warnings (F2)
6. Diff scope bounded to 6 files max (F4)
