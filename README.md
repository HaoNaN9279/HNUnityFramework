# HNUnityFramework

轻量级、模块化的 Unity 游戏开发框架。

## 特性

- **模块化架构** — 基于 MVC 模式的分层设计，各模块低耦合高内聚
- **统一资源管理** — 支持 Addressables、Resources、AssetDatabase 的透明切换
- **对象池系统** — 高效的对象池与 GameObject 池，减少 GC 压力
- **引用池** — 零内存分配的引用对象复用
- **层次状态机 (HFSM)** — 灵活的游戏逻辑状态管理
- **流程管理** — Procedure 驱动的生命周期控制
- **配置表支持** — 编辑器内配置表导入与运行时查询
- **丰富的编辑器工具** — 调试面板、菜单扩展、部署工具

## 模块概览

| 模块 | 说明 |
|------|------|
| Core | 框架初始化、全局设置、Tick 系统 |
| MVC | Model-View-Controller 架构 |
| AssetManager | 统一资源加载接口 |
| ObjectPool | 通用对象池 |
| ReferencePool | 引用池（零 GC） |
| HFSM | 层次有限状态机 |
| Procedure | 流程/生命周期管理 |
| Serialize | 序列化工具 |
| Sheet | 配置表管理 |

## 文档

文档以 Markdown 文件维护在 `~docs-site/docs/` 下，包含：

- **使用指南** — 安装、快速入门、各模块使用说明（`docs/guide/`）
- **开发文档** — 项目架构、贡献指南、代码规范（`docs/dev/`）

## 快速开始

### 安装（通过 Unity Package Manager）

```json
// Packages/manifest.json
{
  "dependencies": {
    "com.hn.framework": "https://github.com/HaoNaN9279/HNUnityFramework.git"
  }
}
```

### 初始化框架

```csharp
using HN.Framework;

// 框架在 Unity 启动时自动初始化
// 通过 HNUnityFramework.Instance 访问框架实例
```

更多内容请参阅 [使用指南](./docs-site~/docs/guide/index.md)。

## 开发

### 环境要求

- Unity 2022.3+

详细开发流程请参阅 [AGENTS.md](./AGENTS.md)。

## 许可证

MIT License
