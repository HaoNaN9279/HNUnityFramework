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

## Phase 2 规划

- C6.1 状态同步：SyncVar → IReadOnlyModel 绑定
- C6.2 客户端预测：FishNet Prediction API
- C6.3 实体权限：EntityManager ↔ FishNet Spawn 集成
- NetworkTransform / NetworkAnimator 框架封装
