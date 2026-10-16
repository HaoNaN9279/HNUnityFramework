# 05 · 表现层形变 PSD 与 DQ Skinning

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[04 LOD](./04-视野剔除与LOD分级.md) · [08 Playable 限制](./08-Playable限制与模块对接.md)
> 最后更新：2026-10-15

---

## 一、定位

**PSD**（Pose Space Deformation，Lewis 2000 SIGGRAPH）：用骨架姿态（pose space）驱动**校正形变**，经 **RBF** per-vertex 散点插值算权重，修复 LBS 关节弯曲的糖纸 / 体积丢失（Maya Pose Editor 即此机制）。**DQ Skinning**（Dual Quaternion）从蒙皮方式侧解决同类问题，与 PSD 互补。

**全程落在表现层（P6/P7）**，逻辑层与数据契约不变（PSD 需逐骨骼姿态，逻辑层契约不含，故 PSD 在表现层闭环）。

## 二、架构位置

```
PlayableGraph P4（最终骨骼姿态 local rotations）
  → P6  PSD：读驱动关节姿态 → RBF → 校正 shape 权重（Burst IAnimationJob）
  → P7  Skinning：蒙皮（LBS / DQ） + 叠校正形变
```

## 三、三种应用路径（全支持）

| 路径 | 应用方式 | DQ | 适用 / 备注 |
|------|----------|:--:|------|
| **A. BlendShape** | `SkinnedMeshRenderer.SetBlendShapeWeight` | ✗（内置仅 LBS） | 默认；shape 数少、CPU 边界可接受 |
| **B. Compute Deformation** | 权重 + delta → `ComputeBuffer` → 自定义 compute/vertex shader | ✓ | 高顶点数、sparse blendshape（≤255 影响）；**须自研剔除** |
| **C. DOTS Deformation** | Hybrid Renderer Deformation System | ✓（自研） | DOTS 项目；需 `ENABLE_COMPUTE_DEFORMATIONS` |

> ⚠ Unity 官方 Compute Deformation **无视锥 / 遮挡剔除**（官方文档：对场景内所有使用者全量处理）→ 路径 B/C 必须叠加自研剔除，否则与 LOD 目标冲突。

## 四、数据模型（L6 扩展，Core）

`PsdConfig`（含 `PsdInterpolator[]` + `CorrectiveShape[]`），命名空间 `...Logic.Animation.Psd`，纯数据走 Luban + AssetRef。核心类型：

| 类型 | 职责 |
|------|------|
| `PsdInterpolator` | RBF 节点（互斥 pose 组）：驱动关节索引 / 关键姿态数组 / RBF 半径 |
| `PsdPose` | 关键姿态：驱动关节旋转 + 触发的校正 shape 索引与权重 |
| `CorrectiveShape` | 校正形变：路径 A 用 BlendShapeIndex，路径 B/C 用 delta 数据 AssetRef |

## 五、运行时求解（V4 扩展 + Jobs）

`PsdWeightJob`（`IAnimationJob` + Burst，挂在 `AnimationScriptPlayable` 内）：读 `AnimationStream` 驱动关节 local rotation → RBF → 写外部 `NativeArray<float>` shapeWeights。

集成时序：

```
PlayableGraph 末端 → AnimationScriptPlayable(PsdWeightJob) 读姿态 → 写 shapeWeights
主线程 LateUpdate → 读 shapeWeights → 应用（A / B / C）
```

> 只**读**姿态、写外部 `NativeArray`、**不写曲线** → 规避 UUM-33944。

## 六、DQ Skinning

- **蒙皮方式开关**（LBS / DQ），与 PSD 叠加：DQ 处理扭转体积，PSD 处理局部校正
- Unity 内置 `SkinnedMeshRenderer` **仅支持 LBS** → DQ 必须自研 skinning，**与路径 B/C 绑定**（路径 A 不可做 DQ）
- 数据：每骨骼 dual quaternion 由骨骼姿态计算（Job，可与 `PsdWeightJob` 合并）
- **叠加顺序（已定）**：**先蒙皮（LBS / DQ）再叠 PSD 校正**

## 七、LOD 降级

见 [04 · PSD/DQ 的 LOD 降级](./04-视野剔除与LOD分级.md)。

> **PSD 求解频率（已定）：每帧**。
