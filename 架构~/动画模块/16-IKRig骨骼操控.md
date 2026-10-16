# 16 · IK Rig（骨骼操控）

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[17 IKRetargeter](./17-IKRetargeter重定向.md) · [20 IK 应用 Solver](./20-IK应用-Solver.md) · [01 分层架构](./01-分层架构与设计原则.md)
> 对标 UE IK Rig（单骨架骨骼操控）
> 最后更新：2026-10-15

---

## 一、定位

为单个 skeleton 定义**可复用的骨骼操控层**：逻辑骨骼链 + 求解器 + 目标。是 Retargeting 与运行时 IK 的基础。

## 二、系统组成

```
IKRig
  ├── Retarget Chains（逻辑骨骼链）
  ├── Solvers（IK/FK 求解器实例）
  └── Goals（目标，可驱动）
```

## 三、归属与架构位置

| 部分 | 归属 |
|------|------|
| 定义（链 / 求解器 / 目标配置） | L6（Core） |
| 求解执行（每帧） | 表现层 **P3**（`AnimationScriptPlayable` + `IAnimationJob` + Burst） |
| Goals 驱动源 | 逻辑层注入（`IIKGoalProvider`） |

> **P3 语义升级为「IK Rig 求解层」**。

## 四、Retarget Chains

- 逻辑骨骼链：`Name / StartBone / EndBone / FK 关联 / 方向`
- 示例：Spine、LeftArm、RightArm、LeftLeg、RightLeg、Head、Root
- 作为重定向映射与求解器的基本单位

## 五、Solvers（首版核心集）

| Solver | 作用 |
|--------|------|
| FKSolver | 链前向运动学（基础） |
| ChainIK | 通用链 IK |
| TwoBoneIK / LimbIK | 双骨（肘 / 膝）IK |
| FABRIK | 多关节 IK |
| PoleVector | 极向控制 |
| AimIK | 瞄准 / 注视 |
| FootIK | 脚部贴地 |

> 首版**含** BodyIK（多链协同求解，详见 [25](./25-BodyIK.md)）。
> 多求解器**按用户配置顺序**链式求解。
> 应用型 Solver（Aim / Foot / Interaction / Body）详见 [20](./20-IK应用-Solver.md)。

## 六、Goals

- 每 Goal：位置 / 旋转 / 权重 / 驱动源
- 驱动源：逻辑层注入（gameplay）/ 静态值 / 链式依赖
- 注入接口 `IIKGoalProvider`（逻辑层）

## 七、运行时（P3 求解层）

- 每骨架实例化 `IKRigInstance`：链 + 求解器状态 + Goals
- 求解在 `IAnimationJob` 内（读 `AnimationStream`，写骨骼）
- 遵 UUM-34442（单输入端口 + 前置 Mixer）；多 solver 链式顺序执行

## 八、与现有技术协作

| 技术 | 关系 |
|------|------|
| Interaction IK | 由 IK Rig 的 **Goals** 实现（手抓取 / 坐姿 / 掩体） |
| Foot IK / Aim IK | 作为**内置 Solver** |
| Inertialization / Additive | 在 IK Rig **之前** |
| PSD / DQ（P6/P7） | 在蒙皮侧，IK **之后** |
| Sync / Warping（逻辑层） | 正交；Warping 给位移，IK Rig 给骨骼对齐 |

## 九、LOD

| 档 | IK Rig |
|:--:|--------|
| L0 | 全求解器 |
| L1 | 关键链（Aim / Foot） |
| L2 | 关 IK |
| L3 | 停 |

## 十、额外开发清单

1. L6：`IKRigDef` / `RetargetChain` / `SolverDef` / `GoalDef`
2. V4：`IKRigInstance` + 求解器库（核心集）
3. V4：**P3 升级为 IK Rig 求解层**
4. 逻辑层：`IIKGoalProvider` 注入接口
5. Interaction IK / Foot IK / Aim IK → 内置 Solver
6. Editor：IK Rig 编辑器（**复用 E2**）
7. LOD 集成 + Settings（求解器顺序）

## 十一、程序集 / 命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.Rig` | Core | IK Rig / Retargeter 数据模型 |
| V4.6 | `HN.Framework.Unity.Level.View.Animation.Rig` | Unity | IK Rig 运行时 + 求解器库（P3） |
| E-新 | `HN.Framework.Editor.Animation.Rig` | Editor | IK Rig 编辑器（复用 E2） |

## 十二、已确认

1. 首版**核心 Solver 集**：FKSolver / ChainIK / TwoBone·LimbIK / FABRIK / PoleVector / AimIK / FootIK
2. **P3 升级**为 IK Rig 求解层
3. **首版含** BodyIK（多链协同，[25](./25-BodyIK.md)）
4. Editor **复用 E2**
5. 多求解器**按用户配置顺序**链式求解
6. Goals 注入接口 `IIKGoalProvider` 放**逻辑层**
