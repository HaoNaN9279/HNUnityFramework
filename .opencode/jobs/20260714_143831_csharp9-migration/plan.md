# 计划：C# 9.0 迁移 — Vendor DLL 化

## 概述

**目标**：移除项目中所有 -langversion:preview 配置，自有代码降至 C# 9.0，使用 C# 10+ 特性的第三方 Vendor（MemoryPack、FixedMathSharp）改为 DLL 引用，其余 Vendor 保留源码但限制为 C# 9.0。完成后运行项目级测试并更新 docs-site 文档。

**非目标**：
- 不修改 HN 自有代码的业务逻辑（仅移除 scoped 关键字，语义不变）
- 不升级/降级 Vendor 库的版本
- 不运行全量测试（仅项目内 EditMode 测试）
- 不改变 API 签名或模块间接口

---

## 上下文分析

### 代码库成熟度：过渡型（Transitional）

- HN 自有代码遵循清晰的架构规范（三层驱动、命名空间规整、XML 注释完善）
- Vendor 代码风格各异，各自独立 asmdef 隔离
- 测试覆盖集中在 Core.Tests 和 Unity.Tests

### 关键发现（已修正）

| 发现 | 详情 |
|------|------|
| -langversion:preview 位置 | 6 个 csc.rsp 文件 |
| HN 自有代码 C# 11 特性 | **3 个文件使用 scoped ref（C# 11）**，共 ~54 处；**5 个测试文件** ~14 处。需替换为 ef（语义兼容） |
| MemoryPack（vendored v1.21.4） | 使用 global using（C#10）、static abstract（C#11）、ile-scoped namespace（C#10） |
| FixedMathSharp（vendored） | 使用 ile-scoped namespace（C#10）。DLL 编译需 C# 10+，但作为预编译 DLL 对消费者无影响 |
| XLua / LitMotion / FishNet / HybridCLR | 均无 C# 10+ 特性，C# 9.0 兼容 |
| 工具链 | dotnet CLI 9.0.100 可用，支持 NuGet 包恢复和 .NET Standard 2.1 编译 |

### HN 自有 scoped ref 影响范围

| 文件 | 出现次数 | 修改方式 |
|------|----------|----------|
| Runtime/HN.Framework.Core/Capability/Serialization/FixedMathSharpFormatters.cs | 16 | scoped ref → ef |
| Runtime/HN.Framework.Core/Capability/Network/Messages/MessageFormatters.cs | 6 | scoped ref → ef |
| Runtime/HN.Framework.Unity/Capability/Serialization/UnityFormatters.cs | 32 | scoped ref → ef |
| Tests/HN.Framework.Core.Tests/Serialization/MemoryPackFormatterTests.cs | 4 | scoped ref → ef |
| Tests/HN.Framework.Core.Tests/Serialization/FixedMathSharpFormattersTests.cs | 2 | scoped ref → ef |
| Tests/HN.Framework.Unity.Tests/Serialization/UnityFormattersTests.cs | 2 | scoped ref → ef |
| Tests/HN.Framework.Core.Tests/Level/Logic/Sheet/ConfigLoaderTests.cs | 4 | scoped ref → ef |
| Tests/HN.Framework.Core.Tests/Level/Logic/Sheet/AssetRefSerializationTests.cs | 2 | scoped ref → ef |

> **语义兼容性说明**：scoped 是 C# 11 引入的生命周期注解，仅给编译器提供逃逸分析提示，不影响 IL 生成。移除后方法签名语义不变，编译结果完全兼容。MemoryPack DLL 中接口仍保留 scoped 元数据，但 CLR 不校验此注解，override 时省略 scoped 合法。

---

## 任务分解

### Wave 1（无依赖，可并行）

| ID | 任务 | 描述 | 委派建议 | 验收标准 |
|----|------|------|----------|----------|
| W1-T1 | **从 NuGet 获取 MemoryPack v1.21.4 DLL** | 使用 dotnet CLI 创建临时 .NET Standard 2.1 项目，安装 MemoryPack NuGet 包 v1.21.4，dotnet build 后从输出目录提取 MemoryPack.dll，复制到 Runtime/HN.Framework.Core/Vendor/MemoryPack/ 下。验证 DLL 包含 MemoryPack.IMemoryPackFormatter<T>、MemoryPack.MemoryPackReader 等核心公开 API。 | Sisyphus-junior | MemoryPack.dll 就位，IL 包含预期公开类型 |
| W1-T2 | **自编译 FixedMathSharp DLL** | 在临时目录创建 .NET Standard 2.1 类库项目（.csproj），用 <LangVersion>10</LangVersion> 支持 file-scoped namespace。包含 Runtime/HN.Framework.Core/Vendor/FixedMathSharp/ 下所有 .cs 源文件，允许 unsafe 代码。dotnet build 后复制 FixedMathSharp.dll 到原 Vendor 目录。 | Sisyphus-junior | FixedMathSharp.dll 就位，namespace FixedMathSharp 下所有公开类型可解析 |
| W1-T3 | **验证 asmdef 引用关系** | 读取 HN.Framework.Core.asmdef、HN.Framework.Core.Vendor.MemoryPack.asmdef、HN.Framework.Core.Vendor.FixedMathSharp.asmdef，确认 MemoryPack GUID 3c1b0d1c2470401da464c2f78e21cbbe、FixedMathSharp GUID 644ddb74a15b97f4d9f2aa9d9cba1cb3 的引用链，确认无其他程序集间接引用这两个 GUID（需找到 GUID 对应 .meta 文件确认名称）。 | librarian | 输出引用链路清单，确认波及范围 |

### Wave 2（依赖 Wave 1，可并行）

| ID | 任务 | 描述 | 委派建议 | 验收标准 |
|----|------|------|----------|----------|
| W2-T1 | **替换 HN 自有代码中的 scoped ref → ef** | 对 8 个 HN 自有文件和测试文件中所有 scoped ref 替换为 ef（纯净文本替换，不影响逻辑）。通过 Unity MCP 的 pply_text_edits 操作。操作范围：以上表格列出的 8 个文件。 | Sisyphus-junior | 所有 scoped ref 已替换为 ef，文件语法有效 |
| W2-T2 | **MemoryPack DLL 替换** | 1) 通过 Unity MCP 删除 MemoryPack.Core/ 下所有 .cs 源码文件和 .cs.meta；2) 删除 csc.rsp 及 .meta；3) 修改 HN.Framework.Core.Vendor.MemoryPack.asmdef，添加 "overrideReferences": true，设置 precompiledReferences: ["MemoryPack.dll", "System.Runtime.CompilerServices.Unsafe.dll", "System.Collections.Immutable.dll"]，保留 llowUnsafeCode: true、
oEngineReferences: true；4) 保留 Analyzers/（MemoryPack.Generator.dll）和 Dependencies/ 不变。 | Sisyphus-junior | asmdef 指向 DLL，源码已清理 |
| W2-T3 | **FixedMathSharp DLL 替换** | 1) 通过 Unity MCP 删除所有 .cs 源码文件和 .cs.meta（保留 Core/、Numerics/ 等子目录结构可删空）；2) 删除 csc.rsp 及 .meta；3) 修改 HN.Framework.Core.Vendor.FixedMathSharp.asmdef，添加 "overrideReferences": true，设置 precompiledReferences: ["FixedMathSharp.dll"]，保留 llowUnsafeCode: true、
oEngineReferences: true。 | Sisyphus-junior | asmdef 指向 DLL，源码已清理 |
| W2-T4 | **移除/更新 csc.rsp 文件** | 删除以下 4 个 csc.rsp（含 .meta）：Runtime/HN.Framework.Core/csc.rsp、Runtime/HN.Framework.Unity/csc.rsp、Tests/HN.Framework.Core.Tests/csc.rsp、Tests/HN.Framework.Unity.Tests/csc.rsp。这些文件移除后 Unity 使用默认 C# 9.0 编译。 | Sisyphus-junior | 4 个 csc.rsp 已删除 |
| W2-T5 | **验证保留源码 Vendor 的 csc.rsp 状态** | 确认 XLua、LitMotion、FishNet（Runtime+CodeGen）、HybridCLR 目录下没有独立的 csc.rsp。已确认这些目录无独立配置文件，移除父级 csc.rsp 后自动使用默认 C# 9.0。如发现则一并移除。 | Sisyphus-junior | 无残留 -langversion:preview 配置 |

### Wave 3（依赖 Wave 2 全部完成，共享 Unity 实例 — 必须串行）

| ID | 任务 | 描述 | 委派建议 | 验收标准 |
|----|------|------|----------|----------|
| W3-T1 | **触发 Unity 编译并修复错误** | 通过 efresh_unity（mode: force, scope: scripts, compile: request, wait_for_ready: true）触发全量脚本重新编译。通过 ead_console（	ypes: [error]）检查编译错误。循环修复直到零错误。预期可能出现的错误类型：(a) 遗漏的 scoped ref，(b) asmdef 引用断裂。 | Sisyphus-junior | Unity Console 无编译错误 |
| W3-T2 | **运行项目级 EditMode 测试** | 先运行 HN.Framework.Core.Tests 程序集，通过后运行 HN.Framework.Unity.Tests 程序集。使用 un_tests + get_test_job 轮询。如测试失败，通过 Console 日志排查，修复后重新运行直到全部通过。 | Sisyphus-junior | 两个测试程序集全部通过 |

### Wave 4（依赖 Wave 3 全部通过，可并行）

| ID | 任务 | 描述 | 委派建议 | 验收标准 |
|----|------|------|----------|----------|
| W4-T1 | **更新 architecture.md** | 更新 docs-site~/docs/dev/architecture.md：将 MemoryPack 和 FixedMathSharp 的描述从 "vendored 源码" 改为 "DLL 引用（MemoryPack: NuGet v1.21.4, FixedMathSharp: 自编译 .NET Standard 2.1）"。同步更新模块状态一览表和依赖关系说明。 | Sisyphus-junior | 文档反映当前依赖方式 |
| W4-T2 | **更新 serialize.md** | 更新 docs-site~/docs/guide/serialize.md：修改 "以独立 asmdef 形式 vendored" 为 "以 DLL 形式引用（NuGet v1.21.4）"，补充说明 MemoryPack.Generator.dll 保持不变。 | Sisyphus-junior | 文档准确 |
| W4-T3 | **更新 hot-update-scripting.md** | 更新 docs-site~/docs/guide/hot-update-scripting.md：确认 xLua 和 HybridCLR 描述（保留源码，C# 9.0），如需补充语言版本说明则添加。 | Sisyphus-junior | 文档准确 |
| W4-T4 | **更新 network.md** | 更新 docs-site~/docs/guide/network.md：确认 FishNet 描述（保留源码，C# 9.0），CodeGen 保留源码。 | Sisyphus-junior | 文档准确 |
| W4-T5 | **更新 ui.md** | 更新 docs-site~/docs/guide/ui.md：确认 LitMotion 描述（保留源码，C# 9.0）。 | Sisyphus-junior | 文档准确 |
| W4-T6 | **更新 api/index.md** | 更新 docs-site~/docs/api/index.md：MemoryPack 和 FixedMathSharp 条目标注为 "DLL 引用"；所有 Vendor 条目补充 "Language Version: C# 9.0" 说明（如有必要）。 | Sisyphus-junior | API 索引准确 |

---

## 依赖图

`mermaid
flowchart TD
    subgraph Wave1["Wave 1 (并行)"]
        W1T1["W1-T1: NuGet 获取 MemoryPack DLL"]
        W1T2["W1-T2: 自编译 FixedMathSharp DLL"]
        W1T3["W1-T3: 验证 asmdef 引用关系"]
    end

    subgraph Wave2["Wave 2 (并行)"]
        W2T1["W2-T1: scoped ref → ref (8 文件)"]
        W2T2["W2-T2: MemoryPack DLL 替换"]
        W2T3["W2-T3: FixedMathSharp DLL 替换"]
        W2T4["W2-T4: 移除 4 个 csc.rsp"]
        W2T5["W2-T5: 验证 Vendor csc.rsp 状态"]
    end

    subgraph Wave3["Wave 3 (串行，共享 Unity 实例)"]
        W3T1["W3-T1: Unity 编译 + 修复"]
        W3T2["W3-T2: 运行 EditMode 测试"]
    end

    subgraph Wave4["Wave 4 (并行)"]
        W4T1["W4-T1: update architecture.md"]
        W4T2["W4-T2: update serialize.md"]
        W4T3["W4-T3: update hot-update-scripting.md"]
        W4T4["W4-T4: update network.md"]
        W4T5["W4-T5: update ui.md"]
        W4T6["W4-T6: update api/index.md"]
    end

    W1T1 --> W2T2
    W1T2 --> W2T3
    W1T3 --> W2T2
    W1T3 --> W2T3

    W2T1 --> W3T1
    W2T2 --> W3T1
    W2T3 --> W3T1
    W2T4 --> W3T1
    W2T5 --> W3T1

    W3T1 --> W3T2
    W3T2 --> W4T1
    W3T2 --> W4T2
    W3T2 --> W4T3
    W3T2 --> W4T4
    W3T2 --> W4T5
    W3T2 --> W4T6
`

---

## 风险与缓解

| 风险 | 可能性 | 影响 | 缓解策略 |
|------|--------|------|----------|
| **NuGet MemoryPack v1.21.4 DLL 与 vendored 源码 API 不一致** | 中 | 高 — 编译时类型缺失 | W1-T1 中先 dotnet build 编译临时项目验证 DLL 包含 IMemoryPackFormatter<T>、MemoryPackReader 等核心类型；如不匹配则考虑用 vendored 源码自编译 DLL（C# 11 编译） |
| **FixedMathSharp 自编译遗漏文件** | 低 | 中 — 运行时缺失类型 | W1-T2 确认所有 .cs 文件纳入编译，dotnet build 输出无警告 |
| **scoped ref 替换遗漏** | 低 | 中 — 编译错误 | W2-T1 全局搜索 scoped ref 确保覆盖；W3-T1 编译会暴露遗漏 |
| **asmdef GUID 引用断裂** | 低 | 低 — 仅影响 GUID 引用 | W1-T3 提前梳理引用关系；MemoryPack/FixedMathSharp 的 asmdef 保留名称不变，引用方通过名称匹配 |
| **Unity 编译缓存导致旧代码残留** | 低 | 中 — 看似通过实则失败 | W3-T1 使用 efresh_unity(mode: force) 强制重编译 |

---

## 技术细节备注

### MemoryPack DLL 替换方案

`
当前:
  Runtime/HN.Framework.Core/Vendor/MemoryPack/
  ├── MemoryPack.Core/*.cs          ← 源码（删除）
  ├── csc.rsp                        ← 删除
  ├── HN.Framework.Core.Vendor.MemoryPack.asmdef
  ├── Analyzers/MemoryPack.Generator.dll  ← 保留
  └── Dependencies/*.dll            ← 保留

目标:
  Runtime/HN.Framework.Core/Vendor/MemoryPack/
  ├── MemoryPack.dll                ← NuGet DLL (新增)
  ├── HN.Framework.Core.Vendor.MemoryPack.asmdef  ← 修改为 DLL 引用
  ├── Analyzers/MemoryPack.Generator.dll  ← 保留不变
  └── Dependencies/*.dll            ← 保留不变
`

asmdef 修改为：
`json
{
    "name": "HN.Framework.Core.Vendor.MemoryPack",
    "rootNamespace": "MemoryPack",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": true,
    "overrideReferences": true,
    "precompiledReferences": [
        "MemoryPack.dll",
        "System.Runtime.CompilerServices.Unsafe.dll",
        "System.Collections.Immutable.dll"
    ],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": true
}
`

### FixedMathSharp DLL 替换方案

同理，删除所有 .cs 源码，添加 FixedMathSharp.dll 到 precompiledReferences。

### MemoryPack NuGet DLL 获取步骤

`ash
mkdir temp_memorypack && cd temp_memorypack
dotnet new classlib -n TempProj -f netstandard2.1
cd TempProj
dotnet add package MemoryPack --version 1.21.4
dotnet build -c Release
# DLL 位于 bin/Release/netstandard2.1/MemoryPack.dll
`

### FixedMathSharp 自编译步骤

创建临时 .csproj：
`xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <LangVersion>10</LangVersion>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <AssemblyName>FixedMathSharp</AssemblyName>
    <RootNamespace>FixedMathSharp</RootNamespace>
  </PropertyGroup>
</Project>
`
复制所有源文件，dotnet build -c Release，输出 FixedMathSharp.dll。

---

*计划版本：v2（已根据 Momus 审核修正）*
*生成时间：2026-07-14 14:38*
*审核通过后使用 start-work 启动 Atlas 执行*
