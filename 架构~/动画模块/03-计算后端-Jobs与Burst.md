# 03 · 计算后端 Jobs 与 Burst

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[02 数据契约](./02-数据契约与逻辑表现分离.md) · [08 Playable 限制](./08-Playable限制与模块对接.md)
> 最后更新：2026-10-15

---

## 定位

Jobs 仅用于**表现层**。逻辑层（Core）保持主线程纯 C#，**不 Job 化**。

## 三层计算模型

| 平面 | 程序集 | 线程 | 计算内容 | Job 类型 |
|------|:------:|------|----------|----------|
| 逻辑层 | Core | 主线程 | 决策 / 状态机 / 权重评估 | 无（纯 C#） |
| 表现姿态平面 | Unity | Burst 并行 | 姿态混合 / IK / Mask | **`IAnimationJob`**（`AnimationScriptPlayable` 内） |
| 表现批处理 | Unity | 并行 | LOD / 可见性距离 + 骨骼剔除批算 | `IJobParallelFor` |

## 数据桥接

```
Core blittable AnimationPoseContract（主线程产出）
  → 表现层 memcpy → NativeArray（PersistentAllocator 双缓冲池）
  → AnimationScriptPlayable 的 IAnimationJob(Burst) 读 → 姿态
```

## Burst 模式开关

Settings SO，随 LOD 配置同处，**全局**：

| 模式 | 说明 |
|------|------|
| `Performance` | Burst + 并行 + fastmath。单机项目默认 |
| `Deterministic` | 关闭 fastmath + 单线程。帧同步 / 回放项目 |

## Jobs 与 Playables 的关系

| 平面 | Job 类型 | 原因 |
|------|----------|------|
| 姿态平面（混合 / IK / Mask） | **只能 `AnimationScriptPlayable` 内 `IAnimationJob`** | Unity 唯一允许写 `AnimationStream` 的 Job 通道 |
| 表现批处理（LOD / 可见性） | 普通 `IJobParallelFor` | 纯数据，不进 `AnimationStream` |

**Jobs ≠ 替代 Playables**：Jobs 是「计算执行方式」，Playables 是「图 + 姿态管线」，二者协作。

## 踩坑约束（Job 化后升级为硬约束）

- UUM-34442：`ScriptPlayable<T>` 单输入端口 + 前置 Mixer
- UUM-33944：避免非 1 权重，或 `GravityWeight` hack
- 限制 B：脚本 Playable 输入权重不生效，混合须 Job 内自算

> 完整清单见 [动画Playable限制与规避方案](../动画Playable限制与规避方案.md)。
