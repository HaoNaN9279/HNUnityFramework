using MemoryPack;

namespace HN.Framework.Core.Capability.Network.Prediction
{
    /// <summary>
    /// 调和（Reconciliation）数据容器。
    /// 服务端计算权威状态快照，下发给客户端用于回滚与重放。
    /// </summary>
    /// <typeparam name="T">权威状态类型，必须为非托管类型</typeparam>
    [MemoryPackable]
    public partial struct PredictionReconcileData<T> where T : unmanaged
    {
        /// <summary>
        /// 客户端发起此输入时的本地 Tick。
        /// </summary>
        [MemoryPackOrder(0)]
        public uint ClientTick;

        /// <summary>
        /// 服务端处理此输入时的权威 Tick。
        /// </summary>
        [MemoryPackOrder(1)]
        public uint ServerTick;

        /// <summary>
        /// 服务端计算的权威状态快照。
        /// 客户端据此回滚并重放未确认的输入。
        /// </summary>
        [MemoryPackOrder(2)]
        public T AuthoritativeState;
    }
}
