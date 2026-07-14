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

- FishNet v4.7.2 源码集成
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

### Phase 2 待开发

- C6.2 客户端预测：FishNet Prediction API
- C6.3 实体权限：EntityManager ↔ FishNet Spawn 集成
- NetworkTransform / NetworkAnimator 框架封装
