# 12 · Additive 动画层

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[07 Inertialization](./07-Inertialization惯性化过渡.md) · [01 分层架构](./01-分层架构与设计原则.md) · [04 LOD](./04-视野剔除与LOD分级.md)
> 最后更新：2026-10-15

---

## 一、技术本质

**Additive 层** = 在基础姿态上**叠加增量（delta）**，而非覆盖。数据是「相对参考姿态的偏移」，非绝对姿态。

| 维度 | Override 层（AvatarMask） | Additive 层 |
|------|---------------------------|-------------|
| 语义 | 上层**替换**下层 | 上层**叠加**到下层 |
| 数据 | 绝对姿态 | **相对参考的 delta** |
| 用途 | 上半身覆盖、武器切换 | 呼吸 / 受击抖动 / 后坐力 / 瞄准偏移 / 情绪 / 疲劳 |
| 混合 | 按权重插值姿态 | 按权重缩放 delta |

**参考姿态（Reference Pose）**——additive 基准，必须全局一致：

| 类型 | 说明 |
|------|------|
| Relative to Bind Pose | 相对绑定姿态（默认） |
| Relative to Frame | 相对 clip 指定帧（预留） |

> 不同参考基准的 additive 不能叠加（会算错）。

**数学**（旋转不是加法，是四元数 delta）：

```
pos:   p = p_base + w · Δp
rot:   q = q_base ⊗ slerp(identity, Δq, w)     Δq = q_ref⁻¹ ⊗ q_add
scale: s = s_base · lerp(1, Δs, w)             Δs = s_add / s_ref
```

## 二、架构位置（表现层 P 扩展）

Additive 是**混合能力**，与 P2 同族，独立节点，占 **P2.5**：

```
P2   基础 / 覆盖混合（Mixer · BlendSpace · AvatarMask）
  → P2.5 Additive 层（本技术）    ← 叠加 delta
  → P2.6 Inertialization（见 07）
  → P3   IK 求解
  → P4   骨骼输出
```

> ⚠ **顺序关键**：Additive 在 **Inertialization 之前** → additive 层进入/退出突变也能被惯性化平滑。

## 三、核心机制

### 3.1 Delta 编码

- additive delta 离线烘焙（制作时）或导入时（Editor）计算
- `Δ = additiveClip ⊖ referencePose`（逐帧、逐骨骼、逐分量）
- 旋转 delta 用四元数 `q_ref⁻¹ ⊗ q_add`；缩放用比值 `s_add / s_ref`

> 除 clip 源外，支持**程序化源**（正弦 / 脉冲 / 弹簧，非 clip），详见 [23 程序化叠加运动](./23-程序化叠加运动.md)。

### 3.2 参考基准一致性

- 同一 base 上所有 additive 层**必须同参考基准**（默认 BindPose）
- 基准在导入时校验，混用报错

### 3.3 权重与遮罩

- 每 additive 层独立 `weight`
- 可叠加 `AvatarMask`（只影响部分骨骼，如「只抖手臂」）
- 多层 additive 顺序叠加，**叠加在全部 override 层之上**；默认层数上限 `4`（可调）

## 四、数据模型（L6 扩展，Core）

命名空间 `...Logic.Animation.Additive`，走 Luban + AssetRef：

| 类型 | 职责 |
|------|------|
| `AdditiveClipDef` | additive clip：参考基准 + delta 数据引用 |
| `AdditiveReferenceType` | 枚举 BindPose / Frame |
| `AdditiveLayerDef` | 层定义：clip / 默认 weight / maskId / 默认参考 |
| 烘焙产物 | delta clip（MemoryPack） |

## 五、运行时实现路径

| 路径 | 方式 | 说明 |
|------|------|------|
| **A. Unity 原生 additive 层（主）** | `AnimationLayerMixerPlayable.SetLayerAdditive(idx, true)` + `SetLayerMaskFromAvatarMask` | 简单、复用原生 |
| **B. 自研 additive 节点（补复杂）** | `AnimationScriptPlayable` + `IAnimationJob`（Burst） | 灵活（自定义参考 / 多 delta 合成）；受 UUM-34442 / 限制 B 约束 |

## 六、契约扩展（逻辑 → 表现）

`AnimationStateSnapshot` 增加 additive 层列表：

| 字段 | 含义 |
|------|------|
| `AdditiveLayers[]` | 每项：`AdditiveClipHash` + `Weight` + `MaskId` |

> 逻辑层决定「播哪个 additive、权重多少、何时启停」；表现层执行叠加。

## 七、与 Inertialization 的顺序

推荐 **Base → Additive → Inertialize**（additive 启停被平滑）：

| 顺序 | 效果 |
|------|------|
| **Base → Additive → Inertialize**（采用） | additive 进入/退出也被平滑 |
| Base → Inertialize → Additive | additive 突变不被平滑，会跳变 |

> 因此 Inertialization 编号由 P2.5 顺延为 **P2.6**（见 [07](./07-Inertialization惯性化过渡.md)）。

## 八、LOD 降级

| 档 | Additive |
|:--:|----------|
| L0 | 全部 additive 层 |
| L1 | 关键 additive 层，降采样 |
| L2 | 关 additive |
| L3 | 停 |

- 路径 B 走 `IAnimationJob` + Burst；逻辑层恒常决策 additive 权重（不受可见性影响）

## 九、与其他技术协作

| 技术 | 关系 |
|------|------|
| Override 层（AvatarMask） | 并存：override 换姿态，additive 加效果 |
| Inertialization | additive 在**其前**，使 additive 启停被平滑 |
| IK | additive 在 IK 前（IK 作用于叠加后姿态） |
| Aim Offset / 受击 / 后坐力 | 均为 additive 的典型应用（见 [23](./23-程序化叠加运动.md)） |
| 逻辑层 Layer3 请求仲裁 | logic 决定 additive 触发与权重 |

## 十、额外开发清单

1. L6：`AdditiveClipDef` / `AdditiveReferenceType` / `AdditiveLayerDef` 数据模型
2. Editor：delta 烘焙器（base + additive pair → delta clip）+ 参考基准校验
3. 契约：`AnimationStateSnapshot.AdditiveLayers[]`
4. 表现层路径 A：原生 additive layer 封装（weight / mask / 参考）
5. 表现层路径 B：自研 additive 节点（`AnimationScriptPlayable` + Job，单输入端口）
6. 逻辑层：additive 触发 / 权重决策（Layer3 请求仲裁接入）
7. 顺序编排：P2.5 Additive → P2.6 Inertialization
8. LOD 集成 + Settings（启用开关 / 层数上限）
9. 旋转 delta 数学（四元数乘法 + 加权 slerp）

## 十一、程序集 / 命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.Additive` | Core | additive 数据模型 |
| V4.5 | `HN.Framework.Unity.Level.View.Animation.Additive` | Unity | additive 节点（原生封装 + 自研 Job） |
| E-新 | `HN.Framework.Editor.Animation.Additive` | Editor | delta 烘焙器 |

## 十二、已确认决策

1. 实现路径：**A 原生为主 + B 自研补复杂**
2. 参考基准：默认 **BindPose**（Frame / Custom 预留）
3. 编号：**Additive = P2.5**，Inertialization 顺延 **P2.6**
4. 旋转混合：**加权 slerp**
5. additive **不含 root motion**
6. 层数上限默认 **4**（可调）；叠加在全部 override 层之上
