---
sidebar_position: 16
---

# 网络

网络层基于 Core 层接口 + Unity 层 FishNet 封装的设计，Core 层仅定义协议，FishNet 专有依赖隔离在 Unity 层。

## 架构

```
┌───────────────────────────────────────────────┐
│  HN.Framework.Core                            │
│  INetworkManager      MessageBase 协议         │
│  接口定义 + 连接事件   消息序列化+池复用         │
└───────────────────┬───────────────────────────┘
                    │ 纯 C# 接口
┌───────────────────┴───────────────────────────┐
│  HN.Framework.Unity                           │
│  FishNetNetworkManager    FishNet 封装          │
│  NetworkEntityView        NetworkBehaviour 子类 │
│  FishNetMessageBus        消息路由              │
│  FishNetConnectionAdapter 连接适配              │
│  FishNetSerializerAdapter MemoryPack 注入       │
└───────────────────────────────────────────────┘
```

## INetworkManager

Core 层抽象接口（`HN.Framework.Core.Capability.Network`），定义：

| 成员 | 类型 | 说明 |
|------|------|------|
| IsServer | bool | 是否已启动服务端 |
| IsClient | bool | 是否已连接为客户端 |
| IsHost | bool | 是否同时为 Host |
| StartServer() | void | 启动服务端 |
| StartClient() | void | 连接服务端 |
| StopConnection() | void | 断开所有连接 |
| Connect() | void | 兼容旧 API |
| Disconnect() | void | 兼容旧 API |
| OnClientConnected | event | 客户端连接事件 |
| OnClientDisconnected | event | 客户端断开事件 |

## FishNet 封装

FishNetNetworkManager 是纯 C# 包装类（不继承 NetworkBehaviour），持有 FishNet.NetworkManager 引用，通过构造注入。NetworkEntityView 继承 FishNet.NetworkBehaviour，作为所有需要网络同步的实体视图基类。

## 消息协议

MessageBase 是抽象基类，标记 `[MemoryPackable]`，实现 `IReference` 接口以支持 ReferencePool 池复用。FishNetMessageBus 通过内部 `IBroadcast` 桥接结构体与 FishNet Broadcast 系统集成。

## FishNet 策略

| 能力 | 策略 |
|------|------|
| NetworkBehaviour / NetworkObject | ✅ 使用 — NetworkEntityView 的基类 |
| SyncVar / SyncList / SyncDictionary | ✅ 使用 — Phase 2 状态同步核心 |
| RPC（ServerRpc / ObserversRpc / TargetRpc） | ✅ 使用 |
| Spawn / Despawn | ✅ 使用 — Phase 2 实体生命周期 |
| Interest Management | ✅ 使用 |
| Custom Serializer | ✅ 使用 — MemoryPack 注入 |
| 底层传输 / 连接管理 / 心跳 | ✅ 直接使用 |

## Phase 1 功能清单

- FishNet v4.7.2 源码集成（C# 9.0 兼容）
- INetworkManager 接口定义与服务端/客户端启停
- MessageBase 消息协议与 ConnectionMessages 类型
- FishNetNetworkManager 包装 FishNet.NetworkManager
- NetworkEntityView 网络实体视图基类
- FishNetSerializerAdapter（MemoryPack → FishNet Custom Serializer）
- FishNetMessageBus 消息路由与 FishNetConnectionAdapter 连接适配
- 编辑模式测试

## Phase 2 已完成

### C6.1 状态同步 ✅

> FishNet v4 使用 `SyncVar<T>` 泛型类（替代旧的 `[SyncVar]` 属性），提供 `.Value` 属性读写和 `OnChange` 事件回调。

**SyncVar → IReadOnlyModel 绑定模式：**

服务端 Core Model 计算权威数据 → Controller 写入 `SyncVar<T>.Value` → FishNet 自动同步到各客户端 → `SyncVar<T>.OnChange` 回调 → `SyncedModel<T>.SetValue()` → `IReadOnlyModel<T>.OnValueChanged` → 客户端 View 通过 `PropertyBinder` 监听。

```csharp
// NetworkEntityView 子类示例
public class PlayerNetworkView : NetworkEntityView
{
    [SerializeField] private SyncVar<int> m_syncHealth = new();
    private readonly SyncedModel<int> m_healthModel = new();

    public IReadOnlyModel<int> HealthModel => m_healthModel;

    private void Awake()
    {
        m_syncHealth.OnChange += OnHealthChanged;
        RegisterSyncedModel(m_healthModel);
    }

    private void OnHealthChanged(int prev, int next, bool asServer)
    {
        ApplySyncValue(m_healthModel, next, asServer);
    }

    // 服务端写入（由 Controller 调用）
    public void UpdateHealth(int health)
    {
        if (IsServerStarted)
            m_syncHealth.Value = health;
    }
}
```

**SyncedModel\<T\>（Core 层）：**

`HN.Framework.Core.Capability.Network.SyncedModel<T>` 实现了 `IReadOnlyModel<T>`，提供：
- `SetValue(T)` — 服务端/网络回调写入新值（值相等时跳过）
- `IsDirty` / `ClearDirty()` — 脏标记追踪
- `Reset(T)` — 重置到默认值
- 线程安全（lock 保护内部数据）

**SyncedList / SyncedDictionary 集合同步：**

| 类型 | 声明方式 | 变更事件 |
|------|---------|---------|
| `FishNetSyncedList<T>` | `[SerializeField] FishNetSyncedList<int> m_items = new();` | `OnCollectionChanged` → `SyncCollectionChange<T>` |
| `FishNetSyncedDictionary<TKey,TValue>` | `[SerializeField] FishNetSyncedDictionary<string,int> m_stats = new();` | `OnCollectionChanged` → `SyncDictChange<TKey,TValue>` |

两者继承 FishNet 的 `SyncList<T>` / `SyncDictionary<TKey,TValue>`，提供框架统一的 `SyncCollectionOperation`（Add/Remove/Insert/Set/Clear）事件映射，View 层通过 `IReadOnlyList<T>` / `IReadOnlyDictionary<TKey,TValue>` 读取。

**Host 模式说明：** `ApplySyncValue` 在内部调用 `SyncedModel<T>.SetValue()`，后者有值相等检查，Host 模式下重复写入不会触发额外事件。

### 帧同步（Lockstep） ✅

> 固定逻辑帧率 + 客户端输入 ServerRpc + 服务端确定性 Tick(`FixedMathSharp`) + 校验和比对。

帧同步与状态同步并列于 C6.1 模块，适用于 RTS、格斗等需要确定性回放的场景。

#### 架构

```
Core 层：
  IFrameSyncManager       — 帧同步管理器接口（继承 ITickable）
  FrameInput              — [MemoryPackable] 帧输入数据结构
  FrameInputBuffer        — 帧输入环形缓冲区
  IChecksumProvider       — 可插拔校验和接口
  XorChecksumProvider     — 默认 XOR 校验和实现
  LockstepManager         — IFrameSyncManager 核心实现（固定帧率驱动 + 输入缓冲 + 校验和）
  FrameInputMessage       — 客户端→服务端帧输入消息（MsgId=100）
  FrameDataMessage        — 服务端→客户端帧数据广播消息（MsgId=101）

Unity 层：
  LockstepNetworkDriver   — 帧同步网络驱动（纯 C#，通过 FishNetMessageBus 收发）
  GameWorld.FrameSyncManager — GameWorld 注入属性，Tick 首位驱动
```

#### 接口说明

```csharp
public interface IFrameSyncManager : ITickable
{
    ulong CurrentFrame { get; }          // 当前逻辑帧号
    int FrameRate { get; set; }          // 帧率（默认 15，可动态修改）
    int BufferSize { get; set; }         // 缓冲大小（默认 3，可动态修改）
    bool IsEnabled { get; set; }         // 启用/禁用

    void SubmitInput(int clientId, FrameInput input);
    bool TryGetInput(ulong frameNumber, out FrameInput input);
    ulong GetChecksum(ulong frameNumber);
    void RegisterChecksum(ulong frameNumber, ulong checksum);

    event Action<ulong> OnFrameStart;    // 每帧开始时触发
}
```

#### 使用示例

```csharp
// GameWorldDriver 或 GameEntry 中初始化（默认）
protected override void InitializeFrameSync()
{
    var lockstepManager = new LockstepManager(frameRate: 15, bufferSize: 3);
    var messageBus = new FishNetMessageBus();
    var driver = new LockstepNetworkDriver(lockstepManager, messageBus);
    driver.Initialize();
    World.FrameSyncManager = lockstepManager;
}

// 在确定性 Tick 中使用
private void OnFrameStart(ulong frameNumber)
{
    // 获取该帧所有客户端的输入
    if (World.FrameSyncManager.TryGetInput(frameNumber, out FrameInput input))
    {
        // 使用 FixedMathSharp 执行确定性状态更新
        foreach (var kvp in input.Actions)
        {
            var actionName = kvp.Key;
            var actionValue = kvp.Value; // Fixed64
            // 更新游戏状态...
        }
    }
}
```

#### 帧同步与状态同步的选择

| 特性 | 状态同步 (SyncVar) | 帧同步 (Lockstep) |
|------|:------------------:|:-----------------:|
| 同步方式 | 自动同步状态字段 | 同步输入，确定性地计算状态 |
| 带宽消耗 | 随同步字段量增加 | 仅传输输入，非常低 |
| 反作弊 | 服务端权威 | 校验和检测不同步 |
| 回放支持 | 需额外记录状态 | 天然支持（回放输入即可） |
| 适用场景 | MMO/FPS/ARPG | RTS/格斗/竞速 |
| 数值类型 | float（FishNet 默认） | Fixed64（FixedMathSharp） |

#### 默认配置

| 参数 | 默认值 | 说明 |
|------|:------:|------|
| `FrameRate` | 15 | 逻辑帧率（tick/s），游戏逻辑在此频率下运行 |
| `BufferSize` | 3 | 帧输入缓冲容量，客户端可提前发送的帧数 |
| `MaxCatchUpFrames` | 9 | 追帧保护上限（BufferSize × 3），防止死亡螺旋 |

### Phase 2 待开发

- C6.2 客户端预测：FishNet Prediction API
- C6.3 实体权限：EntityManager ↔ FishNet Spawn 集成
- NetworkTransform / NetworkAnimator 框架封装
