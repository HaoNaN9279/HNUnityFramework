using System;
using FishNet.Component.ColliderRollback;
using FishNet.Managing.Timing;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Network.Prediction
{
    /// <summary>
    /// FishNet ColliderRollback（延时补偿）框架适配器。
    /// 封装 <see cref="RollbackManager"/> 的常用回滚操作，提供便利 API。
    /// 使用方无需直接操作 FishNet 回滚系统。
    /// 首版仅支持 3D 物理回滚。
    /// </summary>
    public class LagCompensationAdapter : IDisposable
    {
        private readonly RollbackManager m_rollbackManager;
        private readonly TimeManager m_timeManager;

        /// <summary>
        /// 初始化 LagCompensationAdapter。
        /// </summary>
        /// <param name="rollbackManager">FishNet RollbackManager 实例</param>
        /// <param name="timeManager">FishNet TimeManager 实例（用于 PreciseTick 计算）</param>
        /// <exception cref="ArgumentNullException">当 rollbackManager 或 timeManager 为 null 时抛出</exception>
        public LagCompensationAdapter(RollbackManager rollbackManager, TimeManager timeManager)
        {
            m_rollbackManager = rollbackManager ?? throw new ArgumentNullException(nameof(rollbackManager));
            m_timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        }

        /// <summary>
        /// 在指定 Tick 时刻执行回滚射线检测。
        /// 自动管理碰撞体位置的回滚和恢复。
        /// </summary>
        /// <param name="tick">回滚到的目标 Tick</param>
        /// <param name="origin">射线起点</param>
        /// <param name="direction">射线方向（必须是归一化向量）</param>
        /// <param name="maxDistance">最大检测距离</param>
        /// <param name="layerMask">碰撞层级遮罩</param>
        /// <param name="hitInfo">输出参数，包含射线命中信息</param>
        /// <param name="asOwnerAndClientHost">是否为 clientHost 模式优化，仅对自身对象使用，可提升 clientHost 精度</param>
        /// <returns>是否检测到碰撞</returns>
        public bool RollbackRaycast(
            uint tick,
            Vector3 origin,
            Vector3 direction,
            float maxDistance,
            int layerMask,
            out RaycastHit hitInfo,
            bool asOwnerAndClientHost = false)
        {
            PreciseTick preciseTick = m_timeManager.GetPreciseTick(tick);

            // 使用射线路径版的 Rollback，仅回滚射线附近的碰撞体（性能更优）
            m_rollbackManager.Rollback(
                origin,
                direction,
                maxDistance,
                preciseTick,
                RollbackPhysicsType.Physics,
                asOwnerAndClientHost);

            bool result = global::UnityEngine.Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask);
            m_rollbackManager.Return();
            return result;
        }

        /// <summary>
        /// 在指定 Tick 时刻执行回滚球体投射。
        /// 自动管理碰撞体位置的回滚和恢复。
        /// </summary>
        /// <param name="tick">回滚到的目标 Tick</param>
        /// <param name="origin">球体中心起点</param>
        /// <param name="radius">球体半径</param>
        /// <param name="direction">投射方向（必须是归一化向量）</param>
        /// <param name="maxDistance">最大投射距离</param>
        /// <param name="layerMask">碰撞层级遮罩</param>
        /// <param name="hitInfo">输出参数，包含投射命中信息</param>
        /// <returns>是否检测到碰撞</returns>
        public bool RollbackSphereCast(
            uint tick,
            Vector3 origin,
            float radius,
            Vector3 direction,
            float maxDistance,
            int layerMask,
            out RaycastHit hitInfo)
        {
            PreciseTick preciseTick = m_timeManager.GetPreciseTick(tick);

            // 使用通用版 Rollback，回滚全部碰撞体
            m_rollbackManager.Rollback(preciseTick, RollbackPhysicsType.Physics, false);

            bool result = global::UnityEngine.Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask);
            m_rollbackManager.Return();
            return result;
        }

        /// <summary>
        /// 在指定 Tick 时刻执行回滚范围重叠检测。
        /// 自动管理碰撞体位置的回滚和恢复。
        /// </summary>
        /// <param name="tick">回滚到的目标 Tick</param>
        /// <param name="center">球体中心</param>
        /// <param name="radius">球体半径</param>
        /// <param name="layerMask">碰撞层级遮罩</param>
        /// <param name="results">预分配的碰撞体结果数组（使用非分配版本避免 GC）</param>
        /// <returns>检测到并写入 <paramref name="results"/> 的碰撞体数量</returns>
        public int RollbackOverlapSphere(
            uint tick,
            Vector3 center,
            float radius,
            int layerMask,
            Collider[] results)
        {
            PreciseTick preciseTick = m_timeManager.GetPreciseTick(tick);

            // 使用通用版 Rollback，回滚全部碰撞体
            m_rollbackManager.Rollback(preciseTick, RollbackPhysicsType.Physics, false);

            int count = global::UnityEngine.Physics.OverlapSphereNonAlloc(center, radius, results, layerMask);
            m_rollbackManager.Return();
            return count;
        }

        /// <summary>
        /// 显式恢复所有碰撞体到原始位置。
        /// 通常不需要手动调用——每个检测方法（RollbackRaycast、RollbackSphereCast、RollbackOverlapSphere）
        /// 在内部已自动调用 <see cref="RollbackManager.Return"/>。
        /// 仅在需要手动管理回滚生命周期的场景下调用此方法。
        /// </summary>
        public void Return()
        {
            m_rollbackManager.Return();
        }

        /// <summary>
        /// 释放适配器资源。
        /// 注意：<see cref="RollbackManager"/> 和 <see cref="TimeManager"/> 由 FishNet NetworkManager
        /// 管理生命周期，此处不销毁它们，仅清理适配器自身状态。
        /// </summary>
        public void Dispose()
        {
            // RollbackManager 和 TimeManager 由 FishNet NetworkManager 管理生命周期
            // 此处仅做清理，不销毁外部传入的引用
        }
    }
}
