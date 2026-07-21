using HN.Framework.Core.Driver.Common;
using MemoryPack;

namespace HN.Framework.Core.Capability.Network.Messages
{
    /// <summary>
    /// 网络消息基类。所有网络消息应继承此类并标记 <see cref="MemoryPackableAttribute"/>。
    /// </summary>
    [MemoryPackable]
    public abstract partial class MessageBase : IReference
    {
        /// <summary>
        /// 获取消息类型 ID，由具体子类定义。
        /// </summary>
        public abstract uint MessageId { get; }

        /// <summary>
        /// 重置消息状态以便池复用。实现 <see cref="IReference"/> 接口。
        /// 子类必须重写此方法以清理自己的字段。
        /// </summary>
        public abstract void Clear();
    }
}
