---
sidebar_position: 16
---

# 网络 🚧 Stub

网络层基于 Core 层接口 + Unity 层 FishNet 封装的设计，Core 层仅定义协议，FishNet 专有依赖隔离在 Unity 层。

## INetworkManager

Core 层抽象接口（`HN.Framework.Core`），定义 `Connect()`、`Send<T>()`、`RegisterHandler<T>()` 等基础操作。

## FishNet 封装（四组件）

| 组件 | 职责 |
|------|------|
| `FishNetNetworkManager` | 实现 INetworkManager，封装 FishNet Client/Server API |
| `FishNetMessageBus` | 消息路由适配 |
| `FishNetConnectionAdapter` | 连接状态适配 |
| `FishNetSerializerAdapter` | MemoryPack 注入 FishNet |

## FishNet 策略

| 能力 | 策略 |
|------|------|
| 底层传输（TCP/UDP） | ✅ 直接使用 |
| 连接管理 | ✅ 直接使用 |
| NetworkBehaviour / SyncVar / RPC | ❌ **不用**，改用自定义消息协议 |
| 网络对象生成（Spawn） | ❌ **不用**，EntityManager 管理 |
| 带宽统计 / 调试工具 | ✅ 通过接口暴露 |
