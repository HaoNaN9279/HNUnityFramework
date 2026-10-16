# HNUnityFramework 动画模块

> 动画模块（L6 逻辑 + V4 表现）架构文档集。逻辑层 / 表现层双侧分离；表现层支持 Jobs/Burst、LOD、形变（PSD/DQ）、2D 骨骼、惯性化过渡。
> 上级索引：[架构 README](../README.md) · [最终架构](../最终架构.md) · [06-关卡层](../06-关卡层（逻辑+视图+动画）.md)
> 最后更新：2026-10-15

---

## 文档索引

| # | 文档 | 内容 |
|:-:|------|------|
| 01 | [分层架构与设计原则](./01-分层架构与设计原则.md) | 设计目标、核心切分原则、五层→逻辑/表现映射、数据流 |
| 02 | [数据契约与逻辑表现分离](./02-数据契约与逻辑表现分离.md) | 核心契约、blittable 契约、逻辑/表现职责 |
| 03 | [计算后端 Jobs 与 Burst](./03-计算后端-Jobs与Burst.md) | 三层计算模型、Burst 模式、Jobs↔Playables |
| 04 | [视野剔除与 LOD 分级](./04-视野剔除与LOD分级.md) | 视野剔除、屏幕空间判定、LOD 档位（3D / PSD / 2D）、骨骼 LOD 剔除、事件驱动更新 |
| 05 | [表现层形变 PSD 与 DQ Skinning](./05-表现层形变-PSD与DQ-Skinning.md) | PSD 数据模型、三应用路径、DQ |
| 06 | [2D 骨骼动画子系统](./06-2D骨骼动画子系统.md) | Spine 类 cutout：数据模型、导入、求值、渲染 |
| 07 | [Inertialization 惯性化过渡](./07-Inertialization惯性化过渡.md) | 过渡原理、Delta 捕获、衰减模型、集成 |
| 08 | [Playable 限制与模块对接](./08-Playable限制与模块对接.md) | Playable Bug 应对、现有模块对接 |
| 09 | [命名空间与程序集](./09-命名空间与程序集.md) | L6 / V4 命名空间与程序集规划 |
| 12 | [Additive 动画层](./12-Additive动画层.md) | delta 编码、参考基准、原生/自研、与 Inertialization 顺序 |
| 13 | [Sync Groups / Phase Matching](./13-SyncGroups相位匹配.md) | 相位模型、相位匹配、leader 缩放（逻辑层） |
| 14 | [Motion Warping（位移匹配）](./14-MotionWarping位移匹配.md) | warp window、补偿法对齐、朝向对齐（逻辑层） |
| 15 | [Montage 一次性动作层](./15-Montage动作层.md) | 段落/通知/中断仲裁/slot 分层（逻辑层调度） |
| 16 | [IK Rig（骨骼操控）](./16-IKRig骨骼操控.md) | 链/求解器/Goal、核心 Solver 集、P3 求解层 |
| 17 | [IK Retargeter（跨骨架重定向）](./17-IKRetargeter重定向.md) | Chain Map、Retarget Pose、静态烘焙+运行时 |
| 18 | [Animation Compression](./18-AnimationCompression.md) | Unity 内置调查、自研统一压缩管线、量化/关键帧减少 |
| 19 | [网络动画](./19-NetworkedAnimation.md) | 状态同步+本地重建、权威模型、Relevancy、回滚 |
| 20 | [IK 应用 Solver](./20-IK应用-Solver.md) | Aim Offset·AimIK / FootIK / InteractionIK / BodyIK |
| 21 | [Control Rig](./21-ControlRig.md) | 通用程序化骨骼操控、节点图（编译 Job）、Controls/Spaces |
| 22 | [物理二次运动](./22-SecondaryMotion二次运动.md) | Dynamic Bone/Spring（Verlet）、Cloth（PBD） |
| 23 | [程序化叠加运动](./23-程序化叠加运动.md) | 呼吸 / 后坐力 / 受击 / Turn-in-place（程序化 Additive 源） |
| 24 | [Facial 与 Lip Sync](./24-Facial与LipSync.md) | 面部 BlendShape / FACS·ARKit、语音 viseme 口型 |
| 25 | [BodyIK / FullBodyIK](./25-BodyIK.md) | 多链协同 IK、XPBD 迭代、全约束（IK Rig Solver） |

---

## 概览

```
逻辑层（Core, GameWorld.Tick 恒常驱动）
  Layer1 世界信息 → Layer2 决策 → Layer3 请求 → Layer4 状态机 → Layer5 运动意图
        │  AnimationStateSnapshot / MotionIntent（单向契约）
        ▼
表现层（Unity, 受可见性 / LOD 控制）
  P1 快照消费 → P2 姿态混合 → P2.5 Additive → P2.6 Inertialization → P3 IK Rig → P4 输出
  P5 LOD/可见性 · P6 PSD · P7 Skinning(LBS/DQ) · P8 2D Skeleton
```

- **逻辑层**：纯 C#、确定性、永不受可见性影响
- **表现层**：Playable Graph + Jobs/Burst，受 LOD / 视野剔除控制
- **编号**：`L6` = Core（数据模型 + 逻辑运行时）；`V4` = Unity（表现端：3D Playable / PSD / 2D / Inertialization）

## 关联文档

- [动画Playable限制与规避方案](../动画Playable限制与规避方案.md)
- [06-关卡层（逻辑+视图+动画）](../06-关卡层（逻辑+视图+动画）.md)
- [02-GameWorld与命名空间](../02-GameWorld与命名空间.md)
