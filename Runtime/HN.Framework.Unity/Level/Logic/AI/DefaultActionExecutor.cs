using System;
using System.Collections.Generic;
using UnityEngine;
using HN.Framework.Core.Level.Logic.AI;

namespace HN.Framework.Unity.Level.Logic.AI
{
    /// <summary>
    /// 默认动作执行器 — 基于注册-分发模式，项目通过 <see cref="RegisterHandler{T}"/> 注册各 Action 类型的执行逻辑。
    /// 框架不预设任何 Action 类型，所有 Action 的执行均由项目自行注册。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 为何保留自驱动的 Update()：
    /// DefaultActionExecutor 在 <c>Update()</c> 中轮询跨帧动作的完成状态
    /// （如 NavMeshAgent 是否到达目标、Animator 动画是否播放完毕），
    /// 这些操作依赖 Unity 引擎的原生组件状态轮询，属于 View 层执行器，
    /// 而非游戏逻辑层的决策 Tick。因此保留其 MonoBehaviour 自驱动模式，
    /// 与 <see cref="AIAgentComponent"/>（现已由 GameWorld.AISystem 统一驱动）明确分工。
    /// </para>
    /// <para>
    /// 使用示例：
    /// </para>
    /// <code>
    /// var executor = GetComponent&lt;DefaultActionExecutor&gt;();
    /// executor.RegisterHandler&lt;YourMoveToAction&gt;(move =>
    /// {
    ///     navAdapter.SetDestination(new Vector3(move.TargetX, move.TargetY, move.TargetZ));
    ///     return navAdapter.HasReachedDestination();
    /// });
    /// executor.RegisterHandler&lt;YourPlayAnimationAction&gt;(anim =>
    /// {
    ///     animAdapter.PlayAnimation(anim.AnimationName, anim.Speed);
    ///     return animAdapter.IsAnimationFinished();
    /// });
    /// </code>
    /// </remarks>
    public class DefaultActionExecutor : MonoBehaviour
    {
        private readonly Dictionary<Type, Func<IActionCommand, bool>> _handlers = new Dictionary<Type, Func<IActionCommand, bool>>();
        private readonly Queue<IActionCommand> _queue = new Queue<IActionCommand>();
        private IActionCommand _current;

        /// <summary>
        /// 待执行队列中的指令数。
        /// </summary>
        public int PendingCount => _queue.Count;

        /// <summary>
        /// 是否正在执行指令（当前有活跃指令或队列非空）。
        /// </summary>
        public bool IsExecuting => _current != null || _queue.Count > 0;

        /// <summary>
        /// 注册指定类型 Action 的执行处理器。处理器接收 Action 实例，返回 true 表示执行完成。
        /// 重复注册同类型会覆盖之前的处理器。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IActionCommand"/> 的具体 Action 类型。</typeparam>
        /// <param name="handler">执行处理器委托。返回 true 时该 Action 被视为执行完毕并从队列中移除。</param>
        public void RegisterHandler<T>(Func<T, bool> handler) where T : IActionCommand
        {
            if (handler == null)
            {
                return;
            }

            _handlers[typeof(T)] = cmd => handler((T)cmd);
        }

        /// <summary>
        /// 注销指定类型的处理器。注销后该类型 Action 将被跳过。
        /// </summary>
        /// <typeparam name="T">要注销的 Action 类型。</typeparam>
        public void UnregisterHandler<T>() where T : IActionCommand
        {
            _handlers.Remove(typeof(T));
        }

        /// <summary>
        /// 将指令加入执行队列。
        /// </summary>
        /// <param name="cmd">要入队的动作指令。为 null 时忽略。</param>
        public void EnqueueAction(IActionCommand cmd)
        {
            if (cmd != null)
            {
                _queue.Enqueue(cmd);
            }
        }

        /// <summary>
        /// 批量加入执行队列。
        /// </summary>
        /// <param name="commands">动作指令列表。为 null 时忽略。</param>
        public void EnqueueActions(IReadOnlyList<IActionCommand> commands)
        {
            if (commands == null)
            {
                return;
            }

            foreach (var cmd in commands)
            {
                if (cmd != null)
                {
                    _queue.Enqueue(cmd);
                }
            }
        }

        /// <summary>
        /// 清空待执行队列并取消当前正在执行的指令。
        /// </summary>
        public void ClearQueue()
        {
            _current?.Cancel();

            while (_queue.Count > 0)
            {
                _queue.Dequeue()?.Cancel();
            }

            _current = null;
        }

        /// <summary>
        /// 取消所有待执行和正在执行的指令。
        /// </summary>
        public void CancelAll()
        {
            ClearQueue();
        }

        private void Update()
        {
            if (_current == null)
            {
                if (_queue.Count == 0)
                {
                    return;
                }

                _current = _queue.Dequeue();
                _current.Execute();
            }

            bool done = ExecuteCommand(_current);

            if (done)
            {
                _current = null;
            }
        }

        /// <summary>
        /// 执行当前指令：查找已注册的处理器并调用。未注册类型的 Action 直接被标记为 Completed 并跳过。
        /// </summary>
        /// <param name="cmd">当前要执行的指令。</param>
        /// <returns>指令是否执行完毕。</returns>
        private bool ExecuteCommand(IActionCommand cmd)
        {
            Type cmdType = cmd.GetType();

            if (_handlers.TryGetValue(cmdType, out Func<IActionCommand, bool> handler))
            {
                return handler(cmd);
            }

            // 未注册处理器的类型：标记为完成并跳过
            if (cmd.Status == ActionCommandStatus.Running)
            {
                cmd.Status = ActionCommandStatus.Completed;
            }

            return true;
        }
    }
}
