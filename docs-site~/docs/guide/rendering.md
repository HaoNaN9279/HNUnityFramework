---
sidebar_position: 18
---

# 渲染管线 🚧 Stub

HNRenderPipeline 是框架基于 SRP 的自定义渲染管线。

## 核心类型

- `HNRenderPipeline` — SRP 主类，管理渲染循环
- `HNRenderPipelineAsset` — 管线资源，配置渲染质量参数

## ShaderLibrary

框架提供基础 Shader 库，位于 `ShaderLibrary/`：`Core.hlsl`、`PBR.hlsl`、`Shadows.hlsl`、`PostProcess.hlsl`。

## SRP 分工

| 内容 | 仓库 |
|------|------|
| SRP 核心代码 + ShaderLibrary | Framework |
| 项目实际 Shader + SRP Asset 配置 | Art Repo |
