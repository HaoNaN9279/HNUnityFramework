---
slug: fix-pool-systems
status: drafting
intent: clear
pending-action: write .omo/plans/fix-pool-systems.md
approach: Fix 13 pool system problems across 7 components. Core fixes (C1-C2: safety + lifecycle), Unity fixes (C3: prototype destruction), code refactoring (C4: Tick template, C5: PoolSettings, C6: PooledCollections rename), hygiene (C7). Unity Test for all pool systems.
---

# Draft: fix-pool-systems

## Components (topology ledger)
| id | outcome | status | evidence path |
|----|---------|--------|---------------|
| C1 | ObjectPool<T> safety — Release clears+validates, Acquire uses direct cast | active | ObjectPool.cs:170-187, PooledObjectBase.cs |
| C2 | ObjectPoolManager lifecycle — ClearAll releases to ReferencePool, Init renamed | active | ObjectPoolManager.cs:305-313, GameWorld.cs:29-33 |
| C3 | GameObjectPool fixes — Clear() keeps prototype, typo fixed | active | GameObjectPool.cs:198-225, 335-353 |
| C4 | PoolBase Tick template — extract shared Tick logic to PoolBase | active | PoolBase.cs, ObjectPool.cs:203-236, GameObjectPool.cs:289-322 |
| C5 | Initialize overload reduction — PoolSettings struct replaces 41 overloads | active | PoolBase.cs:116-122, ObjectPoolBase.cs, GameObjectPoolBase.cs, ObjectPoolManager.cs |
| C6 | PooledCollections rename — fix misleading class names + sync refs | active | PooledCollections.cs:100-123 |
| C7 | Code hygiene — delete dead editor, fix namespaces, encapsulate fields | active | ObjectPoolViewerEditor.cs, PoolBase.cs:116-122, 架构~/最终架构.md:425-431 |

## Open assumptions (announced defaults)
| assumption | adopted default | rationale | reversible? |
|------------|----------------|-----------|-------------|
| PoolSettings struct in Core | C# struct with public fields | .NET Standard 2.1 compatible, no record support | yes |
| Tick template in PoolBase | abstract SpawnOne/DespawnOne hooks | PoolBase already has abstract spawn/despawn concepts | yes |
| PooledCollections rename scope | All .cs files in workspace | User chose direct rename, no Obsolete | yes (add Obsolete later if needed) |

## Findings (cited - path:lines)
- No existing tests for any pool code (Tests/ directory has zero Pool* files)
- PooledQueue<PooledObjectBase> used internally by ObjectPool<T>: ObjectPool.cs:287
- PooledQueue<GameObject> used internally by GameObjectPool: GameObjectPoolBase.cs:133
- ReferencePool is static and correctly thread-safe: ReferencePool.cs:10-11
- GameWorld owns ObjectPoolManager as instance: GameWorld.cs:11
- GameWorldDriver does NOT register any GameObject pools: GameWorldDriver.cs:15-30
- ObjectPoolViewerEditor is #if false'd dead code: ObjectPoolViewerEditor.cs:1
- Misleading PooledConcurrent* classes have ZERO external references (only defined in PooledCollections.cs) — rename is safe
- PooledList<T> used in Model.cs:132, Controller.cs:135, HFSM.cs:150, HFSM.cs:567
- PooledDictionary<K,V> used in HFSMState.cs:110-111, HFSM.cs:147, HFSM.cs:557, HFSM.cs:562
- PooledQueue<T> used in ObjectPool.cs:287 and GameObjectPoolBase.cs:133
- GlobalSettings uses ScriptableObject (Unity-specific), not usable as Core PoolSettings pattern
- Core asmdef: `noEngineReferences: true` — no UnityEngine types allowed in Core
- ObjectPoolManager.ClearAll() is private (line 305) — no public reset API
- GameWorld.Initialize() calls PoolManager.Tick() at GameWorld.cs:30
- ObjectPool<T>.Spawn() is `abstract void` while GameObjectPool.Spawn() is `bool Spawn(out GameObject)` — incompatible for template method
- Core.asmdef has `noEngineReferences: true` compiled as JSON (no "noEngineReferences" key) — effectively: no references list + no built-in unity references

## Decisions (with rationale)
- P3: GameObjectPool.Clear() does NOT destroy prototype — user chose option A
- P5: PoolSettings struct replaces Initialize overloads — user chose option A
- P6: Direct rename + fix all external references — user chose option B
- Test: Unity Test for all pool systems in Tests/ directory

## Scope IN
- 13 items: P1(Release clear), P2(ClearAll leak), P3(prototype destroy), P4(Tick template), P5(overload reduction), P6(naming), P7(as cast), P8(defense checks), P9(dead code), P10(typo), P11(namespace), P12(encapsulation), P13(init Tick)
- Unity Test coverage for: ObjectPool<T> lifecycle, ReferencePool acquire/release, GameObjectPool lifecycle, ObjectPoolManager create/remove/tick, PooledCollections basic ops

## Scope OUT (Must NOT have)
- No changes to ReferencePool core logic (already correct)
- No changes to PooledObject<T> Unity wrapper (already correct)
- No new external dependencies
- No API changes beyond P6 class renames and P5 Initialize signature changes
- No runtime behavior changes for correctly-used code

## Open questions
(None remaining — all resolved)

## Approval gate
status: approved — plan written to .omo/plans/fix-pool-systems.md
