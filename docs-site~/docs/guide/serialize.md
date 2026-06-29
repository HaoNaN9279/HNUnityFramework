---
sidebar_position: 11
---

# 序列化

Serialize 模块提供 JSON 序列化/反序列化工具，位于命名空间 `HN.Serialize`。

## 概述

模块提供三个层次的序列化支持：

| 类型 | 说明 |
|------|------|
| `Json` | 静态工具类，直接读写 JSON 文件/字符串 |
| `JsonData` | 可序列化的数据容器，自动处理 ISerializationCallbackReceiver |
| `JsonObject` | 数据基类，供 JsonData 内部使用 |

所有序列化基于 Unity 的 `JsonUtility`，因此仅支持标记了 `[Serializable]` 的类型。

## Json 静态类

提供文件和字符串的双向序列化方法。

### 序列化到文件

```csharp
using HN.Serialize;

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
using HN.Serialize;
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

- 序列化基于 `JsonUtility`，只支持 `[Serializable]` 标记的类型
- 不支持字典、多维数组等复杂类型（Unity JsonUtility 限制）
- `JsonData` 需要配合 `JsonObject` 子类使用
- 文件读写使用 `System.IO.File` API，在 WebGL 等平台不可用（需平台适配）
- `DeserializeFromString` 在 JSON 为空时自动使用 `"{}"` 避免报错
