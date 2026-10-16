# 14 · Motion Warping（位移匹配）

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[13 Sync](./13-SyncGroups相位匹配.md) · [01 分层架构](./01-分层架构与设计原则.md) · [04 LOD](./04-视野剔除与LOD分级.md)
> 最后更新：2026-10-15

---

## 一、技术本质

**动态弯曲动画的位移/朝向，使动画的某个关键点对齐到运行时目标**（UE 的 Motion Warping）。

| 场景 | 对齐目标 |
|------|----------|
| 翻越 / 攀爬 | 手抓墙顶、脚落点 |
| 终结技 / 处决 | 攻击者 ↔ 目标角色位置 |
| 掩体转移 / 开门 | 手对齐把手、落脚点 |
| 跳跃 / 关卡交互 | 落点对齐 |

**要解决的**：关卡中玩家位置不固定 → 同一动画需**伸缩位移**以匹配任意目标。

## 二、归属与架构位置（关键：逻辑层）

当前方案已把**根运动移出表现层**（Layer5 输出 `MotionIntent{RootDelta, FacingDelta}` → 移动系统）。**Motion Warping 调整的正是这个位移意图 → 天然归逻辑层**。

| 部分 | 归属 |
|------|------|
| Warp window / target 定义 | L6 数据模型（Core） |
| 位移/朝向对齐计算 | **逻辑层**（Layer5 运动层子模块） |
| Warp Target 注入 | 逻辑层（gameplay 提供） |
| 表现层 | **原地动画**（不含根运动）→ 无新节点 |

> 与 Sync 同属**逻辑层技术**。表现层播放 in-place 动画，位移全由逻辑层的（warp 后）`MotionIntent` 驱动。

## 三、核心机制

### 3.1 Warp Window

- 动画中定义**时间窗口** `[t0, t1]` + 类型
- 窗口内执行 warp，窗口外不受影响

| 类型 | 说明 |
|------|------|
| Translation | 平移对齐 |
| Rotation | 朝向对齐 |
| Combined | 平移 + 朝向 |

### 3.2 Warp Target

- 运行时由 gameplay 提供：位置 + 朝向
- 来源：射线检测（墙顶）、目标实体 Transform、导航点
- 动画触发时设定，**窗口开始时刷新**

### 3.3 平移对齐（补偿法）

```
窗口 [t0, t1]，起始 P0，目标 P_target
全窗口期望位移   D      = P_target − P0
原始动画窗口位移 D_anim
补偿缺口         G      = D − D_anim

每帧：warpedDelta = originalDelta + G · deltaAlpha
    alpha = smoothstep((t − t0)/(t1 − t0))   // 平滑分摊，速度起止连续
```

- 保留原始 root motion 曲线特征，只叠加补偿缺口
- 窗口末总位移 = D（精确对齐）

### 3.4 朝向对齐

- 窗口内按目标朝向与当前朝向夹角，平滑旋转 `FacingDelta`
- 同样用平滑曲线分摊，避免朝向突变
- 与 **Sync 朝向处理统一**

## 四、数据模型（L6 扩展，Core）

命名空间 `...Logic.Animation.Warping`，走 Luban + AssetRef：

| 类型 | 职责 |
|------|------|
| `MotionWarpingDef` | 动画的 warp 定义：窗口列表 + 类型 |
| `WarpWindow` | `[t0, t1]` + 类型（Translation / Rotation / Combined） |
| `WarpTarget` | 运行时目标：位置 + 朝向（gameplay 注入） |
| `WarpAlignMode` | 枚举（对齐终点 / 自定义锚点） |

## 五、逻辑层实现

- Layer5 运动层内独立子模块：读原始 root motion（`RootMotionConfig` 提取）→ 施加 warp → 输出 warp 后的 `MotionIntent`
- **Warp Target 注入接口**：`IWarpTargetProvider`（gameplay 实现，射线/实体查询）
- 触发流：动画状态启动 → 设 warp target → 窗口开始时刷新 → 窗口内计算 → 移动系统消费
- **支持多 warp window 串联**（一个动画多个对齐阶段）

## 六、表现层

- **无新增节点**：表现层播 in-place 动画，位移由逻辑层 `MotionIntent` 驱动
- 姿态层面的「手/脚对齐」由 **Interaction IK**（表现层，见 [20](./20-IK应用-Solver.md)）负责：
  - Warping = 位移/朝向对齐（逻辑层）
  - Interaction IK = 骨骼姿态对齐（表现层）

## 七、与现有技术协作

| 技术 | 关系 |
|------|------|
| Sync Groups（逻辑层） | 相位同步「时间对齐」+ Warping「空间对齐」，互补 |
| 根运动（Layer5） | Warping 是对根运动意图的变形 |
| Inertialization（P2.6） | 姿态过渡；与 warping 正交 |
| Interaction IK（未来） | 骨骼级对齐，与 warping 组合完成完整交互 |
| Layer3 请求仲裁 | 动画触发时携带 warp 定义 |
| C16 Physics / 移动系统 | 消费 warped `MotionIntent` 更新角色位置 |

## 八、LOD / 网络

| 项 | 说明 |
|----|------|
| LOD | warping 在逻辑层 → **恒常**；不可见时照常计算（位移需正确） |
| 网络 | warp target 与结果位移是逻辑状态 → 可确定性同步 / 回放 |

## 九、额外开发清单

1. L6：`MotionWarpingDef` / `WarpWindow` / `WarpTarget` / `WarpAlignMode`
2. 逻辑层：MotionWarping 子模块（补偿法平移 + 朝向对齐）
3. 逻辑层：`IWarpTargetProvider` 注入接口（gameplay 实现）
4. 集成 Layer5 `MotionIntent` 输出 → 移动系统
5. 与 Sync 相位协作（时间 + 空间双对齐）
6. Editor：warp window 标注工具 + 预览
7. Settings：平滑曲线 / 默认对齐锚点

## 十、程序集 / 命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.Warping` | Core | warp 数据模型 + 运行时 |
| E-新 | `HN.Framework.Editor.Animation.Warping` | Editor | window 标注 / 预览工具 |

> 无表现层新节点（位移归逻辑层）。P 系列不变。

## 十一、已确认决策

1. Warping 放**逻辑层 Layer5**（与根运动一致）
2. 对齐锚点：默认**对齐终点** + 支持**自定义锚点**
3. 补偿曲线：**smoothstep**
4. Rotation warp 与 **Sync 朝向统一处理**
5. Warp Target：动画触发时设定、**窗口开始时刷新**
6. 与 Interaction IK 边界：**Warping=位移/朝向（逻辑层）**，Interaction IK=骨骼对齐（表现层）
7. **支持多 warp window 串联**
