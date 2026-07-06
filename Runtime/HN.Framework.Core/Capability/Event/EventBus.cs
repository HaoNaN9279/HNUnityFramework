#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HN.Framework.Core.Capability.Event
{
    /// <summary>
    /// 事件总线，提供事件的订阅、取消订阅与发布能力。
    /// 线程安全，支持多线程并发订阅/取消订阅/发布。
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();
        private readonly object _lock = new();

        /// <summary>
        /// 订阅指定类型的事件。
        /// </summary>
        /// <param name="handler">事件处理委托</param>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <exception cref="ArgumentNullException">handler 为 null 时抛出</exception>
        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var list))
                {
                    list = new List<Delegate>();
                    _handlers[typeof(T)] = list;
                }

                list.Add(handler);
            }
        }

        /// <summary>
        /// 取消订阅指定类型的事件。
        /// </summary>
        /// <param name="handler">事件处理委托</param>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <exception cref="ArgumentNullException">handler 为 null 时抛出</exception>
        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                if (_handlers.TryGetValue(typeof(T), out var list))
                {
                    list.Remove(handler);
                }
            }
        }

        /// <summary>
        /// 发布指定类型的事件，通知所有订阅者。
        /// 某个 handler 抛出异常不会阻止其他 handler 的执行。
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <typeparam name="T">事件数据类型</typeparam>
        public void Publish<T>(T eventData)
        {
            List<Delegate>? snapshot;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var list))
                {
                    return;
                }

                snapshot = new List<Delegate>(list);
            }

            foreach (var handler in snapshot)
            {
                try
                {
                    var action = (Action<T>)handler;
                    action.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// 检查指定类型的事件是否有订阅者。
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <returns>如果有订阅者则为 true，否则为 false</returns>
        public bool HasSubscribers<T>()
        {
            lock (_lock)
            {
                return _handlers.TryGetValue(typeof(T), out var list) && list.Count > 0;
            }
        }
    }
}
