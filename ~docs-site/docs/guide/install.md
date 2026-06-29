---
sidebar_position: 2
---

# 安装

HNUnityFramework 支持三种安装方式，推荐使用 Unity Package Manager (UPM)。

## 方式一：通过 Unity Package Manager（推荐）

在项目的 `Packages/manifest.json` 中添加依赖：

```json
{
  "dependencies": {
    "com.hn.framework": "https://github.com/HaoNaN9279/HNUnityFramework.git"
  }
}
```

保存后 Unity 会自动下载并导入框架。

也可以指定具体版本（tag）：

```json
"com.hn.framework": "https://github.com/HaoNaN9279/HNUnityFramework.git#v1.0.0"
```

## 方式二：通过 Git Submodule

```bash
cd YourProject/Assets
git submodule add https://github.com/HaoNaN9279/HNUnityFramework.git HNUnityFramework
```

导入后框架位于 `Assets/HNUnityFramework/` 目录下。

> 注意：使用 submodule 方式需要手动管理 `.asmdef` 程序集引用。

## 方式三：手动导入

1. 从 [GitHub Releases](https://github.com/HaoNaN9279/HNUnityFramework/releases) 下载最新版本的 `.unitypackage` 或源码压缩包
2. 将 `Runtime/` 和 `Editor/` 目录放入项目的 `Assets/` 下

## 环境要求

| 组件 | 版本要求 |
|------|---------|
| Unity | 2022.3+ |
| .NET SDK | 8.0+ （仅文档生成需要） |
| Node.js | 20+ （仅文档站点开发需要） |

## 安装后验证

安装完成后，确认以下目录结构存在：

```
Assets/HNUnityFramework/
├── Runtime/
│   ├── Core/
│   ├── MVC/
│   ├── AssetManager/
│   ├── ObjectPool/
│   ├── ReferencePool/
│   ├── HFSM/
│   ├── Procedure/
│   ├── Serialize/
│   └── Utils/
├── Editor/
│   ├── Core/
│   └── ...
└── README.md
```

如果使用 UPM 方式安装，框架文件位于 `Packages/com.hn.framework/` 下。

## 下一步

安装完成后，请查看 [快速入门](./quick-start.md) 开始使用框架。
