using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Transporting;
using HN.Framework.Core.Capability.Network;
using HN.Framework.Core.Capability.Network.Prediction;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Network.Prediction
{
    /// <summary>
    /// 支持客户端预测的网络实体视图基类。
    /// 采用辅助方法模式（非 <see cref="ReplicateAttribute"/> 直接标记），
    /// 提供输入缓冲、调和重放和状态管理的便利方法。
    /// 业务子类需自行声明 <c>[Replicate]</c> 和 <c>[Reconcile]</c> 方法，
    /// 并在方法体内调用本类提供的辅助方法。
    /// </summary>
    public abstract class PredictedNetworkEntityView : NetworkEntityView
    {
        /// <summary>
        /// 输入历史缓冲区，容量 = 最大 TickRate(60) × 5 = 300。
        /// 使用 <see cref="List{T}"/> 实现，避免对 FishNet 内部 GameKit 程序集的依赖。
        /// </summary>
        private readonly List<PredictionInputBase> m_inputHistory
            = new List<PredictionInputBase>(300);

        /// <summary>
        /// 输入历史缓冲区的最大容量。
        /// </summary>
        private const int MAX_INPUT_HISTORY = 300;

        /// <summary>
        /// 获取预测配置的 Tick 速率。
        /// </summary>
        protected ushort TickRate => NetworkManager.TimeManager.TickRate;

        /// <summary>
        /// 从对象池获取指定类型的预测输入实例，并设置 Tick。
        /// 在业务子类的 <c>[Replicate]</c> 方法中调用。
        /// </summary>
        /// <typeparam name="T">输入数据类型（必须继承 PredictionInputBase 并有无参构造）</typeparam>
        /// <param name="tick">关联的网络 Tick</param>
        /// <returns>从对象池获取的输入实例</returns>
        protected T CreateReplicateData<T>(uint tick) where T : PredictionInputBase, new()
        {
            var data = ReferencePool.Acquire<T>();
            data.Tick = tick;
            return data;
        }

        /// <summary>
        /// 将预测输入实例归还到对象池。
        /// </summary>
        /// <typeparam name="T">输入数据类型</typeparam>
        /// <param name="data">要释放的输入实例</param>
        protected void ReleaseReplicateData<T>(T data) where T : PredictionInputBase
        {
            ReferencePool.Release(data);
        }

        /// <summary>
        /// 将输入存储到历史缓冲区中，用于后续调和重放。
        /// 在业务子类的 <c>[Replicate]</c> 方法中调用。
        /// 当缓冲区满时，自动淘汰最早的条目并归池。
        /// </summary>
        /// <param name="input">要存储的预测输入</param>
        protected void StoreReplicateInput(PredictionInputBase input)
        {
            // 达到容量上限时，淘汰最旧条目
            if (m_inputHistory.Count >= MAX_INPUT_HISTORY)
            {
                var oldest = m_inputHistory[0];
                m_inputHistory.RemoveAt(0);
                if (oldest != null)
                {
                    ReleaseReplicateData(oldest);
                }
            }
            m_inputHistory.Add(input);
        }

        /// <summary>
        /// 从缓冲区检索指定 Tick 范围内的历史输入列表。
        /// 在业务子类的 <c>[Reconcile]</c> 方法中调用，用于重放未确认的输入。
        /// </summary>
        /// <param name="fromTick">起始 Tick（包含）</param>
        /// <param name="toTick">结束 Tick（包含）</param>
        /// <returns>指定 Tick 范围内的输入列表（不拥有所有权，调用者不应释放）</returns>
        protected List<PredictionInputBase> GetReconcileInputs(uint fromTick, uint toTick)
        {
            var result = new List<PredictionInputBase>();
            for (int i = 0; i < m_inputHistory.Count; i++)
            {
                var input = m_inputHistory[i];
                if (input != null && input.Tick >= fromTick && input.Tick <= toTick)
                {
                    result.Add(input);
                }
            }
            return result;
        }

        /// <summary>
        /// 应用服务端权威状态到关联的 <see cref="SyncedModel{T}"/>。
        /// 在业务子类的 <c>[Reconcile]</c> 方法中调用。
        /// 默认实现为空，子类可通过重写将权威状态应用到自己的 SyncedModel。
        /// </summary>
        /// <typeparam name="T">状态数据类型（unmanaged）</typeparam>
        /// <param name="data">服务端下发的调和数据</param>
        protected void ApplyReconcileState<T>(PredictionReconcileData<T> data) where T : unmanaged
        {
            // 子类可以通过重写此方法将权威状态应用到自己的 SyncedModel
            // 默认行为：子类应遍历其关联模型并更新
        }

        /// <summary>
        /// 获取当前网络时序信息。
        /// </summary>
        /// <returns>(remoteTick, roundTripTime, tickRate)</returns>
        protected (uint remoteTick, long rtt, ushort rate) GetNetworkTickInfo()
        {
            var timeManager = NetworkManager.TimeManager;
            return (timeManager.Tick, timeManager.RoundTripTime, timeManager.TickRate);
        }

        /// <summary>
        /// 当网络实体销毁时，清理输入缓冲区中的所有条目。
        /// </summary>
        public override void OnDespawned()
        {
            // 释放缓冲区中所有输入到对象池
            for (int i = 0; i < m_inputHistory.Count; i++)
            {
                var input = m_inputHistory[i];
                if (input != null)
                {
                    ReleaseReplicateData(input);
                }
            }
            m_inputHistory.Clear();

            base.OnDespawned();
        }
    }
}
