# 22 · 物理二次运动（Dynamic Bone / Spring / Cloth）

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[21 Control Rig](./21-ControlRig.md) · [04 LOD](./04-视野剔除与LOD分级.md) · [03 Jobs](./03-计算后端-Jobs与Burst.md) · [05 PSD/DQ](./05-表现层形变-PSD与DQ-Skinning.md)
> 最后更新：2026-10-15

---

## 一、定位

| 类型 | 载体 | 典型用途 | 算法 |
|------|------|----------|------|
| **Spring / Dynamic Bone** | **骨骼链** | 头发 / 尾巴 / 飘带 / 耳朵 / 装饰 | Verlet + 弹簧约束 |
| **Cloth** | **网格** | 披风 / 布料 / 裙摆 | PBD / XPBD |

> 均为**表现层**物理二次运动。

## 二、归属与架构位置

```
P3 IK Rig 求解
  → P3.5 Dynamic Bone / Spring（骨骼链物理）  ← 新增
  → P4 骨骼输出 → P6 PSD → P7 Skinning（Cloth 在蒙皮侧）
```

- 表现层；**依赖最终骨骼姿态**（父骨骼运动驱动）
- Spring / Dynamic Bone 改**骨骼** → P3 后
- Cloth 改**顶点** → 蒙皮侧（类似 PSD 的形变应用）

## 三、Spring / Dynamic Bone

### 3.1 数据模型（L6）

| 类型 | 职责 |
|------|------|
| `DynamicBoneChain` | 骨骼链：根骨 + 链长 + 有效骨骼 |
| `DynamicBoneParams` | 刚度 / 阻尼 / 重力 / 惯性 / 角度限制 / 风力 |
| `DynamicBoneCollider` | 碰撞体（球 / 胶囊 / 地面） |

### 3.2 算法

```
每粒子（每骨骼）：
  Verlet 积分（惯性）+ 重力 / 外力 / 风力
  → 弹簧约束（保持骨长）+ 角度限制
  → 碰撞求解（球 / 胶囊 / 地面）
  → 迭代收敛
→ 写回骨骼旋转
```

- **Job / Burst** 并行（每链独立）
- 惯性由父骨骼运动计算

## 四、Cloth

### 4.1 数据模型

| 类型 | 职责 |
|------|------|
| `ClothMesh` | 质点 + 约束（结构 / 剪切 / 弯曲） |
| `ClothCollider` | 碰撞体 |

### 4.2 算法

- **PBD / XPBD**（位置基动力学）
- 输出顶点位移 → 蒙皮侧应用（类似 PSD）
- 实现：**CPU Job（默认）/ GPU Compute（可选）双轨**

## 五、与 Control Rig Spring 节点的边界

| 项 | Control Rig Spring 节点（[21]） | Dynamic Bone 系统 |
|----|--------------------------------|-------------------|
| 定位 | **通用节点**（图内） | **专门骨骼链物理系统** |
| 复杂度 | 轻量 | 完整（碰撞 / 风力 / LOD） |
| 用途 | 简单弹性 | 头发 / 尾巴 / 飘带 |

> **Dynamic Bone 独立系统**（专业、可 LOD、可碰撞）；Control Rig Spring 节点用于图内轻量弹性。

## 六、与现有技术协作

| 技术 | 关系 |
|------|------|
| Control Rig（[21]） | Spring 节点做轻量；本系统做专业链物理 |
| LOD（[04]） | 远距关 / 降迭代（**链级粒度**） |
| Jobs/Burst（[03]） | 模拟走 Job |
| PSD / DQ（[05]） | Cloth 顶点应用同侧 |
| C16 Physics | **碰撞体来源** |
| 渲染管线无关 | **不用 Unity 内建 Cloth（PhysX）**，自研 |

## 七、LOD

| 档 | Spring / Dynamic Bone | Cloth |
|:--:|:---------------------:|:-----:|
| L0 | 全链 + 全迭代 + 碰撞 | 全模拟 |
| L1 | 降迭代 / 关碰撞 | 降迭代 |
| L2 | 关（冻结） | 关 |
| L3 | 停 | 停 |

## 八、额外开发清单

1. L6：`DynamicBoneChain` / `Params` / `Collider`、`ClothMesh` / `ClothCollider`
2. V4：Spring / Dynamic Bone 模拟（Verlet + 约束 + 碰撞，Job/Burst）
3. V4：Cloth 模拟（PBD/XPBD；CPU Job + GPU Compute 双轨）
4. 碰撞体接 C16 Physics
5. LOD 集成（链级粒度：关 / 降迭代）
6. Editor：链 / 参数 / 碰撞体配置工具
7. Settings：默认参数 / LOD 粒度

## 九、命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.Secondary` | Core | Dynamic Bone / Cloth 数据模型 |
| V4.8 | `HN.Framework.Unity.Level.View.Animation.Secondary` | Unity | 模拟运行时（Job/Burst/Compute） |
| E-新 | `HN.Framework.Editor.Animation.Secondary` | Editor | 链 / 参数 / 碰撞体配置工具 |

## 十、已确认

1. **Dynamic Bone 独立系统**（非仅复用 Control Rig 节点）
2. Cloth 实现：**CPU Job 默认 + GPU Compute 可选**（双轨）
3. 物理引擎：**自研**（保渲染管线无关）
4. 碰撞体：**复用 C16 Physics**
5. 首版范围：**Spring / Dynamic Bone 优先，Cloth 后续**
6. LOD 粒度：**链级**
7. **不使用** Unity 内建 Cloth（PhysX）
