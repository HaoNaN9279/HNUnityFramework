# 18 · Animation Compression

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[03 Jobs](./03-计算后端-Jobs与Burst.md) · [04 LOD](./04-视野剔除与LOD分级.md) · [05 PSD/DQ](./05-表现层形变-PSD与DQ-Skinning.md) · [06 2D](./06-2D骨骼动画子系统.md) · [17 Retargeting](./17-IKRetargeter重定向.md)
> 最后更新：2026-10-15

---

## 一、Unity 2022 内置压缩（调查结果）

内置压缩为**导入时（offline）**处理，烘焙进 `AnimationClip`；运行时直接消费，**无运行时压缩**。

| `Anim. Compression` | 行为 | Rig |
|---------------------|------|-----|
| **Off** | 不压缩，最高精度，文件/内存最大 | 不推荐 |
| **Keyframe Reduction** | 导入删冗余关键帧 | Generic / Humanoid |
| **Keyframe Reduction and Compression** | 删关键帧 + 存储压缩（仅减**文件**，内存不变） | 仅 Legacy |
| **Optimal**（默认） | 自动择「关键帧减少 或 dense」，运行时选最优曲线表示**减内存** | Generic / Humanoid |

误差参数（Keyframe Reduction / Optimal）：`Rotation Error`（角度）、`Position Error`（%）、`Scale Error`（%）。删帧规则：`|Original − Reduced| > |Original| × errorTolerance`。API：`ModelImporterAnimationCompression`。

> 无官方运行时动画压缩（DOTS Animation 已弃用）。

## 二、框架适用性

| 数据 | 来源 | 内置可用 | 决定 |
|------|------|:--------:|------|
| **3D clip** | Unity `AnimationClip` | ✅ | **采用自研**（统一管线，见下） |
| 2D Skeleton clip | Spine JSON → MemoryPack | ❌ | 自研 |
| PSD delta | 烘焙 → MemoryPack | ❌ | 自研 |
| Retargeting 烘焙 | Editor 烘焙 | ❌ | 自研 |
| 网络同步 | 运行时流 | ❌ | **首版纳入** |

> **决策**：全部数据走**自研统一压缩管线**（含 3D clip）。Unity 内置 `Optimal` 作为导入侧**参考 / 可选 fallback**。

## 三、自研压缩编解码器

### 3.1 范围

3D clip / 2D Skeleton / PSD delta / Retargeting 烘焙 / 网络同步 —— 统一格式。

- **3D clip 自研**：导入时从 `AnimationClip` 采样 → 重编码为压缩格式；运行时解压还原（构造 clip 数据喂 Playable）
  - 权衡：放弃 Unity 内置 `Optimal` 的密度优化，换**统一格式 + 更高压缩比 + 与自定义数据同管线**

### 3.2 技术

| 技术 | 说明 |
|------|------|
| **量化** | 四元数 **smallest-three（32bit）**；position / scale 量化；时间量化 |
| **关键帧减少** | **复刻 Unity 公开算法** + 误差阈值 |
| **冗余通道裁剪** | 恒定通道只存 1 帧 |
| **稀疏 bone mask** | 未动骨骼不存 |
| **位打包** | 位流编码 |
| **分层精度** | per-bone 精度（核心高 / 末端低）→ 与骨骼 LOD 联动 |

### 3.3 解压

- **加载时一次性解压** → 运行时零解压开销（默认）
- （可选流式解压，后续）

## 四、与现有机制协作

| 机制 | 关系 |
|------|------|
| 骨骼 LOD 剔除（[04]） | 剔除骨骼无需解压 → 联动降本 |
| LOD 档位（[04]） | 单精度（**不生成多精度版本**） |
| Jobs/Burst（[03]） | 自定义格式解压可走 Job |
| 2D / PSD（[05]/[06]） | 共用同一编解码器 |
| Retargeting（[17]） | 烘焙产物走压缩管线 |
| 构建管线（E9） | 导入时重编码 + 质检 |

## 五、额外开发清单

1. L6：自研压缩格式 + 编解码器（量化 + 关键帧减少 + 位打包）
2. V4：运行时解压（加载时一次性；Job 内可选）
3. Editor：3D clip 采样重编码工具 + 导入管线集成
4. Editor：压缩质检（体积 / 误差报告）
5. 分层精度（per-bone）配置
6. 2D / PSD / Retargeting 接压缩管线
7. Settings：误差阈值 / 精度分层
8. 网络同步数据压缩（**首版**）；（后续）流式解压

## 六、命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.Compression` | Core | 压缩格式 + 编解码器 |
| V4 | `HN.Framework.Unity.Level.View.Animation.Compression` | Unity | 运行时解压 |
| E-新 | `HN.Framework.Editor.Animation.Compression` | Editor | 重编码 / 质检工具 |

## 七、已确认

1. **3D clip 也用自研**（统一管线；内置 `Optimal` 作参考 / fallback）
2. 自研范围：3D clip / 2D Skeleton / PSD delta / Retargeting 烘焙
3. 量化：四元数 **smallest-three（32bit）**
4. 解压：**加载时一次性解压**
5. **不生成多精度版本**
6. 关键帧减少：**复刻 Unity 公开算法**
7. 网络同步数据压缩：**首版纳入**
