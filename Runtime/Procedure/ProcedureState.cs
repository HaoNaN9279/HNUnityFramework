using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
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
        }

        /// <summary>
        /// 清理状态
        /// </summary>
        public virtual void Clear()
        {
            name = null;
            EnterEvent = null;
            TickEvent = null;
            ExitEvent = null;
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
        public Action EnterEvent;

        /// <summary>
        /// 状态更新事件
        /// </summary>
        public Action TickEvent;

        /// <summary>
        /// 状态帧后更新事件
        /// </summary>
        public Action LateTickEvent;

        /// <summary>
        /// 状态退出事件
        /// </summary>
        public Action ExitEvent;
        #endregion

        #region 私有变量
        protected string name;
        #endregion

    }
}
