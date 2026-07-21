#nullable enable

using System;

namespace HN.Framework.Core.Capability.Event
{
    /// <summary>
    /// 事件总线接口，提供事件的订阅、取消订阅与发布能力
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 订阅指定类型的事件
        /// </summary>
        /// <param name="handler">事件处理委托</param>
        /// <typeparam name="T">事件数据类型</typeparam>
        void Subscribe<T>(Action<T> handler);

        /// <summary>
        /// 取消订阅指定类型的事件
        /// </summary>
        /// <param name="handler">事件处理委托</param>
        /// <typeparam name="T">事件数据类型</typeparam>
        void Unsubscribe<T>(Action<T> handler);

        /// <summary>
        /// 发布指定类型的事件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <typeparam name="T">事件数据类型</typeparam>
        void Publish<T>(T eventData);

        /// <summary>
        /// 检查指定类型的事件是否有订阅者
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <returns>如果有订阅者则为 true，否则为 false</returns>
        bool HasSubscribers<T>();
    }
}
