# 21 · Control Rig（通用程序化骨骼操控）

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[16 IK Rig](./16-IKRig骨骼操控.md) · [20 IK 应用 Solver](./20-IK应用-Solver.md) · [12 Additive](./12-Additive动画层.md) · [07 Inertialization](./07-Inertialization惯性化过渡.md) · [15 Montage](./15-Montage动作层.md) · [04 LOD](./04-视野剔除与LOD分级.md)
> 对标 UE Control Rig
> 最后更新：2026-10-15

---

## 一、定位

**通用程序化骨骼操控框架**（RigVM 式节点图），区别于 IK Rig（面向 IK / 重定向）：

| 能力 | IK Rig（[16]） | **Control Rig** |
|------|:--------------:|:---------------:|
| 任意骨骼操控 | 限链 | **任意** |
| 可视化节点图 | ✗ | **✓** |
| Controls 层级 + Spaces | ✗ | **✓** |
| Forward Solve | 部分 | **✓** |
| 创作（K 帧） | ✗ | ✗（首版不纳入） |

> 比 IK Rig 更宽的**通用**骨骼操控层。

## 二、系统组成

```
ControlRig
  ├── ControlRigGraph（节点图，继承 GraphData L5）
  ├── Controls（控制点层级 + Spaces）
  └── Runtime（Forward Solve，编译为 Job）
```

## 三、与 IK Rig / IK 应用 的关系

- Control Rig = 通用框架；IK Rig（[16]）= IK / 重定向专用
- **节点调用 IK Rig 的 Solver**（[16]/[20] 的 AimIK / FootIK / TwoBoneIK）
- 二者并存

## 四、数据模型（L6 扩展）

命名空间 `...Logic.Animation.ControlRig`：

| 类型 | 职责 |
|------|------|
| `ControlRigGraph` | 节点图（**继承 GraphData** L5，复用 E2 序列化） |
| `ControlRigNode` | 节点：骨骼读写 / 数学 / 约束 / IK / Spring / 自定义 |
| `Control` | 控制点：`Name` + 层级 + `Space` + 形状 |
| `ControlHierarchy` | Controls 树（父子继承） |

## 五、运行时（V4 扩展）

- **编译为 Job**：节点图在（导入 / 首次）**编译为 Burst Job** 执行（非解释）
- **仅 Forward Solve**：Controls → Bones（Backward Solve **首版不做**）
- 执行位置：表现层（`IAnimationJob` + Burst）

## 六、节点类型（首版）

| 类别 | 节点 |
|------|------|
| 骨骼 | Get / Set Bone Transform |
| Controls | 读写控制点 |
| 数学 | 向量 / 插值 / 曲线 |
| 约束 | Parent / Position / Rotation |
| IK | **复用 [20] 的 Solver**（Aim / Foot / TwoBone） |
| **二次运动** | **Spring 节点（含）** |
| 自定义 | C# / 脚本（桥接 C15） |

## 七、Controls 与 Spaces

- **Control 层级**：父子继承（拖父移子）
- **Space**：**World / Parent / Bone 都支持**，可切换

## 八、Editor 工具

- **复用 E2 Graph Editor**（`GraphEditorWindow` + `[GraphNodeType]`）
- 控制点可视化、层级、Space 编辑
- （创作 K 帧**首版不纳入**）

## 九、与现有技术协作

| 技术 | 关系 |
|------|------|
| Additive / Inertialization | Control Rig 在**其前** |
| IK Rig（[16]）/ IK 应用（[20]） | 节点可调用 |
| PSD / DQ（P6/P7） | 蒙皮侧，之后 |
| 二次运动 | **Spring 节点实现** |
| Montage（[15]） | 通知可驱动 Control Rig 参数 |
| C15 脚本 | 自定义节点桥接 |

## 十、LOD

| 档 | Control Rig |
|:--:|-------------|
| L0 | full graph |
| L1 | 关键节点 |
| L2 / L3 | 关 |

## 十一、额外开发清单

1. L6：`ControlRigGraph` / `ControlRigNode` / `Control` / `ControlHierarchy`
2. V4：节点图**编译为 Burst Job** + Forward Solve
3. V4：节点运行时（骨骼 / 数学 / 约束 / Spring）
4. IK 节点复用 [20] Solver
5. Controls 层级 + Spaces（World / Parent / Bone）
6. Editor：节点图（**复用 E2**）+ 控制点 / 层级 / Space 编辑
7. 与 Additive / Inertialization / IK Rig / Montage 集成
8. LOD + Settings

## 十二、命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.ControlRig` | Core | 节点图 / Controls 数据 |
| V4.7 | `HN.Framework.Unity.Level.View.Animation.ControlRig` | Unity | 运行时（编译 Job + Forward Solve） |
| E-新 | `HN.Framework.Editor.Animation.ControlRig` | Editor | 节点图编辑器（复用 E2） |

## 十三、已确认

1. 与 IK Rig 关系：**Control Rig 节点调用 IK Rig Solver**
2. 运行时执行：**编译为 Job**（非解释）
3. 首版节点集：**含 Spring 二次运动节点**
4. **Backward Solve 不做**（首版）
5. **创作 K 帧不纳入**（首版）
6. **Spaces**：World / Parent / Bone 都支持
7. **Editor 复用 E2**
