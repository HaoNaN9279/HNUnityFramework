namespace HN.Framework.Core.Level.Logic.AI.Strategies
{
    using System;
    using System.Collections.Generic;
    using HN.Framework.Core.Driver.Common;
    using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

    #region DecisionTreeNodeType 枚举

    /// <summary>
    /// 决策树节点类型枚举。
    /// </summary>
    public enum DecisionTreeNodeType
    {
        /// <summary>
        /// 条件节点 — 通过条件委托进行分支判断，持有 True/False 子节点引用。
        /// </summary>
        Condition,

        /// <summary>
        /// 动作节点 — 叶子节点，通过动作工厂生成 <see cref="IActionCommand"/>。
        /// </summary>
        Action
    }

    #endregion

    #region DecisionTreeNode 类

    /// <summary>
    /// 决策树节点。
    /// 条件节点（<see cref="NodeType"/> 为 <see cref="DecisionTreeNodeType.Condition"/>）持有
    /// <see cref="Condition"/> 委托和 <see cref="TrueNode"/> / <see cref="FalseNode"/> 分支子节点；
    /// 动作节点（<see cref="NodeType"/> 为 <see cref="DecisionTreeNodeType.Action"/>）持有
    /// <see cref="ActionFactory"/> 委托用于生成 <see cref="IActionCommand"/>。
    /// 通过 <see cref="ReferencePool"/> 管理生命周期。
    /// </summary>
    public class DecisionTreeNode : IReference
    {
        /// <summary>
        /// 节点名称，用于调试和日志追踪。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 节点类型，决定节点作为条件分支还是叶子动作。
        /// </summary>
        public DecisionTreeNodeType NodeType { get; set; }

        /// <summary>
        /// 条件委托。仅在 <see cref="NodeType"/> 为 <see cref="DecisionTreeNodeType.Condition"/> 时有效。
        /// 接收 <see cref="IEvaluationContext"/>，返回条件是否满足。
        /// </summary>
        public Func<IEvaluationContext, bool> Condition { get; set; }

        /// <summary>
        /// 条件为 true 时遍历的子节点。仅在条件节点下有效。
        /// </summary>
        public DecisionTreeNode TrueNode { get; set; }

        /// <summary>
        /// 条件为 false 时遍历的子节点。仅在条件节点下有效。
        /// </summary>
        public DecisionTreeNode FalseNode { get; set; }

        /// <summary>
        /// 动作工厂委托。仅在 <see cref="NodeType"/> 为 <see cref="DecisionTreeNodeType.Action"/> 时有效。
        /// 接收 <see cref="IEvaluationContext"/>，返回 <see cref="IActionCommand"/>。
        /// 返回 null 表示不产生动作指令。
        /// </summary>
        public Func<IEvaluationContext, IActionCommand> ActionFactory { get; set; }

        /// <summary>
        /// 清理节点所有字段，归还引用池前由 <see cref="ReferencePool.Release"/> 调用。
        /// </summary>
        public void Clear()
        {
            Name = null;
            NodeType = default;
            Condition = null;
            TrueNode = null;
            FalseNode = null;
            ActionFactory = null;
        }
    }

    #endregion

    #region DecisionTreeStrategy 类

    /// <summary>
    /// 决策树策略 — 基于条件分支的决策模型。
    /// 通过构建 <see cref="DecisionTreeNode"/> 条件节点和动作节点组成的决策树，
    /// 深度优先遍历评估并收集 <see cref="IActionCommand"/> 列表。
    /// </summary>
    /// <remarks>
    /// 典型使用流程:
    /// <code>
    /// var strategy = new DecisionTreeStrategy();
    /// strategy.Initialize();
    ///
    /// var isEnemyNear = strategy.AddConditionNode("IsEnemyNear", ctx => ctx.Perception.NearestEnemy != null);
    /// var attack = strategy.AddActionNode("Attack", ctx => ReferencePool.Acquire&lt;YourAttackAction&gt;());
    /// var patrol = strategy.AddActionNode("Patrol", ctx => ReferencePool.Acquire&lt;YourMoveToAction&gt;());
    /// isEnemyNear.TrueNode = attack;
    /// isEnemyNear.FalseNode = patrol;
    /// strategy.SetRoot(isEnemyNear);
    ///
    /// // 每帧: var commands = strategy.Evaluate(context);
    /// </code>
    /// </remarks>
    public class DecisionTreeStrategy : IDecisionStrategy
    {
        /// <inheritdoc/>
        public string Name => "DecisionTree";

        /// <inheritdoc/>
        public int Priority { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        private DecisionTreeNode m_Root;
        private List<DecisionTreeNode> m_Nodes;

        /// <summary>
        /// 初始化策略，准备内部节点追踪列表。
        /// 在添加到决策管线时自动调用一次。
        /// </summary>
        public void Initialize()
        {
            m_Nodes = new List<DecisionTreeNode>();
        }

        /// <summary>
        /// 从引用池创建一个条件节点并添加到决策树。
        /// </summary>
        /// <param name="name">节点名称，用于调试和日志追踪。</param>
        /// <param name="condition">
        /// 条件委托。接收 <see cref="IEvaluationContext"/>，返回条件是否满足。
        /// 返回 true 时遍历 <see cref="DecisionTreeNode.TrueNode"/>，否则遍历 <see cref="DecisionTreeNode.FalseNode"/>。
        /// </param>
        /// <returns>创建的条件节点引用，可用于设置 TrueNode / FalseNode 子节点。</returns>
        public DecisionTreeNode AddConditionNode(string name, Func<IEvaluationContext, bool> condition)
        {
            DecisionTreeNode node = ReferencePool.Acquire<DecisionTreeNode>();
            node.Name = name;
            node.NodeType = DecisionTreeNodeType.Condition;
            node.Condition = condition;
            m_Nodes.Add(node);
            return node;
        }

        /// <summary>
        /// 从引用池创建一个动作节点并添加到决策树。
        /// </summary>
        /// <param name="name">节点名称，用于调试和日志追踪。</param>
        /// <param name="actionFactory">
        /// 动作工厂委托。接收 <see cref="IEvaluationContext"/>，返回 <see cref="IActionCommand"/>。
        /// 返回 null 表示不产生动作指令。
        /// </param>
        /// <returns>创建的动作节点引用。</returns>
        public DecisionTreeNode AddActionNode(string name, Func<IEvaluationContext, IActionCommand> actionFactory)
        {
            DecisionTreeNode node = ReferencePool.Acquire<DecisionTreeNode>();
            node.Name = name;
            node.NodeType = DecisionTreeNodeType.Action;
            node.ActionFactory = actionFactory;
            m_Nodes.Add(node);
            return node;
        }

        /// <summary>
        /// 设置决策树的根节点。每次评估从该节点开始深度优先遍历。
        /// </summary>
        /// <param name="root">根节点，通常为条件节点。</param>
        public void SetRoot(DecisionTreeNode root)
        {
            m_Root = root;
        }

        /// <summary>
        /// 评估决策树：从根节点开始深度优先遍历，收集所有叶子动作指令。
        /// </summary>
        /// <param name="context">评估上下文，传递给条件和动作工厂。</param>
        /// <returns>
        /// 动作指令列表。策略禁用或根节点为空时返回空列表。
        /// 注意：返回列表为内部列表的只读包装，调用方不应缓存跨帧引用。
        /// </returns>
        public IReadOnlyList<IActionCommand> Evaluate(IEvaluationContext context)
        {
            if (!IsEnabled || m_Root == null)
            {
                return Array.Empty<IActionCommand>();
            }

            var results = new List<IActionCommand>();
            EvaluateNode(m_Root, context, results);
            return results;
        }

        /// <summary>
        /// 重置策略：释放所有节点到引用池并清空内部状态。
        /// 在关卡重开或 Agent 重生时调用。
        /// </summary>
        public void Reset()
        {
            if (m_Nodes != null)
            {
                foreach (DecisionTreeNode node in m_Nodes)
                {
                    ReferencePool.Release(node);
                }

                m_Nodes.Clear();
            }

            m_Root = null;
            m_Nodes = null;
        }

        /// <summary>
        /// 深度优先遍历决策树节点，将动作指令收集到结果列表。
        /// </summary>
        /// <param name="node">当前遍历的节点。</param>
        /// <param name="context">评估上下文，传递给条件委托和动作工厂。</param>
        /// <param name="results">动作指令结果列表，遍历过程中追加新指令。</param>
        private void EvaluateNode(DecisionTreeNode node, IEvaluationContext context, List<IActionCommand> results)
        {
            if (node == null)
            {
                return;
            }

            if (node.NodeType == DecisionTreeNodeType.Condition)
            {
                if (node.Condition == null)
                {
                    return;
                }

                bool conditionResult = node.Condition(context);
                EvaluateNode(conditionResult ? node.TrueNode : node.FalseNode, context, results);
            }
            else if (node.NodeType == DecisionTreeNodeType.Action)
            {
                if (node.ActionFactory == null)
                {
                    return;
                }

                IActionCommand command = node.ActionFactory(context);
                if (command != null)
                {
                    results.Add(command);
                }
            }
        }
    }

    #endregion
}
