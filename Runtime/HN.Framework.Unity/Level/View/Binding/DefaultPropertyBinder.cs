using System;
using System.Collections.Generic;
using HN.Framework.Core.Level.Logic;

namespace HN.Framework.Unity.Level.View.Binding
{
    /// <summary>
    /// <see cref="PropertyBinder"/> 的默认实现。
    /// 通过 Dictionary 存储 IReadOnlyModel 与取消订阅的回调的映射关系，
    /// 支持泛型 Bind 和批量 UnbindAll 操作。
    /// </summary>
    public sealed class DefaultPropertyBinder : PropertyBinder
    {
        private readonly Dictionary<object, Action> m_bindings = new Dictionary<object, Action>();

        /// <summary>
        /// 绑定数据源，当 <typeparamref name="T"/> 类型的数据发生变化时调用 <paramref name="onValueChanged"/> 回调。
        /// 立即执行一次 onValueChanged 以初始化视图。
        /// </summary>
        public override void Bind<T>(IReadOnlyModel<T> source, Action<T> onValueChanged)
        {
            source.OnValueChanged += onValueChanged;
            m_bindings[source] = () => source.OnValueChanged -= onValueChanged;
            onValueChanged(source.Value);
        }

        /// <summary>
        /// 取消所有绑定，释放数据源订阅。
        /// </summary>
        public override void UnbindAll()
        {
            foreach (var kvp in m_bindings)
            {
                kvp.Value();
            }
            m_bindings.Clear();
        }
    }
}
