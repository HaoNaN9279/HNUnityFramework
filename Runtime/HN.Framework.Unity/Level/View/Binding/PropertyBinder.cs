using System;
using HN.Framework.Core.Level.Logic;

namespace HN.Framework.Unity.Level.View.Binding
{
    /// <summary>
    /// 属性绑定器基类，负责将 Model 数据绑定到 View 上的 UI 组件。
    /// </summary>
    public abstract class PropertyBinder
    {
        /// <summary>
        /// 绑定数据源，当 <typeparamref name="T"/> 类型的数据发生变化时调用 <paramref name="onValueChanged"/> 回调。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="source">数据源，实现了 <see cref="IReadOnlyModel{T}"/> 接口。</param>
        /// <param name="onValueChanged">数据变化时的回调委托。</param>
        public abstract void Bind<T>(IReadOnlyModel<T> source, Action<T> onValueChanged);

        /// <summary>
        /// 解除所有绑定，释放数据源订阅。
        /// </summary>
        public abstract void UnbindAll();
    }
}
