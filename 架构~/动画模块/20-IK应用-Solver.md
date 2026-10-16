# 20 · IK 应用 Solver（Aim / Foot / Interaction / Body）

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[16 IK Rig](./16-IKRig骨骼操控.md) · [12 Additive](./12-Additive动画层.md) · [15 Montage](./15-Montage动作层.md) · [04 LOD](./04-视野剔除与LOD分级.md)
> IK 应用项：均作为 [16] IK Rig 的 Solver / Goals 应用
> 最后更新：2026-10-15

---

## 一、总览

| 项 | 用途 | 归属 |
|----|------|------|
| **Aim Offset** | 上半身偏移混合空间（**非 IK**） | P2 / P2.5 |
| **AimIK** | Look-at / Head-track | P3 Solver |
| **FootIK** | 脚部贴地 | P3 Solver |
| **InteractionIK** | 抓取 / 持械 / 坐姿 / 掩体 | P3 Solver + Goals |
| **BodyIK** | 全身多链协同 | 后续 |

## 二、Aim Offset（混合空间，非 IK）

- 上半身相对根朝向的**偏移混合空间**（pitch / yaw → pose）
- 归 **Additive / Override 层 + BlendSpace**（P2 / P2.5），**不是 IK**
- 与 AimIK 组合：Aim Offset 提供身体偏移，Look-at 精修头部朝向

## 三、AimIK（Look-at / Head-track）

| 项 | 内容 |
|----|------|
| 输入 | 骨骼链（脊柱 → 颈 → 头）、目标方向 / 点、权重 |
| 算法 | 从链末端向上逐级旋转，使末端朝向目标；每级**角度限幅** |
| 分级 | 头 > 颈 > 脊柱 > 骨盆，**从脊柱开始** |
| 平滑 | **角速度限制**（转头不瞬移） |
| Goal | 逻辑层注入（相机方向 / 敌方位置 / 注视点） |

- 限制：最大角度（避免脖子 / 脊柱扭曲）
- 与上半身蒙太奇互斥处理

## 四、FootIK（地形适配，首版）

```
双脚射线（脚骨骼向下 + 前方）→ 命中地面点 + 法线
  → TwoBoneIK 解算腿 → 脚部朝向对齐法线
  → 骨盆高度补偿（取较低脚，避免腿伸直 / 悬空）
```

| 项 | 内容 |
|----|------|
| 输入 | 左右脚链（髋 → 膝 → 踝）、地面射线、骨盆骨骼 |
| 算法 | TwoBoneIK + 脚旋转对齐法线 + **骨盆升降（取较低脚）** |
| Goal | 脚部目标 = 射线命中点 |
| 平滑 | 脚抬起 / 落地权重渐变，避免抖动 |
| 地面数据 | **走 C16 Physics**（raycast API，非 Unity Physics） |

## 五、InteractionIK（首版）

| 场景 | 目标 |
|------|------|
| 持械（双手） | 双手贴武器握点 |
| 抓取 / 开门 | 手对齐把手 |
| 掩体 / 攀爬 | 手 / 脚对齐边缘 |
| 坐姿 | 骨盆 + 脚对齐座椅 |

- 实现：**Goal 设到交互点 → TwoBoneIK 解算**
- **由 Montage（[15]）通知驱动**交互窗口
- Goal 由 gameplay 注入（`IIKGoalProvider`）

## 六、BodyIK / FullBodyIK（首版）

全身多链协同 IK（脚 + 手 + 骨盆 + 脊柱），XPBD 迭代求解 —— 详见 [25](./25-BodyIK.md)。

## 七、与 IK Rig（[16]）的关系

- 以上均作为 **Solver + Goals** 注册进 IK Rig
- 求解顺序由用户配置（[16]）
- 求解位置：表现层 **P3**（`IAnimationJob` + Burst）

## 八、LOD

| Solver | L0 | L1 | L2 | L3 |
|--------|:--:|:--:|:--:|:--:|
| AimIK / FootIK | ✓ | ✓（关键链） | ✗ | ✗ |
| InteractionIK | ✓ | ✓（仅 montage 期） | ✗ | ✗ |
| BodyIK | ✓ | ✗ | ✗ | ✗ |

## 九、额外开发清单

1. V4.6：`AimIK` Solver（分级链旋转 + 限角 + 速度平滑）
2. V4.6：`FootIK` Solver（双脚射线 + TwoBoneIK + 骨盆补偿）
3. V4.6：`InteractionIK`（Goal + TwoBoneIK，接 Montage 通知）
4. C16 Physics 集成（地面射线）
5. 逻辑层：`IIKGoalProvider` 提供注视 / 交互目标
6. （后续）`BodyIK` 多链迭代求解
7. LOD 集成 + Settings（限角 / 速度 / 权重）

## 十、命名空间

并入 [16] 的 `HN.Framework.Unity.Level.View.Animation.Rig`（Solver 库），无新命名空间。

## 十一、已确认

1. **Aim Offset** 归 P2 / P2.5（混合空间），**非 IK**
2. **AimIK 分级**：**从脊柱开始**（头 > 颈 > 脊柱 > 骨盆）
3. **FootIK 射线**：脚骨骼**向下 + 前方**（双脚）
4. **骨盆补偿**：**取较低脚**
5. **InteractionIK** 由 **Montage 通知**驱动
6. **BodyIK**：**首版实现**（[25](./25-BodyIK.md)）
7. **地面射线**走 **C16 Physics**
