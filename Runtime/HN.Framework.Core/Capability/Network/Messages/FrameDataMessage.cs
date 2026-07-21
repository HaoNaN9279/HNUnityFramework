using MemoryPack;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Capability.Network.Messages
{
    /// <summary>
    /// 帧数据消息，由服务端广播至所有客户端，用于分发指定帧号的输入数据。
    /// MessageId = 101。
    /// </summary>
    [MemoryPackable]
    public sealed partial class FrameDataMessage : MessageBase
    {
        /// <summary>
        /// 获取消息类型 ID。
        /// </summary>
        public override uint MessageId => 101;

        /// <summary>
        /// 帧号。
        /// </summary>
        public ulong FrameNumber { get; private set; }

        /// <summary>
        /// 该帧所有客户端的输入数据数组。
        /// </summary>
        public FrameInput[] Inputs { get; private set; }

        /// <summary>
        /// 该帧的服务端校验和，供客户端比对。
        /// </summary>
        public ulong Checksum { get; private set; }

        /// <summary>
        /// 供 MemoryPack 反序列化使用的无参构造。
        /// </summary>
        public FrameDataMessage() { }

        /// <summary>
        /// 创建帧数据消息。
        /// </summary>
        /// <param name="frameNumber">帧号</param>
        /// <param name="inputs">所有客户端的输入</param>
        /// <param name="checksum">服务端校验和</param>
        public FrameDataMessage(ulong frameNumber, FrameInput[] inputs, ulong checksum)
        {
            FrameNumber = frameNumber;
            Inputs = inputs;
            Checksum = checksum;
        }

        /// <summary>
        /// 清理消息状态以便池复用。
        /// </summary>
        public override void Clear()
        {
            FrameNumber = 0;
            Inputs = null;
            Checksum = 0;
        }
    }
}