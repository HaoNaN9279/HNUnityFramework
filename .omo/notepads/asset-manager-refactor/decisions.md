# Decisions: asset-manager-refactor

## 2026-07-02 - Architecture decisions (pre-execution)

### Decision 1: IAssetManager in Core returns void for LoadAsset
Rationale: Core assembly has noEngineReferences=true, cannot return UnityEngine.Object.
Loading progress is communicated via Action<float> callbacks.
The concrete AssetManager in Unity assembly wraps AsyncLoadHandle internally.

### Decision 2: AssetManager uses IAssetOperator internally
Rationale: Preserves existing abstraction for operator swapping (Editor->AssetDatabaseOperator, Runtime->AddressablesOperator).
Keeps backward compat with existing world.AssetOperator property.

### Decision 3: AssetCacheItem integrated via ReferencePool
Rationale: Already implements IAssetCacheItem : IReference. Use ReferencePool.Acquire/Release for allocation.

### Decision 4: Operator auto-select via compile-time #if
Rationale: AssetDatabaseOperator is wrapped in #if UNITY_EDITOR - doesn't exist at runtime.
Using #if UNITY_EDITOR in GameWorldDriver.Awake() is the only viable mechanism.

### Decision 5: Test internal members via InternalsVisibleTo
Rationale: AssetManager exposes internal PendingPreloadCount and SetAutoUnloadDelay for tests.
AssemblyInfo.cs in Unity assembly grants InternalsVisibleTo to test assembly.
