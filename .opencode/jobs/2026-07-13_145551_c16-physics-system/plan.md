# 计划：C16 — 物理系统抽象层

## 概述

为 HNUnityFramework 构建物理系统抽象层，分 Core（纯 C# 接口 + 数据模型）和 Unity（PhysX 封装）两层。目标是让游戏逻辑代码无需直接依赖 UnityEngine.Physics/Physics2D，同时支持 2D/3D 统一接口。

**非目标**：Joint/Constraint、CharacterController、软体物理、自定义物理引擎集成。

## 上下文分析

### 代码库成熟度：纪律型（Disciplined）

项目具有清晰的架构文档、一致的命名空间规范、统一的模块实现模式。C14 Input 模块提供了完美的实现模板。应严格遵循现有模式。

### 关键发现

| 方面 | 发现 |
|------|------|
| **模板参考** | C14 Input 模块（Core 5 文件 + Unity 5 文件） |
| **Core 约束** | noEngineReferences: true — 不能使用 Vector3/Quaternion 等 Unity 类型 |
| **数学类型** | Core 无通用 Vector3 类型，需在 Physics 模块内定义 PhysicsVector3/PhysicsQuaternion |
| **GameWorld 集成** | 属性注入 + (PhysicsWorld as ITickable)?.Tick() 模式 |
| **MCP 串行约束** | ⚠️ 所有 .cs 文件的创建/修改必须通过 Unity MCP，同一时间只允许一个 MCP 操作 |

### 架构设计关键决策

| 决策 | 方案 |
|------|------|
| **接口范围** | IPhysicsWorld(CreateBody/DestroyBody/Raycast/RaycastAll/OverlapSphere/OverlapBox/Step) + IBody(Position/Rotation/Velocity/AngularVelocity/Mass/IsKinematic/BodyType/AddForce/AddTorque + 碰撞/触发回调) |
| **Shape 设计** | Core 纯数据结构 → Unity 层 ColliderFactory 工厂方法创建对应 Collider |
| **2D/3D 统一** | PhysicsDimension 枚举参数，PhysXWorld 内部按维度委托 |
| **物理层抽象** | Core 接口使用 string 层名称，Unity 层内部通过 LayerMask.NameToLayer() 转换 |

---
## 任务分解

> **MCP串行约束**: 所有通过Unity MCP创建/修改.cs文件的任务不可并行。计划按MCP操作批次分组，
> 每个Phase内通过batch_execute一次调用完成多个文件操作，Phase间严格串行。
> 仅文档更新任务(Phase 6)不依赖MCP，可并行。

### Phase 1 — 批量创建Core层文件(单次MCP batch_execute)

| ID | 任务 | 描述 | 委派 | 验收标准 |
|----|------|------|------|----------|
| **P1** | 批量创建Core Physics全部8个文件 | 使用MCP batch_execute一次性创建：PhysicsTypes.cs(PhysicsDimension/BodyType枚举)、PhysicsVector3.cs(readonly struct X/Y/Z)、PhysicsQuaternion.cs(readonly struct X/Y/Z/W)、RaycastHit.cs(readonly struct)、CollisionEvent.cs(readonly struct)、ShapeDefinition.cs(BoxShape/SphereShape/CapsuleShape实现IReference)、IBody.cs(接口)、IPhysicsWorld.cs(接口)。所有文件完整XML文档注释。 | Sisyphus-Junior | 全部8个文件创建成功；编译通过 |

新建文件路径:
- Runtime/HN.Framework.Core/Capability/Physics/PhysicsTypes.cs
- Runtime/HN.Framework.Core/Capability/Physics/PhysicsVector3.cs
- Runtime/HN.Framework.Core/Capability/Physics/PhysicsQuaternion.cs
- Runtime/HN.Framework.Core/Capability/Physics/RaycastHit.cs
- Runtime/HN.Framework.Core/Capability/Physics/CollisionEvent.cs
- Runtime/HN.Framework.Core/Capability/Physics/ShapeDefinition.cs
- Runtime/HN.Framework.Core/Capability/Physics/IBody.cs
- Runtime/HN.Framework.Core/Capability/Physics/IPhysicsWorld.cs

### Phase 2 — 批量创建Core层测试文件(单次MCP batch_execute)

| ID | 任务 | 描述 | 委派 | 验收标准 |
|----|------|------|------|----------|
| **P2** | 批量创建Core Physics全部7个测试文件 | 使用MCP batch_execute一次性创建：PhysicsTypesTests.cs、PhysicsVector3Tests.cs、RaycastHitTests.cs、CollisionEventTests.cs、ShapeDefinitionTests.cs、IBodyContractTests.cs(含MockBody内部类)、IPhysicsWorldContractTests.cs(含MockPhysicsWorld内部类)。遵循项目测试规范(#nullable enable、[TestFixture]、[SetUp]/[TearDown]、NUnit断言、命名空间HN.Framework.Core.Tests.Physics)。 | Sisyphus-Junior | 全部7个文件创建成功；命名空间正确 |

新建文件路径:
- Tests/HN.Framework.Core.Tests/Physics/PhysicsTypesTests.cs
- Tests/HN.Framework.Core.Tests/Physics/PhysicsVector3Tests.cs
- Tests/HN.Framework.Core.Tests/Physics/RaycastHitTests.cs
- Tests/HN.Framework.Core.Tests/Physics/CollisionEventTests.cs
- Tests/HN.Framework.Core.Tests/Physics/ShapeDefinitionTests.cs
- Tests/HN.Framework.Core.Tests/Physics/IBodyContractTests.cs
- Tests/HN.Framework.Core.Tests/Physics/IPhysicsWorldContractTests.cs

### Phase 3 — 运行Core层测试(验证数据模型与接口契约)

| ID | 任务 | 描述 | 委派 | 验收标准 |
|----|------|------|------|----------|
| **P3** | 运行Core Physics测试 | 通过MCP run_tests运行HN.Framework.Core.Tests程序集中的Physics相关测试(test_names参数过滤)。确认全部通过。如有失败则进入P1/P2修复循环。 | Sisyphus-Junior | 所有Core Physics测试通过(预计~50个测试用例) |

### Phase 4 — 创建Unity层文件 + GameWorld集成(单次MCP batch_execute)

| ID | 任务 | 描述 | 委派 | 验收标准 |
|----|------|------|------|----------|
| **P4** | 批量创建UnityBody.cs + PhysXWorld.cs并修改GameWorld | 使用MCP batch_execute一次性：(1)创建UnityBody.cs(MonoBehaviour:IBody,持有Rigidbody/Rigidbody2D,实现Position/Rotation/Velocity/AddForce/AddTorque/碰撞回调转发,IDisposable); (2)创建PhysXWorld.cs(实现IPhysicsWorld+ITickable,接受PhysicsDimension参数,封装Physics/Physics2D的Raycast/Overlap/Simulate,Vector3<>PhysicsVector3转换,IDisposable); (3)修改GameWorld.cs添加IPhysicsWorld? PhysicsWorld属性+Tick集成; (4)修改GameWorldDriver.cs添加PhysXWorld创建注入+OnDestroy中Dispose。 | Sisyphus-Junior | 2新文件+2修改文件编译通过；GameWorld API无破坏性变更 |

新建文件:
- Runtime/HN.Framework.Unity/Capability/Physics/UnityBody.cs
- Runtime/HN.Framework.Unity/Capability/Physics/PhysXWorld.cs

修改文件:
- Runtime/HN.Framework.Core/Driver/GameWorld/GameWorld.cs (添加PhysicsWorld属性)
- Runtime/HN.Framework.Unity/Driver/Platform/GameWorldDriver.cs (添加PhysXWorld创建/注入/Dispose)

### Phase 5 — 创建并运行Unity层测试(两次MCP操作串行)

| ID | 任务 | 描述 | 委派 | 验收标准 |
|----|------|------|------|----------|
| **P5a** | 批量创建Unity Physics测试文件 | 使用MCP batch_execute创建：UnityBodyTests.cs(EditMode,覆盖Position/Velocity/Mass/AddForce/IsKinematic/BodyType)、PhysXWorldTests.cs(EditMode,覆盖Raycast/OverlapSphere/CreateBody/DestroyBody/2D+3D双模式)。遵循项目测试规范：HideFlags.HideAndDontSave、DestroyImmediate、命名空间HN.Framework.Unity.Tests.Capability.Physics。 | Sisyphus-Junior | 2个测试文件创建成功；编译通过 |
| **P5b** | 运行Unity Physics测试 | 通过MCP run_tests运行HN.Framework.Unity.Tests中的Physics测试。确认全部通过。如有失败则修复。 | Sisyphus-Junior | 所有Unity Physics测试通过(预计~22个测试用例) |

新建文件:
- Tests/HN.Framework.Unity.Tests/Capability/Physics/UnityBodyTests.cs
- Tests/HN.Framework.Unity.Tests/Capability/Physics/PhysXWorldTests.cs

### Phase 6 — 文档更新(不依赖MCP，可并行)

| ID | 任务 | 描述 | 委派 | 验收标准 |
|----|------|------|------|----------|
| **P6a** | 更新架构文档 | 修改架构~/最终架构.md：将C16状态从"规划中"改为"已实现"；更新目录结构附录反映新增文件。使用bash操作(非MCP)。 | Sisyphus-Junior | 文档中C16状态正确 |
| **P6b** | 创建API文档 | 创建docs/api/physics.md，记录IPhysicsWorld、IBody、RaycastHit、CollisionEvent、ShapeDefinition、PhysicsVector3、PhysicsQuaternion的API与使用示例。使用bash操作(非MCP)。 | Sisyphus-Junior | API文档覆盖所有公共类型 |

---

## 依赖图

```
P1 → P2 → P3 → P4 → P5a → P5b → (P6a || P6b)

全部串行：每个Phase依赖前一个完成。
Phase 6内部(P6a和P6b)可并行(都不依赖MCP)。
```

## 文件清单

### 新建文件(19个)

| 文件 | 层级 | Phase |
|------|:---:|:---:|
| Runtime/HN.Framework.Core/Capability/Physics/PhysicsTypes.cs | Core | P1 |
| Runtime/HN.Framework.Core/Capability/Physics/PhysicsVector3.cs | Core | P1 |
| Runtime/HN.Framework.Core/Capability/Physics/PhysicsQuaternion.cs | Core | P1 |
| Runtime/HN.Framework.Core/Capability/Physics/RaycastHit.cs | Core | P1 |
| Runtime/HN.Framework.Core/Capability/Physics/CollisionEvent.cs | Core | P1 |
| Runtime/HN.Framework.Core/Capability/Physics/ShapeDefinition.cs | Core | P1 |
| Runtime/HN.Framework.Core/Capability/Physics/IBody.cs | Core | P1 |
| Runtime/HN.Framework.Core/Capability/Physics/IPhysicsWorld.cs | Core | P1 |
| Tests/HN.Framework.Core.Tests/Physics/PhysicsTypesTests.cs | Test | P2 |
| Tests/HN.Framework.Core.Tests/Physics/PhysicsVector3Tests.cs | Test | P2 |
| Tests/HN.Framework.Core.Tests/Physics/RaycastHitTests.cs | Test | P2 |
| Tests/HN.Framework.Core.Tests/Physics/CollisionEventTests.cs | Test | P2 |
| Tests/HN.Framework.Core.Tests/Physics/ShapeDefinitionTests.cs | Test | P2 |
| Tests/HN.Framework.Core.Tests/Physics/IBodyContractTests.cs | Test | P2 |
| Tests/HN.Framework.Core.Tests/Physics/IPhysicsWorldContractTests.cs | Test | P2 |
| Runtime/HN.Framework.Unity/Capability/Physics/UnityBody.cs | Unity | P4 |
| Runtime/HN.Framework.Unity/Capability/Physics/PhysXWorld.cs | Unity | P4 |
| Tests/HN.Framework.Unity.Tests/Capability/Physics/UnityBodyTests.cs | Test | P5a |
| Tests/HN.Framework.Unity.Tests/Capability/Physics/PhysXWorldTests.cs | Test | P5a |

### 修改文件(4个)

| 文件 | 修改内容 | Phase |
|------|----------|:---:|
| Runtime/HN.Framework.Core/Driver/GameWorld/GameWorld.cs | 添加IPhysicsWorld? PhysicsWorld属性+Tick集成 | P4 |
| Runtime/HN.Framework.Unity/Driver/Platform/GameWorldDriver.cs | 添加PhysXWorld创建/注入/Dispose | P4 |
| 架构~/最终架构.md | 更新C16状态+目录结构 | P6a |
| docs/api/physics.md | 新建API文档 | P6b |

## 风险与缓解

| 风险 | 可能性 | 影响 | 缓解策略 |
|------|--------|------|----------|
| Core层缺少通用Vector3/Quaternion类型 | 中 | 中 | 在Physics模块内定义最小PhysicsVector3/PhysicsQuaternion；后续可迁移到D2共享Math |
| Unity EditMode测试中Rigidbody行为受限 | 低 | 低 | EditMode下基本功能可用；如有限制则降级为PlayMode或简化断言 |
| MCP batch_execute单次调用文件数限制 | 低 | 低 | 上限25个，P1创建8个远未触及 |
| PhysXWorld.Step()与Unity自动物理步进冲突 | 中 | 中 | 仅在Physics.autoSimulation=false时可用Step()；文档明确说明 |
| 测试覆盖率不足(纯接口层) | 低 | 低 | Core通过合约测试(Mock)验证接口契约；Unity通过EditMode测试验证实现 |

---

> **计划状态**: 待用户批准
> **总Phase数**: 6(全部串行)
> **策略**: TDD — Core测试先行(P2→P3)，确保接口契约正确后再进入Unity实现(P4→P5)
