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

---

## 测试

### 测试目录结构

```
Tests/
├── HN.Framework.Core.Tests/          # Core 层纯 C# 单元测试（EditMode）
│   └── HN.Framework.Core.Tests.asmdef
└── HN.Framework.Unity.Tests/         # Unity 层测试（EditMode）
    ├── HN.Framework.Unity.Tests.asmdef
    ├── AssetManagerTests.cs
    ├── PoolTests.cs
    ├── GameObjectPoolTests.cs
    ├── HFSMTests.cs
    ├── ProcedureTests.cs
    ├── MVCTests.cs
    └── Serialization/
        ├── JsonDataTests.cs
        ├── JsonRoundtripTests.cs
        └── JsonEdgeCaseTests.cs
```

- `HN.Framework.Core.Tests` — Core 层纯 C# 测试，引用 `HN.Framework.Core`（无 Unity 依赖）
- `HN.Framework.Unity.Tests` — Unity 层测试，引用 `HN.Framework.Core` + `HN.Framework.Unity`

### 测试编写规范

所有测试必须遵循 Unity Test Framework 标准：

**程序集定义要求：**

- `"overrideReferences": true` — 手动管理引用
- `"precompiledReferences": ["nunit.framework.dll"]` — 引入 NUnit
- `"defineConstraints": ["UNITY_INCLUDE_TESTS"]` — 仅测试环境下编译
- `"includePlatforms": ["Editor"]` — 仅 Editor 平台编译

**代码规范：**

- **命名空间**：测试文件的命名空间必须与目录路径一致。例如 `Serialization/` 目录下的文件使用 `HN.Framework.Unity.Tests.Serialization`
- **测试类**：使用 `[TestFixture]` 属性标记
- **单元测试**：使用 `[Test]` 属性标记
- **生命周期**：`[SetUp]` 初始化测试环境，`[TearDown]` 清理资源避免测试间污染
- **断言**：统一使用 NUnit 断言（`Assert.That()`、`Assert.AreEqual()`、`Assert.IsTrue()` 等）
- **命名模式**：`方法名_场景_预期结果`（如 `Release_DeactivatesAndReparents`）
- **独立性**：每个测试必须可独立运行，`[SetUp]`/`[TearDown]` 确保状态完全隔离
- **隐藏对象**：测试中创建的临时 `GameObject` 必须设置 `HideFlags.HideAndDontSave`，并在 `[TearDown]` 中用 `Object.DestroyImmediate` 销毁
- **临时文件**：避免硬编码路径，使用 `Path.GetTempFileName()` 或 `Guid.NewGuid()` 生成唯一临时文件

---

## 开发流程

### 代码变更流程

1. 从 `main` 分支创建功能分支
2. 编写代码，包含完整 XML 文档注释
3. 通过 Unity MCP 运行测试验证代码正确性:
   - 确保 Unity Editor 已启动并连接 MCP
   - 使用 `run_tests` 工具运行 EditMode 测试
   - 测试未通过则继续排查修复，直到全部通过
4. 更新相关文档:
   - 影响公共 API → 更新 `~docs-site/docs/api/index.md`
   - 新增模块 → 在对应指南文档中添加说明
   - 架构变更 → 更新 `~docs-site/docs/dev/architecture.md`
5. 提交并创建 PR

### 测试驱动开发（TDD）准则

- **新增功能**：先编写测试覆盖功能场景，再实现功能，最后运行测试验证
- **修复 Bug**：先编写暴露 Bug 的测试，确认测试失败，再修复代码，确认测试通过
- **修改功能**：修改后必须通过 Unity MCP 运行对应模块测试，不通过则继续修复
- **项目级测试**：运行测试时，默认只运行当前项目的测试，在用户没有明确说明的情况下，**不允许**进行全量测试

### MCP 测试运行流程

在功能开发或修改过程中，通过以下步骤确保代码正确性：

1. 确认 Unity Editor 处于运行状态，MCP 已连接
2. 运行 EditMode 测试：

```csharp
// 在 Unity MCP 中执行
vibe_unityMCP_run_tests(mode: "EditMode")
```

3. 查看测试结果：
   - 全部通过 → 继续后续流程
   - 有失败 → 分析失败原因，修复代码，重新运行直到全部通过
4. PR 合并前，运行全量 EditMode 测试确认无回归

### 工作流检查清单

每次 PR 合并前确认:

- [ ] 公共 API 有完整的 XML 文档注释?
- [ ] 新增模块已在 `~docs-site/docs/api/index.md` 中索引?
- [ ] 架构变更已更新 `~docs-site/docs/dev/architecture.md`?
- [ ] 本次变更是否需要更新 `AGENTS.md`?
- [ ] 本次变更是否需要更新 `README.md`?
- [ ] 所有相关 EditMode 测试已通过 Unity MCP 验证?

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
