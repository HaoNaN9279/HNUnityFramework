# AGENTS.md

> AI 编程助手和项目贡献者的开发指南。记录当前架构现状，包括已知不一致。

---

## 项目概述

**HNUnityFramework** 是一个轻量级、模块化的 Unity 游戏开发框架，分三层程序集组织：纯 C# 逻辑层 (Core)、Unity 平台层 (Unity)、编辑器工具层 (Editor)。

- **仓库**: `HaoNaN9279/HNUnityFramework`
- **目标平台**: Unity 2022.3+
- **语言**: C# (.NET Standard 2.1)

---

## 架构

- 见架构文档：架构~/最终架构.md

## 编码规范

### 命名规范
- **命名空间**: 见架构~/最终架构.md，新代码统一使用 `HN.Framework.Core.*` / `HN.Framework.Unity.*` / `HN.Framework.Editor`
- **类/接口**: PascalCase，接口以 `I` 开头
- **方法**: PascalCase
- **私有字段**: camelCase

### 文档注释
所有公共 API 必须使用 XML 文档注释:

```csharp
/// <summary>
/// 对象池管理器，负责所有对象池的生命周期管理。
/// </summary>
public class ObjectPoolManager { }
```

### XML 注释标签规范
- `<summary>`: 类型或方法的简要描述（必填）
- `<typeparam>`: 泛型类型参数说明
- `<param>`: 方法参数说明
- `<returns>`: 返回值说明
- `<remarks>`: 补充说明

---

## 开发流程

### 代码变更流程

1. 从 `main` 分支创建功能分支
2. 编写代码，包含完整 XML 文档注释
3. 更新相关文档:
   - 影响公共 API → 更新 `~docs-site/docs/api/index.md`
   - 新增模块 → 在对应指南文档中添加说明
   - 架构变更 → 更新 `~docs-site/docs/dev/architecture.md`
4. 提交并创建 PR

### 工作流检查清单

每次 PR 合并前确认:

- [ ] 公共 API 有完整的 XML 文档注释?
- [ ] 新增模块已在 `~docs-site/docs/api/index.md` 中索引?
- [ ] 架构变更已更新 `~docs-site/docs/dev/architecture.md`?
- [ ] 本次变更是否需要更新 `AGENTS.md`?
- [ ] 本次变更是否需要更新 `README.md`?

### 架构修改

- 回答用户问题时，首先阅读当前项目的架构文档，再给出最合理的解决方案。
- 当用户提出或者和用户一起讨论出新的架构方案时，同步修改架构文档。

---

## 提交规范

- `feat:` — 新功能
- `fix:` — Bug 修复
- `docs:` — 文档更新
- `refactor:` — 代码重构
- `chore:` — 构建/工具变更

---

## 占位说明

项目中标记为 🚧 Stub 的模块目录已创建，但只包含骨架类。待后续完善。
