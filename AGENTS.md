# AGENTS.md

> 本文档为 AI 编程助手和项目贡献者提供 HNUnityFramework 项目的开发指南。

---

## 项目概述

**HNUnityFramework** 是一个轻量级、模块化的 Unity 游戏开发框架，为 Unity 项目提供基础设施层的通用解决方案。

- **仓库**：`HaoNaN9279/HNUnityFramework`
- **目标平台**：Unity 2022.3+
- **语言**：C# (.NET Standard 2.1)

## 技术栈

| 组件 | 技术 |
|------|------|
| 运行时脚本 | C# / Unity |
| 编辑器扩展 | C# / Unity Editor API |
| 文档系统 | Docusaurus 3.x + DocFX |
| CI/CD | GitHub Actions |

## 项目结构

```
HNUnityFramework/
├── Runtime/              # 运行时代码 (HN.Framework 程序集)
│   ├── Core/             # 框架核心
│   ├── MVC/              # MVC 模式
│   ├── AssetManager/     # 资源管理
│   ├── ObjectPool/       # 对象池
│   ├── ReferencePool/    # 引用池
│   ├── HFSM/             # 层次状态机
│   ├── Procedure/        # 流程管理
│   ├── Serialize/        # 序列化
│   ├── Sheet/            # 配置表
│   └── Utils/            # 工具类
├── Editor/               # 编辑器代码 (HN.Framework.Editor 程序集)
│   ├── Core/             # 编辑器核心
│   ├── Sheet/            # 配置表编辑器
│   ├── ObjectPool/       # 调试面板
│   └── ...
├── ~docs-site/           # 文档站点 (Docusaurus, Unity 忽略)
│   ├── docs/
│   │   ├── guide/        # 使用指南
│   │   ├── dev/          # 开发文档
│   │   └── api/          # API 文档概览
│   ├── docfx/            # DocFX 配置
│   └── static/           # 静态资源
└── .github/workflows/    # CI/CD
```

## 编码规范

### 命名规范
- **命名空间**：`HN.Framework` (Runtime)、`HN.Framework.Editor` (Editor)
- **类/接口**：PascalCase，接口以 `I` 开头
- **方法**：PascalCase
- **私有字段**：camelCase

### 文档注释
所有公共 API 必须使用 XML 文档注释（`<summary>`），用于自动生成 API 文档。

```csharp
/// <summary>
/// 对象池管理器，负责所有对象池的生命周期管理。
/// </summary>
public class ObjectPoolManager { }
```

### XML 注释标签规范
- `<summary>`：类型或方法的简要描述（必填）
- `<typeparam>`：泛型类型参数说明
- `<param>`：方法参数说明
- `<returns>`：返回值说明
- `<remarks>`：补充说明，使用详情

## 提交规范

- `feat:` — 新功能
- `fix:` — Bug 修复
- `docs:` — 文档更新
- `refactor:` — 代码重构
- `chore:` — 构建/工具变更

---

## 开发流程

### 1. 代码变更流程

1. 从 `main` 分支创建功能分支
2. 编写代码，确保包含完整的 XML 文档注释
3. 更新相关文档：
   - 如影响公共 API → 更新 `~docs-site/docs/api/index.md` 中的模块索引
   - 如新增模块/功能 → 在对应指南文档中添加说明
   - 如架构变更 → 更新 `~docs-site/docs/dev/architecture.md`
4. 提交并创建 PR

### 2. 文档更新流程

#### 自动更新（CI 自动执行）
- **API 文档**：推送到 `main` 分支时，GitHub Actions 自动：
  1. 运行 DocFX 从源码 XML 注释生成 API 参考文档
  2. 构建 Docusaurus 站点
  3. 部署到 GitHub Pages (`gh-pages` 分支)

#### 手动更新
- **AGENTS.md**：当项目结构、开发流程、编码规范变更时更新
- **README.md**：当项目概述、功能列表变更时更新
- **使用指南** (`~docs-site/docs/guide/`)：新增功能或使用方式变更时更新
- **开发文档** (`~docs-site/docs/dev/`)：架构或开发流程变更时更新

### 3. 工作流检查清单

每次 PR 合并前确认：

- [ ] 公共 API 有完整的 XML 文档注释？
- [ ] 新增模块是否已在 `~docs-site/docs/api/index.md` 中索引？
- [ ] 架构变更是否已更新 `~docs-site/docs/dev/architecture.md`？
- [ ] 本次变更是否需要更新 `AGENTS.md`？
- [ ] 本次变更是否需要更新 `README.md`？
- [ ] 本地运行 `npm run build`（在 `~docs-site/` 下）是否通过？

### 4. 文档本地开发

```bash
cd ~docs-site

# 安装依赖
npm install

# 启动开发服务器
npm start

# 生成 API 文档（需要 .NET 8.0 SDK）
npm run docs:api

# 完整构建（含 API 文档）
npm run build:full
```

---

## 模块说明

### Runtime 模块

| 模块 | 命名空间 | 说明 |
|------|----------|------|
| Core | `HN.Framework` | 框架初始化、全局设置、Tick 系统 |
| MVC | `HN.Framework` | Model-View-Controller 架构实现 |
| AssetManager | `HN.Framework` | 统一资源加载（Addressables/Resources/AssetDatabase） |
| ObjectPool | `HN.Framework` | 通用对象池与 GameObject 池 |
| ReferencePool | `HN.Framework` | 零 GC 引用对象管理 |
| HFSM | `HN.Framework` | 层次有限状态机 |
| Procedure | `HN.Framework` | 游戏流程/生命周期管理 |
| Serialize | `HN.Serialize` | 序列化工具 |
| Sheet | `HN.Framework` | 配置表数据 |
| Utils | `HN` | 通用工具（HNDictionary 等） |

### Editor 模块

| 模块 | 命名空间 | 说明 |
|------|----------|------|
| Core | `HN.Framework.Editor` | 编辑器全局设置、菜单、部署工具 |
| Sheet | `HN.Framework.Editor` | 配置表导入与编辑 |
| ObjectPool | `HN.Framework.Editor` | 对象池调试面板 |
| Addressables | `HN.Framework.Editor` | Addressables 分组管理扩展 |
