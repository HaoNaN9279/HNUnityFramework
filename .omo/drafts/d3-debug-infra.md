---
slug: d3-debug-infra
status: approved
intent: clear
pending-action: none (plan written, ready for $start-work)
approach: TDD, 5 D3 数据模型 + ILogProvider 扩展 + UnityLogProvider 更新 + GameWorld 集成。DebugHub 与 ILogProvider 职责完全分离。
---

# Draft: d3-debug-infra

## Components (topology ledger)
| id | outcome | status | evidence |
|----|---------|:------:|------|
| C1 | D3 数据模型 5 files | active | 最终架构.md:568-609 |
| C2 | ILogProvider 扩展 (保留兼容) | active | 最终架构.md:785-788 |
| C3 | UnityLogProvider 适配 | active | 最终架构.md:650-653 |
| C4 | DebugHub 实现 + GameWorld 集成 | active | 最终架构.md:341-367 |
| C5 | 单元测试 TDD | active | AGENTS.md:114-120 |

## Open assumptions
| assumption | adopted default | rationale | reversible? |
|------------|-----------------|-----------|:----------:|
| ILogProvider 新签名 | `void Log(LogLevel, string channel, string message, object context = null)` | 架构.md §八.C5；不加 `?` 与现有风格一致 | 是 |
| DebugHub 不耦合 ILogProvider | DebugHub 仅存储+事件，不做输出；ILogProvider 是唯一输出通道 | Metis F1 分析 | 是 |
| LogLevel 在 Driver，ILogProvider 引用它 | Capability → Driver 是框架允许的依赖方向 | 架构.md §三.3.1 | 否（架构设计） |
| LogEntry equality | 默认 readonly struct 逐字段比较，Context 按引用比较 | Metis F5 分析 | 是 |
| 新建文件启用 #nullable enable | 仅新 Core 文件；已有文件不改 | 最佳实践 | 是 |
| UnityLogProvider 测试用 LogAssert.Expect | NUnit + Unity Test Framework | AGENTS.md 规范 | 否 |

## Metis findings resolved
| # | Severity | Issue | Resolution |
|---|:--------:|-------|------------|
| F1 | Critical | DebugHub ↔ ILogProvider 关系未定义 | 职责完全分离 — DebugHub 存储+事件，ILogProvider 唯一输出 |
| F2 | High | GameWorld 初始化顺序 | F1 分离方案自然解决 — DebugHub 不需要 ILogProvider |
| F3 | High | LogLevel 命名空间依赖方向 | 确认 Capability → Driver 是正确架构方向，不做迁移 |
| F4 | Medium | ILogChannel 被 ILogProvider 忽略 | ILogChannel 供 DebugHub 使用（RegisterChannel），ILogProvider 使用原始 string channel |
| F5 | Medium | LogEntry struct + object Context | 明确相等性语义：Context 按引用比较，不实现 IEquatable |
| F6 | Medium | Core Tests 目录空 | Todo 会创建目录和文件 |
| F8 | Low | UnityLogProvider 测试策略 | 已明确使用 LogAssert.Expect |
| F7,F9,F10 | Low | 已记录，不阻塞 | — |

## Approval gate
status: approved
plan written: .omo/plans/d3-debug-infra.md
ready for: $start-work
