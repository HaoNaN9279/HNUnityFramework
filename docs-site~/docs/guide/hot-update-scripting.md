---
sidebar_position: 19
---

# 热更新与脚本系统

HNUnityFramework 采用**双引擎策略**实现热更新和脚本扩展：

- **HybridCLR** — C# 代码热更新（性能关键路径，逻辑修复、玩法更新）
- **xLua** — Lua Mod 脚本系统（用户自定义 Mod，沙箱隔离）

## 架构定位

| 系统 | 定位 | 适用场景 |
|------|------|---------|
| **HybridCLR** | C# 程序集热更新 | 线上 Bug 修复、玩法逻辑更新、性能关键路径 |
| **xLua Mod** | Lua 沙箱脚本 | 用户自定义 Mod、活动脚本、低风险热更 |

### 架构关系

```
HN.Framework.Core.Capability.Scripting
  ┌────────────────────────────────┐
  │  IScriptEngine (interface)     │  ← 脚本引擎抽象
  │  IScriptMod (interface)        │  ← Mod 生命周期
  │  ModState (enum)               │  ← 状态枚举
  │  ScriptModConfig (class)       │  ← 配置数据模型
  │  IHotUpdateEntry (interface)   │  ← 热更新入口
  └────────────────────────────────┘
            ▲            ▲
            │            │ implements
            │            │
HN.Framework.Unity.Capability.Scripting
  ┌────────────────┐  ┌──────────────────────┐
  │ HybridCLRAdapter │  │  LuaModManager        │
  │ · AOT 元数据加载  │  │ · Mod 生命周期管理     │
  │ · 热更 DLL 加载   │  │ · LuaEnv 沙箱隔离     │
  │ · IHotUpdateEntry │  │ · API 白名单机制      │
  │   查找与激活       │  │ · IScriptEngine 实现  │
  └────────────────┘  └──────────────────────┘
```

### 热更新流程

```
[Editor/构建时]                    [运行时]
                                   
HybridCLRBuildProcessor              HybridCLRAdapter
  ┌─────────────────┐                  ┌──────────────────┐
  │ 1. AOT 元数据生成 │  → AOTMetadata  │ 1. Initialize()   │
  │ 2. 原生库拷贝     │     (Addressables│    → 加载 AOT 元数据 │
  │ 3. 安装验证       │       标签)      │ 2. LoadDLL()       │
  └─────────────────┘                  │    → 加载热更程序集 │
                                       │ 3. FindEntry()     │
                                       │    → 查找入口类型  │
                                       │ 4. ActivateEntry() │
                                       │    → 注册模块到     │
                                       │      GameWorld      │
                                       └──────────────────┘
```

### Mod 生命周期

```
ScriptModConfig → LoadMod() → IScriptMod
                              ├── OnLoad()    → ModState.Loaded
                              ├── OnEnable()  → ModState.Enabled
                              ├── OnDisable() → ModState.Disabled
                              └── OnUnload()  → ModState.NotLoaded
```

## HybridCLR 快速上手

### 1. 安装 HybridCLR 包

通过 Unity Package Manager 安装 `com.code-philosophy.hybridclr`。

### 2. 创建构建设置

在 Project 窗口中右键：**Assets/Create/HNUnityFramework/HybridCLR Build Settings**，生成 `HybridCLRBuildSettings.asset` 并配置：

```json
{
  "aotAssembliesRoot": "Assets/StreamingAssets/AOTMetadata",
  "hotUpdateAssembliesRoot": "Assets/StreamingAssets/HotUpdates",
  "aotAssemblyNames": ["Assembly-CSharp", "Assembly-CSharp-firstpass"],
  "hotUpdateAssemblyNames": ["HotFix"],
  "autoGenerateMetadata": true,
  "autoCopyNativeLibs": true
}
```

### 3. 生成 AOT 元数据

通过菜单 **Tools/HNUnityFramework/HybridCLR/Generate AOT Metadata** 一键生成，或将构建处理器 `HybridCLRBuildProcessor` 集成到构建管线中自动执行。

### 4. 编写热更新代码

在单独的程序集（如 `HotFix`）中实现 `IHotUpdateEntry` 接口：

```csharp
using HN.Framework.Core.Capability.Scripting;
using HN.Framework.Core.Driver;

public class HotFixEntry : IHotUpdateEntry
{
    public void OnRegisterGameModules(GameWorld world)
    {
        // 注册热更新模块到 GameWorld
        // world.RegisterModule(...);
    }
}
```

### 5. 运行时加载

```csharp
using HN.Framework.Unity.Capability.Scripting;

var adapter = new HybridCLRAdapter(GameWorld.Instance);
var entry = adapter.InitializeHotUpdate();
// entry.OnRegisterGameModules 已自动调用
```

## xLua Mod 开发

### 1. 安装 xLua 包

通过 Unity Package Manager 安装 `com.tencent.xlua`。

### 2. 配置 Mod 目录结构

```
Assets/StreamingAssets/Mods/
├── MyMod/
│   ├── main.lua         # 入口脚本
│   └── config.lua       # 配置脚本
└── AnotherMod/
    └── init.lua
```

### 3. 编写 Lua Mod

```lua
-- MyMod/main.lua
local MyMod = {}

function MyMod.OnLoad()
    print("MyMod loaded")
end

function MyMod.OnEnable()
    print("MyMod enabled")
end

function MyMod.OnDisable()
    print("MyMod disabled")
end

function MyMod.OnUnload()
    print("MyMod unloaded")
end

return MyMod
```

### 4. 运行时加载 Mod

```csharp
using HN.Framework.Unity.Capability.Scripting;
using HN.Framework.Core.Capability.Scripting;

var modManager = new LuaModManager();

var config = new ScriptModConfig
{
    ModId = "MyMod",
    ModName = "My First Mod",
    Version = "1.0.0",
    ScriptPaths = new[] { "main.lua", "config.lua" },
    Sandboxed = true
};

var mod = modManager.LoadMod(config);
modManager.EnableMod(mod.ModId);

// 调用 Mod 函数
modManager.CallFunction("MyMod", "OnLoad");
```

### 5. 注册 Mod API 白名单

```csharp
// 注册自定义 API 供 Lua 脚本访问
modManager.RegisterApi("UnityEngine.GameObject");
modManager.RegisterApi("MyGame.MyAPI");

// 注册全局对象
modManager.RegisterGlobal("myHelper", new MyHelper());
```

默认白名单包含：`UnityEngine.Debug`、`UnityEngine.Time`、`UnityEngine.Mathf`、`UnityEngine.Vector2`、`UnityEngine.Vector3`、`UnityEngine.Color`、`UnityEngine.Quaternion`。

### 安全沙箱

每个 Mod 拥有独立的 `LuaEnv` 实例，确保脚本环境相互隔离。沙箱自动禁用 `os.execute`、`os.exit`、`os.remove`、`io.popen` 等危险 API 以防止恶意脚本。

## API 参考

| 类型 | 位置 | 说明 |
|------|------|------|
| `IScriptEngine` | Core.Capability.Scripting | 脚本引擎抽象，定义 Execute/RegisterGlobal/CallFunction |
| `IScriptMod` | Core.Capability.Scripting | Mod 生命周期接口（OnLoad/OnEnable/OnDisable/OnUnload） |
| `ModState` | Core.Capability.Scripting | Mod 状态枚举（NotLoaded/Loaded/Enabled/Disabled/Error） |
| `ScriptModConfig` | Core.Capability.Scripting | Mod 配置数据模型 |
| `IHotUpdateEntry` | Core.Capability.Scripting | 热更新 DLL 入口接口 |
| `LuaModManager` | Unity.Capability.Scripting | xLua Mod 管理器，实现 IScriptEngine |
| `HybridCLRAdapter` | Unity.Capability.Scripting | HybridCLR 运行时适配器 |
| `HybridCLRBuildProcessor` | Editor.Scripting | HybridCLR 构建管线处理器 |
| `HybridCLRMetadataGenerator` | Editor.Scripting | AOT 元数据生成器 |
| `HybridCLRBuildSettings` | Editor.Scripting | HybridCLR 构建配置 |
| `HybridCLRNativeLibManager` | Editor.Scripting | HybridCLR 原生库管理器 |
