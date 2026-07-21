namespace HN.Framework.Core.Level.Logic.AI
{
    using System.Collections.Generic;
    using HN.Framework.Core.Driver.Common;
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;

    /// <summary>
    /// Agent 运行状态
    /// </summary>
    public enum AgentStatus
    {
        /// <summary>
        /// 未激活 — Agent 尚未初始化或已停用
        /// </summary>
        Inactive,

        /// <summary>
        /// 激活 — Agent 正常执行感知/决策/动作循环
        /// </summary>
        Active,

        /// <summary>
        /// 暂停 — Agent 已初始化但暂时停止 Tick
        /// </summary>
        Paused,
    }

    /// <summary>
    /// AI Agent 容器 — 聚合 KnowledgePool、DecisionPipeline、ActionExecutor，
    /// 提供统一的 Agent 生命周期管理与每帧感知→决策→执行循环。
    /// 实现 <see cref="IReference"/> 接口以支持引用池复用。
    /// </summary>
    public class AIAgent : IReference
    {
        // === KnowledgePool ===

        /// <summary>
        /// 黑板 — Agent 内部上下文数据共享
        /// </summary>
        public Blackboard Blackboard { get; private set; }

        /// <summary>
        /// 世界状态缓存 — 感知数据与运行时环境上下文
        /// </summary>
        public WorldStateCache WorldState { get; private set; }

        /// <summary>
        /// 感知状态聚合 — 汇总视觉、听觉等多通道感知数据
        /// </summary>
        public PerceptionState Perception { get; private set; }

        private StrategyContext m_Context;

        // === 决策管线 ===

        /// <summary>
        /// 决策管线编排器 — 按层级组织策略链
        /// </summary>
        public DecisionPipeline Pipeline { get; private set; }

        /// <summary>
        /// 策略注册中心 — 注册、查询和管理所有可用决策策略
        /// </summary>
        public StrategyRegistry Registry { get; private set; }

        // === 动作执行 ===

        private Queue<IActionCommand> m_ActionQueue;

        /// <summary>
        /// 每帧最大执行动作数，默认为 5
        /// </summary>
        public int MaxActionsPerTick { get; set; } = 5;

        // === Agent 状态 ===

        /// <summary>
        /// Agent 当前运行状态
        /// </summary>
        public AgentStatus Status { get; private set; } = AgentStatus.Inactive;

        /// <summary>
        /// Agent 名称，用于日志追踪和调试
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 初始化 Agent 的所有子组件：
        /// Blackboard、WorldStateCache、PerceptionState、StrategyContext、
        /// DecisionPipeline、StrategyRegistry 以及内部动作队列。
        /// </summary>
        /// <param name="name">Agent 名称</param>
        public void Initialize(string name)
        {
            Name = name;

            Blackboard = new Blackboard();
            Blackboard.Initialize();

            WorldState = new WorldStateCache();
            WorldState.Initialize();

            Perception = new PerceptionState();
            Perception.Initialize();

            m_Context = new StrategyContext(Blackboard, WorldState, Perception);

            Pipeline = new DecisionPipeline();
            Pipeline.Initialize();

            Registry = new StrategyRegistry();
            Registry.Initialize();

            m_ActionQueue = new Queue<IActionCommand>();
            Status = AgentStatus.Inactive;
        }

        /// <summary>
        /// 激活 Agent。仅当决策管线中至少有任一启用层时才会切换为 Active 状态。
        /// </summary>
        public void Activate()
        {
            if (Pipeline.HasAnyEnabled())
            {
                Status = AgentStatus.Active;
            }
        }

        /// <summary>
        /// 停用 Agent，清空待执行动作队列并切回 Inactive 状态。
        /// </summary>
        public void Deactivate()
        {
            Status = AgentStatus.Inactive;
            m_ActionQueue.Clear();
        }

        /// <summary>
        /// 暂停 Agent 执行（仅在 Active 状态下生效）。
        /// </summary>
        public void Pause()
        {
            if (Status == AgentStatus.Active)
            {
                Status = AgentStatus.Paused;
            }
        }

        /// <summary>
        /// 恢复 Agent 执行（仅在 Paused 状态下生效）。
        /// </summary>
        public void Resume()
        {
            if (Status == AgentStatus.Paused)
            {
                Status = AgentStatus.Active;
            }
        }

        /// <summary>
        /// 清空感知数据，每帧开始前调用。
        /// </summary>
        public void ClearPerception()
        {
            Perception.ClearFrame();
        }

        /// <summary>
        /// Agent 更新入口 — 每帧调用，执行标准感知→决策→执行循环：
        /// 1. 更新世界状态内部时间
        /// 2. 通过决策管线评估当前上下文，生成动作指令并入队
        /// 3. 从动作队列中按序执行，每帧执行数量受 <see cref="MaxActionsPerTick"/> 限制
        /// </summary>
        /// <param name="deltaTime">帧增量时间（秒）</param>
        public void Tick(float deltaTime)
        {
            if (Status != AgentStatus.Active)
            {
                return;
            }

            // 1. 更新世界状态时间
            WorldState.Tick(deltaTime);

            // 2. 决策管线评估
            var commands = Pipeline.Execute(m_Context);
            if (commands != null)
            {
                foreach (var cmd in commands)
                {
                    m_ActionQueue.Enqueue(cmd);
                }
            }

            // 3. 执行动作（每帧有限数量）
            int executed = 0;
            while (m_ActionQueue.Count > 0 && executed < MaxActionsPerTick)
            {
                var action = m_ActionQueue.Dequeue();
                action.Execute();
                // 注意: 实际执行由 Unity 层适配器处理
                executed++;
            }
        }

        /// <summary>
        /// 获取评估上下文，提供给外部需要访问知识池数据的组件。
        /// </summary>
        /// <returns>当前的策略评估上下文</returns>
        public IEvaluationContext GetContext()
        {
            return m_Context;
        }

        /// <summary>
        /// 清空待执行动作队列。
        /// </summary>
        public void ClearActionQueue()
        {
            m_ActionQueue.Clear();
        }

        /// <summary>
        /// 待执行动作数量。
        /// </summary>
        public int PendingActionCount
        {
            get
            {
                return m_ActionQueue.Count;
            }
        }

        /// <summary>
        /// 实现 <see cref="IReference.Clear"/> — 依次释放所有子组件资源，
        /// 清空动作队列，重置所有状态字段。
        /// </summary>
        public void Clear()
        {
            Blackboard?.Clear();
            WorldState?.Clear();
            Perception?.Clear();
            Pipeline?.Clear();
            Registry?.Clear();
            m_ActionQueue?.Clear();
            m_Context = null;
            Status = AgentStatus.Inactive;
            Name = null;
        }
    }
}
