using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Broadcast;
using FishNet.Transporting;
using HN.Framework.Core.Capability.Network;
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
    /// 网络消息总线，提供类型安全的消息发送与接收方法。
    /// 封装 FishNet 的底层 Broadcast API，通过 <see cref="NetworkMessageWrapper"/> 桥接
    /// 框架的 <see cref="MessageBase"/> 协议与 FishNet 的 <see cref="IBroadcast"/> 结构体约束。
    /// </summary>
    /// <remarks>
    /// 使用方式：
    /// <list type="bullet">
    /// <item>客户端 → 服务端：调用 <see cref="SendToServer{T}(T)"/></item>
    /// <item>服务端 → 指定客户端：调用 <see cref="SendToClient{T}(int, T)"/></item>
    /// <item>服务端 → 全部客户端：调用 <see cref="SendToAll{T}(T)"/></item>
    /// <item>注册接收：调用 <see cref="RegisterHandler{T}(Action{T})"/></item>
    /// <item>取消注册：调用 <see cref="UnregisterHandler{T}(Action{T})"/></item>
    /// </list>
    /// </remarks>
    public class FishNetMessageBus : IDisposable
    {
        /// <summary>
        /// 消息接收处理器注册表。键为 <see cref="MessageBase.MessageId"/>，
        /// 值为处理器委托列表和对应的反序列化函数。
        /// </summary>
        private readonly Dictionary<uint, List<(Delegate Handler, Func<byte[], object> Deserializer)>> m_receiveHandlers = new();

        /// <summary>
        /// 是否已启动广播监听。
        /// </summary>
        private bool m_isListening;

        /// <summary>
        /// 广播消息接收回调引用，用于确保 Register/Unregister 使用同一委托实例。
        /// </summary>
        private Action<NetworkMessageWrapper, Channel> m_receiveCallback;

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

        /// <summary>
        /// 注册指定消息类型的接收处理器。当收到对应类型的消息时，注册的处理器将被调用。
        /// 首次调用时会自动启动广播监听。同一消息类型可注册多个处理器。
        /// </summary>
        /// <typeparam name="T">消息类型，必须继承 <see cref="MessageBase"/> 并有无参构造函数</typeparam>
        /// <param name="handler">消息处理器委托</param>
        public void RegisterHandler<T>(Action<T> handler) where T : MessageBase, new()
        {
            uint messageId = new T().MessageId;

            if (!m_receiveHandlers.TryGetValue(messageId, out var list))
            {
                list = new List<(Delegate, Func<byte[], object>)>();
                m_receiveHandlers[messageId] = list;
            }

            Func<byte[], object> deserializer = static bytes => MemoryPackSerializer.Deserialize<T>(bytes);
            list.Add((handler, deserializer));

            if (!m_isListening)
            {
                StartListening();
            }
        }

        /// <summary>
        /// 取消注册指定消息类型的接收处理器。
        /// </summary>
        /// <typeparam name="T">消息类型，必须继承 <see cref="MessageBase"/> 并有无参构造函数</typeparam>
        /// <param name="handler">要取消注册的处理器委托</param>
        public void UnregisterHandler<T>(Action<T> handler) where T : MessageBase, new()
        {
            uint messageId = new T().MessageId;

            if (m_receiveHandlers.TryGetValue(messageId, out var list))
            {
                list.RemoveAll(item => item.Handler == (Delegate)handler);
                if (list.Count == 0)
                {
                    m_receiveHandlers.Remove(messageId);
                }
            }
        }

        /// <summary>
        /// 释放资源，停止广播监听并清空处理器注册表。
        /// </summary>
        public void Dispose()
        {
            if (m_isListening && m_receiveCallback != null)
            {
                InstanceFinder.ClientManager.UnregisterBroadcast<NetworkMessageWrapper>(m_receiveCallback);
                m_isListening = false;
                m_receiveCallback = null;
            }
            m_receiveHandlers.Clear();
        }

        /// <summary>
        /// 启动广播监听，订阅 FishNet 的 ClientManager 广播接收。
        /// </summary>
        private void StartListening()
        {
            m_receiveCallback = (wrapper, channel) => OnMessageReceived(wrapper);
            InstanceFinder.ClientManager.RegisterBroadcast<NetworkMessageWrapper>(m_receiveCallback);
            m_isListening = true;
        }

        /// <summary>
        /// 收到广播消息时的回调，按 MessageId 路由到注册的处理器。
        /// </summary>
        /// <param name="wrapper">网络消息包装器</param>
        private void OnMessageReceived(NetworkMessageWrapper wrapper)
        {
            if (!m_receiveHandlers.TryGetValue(wrapper.MessageId, out var list))
                return;

            foreach (var (handler, deserializer) in list)
            {
                object message = deserializer(wrapper.Payload);
                handler.DynamicInvoke(message);
            }
        }
    }
}
