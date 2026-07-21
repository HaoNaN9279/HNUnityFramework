using MemoryPack;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Capability.Network.Messages
{
    /// <summary>
    /// 帧输入消息，由客户端发送至服务端，用于提交指定帧号的输入快照。
    /// MessageId = 100。
    /// </summary>
    [MemoryPackable]
    public sealed partial class FrameInputMessage : MessageBase
    {
        /// <summary>
        /// 获取消息类型 ID。
        /// </summary>
        public override uint MessageId => 100;

        /// <summary>
        /// 发送输入的客户端 ID。
        /// </summary>
        public int ClientId { get; private set; }

        /// <summary>
        /// 帧输入数据，包含帧号和动作键值对。
        /// </summary>
        public FrameInput Input { get; private set; }

        /// <summary>
        /// 供 MemoryPack 反序列化使用的无参构造。
        /// </summary>
        public FrameInputMessage() { }

        /// <summary>
        /// 创建帧输入消息。
        /// </summary>
        /// <param name="clientId">客户端 ID</param>
        /// <param name="input">帧输入数据</param>
        public FrameInputMessage(int clientId, FrameInput input)
        {
            ClientId = clientId;
            Input = input;
        }

        /// <summary>
        /// 清理消息状态以便池复用。
        /// </summary>
        public override void Clear()
        {
            ClientId = 0;
            Input = default;
        }
    }
}