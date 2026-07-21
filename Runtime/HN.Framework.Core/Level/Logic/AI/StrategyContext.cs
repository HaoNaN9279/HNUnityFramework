namespace HN.Framework.Core.Level.Logic.AI
{
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;

    /// <summary>
    /// 评估上下文实现 — 聚合黑板、世界状态缓存和感知状态，
    /// 为策略评估提供统一的只读数据入口。
    /// </summary>
    public class StrategyContext : IEvaluationContext
    {
        /// <summary>
        /// 使用指定的知识池组件初始化上下文。
        /// </summary>
        /// <param name="blackboard">黑板</param>
        /// <param name="worldState">世界状态缓存</param>
        /// <param name="perception">感知状态</param>
        public StrategyContext(Blackboard blackboard, WorldStateCache worldState, PerceptionState perception)
        {
            Blackboard = blackboard;
            WorldState = worldState;
            Perception = perception;
        }

        /// <summary>
        /// 黑板 — Agent 内部上下文数据共享。
        /// </summary>
        public Blackboard Blackboard { get; }

        /// <summary>
        /// 世界状态缓存 — 感知数据 + 环境上下文。
        /// </summary>
        public WorldStateCache WorldState { get; }

        /// <summary>
        /// 感知状态聚合。
        /// </summary>
        public PerceptionState Perception { get; }
    }
}
