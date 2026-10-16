# 08 · Playable 限制与模块对接

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[03 Jobs](./03-计算后端-Jobs与Burst.md) · [动画Playable限制与规避方案](../动画Playable限制与规避方案.md)
> 最后更新：2026-10-15

---

## 一、与 Playable 已知限制的对应

> 完整清单见 [动画Playable限制与规避方案](../动画Playable限制与规避方案.md)。

| 限制 | 本方案应对 |
|------|-----------|
| UUM-30899 / 14492（Pause 复活） | 表现层统一 `SetSpeed(0)`，框架自维护暂停位 |
| UUM-33177（seek 根运动突变） | 根运动移出表现层 → 天然规避 |
| 限制 C（不可见不跑 ProcessAnimation） | L3 档主动利用；L0/L1 需 IK 时 `AlwaysAnimate` |
| UUM-34442（多输入脚本端口） | P2.6 / P3 单输入端口 + 前置 Mixer |
| UUM-33944（权重≠1 写曲线失效） | 避免非 1 权重，或 `GravityWeight` hack |
| UUM-31822（TransformStreamHandle） | 物理层不绑 Animator 根节点 |
| 限制 A（IsPlaying 恒 false） | 逻辑层自维护状态标志，不依赖 `IsPlaying()` |
| 限制 C（PSD） | PSD 依赖姿态 → 不可见同样冻结，与 L3 一致（预期） |
| UUM-34442（PSD） | P6 的 `AnimationScriptPlayable` 同须单输入端口 |
| UUM-33944（PSD） | P6 只读姿态、不写曲线 → 规避 |
| UUM-31822（DQ） | 路径 B/C 避开 Animator 根节点 handle |
| 2D Skeleton | 不走 Playables，本节限制不适用 |

---

## 二、与现有模块对接

| 模块 | 关系 |
|------|------|
| **GameWorld**（D1） | 新增内置 `AnimationLogicSystem : ITickable`，仿 AISystem 统一驱动所有 `AnimationLogicInstance`，走 `HNLogicTime.DeltaTime` |
| **L2 HFSM** | Layer4 直接用 HFSM 做状态机 |
| **L3 Entity** | 逻辑实例按 EntityId 挂载 |
| **L0 GameplayTag** | 状态 / 条件标记 |
| **C16 Physics** | 接收 `MotionIntent` 做位移；物理计算从动画层剥离 |
| **C7 EventBus** | 发布动画状态变更事件供 UI / 战斗消费 |
| **C4 AssetManager + AssetRef** | clip / AvatarMask / PSD delta / 2D 图集 经 `AssetRef<T>` + Addressables 加载 |
| **D4.6 Settings** | LOD 阈值 + Burst 模式 + Inertialization 参数配置 SO |
| **V2 EntityView** | 表现层挂 EntityView 上，逻辑与表现按 EntityId 解耦 |
| **Burst / Collections** | 表现层 Jobs 后端依赖（Unity 侧，Core 不感知） |
| **E2 Graph Editor** | 可复用做 PSD 编辑 / 可视化 |
| **Skeleton2D** | 复用逻辑层（选 animation / 时间 / 事件）+ LOD + Jobs/Burst；独立表现端子系统 |
| **Editor 导入器** | Spine JSON → MemoryPack `.bin`；图集经 AssetRef + Addressables |
