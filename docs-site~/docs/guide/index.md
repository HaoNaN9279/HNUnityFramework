---
sidebar_position: 1
---

# HNUnityFramework 使用指南

欢迎使用 HNUnityFramework —— 一个轻量级、模块化的 Unity 游戏开发框架。

## 框架概述

HNUnityFramework 采用三层架构设计，自底向上分为**驱动层 (DriverLayer)**（GameWorld、基础类库、对象池）、**通用能力层 (CapabilityModule)** 和**关卡层 (Level)**，配合**编辑器工具**提供完整的开发支持。

- **模块化架构**：基于 MVC 模式的分层设计，各模块低耦合高内聚
- **资源管理**：统一的资源加载接口，推荐使用 Addressables
- **对象池**：高效的对象池系统，支持普通对象和 GameObject
- **引用池**：零 GC 压力的引用对象管理
- **层次状态机（HFSM）**：灵活的游戏逻辑状态管理
- **流程管理**：Procedure 驱动的游戏生命周期控制
- **序列化**：统一的 JSON 序列化接口
- **工具类**：可序列化字典等实用工具

## 快速开始

查看 [安装指南](./install) 了解如何将框架集成到你的项目中，然后参考 [快速入门](./quick-start) 开始使用。

## 架构导航

### 驱动层 (DriverLayer)

驱动层是框架的基石，提供全局驱动根节点、基础类库和对象池系统。

- **[GameWorld / GameWorldDriver](./core)** — 框架入口与全局驱动根节点，管理固定步长逻辑帧和全局设置
- **基础类库** — [参考池 (ReferencePool)](./reference-pool) 零 GC 引用对象管理，[序列化 (Serialize)](./serialize) JSON 工具，HNLogicTime 逻辑时间
- **[对象池 (ObjectPool)](./object-pool)** — 支持普通 C# 对象和 GameObject 的通用对象池

### 通用能力层 (CapabilityModule)

通用能力层提供跨关卡复用的服务接口和通用实现。

- **[资源管理 (AssetManager)](./asset-manager)** — `IAssetManager` 统一资源接口，`AddressablesOperator` Addressables 加载器
- **日志** — `ILogProvider` / `UnityLogProvider` 日志接口与 Unity 实现
- **[本地化 (C3)](./localization)** — 多语言字符串查询、TextLocalizer 自动文本更新、AssetLocalizer 按语言加载资源
- **网络 (✅)** — `INetworkManager` 网络服务接口（C6.1 状态同步 + 帧同步 + C6.2 客户端预测已完成，详见[网络指南](./network)）
- **存储** — `IStorageProvider` 持久化存储接口
- **[物理系统 (Physics)](./physics)** — `IPhysicsWorld` 纯 C# 物理抽象层，统一 2D/3D 物理接口，支持 PhysX 封装、射线检测、碰撞事件
- **[流程管理 (Procedure)](./procedure)** — `ProcedureManager` 事件驱动的游戏流程控制
- **事件 (🚧 Stub)** — `IEventBus` 事件总线接口

### 关卡层 (Level)

关卡层是游戏逻辑和表现的核心，包含 MVC 框架和状态机等基础设施。

- **[MVC 框架](./mvc)** — Model-View-Controller 分层架构
- **[HFSM](./hfsm)** — 层次有限状态机，支持条件转换和状态嵌套
- **[Combat 战斗数值体系](./combat)** — 属性/效果/Buff/技能 四模块数值系统（AttributeSet/EffectPipeline/DamagePipeline/BuffSystem/AbilitySystem）
- **Entity 系统 (🚧 Stub)** — Entity 基类与 EntityView 基类
- **视图工厂 (🚧 Stub)** — ViewFactory 基类与 PropertyBinder

### 编辑器工具 (Editor)

编辑器扩展工具，提升开发调试效率。

- **设置面板** — 框架全局配置编辑器面板
- **配置表编辑器** — 配置表导入与管理工具
- **对象池调试** — 对象池运行状态监控窗口
- **Addressables 扩展** — Addressables 分组与构建辅助工具
- **Editor UI 扩展 ✅** — Odin-like 自定义 Inspector 增强，19 个运行时属性（ReadOnly/Button/ShowIf/HideIf/EnableIf/DisableIf/ShowInInspector/HideInInspector/Required/ValidateInput/OnValueChanged/Title/InfoBox/PropertyOrder/FoldoutGroup/BoxGroup/TabGroup/HorizontalGroup/VerticalGroup）+ 扩展接口体系 + HNSmartInspector
- **资源质检 ✅** — 基于规则注册中心的资源质量检测系统，框架预设 4 种通用规则（缺失脚本/命名规范/引用完整性/Addressables 配置），项目可通过 `IAssetRule` 接口扩展自定义规则
- **自动化构建 ✅** — 构建管线编排器（质检门禁 → Addressables → Player → 后处理）+ 版本管理（语义化版本/自动递增/Git tag）+ 构建报告生成（JSON/Markdown）
- **冗余资源清理 ✅** — 基于引用图的冗余扫描器，可视化窗口支持冗余资源审查和移动到备份目录

## 工具类

查看[工具类文档](./utils)了解 HNDictionary、SerializableDictionary 等实用工具。
