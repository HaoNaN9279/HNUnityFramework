using System;

namespace HN.Framework
{
    public interface IHFSMTransition : IReference
    {
        /// <summary>
        /// 此转换是否激活
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// 此转换来时的状态
        /// </summary>
        public IHFSMState FromState { get; }

        /// <summary>
        /// 转换的目标状态
        /// </summary>
        public IHFSMState TargetState { get; }


        /// <summary>
        /// 转换初始化
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="targetState"></param>
        /// <param name="conditionFunc"></param>
        public void Initialize(IHFSMState fromState, IHFSMState targetState, Func<bool> conditionFunc);

        /// <summary>
        /// 计算条件方法
        /// </summary>
        /// <returns></returns>
        public bool Eval();

        /// <summary>
        /// 替换条件方法
        /// </summary>
        /// <param name="conditionFunc"></param>
        public void ChangeConditionFunc(Func<bool> conditionFunc);
    }


    public class HFSMTransition : IHFSMTransition
    {
        /// <summary>
        /// 转换初始化
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="targetState"></param>
        /// <param name="conditionFunc"></param>
        public void Initialize(IHFSMState fromState, IHFSMState targetState, Func<bool> conditionFunc)
        {
            fromState.AddOutputTransition(this);
            this.fromState = fromState;
            targetState.AddInputTransition(this);
            this.targetState = targetState;
            this.conditionFunc = conditionFunc;
        }

        /// <summary>
        /// 计算条件方法
        /// </summary>
        /// <returns></returns>
        public bool Eval() => conditionFunc == null ? true : conditionFunc.Invoke();

        /// <summary>
        /// 替换条件方法
        /// </summary>
        /// <param name="conditionFunc"></param>
        public void ChangeConditionFunc(Func<bool> newConditionFunc)
        {
            this.conditionFunc = newConditionFunc;
        }

        /// <summary>
        /// 清理转换
        /// </summary>
        public void Clear()
        {
            conditionFunc = null;
            targetState = null;
            fromState = null;
            active = true;
        }


        /// <summary>
        /// 此转换是否激活
        /// </summary>
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        /// <summary>
        /// 此转换来时的状态
        /// </summary>
        public IHFSMState FromState => fromState;

        /// <summary>
        /// 转换的目标状态
        /// </summary>
        public IHFSMState TargetState => targetState;


        /// <summary>
        /// 此转换是否激活
        /// </summary>
        protected bool active = true;

        /// <summary>
        /// 此转换来时的状态
        /// </summary>
        protected IHFSMState fromState;

        /// <summary>
        /// 转换的目标状态
        /// </summary>
        protected IHFSMState targetState;

        /// <summary>
        /// 条件方法
        /// </summary>
        protected Func<bool> conditionFunc;

    }
}
