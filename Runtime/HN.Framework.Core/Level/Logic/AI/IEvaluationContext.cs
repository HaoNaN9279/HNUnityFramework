namespace HN.Framework.Core.Level.Logic.AI
{
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;

    /// <summary>
    /// 评估上下文输入接口 — 策略通过此接口访问知识池数据。
    /// 将 Blackboard、WorldState、Perception 聚合为统一入口，
    /// 避免策略直接依赖多个独立数据源。
    /// </summary>
    public interface IEvaluationContext
    {
        /// <summary>
        /// 黑板 — Agent 内部上下文数据共享。
        /// </summary>
        Blackboard Blackboard { get; }

        /// <summary>
        /// 世界状态缓存 — 感知数据与环境上下文。
        /// </summary>
        WorldStateCache WorldState { get; }

        /// <summary>
        /// 感知状态聚合。
        /// </summary>
        PerceptionState Perception { get; }
    }
}
