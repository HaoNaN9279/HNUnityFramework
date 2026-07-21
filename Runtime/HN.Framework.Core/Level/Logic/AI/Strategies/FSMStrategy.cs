using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;
using HN.Framework.Core.Level.Logic;

namespace HN.Framework.Core.Level.Logic.AI.Strategies
{
    /// <summary>
    /// FSM 策略 — 包装 L2 <see cref="HFSM"/>，将有限状态机作为决策管线中的可插拔策略。
    /// 每个状态可通过 <see cref="AddState(string, Func{IEvaluationContext, IActionCommand})"/> 绑定一个动作工厂，
    /// 状态进入时通过工厂生成 <see cref="IActionCommand"/> 并交由决策管线执行。
    /// </summary>
    /// <remarks>
    /// 典型使用流程:
    /// <code>
    /// var strategy = new FSMStrategy();
    /// strategy.Initialize();
    /// var idle = strategy.AddState("Idle", ctx => new IdleCommand());
    /// var patrol = strategy.AddState("Patrol", ctx => new PatrolCommand());
    /// strategy.AddTransition(idle, patrol, () => IsBored());
    /// strategy.AddTransition(patrol, idle, () => IsTargetLost());
    /// strategy.SetStartState("Idle");
    /// // 每帧: var commands = strategy.Evaluate(context);
    /// </code>
    /// </remarks>
    public class FSMStrategy : IDecisionStrategy
    {
        /// <inheritdoc/>
        public string Name => "FSM";

        /// <inheritdoc/>
        public int Priority { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 当前 FSM 是否正在运行。
        /// </summary>
        public bool IsRunning => m_FSM != null && m_FSM.IsAlive;

        /// <summary>
        /// FSM 当前活跃状态。未初始化或已停止时返回 null。
        /// </summary>
        public IHFSMState CurrentState => m_FSM?.CurrentState;

        private HFSM m_FSM;
        private Dictionary<IHFSMState, Func<IEvaluationContext, IActionCommand>> m_StateActions;

        /// <summary>
        /// 初始化策略，从引用池获取 <see cref="HFSM"/> 实例并完成内部初始化。
        /// 在添加到决策管线时自动调用一次。
        /// </summary>
        public void Initialize()
        {
            m_FSM = ReferencePool.Acquire<HFSM>();
            m_FSM.Initialize();
            m_StateActions = new Dictionary<IHFSMState, Func<IEvaluationContext, IActionCommand>>();
        }

        /// <summary>
        /// 驱动 FSM 转换并返回当前状态对应的动作指令。
        /// 每帧调用：先推进状态机处理条件转换，再根据当前状态调用绑定的动作工厂。
        /// </summary>
        /// <param name="context">评估上下文，会传递给状态绑定的动作工厂。</param>
        /// <returns>当前状态产生的动作指令列表，无可执行动作时返回空数组。</returns>
        public IReadOnlyList<IActionCommand> Evaluate(IEvaluationContext context)
        {
            if (!IsEnabled || m_FSM == null || !m_FSM.IsAlive)
            {
                return Array.Empty<IActionCommand>();
            }

            m_FSM.Update();

            IHFSMState currentState = m_FSM.CurrentState;
            if (currentState != null && m_StateActions.TryGetValue(currentState, out Func<IEvaluationContext, IActionCommand> factory))
            {
                IActionCommand command = factory(context);
                if (command != null)
                {
                    return new IActionCommand[] { command };
                }
            }

            return Array.Empty<IActionCommand>();
        }

        /// <summary>
        /// 重置策略：清空状态-动作映射并将 <see cref="HFSM"/> 实例归还引用池。
        /// 在关卡重开或 Agent 重生时调用。
        /// </summary>
        public void Reset()
        {
            m_StateActions?.Clear();

            if (m_FSM != null)
            {
                ReferencePool.Release(m_FSM);
                m_FSM = null;
            }

            m_StateActions = null;
        }

        #region 状态管理

        /// <summary>
        /// 向 FSM 添加自定义状态（不绑定动作工厂）。
        /// 适用于继承 <see cref="HFSMState"/> 并在子类中实现完整状态逻辑的场景。
        /// </summary>
        /// <typeparam name="T">状态类型，必须继承 <see cref="HFSMState"/> 并有无参构造函数。</typeparam>
        /// <param name="name">状态名称，在状态机内必须唯一。</param>
        /// <returns>新创建的状态实例。</returns>
        public T AddState<T>(string name) where T : HFSMState, new()
        {
            return m_FSM.AddState<T>(name);
        }

        /// <summary>
        /// 添加状态并绑定动作工厂。
        /// 状态进入时，<paramref name="actionFactory"/> 生成的 <see cref="IActionCommand"/> 将通过 <see cref="Evaluate"/> 返回。
        /// </summary>
        /// <param name="name">状态名称，在状态机内必须唯一。</param>
        /// <param name="actionFactory">
        /// 动作工厂委托，每次 <see cref="Evaluate"/> 调用时如果处于该状态则执行此工厂。
        /// 接收 <see cref="IEvaluationContext"/>，返回 <see cref="IActionCommand"/>，返回 null 表示无需执行动作。
        /// </param>
        /// <returns>新创建的 <see cref="HFSMState"/> 包装实例，可用于后续添加转换。</returns>
        public HFSMState AddState(string name, Func<IEvaluationContext, IActionCommand> actionFactory)
        {
            var state = m_FSM.AddState<FSMStateWrapper>(name);
            state.SetActionFactory(actionFactory);
            m_StateActions[state] = actionFactory;
            return state;
        }

        /// <summary>
        /// 按名称获取已注册的状态。
        /// </summary>
        /// <param name="name">状态名称。</param>
        /// <returns>对应的状态实例，不存在时返回 null。</returns>
        public IHFSMState GetState(string name)
        {
            if (m_FSM != null && m_FSM.States.ContainsKey(name))
            {
                return m_FSM.States[name];
            }

            return null;
        }

        /// <summary>
        /// 按名称删除状态，同时清理其关联的动作工厂和所有传入/传出转换。
        /// </summary>
        /// <param name="name">要删除的状态名称。</param>
        public void RemoveState(string name)
        {
            if (m_FSM == null)
            {
                return;
            }

            IHFSMState state = GetState(name);
            if (state != null && m_StateActions != null)
            {
                m_StateActions.Remove(state);
            }

            m_FSM.RemoveState(name);
        }

        #endregion

        #region 转换管理

        /// <summary>
        /// 添加默认类型的状态转换（使用名称查找状态）。
        /// </summary>
        /// <param name="fromName">源状态名称。</param>
        /// <param name="toName">目标状态名称。</param>
        /// <param name="condition">转换条件，返回 true 时触发转换。</param>
        /// <returns>创建的转换实例，已注册的状态对应的状态不存在则返回 null。</returns>
        public HFSMTransition AddTransition(string fromName, string toName, Func<bool> condition)
        {
            return AddTransition<HFSMTransition>(fromName, toName, condition);
        }

        /// <summary>
        /// 添加指定类型的状态转换（使用名称查找状态）。
        /// </summary>
        /// <typeparam name="T">转换类型，必须继承 <see cref="HFSMTransition"/> 并有无参构造函数。</typeparam>
        /// <param name="fromName">源状态名称。</param>
        /// <param name="toName">目标状态名称。</param>
        /// <param name="condition">转换条件，返回 true 时触发转换。</param>
        /// <returns>创建的转换实例，已注册的状态对应的状态不存在则返回 null。</returns>
        public T AddTransition<T>(string fromName, string toName, Func<bool> condition) where T : HFSMTransition, new()
        {
            IHFSMState fromState = GetState(fromName);
            IHFSMState toState = GetState(toName);

            if (fromState == null || toState == null)
            {
                return null;
            }

            return m_FSM.AddTransition<T>(fromState, toState, condition);
        }

        /// <summary>
        /// 添加默认类型的状态转换（使用状态引用）。
        /// </summary>
        /// <param name="fromState">源状态引用。</param>
        /// <param name="toState">目标状态引用。</param>
        /// <param name="condition">转换条件，返回 true 时触发转换。</param>
        /// <returns>创建的转换实例。</returns>
        public HFSMTransition AddTransition(IHFSMState fromState, IHFSMState toState, Func<bool> condition)
        {
            return m_FSM.AddTransition<HFSMTransition>(fromState, toState, condition);
        }

        /// <summary>
        /// 添加指定类型的状态转换（使用状态引用）。
        /// </summary>
        /// <typeparam name="T">转换类型，必须继承 <see cref="HFSMTransition"/> 并有无参构造函数。</typeparam>
        /// <param name="fromState">源状态引用。</param>
        /// <param name="toState">目标状态引用。</param>
        /// <param name="condition">转换条件，返回 true 时触发转换。</param>
        /// <returns>创建的转换实例。</returns>
        public T AddTransition<T>(IHFSMState fromState, IHFSMState toState, Func<bool> condition) where T : HFSMTransition, new()
        {
            return m_FSM.AddTransition<T>(fromState, toState, condition);
        }

        #endregion

        #region FSM 控制

        /// <summary>
        /// 设置起始状态并按名称启动状态机。
        /// 调用后 FSM 进入激活状态，下一次 <see cref="Evaluate"/> 将从该状态开始。
        /// </summary>
        /// <param name="name">起始状态名称。</param>
        public void SetStartState(string name)
        {
            if (m_FSM != null && m_FSM.States.ContainsKey(name))
            {
                m_FSM.Start(name);
            }
        }

        /// <summary>
        /// 设置起始状态并按引用启动状态机。
        /// </summary>
        /// <param name="state">起始状态引用。</param>
        public void SetStartState(IHFSMState state)
        {
            m_FSM?.Start(state);
        }

        /// <summary>
        /// 关闭状态机，停止所有转换评估。
        /// 关闭后 <see cref="Evaluate"/> 将返回空列表，直到再次调用 <see cref="SetStartState(string)"/>。
        /// </summary>
        public void Shutdown()
        {
            m_FSM?.Shutdown();
        }

        #endregion

        /// <summary>
        /// FSM 状态包装器 — 继承 <see cref="HFSMState"/>，额外持有动作工厂引用。
        /// 由 <see cref="FSMStrategy.AddState(string, Func{IEvaluationContext, IActionCommand})"/> 内部创建，
        /// 通过 <see cref="Evaluate"/> 调用工厂生成当前状态对应的 <see cref="IActionCommand"/>。
        /// </summary>
        private class FSMStateWrapper : HFSMState
        {
            private Func<IEvaluationContext, IActionCommand> m_ActionFactory;

            /// <summary>
            /// 设置动作工厂委托。由 <see cref="FSMStrategy.AddState(string, Func{IEvaluationContext, IActionCommand})"/> 调用。
            /// </summary>
            /// <param name="factory">动作工厂委托。</param>
            public void SetActionFactory(Func<IEvaluationContext, IActionCommand> factory)
            {
                m_ActionFactory = factory;
            }

            /// <inheritdoc/>
            public override void Clear()
            {
                base.Clear();
                m_ActionFactory = null;
            }
        }
    }
}
