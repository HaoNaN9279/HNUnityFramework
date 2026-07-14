using System;
using System.Collections.Generic;
using HN.Framework.Core.Level.Logic;

namespace HN.Framework.Core.Capability.Network
{
    /// <summary>
    /// 同步数据模型，实现 <see cref="IReadOnlyModel{T}"/> 接口。
    /// 支持服务端权威写入（<see cref="SetValue"/>）、客户端只读访问（<see cref="Value"/>）、
    /// 值变更通知（<see cref="OnValueChanged"/>）和脏标记追踪（<see cref="IsDirty"/>）。
    /// 线程安全，适用于网络回调解发与主线程读取的并发场景。
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public sealed class SyncedModel<T> : IReadOnlyModel<T>
    {
        private T m_value;
        private bool m_isDirty;
        private readonly object m_lock = new();

        /// <summary>
        /// 获取当前值。
        /// </summary>
        public T Value
        {
            get
            {
                lock (m_lock)
                {
                    return m_value;
                }
            }
        }

        /// <summary>
        /// 获取值是否自上次 <see cref="ClearDirty"/> 调用后发生过变更。
        /// </summary>
        public bool IsDirty
        {
            get
            {
                lock (m_lock)
                {
                    return m_isDirty;
                }
            }
        }

        /// <summary>
        /// 值发生变化时触发，参数为新值。
        /// </summary>
        public event Action<T> OnValueChanged;

        /// <summary>
        /// 初始化同步数据模型实例。
        /// </summary>
        /// <param name="initialValue">初始值，默认为 default(T)</param>
        public SyncedModel(T initialValue = default)
        {
            m_value = initialValue;
        }

        /// <summary>
        /// 设置新值。仅当新值与当前值不同时更新并触发 <see cref="OnValueChanged"/> 事件。
        /// 设置后 <see cref="IsDirty"/> 标记为 true。
        /// </summary>
        /// <param name="newValue">新的值</param>
        public void SetValue(T newValue)
        {
            T currentValue;

            lock (m_lock)
            {
                if (EqualityComparer<T>.Default.Equals(m_value, newValue))
                {
                    return;
                }

                currentValue = newValue;
                m_value = newValue;
                m_isDirty = true;
            }

            // 在锁外触发事件，避免潜在的死锁
            OnValueChanged?.Invoke(currentValue);
        }

        /// <summary>
        /// 重置脏标记为 false。通常在每帧同步数据消费完毕后调用。
        /// </summary>
        public void ClearDirty()
        {
            lock (m_lock)
            {
                m_isDirty = false;
            }
        }

        /// <summary>
        /// 重置模型到指定默认值。清除脏标记并触发 <see cref="OnValueChanged"/> 事件。
        /// </summary>
        /// <param name="defaultValue">默认值，默认为 default(T)</param>
        public void Reset(T defaultValue = default)
        {
            lock (m_lock)
            {
                m_value = defaultValue;
                m_isDirty = false;
            }

            OnValueChanged?.Invoke(defaultValue);
        }
    }
}
