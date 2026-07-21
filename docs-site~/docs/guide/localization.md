---
sidebar_position: 14
---

# 本地化系统 (C3)

HNUnityFramework 的本地化系统提供端到端的多语言支持，从字符串表管理到 TMP 文本自动更新，覆盖完整的本地化工作流。

## 架构

```
┌─────────────────────────────────────────────────────────────┐
│                    GameWorld.LocaleProvider                   │
│                         (ILocaleProvider)                     │
├─────────────────────────────────────────────────────────────┤
│                      LocaleManager                           │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────────────┐  │
│  │ StringTable  │  │ LocaleSelector│  │ ILocaleDataLoader  │  │
│  │  字典映射    │  │ PlayerPrefs   │  │   数据加载接口     │  │
│  └─────────────┘  │ 持久化偏好    │  └────────────────────┘  │
│                   └──────────────┘         ↕                    │
│                                    AddressableStringTableLoader │
│                                    (Addressables JSON 加载)     │
├─────────────────────────────────────────────────────────────┤
│                     TextLocalizer (MonoBehaviour)             │
│             自动监听语言切换，更新 TMP_Text 文本                │
├─────────────────────────────────────────────────────────────┤
│                     AssetLocalizer (静态工具类)                 │
│             按路径拼接规则加载不同语言的 Addressables 资源       │
└─────────────────────────────────────────────────────────────┘
```

## 快速开始

### 1. 准备本地化数据

创建 JSON 格式的字符串表文件，放在 Addressables 可寻址的位置：

```json
{
  "entries": [
    { "key": "ui_start", "value": "开始游戏" },
    { "key": "ui_settings", "value": "设置" },
    { "key": "ui_quit", "value": "退出" }
  ]
}
```

Addressables key 规则：`{baseKey}_{locale.Code}`（默认 baseKey 为 "locale"）。

例如：
- `locale_zh-CN` → 中文简体字符串表
- `locale_en-US` → 英文字符串表
- `locale_ja-JP` → 日文字符串表

### 2. 注册可用语言

`LocaleManager` 由 `GameWorldDriver` 在 `Awake` 中自动创建，默认注册了 `zh-CN`、`en-US`、`ja-JP`、`ko-KR`、`zh-TW` 五种语言。

如果需要自定义可用语言列表或回退语言，重写 `GameWorldDriver.OnRegisterGameModules`：

```csharp
protected override void OnRegisterGameModules(GameWorld world)
{
    // 替换默认的 LocaleProvider
    var customLocales = new List<Locale> { Locale.zhCN, Locale.enUS };
    var loader = new AddressableStringTableLoader("my_locale");
    var localeManager = new LocaleManager(loader, customLocales, Locale.zhCN);
    world.LocaleProvider = localeManager;
}
```

### 3. 在 UI 中使用 TextLocalizer

将 `TextLocalizer` 组件挂载到含有 `TMP_Text` 的 GameObject 上：

1. 在 Hierarchy 中选择 Text (TMP) 对象
2. 添加 Component → `TextLocalizer`
3. 在 Inspector 中设置 `_key` 字段为本地化键（如 `"ui_start"`）

运行时会自动：
- 在 `Awake` 时获取当前语言的文本
- 监听 `OnLocaleChanged` 事件，语言切换时自动刷新

### 4. 代码中查询本地化字符串

通过 `GameWorld` 访问 `LocaleProvider`：

```csharp
var world = GameWorldDriver.Instance.World; // 或通过依赖注入
string text = world.LocaleProvider.GetString("ui_start");
```

### 5. 切换语言

```csharp
world.LocaleProvider.SetLocale(Locale.enUS);
```

切换后：
- `OnLocaleChanged` 事件触发
- 所有 `TextLocalizer` 自动更新文本
- 语言偏好保存到 `PlayerPrefs`

## 资产本地化

`AssetLocalizer` 提供按语言加载资源的静态方法：

```csharp
// 路径规则：{basePath}/{locale.Code}/{assetName}
// 例如：Assets/Localization/Textures/zh-CN/icon_health.png

// 回调式异步加载
AssetLocalizer.LoadLocalizedAssetAsync<Texture2D>(
    Locale.zhCN,
    "Assets/Localization/Textures",
    "icon_health.png",
    texture => { /* 使用 texture */ }
);

// 协程式加载
StartCoroutine(AssetLocalizer.LoadLocalizedAssetCoroutine<Sprite>(
    Locale.zhCN,
    "Assets/Localization/Sprites",
    "item_sword.png",
    sprite => { /* 使用 sprite */ }
));

// 获取路径（用于 Addressables.Label 或 Resources.Load）
string path = AssetLocalizer.GetLocalizedAssetPath(
    "Assets/Localization/Prefabs", Locale.enUS, "ui_panel.prefab"
);
// 返回: "Assets/Localization/Prefabs/en-US/ui_panel.prefab"
```

## 高级用法

### 自定义数据加载器

实现 `ILocaleDataLoader` 接口从自定义数据源加载字符串表：

```csharp
public class MyCustomLoader : ILocaleDataLoader
{
    public async Task<StringTable> LoadTableAsync(Locale locale, CancellationToken ct = default)
    {
        // 从远程 API、Resources 或其他数据源加载
        // ...
        return new StringTable(entries);
    }

    public bool IsAvailable(Locale locale) { /* ... */ }
}

// 使用时注入
var loader = new MyCustomLoader();
var manager = new LocaleManager(loader, locales, Locale.zhCN);
```

### 预加载所有语言

在场景加载时预加载所有语言的字符串表，消除切换语言时的延迟：

```csharp
var localeManager = (LocaleManager)world.LocaleProvider;
await localeManager.PreloadAllTablesAsync();
```

## API 参考

| 类型 | 说明 |
|------|------|
| `LocaleManager` | 实现 `ILocaleProvider`，管理字符串表和语言切换 |
| `ILocaleDataLoader` | 数据加载接口，可注入自定义实现 |
| `AddressableStringTableLoader` | Addressables JSON 加载器 |
| `LocaleSelector` | PlayerPrefs 语言偏好持久化 |
| `TextLocalizer` | MonoBehaviour，自动更新 TMP 文本 |
| `AssetLocalizer` | 静态工具类，按路径加载本地化资源 |

## 依赖

- `Unity.TextMeshPro` — TextLocalizer 依赖
- `Unity.Addressables` — AddressableStringTableLoader 和 AssetLocalizer 依赖
- 所有依赖在 `HN.Framework.Unity.asmdef` 中已声明

## 注意事项

- **StringTable 默认降级**：键不存在时返回键本身，不会抛出异常
- **回退策略**：当前语言 → 回退语言（默认为 zh-CN）→ 返回键本身
- **测试**：28 个 EditMode 单元测试覆盖 LocaleManager / TextLocalizer / LocaleSelector
- **Editor 工具 (E4)**：字符串表编辑器窗口目前规划中，暂未实现
