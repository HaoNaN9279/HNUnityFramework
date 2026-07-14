---
sidebar_position: 11
---

# 序列化

Serialize 模块提供 JSON 序列化/反序列化工具，按依赖拆分为两层。

## 概述

序列化模块按依赖拆分为 Core 和 Unity 两层：

| 类型 | 层 | 命名空间 | 说明 |
|------|-----|----------|------|
| `Json` | Core | `HN.Framework.Core.Driver.Common.Serialization` | 静态工具类，直接读写 JSON 文件/字符串 |
| `JsonObject` | Core | `HN.Framework.Core.Driver.Common.Serialization` | 数据基类，供 JsonData 内部使用 |
| `JsonData` | Unity | `HN.Framework.Unity.Driver.Platform.Serialization` | 可序列化的数据容器，自动处理 ISerializationCallbackReceiver |

> **分层说明**：Core 层（`Json`/`JsonObject`）为纯 C# 实现，不依赖 Unity 运行时；`Json` 类为纯 C# 手写 JSON 序列化器。Unity 层（`JsonData`）依赖 `ISerializationCallbackReceiver`，内部调用 Core 层的 `Json` 进行序列化，用于 Inspector 编辑场景。

## Json 静态类

提供文件和字符串的双向序列化方法。

### 序列化到文件

```csharp
using HN.Framework.Core.Driver.Common.Serialization;

[Serializable]
public class GameConfig
{
    public int Volume;
    public float Sensitivity;
}

// 序列化到文件
var config = new GameConfig { Volume = 80, Sensitivity = 1.5f };
Json.Serialize(config, "Assets/Config/game_config.json");

// 序列化为字符串
string json = Json.Serialize(config);
```

### 从文件反序列化

```csharp
// 方式一：创建新对象
var config = Json.Deserialize<GameConfig>("Assets/Config/game_config.json");

// 方式二：覆盖已有对象
var existingConfig = new GameConfig();
Json.Deserialize(existingConfig, "Assets/Config/game_config.json");
```

### 字符串反序列化

```csharp
// 创建新对象
var config = Json.DeserializeFromString<GameConfig>(jsonString);

// 覆盖已有对象
Json.DeserializeFromString(existingConfig, jsonString);

// 通过类型名动态反序列化
var obj = Json.DeserializeFromString("MyNamespace.MyType, MyAssembly", jsonString);
```

### 文件读写

```csharp
// 直接写文本到文件
Json.WriteToDisk("path/to/file.txt", "content");

// 直接读文本
string content = Json.ReadFromDisk("path/to/file.txt");
```

## JsonData — 可序列化数据容器

`JsonData` 实现了 `ISerializationCallbackReceiver`，适用于需要在 Inspector 中编辑 JSON 数据的场景。

```csharp
using HN.Framework.Core.Driver.Common.Serialization;
using HN.Framework.Unity.Driver.Platform.Serialization;
using UnityEngine;

[Serializable]
public class MyGameData : JsonObject
{
    public int Score;
    public string PlayerName;
}

public class DataHolder : MonoBehaviour
{
    [SerializeField]
    private JsonData gameData;  // 在 Inspector 中可编辑

    void Start()
    {
        // 创建 JsonData，指定数据类型
        gameData = new JsonData(typeof(MyGameData));
        (gameData.Obj as MyGameData).Score = 100;
        
        // 手动序列化
        gameData.Serialize();
        Debug.Log(gameData.JsonText);  // {"Score":100,"PlayerName":""}
    }
}
```

### 序列化回调流程

```
OnBeforeSerialize (Inspector 保存时):
  → Serialize() → Obj 序列化为 JSON → 存入 jsonText

OnAfterDeserialize (Inspector 加载时):
  → DeserializeFromString(jsonText) → JSON 反序列化回 Obj
```

`JsonData` 内部保存了类型信息（`objTypeName` + `objAssemblyName`），确保反序列化时能正确重建对象。

## JsonObject

`JsonObject` 是所有数据类型的基类（空类，仅标记 `[Serializable]`）：

```csharp
[Serializable]
public class JsonObject { }
```

所有需要和 `JsonData` 配合使用的数据类型都需继承 `JsonObject`：

```csharp
[Serializable]
public class PlayerData : JsonObject
{
    public string Name;
    public int Level;
}
```

## 注意事项

- 支持基本类型（int/float/double/string/bool 等）、数组、`List<T>`、`Dictionary<K,V>`、嵌套对象等；类型需标记为 `[Serializable]`
- 枚举序列化为整数值（如 `0`、`1`），反序列化时兼容读取旧的字符串格式（如 `"ValueName"`）
- `JsonData` 需要配合 `JsonObject` 子类使用
- 文件读写使用 `System.IO.File` API，在 WebGL 等平台不可用（需平台适配）
- `DeserializeFromString` 在 JSON 为空时自动使用 `"{}"` 避免报错

## MemoryPack 二进制序列化

MemoryPack 是高性能 C# 二进制序列化库，性能约为 JsonUtility 的 3-10 倍，且支持零 GC 分配。

### JSON 与 MemoryPack 的选择

| 场景 | 推荐方案 | 原因 |
|------|---------|------|
| Inspector 编辑、配置文件 | JSON | 人类可读，可直接在编辑器中修改 |
| 网络消息、存档、配置表运行时加载 | MemoryPack | 高性能 + 小体积 + 零 GC |
| 需要跨语言/跨平台交换 | JSON | 语言无关，通用性强 |
| 高性能要求的内网通信 | MemoryPack | 带宽和速度优势明显 |

### 核心类型

序列化模块按依赖拆分为 Core 和 Unity 两层：

| 类型 | 层 | 命名空间 | 说明 |
|------|-----|----------|------|
| `MemoryPackSerializer` | Core | `HN.Framework.Core.Driver.Common.Serialization` | 静态包装器，封装 vendored MemoryPack API |
| `ISerializer` | Core | `HN.Framework.Core.Capability.Serialization` | 统一序列化接口，支持泛型和非泛型 |
| `MemoryPackFormatterProvider` | Core | `HN.Framework.Core.Capability.Serialization` | 格式化器注册提供器，框架友好封装 |
| `UnityFormatters` | Unity | `HN.Framework.Unity.Capability.Serialization` | 16 种 Unity 内置类型格式化器 |
| `UnityFormattersInitializer` | Unity | `HN.Framework.Unity.Capability.Serialization` | 批量注册所有 Unity 格式化器 |

### ISerializer 接口

```csharp
public interface ISerializer
{
    byte[] Serialize<T>(T obj);
    T Deserialize<T>(byte[] data);
    byte[] Serialize(Type type, object obj);
    object Deserialize(Type type, byte[] data);
}
```

默认实现委托给 MemoryPack 引擎。

### MemoryPackSerializer 使用示例

```csharp
using HN.Framework.Core.Driver.Common.Serialization;

[MemoryPackable]
public partial class PlayerData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Score { get; set; }
}

// 序列化
var player = new PlayerData { Id = 1, Name = "Alice", Score = 99.5f };
byte[] data = MemoryPackSerializer.Serialize(player);

// 反序列化
var restored = MemoryPackSerializer.Deserialize<PlayerData>(data);

// 流式读写
using var stream = File.OpenWrite("player.bin");
MemoryPackSerializer.Serialize(stream, player);
```

### `[MemoryPackable]` 属性

所有需要通过 MemoryPack 序列化的类型必须标记 `[MemoryPackable]` 属性（使用 Source Generator 模式须为 `partial class`）：

```csharp
[MemoryPackable]
public partial class PlayerData
{
    [MemoryPackIgnore]
    public int NotSerialized; // 跳过字段

    [MemoryPackInclude]
    private int includedField; // 显式包含私有字段
}
```

### Unity 类型格式化器

`UnityFormatters` 包含以下 16 种 Unity 内置类型的 MemoryPack 格式化器：

| 类型 | 格式化器 | 说明 |
|------|---------|------|
| `Vector2` | `Vector2Formatter` | 2D 向量 |
| `Vector3` | `Vector3Formatter` | 3D 向量 |
| `Vector4` | `Vector4Formatter` | 4D 向量 |
| `Vector2Int` | `Vector2IntFormatter` | 2D 整数向量 |
| `Vector3Int` | `Vector3IntFormatter` | 3D 整数向量 |
| `Quaternion` | `QuaternionFormatter` | 旋转四元数 |
| `Color` | `ColorFormatter` | RGBA 颜色 |
| `Color32` | `Color32Formatter` | 字节颜色 |
| `Bounds` | `BoundsFormatter` | 包围盒 |
| `BoundsInt` | `BoundsIntFormatter` | 整数包围盒 |
| `Rect` | `RectFormatter` | 矩形 |
| `RectInt` | `RectIntFormatter` | 整数矩形 |
| `Matrix4x4` | `Matrix4x4Formatter` | 4x4 变换矩阵 |
| `LayerMask` | `LayerMaskFormatter` | 层级遮罩 |
| `AnimationCurve` | `AnimationCurveFormatter` | 动画曲线 |
| `Gradient` | `GradientFormatter` | 渐变 |

### 格式化器初始化

在 GameWorld 初始化阶段调用 `UnityFormattersInitializer.RegisterAll()` 批量注册所有 Unity 类型格式化器：

```csharp
using HN.Framework.Unity.Capability.Serialization;

// 建议在 GameWorld 初始化阶段调用
UnityFormattersInitializer.RegisterAll();
```

`RegisterAll` 内部会调用 `MemoryPackFormatterProvider.Register<T>()` 逐一注册每个格式化器，同时委托给 MemoryPack 的全局注册表。

### IL2CPP 兼容性与 Source Generator 限制

- **IL2CPP 环境**：MemoryPack 依赖运行时反射进行序列化。在 IL2CPP 下，建议使用 Source Generator 生成序列化代码以避免 AOT 问题。
- **Source Generator 模式**：需为每个可序列化类型添加 `[MemoryPackable]` 属性并将类标记为 `partial`。生成代码由 MemoryPack 的 `csgen` 工具自动完成。`MemoryPack.Generator.dll` 源码生成器保留在 `Vendor/MemoryPack/Analyzers/` 下。
- **Unity 类型格式化器**：`UnityFormatters` 中的 16 种格式化器为手动编写，不依赖 Source Generator，在 IL2CPP 下正常工作。
- **DLL 引用**：项目使用 MemoryPack v1.21.4，以 DLL 形式引用（从 NuGet 获取），位于 `Runtime/HN.Framework.Core/Vendor/MemoryPack/MemoryPack.dll`，不通过 NuGetForUnity 管理。

### 性能对比

MemoryPack 相比框架内置的 JSON 序列化（`Json.Serialize`）：

| 指标 | JsonUtility | MemoryPack |
|------|:-----------:|:----------:|
| 吞吐量 | 1x（基准） | 3-10x 更快 |
| GC 分配 | 每次调用分配 | 零分配（`ArrayPool` 缓冲） |
| 数据体积 | UTF-8 文本 | 紧凑二进制 |
| Unity 类型支持 | 有限 | 16 种内置格式化器 |
