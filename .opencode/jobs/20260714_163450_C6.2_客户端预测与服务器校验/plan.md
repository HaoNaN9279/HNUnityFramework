# 计划：C6.2 客户端预测与服务器校验

## 概述

**目标**：封装 FishNet Prediction API，提供框架级的客户端预测与服务器校验能力，与现有 C6.1 状态同步（SyncedModel → PropertyBinder → View）链路无缝集成。

**非目标**：
- 不自建预测/回滚引擎（FishNet 已提供完整 Prediction 系统）
- 不重新实现 Lag Compensation（FishNet ColliderRollback 已就绪）
- 不修改 FishNet Vendor 源码

---

## 上下文分析

### 代码库成熟度

**过渡型（Transitional）** — 框架核心基础设施（GameWorld、MVC、EventBus、ReferencePool）已规范化，但 C6 网络模块刚完成 C6.1 状态同步（✅），C6.2 为全新功能。FishNet v4.7.2 的 Prediction API 是成熟稳定的外部依赖。

### 关键发现

1. **FishNet Prediction API 功能完备**：`[Replicate]`/`[Reconcile]` 特性驱动代码生成、`IReplicateData`/`IReconcileData` 数据接口、`PredictionManager` 管理预测生命周期、`PredictionRigidbody` 物理预测、`ColliderRollback` 延时补偿。框架**无需重复实现**预测循环。

2. **现有集成点**：
   - `NetworkEntityView` (`HN.Framework.Unity.Capability.Network`) — 继承 FishNet.NetworkBehaviour，提供 SyncedModel 注册 + ApplySyncValue 辅助
   - `FishNetSerializerAdapter` — MemoryPack 已注册为 FishNet Custom Serializer
   - `FishNetMessageBus` — 消息收发基础设施
   - `SyncedModel<T>` (Core) — 线程安全同步数据模型
   - `Entity` + `EntityManager` (Core L3) / `ViewFactory` (Unity V2) — 实体生命周期

3. **架构文档指定策略**（`架构~/最终架构.md` C6.2 节）：
   > 使用 FishNet 内置 Prediction 系统，不自建。集成流程：客户端输入 → ServerRpc 发送 + 立即本地预测 → 服务端权威计算 → ObserversRpc 下发矫正 → FishNet Reconciliation 自动回滚/重放。MemoryPack 集成：预测状态 + Reconciliation 快照使用 MemoryPack 序列化/恢复。

4. **框架层的价值**：不是重复 FishNet 功能，而是提供：
   - Core 层定义预测抽象（接口/数据模型），保持 Core 的纯 C# 特性
   - Unity 层提供便利基类和适配器，统一 API 风格
   - 与 SyncedModel / Entity / Input 系统的无缝衔接
   - Lag Compensation 的便利封装

5. **[已确认] FishNet 代码生成限制**：`[Replicate]`/`[Reconcile]` 特性必须标记在**非抽象、非虚方法**上才能被 FishNet 代码生成器正确处理。因此框架基类**不能**直接提供带特性的虚方法，而应采用**辅助方法模式**：基类提供 `CreateReplicateData<T>()` / `ApplyReconcileData<T>()` 等辅助方法，由业务子类声明具体的 `[Replicate]`/`[Reconcile]` 方法后调用。

---

## 任务分解

### Wave 1（无依赖，可并行）

| ID | 任务 | 描述 | 委派建议 | 验收标准 |
|----|------|------|----------|----------|
| T1 | Core 层预测数据模型 | 在 `HN.Framework.Core.Capability.Network.Prediction` 命名空间创建 `PredictionInputBase` 抽象基类（实现 IReference + MemoryPackable，含 Tick 属性）、`PredictionReconcileData<T>` 泛型调和数据结构体（unmanaged 约束，MemoryPackable） | Sisyphus-junior | 编译通过；类型实现 MemoryPackable 且不引入 UnityEngine 依赖 |
| T2 | Core 层 IPredictedEntity 接口 | 定义 `IPredictedEntity` 接口：`void Simulate(PredictionInputBase input)` / `void Reconcile<T>(PredictionReconcileData<T> data) where T : unmanaged` / `uint GetLastProcessedTick()` | Sisyphus-junior | 接口编译通过；无 Unity 依赖 |
| T3 | Unity 层 PredictedNetworkEntityView 基类 | 在 `HN.Framework.Unity.Capability.Network.Prediction` 命名空间创建 `PredictedNetworkEntityView` 抽象类，继承 `NetworkEntityView`。采用**辅助方法模式**（不直接贴 `[Replicate]`/`[Reconcile]`）：提供 `CreateReplicateData<T>()` / `StoreReplicateInput(PredictionInputBase)` / `GetReconcileInputs(uint fromTick, uint toTick)` / `ApplyReconcileState<T>(PredictionReconcileData<T>)` 等辅助方法；内建输入历史环形缓冲区（容量 = TickRate × 5）；集成 SyncedModel 用于权威状态展示 | Sisyphus-junior | 编译通过；正确继承 NetworkEntityView 并复用 SyncedModel 注册；辅助方法签名清晰，可直接被业务子类的 `[Replicate]`/`[Reconcile]` 方法调用 |
| T4 | Unity 层 PredictionManagerAdapter | 封装 FishNet `PredictionManager` 访问，暴露 `IsReconciling`、`ClientReplayTick`、`ServerReplayTick`、`StateInterpolation` 等属性，提供 `OnPreReconcile`/`OnPostReconcile` 事件转发 | Sisyphus-junior | 编译通过；通过 FishNetNetworkManager 注入 |
| T5 | Unity 层 LagCompensationAdapter | 封装 FishNet `RollbackManager` + `ColliderRollback`，提供 `RollbackRaycast(PreciseTick, Vector3, Vector3, float, int, out RaycastHit)` / `RollbackSphereCast(PreciseTick, ...)` / `RollbackOverlapSphere(PreciseTick, ...)` 便利方法，自动管理回滚/恢复。首版仅支持 3D 物理 | Sisyphus-junior | 编译通过；API 清晰隐藏 FishNet 内部类型暴露 |

### Wave 2（依赖 Wave 1）

| ID | 任务 | 描述 | 委派建议 | 验收标准 |
|----|------|------|----------|----------|
| T6 | FishNetNetworkManager 扩展 | 在 `FishNetNetworkManager` 中新增 `PredictionManager` 访问属性、创建并注入 `PredictionManagerAdapter`、暴露网络 Tick 信息（RoundTripTime、TickRate），更新 `GameWorldDriver` 注入链路使 GameWorld 可通过 NetworkManager 访问预测管理器 | Sisyphus-junior | 编译通过；GameWorld 可访问网络预测状态 |
| T7 | PredictionInputBase MemoryPack 格式化器 | 在 `HN.Framework.Core.Capability.Network.Prediction` 创建泛型格式化器 `PredictionInputFormatter<T>`（支持 MemoryPackUnion 多态序列化），注册到 `MemoryPackFormatterProvider`；更新 `FishNetSerializerAdapter.RegisterAllKnownTypes()` 注册预测相关类型 | Sisyphus-junior | 编译通过；MemoryPack 可正确序列化/反序列化 PredictionInputBase 派生类型 |

### Wave 3（依赖 Wave 2，集成验证 + 文档）

| ID | 任务 | 描述 | 委派建议 | 验收标准 |
|----|------|------|----------|----------|
| T8A | 预测核心逻辑单元测试（EditMode，Mock） | 在 `HN.Framework.Unity.Tests` 编写 Mock 版单元测试：验证 PredictedNetworkEntityView 的输入缓冲区（StoreReplicateInput/GetReconcileInputs）、CreateReplicateData 分配/回收、辅助方法边界条件、SyncedModel 集成 | Sisyphus-junior | EditMode 测试全部通过（不依赖 FishNet 运行时） |
| T8B | 预测流程集成测试（PlayMode，需 Unity MCP） | 编写 PlayMode 集成测试：验证完整的客户端预测→服务端校验→调和回滚流程（需要 FishNet NetworkManager 运行时） | Sisyphus-junior | PlayMode 测试全部通过（通过 Unity MCP 在 Editor 中运行） |
| T9 | Lag Compensation 集成测试 | 编写 `LagCompensationAdapterTests`：验证 RollbackManager 封装、Raycast/Overlap 回滚 API 正确性。使用 Mock RollbackManager 做 EditMode 测试 | Sisyphus-junior | EditMode 测试全部通过 |
| T10 | 更新架构文档 | 更新 `架构~/最终架构.md` 中 C6.2 状态从 📋 → ✅；更新 C6 网络系统章节补充预测与校验详细设计；更新 `架构~/开发优先级.md` C6.2 状态 | Sisyphus-junior | 文档内容与实现一致 |
| T11 | 更新 docs-site 文档 | 更新 `docs-site~/docs/guide/network.md`（C6.2 完成状态 + 使用指南）、`docs-site~/docs/api/index.md`（新增 API）、`docs-site~/docs/dev/architecture.md`（C6 模块状态同步）、`docs-site~/docs/guide/index.md`（C6 状态更新） | Sisyphus-junior | 4 个文档文件更新完毕，内容准确 |

---

## 依赖图

T1[Core: 预测数据模型] → T7[MemoryPack 格式化器]
T2[Core: IPredictedEntity 接口] → T3[Unity: PredictedNetworkEntityView]
T1 → T3
T3 → T6[FishNetNetworkManager 扩展]
T4[PredictionManagerAdapter] → T6
T5[LagCompensationAdapter] → T9[Lag Compensation 测试]
T6 → T8A[预测单元测试 EditMode]
T6 → T8B[预测集成测试 PlayMode]
T3 → T8A
T3 → T8B
T7 → T8A
T5 → T9
T8A → T10[更新架构文档]
T8B → T10
T9 → T11[更新 docs-site]
T10 → T11

### Wave 分组

- **Wave 1**：T1、T2、T3、T4、T5 — 全部无相互依赖，可并行执行
- **Wave 2**：T6（依赖 T3 + T4）、T7（依赖 T1）— 可并行
- **Wave 3**：T8A（依赖 T3 + T6 + T7）、T8B（依赖 T3 + T6）、T9（依赖 T5）— 可部分并行；T10 → T11 串行

---

## 详细设计

### 1. Core 层数据模型设计（T1, T2）

**命名空间**：`HN.Framework.Core.Capability.Network.Prediction`

**PredictionInputBase.cs**
- 抽象基类，同时实现 `IReference`（池复用）和 MemoryPackable（序列化）
- `[MemoryPackOrder(0)] public uint Tick { get; set; }` — 关联的网络 Tick
- `abstract void Clear()` — IReference 接口实现
- 业务子类添加自身字段并用 `[MemoryPackOrder(n)]` 标注

**PredictionReconcileData.cs**
- MemoryPackable 泛型结构体，约束 `T : unmanaged`
- 字段：`ClientTick`(uint) / `ServerTick`(uint) / `AuthoritativeState`(T)
- 服务端计算的权威状态快照，用于客户端回滚

**IPredictedEntity.cs**
- 接口定义：`Simulate(PredictionInputBase)` / `Reconcile<T>(PredictionReconcileData<T>)` / `GetLastProcessedTick()`
- 由 Core 层 Model/Controller 实现，定义预测实体的行为契约

### 2. Unity 层 PredictedNetworkEntityView 设计（T3）

> **关键设计决策**（已修复 Momus 审核阻塞点）：基类采用**辅助方法模式**，不在基类上贴 `[Replicate]`/`[Reconcile]` 特性。FishNet 代码生成器要求特性标记在非抽象、非虚的具体方法上。

**命名空间**：`HN.Framework.Unity.Capability.Network.Prediction`
**继承链**：`PredictedNetworkEntityView` → `NetworkEntityView` → `FishNet.NetworkBehaviour`

**辅助方法清单**（供业务子类调用）：

| 方法 | 用途 |
|------|------|
| `CreateReplicateData<T>(uint tick)` | 从对象池获取 `T` 实例（T : PredictionInputBase, new()），设置 Tick |
| `ReleaseReplicateData<T>(T data)` | 归还到对象池 |
| `StoreReplicateInput(PredictionInputBase)` | 将输入存入环形缓冲区，用于后续调和重放 |
| `GetReconcileInputs(uint fromTick, uint toTick)` | 从缓冲区检索指定 Tick 范围的历史输入列表 |
| `ApplyReconcileState<T>(PredictionReconcileData<T>)` | 应用服务端权威状态，驱动关联的 SyncedModel 更新 |
| `GetNetworkTickInfo()` | 获取当前网络 Tick / RTT 等时序信息 |

**缓冲区设计**：
- `RingBuffer<PredictionInputBase>`，容量 = `TimeManager.TickRate * 5`（约 150 条目 @ 30Hz）
- 自动淘汰过期条目（Tick < 当前 - 容量）
- O(1) 写入，O(log n) Tick 查询

**业务子类使用示例**：
```csharp
public class PlayerPredictedView : PredictedNetworkEntityView
{
    [Replicate]
    private void OnReplicate(PlayerInput input, ReplicateState state, Channel channel)
    {
        // 1. 从池中获取输入数据
        var data = CreateReplicateData<PlayerInput>(input.Tick);
        data.MoveDirection = input.MoveDirection;
        // 2. 立即本地预测（乐观执行）
        SimulateMovement(data);
        // 3. 存入缓冲区等待调和
        StoreReplicateInput(data);
    }

    [Reconcile]
    private void OnReconcile(PlayerState state, Channel channel)
    {
        // 1. 应用服务端权威状态
        ApplyReconcileState(state);
        // 2. 重放未确认输入
        var pendingInputs = GetReconcileInputs(state.ClientTick, state.ServerTick);
        foreach (var input in pendingInputs)
            SimulateMovement(input);
    }
}
```

### 3. PredictionManagerAdapter 设计（T4）

```
PredictionManagerAdapter（纯 C# 类，通过 FishNetNetworkManager 持有并注入）
  ├── IsReconciling / ClientReplayTick / ServerReplayTick
  ├── StateInterpolation / RedundancyCount
  ├── event OnPreReconcile(uint clientTick, uint serverTick)
  ├── event OnPostReconcile(uint clientTick, uint serverTick)
  ├── event OnPreReplicateReplay(uint clientTick, uint serverTick)
  ├── event OnPostReplicateReplay(uint clientTick, uint serverTick)
  └── GetNetworkTickInfo() → (uint remoteTick, long rtt, ushort tickRate)
```

### 4. LagCompensationAdapter 设计（T5）

```
LagCompensationAdapter（纯 C# 类，持有 RollbackManager 引用）
  ├── bool RollbackRaycast(PreciseTick tick, Vector3 origin, Vector3 dir,
  │                        float dist, int layerMask, out RaycastHit hit)
  ├── bool RollbackSphereCast(PreciseTick tick, Vector3 origin, float radius,
  │                           Vector3 dir, float dist, int layerMask, out RaycastHit hit)
  ├── int  RollbackOverlapSphere(PreciseTick tick, Vector3 center, float radius,
  │                              int layerMask, Collider[] results)
  └── void Return() // 恢复碰撞体到原始位置
```

> 首版仅支持 3D 物理。2D 物理回滚按需在后续版本扩展。

---

## 文件变更清单

### Core 层新增文件（`Runtime/HN.Framework.Core/Capability/Network/Prediction/`）

| 文件 | 说明 | 所属任务 |
|------|------|----------|
| `PredictionInputBase.cs` | 预测输入基类（MemoryPackable + IReference） | T1 |
| `PredictionReconcileData.cs` | 调和数据泛型结构体 | T1 |
| `IPredictedEntity.cs` | 预测实体接口 | T2 |
| `PredictionInputFormatter.cs` | MemoryPack 多态格式化器 | T7 |

### Unity 层新增文件（`Runtime/HN.Framework.Unity/Capability/Network/Prediction/`）

| 文件 | 说明 | 所属任务 |
|------|------|----------|
| `PredictedNetworkEntityView.cs` | 客户端预测网络实体视图基类 | T3 |
| `PredictionManagerAdapter.cs` | FishNet PredictionManager 封装 | T4 |
| `LagCompensationAdapter.cs` | FishNet ColliderRollback 封装 | T5 |

### 修改文件

| 文件 | 变更 | 所属任务 |
|------|------|----------|
| `FishNetNetworkManager.cs` | 新增 PredictionManager 访问 + PredictionManagerAdapter 注入 | T6 |
| `FishNetSerializerAdapter.cs` | 扩展 RegisterAllKnownTypes 注册预测类型 | T7 |
| `GameWorldDriver.cs` | 更新网络注入链路（如需要） | T6 |

### 测试新增文件

| 文件 | 说明 | 所属任务 |
|------|------|----------|
| `PredictedEntityViewTests.cs` | EditMode 单元测试（Mock） | T8A |
| `PredictedEntityViewIntegrationTests.cs` | PlayMode 集成测试 | T8B |
| `LagCompensationAdapterTests.cs` | EditMode 单元测试（Mock RollbackManager） | T9 |

### 文档变更

| 文件 | 变更 | 所属任务 |
|------|------|----------|
| `架构~/最终架构.md` | C6.2 状态 📋→✅；补充预测与校验详细设计 | T10 |
| `架构~/开发优先级.md` | C6.2 状态更新 | T10 |
| `docs-site~/docs/guide/network.md` | C6.2 完成标记 + 使用指南 + 代码示例 | T11 |
| `docs-site~/docs/api/index.md` | 新增预测 API 条目 | T11 |
| `docs-site~/docs/dev/architecture.md` | C6 模块状态同步更新 | T11 |
| `docs-site~/docs/guide/index.md` | 网络模块状态更新 | T11 |

---

## 风险与缓解

| 风险 | 可能性 | 影响 | 缓解策略 |
|------|--------|------|----------|
| FishNet 代码生成器不支持基类 `[Replicate]`/`[Reconcile]` 虚方法（已确认限制） | ~~中~~ → **已确认** | 高 | ✅ 已采用辅助方法模式（T3 详细设计）：基类提供 `CreateReplicateData`/`GetReconcileInputs`/`ApplyReconcileState` 等辅助方法，业务子类声明具体 `[Replicate]`/`[Reconcile]` 方法后调用 |
| `PredictionInputBase` 作为 MemoryPackable 抽象基类，派生类的序列化需确保多态支持 | 低 | 中 | 使用 `[MemoryPackUnion]` 多态序列化机制 + `PredictionInputFormatter<T>` 泛型格式化器（T7） |
| PlayMode 集成测试（T8B）可能在当前测试环境中无法运行 FishNet Prediction 完整流程 | 中 | 低 | T8A 使用 Mock 覆盖核心逻辑；T8B 作为可选验证步骤，标注为需要完整 FishNet 运行时环境 |
| `LagCompensationAdapter` 需要直接引用 FishNet `RollbackManager` 内部类型 | 高 | 中 | 封装在适配器内部，暴露框架自有 API；FishNet 版本升级时仅需修改适配器 |
| C6.2 的 MemoryPack 序列化格式可能与 FishNet 未来版本的内置序列化冲突 | 低 | 低 | 通过 `FishNetSerializerAdapter` 统一管理序列化注册，升级时仅需更新适配器 |

---

## 关键决策点（需用户确认）

1. **封装策略（已确定）**：框架基类采用**辅助方法模式**，不直接贴 `[Replicate]`/`[Reconcile]` 特性，由业务子类声明具体方法后调用辅助方法。这意味着 C6.2 定位为"SDK 集成框架 + 便利工具"，而非完全透明的抽象层。

2. **Lag Compensation 范围**：当前计划仅封装 3D 物理回滚（Raycast/SphereCast/Overlap）。2D 物理回滚是否需要在首版实现？（建议按需扩展到后续版本）

3. **PredictionManagerAdapter 生命周期**：由 `FishNetNetworkManager` 持有并注入 GameWorld，还是由 GameWorld 作为独立服务注入？（建议前者，与 FishNet 生命周期绑定）