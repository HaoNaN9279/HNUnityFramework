using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using HN.Framework.Core.Capability.Network;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// 网络实体视图基类。所有需要网络同步的实体视图应继承此类。
    /// 继承 FishNet.NetworkBehaviour，通过 NetworkObject 提供网络身份。
    /// 提供 <see cref="SyncedModel{T}"/> 注册与生命周期管理，以及 <see cref="ApplySyncValue{T}"/>
    /// 辅助方法用于在 SyncVar 变更回调中更新同步模型。
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public abstract class NetworkEntityView : NetworkBehaviour
    {
        /// <summary>
        /// 已注册的同步模型列表，用于在实体销毁时自动清理。
        /// </summary>
        private readonly List<object> m_registeredSyncModels = new List<object>();

        /// <summary>
        /// 与 Core 层关联的 EntityId，通过 FishNet SyncVar 自动同步到所有客户端。
        /// 服务端在 Spawn 前由 <see cref="SetEntityId"/> 写入，客户端在 OnSpawned 时读取。
        /// </summary>
        [SerializeField]
        private readonly SyncVar<uint> m_syncedEntityId = new();

        /// <summary>
        /// 获取此网络实体的网络 ID。
        /// </summary>
        public int NetId => NetworkObject != null ? NetworkObject.ObjectId : 0;

        /// <summary>
        /// 获取与此网络视图关联的 Core 层 EntityId。
        /// 在服务端 Spawn 前通过 <see cref="SetEntityId"/> 设置，通过 FishNet SyncVar 同步到客户端。
        /// </summary>
        public uint SyncedEntityId => m_syncedEntityId.Value;

        /// <summary>
        /// 获取当前客户端是否拥有此网络对象的控制权。
        /// </summary>
        public bool IsOwnerValue => IsOwner;

        /// <summary>
        /// 获取当前客户端是否拥有此网络实体的控制权（语义别名）。
        /// </summary>
        public bool IsOwnedByMe => IsOwner;

        /// <summary>
        /// 获取此实体是否由服务器拥有（无客户端所有者）。
        /// 仅在服务端有意义。
        /// </summary>
        public bool IsOwnedByServer => IsServerStarted && NetworkObject != null && NetworkObject.OwnerId == -1;

        /// <summary>
        /// 服务端设置与此网络视图关联的 Core 层 EntityId。
        /// 必须在调用 FishNet ServerManager.Spawn() 之前调用。
        /// </summary>
        /// <param name="id">Core 层 Entity 的 EntityId。</param>
        internal void SetEntityId(uint id)
        {
            m_syncedEntityId.Value = id;
        }

        /// <summary>
        /// 网络实体生成时调用。子类可重写以执行初始化逻辑。
        /// </summary>
        public virtual void OnSpawned()
        {
            // 子类可在此访问 SyncedEntityId 获取 Core 层 EntityId
        }

        /// <summary>
        /// 网络实体销毁时调用。子类可重写以执行清理逻辑。
        /// 重写此方法时请调用 base.OnDespawned() 以确保已注册的同步模型被清理。
        /// </summary>
        public virtual void OnDespawned()
        {
            m_registeredSyncModels.Clear();
        }

        /// <summary>
        /// 注册 <see cref="SyncedModel{T}"/> 用于生命周期追踪。
        /// 当网络实体销毁时（<see cref="OnDespawned"/>），自动清理所有已注册的同步模型。
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="model">要注册的同步模型实例</param>
        protected void RegisterSyncedModel<T>(SyncedModel<T> model)
        {
            if (model != null && !m_registeredSyncModels.Contains(model))
            {
                m_registeredSyncModels.Add(model);
            }
        }

        /// <summary>
        /// 注销指定的 <see cref="SyncedModel{T}"/>，解除生命周期追踪。
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="model">要注销的同步模型实例</param>
        protected void UnregisterSyncedModel<T>(SyncedModel<T> model)
        {
            if (model != null)
            {
                m_registeredSyncModels.Remove(model);
            }
        }

        /// <summary>
        /// 从 SyncVar 变更回调中应用同步值到 <see cref="SyncedModel{T}"/>。
        /// 此方法在设计上可安全地在 <c>SyncVar&lt;T&gt;.OnChange</c> 回调中调用，
        /// 在 Host 模式下由于 <see cref="SyncedModel{T}.SetValue"/> 内部的值相等检查，
        /// 重复更新不会触发额外事件。
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="model">要更新的同步模型</param>
        /// <param name="newValue">来自 SyncVar 的新值</param>
        /// <param name="asServer">回调上下文标记，true 表示服务端上下文，false 表示客户端上下文</param>
        public static void ApplySyncValue<T>(SyncedModel<T> model, T newValue, bool asServer)
        {
            if (model != null)
            {
                model.SetValue(newValue);
            }
        }
    }
}
