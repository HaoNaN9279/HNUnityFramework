using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    #region 状态机状态接口
    /// <summary>
    /// 状态机状态接口
    /// </summary>
    public interface IFSMState : IReference
    {
        /// <summary>
        /// 状态名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 初始化状态
        /// </summary>
        /// <param name="name"></param>
        public void Initialize(string name);

        /// <summary>
        /// 判断当前状态是否可以转换到目标状态
        /// </summary>
        /// <param name="targetState"></param>
        /// <returns></returns>
        public bool CanTransTo(FSMState targetState);

        /// <summary>
        /// 添加状态转换
        /// </summary>
        /// <param name="targetState"></param>
        /// <param name="condition"></param>
        public void AddTransition(FSMState targetState, Func<bool> condition);

        /// <summary>
        /// 设置状态转换条件
        /// </summary>
        /// <param name="targetState"></param>
        /// <param name="condition"></param>
        public void SetTransitionCondition(FSMState targetState, Func<bool> condition);

        /// <summary>
        /// 添加状态或设置状态转换
        /// </summary>
        /// <param name="targetState"></param>
        /// <param name="condition"></param>
        public void AddOrSetTransition(FSMState targetState, Func<bool> condition);

        /// <summary>
        /// 移除状态转换
        /// </summary>
        /// <param name="targetState"></param>
        public void RemoveTransition(FSMState targetState);
    }
    #endregion

    /// <summary>
    /// 状态机状态
    /// </summary>
    public class FSMState : IFSMState
    {
        #region 对外函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public FSMState()
        {
        }

        /// <summary>
        /// 初始化状态
        /// </summary>
        /// <param name="name"></param>
        public virtual void Initialize(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// 判断当前状态是否可以转换到目标状态
        /// </summary>
        /// <param name="targetState"></param>
        /// <returns></returns>
        public bool CanTransTo(FSMState targetState)
        {
            return fsmTransitions.ContainsKey(targetState);
        }

        /// <summary>
        /// 添加状态转换
        /// </summary>
        /// <param name="targetState"></param>
        /// <param name="condition"></param>
        public void AddTransition(FSMState targetState, Func<bool> condition)
        {
            if (fsmTransitions.ContainsKey(targetState))
            {
                Debug.LogError($"State {this} already contains transition target {targetState}.");
            }

            fsmTransitions.Add(targetState, condition);
        }

        /// <summary>
        /// 设置状态转换条件
        /// </summary>
        /// <param name="targetState"></param>
        /// <param name="condition"></param>
        public void SetTransitionCondition(FSMState targetState, Func<bool> condition)
        {
            if (!fsmTransitions.ContainsKey(targetState))
            {
                Debug.LogError($"State {this} does not contains transition target {targetState}.");
            }

            fsmTransitions[targetState] = condition;
        }

        /// <summary>
        /// 添加状态或设置状态转换
        /// </summary>
        /// <param name="targetState"></param>
        /// <param name="condition"></param>
        public void AddOrSetTransition(FSMState targetState, Func<bool> condition)
        {
            if (fsmTransitions.ContainsKey(targetState))
            {
                fsmTransitions[targetState] = condition;
            }
            else
            {
                fsmTransitions.Add(targetState, condition);
            }
        }

        /// <summary>
        /// 移除状态转换
        /// </summary>
        /// <param name="targetState"></param>
        public void RemoveTransition(FSMState targetState)
        {
            if (!fsmTransitions.ContainsKey(targetState))
            {
                Debug.LogError($"State {this} does not contains transition target {targetState}.");
            }

            fsmTransitions.Remove(targetState);
        }

        /// <summary>
        /// 清理状态
        /// </summary>
        public void Clear()
        {
            fsmTransitions.Clear();
            name = null;
            CreateEvent = null;
            EnterEvent = null;
            UpdateEvent = null;
            ExitEvent = null;
            DestroyEvent = null;
        }
        #endregion

        #region 对外属性
        /// <summary>
        /// 状态转换字典
        /// 键为目标状态，值为转换条件
        /// </summary>
        public IReadOnlyDictionary<FSMState, Func<bool>> FSMTransitions => fsmTransitions;

        /// <summary>
        /// 可转换的状态数量
        /// </summary>
        public int TransitionCount => fsmTransitions.Count;

        /// <summary>
        /// 状态名称
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 创建状态事件
        /// </summary>
        public Action CreateEvent;

        /// <summary>
        /// 进入状态事件
        /// </summary>
        public Action EnterEvent;

        /// <summary>
        /// 更新状态事件
        /// </summary>
        public Action UpdateEvent;

        /// <summary>
        /// 退出状态事件
        /// </summary>
        public Action ExitEvent;

        /// <summary>
        /// 销毁状态事件
        /// </summary>
        public Action DestroyEvent;
        #endregion

        #region 内部变量
        /// <summary>
        /// 状态转换字典
        /// </summary>
        protected Dictionary<FSMState, Func<bool>> fsmTransitions = new Dictionary<FSMState, Func<bool>>();

        /// <summary>
        /// 状态名称
        /// </summary>
        protected string name;
        #endregion
    }
}
