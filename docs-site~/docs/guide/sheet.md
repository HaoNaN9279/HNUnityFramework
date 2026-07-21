---
sidebar_position: 17
---

# 配置表系统 (Sheet)

## 概述

配置表系统是 HNUnityFramework 中对接 **Luban** 配置工具的数据层，提供运行时配置数据查询、资产引用解析和 Unity Editor 内 Excel 编辑能力。

### 核心能力

- **运行时查询** — 通过 `ISheetManager` / `IConfigTable<TKey, TRow>` 接口实现 O(1) 查询配置数据
- **资产引用** — `AssetRef<T>` 存储 Addressables Label，运行时自动解析
- **MemoryPack 序列化** — 高性能二进制加载，后续支持 Luban ByteBuf
- **Editor 内编辑** — Unity Editor 内直接编辑 Excel 配置表，支持资产引用单元格拖拽

## 运行时架构

```
┌─────────────────────────────────────────────────────────────┐
│                     运行时数据流                                │
│                                                              │
│  Luban CLI                                                    │
│    │                                                          │
│    ├──→ *.bin (MemoryPack 二进制)                              │
│    │                                                          │
│    ▼                                                          │
│  ConfigLoader.LoadFromBytes<T>(byte[])                        │
│    │                                                          │
│    ▼                                                          │
│  LubanTablesAdapter.LoadTables<TTables>(manager, data)        │
│    │                                                          │
│    ▼                                                          │
│  cfg.Tables (实现了 ISheetRegistrar)                           │
│    │                                                          │
│    ├──→ ISheetManager.RegisterTable("TbMonster", table)       │
│    │                                                          │
│    ▼                                                          │
│  游戏代码查询: manager.GetTable<int, MonsterRow>("TbMonster") │
│    │                                                          │
│    ▼                                                          │
│  AssetRef<T>.LoadAssetAsync<Sprite>() → Addressables          │
└─────────────────────────────────────────────────────────────┘
```

## 快速入门

### 1. 安装与配置

确保项目中已安装以下依赖：

- HNUnityFramework Core + Unity 程序集
- Addressables 包
- MemoryPack 包（vendored 在框架内）
- Luban CLI（外部工具，用于生成代码和数据）

### 2. 创建 SheetManager

```csharp
using HN.Framework.Core.Level.Logic.Sheet;
using HN.Framework.Unity.Capability.Sheet;

var sheetManager = new SheetManager();
```

### 3. 加载并注册配置表

使用 `LubanTablesAdapter` 加载 Luban 生成的二进制数据：

```csharp
// 从 Addressables 或 Resources 加载 .bin 文件
byte[] binData = await Addressables.LoadAssetAsync<TextAsset>("TbMonster").Task
    .ContinueWith(t => t.Result.bytes);

// 反序列化并注册到 SheetManager
LubanTablesAdapter.LoadTables<cfg.Tables>(sheetManager, binData);
```

实现 `ISheetRegistrar` 的 `cfg.Tables` 会自动调用 `RegisterTable` 注册所有表。

### 4. 查询数据

```csharp
// 获取表
var monsterTable = sheetManager.GetTable<int, MonsterRow>("TbMonster");

// 通过主键查询
var monster = monsterTable.Get(1001);
Debug.Log($"Monster: {monster.Name}, HP: {monster.HP}");

// 安全查询
if (monsterTable.TryGet(1002, out var other))
{
    Debug.Log($"Found: {other.Name}");
}

// 遍历所有行
foreach (var row in monsterTable.GetAll())
{
    ProcessMonster(row);
}

// 检查是否存在
if (monsterTable.ContainsKey(2001))
{
    // ...
}

// 获取行数
int count = monsterTable.Count;
```

### 5. 资产引用 (AssetRef)

`AssetRef<T>` 在 Core 层定义，使用 MemoryPack 序列化，仅存储 Addressables Label 字符串。Unity 侧提供扩展方法进行异步加载。

```csharp
// 配置表中定义: MonsterRow.Icon 为 AssetRef<Sprite> 类型

// 加载资源
var icon = await monster.Icon.LoadAssetAsync<Sprite>();
image.sprite = icon;

// 释放资源
monster.Icon.ReleaseAsset(icon);
```

AssetRef 是只读结构体，支持相等比较：

```csharp
if (monster.Icon.IsValid)
{
    // 有有效引用
}

if (monster.Icon == AssetRef<Sprite>.Empty)
{
    // 空引用
}
```

## Editor 使用

### Sheet Editor

通过菜单 `HNFramework → Sheet Editor` 打开配置表编辑器窗口。

**工具栏：**
- Open — 打开 .xlsx 文件
- Save — 保存修改到源文件
- Refresh — 重新加载数据

**网格视图 (SheetGrid)：**
- 冻结表头行，滚动时表头始终可见
- 列宽拖拽调整
- 按类型分发单元格编辑器

**资产引用单元格 (AssetRefCell)：**
- 显示 64×64 缩略图预览
- 从 Project 窗口拖拽资源到单元格替换引用
- 右键菜单清除引用
- 点击定位到资源

### 数据模型

| 类型 | 说明 |
|------|------|
| `TableModel` | 表模型，包含表名、列定义、行数据 |
| `ColumnDef` | 列定义（字段名、类型、索引） |
| `RowData` | 行数据，包含单元格数组 |
| `CellData` | 单元格数据，支持多种类型 |
| `SchemaProvider` | Excel schema 解析器，解析字段名和类型标注 |
| `ExcelSourceParser` | .xlsx 文件解析器，基于 ClosedXML |
| `ExcelSerializer` | 修改后写回 .xlsx |

## 注意事项

- **编辑器依赖 ClosedXML** — 用于解析和写入 .xlsx 文件，仅在 Editor 环境下使用
- **AssetRef 资源管理** — 使用完资源后调用 `ReleaseAsset` 释放，避免内存泄漏
- **Addressables Label** — AssetRef 存储的是 Addressables Label，资源必须已设置对应 Label
- **Luban 集成** — 框架提供运行时接口和 Editor 工具，Luban 代码生成和数据导出需额外配置
- **Phase 2 计划** — 后续将支持 Luban ByteBuf（cs-bin 模式）替代 MemoryPack 作为主要序列化格式
