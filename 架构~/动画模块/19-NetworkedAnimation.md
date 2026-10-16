# 19 · 网络动画

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[03 Jobs](./03-计算后端-Jobs与Burst.md) · [04 LOD](./04-视野剔除与LOD分级.md) · [07 Inertialization](./07-Inertialization惯性化过渡.md) · [15 Montage](./15-Montage动作层.md) · [18 Compression](./18-AnimationCompression.md) · [GameWorld 与命名空间](../02-GameWorld与命名空间.md)（C6 Network）
> 最后更新：2026-10-15

---

## 一、技术本质

多人在线需**同步动画**。两条路线：

| 路线 | 同步内容 | 带宽 | 精度 |
|------|----------|:----:|------|
| **状态同步（采用）** | 动画参数 / 状态 / 相位 | 低 | 远端本地重建 |
| 姿态同步 | 骨骼姿态（压缩） | 高 | 精确 |

**采用 = 状态同步 + 本地重建**：动画确定性，给「状态 + 参数 + 相位」远端即可重建。

> **契合**：框架逻辑层纯 C# + 确定性（Fixed64）+ 状态机 → 同步**逻辑状态**，表现层本地重建。

## 二、归属

| 部分 | 归属 |
|------|------|
| 动画状态（权威、可序列化） | 逻辑层 |
| 传输 / 权限 / 预测校验 | C6 Network（FishNet 封装） |
| 远端重建（表现层） | Unity |
| 逻辑 ↔ 网络桥接 | 适配器（**放 Framework**） |

## 三、核心设计

### 3.1 同步内容（最小集）

| 字段 | 说明 |
|------|------|
| `AnimStateHash` | 当前状态 |
| `NormalizedTime` / 相位 | 播放位置（Sync 相位） |
| `BlendParams` | 混合参数（speed / direction / blendspace 参数） |
| `MontageState` | montage 段 + 时间（若有） |
| `Flags` | grounded 等状态位 |

> Root motion 位移由**移动系统同步**（不复用动画通道）。

### 3.2 同步频率 & 变化驱动

- **可配置**（Settings）；默认 **15Hz**（10~20Hz 区间）
- **变化驱动**：状态切换立即发
- 空闲角色可进一步降频

### 3.3 插值 / 外推

- 远端按相位**外推**：`phase += speed · dt`
- 状态切换 → **Inertialization（P2.6）在远端同样适用**
- 修正误差用短 blend

### 3.4 权威模型（三者都支持，按项目选）

| 模式 | 说明 |
|------|------|
| **服务器权威** | Server 算状态 → 广播 |
| **客户端预测 + 服务器校验** | 本地预测，Server 校正（对齐 C6.2） |
| **帧同步（Lockstep）** | 确定性重放（对齐 Fixed64 + C6 LockstepManager） |

### 3.5 Relevancy

- **复用视野剔除 / LOD 判定**（[04]）：只同步视野内 / 关心的角色
- 距离 / 重要性优先级

### 3.6 回滚支持

- 动画状态 **MemoryPack 可序列化** + **Fixed64 确定性** → 回滚**仅存状态快照 + Deterministic 重放**
- 通知（Montage / 帧事件）可重放（与 [15] 一致）

## 四、与 C6 Network 集成

| C6 子模块 | 关系 |
|-----------|------|
| C6.1 状态同步 | 动画状态走 SyncVar / 批量 |
| C6.2 预测与校验 | 本地预测 + Server 校正动画状态 |
| C6.3 权限与生命周期 | 网络实体 → 动画实例绑定 |
| LockstepManager | 帧同步重放动画状态 |

- 适配器 `NetworkedAnimationAdapter`（逻辑状态 ↔ 网络包）**放 Framework**

## 五、与压缩协作（首版纳入）

网络同步的动画数据走**自研压缩管线（[18]）**：

| 数据 | 压缩 |
|------|------|
| 状态参数 | 量化（StateHash 16bit / 相位 8~16bit / BlendParams 各 8bit） |
| 增量 | 只发变化字段 |

> [18] 的「网络暂缓」已调整为 **首版纳入网络同步数据压缩**。

## 六、额外开发清单

1. L6：`NetAnimationState`（可序列化动画状态）+ 量化
2. 桥接：`NetworkedAnimationAdapter`（打包 / 解包 ↔ C6，放 Framework）
3. 逻辑层：状态采集 + 变化驱动发送
4. 远端：状态应用 + 相位外推 + Inertialization 修正
5. Relevancy：与视野剔除 / LOD 联动
6. 回滚：状态快照 + Deterministic 重放（含通知）
7. 与 [18] 压缩管线接入（首版）
8. C6 集成：SyncVar / 预测校验 / Lockstep
9. Settings：同步频率 / 量化精度 / Relevancy 半径

## 七、命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.Net` | Core | 网络动画状态 + 序列化 |
| V4 | `HN.Framework.Unity.Level.View.Animation.Net` | Unity | 网络适配器 + 远端重建 |

## 八、已确认

1. 采用**状态同步 + 本地重建**（非姿态同步）
2. 权威模型**三种都支持**（服务器权威 / 预测 / 帧同步，按项目选）
3. 同步频率**可配置**（默认 15Hz）
4. Relevancy **复用视野剔除 / LOD** 判定
5. **姿态同步暂不考虑**
6. 回滚：**仅状态快照 + Deterministic 重放**
7. 适配器**放 Framework**
8. 网络数据压缩**首版纳入**（[18]）
