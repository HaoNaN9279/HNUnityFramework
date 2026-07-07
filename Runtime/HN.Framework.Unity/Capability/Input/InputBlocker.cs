#nullable enable

using System.Collections.Generic;
using System.Linq;
using HN.Framework.Core.Capability.Input;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Input
{
    /// <summary>
    /// 输入屏蔽器，提供基于优先级的输入屏蔽栈。
    /// 更高优先级的屏蔽会阻挡较低优先级的输入动作。
    /// 相同优先级时，后 Push 的 token 生效。
    /// </summary>
    internal sealed class InputBlocker : IInputBlocker
    {
        private readonly Dictionary<object, int> _tokens = new Dictionary<object, int>();
        private int _maxPriority = int.MinValue;

        /// <summary>
        /// 推入一个输入屏蔽令牌。
        /// 如果同一令牌已存在，则更新其优先级。
        /// </summary>
        /// <param name="token">屏蔽令牌对象。</param>
        /// <param name="priority">屏蔽优先级，数值越大优先级越高。</param>
        public void Push(object token, int priority)
        {
            _tokens[token] = priority;

            if (priority > _maxPriority)
            {
                _maxPriority = priority;
            }
            else if (priority < _maxPriority)
            {
                // The previous max might have been removed or overridden; recalc
                _maxPriority = _tokens.Values.Max();
            }
        }

        /// <summary>
        /// 弹出指定的屏蔽令牌。若令牌不存在则为空操作。
        /// </summary>
        /// <param name="token">屏蔽令牌对象。</param>
        public void Pop(object token)
        {
            if (!_tokens.Remove(token))
                return;

            if (_tokens.Count == 0)
            {
                _maxPriority = int.MinValue;
            }
            else
            {
                _maxPriority = _tokens.Values.Max();
            }
        }

        /// <summary>
        /// 检查指定优先级的输入动作是否被屏蔽。
        /// </summary>
        /// <param name="actionPriority">输入动作的优先级。</param>
        /// <returns>如果被屏蔽则返回 true，否则返回 false。</returns>
        public bool IsBlocked(int actionPriority)
        {
            return _tokens.Count > 0 && _maxPriority >= actionPriority;
        }

        /// <summary>
        /// 清除所有屏蔽令牌。
        /// </summary>
        public void Clear()
        {
            _tokens.Clear();
            _maxPriority = int.MinValue;
        }
    }
}
