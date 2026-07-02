---
slug: zhihu-arch-refactor
status: approved-revised
intent: clear
approach: 6-stage refactoring per 重构规划.md, WITH code-level fixes for Unity dependencies
decisions-revised:
  - ObjectPoolManager split: pure-C# pool registry in HN.Framework, GameObject root/ObjectPoolViewer/GameObjectPool factory in GameWorldDriver
  - Serialize.cs→Json.cs: replace JsonUtility with System.Text.Json (pure C#)
  - JsonData.cs→HN.Framework.Unity (uses Unity ISerializationCallbackReceiver)
  - HNDictionary+SerializableDictionary→HN.Framework.Unity (Unity serialization)
  - SheetElementTypeAttribute→HN.Framework.Unity (typeof(GameObject) etc.)
  - IAssetOperator.cs: change UnityEngine.Object→System.Object in interface
  - PoolBase.cs split: PoolBase+ObjectPoolBase→HN.Framework, GameObjectPoolBase→HN.Framework.Unity
  - Debug.Log→ILogProvider in ControllerManager/ProcedureManager/HFSM/ReferencePool
  - HN.Framework.asmdef move to Runtime/HN.Framework/HN.Framework.asmdef
  - HNUnityFramework.cs: delete after code extraction to GameWorld+GameWorldDriver
  - Add missing placeholders: ConnectionMessages.cs, PlayerActionMessages.cs, SRP rendering files
scope-in: 重构规划.md §3.1 + §4.1 full coverage, with corrected assembly assignments
scope-out: no FishNet/Luban downloads; no feature impl for placeholders
---

# Draft: zhihu-arch-refactor (REVISED after dual-Momus review)
## Momus findings addressed
1. Unity dep files → corrected assembly assignment or code-level fix
2. asmdef move → explicit task added
3. Missing placeholders → added SRP, network messages
4. Debug.Log removal → ILogProvider injection
5. HNUnityFramework.cs fate → explicit deletion after extraction
