using System;
using FishNet.Managing.Predicting;
using FishNet.Managing.Timing;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Network.Prediction
{
    /// <summary>
    /// FishNet <see cref="PredictionManager"/> 的框架封装。
    /// 提供统一的预测状态访问 API，由 <see cref="FishNetNetworkManager"/> 持有并注入。
    /// 封装 FishNet 内部事件订阅，业务代码无需直接依赖 PredictionManager。
    /// </summary>
    public class PredictionManagerAdapter : IDisposable
    {
        private readonly PredictionManager m_predictionManager;
        private readonly TimeManager m_timeManager;

        /// <summary>
        /// 初始化 PredictionManagerAdapter。
        /// </summary>
        /// <param name="predictionManager">FishNet PredictionManager 实例</param>
        /// <param name="timeManager">FishNet TimeManager 实例</param>
        public PredictionManagerAdapter(PredictionManager predictionManager, TimeManager timeManager)
        {
            m_predictionManager = predictionManager ?? throw new ArgumentNullException(nameof(predictionManager));
            m_timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));

            // 转发 PredictionManager 事件
            m_predictionManager.OnPreReconcile += OnPreReconcileInternal;
            m_predictionManager.OnReconcile += OnReconcileInternal;
            m_predictionManager.OnPostReconcile += OnPostReconcileInternal;
            m_predictionManager.OnPreReplicateReplay += OnPreReplicateReplayInternal;
            m_predictionManager.OnPostReplicateReplay += OnPostReplicateReplayInternal;
        }

        #region 暴露属性

        /// <summary>
        /// 当前是否正在执行调和（Reconciliation）。
        /// </summary>
        public bool IsReconciling => m_predictionManager.IsReconciling;

        /// <summary>
        /// 本地客户端正在重放的权威输入 Tick。
        /// </summary>
        public uint ClientReplayTick => m_predictionManager.ClientReplayTick;

        /// <summary>
        /// 本地客户端正在重放的非权威输入 Tick。
        /// </summary>
        public uint ServerReplayTick => m_predictionManager.ServerReplayTick;

        /// <summary>
        /// 最近一次调和时的客户端 Tick。
        /// </summary>
        public uint ClientStateTick => m_predictionManager.ClientStateTick;

        /// <summary>
        /// 最近一次调和时的服务端 Tick。
        /// </summary>
        public uint ServerStateTick => m_predictionManager.ServerStateTick;

        /// <summary>
        /// 状态插值缓冲量。较大值增强网络波动容错，但增加延迟。
        /// </summary>
        public byte StateInterpolation => m_predictionManager.StateInterpolation;

        /// <summary>
        /// 当前网络 Tick 速率。
        /// </summary>
        public ushort TickRate => m_timeManager.TickRate;

        /// <summary>
        /// 网络往返时间（毫秒）。
        /// </summary>
        public long RoundTripTime => m_timeManager.RoundTripTime;

        #endregion

        #region 转发的预测事件

        /// <summary>
        /// 调和即将开始前触发。
        /// </summary>
        public event Action<uint, uint> OnPreReconcile;

        /// <summary>
        /// 调和进行中触发。
        /// </summary>
        public event Action<uint, uint> OnReconcile;

        /// <summary>
        /// 调和完成后触发。
        /// </summary>
        public event Action<uint, uint> OnPostReconcile;

        /// <summary>
        /// 重放复制方法前触发（物理模拟前）。
        /// </summary>
        public event Action<uint, uint> OnPreReplicateReplay;

        /// <summary>
        /// 重放复制方法后触发（物理模拟后）。
        /// </summary>
        public event Action<uint, uint> OnPostReplicateReplay;

        #endregion

        #region 内部事件转发

        private void OnPreReconcileInternal(uint clientTick, uint serverTick)
        {
            OnPreReconcile?.Invoke(clientTick, serverTick);
        }

        private void OnReconcileInternal(uint clientTick, uint serverTick)
        {
            OnReconcile?.Invoke(clientTick, serverTick);
        }

        private void OnPostReconcileInternal(uint clientTick, uint serverTick)
        {
            OnPostReconcile?.Invoke(clientTick, serverTick);
        }

        private void OnPreReplicateReplayInternal(uint clientTick, uint serverTick)
        {
            OnPreReplicateReplay?.Invoke(clientTick, serverTick);
        }

        private void OnPostReplicateReplayInternal(uint clientTick, uint serverTick)
        {
            OnPostReplicateReplay?.Invoke(clientTick, serverTick);
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取网络时序信息统一元组。
        /// </summary>
        public (uint tick, long rtt, ushort tickRate) GetNetworkTickInfo()
        {
            return (m_timeManager.Tick, m_timeManager.RoundTripTime, m_timeManager.TickRate);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 释放资源，取消事件订阅。
        /// </summary>
        public void Dispose()
        {
            if (m_predictionManager != null)
            {
                m_predictionManager.OnPreReconcile -= OnPreReconcileInternal;
                m_predictionManager.OnReconcile -= OnReconcileInternal;
                m_predictionManager.OnPostReconcile -= OnPostReconcileInternal;
                m_predictionManager.OnPreReplicateReplay -= OnPreReplicateReplayInternal;
                m_predictionManager.OnPostReplicateReplay -= OnPostReplicateReplayInternal;
            }
        }

        #endregion
    }
}
