---
sidebar_position: 17
---

# 配置表

> 🚧 **Stub** — 本文档为骨架占位，待后续补充。

Sheet 模块提供配置表管理能力，与 **Luban** 外部工具和 **Sheet 编辑器**（框架内）协同工作。

## 协作流程

1. **策划**在 Excel 中维护配置数据
2. **Luban** 将 Excel 导出为 C# 代码（`Tables` 类）与二进制数据文件（`.bin`）
3. **Sheet 编辑器**（Unity Editor）加载数据，为资源引用字段选择 Addressables 资源
4. 运行时 **Luban** 加载 `.bin` 数据，**Sheet** 查询资源地址，通过 `Addressables.LoadAssetAsync()` 加载

## 分工

| 工具 | 负责 | 产出 |
|------|------|------|
| **Luban** | Excel/CSV → C# 代码 + 二进制数据导出 | `Tables` 类 + `.bin` 文件 |
| **Sheet 编辑器** | Unity Editor 中编辑资源引用字段 | ScriptableObject，存储行 → Addressables 映射 |
