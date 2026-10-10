# 动画 Playable 限制与规避方案

> 来源：知乎《【Unity】Animation Playable Bug、限制及解决方案汇总》（Solarian，2023-05-22）。
> 目的：记录 Unity Playable 的已知 Bug 与规则，及其对框架动画模块（L6 数据模型 / V4 Playable 运行时 / 动画五层架构 / C12 过场）的影响与替代方案，作为后续开发参考。
> 目标版本：Unity 2022.3+。
> 最后更新：2026-10-10

---

## 一、Bug 清单

| 编号 | 现象 | 命中模块 | 官方状态 | 规避 |
|------|------|---------|:--------:|------|
| UUM-30899 | `AnimationClipPlayable` 调 `Playable.Pause()` 后姿态暂停，但**根运动继续移动** | 暂停/时停 | 可规避 | 用 `SetSpeed(0)` 替代 `Pause()`，或补一次 `SetTime()` |
| UUM-14492 | 已暂停的 Playable 在角色**可见状态变化**（进出相机视野）时自动恢复播放 | 暂停/时停 | 可规避 | 用 `SetSpeed(0)` 替代 `Pause()` |
| UUM-32824 | `UpdateMode` 从 `Manual` 切回非 Manual 后**无法恢复自动播放** | 手动/确定性驱动 | 可规避 | `Stop()` + `Play()` 强制刷新状态 |
| UUM-33177 | 连续 2 次 `SetTime()` 跳时间时，**区间根运动被一次性施加**，位置突变 | Seek/跳帧/回放/网络校正 | 可规避，**不修** | `AnimationClipPlayable` 后接 `AnimationScriptPlayable`，Job 内手动剔除时间差根运动 |
| UUM-36098 | Humanoid 下 `AnimationStream.velocity` **改根运动速度不生效** | 自定义根运动 | 可规避 | 不用 `velocity`，手动从 `AnimationStream` 收集根运动并手动施加（引向 UUM-33952） |
| UUM-33952 | `Time.timeScale` 污染 `AnimationStream` 根运动旋转数据，**手动算根运动产生滑步** | 自定义根运动 | Unity 2022 难复现，**不修** | 接受精度损失；根运动尽量交由框架移动系统而非手写 |
| UUM-34442 | `AnimationScriptPlayable` 接入 `ScriptPlayable<T>` **非第 0 个输入端口时 Job 不执行** | 脚本 Job 扩展点 | 可规避，**不修** | `ScriptPlayable<T>` 只留 1 个输入端口，前面挂 `AnimationMixerPlayable` / `AnimationLayerMixerPlayable` |
| UUM-33944 | `AnimationScriptPlayable` **有效权重≠1** 时，Job 内 `PropertyStreamHandle` 改曲线失效 | IK/曲线写入 | 可规避 | 给 `Animator` 绑定 `GravityWeight` 属性（副作用未知），或避免非 1 权重 |
| UUM-31822 | `Animator.applyRootMotion=false` 时，绑定 Animator 节点的 `TransformStreamHandle` 导致**角色位置异常** | 物理驱动 | 无解 | 避开：不绑定 Animator 根节点 handle |

## 二、限制 / 规则

| 编号 | 规则 | 说明 |
|------|------|------|
| A | Editor 逐帧播放时 `PlayableGraph.IsPlaying()` **恒返回 false** | 设计如此。Runtime 逻辑若依赖该返回值，逐帧调试会行为不一致 |
| B | `AnimationScriptPlayable` 的**输入不受连接权重影响** | 预期行为，混合需在 Job 内手动处理 |
| C | 角色**不被渲染**时，脚本 Playable 的 `ProcessAnimation()` 不执行 | 预期行为；要强制执行需 `Animator.cullingMode = AlwaysAnimate`（性能代价） |
| D | 未输出到 `AnimationPlayableOutput` 的脚本 Playable，其 Job 不执行 | 预期行为，无影响 |

---

## 三、对当前动画规划的影响

规划涉及面：L6 `RootMotionConfig / IKConfig / SkeletonMask / BlendSpace1D/2D / AnimationEvent`；V4「根运动提取与应用 / IK 求解器 / `AnimationScriptPlayable` 自定义 Job 扩展点 / 混合空间 / AvatarMask」；五层架构 Layer5「根运动集成·物理计算·IK 求解」、Layer3「双缓冲」；C12 过场（`ICutscenePlayer` 含 `Pause/Resume/SeekTo`，底层 `PlayableDirector`）。

### 3.1 根运动（受影响最重）

- **暂停 / 时停**（游戏暂停、UI 弹出、C12 `PauseAll/ResumeAll`）→ UUM-30899 + UUM-14492。用 `Pause()` 会「姿态停、角色仍位移」，且镜头/可见性变化会自行复活。**无法直接用 `Pause()`**。
  - 替代：一律 `SetSpeed(0)`（必要时补一次 `SetTime`），框架自维护「暂停」状态位。
- **Seek / 跳帧 / 回放 / 网络时间校正**（C12 `SeekTo`、skip；Entity 网络同步时的动画对齐）→ UUM-33177。带根运动动画用 `SetTime()` 跳转会**把中间区段根运动一次性砸到角色上**。**无法直接做到精确 seek**。
  - 替代：`AnimationClipPlayable` 后串 `AnimationScriptPlayable`，Job 内剔除时间差根运动（引向 36098/33952）。
- **自定义根运动速度 / 缩放**（`RootMotionConfig` 提取轴向、缩放系数）→ UUM-36098。`AnimationStream.velocity` 改速度不生效。
  - 替代：手动收集根运动再手动施加；但 UUM-33952 会让手动根运动在 `timeScale` 下旋转数据被污染、产生滑步。**手写根运动无法完全等价。**
- **物理驱动 + 关闭 Animator 根运动**（Layer5「纯物理计算，与动画数据无关」）→ UUM-31822。`applyRootMotion=false` 时绑定 Animator 所在节点 `TransformStreamHandle` 会位置异常。
  - 替代：物理层不绑 Animator 根节点 handle；位移完全交由框架移动系统、动画只出姿态（与五层解耦方向一致，但**必须显式绕开该 handle**）。

### 3.2 IK 求解器（`IKConfig` / Layer5「IK 求解」）

IK 常规实现为 `AnimationScriptPlayable` + Job 读写 `AnimationStream`，直接命中多条：

- UUM-33944：IK 层**权重≠1 时写曲线失效**（IK 权重曲线、影响因子）。
  - 替代：绑 `GravityWeight` 属性绕过（副作用未知），或避免非 1 权重。
- 限制 C：角色**被视锥剔除后 `ProcessAnimation()` 不执行** → IK（脚部贴地、瞄准）冻结。
  - 替代：`cullingMode=AlwaysAnimate`（性能代价，与「不可见就不算」的初衷冲突），或接受剔除即冻结。
- UUM-34442：若多个脚本层串联时。

### 3.3 脚本 Job 扩展点（V4 明确预留）

- **UUM-34442**：多个脚本 Playable 挂到 `ScriptPlayable<T>` 多个输入端口 → Job 不执行。**无法直接做到多输入脚本拓扑**。
  - 替代：`ScriptPlayable<T>` 只留 **1 个输入端口**，前置 `AnimationMixerPlayable` / `AnimationLayerMixerPlayable` 做多入。
- **限制 B**：脚本 Playable 输入权重不生效，混合需在 Job 内自行计算 → 框架需自实现混合，不能靠 graph 权重。
- 根运动相关见 3.1。

### 3.4 混合空间 / 骨骼遮罩

- `BlendSpace1D/2D` 用 `AnimationMixerPlayable` 权重混合，**基本不受影响，可做**；若把评估搬进脚本 Job，则受限 B、UUM-34442。
- `SkeletonMask`（AvatarMask 走 `AnimationLayerMixerPlayable`）**可做**；若改为脚本内按 mask 手动混合，则同受限 B / UUM-34442。

### 3.5 动画事件 / 状态机 / 请求层

- `AnimationEvent`（标准化时间点）本身可做；只要不靠 `SetTime` 快进就不碰 33177。
- Layer3「双缓冲交换」、Layer4「状态机转换」是框架自身逻辑，**不受 Playable Bug 影响**。
- 限制 A：Editor 逐帧调试时 `IsPlaying()` 恒 false → 若 Layer3/4 用 `IsPlaying()` 判播放状态，**调试期与运行时行为不一致**。
  - 替代：框架自维护状态标志，不依赖 `IsPlaying()`；`UpdateMode` 切换用 `Stop()+Play()` 刷新。

### 3.6 C12 过场（现有实现）

`CutscenePlayer` 包 `PlayableDirector`，`ICutscenePlayer` 含 `Pause/Resume/SeekTo/Skip`。驱动带根运动角色时：`SeekTo/Skip` 命中 UUM-33177；`Pause/Resume` 需确认 `PlayableDirector` 内部是否已规避 UUM-30899/14492。

---

## 四、结论

### 无法做到

1. 用 `AnimationStream.velocity` 改 Humanoid 根运动速度（UUM-36098）。
2. `applyRootMotion=false` 下用 Animator 节点 `TransformStreamHandle` 且位置正常（UUM-31822）。
3. 脚本 Playable 接 `ScriptPlayable<T>` 非 0 输入端口时执行 Job（UUM-34442）。
4. 保证角色不可见时脚本 Playable 的 `ProcessAnimation` 执行（限制 C，除非 AlwaysAnimate）。

### 无法「直接」做到（需 hack / 手动，且有代价）

1. 用 `Pause()` 可靠暂停（根运动 / 可见性）→ 换 `SetSpeed(0)`。
2. 用 `SetTime()` 精确 seek 带根运动动画 → 加脚本 Playable 手动剔根运动。
3. 手写根运动计算在 `timeScale` 下无滑步（UUM-33952）→ 接受精度损失。
4. 脚本 Playable 权重≠1 时写曲线（UUM-33944）→ `GravityWeight` hack。
5. 依赖 `IsPlaying()` 判状态（限制 A）→ 自维护标志。
6. 脚本 Playable 输入权重自动混合（限制 B）→ 自实现。

### 替代方案汇总

| 场景 | 方案 |
|------|------|
| 暂停 | `SetSpeed(0)` 替代 `Pause()`（+ 补一次 `SetTime`） |
| UpdateMode 切换 | `Stop()` + `Play()` 强制刷新 |
| Seek + 根运动 | `AnimationClipPlayable` → `AnimationScriptPlayable`，Job 内剔根运动 |
| 根运动速度自定义 | 手动收集根运动替代 `velocity`（接受滑步） |
| 多输入脚本拓扑 | 单输入端口 + 前置 Mixer |
| 非 1 权重写曲线 | 绑 `GravityWeight` 属性 |
| Editor 状态判断 | 自维护播放状态标志，不依赖 `IsPlaying()` |
| 位移驱动 | 交五层 Layer5 物理系统，动画只出姿态（同时绕开 `TransformStreamHandle`） |

---

## 五、开发约束建议

- **`RootMotionConfig`**：若需「精确 seek / 回放 / 网络对齐」，须预留 `AnimationScriptPlayable` 根运动剔除层。
- **`IKConfig` / Layer5 IK**：接受「剔除即冻结」，或承担 `AlwaysAnimate` 开销，二选一并在文档中写明。
- **V4 脚本 Job 扩展点**：把「单输入端口 + 前置 Mixer」「输入权重需自算」列为硬约束写入接口文档。
- **暂停语义**：框架级暂停统一走 `SetSpeed(0)`，禁止直接 `Pause()`。
