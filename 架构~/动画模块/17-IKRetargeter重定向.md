# 17 · IK Retargeter（跨骨架重定向）

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[16 IKRig](./16-IKRig骨骼操控.md) · [01 分层架构](./01-分层架构与设计原则.md)
> 对标 UE IK Retargeter
> 最后更新：2026-10-15

---

## 一、定位

连接两个 IK Rig（**源** + **目标**），把源姿态传递到目标骨架 → **一套动画跨骨架 / 跨比例复用**（换装、MOBA 多英雄、异形角色）。

## 二、归属

| 部分 | 归属 |
|------|------|
| Chain Map / Retarget Pose 配置 | L6（Core） |
| **静态烘焙** | Editor / 导入时 |
| 运行时重定向 | 表现层 |

## 三、Chain Map

- 源 IK Rig chain → 目标 IK Rig chain（按 [16] 定义的逻辑链）
- 支持比例差异（目标骨骼长度不同）

## 四、Retarget Pose

- 源 / 目标**对位姿态**（编辑期定义）
- 用于把源姿态转换到目标骨架空间

## 五、两种模式（静态烘焙为主 + 运行时为辅）

| 模式 | 时机 | 用途 |
|------|------|------|
| **静态烘焙（主）** | 导入 / Editor | 源动画烘焙到目标骨架 → 目标 clip |
| 运行时（辅） | 每帧 | 无烘焙实时重定向（换装 / 动态骨架） |

## 六、转换算法

- 源链 FK → 目标链 FK（按 chain map + retarget pose 求差）
- 保留目标骨架比例特征（避免拉伸失真）

## 七、与动画管线协同

- **多骨架共享动画**（MOBA 多英雄）
- 接入动画加载管线：`AssetRef` 源 clip → Retargeter → 目标 clip / 姿态

## 八、额外开发清单

1. L6：`IKRetargeterDef` / `ChainMap`
2. Editor：**静态烘焙工具**（源动画 → 目标骨架 clip）
3. V4：运行时重定向
4. Editor：Retargeter 编辑器（**复用 E2**）
5. 多骨架共享动画管线（接入动画加载）

## 九、程序集 / 命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.Rig` | Core | IK Retargeter 数据模型 |
| V4.6 | `HN.Framework.Unity.Level.View.Animation.Rig` | Unity | 运行时重定向 |
| E-新 | `HN.Framework.Editor.Animation.Rig` | Editor | 静态烘焙 + Retargeter 编辑器（复用 E2） |

## 十、已确认

1. 静态烘焙为主 + 运行时为辅
2. Editor **复用 E2**
