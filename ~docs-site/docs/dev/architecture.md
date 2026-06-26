---
sidebar_position: 2
---

# 项目架构

HNUnityFramework 采用模块化分层架构设计。

## 整体结构

```
HNUnityFramework/
├── Runtime/          # 运行时代码
│   ├── Core/         # 框架核心（启动、全局设置、Tick 系统）
│   ├── MVC/          # MVC 模式实现（Model、View、Controller）
│   ├── AssetManager/ # 资源管理（Addressables/Resources/AssetDatabase）
│   ├── ObjectPool/   # 对象池系统
│   ├── ReferencePool/# 引用池系统
│   ├── HFSM/         # 层次有限状态机
│   ├── Procedure/    # 流程管理
│   ├── Serialize/    # 序列化
│   ├── Sheet/        # 配置表
│   └── Utils/        # 工具类
├── Editor/           # 编辑器扩展代码
│   ├── Core/         # 编辑器核心
│   ├── Sheet/        # 配置表编辑器
│   ├── ObjectPool/   # 对象池调试面板
│   ├── AddressablesExtensitions/ # Addressables 扩展
│   └── Utils/        # 编辑器工具
└── ~docs-site/       # 文档站点
```

## 核心设计原则

- **低耦合、高内聚**：各模块职责清晰，通过接口通信
- **可扩展**：基于接口设计，易于替换和扩展
- **零 GC 压力**：核心运行时使用引用池避免内存分配
- **编辑器友好**：提供丰富的编辑器工具和调试面板
