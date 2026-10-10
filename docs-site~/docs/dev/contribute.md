# 贡献指南

欢迎为 HNUnityFramework 做出贡献！本文档介绍开发流程和规范。

## 开发环境

| 组件 | 版本要求 |
|------|---------|
| Unity | 2022.3+ |

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
- [ ] 新增模块是否已在 `~docs-site/docs/guide/` 中有对应说明
- [ ] 架构变更是否已更新 `~docs-site/docs/dev/architecture.md`
- [ ] 本次变更是否需要更新 `AGENTS.md`
- [ ] 本次变更是否需要更新 `README.md`

## 文档维护

文档以 Markdown 文件形式维护在 `~docs-site/docs/` 下，分为两类：

- `docs/guide/` — 使用指南
- `docs/dev/` — 开发文档

直接编辑对应的 `.md` 文件即可，无构建或部署流程。文档间链接使用相对路径（如 `./object-pool.md`、`../dev/architecture.md`）。

## 扩展模块配置（Settings）

框架支持去中心化的模块级配置架构。为新增模块添加 Project Settings 配置面板的步骤：

### 1. 创建配置 ScriptableObject

在模块 Runtime 目录下创建 `Settings/` 子目录，定义配置类：

```csharp
// Runtime/.../Capability/MyModule/Settings/MySettings.cs
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework.Unity.Capability.MyModule
{
    public class MySettings : ScriptableObject
    {
        [SerializeField] private string m_SomeProperty = "default";
        public string SomeProperty => m_SomeProperty;

#if UNITY_EDITOR
        public static MySettings GetOrCreateSettings()
        {
            return HNModuleSettingsUtility.GetOrCreateSettings<MySettings>(AssetPath);
        }
        public static SerializedObject GetSerializedSettings()
        {
            return HNModuleSettingsUtility.GetSerializedSettings<MySettings>(AssetPath);
        }
#endif
        public static readonly string AssetPath =
            "Assets/Project/RuntimeAssets/MyModule/MySettings.asset";
    }
}
```

### 2. 创建 SettingsProvider

在模块 Editor 目录下创建 `Settings/` 子目录，注册到 Project Settings：

```csharp
// Editor/MyModule/Settings/MySettingsProvider.cs
using UnityEditor;

namespace HN.Framework.Editor.MyModule
{
    public static class MySettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateMySettingsProvider()
        {
            return new SettingsProvider("Project/HN Unity Framework/MyModule", SettingsScope.Project)
            {
                label = "MyModule",
                guiHandler = (searchContext) =>
                {
                    var settings = HN.Framework.Unity.Capability.MyModule.MySettings.GetSerializedSettings();
                    // 使用 SerializedProperty 绘制配置字段
                }
            };
        }
    }
}
```

### 3. 集成到部署

在 `FrameworkDeployer.cs` 中添加一行调用即可确保 .asset 文件在部署时自动创建：

```csharp
HN.Framework.Unity.Capability.MyModule.MySettings.GetOrCreateSettings();
```

### 4. 命名注意事项

- **避免类型冲突**：配置类名不应与 `UnityEngine` 名称空间中的类型重名（如 `AudioSettings` 与 `UnityEngine.AudioSettings` 冲突），Editor 代码中使用全限定名引用
- **注册路径约定**：统一使用 `"Project/HN Unity Framework/{ModuleName}"` 作为 SettingsProvider 路径

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

## 文档编辑

文档为纯 Markdown，直接用任意编辑器编辑 `~docs-site/docs/` 下的 `.md` 文件即可。
