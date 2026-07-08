using System;

namespace HN.Framework.Core.Capability.Network
{
    /// <summary>
    /// 网络管理器接口，定义网络连接、服务端/客户端模式切换及连接事件。
    /// </summary>
    public interface INetworkManager
    {
        /// <summary>
        /// 获取是否已启动服务端。
        /// </summary>
        bool IsServer { get; }

        /// <summary>
        /// 获取是否已连接为客户端。
        /// </summary>
        bool IsClient { get; }

        /// <summary>
        /// 获取是否同时作为服务端和客户端（Host 模式）。
        /// </summary>
        bool IsHost { get; }

        /// <summary>
        /// 启动服务端，等待客户端连接。
        /// </summary>
        void StartServer();

        /// <summary>
        /// 作为客户端连接到指定服务端地址。
        /// </summary>
        void StartClient();

        /// <summary>
        /// 断开当前所有连接。
        /// </summary>
        void StopConnection();

        /// <summary>
        /// 连接到远程服务端（旧式 API，与 StartClient 等价）。
        /// </summary>
        void Connect();

        /// <summary>
        /// 断开连接（旧式 API，与 StopConnection 等价）。
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 有新客户端连接时触发，参数为连接的客户端 ID。
        /// </summary>
        event Action<int> OnClientConnected;

        /// <summary>
        /// 有客户端断开连接时触发，参数为断开的客户端 ID。
        /// </summary>
        event Action<int> OnClientDisconnected;
    }
}
