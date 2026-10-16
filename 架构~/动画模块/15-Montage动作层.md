# 15 · Montage 一次性动作层

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[07 Inertialization](./07-Inertialization惯性化过渡.md) · [12 Additive](./12-Additive动画层.md) · [13 Sync](./13-SyncGroups相位匹配.md) · [14 Warping](./14-MotionWarping位移匹配.md) · [01 分层架构](./01-分层架构与设计原则.md)
> 最后更新：2026-10-15

---

## 一、技术本质

**Montage**（借 UE AnimMontage）：管理**一次性动作**（技能 / 攻击 / 受击 / 处决 / 交互）的播放、分层、中断。

| 维度 | 循环动画（Locomotion） | Montage |
|------|----------------------|---------|
| 生命周期 | 常驻循环 | 播放一次即结束 |
| 触发 | 由状态机常驻 | 由请求触发 |
| 中断 | 状态切换 | 优先级 / 中断规则 |
| 分层 | 基础层 | 可全身 / 上半身 / 叠加 |

> 当前 Layer3 请求层已有「过滤 / 优先级仲裁 / 双缓冲」——**Montage 是它的实例化承载**：把「动画请求」升级为「可中断的动作层」。

## 二、归属与架构位置

| 部分 | 归属 |
|------|------|
| Montage 定义（段落 / 通知 / 分层 / 优先级） | L6 数据模型（Core） |
| 调度 / 仲裁 / 中断 / 段落流转 | **逻辑层**（Layer3/L4） |
| 逐帧推进 + 事件触发 | **逻辑层**（`HNLogicTime`，确定性） |
| Root motion 意图 | 逻辑层（→ `MotionIntent`，可被 Warping 作用） |
| 层混合 / mask 覆盖 | 表现层（复用 P2 override / additive 层） |

> 逻辑层调度为主，表现层复用现有混合层（**不新增 P 节点**）。

## 三、核心机制

### 3.1 Montage 定义

| 要素 | 说明 |
|------|------|
| 段落（Sections） | 一个 montage 分段（连招 1→2→3），可跳转 |
| 通知（Notifies） | 关键时刻事件（伤害帧 / 特效 / 无敌帧）——**与帧事件统一** |
| 分层（LayerType） | FullBody / UpperBody / Additive |
| 优先级 | 数字，决定可否打断 / 被中断 |
| 混合时间 | 进入 / 退出 blend 时长 |

### 3.2 Slot（动作层槽位）

| Slot | 表现层映射 |
|------|-----------|
| FullBody | override（压制 locomotion） |
| UpperBody | override + 上半身 AvatarMask |
| Additive | P2.5 additive 层 |

### 3.3 中断与优先级仲裁

```
新请求 → 比较优先级
  ├── 高 → 打断当前（走 blend out）
  ├── 低 → 排队（可配开关）或忽略
  └── 规则：CanBeInterruptedBy / 中断窗口 / 已播放比例
```

- 每个 montage：`InterruptPriority` + `CanBeInterruptedBy` + 中断窗口
- **中断粒度支持两种**：按「已播放比例窗口」或「段落边界」
- 全身 montage 压制 locomotion；上半身 montage 与 locomotion 并行

### 3.4 段落与连招

- 段落边界 + **提前输入缓冲**（combo buffer，时长可配）
- 段内可跳转下一段（链式），窗口外输入被缓冲 / 丢弃

## 四、数据模型（L6 扩展，Core）

命名空间 `...Logic.Animation.Montage`，走 Luban + AssetRef：

| 类型 | 职责 |
|------|------|
| `MontageDef` | montage 根：clip / 段落 / 通知 / slot / 优先级 / 混合时间 |
| `MontageSection` | 段落：时间范围 + 跳转目标 |
| `MontageNotify` | 通知：时间点 + 类型 + 参数（= 帧事件） |
| `MontageInterruptRule` | 中断规则：优先级 + CanBeInterruptedBy + 窗口 |
| `MontageSlotType` | 枚举 FullBody / UpperBody / Additive |

## 五、逻辑层实现

- **`MontageManager`（逻辑层，ITickable）**：
  - 请求 → 仲裁 → 激活 / 排队 / 打断
  - 逐帧推进时间、触发通知（帧事件）
  - 段落流转 + combo 缓冲
  - 输出：当前 montage 状态 + slot + root motion 意图
- 与状态机协作：**FullBody montage 抑制 locomotion 状态推进（逻辑层暂停其 Tick）**；UpperBody 并行
- 与 Layer3 请求层：Montage 是其承载，仲裁逻辑统一

## 六、表现层

- **复用现有混合层**（P2 override / P2.5 additive），按 slot + mask 叠加
- 进入 / 退出由 **Inertialization（P2.6）** 平滑
- 无新增 P 节点（可选：封装 `MontageLayer` 便捷组件）

## 七、契约扩展（逻辑 → 表现）

`AnimationStateSnapshot` 增加 montage 层：

| 字段 | 含义 |
|------|------|
| `MontageLayers[]` | 每项：`MontageHash` + `NormalizedTime` + `Weight` + `SlotType` + `MaskId` |

## 八、与现有技术协作

| 技术 | 关系 |
|------|------|
| Layer3 请求层 | Montage 是其实例化承载（优先级仲裁统一） |
| 帧事件 | Montage 通知 = 帧事件，逻辑层自触发 |
| Inertialization（P2.6） | montage 进入/退出平滑 |
| Additive（P2.5） | Additive slot 映射 |
| Sync Groups（逻辑层） | montage 自身**可携带 sync markers** |
| Motion Warping（逻辑层） | montage 的 root motion 可被 warp（处决对齐），**可携带 warp windows** |
| Interaction IK | montage 期间的手/脚骨骼对齐 |

## 九、LOD / 网络

| 项 | 说明 |
|----|------|
| LOD | 逻辑层恒常；表现层层混合随 LOD 降级（可关上层） |
| 网络 | montage 状态（段 / 时间 / 通知）是逻辑状态 → 可确定性同步；回滚需重放通知 |

## 十、额外开发清单

1. L6：`MontageDef` / `MontageSection` / `MontageNotify` / `MontageInterruptRule` / `MontageSlotType`
2. 逻辑层：`MontageManager`（调度 / 仲裁 / 推进 / 段落 / combo 缓冲）
3. 逻辑层：与 Layer3 请求层统一仲裁
4. 逻辑层：通知 → 帧事件统一
5. 契约：`AnimationStateSnapshot.MontageLayers[]`
6. 表现层：slot → 混合层 + mask 映射（复用 P2 / P2.5）
7. 与 Inertialization / Sync / Warping / Interaction IK 协作
8. Editor：montage 段落 / 通知标注工具
9. Settings：默认混合时间 / 中断策略 / combo 缓冲时长

## 十一、程序集 / 命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.Montage` | Core | montage 数据模型 + `MontageManager` 运行时 |
| E-新 | `HN.Framework.Editor.Animation.Montage` | Editor | 段落 / 通知标注工具 |

> 表现层复用现有混合层，无新 P 节点。

## 十二、已确认决策

1. Montage 调度放**逻辑层**；表现层复用混合层
2. 低优先级请求：**排队**（可配开关，默认排队）
3. 中断粒度：**支持「已播放比例窗口」与「段落边界」两种**
4. 通知与帧事件**统一为一套**
5. combo 缓冲窗口：**可配置**
6. FullBody 抑制 locomotion：**逻辑层暂停其 Tick**
7. montage **可携带 sync markers / warp windows**
