---
sidebar_position: 21
---

# Physics — 物理系统抽象层

## 概览

物理系统抽象层（C16）为游戏逻辑代码提供不依赖 Unity 物理 API 的接口层。通过 Core（纯 C# 接口 + 数据模型）和 Unity（PhysX 封装）两层分离，使上层逻辑可测试且平台无关。

## 架构

```
┌─────────────────────────────────────┐
│          Game Logic (L层)           │
├─────────────────────────────────────┤
│   IPhysicsWorld / IBody / 数据模型  │ ← Core 层接口
├─────────────────────────────────────┤
│   PhysXWorld / UnityBody            │ ← Unity 层实现
└─────────────────────────────────────┘
```

### Core 层 (`HN.Framework.Core.Capability.Physics`)

纯 C# 接口与数据模型，无 Unity 依赖。

### Unity 层 (`HN.Framework.Unity.Capability.Physics`)

基于 Unity Physics/Physics2D 的 PhysX 封装实现。

---

## 核心接口

### IPhysicsWorld

物理世界接口，负责管理刚体生命周期和执行物理查询。

```csharp
public interface IPhysicsWorld : IDisposable
{
    PhysicsDimension Dimension { get; }
    
    IBody CreateBody(ShapeDefinition shape, PhysicsVector3 position, 
        PhysicsQuaternion rotation, BodyType bodyType, string layer);
    void DestroyBody(IBody body);
    
    bool Raycast(PhysicsVector3 origin, PhysicsVector3 direction, 
        out RaycastHit hitInfo, float maxDistance, string layerMask);
    RaycastHit[] RaycastAll(PhysicsVector3 origin, PhysicsVector3 direction, 
        float maxDistance, string layerMask);
    RaycastHit[] OverlapSphere(PhysicsVector3 center, float radius, 
        string layerMask);
    RaycastHit[] OverlapBox(PhysicsVector3 center, PhysicsVector3 halfExtents, 
        PhysicsQuaternion rotation, string layerMask);
    
    void Step(float deltaTime);
}
```

### IBody

物理刚体接口，表示物理世界中的独立刚体。

```csharp
public interface IBody : IDisposable
{
    PhysicsVector3 Position { get; set; }
    PhysicsQuaternion Rotation { get; set; }
    PhysicsVector3 Velocity { get; set; }
    PhysicsVector3 AngularVelocity { get; set; }
    float Mass { get; set; }
    bool IsKinematic { get; set; }
    BodyType BodyType { get; set; }
    int InstanceId { get; }
    
    void AddForce(PhysicsVector3 force);
    void AddTorque(PhysicsVector3 torque);
    
    event Action<CollisionEvent>? OnCollisionEnter;
    event Action<CollisionEvent>? OnCollisionStay;
    event Action<CollisionEvent>? OnCollisionExit;
    event Action<CollisionEvent>? OnTriggerEnter;
    event Action<CollisionEvent>? OnTriggerStay;
    event Action<CollisionEvent>? OnTriggerExit;
}
```

---

## 数据模型

### PhysicsVector3

三维向量，纯 C# 实现（无 UnityEngine.Vector3 依赖）。

| 成员 | 描述 |
|------|------|
| `X, Y, Z` | 分量字段 |
| `Magnitude` | 向量长度 |
| `SqrMagnitude` | 向量长度平方 |
| `Normalized` | 归一化向量 |
| `Dot(a, b)` | 点积 |
| `Cross(a, b)` | 叉积 |
| `Distance(a, b)` | 距离 |
| `Lerp(a, b, t)` | 线性插值 |
| `+, -, *, /` | 算术运算符 |
| `==, !=` | 相等比较 |

### PhysicsQuaternion

四元数，纯 C# 实现。

| 成员 | 描述 |
|------|------|
| `X, Y, Z, W` | 分量字段 |
| `Identity` | 单位四元数 |
| `Euler(x, y, z)` | 欧拉角转四元数（ZYX 顺序） |
| `AngleAxis(angle, axis)` | 轴角转四元数 |
| `LookRotation(forward)` | 朝向旋转 |
| `*` | 四元数乘法 |

### RaycastHit

射线检测命中结果。

| 字段 | 类型 | 描述 |
|------|------|------|
| `Point` | PhysicsVector3 | 命中点 |
| `Normal` | PhysicsVector3 | 法线方向 |
| `Distance` | float | 命中距离 |
| `ColliderInstanceId` | int | 碰撞体实例 ID |

### CollisionEvent

碰撞事件。

| 字段 | 类型 | 描述 |
|------|------|------|
| `SelfInstanceId` | int | 自身实例 ID |
| `OtherInstanceId` | int | 对方实例 ID |
| `RelativeVelocity` | PhysicsVector3 | 相对速度 |
| `Type` | CollisionEventType | 事件类型 |

### ShapeDefinition 及派生类

碰撞形状定义，实现 `IReference`（可被引用池管理）。

| 类 | 属性 | 描述 |
|----|------|------|
| `BoxShape` | `HalfExtents` | 盒体半尺寸 |
| `SphereShape` | `Radius` | 球体半径 |
| `CapsuleShape` | `Radius, Height` | 胶囊体半径和高度 |

---

## 枚举

### PhysicsDimension

```csharp
public enum PhysicsDimension { D2, D3 }
```

### BodyType

```csharp
public enum BodyType { Static, Dynamic, Kinematic }
```

### CollisionEventType

```csharp
public enum CollisionEventType { Enter, Stay, Exit }
```

---

## Unity 层实现

### PhysXWorld

- 封装 `UnityEngine.Physics`（3D）和 `UnityEngine.Physics2D`（2D）
- 实现 `IPhysicsWorld` + `ITickable`
- 构造函数接受 `PhysicsDimension` 参数
- 创建刚体时自动添加对应 Collider（Box/Sphere/Capsule）
- Dispose 时清理所有创建的 GameObject

### UnityBody

- `MonoBehaviour` 实现 `IBody`
- 运行时自动检测 Rigidbody 或 Rigidbody2D
- 碰撞/触发事件通过 Unity 消息回调转发
- 提供内部转换辅助方法：`ToUnityVector3`、`FromUnityVector3` 等

---

## GameWorld 集成

在 `GameWorldDriver.Awake()` 中创建并注入：

```csharp
World.PhysicsWorld = new PhysXWorld(PhysicsDimension.D3);
```

在 `GameWorld.Tick()` 中自动调用 `(PhysicsWorld as ITickable)?.Tick()`
在 `GameWorldDriver.OnDestroy()` 中自动清理：`(World?.PhysicsWorld as IDisposable)?.Dispose()`

---

## 使用示例

### 创建刚体

```csharp
var world = new PhysXWorld(PhysicsDimension.D3);
var shape = new BoxShape(new PhysicsVector3(1f, 1f, 1f));
var body = world.CreateBody(
    shape,
    new PhysicsVector3(0f, 10f, 0f),
    PhysicsQuaternion.Identity,
    BodyType.Dynamic,
    "Default");
body.AddForce(new PhysicsVector3(0f, 500f, 0f));
```

### 射线检测

```csharp
bool hit = world.Raycast(
    new PhysicsVector3(0f, 0f, 0f),
    new PhysicsVector3(0f, 0f, 1f),
    out var hitInfo,
    100f,
    "Default");
if (hit)
{
    Debug.Log($"Hit at {hitInfo.Point}, distance: {hitInfo.Distance}");
}
```

### 碰撞事件订阅

```csharp
body.OnCollisionEnter += evt =>
{
    Debug.Log($"Collision between {evt.SelfInstanceId} and {evt.OtherInstanceId}");
};
```

---

## 测试

| 命名空间 | 测试类型 | 文件 |
|----------|----------|------|
| `HN.Framework.Core.Tests.Physics` | 单元测试（合约测试） | `Tests/HN.Framework.Core.Tests/Physics/` |
| `HN.Framework.Unity.Tests.Capability.Physics` | EditMode 测试 | `Tests/HN.Framework.Unity.Tests/Capability/Physics/` |

Core 测试通过 Mock 实现验证接口契约（无需 Unity 运行时）。
Unity 测试通过创建实际 GameObject 和 Rigidbody 验证 EditMode 下的基本功能。
