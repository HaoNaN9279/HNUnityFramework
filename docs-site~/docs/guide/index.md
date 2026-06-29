---
sidebar_position: 1
---

# HNUnityFramework 使用指南

欢迎使用 HNUnityFramework —— 一个轻量级、模块化的 Unity 游戏开发框架。

## 框架概述

HNUnityFramework 提供了一套完整的 Unity 开发基础设施，包括：

- **模块化架构**：基于 MVC 模式的分层设计
- **资源管理**：统一的资源加载接口，推荐使用 Addressables
- **对象池**：高效的对象池系统，支持普通对象和 GameObject
- **引用池**：零 GC 压力的引用对象管理
- **层次状态机（HFSM）**：灵活的游戏逻辑状态管理
- **流程管理**：Procedure 驱动的游戏生命周期控制
- **序列化**：统一的 JSON 序列化接口
- **工具类**：可序列化字典等实用工具

## 快速开始

查看 [安装指南](./install) 了解如何将框架集成到你的项目中，然后参考 [快速入门](./quick-start) 开始使用。

## 模块指南

| 模块 | 说明 |
|------|------|
| [Core](./core) | 框架入口、固定步长逻辑帧、全局设置 |
| [MVC](./mvc) | Model-View-Controller 分层架构 |
| [资源管理](./asset-manager) | 资源加载（推荐 Addressables，旧 AssetManager 已弃用） |
| [对象池](./object-pool) | C# 对象池与 GameObject 池 |
| [引用池](./reference-pool) | 零 GC 引用对象管理，16 种池化集合 |
| [HFSM](./hfsm) | 层次有限状态机，支持条件转换和状态嵌套 |
| [Procedure](./procedure) | 事件驱动的游戏流程管理 |
| [序列化](./serialize) | JSON 序列化/反序列化工具 |
| [工具类](./utils) | 可序列化字典（HNDictionary / SerializableDictionary） |
