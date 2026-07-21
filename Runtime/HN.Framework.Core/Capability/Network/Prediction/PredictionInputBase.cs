using MemoryPack;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Capability.Network.Prediction
{
    /// <summary>
    /// 预测输入基类。所有客户端预测输入数据应继承此类。
    /// 实现 <see cref="IReference"/> 以支持 ReferencePool 复用，
    /// 标记 <see cref="MemoryPackableAttribute"/> 以支持二进制序列化。
    /// </summary>
    [MemoryPackable]
    public abstract partial class PredictionInputBase : IReference
    {
        /// <summary>
        /// 关联的网络 Tick（FishNet TimeManager.Tick）。
        /// </summary>
        [MemoryPackOrder(0)]
        public uint Tick { get; set; }

        /// <summary>
        /// 重置输入状态以便池复用。实现 <see cref="IReference"/> 接口。
        /// 子类必须重写此方法以清理自己的字段。
        /// </summary>
        public abstract void Clear();
    }
}
