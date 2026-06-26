using System.Collections.Generic;

namespace HN.Framework
{
    public interface IHFSMState : IReference
    {
        /// <summary>
        /// 状态名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 此状态是否允许转换
        /// 默认为false
        /// </summary>
        public bool IsAllowedTrans { get; }

        /// <summary>
        /// 进入到此状态的转换列表
        /// </summary>
        public IReadOnlyDictionary<KeyValuePair<IHFSMState, IHFSMState>, IHFSMTransition> InputTransitions { get; }

        /// <summary>
        /// 从此状态退出的转换列表
        /// </summary>
        public IReadOnlyDictionary<KeyValuePair<IHFSMState, IHFSMState>, IHFSMTransition> OutputTransitions { get; }


        /// <summary>
        /// 状态初始化
        /// </summary>
        /// <param name="name"></param>
        public void Initialize(string name);

        /// <summary>
        /// 添加一个进入到此状态的转换
        /// </summary>
        /// <param name="inputTransition"></param>
        public void AddInputTransition(IHFSMTransition inputTransition);

        /// <summary>
        /// 添加一个从此状态退出的转换
        /// </summary>
        /// <param name="outputTransition"></param>
        public void AddOutputTransition(IHFSMTransition outputTransition);

        /// <summary>
        /// 移除一个进入此状态的转换
        /// </summary>
        /// <param name="inputTransition"></param>
        public void RemoveInputTransition(IHFSMTransition inputTransition);

        /// <summary>
        /// 移除一个从此状态退出的转换
        /// </summary>
        /// <param name="outuputTransition"></param>
        public void RemoveOutputTransition(IHFSMTransition outuputTransition);

        /// <summary>
        /// 是否能从指定状态转换到此状态
        /// </summary>
        /// <param name="fromState"></param>
        /// <returns></returns>
        public bool CanTransFrom(IHFSMState fromState);

        /// <summary>
        /// 是否能从此状态转换到指定状态
        /// </summary>
        /// <param name="targetState"></param>
        /// <returns></returns>
        public bool CanTransTo(IHFSMState targetState);

        /// <summary>
        /// 状态创建时
        /// </summary>
        public void OnCreate();

        /// <summary>
        /// 进入状态时
        /// </summary>
        public void OnEnter();

        /// <summary>
        /// 更新状态
        /// </summary>
        public void Update();

        /// <summary>
        /// 状态退出时
        /// </summary>
        public void OnExit();

        /// <summary>
        /// 状态销毁时
        /// </summary>
        public void OnDestroy();
    }


    public class HFSMState : IHFSMState
    {
        /// <summary>
        /// 状态初始化
        /// </summary>
        /// <param name="name"></param>
        public virtual void Initialize(string name)
        {
            this.name = name;
            inputTransitions = ReferencePool.Acquire<PooledDictionary<KeyValuePair<IHFSMState, IHFSMState>, IHFSMTransition>>();
            outputTransitions = ReferencePool.Acquire<PooledDictionary<KeyValuePair<IHFSMState, IHFSMState>, IHFSMTransition>>();
        }

        /// <summary>
        /// 添加一个进入到此状态的转换
        /// </summary>
        /// <param name="inputTransition"></param>
        public void AddInputTransition(IHFSMTransition inputTransition)
        {
            if (inputTransitions != null && !inputTransitions.ContainsValue(inputTransition))
            {
                inputTransitions.Add(new KeyValuePair<IHFSMState, IHFSMState>(inputTransition.FromState, inputTransition.TargetState), inputTransition);
            }
        }

        /// <summary>
        /// 添加一个从此状态退出的转换
        /// </summary>
        /// <param name="outputTransition"></param>
        public void AddOutputTransition(IHFSMTransition outputTransition)
        {
            if (outputTransitions != null && !outputTransitions.ContainsValue(outputTransition))
            {
                outputTransitions.Add(new KeyValuePair<IHFSMState, IHFSMState>(outputTransition.FromState, outputTransition.TargetState), outputTransition);
            }
        }

        /// <summary>
        /// 移除一个进入此状态的转换
        /// </summary>
        /// <param name="inputTransition"></param>
        public void RemoveInputTransition(IHFSMTransition inputTransition)
        {
            if (inputTransitions != null && inputTransitions.ContainsValue(inputTransition))
            {
                inputTransitions.Remove(new KeyValuePair<IHFSMState, IHFSMState>(inputTransition.FromState, inputTransition.TargetState));
            }
        }

        /// <summary>
        /// 移除一个从此状态退出的转换
        /// </summary>
        /// <param name="outuputTransition"></param>
        public void RemoveOutputTransition(IHFSMTransition outputTransition)
        {
            if (outputTransitions != null && outputTransitions.ContainsValue(outputTransition))
            {
                outputTransitions.Remove(new KeyValuePair<IHFSMState, IHFSMState>(outputTransition.FromState, outputTransition.TargetState));
            }
        }

        /// <summary>
        /// 是否能从指定状态转换到此状态
        /// </summary>
        /// <param name="fromState"></param>
        /// <returns></returns>
        public bool CanTransFrom(IHFSMState fromState)
        {
            if (inputTransitions == null)
            {
                return false;
            }

            foreach (var trans in inputTransitions.Values)
            {
                if (trans.Active == true && fromState == trans.FromState)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否能从此状态转换到指定状态
        /// </summary>
        /// <param name="targetState"></param>
        /// <returns></returns>
        public bool CanTransTo(IHFSMState targetState)
        {
            if (outputTransitions == null)
            {
                return false;
            }

            foreach (var trans in outputTransitions.Values)
            {
                if (trans.Active == true && targetState == trans.TargetState)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 状态创建时
        /// </summary>
        public virtual void OnCreate() { }

        /// <summary>
        /// 进入状态时
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// 更新状态
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// 状态退出时
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// 状态销毁时
        /// </summary>
        public virtual void OnDestroy() { }

        /// <summary>
        /// 清理状态
        /// </summary>
        public void Clear()
        {
            outputTransitions.Clear();
            inputTransitions.Clear();
            name = default;
        }


        /// <summary>
        /// 状态名称
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 此状态是否允许转换
        /// 默认为false
        /// </summary>
        public bool IsAllowedTrans => isAllowedTrans;

        /// <summary>
        /// 进入到此状态的转换列表
        /// </summary>
        public IReadOnlyDictionary<KeyValuePair<IHFSMState, IHFSMState>, IHFSMTransition> InputTransitions => inputTransitions;

        /// <summary>
        /// 从此状态退出的转换列表
        /// </summary>
        public IReadOnlyDictionary<KeyValuePair<IHFSMState, IHFSMState>, IHFSMTransition> OutputTransitions => outputTransitions;


        /// <summary>
        /// 状态名称
        /// </summary>
        protected string name;

        /// <summary>
        /// 此状态是否允许转换
        /// 默认为false
        /// </summary>
        protected bool isAllowedTrans = false;

        /// <summary>
        /// 进入到此状态的转换列表
        /// </summary>
        protected PooledDictionary<KeyValuePair<IHFSMState, IHFSMState>, IHFSMTransition> inputTransitions;

        /// <summary>
        /// 从此状态退出的转换列表
        /// </summary>
        protected PooledDictionary<KeyValuePair<IHFSMState, IHFSMState>, IHFSMTransition> outputTransitions;

    }


    /// <summary>
    /// 状态机入口状态
    /// </summary>
    public class HFSMEntryState : HFSMState
    {
        public override void OnCreate()
        {
            base.OnCreate();
            isAllowedTrans = true;
        }


        /// <summary>
        /// 入口状态默认名称
        /// </summary>
        public static readonly string EntryStateName = "Entry";
    }


    /// <summary>
    /// 状态机出口状态
    /// </summary>
    public class HFSMExitState : HFSMState
    {
        /// <summary>
        /// 出口状态默认名称
        /// </summary>
        public static readonly string ExitStateName = "Exit";
    }

}
