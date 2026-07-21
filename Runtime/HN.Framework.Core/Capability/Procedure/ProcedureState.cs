using System;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Capability
{
    #region Procedure子流程接口
    /// <summary>
    /// Procedure子流程接口，状态切换时框架自动取消所有已注册的子流程
    /// </summary>
    public interface IProcedureSubProcess : IReference
    {
        /// <summary>
        /// 取消子流程
        /// </summary>
        void Cancel();
    }
    #endregion

    #region Procedure状态接口
    /// <summary>
    /// 状态接口
    /// </summary>
    public interface IProcedureState : IReference
    {
        /// <summary>
        /// 状态名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 初始化状态
        /// </summary>
        /// <param name="name"></param>
        void Initialize(string name);
    }
    #endregion

    public class ProcedureState : IProcedureState
    {
        #region 实现接口 IProcedureState
        /// <summary>
        /// 初始化状态
        /// </summary>
        /// <param name="name"></param>
        public virtual void Initialize(string name)
        {
            this.name = name;
            subProcesses = ReferencePool.Acquire<PooledList<IProcedureSubProcess>>();
        }

        /// <summary>
        /// 清理状态
        /// </summary>
        public virtual void Clear()
        {
            name = null;
            if (subProcesses != null)
            {
                foreach (var sp in subProcesses)
                {
                    sp.Cancel();
                    ReferencePool.Release(sp);
                }
                ReferencePool.Release(subProcesses);
                subProcesses = null;
            }
            EnterEvent = null;
            TickEvent = null;
            LateTickEvent = null;
            ExitEvent = null;
        }

        internal void InvokeEnterEvent() => EnterEvent?.Invoke();
        internal void InvokeTickEvent() => TickEvent?.Invoke();
        internal void InvokeLateTickEvent() => LateTickEvent?.Invoke();
        internal void InvokeExitEvent() => ExitEvent?.Invoke();

        /// <summary>
        /// 注册子流程
        /// </summary>
        /// <param name="subProcess">子流程实例</param>
        public void RegisterSubProcess(IProcedureSubProcess subProcess)
        {
            if (subProcess == null)
            {
                throw new ArgumentNullException(nameof(subProcess));
            }

            subProcesses.Add(subProcess);
        }

        /// <summary>
        /// 取消注册子流程（不自动 Cancel，调用方自行管理子流程生命周期）
        /// </summary>
        /// <param name="subProcess">子流程实例</param>
        public void UnregisterSubProcess(IProcedureSubProcess subProcess)
        {
            if (subProcess == null)
            {
                throw new ArgumentNullException(nameof(subProcess));
            }

            subProcesses.Remove(subProcess);
        }

        /// <summary>
        /// 取消并释放所有已注册的子流程（由 ProcedureManager 在状态切换时调用）
        /// </summary>
        internal void CancelAllSubProcesses()
        {
            if (subProcesses == null) return;

            foreach (var sp in subProcesses)
            {
                sp.Cancel();
                ReferencePool.Release(sp);
            }
            subProcesses.Clear();
        }
        #endregion

        #region 对外属性
        /// <summary>
        /// 状态名称
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 状态进入事件
        /// </summary>
        public event Action EnterEvent;

        /// <summary>
        /// 状态更新事件
        /// </summary>
        public event Action TickEvent;

        /// <summary>
        /// 状态帧后更新事件
        /// </summary>
        public event Action LateTickEvent;

        /// <summary>
        /// 状态退出事件
        /// </summary>
        public event Action ExitEvent;
        #endregion

        #region 私有变量
        protected string name;

        /// <summary>
        /// 已注册的子流程列表
        /// </summary>
        private PooledList<IProcedureSubProcess> subProcesses;
        #endregion

    }
}
