---
sidebar_position: 13
---

# 通用能力层 (CapabilityModule)

CapabilityModule 位于 DriverLayer 与 Level 之间，提供跨关卡复用的通用服务接口和框架级实现。各接口通过 `GameWorld` 属性暴露，由 `GameWorldDriver` 在初始化时注入具体实现。

## 服务接口一览

| 接口 | 状态 | 说明 |
|------|------|------|
| `ILogProvider` | ✅ | 日志服务接口。已实现 `UnityLogProvider`，封装 `Debug.Log` |
| `IAssetManager` | ✅ | 资源管理器接口。已实现 `AssetManager`，支持 Addressables / AssetDatabase / Resources 三种 Operator |
| `INetworkManager` | 🚧 | 网络服务接口。`FishNetNetworkManager` 骨架已创建，FishNet 封装待实现 |
| `IInputManager` | ✅ | 输入管理器接口。InputSystem 封装，支持事件驱动的动作注册/注销、优先级屏蔽、自动设备检测 |
| `IStorageProvider` | ✅ | 存储服务接口。提供 `Save` / `Load` / `Delete` 方法，接口已定义 |
| `IEventBus` | ✅ | 事件总线接口。支持 Subscribe/Unsubscribe/Publish/HasSubscribers，线程安全 |
| `ICameraManager` | ✅ | 摄像机管理器接口。Cinemachine Brain 桥接，支持预设注册/激活/混合/振动 |
