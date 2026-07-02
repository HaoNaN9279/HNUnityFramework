using System;
using System.Collections.Generic;
using System.Diagnostics;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Level.Logic
{
    public interface IHFSM : IReference
    {
        /// <summary>
        /// 状态机状态数量
        /// </summary>
        public int StateCount { get; }

        /// <summary>
        /// 是否暂停状态机
        /// </summary>
        public bool Paused { get; set; }

        /// <summary>
        /// 状态机是否激活
        /// </summary>
        public bool IsAlive { get; }

        /// <summary>
        /// 状态机入口状态
        /// </summary>
        public IHFSMState EntryState { get; }

        /// <summary>
        /// 状态机出口状态
        /// </summary>
        public IHFSMState ExitState { get; }

        /// <summary>
        /// 状态机当前状态
        /// </summary>
        public IHFSMState CurrentState { get; }

        /// <summary>
        /// 状态机状态列表
        /// </summary>
        public IReadOnlyDictionary<string, IHFSMState> States { get; }

        /// <summary>
        /// 状态机转换列表
        /// </summary>
        public IReadOnlyDictionary<KeyValuePair<IHFSMState, IHFSMState>, IHFSMTransition> Transitions { get; }


        /// <summary>
        /// 状态机初始化
        /// </summary>
        public void Initialize();

        /// <summary>
        /// 启动状态机
        /// 当前状态为入口状态
        /// </summary>
        public void Start();

        /// <summary>
        /// 启动状态机
        /// 指定当前状态
        /// </summary>
        /// <param name="stateName"></param>
        public void Start(string stateName);

        /// <summary>
        /// 启动状态机
        /// 指定当前状态
        /// </summary>
        /// <param name="startState"></param>
        public void Start(IHFSMState startState);

        /// <summary>
        /// 关闭状态机
        /// </summary>
        public void Shutdown();

        /// <summary>
        /// 给状态机添加指定名称的状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public T AddState<T>(string stateName) where T : HFSMState, new();

        /// <summary>
        /// 从状态机移除指定名称的状态
        /// </summary>
        /// <param name="stateName"></param>
        public void RemoveState(string stateName);

        /// <summary>
        /// 从状态机移除指定状态
        /// </summary>
        /// <param name="state"></param>
        public void RemoveState(IHFSMState state);

        /// <summary>
        /// 往状态机添加状态转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromState"></param>
        /// <param name="targetState"></param>
        /// <param name="conditionFunc"></param>
        /// <returns></returns>
        public T AddTransition<T>(IHFSMState fromState, IHFSMState targetState, Func<bool> conditionFunc) where T : HFSMTransition, new();

        /// <summary>
        /// 从状态机移除状态转换
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="targetState"></param>
        public void RemoveTransition(IHFSMState fromState, IHFSMState targetState);

        /// <summary>
        /// 从状态机移除指定的状态转换
        /// </summary>
        /// <param name="transition"></param>
        public void RemoveTransition(IHFSMTransition transition);

        /// <summary>
        /// 改变当前状态到指定名称的状态
        /// </summary>
        /// <param name="targetStateName"></param>
        public void ChangeState(string targetStateName);

        /// <summary>
        /// 改变当前状态到指定状态
        /// </summary>
        /// <param name="targetState"></param>
        public void ChangeState(IHFSMState targetState);
    }


    public class HFSM : IHFSM
    {
        /// <summary>
        /// 初始化状态机
        /// </summary>
        public void Initialize()
        {
            this.paused = false;
            currentState = null;
            states = ReferencePool.Acquire<PooledDictionary<string, IHFSMState>>();
            entryState = AddState<HFSMEntryState>(HFSMEntryState.EntryStateName);
            exitState = AddState<HFSMExitState>(HFSMExitState.ExitStateName);
            m_TempStates = ReferencePool.Acquire<PooledList<HFSMState>>();
        }

        /// <summary>
        /// 启动状态机
        /// 当前状态为入口状态
        /// </summary>
        public void Start()
        {
            if (entryState == null)
            {
                return;
            }

            isAlive = true;
            currentState = entryState;
        }

        /// <summary>
        /// 启动状态机
        /// 指定当前状态
        /// </summary>
        /// <param name="stateName"></param>
        public void Start(string stateName)
        {
            if (!states.ContainsKey(stateName))
            {
                return;
            }

            isAlive = true;
            currentState = states[stateName];
            currentState.OnEnter();
        }

        /// <summary>
        /// 启动状态机
        /// 指定当前状态
        /// </summary>
        /// <param name="startState"></param>
        public void Start(IHFSMState startState)
        {
            if (!states.ContainsValue(startState))
            {
                return;
            }

            isAlive = true;
            currentState = startState;
            currentState.OnEnter();
        }

        /// <summary>
        /// 关闭状态机
        /// </summary>
        public void Shutdown()
        {
            if (currentState == null)
            {
                return;
            }

            isAlive = false;
            currentState.OnExit();
            currentState = null;
        }

        /// <summary>
        /// 给状态机添加指定名称的状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public T AddState<T>(string stateName) where T : HFSMState, new()
        {
            if (states.ContainsKey(stateName))
            {
                Debug.WriteLine($"State machine {this} already contains state: {stateName}.");
                return states[stateName] as T;
            }

            T newState = ReferencePool.Acquire<T>();
            newState.Initialize(stateName);
            states.Add(stateName, newState);
            newState.OnCreate();
            return newState;
        }

        /// <summary>
        /// 从状态机移除指定名称的状态
        /// </summary>
        /// <param name="stateName"></param>
        public void RemoveState(string stateName)
        {
            if (!states.ContainsKey(stateName))
            {
                Debug.WriteLine($"State machine {this} does not contain state: {stateName}.");
                return;
            }

            IHFSMState state = states[stateName];
            state.OnDestroy();
            var inputTransitions = state.InputTransitions;
            var outputTransitions = state.OutputTransitions;
            states.Remove(stateName);
            foreach (var transKey in inputTransitions.Keys)
            {
                if (transitions.ContainsKey(transKey))
                {
                    var trans = transitions[transKey];
                    transitions.Remove(transKey);
                    ReferencePool.Release(trans);
                }
            }
            foreach (var transKey in outputTransitions.Keys)
            {
                if (transitions.ContainsKey(transKey))
                {
                    var trans = transitions[transKey];
                    transitions.Remove(transKey);
                    ReferencePool.Release(trans);
                }
            }
            ReferencePool.Release(state);
        }

        /// <summary>
        /// 从状态机移除指定状态
        /// </summary>
        /// <param name="state"></param>
        public void RemoveState(IHFSMState state)
        {
            if (!states.ContainsValue(state))
            {
                Debug.WriteLine($"State machine {this} does not contain state: {state.Name}.");
                return;
            }

            state.OnDestroy();
            var inputTransitions = state.InputTransitions;
            var outputTransitions = state.OutputTransitions;
            states.Remove(state.Name);
            foreach (var transKey in inputTransitions.Keys)
            {
                if (transitions.ContainsKey(transKey))
                {
                    var trans = transitions[transKey];
                    transitions.Remove(transKey);
                    ReferencePool.Release(trans);
                }
            }
            foreach (var transKey in outputTransitions.Keys)
            {
                if (transitions.ContainsKey(transKey))
                {
                    var trans = transitions[transKey];
                    transitions.Remove(transKey);
                    ReferencePool.Release(trans);
                }
            }
            ReferencePool.Release(state);
        }

        /// <summary>
        /// 往状态机添加状态转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromState"></param>
        /// <param name="targetState"></param>
        /// <param name="conditionFunc"></param>
        /// <returns></returns>
        public T AddTransition<T>(IHFSMState fromState, IHFSMState targetState, Func<bool> conditionFunc) where T : HFSMTransition, new()
        {
            var key = new KeyValuePair<IHFSMState, IHFSMState>(fromState, targetState);
            if (transitions.ContainsKey(key))
            {
                transitions[key].ChangeConditionFunc(conditionFunc);
                return transitions[key] as T;
            }
            else
            {
                T newTransition = ReferencePool.Acquire<T>();
                newTransition.Initialize(fromState, targetState, conditionFunc);
                transitions.Add(key, newTransition);
                return newTransition;
            }
        }

        /// <summary>
        /// 从状态机移除状态转换
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="targetState"></param>
        public void RemoveTransition(IHFSMState fromState, IHFSMState targetState)
        {
            var key = new KeyValuePair<IHFSMState, IHFSMState>(fromState, targetState);
            if (!transitions.ContainsKey(key))
            {
                Debug.WriteLine($"State machine {this} does not contain transition: {fromState.Name} -> {targetState.Name}.");
                return;
            }

            var transition = transitions[key];
            fromState.RemoveOutputTransition(transition);
            targetState.RemoveInputTransition(transition);
            transitions.Remove(key);
            ReferencePool.Release(transition);
        }

        /// <summary>
        /// 从状态机移除指定的状态转换
        /// </summary>
        /// <param name="transition"></param>
        public void RemoveTransition(IHFSMTransition transition)
        {
            var key = new KeyValuePair<IHFSMState, IHFSMState>(transition.FromState, transition.TargetState);
            if (!transitions.ContainsKey(key))
            {
                Debug.WriteLine($"State machine {this} does not contain transition: {transition.FromState.Name} -> {transition.TargetState.Name}.");
                return;
            }

            transitions.Remove(key);
            ReferencePool.Release(transition);
        }

        /// <summary>
        /// 改变当前状态到指定名称的状态
        /// </summary>
        /// <param name="targetStateName"></param>
        public void ChangeState(string targetStateName)
        {
            if (!states.ContainsKey(targetStateName))
            {
                Debug.WriteLine($"State machine {this} does not contain state {targetStateName}.");
                return;
            }

            if (currentState != null)
            {
                currentState.OnExit();
            }
            currentState = states[targetStateName];
            currentState.OnEnter();
        }

        /// <summary>
        /// 改变当前状态到指定状态
        /// </summary>
        /// <param name="targetState"></param>
        public void ChangeState(IHFSMState targetState)
        {
            if (!states.ContainsKey(targetState.Name))
            {
                Debug.WriteLine($"State machine {this} does not contain state {targetState.Name}.");
                return;
            }

            if (currentState != null)
            {
                currentState.OnExit();
            }
            currentState = targetState;
            currentState.OnEnter();
        }

        /// <summary>
        /// 更新状态机
        /// </summary>
        public void Update()
        {
            if (!isAlive)
            {
                Debug.WriteLine($"State machine {this} is not alive.");
                return;
            }

            if (paused)
            {
                Debug.WriteLine($"State machine {this} is paused.");
                return;
            }

            m_TempStates.Clear();
            while (currentState != null && currentState.IsAllowedTrans)
            {
                m_TempStates.Add(currentState as HFSMState);
                foreach (var transition in currentState.OutputTransitions.Values)
                {
                    if (transition.Active && transition.Eval())
                    {
                        ChangeState(transition.TargetState);
                        currentState.Update();
                        break;
                    }
                }
                if (m_TempStates.Contains(currentState as HFSMState))
                {
                    throw new InvalidOperationException($"State machine {this} has death loop.");
                }
            }

            if (currentState == exitState)
            {
                Shutdown();
            }

        }

        /// <summary>
        /// 清除状态机
        /// </summary>
        public void Clear()
        {
            var transitionsValues = transitions.Values;
            transitions.Clear();
            foreach (var trans in transitionsValues)
            {
                ReferencePool.Release(trans);
            }

            currentState = null;

            var statesValues = states.Values;
            states.Clear();
            foreach (var state in statesValues)
            {
                ReferencePool.Release(state);
            }

            paused = false;
        }


        /// <summary>
        /// 状态机转台数量
        /// </summary>
        public int StateCount => states == null ? 0 : states.Count;

        /// <summary>
        /// 是否暂停状态机
        /// </summary>
        public bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }

        /// <summary>
        /// 状态机是否激活
        /// </summary>
        public bool IsAlive => isAlive;

        /// <summary>
        /// 状态机入口状态
        /// </summary>
        public IHFSMState EntryState => entryState;

        /// <summary>
        /// 状态机出口状态
        /// </summary>
        public IHFSMState ExitState => exitState;

        /// <summary>
        /// 状态机当前状态
        /// </summary>
        public IHFSMState CurrentState => currentState;

        /// <summary>
        /// 状态机状态列表
        /// </summary>
        public IReadOnlyDictionary<string, IHFSMState> States => states;

        /// <summary>
        /// 状态机转换列表
        /// </summary>
        public IReadOnlyDictionary<KeyValuePair<IHFSMState, IHFSMState>, IHFSMTransition> Transitions => transitions;


        /// <summary>
        /// 是否暂停状态机
        /// </summary>
        protected bool paused = false;

        /// <summary>
        /// 状态机是否激活
        /// </summary>
        protected bool isAlive = false;

        /// <summary>
        /// 状态机入口状态
        /// </summary>
        protected IHFSMState entryState;

        /// <summary>
        /// 状态机出口状态
        /// </summary>
        protected IHFSMState exitState;

        /// <summary>
        /// 状态机当前状态
        /// </summary>
        protected IHFSMState currentState;

        /// <summary>
        /// 状态机状态列表
        /// </summary>
        protected PooledDictionary<string, IHFSMState> states;

        /// <summary>
        /// 状态机转换列表
        /// </summary>
        protected PooledDictionary<KeyValuePair<IHFSMState, IHFSMState>, IHFSMTransition> transitions;

        /// <summary>
        /// 存储本次更新执行过的状态，用于检测死循环
        /// </summary>
        private PooledList<HFSMState> m_TempStates;
    }
}
