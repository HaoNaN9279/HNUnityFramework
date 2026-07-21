---
sidebar_position: 12
---

# 工具类

Utils 模块提供通用工具类，位于命名空间 `HN.Framework.Unity.Driver.Platform.DataStructures`。目前包含两种可序列化字典实现。

## 核心类型

| 类型 | 命名空间 | 说明 |
|------|---------|------|
| `HNDictionary<TKey, TValue>` | `HN.Framework.Unity.Driver.Platform.DataStructures` | 功能完整的可序列化字典，实现 `IDictionary<TKey,TValue>` |
| `SerializableDictionary<K, V>` | `HN.Framework.Unity.Driver.Platform.DataStructures` | 轻量可序列化字典，直接在 Inspector 中编辑 |

## HNDictionary — 可序列化字典

`HNDictionary<TKey, TValue>` 是一个功能完整的 `IDictionary<TKey, TValue>` 实现，支持在 Unity Inspector 中编辑。

### 特性

- 实现完整的 `IDictionary<TKey, TValue>` 接口
- 内部使用 `List<SerializableKeyValuePair>` 存储（可在 Inspector 中编辑）
- 维护 `Dictionary<TKey, int>` 索引加速按键查找（O(1) 查询）
- 支持序列化（`ISerializationCallbackReceiver`）

### 使用示例

```csharp
using HN.Framework.Unity.Driver.Platform.DataStructures;
using UnityEngine;

public class ConfigHolder : MonoBehaviour
{
    [SerializeField]
    private HNDictionary<string, int> scores = new HNDictionary<string, int>();

    void Start()
    {
        // 添加
        scores.Add("Player1", 100);
        scores["Player2"] = 200;

        // 访问
        int p1Score = scores["Player1"];

        // 检查
        if (scores.ContainsKey("Player3"))
        {
            // ...
        }

        // 遍历
        foreach (var kvp in scores)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }

        // 移除
        scores.Remove("Player1");
    }
}
```

### 在 Inspector 中的显示

使用 `HNDictionaryDrawer`（`[CustomPropertyDrawer(typeof(HNDictionary), true)]`）在 Inspctor 中绘制为可折叠的键值对列表，支持增删改。

## SerializableDictionary — 轻量可序列化字典

`SerializableDictionary<K, V>` 是更简单的实现，直接继承 `Dictionary<K, V>` 并实现序列化：

```csharp
using HN.Framework.Unity.Driver.Platform.DataStructures;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    [SerializeField]
    private SerializableDictionary<int, ItemData> items = 
        new SerializableDictionary<int, ItemData>();
}
```

### 序列化机制

```
OnBeforeSerialize (保存时):
  遍历字典 → keys 列表 + values 列表

OnAfterDeserialize (加载时):
  keys/values 列表 → 重建字典
```

## 对比

| 特性 | HNDictionary | SerializableDictionary |
|------|-------------|----------------------|
| 基类 | `HNDictionary` (自定义) | `Dictionary<K,V>` |
| 实现接口 | `IDictionary<TKey,TValue>` | `Dictionary<K,V>` (继承) |
| 内部存储 | `List<SerializableKeyValuePair>` | `List<K>` + `List<V>` |
| 查询性能 | O(1) 索引加速 | O(1) Dictionary 原生 |
| Inspector 编辑 | 专用 PropertyDrawer | 默认双列表 |
| 适用场景 | 需要完整字典接口 + Inspector 编辑 | 简单序列化需求 |

## 编辑器支持

框架提供了 `SerializableDictionaryDrawer`（`[CustomPropertyDrawer(typeof(HNDictionary), true)]`），在 Inspector 中自动使用可折叠的键值对列表替代默认的双 List 显示方式。

## 注意事项

- `HNDictionary` 的键类型需要实现正确的 `Equals`/`GetHashCode`（用于内部索引字典）
- 两者都要求键值类型标记 `[Serializable]`
- 序列化/反序列化在 Inspector 保存/加载时自动触发，无需手动干预
- `HNDictionary` 的 `Remove` 操作会更新后续所有元素的索引（O(n)），频繁增删的场景建议预处理完后批量操作
