using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.AI
{
    /// <summary>
    /// 决策策略接口 — 所有内置策略的插件核心。
    /// 每个策略独立评估当前上下文并返回动作指令列表。
    /// </summary>
    public interface IDecisionStrategy
    {
        /// <summary>
        /// 策略名称，用于日志追踪和调试。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 优先级，数值越高在管线中越先评估。
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 策略是否启用。禁用后管线将跳过此策略。
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 初始化策略。在添加到决策管线时调用一次。
        /// </summary>
        void Initialize();

        /// <summary>
        /// 评估当前上下文，返回决策的动作指令列表。
        /// </summary>
        /// <param name="context">评估上下文，提供 WorldState、Blackboard、PerceptionState 访问。</param>
        /// <returns>要执行的动作指令列表，可为空。</returns>
        IReadOnlyList<IActionCommand> Evaluate(IEvaluationContext context);

        /// <summary>
        /// 重置策略状态。在关卡重开或 Agent 重生时调用。
        /// </summary>
        void Reset();
    }
}
