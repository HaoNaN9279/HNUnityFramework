---
sidebar_position: 43
---

# 资源质检与自动化构建

## 概述

E9 资源质检与自动化构建模块（命名空间 `HN.Framework.Editor.BuildPipeline`，位于 `Editor/BuildPipeline/`）为项目提供构建前的资源质量门禁与多平台构建编排能力。核心由四部分组成：

1. **规则注册中心**：以 `IAssetRule` 为扩展点，按 `AssetImporter` 类型分组注册规则，可扩展到任意自定义 Importer。
2. **构建管线**：`BuildPipelineOrchestrator` 按顺序执行构建步骤，任一步骤失败即中止。
3. **冗余检测**：基于 `DependencyGraph` 的引用图分析，识别无根资源引用的孤立资源。
4. **版本与报告**：语义化版本管理、Git tag、构建清单与 JSON/Markdown 报告。

模块仅依赖 UnityEditor 内建 API，无新增外部依赖，不影响运行时程序集。

## 编辑器窗口

菜单入口均为 `HNFramework/`：

| 窗口 | 菜单路径 | 职责 |
|------|----------|------|
| `AssetValidatorWindow` | HNFramework/Asset Validator | 选择目标路径与规则，执行资源质检并查看结果 |
| `RedundancyCleanerWindow` | HNFramework/Redundancy Cleaner | 构建引用图、扫描冗余资源、审查并移动到备份目录 |
| `BuildPipelineWindow` | HNFramework/Build Pipeline | 编排构建步骤，触发多平台构建 |
| `BuildReportWindow` | HNFramework/Build Reports | 查看 JSON/Markdown 构建报告 |

## 资源质检

### 规则接口

```csharp
public interface IAssetRule
{
    string RuleName { get; }
    string Description { get; }
    // 返回 null 表示适用于所有导入器类型
    Type TargetImporterType { get; }
    IReadOnlyList<RuleResult> Validate(string assetPath);
}
```

`RuleResult` 通过静态工厂创建结果：

```csharp
RuleResult.Pass(ruleName, assetPath);
RuleResult.Fail(ruleName, assetPath, RuleSeverity.Warning, "描述");
```

`RuleSeverity` 等级：`Info` < `Warning` < `Error` < `Fatal`。`RuleResult.Passed` 在严重度低于 `Error` 时为 `true`。

### 规则注册

项目可在 `[InitializeOnLoad]` 中注册自定义规则：

```csharp
using HN.Framework.Editor.BuildPipeline;
using UnityEditor;

[InitializeOnLoad]
public static class MyProjectRules
{
    static MyProjectRules()
    {
        // 针对特定 Importer 类型
        AssetRuleRegistry.RegisterRule<TextureImporter>(new TextureSizeRule());
        // 全局规则（适用于所有导入器）
        AssetRuleRegistry.RegisterGlobalRule(NamingConventionRule.CreateDefault());
    }
}
```

`AssetRuleRegistry` 主要 API：

- `RegisterRule<TImporter>(IAssetRule)` / `RegisterRule(Type, IAssetRule)`
- `RegisterGlobalRule(IAssetRule)`
- `GetRulesForImporter(Type)` —— 返回全局规则 + 该类型的注册规则
- `GetAllRules()` / `GetRegisteredImporterTypes()` / `Clear()`

### 框架预设通用规则

| 规则 | 检查内容 | 默认严重度 |
|------|----------|-----------|
| `MissingScriptRule` | Prefab/Scene 中 MonoBehaviour 引用断裂 | Error |
| `NamingConventionRule` | 文件名是否符合可配置正则（`CreateDefault()` 为字母开头、仅字母数字下划线） | Warning |
| `ReferenceIntegrityRule` | 资源引用链完整性（引用目标是否存在） | Error |
| `AddressablesRule` | Addressables 同 Key 冲突等配置检查，未安装包时静默跳过 | Warning/Error |

预设规则均为项目无关的通用约束。纹理尺寸、压缩格式、模型动画类型、音频加载方式等业务相关规则由项目自行实现并注册。

### 执行校验

```csharp
var validator = new AssetValidator();
validator.AddRule(new MyTemporaryRule()); // 临时附加规则，不改动全局注册中心

IReadOnlyList<RuleResult> results = validator.Validate(
    "Assets/Art", recursive: true, onProgress: (current, total) => { });

// 单资源校验
IReadOnlyList<RuleResult> single = validator.ValidateSingle("Assets/Art/foo.png");
```

`AssetValidator` 遍历目录 → 识别资源 Importer 类型 → 匹配全局规则与附加规则 → 执行并汇总结果。

## 构建管线

### 步骤与上下文

```csharp
public interface IBuildStep
{
    string StepName { get; }
    string Description { get; }
    bool Execute(BuildContext context);
}
```

`BuildContext` 携带构建参数与状态：`BuildTarget`、`BuildTargetGroup`、`OutputPath`、`Version`、`EnableValidation`、`IsCancelled`、`CustomData`、`StepLogs`。

### 编排执行

```csharp
var orchestrator = new BuildPipelineOrchestrator();
orchestrator.AddSteps(
    new PreBuildValidationStep(),   // 质检门禁
    new AddressablesBuildStep(),    // Addressables 构建（未安装包时跳过）
    new PlayerBuildStep(),          // Player 打包
    new PostBuildStep());           // 后处理（清单 + 报告）

orchestrator.Configure(
    BuildTarget.StandaloneWindows64,
    BuildTargetGroup.Standalone,
    "Builds/Windows",
    VersionManager.GetVersionString());

bool success = orchestrator.Execute();
```

任一步骤返回 `false` 或抛出异常时管线立即中止并返回 `false`；调用 `Cancel()` 后后续步骤将被跳过。

## 冗余资源检测

```csharp
var graph = new DependencyGraph();
graph.Build("Assets", onProgress: (c, t) => { });
// graph.GetStats() → "Assets: N, Edges: M, Orphans: K"

var scanner = new RedundancyScanner(graph);
scanner.Scan(onProgress: (c, t) => { });

IReadOnlyList<string> redundant = scanner.RedundantAssets;
long bytes = scanner.GetEstimatedSavings();
int moved = scanner.MoveToFolder("Assets/_Redundant");
```

扫描策略：将场景、Prefab、ScriptableObject、材质、动画等视为根资源，未被任何根资源直接或间接引用的资源标记为冗余；`Assets/Editor`、`Assets/Plugins`、`Assets/Resources`、`Assets/StreamingAssets` 及脚本文件会被跳过。

## 版本管理与构建清单

```csharp
var config = VersionManager.GetOrCreateVersionConfig();
VersionManager.IncrementBuildNumber();
VersionManager.IncrementPatch();   // Patch++，BuildNumber 重置
string version = VersionManager.GetVersionString(); // "0.1.0 (1)"

VersionManager.TryCreateGitTag(version);            // 创建 Git tag（git 仓库内）
BuildManifest manifest = VersionManager.GenerateBuildManifest(orchestrator.Context);
```

`VersionConfig`（ScriptableObject，`Assets/Editor/BuildPipeline/Versioning/VersionConfig.asset`）保存 `Major`/`Minor`/`Patch`/`BuildNumber`/`PreReleaseTag`/`BuildMetadata`，`GetVersionString()` 输出语义化版本 `major.minor.patch[-pre][+meta]`。

`BuildManifest` 记录 `BuildTime`、`Version`、`Branch`、`CommitHash`、`Platform`、`BuildSize`、`OutputPath`、`BuildNumber`。

## 构建报告

```csharp
string json = BuildReportGenerator.GenerateJson(manifest, validationResults);
string md = BuildReportGenerator.GenerateMarkdown(manifest, validationResults);
```

报告包含构建元数据与质检结果统计（错误数/警告数），Markdown 版本输出校验结果明细表。

## 全局设置

`BuildPipelineSettings`（ScriptableObject，经 `HNModuleSettingsUtility` 管理）提供：

| 属性 | 说明 |
|------|------|
| `BuildOutputRoot` | 构建输出根目录（相对项目根） |
| `EnablePreBuildValidation` | 是否启用构建前质检门禁 |
| `BlockBuildOnValidationFailure` | 质检未通过时是否阻止构建 |
| `AutoGenerateBuildManifest` | 是否自动生成构建清单 |
| `VersionConfigPath` | 版本配置资源路径 |

在 Project Settings 的 `Project/HN Unity Framework/Build Pipeline` 面板中配置。

## 相关源码

- 规则框架：`Editor/BuildPipeline/Rules/`
- 校验执行：`Editor/BuildPipeline/Validator/`
- 构建管线：`Editor/BuildPipeline/Pipeline/`
- 冗余检测：`Editor/BuildPipeline/Redundancy/`
- 版本管理：`Editor/BuildPipeline/Versioning/`
- 构建报告：`Editor/BuildPipeline/Reporting/`
- 全局设置：`Editor/BuildPipeline/Settings/`
