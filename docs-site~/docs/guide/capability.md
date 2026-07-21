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
| `ILocaleProvider` | ✅ | 本地化提供程序接口（多语言字符串查询、语言区域切换、变更事件）。已实现 LocaleManager，注入 GameWorld.LocaleProvider |
| `IPhysicsWorld` | ✅ | 物理系统抽象接口。已实现 `PhysXWorld`，封装 UnityEngine.Physics/Physics2D，支持刚体创建、射线检测、碰撞事件 |

## 模块配置系统

每个模块可以定义自己的 ScriptableObject 配置，与模块代码**同目录放置**，通过 `HNModuleSettingsUtility` 统一管理。

已注册到 **Project Settings → HN Unity Framework** 的模块配置：

| 面板 | 路径 | 说明 |
|------|------|------|
| 根配置 | HN Unity Framework | 框架核心参数（LogicRate 等） |
| Audio | HN Unity Framework → Audio | 音频设置：MasterVolume / MaxConcurrentSounds / EnableSpatialAudio |
| Build Pipeline | HN Unity Framework → Build Pipeline | 构建管线设置 |
| Localization | HN Unity Framework → Localization | 本地化设置：DefaultLocale / AutoDetectLocale |
| UI | HN Unity Framework → UI | UI 设置：Toast 时长/并发/动画/遮罩/字号 |
| Network | HN Unity Framework → Network | 网络设置：帧同步帧率/缓冲/连接地址/端口 |
| Asset | HN Unity Framework → Asset | 资源设置：自动卸载延迟/开关 |
| Cutscene | HN Unity Framework → Cutscene | 过场动画：全局跳过/速度倍率 |
| HybridCLR | HN Unity Framework → HybridCLR | 热更新：AOT 元数据/热更程序集/构建选项 |
| AI | HN Unity Framework → AI | AI 设置：动作频率/模糊推理/管线层级 |

> 扩展指引：参见 [贡献指南 - 扩展模块配置](../dev/contribute.md#扩展模块配置settings)
