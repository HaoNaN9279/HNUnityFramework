using HN.Framework.Unity.Level.View.Binding;
using UnityEngine;

namespace HN.Framework.Unity.Level.View
{
    /// <summary>
    /// 实体视图基类，所有可见的 View 对象都应继承此类。
    /// </summary>
    public abstract class EntityView : MonoBehaviour
    {
        /// <summary>
        /// 当前 View 使用的属性绑定器。
        /// </summary>
        protected PropertyBinder binder;

        /// <summary>
        /// 绑定数据源，将 <paramref name="binder"/> 与本 View 关联。
        /// </summary>
        /// <param name="binder">属性绑定器实例。</param>
        public virtual void BindData(PropertyBinder binder)
        {
            this.binder = binder;
        }
    }
}
