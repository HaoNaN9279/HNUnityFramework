---
sidebar_position: 3
---

# 贡献指南

欢迎为 HNUnityFramework 做出贡献！本文档介绍开发流程和规范。

## 开发环境

| 组件 | 版本要求 |
|------|---------|
| Unity | 2022.3+ |
| .NET SDK | 8.0+ （用于生成 API 文档） |
| Node.js | 20+ （用于文档站点开发） |

## 分支策略

1. 从 `main` 分支创建功能分支：`git checkout -b feat/my-feature`
2. 编写代码，确保包含完整的 XML 文档注释
3. 更新相关文档
4. 提交并创建 Pull Request

## 提交规范

遵循以下前缀格式：

| 前缀 | 用途 |
|------|------|
| `feat:` | 新功能 |
| `fix:` | Bug 修复 |
| `docs:` | 文档更新 |
| `refactor:` | 代码重构 |
| `chore:` | 构建/工具变更 |

示例：
```
feat: 添加对象池自动调节功能
fix: 修复 HFSM 死循环检测误报
docs: 更新 Core 模块使用指南
```

## PR 检查清单

每次 PR 合并前确认：

- [ ] 公共 API 有完整的 XML 文档注释（`<summary>` 必填）
- [ ] 新增模块是否已在 `~docs-site/docs/api/index.md` 中索引
- [ ] 架构变更是否已更新 `~docs-site/docs/dev/architecture.md`
- [ ] 本次变更是否需要更新 `AGENTS.md`
- [ ] 本次变更是否需要更新 `README.md`
- [ ] 本地运行 `npm run build`（在 `~docs-site/` 下）是否通过

## 文档更新

### 自动更新（CI）

推送到 `main` 分支时，GitHub Actions 自动：
1. 运行 DocFX 从源码 XML 注释生成 API 参考文档
2. 构建 Docusaurus 站点
3. 部署到 GitHub Pages

### 手动更新

以下文档需要手动维护：

| 文档 | 何时更新 |
|------|---------|
| `AGENTS.md` | 项目结构、开发流程、编码规范变更 |
| `README.md` | 项目概述、功能列表变更 |
| `~docs-site/docs/guide/` | 新增功能或使用方式变更 |
| `~docs-site/docs/dev/` | 架构或开发流程变更 |

## 代码变更流程

1. 从 `main` 分支创建功能分支
2. 编写代码，确保：
   - 所有公共 API 包含 XML `<summary>` 注释
   - 遵循 [代码规范](./code-standard.md)
   - 通过 Unity Console 无编译错误
3. 更新相关文档
4. 提交并创建 PR
5. Code Review 通过后合并到 `main`

## 文档本地开发

```bash
cd ~docs-site

# 安装依赖
npm install

# 启动开发服务器（支持热更新）
npm start
# → http://localhost:3000/HNUnityFramework/

# 生成 API 文档（需要 .NET 8.0 SDK）
npm run docs:api

# 完整构建（含 API 文档）
npm run build:full
```
