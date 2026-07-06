---
slug: serialization-fix
status: drafting
intent: clear
pending-action: write .omo/plans/serialization-fix.md
approach: Fix all 11 identified serialization module issues across 3 source files + add EditMode tests + update docs
---

# Draft: serialization-fix

## Components (topology ledger)
| id | outcome | status | evidence path |
|----|---------|--------|---------------|
| C1 | Json.cs: 6 fixes (WriteToDisk leak, silent catch, unicode, depth, enum int, ReadFromDisk) | active | Runtime/HN.Framework.Core/Driver/Common/Serialization/Json.cs |
| C2 | JsonData.cs: 3 fixes (Obj cache, dead reflection, redundant assign) | active | Runtime/HN.Framework.Unity/Driver/Platform/Serialization/JsonData.cs |
| C3 | Tests: Unity Test Framework EditMode tests for serialization | active | Tests/HN.Framework.Unity.Tests/ (existing infrastructure) |
| C4 | Docs: fix serialize.md, verify architecture docs | active | docs-site~/docs/guide/serialize.md, docs-site~/docs/dev/architecture.md |

## Open assumptions (announced defaults)
| assumption | adopted default | rationale | reversible? |
|------------|----------------|-----------|-------------|
| Enum format change breaking existing data | User accepted BREAKING change | Framework early stage, industry standard | yes (revert to string if needed) |
| Test strategy | Unity Test Framework EditMode | Follows existing test infrastructure in Tests/HN.Framework.Unity.Tests/ | yes |
| Core layer error logging | System.Diagnostics.Debug.WriteLine | noEngineRefs prevents Debug.Log; Trace is in .NET Standard 2.1 | yes |
| ISerializer interface | Deferred to MemoryPack integration | Scope-creep; not in identified issues list | yes |
| Unicode support scope | Basic \uXXXX only (not surrogate pairs) | Surrogate pairs rarely needed for game config JSON | yes |

## Findings (cited - path:lines)
- Core serializer is hand-written, NOT JsonUtility-based: Json.cs:1-621
- WriteToDisk opens FileStream (never disposed) then File.CreateText on same path: Json.cs:25-33
- Obj getter calls DeserializeFromString every time: JsonData.cs:25-29
- Silent catch{} in ConvertJsonValue: Json.cs:479-487
- Dead Assembly.Load code in SerializeToJson: JsonData.cs:85-87
- Redundant jsonText="" in OnBeforeSerialize: JsonData.cs:111
- JsonObject is empty [Serializable] marker class: JsonObject.cs:1-10
- Zero test coverage for serialization module
- serialize.md falsely claims JsonUtility-based: docs-site~/docs/guide/serialize.md:19
- Existing test asmdef at Tests/HN.Framework.Unity.Tests/ references both Core and Unity assemblies, uses NUnit

## Decisions (with rationale)
1. Enum → integer: User chose. Industry standard, matches Newtonsoft.Json/System.Text.Json behavior.
2. Unity Test Framework EditMode: User chose. Reuses existing test infrastructure.
3. No ISerializer interface now: Deferred. Adding interface without MemoryPack integration is premature abstraction.
4. Keep hand-written parser: Feature-complete for framework needs. 3rd-party lib is scope-creep.

## Scope IN
- Json.cs: Fix WriteToDisk, ConvertJsonValue, unicode support, depth limit, enum int, ReadFromDisk
- JsonData.cs: Fix Obj caching, remove dead reflection, remove redundant assignment
- JsonObject.cs: Add XML doc comment clarifying purpose
- Tests: Serialize, Deserialize, FileIO, JsonData behavior (4 test files or classes)
- Docs: serialize.md full rewrite of technical description, architecture.md verify

## Scope OUT (Must NOT have)
- Must NOT introduce 3rd-party JSON library
- Must NOT add ISerializer interface abstraction
- Must NOT modify JsonObject beyond XML doc comment
- Must NOT touch any other module (HFSM, MVC, Pool, etc.)
- Must NOT modify asmdef references in Core or Unity assemblies

## Open questions
(None remaining - all forks resolved)

## Approval gate
status: approved → plan written to .omo/plans/serialization-fix.md
