using FishNet.Object;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// 网络实体视图基类。所有需要网络同步的实体视图应继承此类。
    /// 继承 FishNet.NetworkBehaviour，通过 NetworkObject 提供网络身份。
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public abstract class NetworkEntityView : NetworkBehaviour
    {
        /// <summary>
        /// 获取此网络实体的网络 ID。
        /// </summary>
        public int NetId => NetworkObject != null ? NetworkObject.ObjectId : 0;

        /// <summary>
        /// 获取当前客户端是否拥有此网络对象的控制权。
        /// </summary>
        public bool IsOwnerValue => IsOwner;

        /// <summary>
        /// 网络实体生成时调用。子类可重写以执行初始化逻辑。
        /// </summary>
        public virtual void OnSpawned() { }

        /// <summary>
        /// 网络实体销毁时调用。子类可重写以执行清理逻辑。
        /// </summary>
        public virtual void OnDespawned() { }
    }
}
