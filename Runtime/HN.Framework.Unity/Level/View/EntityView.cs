using HN.Framework.Unity.Level.View.Binding;
using UnityEngine;

namespace HN.Framework.Unity.Level.View
{
    /// <summary>
    /// 实体视图基类。所有可见的 View 对象应从此类派生，
    /// 通过重写 <see cref="OnSpawned"/> 和 <see cref="OnDespawned"/> 实现自定义生命周期行为。
    /// </summary>
    public class EntityView : MonoBehaviour
    {
        /// <summary>
        /// 当前 View 使用的属性绑定器。
        /// </summary>
        protected PropertyBinder binder;

        /// <summary>
        /// 获取实体 ID。
        /// </summary>
        public uint EntityId { get; private set; }

        /// <summary>
        /// 获取实体配置表 ID。
        /// </summary>
        public int EntityDefId { get; private set; }

        /// <summary>
        /// 获取当前使用的属性绑定器实例。
        /// </summary>
        protected PropertyBinder Binder => binder;

        /// <summary>
        /// 绑定数据源，将 <paramref name="binderInstance"/> 与本 View 关联。
        /// </summary>
        /// <param name="binderInstance">属性绑定器实例。</param>
        public virtual void BindData(PropertyBinder binderInstance)
        {
            binder = binderInstance;
        }

        /// <summary>
        /// 在视图被生成时调用。此时 <see cref="EntityId"/>、<see cref="EntityDefId"/> 和 <see cref="binder"/> 已设置完毕。
        /// 子类可重写此方法以执行自定义初始化逻辑。
        /// </summary>
        protected virtual void OnSpawned()
        {
        }

        /// <summary>
        /// 在视图被回收时调用。子类可重写此方法以执行自定义清理逻辑。
        /// </summary>
        protected virtual void OnDespawned()
        {
        }

        /// <summary>
        /// 初始化视图。仅供 ViewFactory 调用。
        /// </summary>
        internal void Initialize(uint entityId, int entityDefId)
        {
            EntityId = entityId;
            EntityDefId = entityDefId;
            if (binder == null)
            {
                binder = new DefaultPropertyBinder();
            }
            OnSpawned();
        }

        /// <summary>
        /// 反初始化视图。仅供 ViewFactory 调用。
        /// </summary>
        internal void Deinitialize()
        {
            OnDespawned();
            if (binder != null)
            {
                binder.UnbindAll();
            }
            EntityId = 0;
            EntityDefId = 0;
        }
    }
}
