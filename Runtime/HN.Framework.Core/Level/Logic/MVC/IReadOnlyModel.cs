using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic
{
    /// <summary>
    /// 只读数据模型接口，提供数据只读访问与变化通知能力。
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public interface IReadOnlyModel<T>
    {
        /// <summary>
        /// 获取当前值。
        /// </summary>
        T Value { get; }

        /// <summary>
        /// 值发生变化时触发，参数为新值。
        /// </summary>
        event Action<T> OnValueChanged;
    }

    /// <summary>
    /// 只读数据模型的默认实现。当值通过 <see cref="SetValue"/> 被修改时触发 <see cref="OnValueChanged"/> 事件。
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public sealed class ReadOnlyModel<T> : IReadOnlyModel<T>
    {
        private T m_value;

        /// <summary>
        /// 获取当前值。
        /// </summary>
        public T Value => m_value;

        /// <summary>
        /// 值发生变化时触发，参数为新值。
        /// </summary>
        public event Action<T> OnValueChanged;

        /// <summary>
        /// 初始化只读数据模型实例。
        /// </summary>
        /// <param name="initialValue">初始值，默认为 default(T)</param>
        public ReadOnlyModel(T initialValue = default)
        {
            m_value = initialValue;
        }

        /// <summary>
        /// 设置新值。仅当新值与当前值不同时更新并触发 <see cref="OnValueChanged"/> 事件。
        /// </summary>
        /// <param name="newValue">新的值</param>
        public void SetValue(T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(m_value, newValue))
            {
                m_value = newValue;
                OnValueChanged?.Invoke(m_value);
            }
        }
    }
}
