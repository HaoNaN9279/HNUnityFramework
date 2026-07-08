using FishNet;
using FishNet.Broadcast;
using HN.Framework.Core.Capability.Network.Messages;
using MemoryPack;

namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// FishNet 广播消息包装器，用于在 FishNet 的 <see cref="IBroadcast"/> 基础设施上传输框架的 <see cref="MessageBase"/> 消息。
    /// 将 <see cref="MessageBase"/> 序列化为字节数组后通过 FishNet 的 Broadcast 系统发送。
    /// </summary>
    internal struct NetworkMessageWrapper : IBroadcast
    {
        /// <summary>
        /// 消息类型标识符，由 <see cref="MessageBase.MessageId"/> 提供。
        /// </summary>
        public uint MessageId;

        /// <summary>
        /// 经过 MemoryPack 序列化的消息负载。
        /// </summary>
        public byte[] Payload;
    }

    /// <summary>
    /// 网络消息总线，提供类型安全的消息发送方法。
    /// 封装 FishNet 的底层 Broadcast API，通过 <see cref="NetworkMessageWrapper"/> 桥接
    /// 框架的 <see cref="MessageBase"/> 协议与 FishNet 的 <see cref="IBroadcast"/> 结构体约束。
    /// </summary>
    /// <remarks>
    /// 使用方式：
    /// <list type="bullet">
    /// <item>客户端 → 服务端：调用 <see cref="SendToServer{T}(T)"/></item>
    /// <item>服务端 → 指定客户端：调用 <see cref="SendToClient{T}(int, T)"/></item>
    /// <item>服务端 → 全部客户端：调用 <see cref="SendToAll{T}(T)"/></item>
    /// </list>
    /// </remarks>
    public class FishNetMessageBus
    {
        /// <summary>
        /// 发送消息到服务端。
        /// 将消息序列化后通过 FishNet 的 ClientManager.Broadcast 发送。
        /// 仅在客户端模式下有效。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="MessageBase"/> 的具体消息类型，必须标记 <see cref="MemoryPackableAttribute"/></typeparam>
        /// <param name="message">要发送的消息实例</param>
        public void SendToServer<T>(T message) where T : MessageBase
        {
            var payload = MemoryPackSerializer.Serialize(message);
            var wrapper = new NetworkMessageWrapper
            {
                MessageId = message.MessageId,
                Payload = payload
            };
            InstanceFinder.ClientManager.Broadcast(wrapper);
        }

        /// <summary>
        /// 发送消息到指定客户端。
        /// 将消息序列化后通过 FishNet 的 ServerManager.Broadcast 发送到目标连接。
        /// 仅在服务端模式下有效。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="MessageBase"/> 的具体消息类型，必须标记 <see cref="MemoryPackableAttribute"/></typeparam>
        /// <param name="clientId">目标客户端的连接 ID，对应 <see cref="FishNet.Connection.NetworkConnection.ClientId"/></param>
        /// <param name="message">要发送的消息实例</param>
        public void SendToClient<T>(int clientId, T message) where T : MessageBase
        {
            var payload = MemoryPackSerializer.Serialize(message);
            var wrapper = new NetworkMessageWrapper
            {
                MessageId = message.MessageId,
                Payload = payload
            };

            if (InstanceFinder.ServerManager.Clients.TryGetValue(clientId, out var connection))
            {
                InstanceFinder.ServerManager.Broadcast(connection, wrapper);
            }
        }

        /// <summary>
        /// 广播消息到所有已连接的客户端。
        /// 将消息序列化后通过 FishNet 的 ServerManager.Broadcast 广播到全部客户端。
        /// 仅在服务端模式下有效。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="MessageBase"/> 的具体消息类型，必须标记 <see cref="MemoryPackableAttribute"/></typeparam>
        /// <param name="message">要广播的消息实例</param>
        public void SendToAll<T>(T message) where T : MessageBase
        {
            var payload = MemoryPackSerializer.Serialize(message);
            var wrapper = new NetworkMessageWrapper
            {
                MessageId = message.MessageId,
                Payload = payload
            };
            InstanceFinder.ServerManager.Broadcast(wrapper);
        }
    }
}
