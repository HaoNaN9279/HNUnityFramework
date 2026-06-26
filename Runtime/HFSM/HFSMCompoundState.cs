

namespace HN.Framework
{
    public interface IHFSMCompoundState : IHFSMState
    {
        /// <summary>
        /// 此状态内的子状态机
        /// </summary>
        public HFSM SubStateMachine { get; }
    }


    /// <summary>
    /// 复合状态
    /// 包含一个子状态机
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HFSMCompoundState<T> : HFSMState, IHFSMCompoundState where T : HFSM, new()
    {
        /// <summary>
        /// 此状态内的子状态机
        /// </summary>
        public HFSM SubStateMachine => subStateMachine;


        /// <summary>
        /// 状态初始化
        /// </summary>
        /// <param name="name"></param>
        public override void Initialize(string name)
        {
            base.Initialize(name);
            subStateMachine = ReferencePool.Acquire<T>();
            subStateMachine.Initialize();
        }

        /// <summary>
        /// 进入状态时
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            isAllowedTrans = false;
            subStateMachine.Start();
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        public override void Update()
        {
            base.Update();
            subStateMachine.Update();
            if (!subStateMachine.IsAlive)
            {
                isAllowedTrans = true;
            }
        }


        /// <summary>
        /// 此状态内的子状态机
        /// </summary>
        protected HFSM subStateMachine;
    }
}
