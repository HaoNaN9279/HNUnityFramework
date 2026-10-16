# 04 · 视野剔除与 LOD 分级

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[05 PSD](./05-表现层形变-PSD与DQ-Skinning.md) · [06 2D Skeleton](./06-2D骨骼动画子系统.md) · [07 Inertialization](./07-Inertialization惯性化过渡.md)
> 最后更新：2026-10-15

---

## 一、视野剔除方案

| 环节 | 行为 |
|------|------|
| 逻辑层 | **永不受可见性影响**，恒常 Tick（与 GameWorld 帧率一致） |
| 表现层 | 由**全局** `AnimationRepresentationLod` 管理器按可见性 + **屏幕空间占比**决定档位（含骨骼剔除 / 更新策略） |
| P5 关停 | 用 `SetSpeed(0)` / `graph.Stop()`，**禁止 `Pause()`**（UUM-30899/14492 会让角色可见性变化时自复活） |
| 恢复 | 用逻辑层快照的 `NormalizedTime` + `SetTime()` 对齐姿态 |

- **根运动移出表现层** → 表现层 `SetTime()` **不再命中 UUM-33177**（带根运动 seek 时位移突变），重新激活对齐安全。
- **帧事件双轨**：逻辑层用归一化时间 + 事件时间表**自触发**关键帧事件（攻击判定），**不依赖 Playable 回调**（否则不可见时攻击判定丢失）。表现层只消费 `FiredEventIds` 播纯视觉特效。

### 屏幕空间尺寸判定

LOD 分档主判据 = **屏幕空间占比**（对齐 Unity `LODGroup` 的 Screen Relative Transition Height）。

```
screenPercent = radius / (distance · tan(fov/2))
pixelHeight   = screenPercent · screenHeight
```

| 档 | 屏幕占比（屏高） |
|:--:|------------------|
| L0 | > 0.15 |
| L1 | 0.05 – 0.15 |
| L2 | 0.01 – 0.05 |
| L3 | < 0.01 或视锥外 |

- **分档**：`screenPercent` + **百分比滞回（默认 ±20%）** 防抖
- **剔除**：视锥外 或 超「最大距离」硬上限（距离**仅作硬上限剔除辅助**，不单独分档）
- **包围体**：统一**包围球**（半径含缩放）
- **多相机**：取**最大占比**的相机
- **正交相机**：`screenPercent = radius / orthoSize`
- **实现**：P5 中枢每帧批处理（`IJobParallelFor`，与可见性批处理合并），包围球半径加载时缓存
- **联动**：驱动骨骼剔除 / 更新策略 / Relevancy（[19]）/ 各子系统 LOD
- **阈值**：走模块 Settings SO

---

## 二、LOD 档位（3D）

| 档 | 触发 | 表现内容 | 逻辑层 |
|:--:|------|---------|:------:|
| **L0 Full** | 近距 + 视锥内 | 完整 Graph：多层混合 + IK + 帧事件 + 全帧率 | 照常 |
| **L1 Reduced** | 中距 / 屏幕边缘 | 关 IK + 关次层，降采样（每 N 帧更新） | 照常 |
| **L2 Simplified** | 远距 | 单层 AnimatorController 或烘焙姿态，不跑脚本 Job | 照常 |
| **L3 Culled** | 视野外 | 停 Graph（`SetSpeed(0)`），保留最后姿态或隐藏 | **照常** |

**切换策略**：距离阈值 + 视锥判断 + **滞回（hysteresis）** 防抖；档位阈值走模块 Settings SO（D4.6 体系）。

**与 Unity cullingMode 的关系（取舍点）：**

| 档 | Unity cullingMode | 代价 |
|:--:|------------------|------|
| L0/L1 含脚本 Job(IK) | 需 `AlwaysAnimate` | 丧失 Unity 原生剔除（限制 C） |
| L0/L1 无脚本 Job | `CullUpdateTransforms` 足够 | — |
| L3 | `CullCompletely` | 与原生剔除一致，免费 |

> 要 IK 就得吃 `AlwaysAnimate`。**已定：L1 档砍 IK**（省 `AlwaysAnimate`）。

---

## 三、PSD / DQ 的 LOD 降级

| 档 | PSD | DQ Skinning |
|:--:|-----|:-----------:|
| L0 Full | 全 Interpolator + 全 shape，全帧率 | 开 |
| L1 Reduced | 关键 Interpolator + 主 shape，降采样 | 开 |
| L2 Simplified | 关 PSD（纯蒙皮） | 关（回 LBS） |
| L3 Culled | 不形变 | — |

## 四、2D Skeleton 的 LOD 降级

| 档 | 2D Skeleton |
|:--:|-------------|
| L0 Full | 全骨架求解 + skinning + 全帧率 |
| L1 Reduced | 减顶点 / 降采样 |
| L2 Simplified | 静态帧（不求解） |
| L3 Culled | 停 |

## 五、Additive 的 LOD 降级

| 档 | Additive |
|:--:|----------|
| L0 | 全部 additive 层 |
| L1 | 关键 additive 层，降采样 |
| L2 | 关 additive |
| L3 | 停 |

## 六、Inertialization 的 LOD 特殊性

- **不能降采样**（否则抖动）→ L0 / L1 均保留每帧过渡，仅可简化旋转衰减精度
- L2 关闭 → 退化短 cross-fade 或 snap
- L3 停（Job 不跑）→ 过渡冻结，需处理（详见 [07](./07-Inertialization惯性化过渡.md)）

---

## 七、骨骼 LOD 剔除（Bone LOD Culling）

### 7.1 定位

骨骼级粒度：低档**选择性不更新末端骨骼**（手指 / 脚趾 / 尾巴 / 飘带 / 面部），进一步降本。

### 7.2 骨骼分级（Tier）

| Tier | 内容 | 低档行为 |
|:----:|------|----------|
| **T0 核心** | Root / Hips / Spine / Head / 四肢主链 | 始终更新 |
| **T1 次末端** | 手 / 脚 / 上臂 | L2+ 可剔除 |
| **T2 末端** | 手指 / 脚趾 / 尾巴 / 飘带 / 面部 | L1+ 可剔除 |

### 7.3 数据模型（L6）

| 类型 | 职责 |
|------|------|
| `BoneLodGroup` | 骨骼组：`Name` + `BoneIndices[]` + `Tier` |
| `BoneLodProfile` | 每 LOD 档 → 激活的 Tier 集合 |

### 7.4 运行时

- 在 `IAnimationJob` 内按 `boneActiveMask` **跳过末端骨骼 writer handle**
- 剔除骨骼**冻结在上一姿态**（默认策略）
- Skinning / PSD / DQ **仍用冻结矩阵**（渲染正确，仅姿态不更新）

### 7.5 档位集成

| 档 | 骨骼剔除 |
|:--:|----------|
| L0 | 全骨骼更新 |
| L1 | 剔 T2（末端） |
| L2 | 剔 T1 + T2 |
| L3 | 全剔（停） |

### 7.6 平滑过渡

Tier 切换（剔除 ↔ 恢复）时末端骨骼会突变 → 复用 **P2.6 Inertialization** 淡出 / 淡入。

### 7.7 约束

- **冲突检测（硬约束）**：IK Goal / 武器挂点骨骼**不可剔**（配置校验）
- 与 Sync 无关（相位在逻辑层）

---

## 八、表现层事件驱动更新

### 8.1 定位

低活跃 / 远距角色平时**不逐帧更新**，仅**事件触发时刷新**，降低空转开销。

### 8.2 UpdatePolicy

| 模式 | 说明 | 档 |
|------|------|:--:|
| `EveryFrame` | 每帧更新 | L0 |
| `Throttled(N)` | 每 N 帧更新（默认 **N=2**） | L1 |
| `EventDriven` | 仅事件触发 | L2 |
| `Paused` | 停 | L3 |

### 8.3 触发源

契约变化（`Version` / `DominantState` / 参数）· 显式事件（帧事件 / Montage 通知 / gameplay）· 可见性变化 · 时间阈值

### 8.4 机制

```
逻辑层 → 脏标记 / 直接回调（低延迟）→ 表现层刷新一帧
   EventDriven：仅事件时刷新；否则保持当前姿态
   Throttled：定时刷新 + 事件立即刷新（混合）
```

### 8.5 边界

- **循环动画** EventDriven → 事件间**静止帧**（远距 / 遮挡可接受）
- 需**连续姿态**的（近距 IK / PSD / 物理）**不可 EventDriven**
- **Inertialization 过渡期间强制 EveryFrame**

### 8.6 与 Sync / 骨骼 LOD 协同

- 相位恒常（逻辑层），降频时按相位采样
- 远距角色 = `EventDriven` + 剔 T1/T2 → **极低开销**

---

## 九、LOD 档位总表

| 档 | 更新策略 | 骨骼剔除 | IK Rig | PSD / DQ | Additive | Inertialization |
|:--:|----------|:--------:|:------:|:--------:|:--------:|:---------------:|
| L0 | EveryFrame | 全骨骼 | 全求解器 | 全 | 全 | 全 |
| L1 | Throttled(2) | 剔 T2 | 砍（关） | 关键 | 关键 | 全（不降采样） |
| L2 | EventDriven | 剔 T1+T2 | 关 | 关 | 关 | 短 cross-fade / snap |
| L3 | Paused | 全剔 | 停 | 不形变 | 停 | 停 |
