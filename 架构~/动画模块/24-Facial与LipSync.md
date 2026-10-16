# 24 · Facial 与 Lip Sync

> 动画模块文档 · 上级索引：[README](./README.md) · 关联：[05 PSD/DQ](./05-表现层形变-PSD与DQ-Skinning.md) · [21 Control Rig](./21-ControlRig.md) · [12 Additive](./12-Additive动画层.md) · [18 Compression](./18-AnimationCompression.md) · [04 LOD](./04-视野剔除与LOD分级.md)
> 最后更新：2026-10-15

---

## 一、定位

| 项 | 内容 | 驱动 |
|----|------|------|
| **Facial（表情）** | 情绪 / 微表情 | 参数 / 预设 / 事件 |
| **Lip Sync（口型）** | 音素 → viseme → 口型 | **仅语音分析** |

## 二、面部实现（BlendShape 为主）

| 方式 | 说明 | 采用 |
|------|------|:----:|
| **BlendShape** | 面部 blendshape（ARKit 52 / FACS 风格） | **主** |
| 骨骼（Bone） | 面部骨骼链 | 用 Control Rig（[21]）节点 |
| 双轨 | 骨骼 + blendshape 叠加 | 复杂角色 |

- **标准**：**FACS 与 ARKit 52 命名并存**（可配），支持自定义
- blendshape 复用 [05] **路径 A** 的形变管线

## 三、Lip Sync（仅语音分析）

```
语音 → 音素 → viseme（口型单元）→ 时间轴 → 平滑混合 → blendshape / 骨骼权重
```

| 驱动 | 时机 | 实现 |
|------|------|------|
| **离线预生成（优先）** | 配音 / 对白 | 分析音频 → viseme 序列（存 Luban / 资产） |
| **实时分析（可选）** | 语音 / 直播 | **提供接口**，项目/第三方接入（框架**不自研**） |

- **只从语音分析**，**不做文本（TTS）分析**
- viseme 序列 → 平滑混合（避免口型跳变）
- 与音频播放（**C11**）时间同步

## 四、归属

| 部分 | 归属 |
|------|------|
| `FacialRig` / `FacialExpression` / `VisemeMap` | L6（Core） |
| 表情混合 + Lip Sync 驱动 | 表现层（V4） |
| 实时音频分析 | **接口**（项目 / 第三方实现） |
| 面部 rig / viseme 配置 / 音频标注 | Editor |

## 五、与现有技术协作

| 技术 | 关系 |
|------|------|
| PSD（[05]） | 面部 blendshape 复用**路径 A** 形变管线 |
| Control Rig（[21]） | 骨骼面部用 Control Rig 节点 |
| Additive（[12]） | 表情可作为 additive 叠加 |
| Compression（[18]） | 面部 / viseme 数据走压缩管线 |
| C11 Audio | Lip Sync 音频源 + 同步 |
| C3 Localization | 多语言 viseme / 对白 |
| 网络动画（[19]） | **面部不同步**（带宽大，不做） |

## 六、LOD

| 档 | Facial | Lip Sync |
|:--:|--------|----------|
| L0 | 全 blendshape | ✓ |
| L1 | 关键表情 | ✓（近距） |
| L2 / L3 | 关 | 关 |

## 七、额外开发清单

1. L6：`FacialRig` / `FacialExpression` / `VisemeMap`
2. V4：表情混合（多表情叠加 / 掩码）
3. V4：Lip Sync 驱动器（viseme 序列 → 平滑混合 → 权重）
4. **实时分析接口**（`ILipSyncAnalyzer`，项目 / 第三方实现）
5. Editor：面部 rig 配置 + viseme 映射 + 音频标注工具
6. 与 C11 Audio 同步
7. LOD + Settings

## 八、命名空间

| 编号 | 命名空间 | 程序集 | 内容 |
|------|----------|:------:|------|
| L6 | `HN.Framework.Core.Level.Logic.Animation.Facial` | Core | 面部 / viseme 数据 + `ILipSyncAnalyzer` |
| V4.9 | `HN.Framework.Unity.Level.View.Animation.Facial` | Unity | 表情混合 + Lip Sync 运行时 |
| E-新 | `HN.Framework.Editor.Animation.Facial` | Editor | 面部 rig / viseme 配置工具 |

## 九、已确认

1. 面部实现：**BlendShape 为主**（骨骼走 Control Rig）
2. 标准：**FACS 与 ARKit 52 命名并存**（可配）
3. Lip Sync：**离线预生成优先 + 实时可选**
4. 实时分析：**提供接口，不自研**
5. 与 Control Rig：骨骼面部用 CR 节点，blendshape 面部独立
6. **面部网络同步：不需要**
7. 首版**分级**：**表情优先 / Lip Sync 后续**
8. **只从语音分析**（不做文本 TTS 分析）
