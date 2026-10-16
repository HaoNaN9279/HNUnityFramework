# 13 · Sync Groups / Phase Matching

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[07 Inertialization](./07-Inertialization惯性化过渡.md) · [12 Additive](./12-Additive动画层.md) · [01 分层架构](./01-分层架构与设计原则.md) · [04 LOD](./04-视野剔除与LOD分级.md)
> 最后更新：2026-10-15

---

## 一、技术本质

| 概念 | 定义 |
|------|------|
| **Sync Groups** | 一族相关 clip 的**相位保持同步**（walk↔run↔sprint 步态连续，不跳脚） |
| **Phase Matching** | 切换时对齐两 clip 的**相位/播放位置**，使过渡自然 |

**要解决的问题**：传统 cross-fade 从 walk 切 run，若 walk 在 30% 相位、run 从 0% 起 → 脚步滑动/踩错。同步组让目标 clip 从**相同相位**起步。

**典型场景**：Locomotion 步态（idle→walk→jog→run）、BlendSpace 内多 clip、战斗连招衔接、2D 步态。

## 二、归属与架构位置（关键：逻辑层为主）

**相位 = 播放位置信息 → 归逻辑层**（与帧事件、根运动一致）。

| 部分 | 归属 |
|------|------|
| Sync Group 定义 / sync markers | L6 数据模型（Core） |
| 相位维护 / 相位匹配决策 / leader 缩放 | **逻辑层**（Layer4 行为层，或独立 Sync 子模块） |
| 按相位采样姿态 | 表现层（依契约 `NormalizedTime`） |
| 姿态过渡衔接 | 表现层（P2.6 Inertialization） |

> 与 Inertialization/Additive（表现层）不同：**Sync 主体在逻辑层**。因当前契约已有 `NormalizedTime`，相位匹配结果就是「逻辑层把目标状态的归一化时间设为匹配值」，**契约几乎无需扩展**。

## 三、核心机制

### 3.1 Sync Group + Markers

- 每个 clip 标注 **sync marker**（时间点 + 语义，如「左脚踏地」「右脚踏地」）
- 组内 clip 按 marker 语义对齐；组有 **leader clip**，其余跟随

### 3.2 相位模型

| 模型 | 说明 |
|------|------|
| 归一化时间相位 | `p = time / duration`，简单；但不同长度 clip 步态不同步 |
| **Sync marker 相位（采用）** | `p` 定义为「相邻两 marker 区间的插值位置」，按语义对齐 |

### 3.3 相位匹配算法

```
源 S 当前时间 ts → 归一化 pS = ts / durS
  → 找 S 相邻 markers [mS_i, mS_{i+1}]，局部比例 r = (pS − mS_i)/(mS_{i+1} − mS_i)
  → 目标 T 同语义区间 [mT_j, mT_{j+1}]
  → 目标时间 tT = (mT_j + r·(mT_{j+1} − mT_j)) · durT
```

| 模式 | 说明 |
|------|------|
| 直接匹配 | `tT = f(pS)` |
| **预测式匹配（默认）** | 计入过渡时长 D：`pS' = pS + D·speed / durS` 后再映射，避免过渡完仍错位 |
| Foot-plant 对齐 | 让目标下一个着地 marker 对齐 |

### 3.4 组内时长缩放（leader 驱动）

- 跟随 clip 相位由 leader 驱动：`time_follow = leaderPhase · durFollow`
- 使组内不同时长 clip 相位一致（BlendSpace 内尤其重要）

## 四、数据模型（L6 扩展，Core）

命名空间 `...Logic.Animation.Sync`，走 Luban + AssetRef：

| 类型 | 职责 |
|------|------|
| `SyncGroupDef` | 组：成员 clip 索引 + leader + 参考语义 |
| `SyncMarker` | 时间点 + 语义（FootPlantL / FootPlantR / Custom） |
| `SyncClipEntry` | clip 的 markers 表 + 归属组 |
| `SyncMatchMode` | 枚举 Direct / Predictive / FootPlant |

## 五、逻辑层实现

- 每个动画实例维护**当前相位**（含所属 sync marker 区间）
- 状态切换时执行**相位匹配**（§3.3），把目标状态 `NormalizedTime` 设为匹配值
- 组内以 **leader 相位**驱动跟随者
- **帧事件与 marker 统一为一套标记** → 相位匹配后事件时间同步映射
- 与根运动协作：相位对齐 = 根运动轨迹对齐（减少位移跳变）

> 相位推进用 `HNLogicTime.DeltaTime` → 确定性、可回放、可网络同步。

## 六、表现层应用

- 仅按契约 `NormalizedTime` 采样（**无新增节点**）
- 姿态差异由 P2.6 Inertialization 吸收（相位接连续 + 惯性化姿态）
- BlendSpace 内相位由逻辑层给（表现层不做步态同步）

## 七、与现有技术协作

| 技术 | 关系 |
|------|------|
| BlendSpace（P2） | 组内 clip 相位对齐（否则混合时脚步错位） |
| Inertialization（P2.6） | Sync 管「位置」连续，惯性化管「姿态」连续，互补 |
| 帧事件（逻辑层） | marker 即事件点，相位匹配后同步映射 |
| 根运动（逻辑层） | 相位对齐减少位移跳变 |
| Motion Warping（未来） | 相位 + 位移双对齐 |
| Layer3 请求仲裁 | 切换时附带相位匹配策略 |

## 八、LOD / 视野剔除 / 网络

| 项 | 说明 |
|----|------|
| LOD | 相位在逻辑层，**恒常推进**；表现降采样时仍按逻辑 time 采样 |
| 视野剔除 | 逻辑层持续推进相位 → 重新可见时**直接采正确相位**（比 Inertialization 更自然） |
| 网络 | 相位是逻辑状态，可序列化同步（契合帧同步/回放） |

## 九、额外开发清单

1. L6：`SyncGroupDef` / `SyncMarker` / `SyncClipEntry` / `SyncMatchMode`
2. Editor：sync marker 标注工具（时间轴标 marker）+ 组配置 + 校验
3. 逻辑层：相位维护 + 相位匹配（Direct / Predictive / FootPlant）
4. 逻辑层：leader 驱动的组内时长缩放
5. 逻辑层：帧事件与 marker 时间点统一
6. 与 Layer3 请求仲裁 / 状态切换集成
7. 与 BlendSpace 相位对齐
8. Settings：默认匹配模式 / 过渡时长预测参数
9. （可选）Editor 相位可视化调试

## 十、程序集 / 命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.Sync` | Core | Sync Group / marker / 相位数据 + 运行时 |
| E-新 | `HN.Framework.Editor.Animation.Sync` | Editor | marker 标注 / 组配置工具 |

> 无表现层新节点（相位归逻辑层）。P 系列不变。

## 十一、已确认决策

1. 相位运行时归**逻辑层**（复用 `NormalizedTime`，无表现层新节点）
2. 默认**预测式**匹配（Direct / FootPlant 可选）
3. marker 语义：内置 FootPlantL / FootPlantR + 自定义
4. 预测时长 D 由 **Inertialization 自适应 period** 提供
5. BlendSpace 内相位由 **leader 驱动**
6. **帧事件与 marker 统一**为一套标记
7. **2D Skeleton 共用**同一 sync 机制
