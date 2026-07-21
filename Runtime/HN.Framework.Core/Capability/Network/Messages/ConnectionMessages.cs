using MemoryPack;

namespace HN.Framework.Core.Capability.Network.Messages
{
    /// <summary>
    /// 客户端已连接消息。当新客户端成功连接到服务端时发送。
    /// </summary>
    [MemoryPackable]
    public sealed partial class ClientConnectedMessage : MessageBase
    {
        /// <summary>
        /// 获取消息类型 ID。
        /// </summary>
        public override uint MessageId => 1;

        /// <summary>
        /// 获取连接的客户端 ID。
        /// </summary>
        public int ClientId { get; private set; }

        /// <summary>
        /// 供 MemoryPack 反序列化使用的参数化构造。
        /// </summary>
        public ClientConnectedMessage() { }

        /// <summary>
        /// 创建连接消息。
        /// </summary>
        /// <param name="clientId">客户端 ID。</param>
        public ClientConnectedMessage(int clientId)
        {
            ClientId = clientId;
        }

        /// <summary>
        /// 清理消息状态以便池复用。
        /// </summary>
        public override void Clear()
        {
            ClientId = 0;
        }
    }

    /// <summary>
    /// 客户端断开连接消息。当客户端断开连接时发送。
    /// </summary>
    [MemoryPackable]
    public sealed partial class ClientDisconnectedMessage : MessageBase
    {
        /// <summary>
        /// 获取消息类型 ID。
        /// </summary>
        public override uint MessageId => 2;

        /// <summary>
        /// 获取断开的客户端 ID。
        /// </summary>
        public int ClientId { get; private set; }

        /// <summary>
        /// 获取断开原因。
        /// </summary>
        public string Reason { get; private set; } = string.Empty;

        /// <summary>
        /// 供 MemoryPack 反序列化使用的参数化构造。
        /// </summary>
        public ClientDisconnectedMessage() { }

        /// <summary>
        /// 创建断开连接消息。
        /// </summary>
        /// <param name="clientId">客户端 ID。</param>
        /// <param name="reason">断开原因。</param>
        public ClientDisconnectedMessage(int clientId, string reason)
        {
            ClientId = clientId;
            Reason = reason ?? string.Empty;
        }

        /// <summary>
        /// 清理消息状态以便池复用。
        /// </summary>
        public override void Clear()
        {
            ClientId = 0;
            Reason = string.Empty;
        }
    }

    /// <summary>
    /// 服务端准备完成消息。当服务端完成初始化并准备接受连接时发送。
    /// </summary>
    [MemoryPackable]
    public sealed partial class ServerReadyMessage : MessageBase
    {
        /// <summary>
        /// 获取消息类型 ID。
        /// </summary>
        public override uint MessageId => 3;

        /// <summary>
        /// 供 MemoryPack 反序列化使用的参数化构造。
        /// </summary>
        public ServerReadyMessage() { }

        /// <summary>
        /// 清理消息状态以便池复用。
        /// </summary>
        public override void Clear()
        {
        }
    }
}
