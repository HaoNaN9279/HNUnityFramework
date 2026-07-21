namespace HN.Framework.Core.Level.Logic.AI.Strategies
{
    using System;
    using System.Collections.Generic;
    using HN.Framework.Core.Driver.Common;
    using HN.Framework.Core.Driver.Common.Pool.ReferencePool;
    using HN.Framework.Core.Level.Logic.AI;

    #region BTNodeStatus 枚举

    /// <summary>
    /// 行为树节点执行状态枚举。
    /// </summary>
    public enum BTNodeStatus
    {
        /// <summary>
        /// 执行成功。
        /// </summary>
        Success,

        /// <summary>
        /// 执行失败。
        /// </summary>
        Failure,

        /// <summary>
        /// 执行中，尚未完成。
        /// </summary>
        Running
    }

    #endregion

    #region BTNode 抽象基类

    /// <summary>
    /// 行为树节点抽象基类。
    /// 所有行为树节点（组合、装饰、动作）均继承自此基类，
    /// 通过 <see cref="ReferencePool"/> 管理生命周期。
    /// </summary>
    public abstract class BTNode : IReference
    {
        /// <summary>
        /// 节点名称，用于调试和日志追踪。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 上次评估后的节点状态。
        /// </summary>
        public BTNodeStatus Status { get; protected set; }

        /// <summary>
        /// 当前节点产生的动作指令。
        /// 由叶子节点（<see cref="ActionNode"/>）生成，
        /// 组合节点和装饰节点会向上传播子节点的动作。
        /// </summary>
        public IActionCommand CurrentAction { get; set; }

        /// <summary>
        /// 评估当前节点，根据上下文决定节点状态和产生的动作。
        /// </summary>
        /// <param name="context">评估上下文，提供 Blackboard、WorldState、Perception 访问。</param>
        /// <returns>执行状态：Success、Failure 或 Running。</returns>
        public abstract BTNodeStatus OnEvaluate(IEvaluationContext context);

        /// <summary>
        /// 清理节点状态，归还引用池前由 <see cref="ReferencePool.Release"/> 调用。
        /// </summary>
        public virtual void Clear()
        {
            Name = null;
            Status = default;
            CurrentAction = null;
        }
    }

    #endregion

    #region CompositeNode 抽象类

    /// <summary>
    /// 组合节点抽象基类 — 包含多个子节点的行为树节点。
    /// 子类决定子节点的遍历和执行策略。
    /// </summary>
    public abstract class CompositeNode : BTNode
    {
        /// <summary>
        /// 子节点列表。
        /// </summary>
        protected readonly List<BTNode> m_Children = new List<BTNode>();

        /// <summary>
        /// 子节点只读列表。
        /// </summary>
        public IReadOnlyList<BTNode> Children => m_Children;

        /// <summary>
        /// 向组合节点添加子节点。
        /// </summary>
        /// <param name="child">要添加的子节点，为 null 时忽略。</param>
        public void AddChild(BTNode child)
        {
            if (child != null)
            {
                m_Children.Add(child);
            }
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            m_Children.Clear();
            base.Clear();
        }
    }

    #endregion

    #region SequenceNode

    /// <summary>
    /// 顺序节点 — 按添加顺序依次执行所有子节点。
    /// 所有子节点全部返回 <see cref="BTNodeStatus.Success"/> 时，本节点返回 Success；
    /// 任意子节点返回 <see cref="BTNodeStatus.Failure"/> 时，本节点立即返回 Failure；
    /// 任意子节点返回 <see cref="BTNodeStatus.Running"/> 时，本节点返回 Running 并记住当前位置，
    /// 下次评估从该子节点继续执行。
    /// </summary>
    public class SequenceNode : CompositeNode
    {
        private int m_CurrentIndex;

        /// <inheritdoc/>
        public override BTNodeStatus OnEvaluate(IEvaluationContext context)
        {
            while (m_CurrentIndex < m_Children.Count)
            {
                BTNode child = m_Children[m_CurrentIndex];
                BTNodeStatus childStatus = child.OnEvaluate(context);

                if (childStatus == BTNodeStatus.Failure)
                {
                    m_CurrentIndex = 0;
                    Status = BTNodeStatus.Failure;
                    CurrentAction = child.CurrentAction;
                    return BTNodeStatus.Failure;
                }

                if (childStatus == BTNodeStatus.Running)
                {
                    Status = BTNodeStatus.Running;
                    CurrentAction = child.CurrentAction;
                    return BTNodeStatus.Running;
                }

                // childStatus == Success, 继续下一个子节点
                CurrentAction = child.CurrentAction;
                m_CurrentIndex++;
            }

            // 所有子节点执行完毕
            m_CurrentIndex = 0;
            Status = BTNodeStatus.Success;
            return BTNodeStatus.Success;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            m_CurrentIndex = 0;
            base.Clear();
        }
    }

    #endregion

    #region SelectorNode

    /// <summary>
    /// 选择节点 — 按添加顺序依次尝试执行子节点，直到找到一个成功的子节点。
    /// 任意子节点返回 <see cref="BTNodeStatus.Success"/> 时，本节点立即返回 Success；
    /// 任意子节点返回 <see cref="BTNodeStatus.Running"/> 时，本节点返回 Running 并记住当前位置，
    /// 下次评估从该子节点继续执行；
    /// 所有子节点返回 <see cref="BTNodeStatus.Failure"/> 时，本节点返回 Failure。
    /// </summary>
    public class SelectorNode : CompositeNode
    {
        private int m_CurrentIndex;

        /// <inheritdoc/>
        public override BTNodeStatus OnEvaluate(IEvaluationContext context)
        {
            while (m_CurrentIndex < m_Children.Count)
            {
                BTNode child = m_Children[m_CurrentIndex];
                BTNodeStatus childStatus = child.OnEvaluate(context);

                if (childStatus == BTNodeStatus.Success)
                {
                    m_CurrentIndex = 0;
                    Status = BTNodeStatus.Success;
                    CurrentAction = child.CurrentAction;
                    return BTNodeStatus.Success;
                }

                if (childStatus == BTNodeStatus.Running)
                {
                    Status = BTNodeStatus.Running;
                    CurrentAction = child.CurrentAction;
                    return BTNodeStatus.Running;
                }

                // childStatus == Failure, 继续尝试下一个子节点
                m_CurrentIndex++;
            }

            // 所有子节点都失败了
            m_CurrentIndex = 0;
            Status = BTNodeStatus.Failure;
            CurrentAction = null;
            return BTNodeStatus.Failure;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            m_CurrentIndex = 0;
            base.Clear();
        }
    }

    #endregion

    #region ParallelNode

    /// <summary>
    /// 并行节点 — 同时执行所有子节点，根据成功阈值决定最终状态。
    /// 统计所有子节点中返回 <see cref="BTNodeStatus.Success"/> 的数量，
    /// 达到或超过 <see cref="SuccessThreshold"/> 时返回 Success；
    /// 任意子节点返回 <see cref="BTNodeStatus.Running"/> 时返回 Running；
    /// 否则返回 Failure。
    /// </summary>
    public class ParallelNode : CompositeNode
    {
        /// <summary>
        /// 成功阈值 — 达到此数量的子节点返回 Success 时，本节点返回 Success。
        /// </summary>
        public int SuccessThreshold { get; set; }

        /// <inheritdoc/>
        public override BTNodeStatus OnEvaluate(IEvaluationContext context)
        {
            int successCount = 0;
            int failureCount = 0;
            bool hasRunning = false;
            IActionCommand runningAction = null;

            foreach (BTNode child in m_Children)
            {
                BTNodeStatus childStatus = child.OnEvaluate(context);

                switch (childStatus)
                {
                    case BTNodeStatus.Success:
                        successCount++;
                        break;
                    case BTNodeStatus.Failure:
                        failureCount++;
                        break;
                    case BTNodeStatus.Running:
                        hasRunning = true;
                        runningAction = child.CurrentAction;
                        break;
                }
            }

            if (successCount >= SuccessThreshold)
            {
                Status = BTNodeStatus.Success;
                CurrentAction = null;
                return BTNodeStatus.Success;
            }

            if (hasRunning)
            {
                Status = BTNodeStatus.Running;
                CurrentAction = runningAction;
                return BTNodeStatus.Running;
            }

            Status = BTNodeStatus.Failure;
            CurrentAction = null;
            return BTNodeStatus.Failure;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            SuccessThreshold = 0;
            base.Clear();
        }
    }

    #endregion

    #region DecoratorNode 抽象类

    /// <summary>
    /// 装饰节点抽象基类 — 包含单个子节点的行为树节点。
    /// 子类通过修改或条件化子节点的评估结果来增强行为树逻辑。
    /// </summary>
    public abstract class DecoratorNode : BTNode
    {
        /// <summary>
        /// 被装饰的子节点。
        /// </summary>
        public BTNode Child { get; set; }

        /// <inheritdoc/>
        public override void Clear()
        {
            Child = null;
            base.Clear();
        }
    }

    #endregion

    #region InverterNode

    /// <summary>
    /// 反转节点 — 反转子节点的执行结果。
    /// 子节点返回 Success 时返回 Failure，返回 Failure 时返回 Success，
    /// 返回 Running 时保持 Running。
    /// </summary>
    public class InverterNode : DecoratorNode
    {
        /// <inheritdoc/>
        public override BTNodeStatus OnEvaluate(IEvaluationContext context)
        {
            if (Child == null)
            {
                Status = BTNodeStatus.Failure;
                CurrentAction = null;
                return BTNodeStatus.Failure;
            }

            BTNodeStatus childStatus = Child.OnEvaluate(context);
            CurrentAction = Child.CurrentAction;

            switch (childStatus)
            {
                case BTNodeStatus.Success:
                    Status = BTNodeStatus.Failure;
                    return BTNodeStatus.Failure;
                case BTNodeStatus.Failure:
                    Status = BTNodeStatus.Success;
                    return BTNodeStatus.Success;
                case BTNodeStatus.Running:
                    Status = BTNodeStatus.Running;
                    return BTNodeStatus.Running;
                default:
                    Status = BTNodeStatus.Failure;
                    return BTNodeStatus.Failure;
            }
        }
    }

    #endregion

    #region RepeaterNode

    /// <summary>
    /// 重复节点 — 重复执行子节点指定次数。
    /// <see cref="RepeatCount"/> 为 -1 时表示无限重复（始终返回 Running）。
    /// 每次子节点返回 Success 或 Failure 后，重复计数递增；
    /// 达到指定次数后返回 Success。
    /// </summary>
    public class RepeaterNode : DecoratorNode
    {
        /// <summary>
        /// 重复总次数。-1 表示无限重复。
        /// </summary>
        public int RepeatCount { get; set; }

        /// <summary>
        /// 当前已完成的重复次数。
        /// </summary>
        public int CurrentCount { get; set; }

        /// <inheritdoc/>
        public override BTNodeStatus OnEvaluate(IEvaluationContext context)
        {
            if (Child == null)
            {
                Status = BTNodeStatus.Failure;
                CurrentAction = null;
                return BTNodeStatus.Failure;
            }

            // 无限重复
            if (RepeatCount == -1)
            {
                BTNodeStatus childStatus = Child.OnEvaluate(context);
                CurrentAction = Child.CurrentAction;

                if (childStatus == BTNodeStatus.Running)
                {
                    Status = BTNodeStatus.Running;
                    return BTNodeStatus.Running;
                }

                // 子节点完成（Success 或 Failure），重新开始
                CurrentCount++;
                Status = BTNodeStatus.Running;
                return BTNodeStatus.Running;
            }

            // 有限次数重复
            if (CurrentCount >= RepeatCount)
            {
                Status = BTNodeStatus.Success;
                CurrentAction = null;
                return BTNodeStatus.Success;
            }

            BTNodeStatus status = Child.OnEvaluate(context);
            CurrentAction = Child.CurrentAction;

            if (status == BTNodeStatus.Running)
            {
                Status = BTNodeStatus.Running;
                return BTNodeStatus.Running;
            }

            // 子节点完成一次（Success 或 Failure）
            CurrentCount++;

            if (CurrentCount >= RepeatCount)
            {
                Status = BTNodeStatus.Success;
                return BTNodeStatus.Success;
            }

            Status = BTNodeStatus.Running;
            return BTNodeStatus.Running;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            RepeatCount = 0;
            CurrentCount = 0;
            base.Clear();
        }
    }

    #endregion

    #region ConditionalDecorator

    /// <summary>
    /// 条件装饰节点 — 仅在条件满足时评估子节点。
    /// 条件不满足时直接返回 <see cref="BTNodeStatus.Failure"/>。
    /// 适用于在行为树运行中动态启用/禁用分支。
    /// </summary>
    public class ConditionalDecorator : DecoratorNode
    {
        /// <summary>
        /// 条件委托。接收 <see cref="IEvaluationContext"/>，返回是否满足条件。
        /// </summary>
        public Func<IEvaluationContext, bool> Condition { get; set; }

        /// <inheritdoc/>
        public override BTNodeStatus OnEvaluate(IEvaluationContext context)
        {
            if (Condition == null)
            {
                Status = BTNodeStatus.Failure;
                CurrentAction = null;
                return BTNodeStatus.Failure;
            }

            if (!Condition(context))
            {
                Status = BTNodeStatus.Failure;
                CurrentAction = null;
                return BTNodeStatus.Failure;
            }

            if (Child == null)
            {
                Status = BTNodeStatus.Failure;
                CurrentAction = null;
                return BTNodeStatus.Failure;
            }

            BTNodeStatus childStatus = Child.OnEvaluate(context);
            Status = childStatus;
            CurrentAction = Child.CurrentAction;
            return childStatus;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            Condition = null;
            base.Clear();
        }
    }

    #endregion

    #region ActionNode

    /// <summary>
    /// 动作节点 — 行为树的叶子节点，通过工厂委托生成 <see cref="IActionCommand"/>。
    /// 每次评估时调用 <see cref="ActionFactory"/> 创建新的动作指令，
    /// 返回 <see cref="BTNodeStatus.Success"/>。
    /// </summary>
    public class ActionNode : BTNode
    {
        /// <summary>
        /// 动作工厂委托。接收 <see cref="IEvaluationContext"/>，返回 <see cref="IActionCommand"/>。
        /// 返回 null 表示不产生动作指令，此时返回 Failure。
        /// </summary>
        public Func<IEvaluationContext, IActionCommand> ActionFactory { get; set; }

        /// <inheritdoc/>
        public override BTNodeStatus OnEvaluate(IEvaluationContext context)
        {
            if (ActionFactory == null)
            {
                Status = BTNodeStatus.Failure;
                CurrentAction = null;
                return BTNodeStatus.Failure;
            }

            IActionCommand command = ActionFactory(context);
            if (command == null)
            {
                Status = BTNodeStatus.Failure;
                CurrentAction = null;
                return BTNodeStatus.Failure;
            }

            Status = BTNodeStatus.Success;
            CurrentAction = command;
            return BTNodeStatus.Success;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            ActionFactory = null;
            base.Clear();
        }
    }

    #endregion

    #region BehaviorTreeStrategy

    /// <summary>
    /// 行为树策略 — 基于行为树（Behavior Tree）的决策模型。
    /// 通过组合节点（Sequence、Selector、Parallel）、装饰节点（Inverter、Repeater、ConditionalDecoarator）
    /// 和动作节点（ActionNode）构建决策树，每帧从根节点开始评估并返回单一动作指令。
    /// </summary>
    /// <remarks>
    /// 典型使用流程:
    /// <code>
    /// var strategy = new BehaviorTreeStrategy();
    /// strategy.Initialize();
    ///
    /// // 构建行为树
    /// var root = strategy.AddSelector("RootSelector");
    /// var attackSeq = strategy.AddSequence("AttackSequence");
    /// attackSeq.AddChild(strategy.AddConditional("EnemyNear", ctx => ctx.Perception.NearestEnemy != null));
    /// attackSeq.AddChild(strategy.AddAction("Attack", ctx => ctx.Blackboard.TryGet&lt;IActionCommand&gt;("attack_cmd", out var cmd) ? cmd : null));
    /// root.AddChild(attackSeq);
    /// root.AddChild(strategy.AddAction("Idle", ctx => ctx.Blackboard.TryGet&lt;IActionCommand&gt;("idle_cmd", out var cmd) ? cmd : null));
    ///
    /// strategy.SetRoot(root);
    ///
    /// // 每帧评估: var commands = strategy.Evaluate(context);
    /// </code>
    /// </remarks>
    public class BehaviorTreeStrategy : IDecisionStrategy
    {
        /// <inheritdoc/>
        public string Name { get; set; } = "BehaviorTree";

        /// <inheritdoc/>
        public int Priority { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        private BTNode m_Root;
        private readonly List<BTNode> m_AllNodes = new List<BTNode>();

        /// <summary>
        /// 获取行为树的根节点。
        /// </summary>
        public BTNode Root => m_Root;

        /// <summary>
        /// 初始化策略，准备内部节点追踪列表。
        /// 在添加到决策管线时自动调用一次。
        /// </summary>
        public void Initialize()
        {
            m_AllNodes.Clear();
            m_Root = null;
        }

        /// <summary>
        /// 评估行为树：从根节点开始递归评估，返回根节点执行后产生的动作指令列表。
        /// </summary>
        /// <param name="context">评估上下文，传递给各节点的 OnEvaluate。</param>
        /// <returns>
        /// 动作指令列表。策略禁用、根节点为空或评估结果不产生动作时返回空列表。
        /// 注意：返回列表为新创建的列表，调用方负责管理列表内指令的生命周期。
        /// </returns>
        public IReadOnlyList<IActionCommand> Evaluate(IEvaluationContext context)
        {
            if (!IsEnabled || m_Root == null)
            {
                return Array.Empty<IActionCommand>();
            }

            BTNodeStatus status = m_Root.OnEvaluate(context);

            if (status == BTNodeStatus.Failure)
            {
                return Array.Empty<IActionCommand>();
            }

            IActionCommand action = m_Root.CurrentAction;
            if (action == null)
            {
                return Array.Empty<IActionCommand>();
            }

            return new List<IActionCommand> { action };
        }

        /// <summary>
        /// 重置策略：递归释放所有节点到引用池并清空内部状态。
        /// 在关卡重开或 Agent 重生时调用。
        /// </summary>
        public void Reset()
        {
            if (m_AllNodes != null)
            {
                // 逆序释放，确保子节点在父节点之前释放
                for (int i = m_AllNodes.Count - 1; i >= 0; i--)
                {
                    ReferencePool.Release(m_AllNodes[i]);
                }

                m_AllNodes.Clear();
            }

            m_Root = null;
        }

        /// <summary>
        /// 设置行为树的根节点。每次评估从该节点开始递归遍历。
        /// </summary>
        /// <param name="root">根节点，通常为组合节点。</param>
        public void SetRoot(BTNode root)
        {
            m_Root = root;
        }

        /// <summary>
        /// 从引用池创建一个 <see cref="SequenceNode"/> 并注册到内部追踪列表。
        /// </summary>
        /// <param name="name">节点名称，用于调试。</param>
        /// <returns>创建的 SequenceNode 引用。</returns>
        public SequenceNode AddSequence(string name)
        {
            SequenceNode node = ReferencePool.Acquire<SequenceNode>();
            node.Name = name;
            m_AllNodes.Add(node);
            return node;
        }

        /// <summary>
        /// 从引用池创建一个 <see cref="SelectorNode"/> 并注册到内部追踪列表。
        /// </summary>
        /// <param name="name">节点名称，用于调试。</param>
        /// <returns>创建的 SelectorNode 引用。</returns>
        public SelectorNode AddSelector(string name)
        {
            SelectorNode node = ReferencePool.Acquire<SelectorNode>();
            node.Name = name;
            m_AllNodes.Add(node);
            return node;
        }

        /// <summary>
        /// 从引用池创建一个 <see cref="ParallelNode"/> 并注册到内部追踪列表。
        /// </summary>
        /// <param name="name">节点名称，用于调试。</param>
        /// <param name="successThreshold">成功阈值。达到此数量的子节点返回 Success 时，并行节点返回 Success。</param>
        /// <returns>创建的 ParallelNode 引用。</returns>
        public ParallelNode AddParallel(string name, int successThreshold)
        {
            ParallelNode node = ReferencePool.Acquire<ParallelNode>();
            node.Name = name;
            node.SuccessThreshold = successThreshold;
            m_AllNodes.Add(node);
            return node;
        }

        /// <summary>
        /// 从引用池创建一个 <see cref="InverterNode"/> 并注册到内部追踪列表。
        /// </summary>
        /// <param name="name">节点名称，用于调试。</param>
        /// <returns>创建的 InverterNode 引用。</returns>
        public InverterNode AddInverter(string name)
        {
            InverterNode node = ReferencePool.Acquire<InverterNode>();
            node.Name = name;
            m_AllNodes.Add(node);
            return node;
        }

        /// <summary>
        /// 从引用池创建一个 <see cref="RepeaterNode"/> 并注册到内部追踪列表。
        /// </summary>
        /// <param name="name">节点名称，用于调试。</param>
        /// <param name="repeatCount">重复次数。-1 表示无限重复。</param>
        /// <returns>创建的 RepeaterNode 引用。</returns>
        public RepeaterNode AddRepeater(string name, int repeatCount)
        {
            RepeaterNode node = ReferencePool.Acquire<RepeaterNode>();
            node.Name = name;
            node.RepeatCount = repeatCount;
            m_AllNodes.Add(node);
            return node;
        }

        /// <summary>
        /// 从引用池创建一个 <see cref="ConditionalDecorator"/> 并注册到内部追踪列表。
        /// </summary>
        /// <param name="name">节点名称，用于调试。</param>
        /// <param name="condition">
        /// 条件委托。接收 <see cref="IEvaluationContext"/>，返回是否满足条件。
        /// 条件不满足时节点返回 Failure 而不评估子节点。
        /// </param>
        /// <returns>创建的 ConditionalDecorator 引用。</returns>
        public ConditionalDecorator AddConditional(string name, Func<IEvaluationContext, bool> condition)
        {
            ConditionalDecorator node = ReferencePool.Acquire<ConditionalDecorator>();
            node.Name = name;
            node.Condition = condition;
            m_AllNodes.Add(node);
            return node;
        }

        /// <summary>
        /// 从引用池创建一个 <see cref="ActionNode"/> 并注册到内部追踪列表。
        /// </summary>
        /// <param name="name">节点名称，用于调试。</param>
        /// <param name="actionFactory">
        /// 动作工厂委托。接收 <see cref="IEvaluationContext"/>，返回 <see cref="IActionCommand"/>。
        /// 返回 null 时节点返回 Failure。
        /// </param>
        /// <returns>创建的 ActionNode 引用。</returns>
        public ActionNode AddAction(string name, Func<IEvaluationContext, IActionCommand> actionFactory)
        {
            ActionNode node = ReferencePool.Acquire<ActionNode>();
            node.Name = name;
            node.ActionFactory = actionFactory;
            m_AllNodes.Add(node);
            return node;
        }
    }

    #endregion
}
