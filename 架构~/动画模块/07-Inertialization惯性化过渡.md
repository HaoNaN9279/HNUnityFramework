# 07 · Inertialization（惯性化过渡）

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[01 分层架构](./01-分层架构与设计原则.md) · [02 数据契约](./02-数据契约与逻辑表现分离.md) · [03 Jobs](./03-计算后端-Jobs与Burst.md) · [04 LOD](./04-视野剔除与LOD分级.md)
> 最后更新：2026-10-15

---

## 一、技术本质

GDC 2018（Bollo, Gears of War）：**过渡时只采样目标动画，把「新旧姿态差 + 旧姿态速度」用一个衰减弹簧在数帧内叠加抵消**。

| 维度 | cross-fade（传统） | Inertialization |
|------|-------------------|-----------------|
| 过渡期采样 | 同时采样 A + B（2×） | **只采样目标**（1×） |
| 开销 | 高（多 clip 混合） | 低（一个 Delta 叠加） |
| 中断处理 | 再叠加 → 模糊 | Delta 累积，**不模糊** |
| 速度连续 | 否（位置硬插值） | **是**（保留 velocity） |
| 适用 | 长过渡、需双动画 | 短过渡、频繁切换、中断多 |

**数学核心**：临界阻尼弹簧解析解，对每骨骼分量维护 `(offset x, velocity v)`：

```
x(t) = (x₀ + (v₀ + ω·x₀)·t)·e^(−ω·t)     ω = 2π / period
```

t → ∞ 时 offset 与 velocity 同时归零 → 姿态平滑、无速度突变。

## 二、架构位置（表现层 P 扩展）

```
P2   姿态混合（Mixer/BlendSpace）
  → P2.5 Additive 层（见 12）
  → P2.6 Inertialization 节点  ← 本技术（AnimationScriptPlayable + IAnimationJob）
  → P3   IK 求解
  → P4   骨骼输出
```

- 插在 **P2.5(Additive) 之后、P3 之前** → additive 启停也被平滑；IK 在过渡后姿态上重算
- 逻辑侧仅加过渡策略标记，与 P 系列并列；编号 **P2.6**

## 三、核心机制

### 3.1 Delta 捕获（切换瞬间）

切换触发时（P1 检测到 `DominantStateHash` / `Version` 变化）：

- `offset = mixedPose(上一帧) ⊖ newPose(目标首帧)`
- `velocity = (mixedPose(上一帧) − mixedPose(上一帧−1)) / dt`（旧姿态变化率）

> 需缓存**上一帧 P2 混合输出姿态**（每骨骼 T/R/S）。这是 Inertialization 唯一额外状态量。

### 3.2 衰减模型（推荐 A）

| 模型 | 说明 |
|------|------|
| **A. 临界阻尼弹簧**（Bollo 原版） | 速度连续、无时长参数（由 period 控制），推荐 |
| B. 定时多项式（cubic/quintic，固定 T 内归零） | 时长可控、实现简单 |

- 平移用 `float3`，缩放按分量；**旋转对四元数增量**：差分 → 轴角 → 对角速度衰减 → 重建 quaternion
- 每帧在 `IAnimationJob` 内：`output = input ⊕ decay(offset, velocity, dt)`

### 3.3 自适应时长

过渡时长由 **Delta 幅度**决定（位移差 + 旋转角差），避免小切换拖沓、大切换生硬：

```
period = clamp(basePeriod * f(deltaMagnitude), minPeriod, maxPeriod)
```

- 幅度越大 → period 越长；`f()` 曲线走 Settings 配置

## 四、数据契约（逻辑 → 表现）

`AnimationStateSnapshot` 扩展（或并行 `TransitionHint`）：

| 字段 | 含义 |
|------|------|
| `TransitionPolicy` | 枚举 `None / CrossFade / Inertialization` |
| `TransitionDuration` | 建议时长（0 = 表现层自适应） |

- **逻辑层决定「用哪种过渡 + 时长上限」，表现层执行**（职责清晰）
- 未指定时表现层默认 `Inertialization`

## 五、运行时结构（表现层）

| 组件 | 职责 |
|------|------|
| `InertializationNode` | `AnimationScriptPlayable` + `IAnimationJob`（Burst）；单输入端口 |
| `InertializationBuffer` | NativeArray：每骨骼 offset(T/R/S) + velocity + 上一帧姿态缓存 |
| `TransitionRequest` | P1 检测状态变化 → 置标志 → 节点下一帧捕获 |
| 衰减求解 | Job 内并行（每骨骼独立） |

**节点逻辑**：input 流 → 若收到 capture 请求则记录 delta/velocity → 每帧 decay → 写 output。

## 六、运行时流程

```
逻辑层切换状态 → 契约 TransitionPolicy/Duration
  → P1 检测变化 → 置 capture 标志 + 缓存上一帧 P2 姿态
  → P2.6 InertializationJob(ProcessAnimation)：
       capture(offset, velocity)  →  decay(dt)  →  output = input ⊕ offset
  → P3 IK → P4 输出
```

## 七、与 Playable 限制的关系

| 限制 | 应对 |
|------|------|
| UUM-34442（多输入端口） | `InertializationNode` **单输入端口 + 前置 Mixer** |
| 限制 B（输入权重不生效） | 衰减在 Job 内自算（不依赖 graph 权重） |
| UUM-33944（权重≠1 写曲线失效） | 节点固定挂**主层权重 1**，规避 |
| 限制 C（不可见不跑 ProcessAnimation） | 见 §八，需处理过渡冻结 |

## 八、与 LOD / 视野剔除 / Jobs 集成

| 档 | Inertialization |
|:--:|-----------------|
| L0 | 全量（自适应时长 + 旋转衰减） |
| L1 | 保留每帧过渡（**不能降采样**，否则抖动）；可简化旋转衰减精度 |
| L2 | 关闭 → 退化为短 cross-fade 或 snap |
| L3 | 停（Job 不跑） |

**不可见时的过渡冻结处理**（关键）：Job 不跑 → offset 不衰减。两策略：
- **A（推荐）**：记录过渡剩余时间；重新可见时若已超时 → `offset = 0` 直接完成（无跳变）
- B：按经过时间快进衰减

> Inertialization 天然依赖每帧连续更新，是**唯一不能降采样**的过渡技术。

## 九、与其他技术协作

| 技术 | 关系 |
|------|------|
| cross-fade | 并用：长过渡 / 需双动画用 cross-fade，短切换用 Inertialization（`TransitionPolicy` 选） |
| Additive 层 | 顺序：Base → Inertialize → Additive |
| Sync Groups / Motion Warping | 正交，可叠加 |
| 逻辑层 Layer3 请求仲裁/双缓冲 | 逻辑层决定**切到哪个状态**；本技术决定**怎么过渡**（表现层） |

## 十、额外开发清单

1. L6：`TransitionPolicy` / `TransitionDuration` 契约字段（或并行 `TransitionHint`）
2. V4：`InertializationNode`（`AnimationScriptPlayable` + `IAnimationJob` + Burst）
3. V4：`InertializationBuffer`（NativeArray 双缓冲 + 姿态缓存）
4. 衰减模型实现（临界阻尼弹簧）+ 旋转轴角衰减
5. 自适应时长算法（`f(deltaMagnitude)`）
6. P1 检测状态变化 → capture 触发
7. LOD / 不可见集成（剩余时间快进 or 归零）
8. Settings SO：basePeriod / min / max / 曲线 / 启用开关
9. （可选）Editor 调试可视化（过渡曲线 / offset 实时值）

## 十一、程序集 / 命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.Transition` | Core | `TransitionPolicy` 契约 |
| V4 | `HN.Framework.Unity.Level.View.Animation.Inertialization` | Unity | `InertializationNode` + Job + Buffer |

## 十二、待确认

1. **策略归属**：逻辑层给 `TransitionPolicy`（推荐），还是表现层全权决定？
2. **衰减模型**：临界阻尼弹簧（推荐）vs 定时多项式？
3. **旋转衰减数学**：轴角衰减 vs 四元数 log 空间？
4. **自适应时长**：`f(deltaMagnitude)` 曲线形态待定。
5. **与 cross-fade 的切换判据**：何时用哪个？
6. **不可见处理**：快进（A）vs 归零（B）？推荐 A。
7. **姿态缓存来源**：用 P2 混合输出（不含 IK），确认。
