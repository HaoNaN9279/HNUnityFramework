using System;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Managing.Client;
using FishNet.Transporting;
using HN.Framework.Core.Capability.Network;

namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// FishNet 网络管理器的包装器，将 FishNet 的 NetworkManager 适配为框架的 INetworkManager 接口。
    /// 不继承 NetworkBehaviour / MonoBehaviour，仅作为纯 C# 门面委托给场景中已存在的 FishNet.NetworkManager。
    /// </summary>
    public class FishNetNetworkManager : INetworkManager
    {
        private NetworkManager _fishNetManager;

        /// <summary>
        /// 是否有客户端连接时触发，参数为连接的客户端 ID。
        /// </summary>
        public event Action<int> OnClientConnected;

        /// <summary>
        /// 是否有客户端断开连接时触发，参数为断开的客户端 ID。
        /// </summary>
        public event Action<int> OnClientDisconnected;

        /// <summary>
        /// 获取是否已启动服务端。
        /// 若尚未调用 Initialize 或已 Dispose，始终返回 false。
        /// </summary>
        public bool IsServer => _fishNetManager != null && _fishNetManager.ServerManager != null && _fishNetManager.ServerManager.Started;

        /// <summary>
        /// 获取是否已连接为客户端。
        /// 若尚未调用 Initialize 或已 Dispose，始终返回 false。
        /// </summary>
        public bool IsClient => _fishNetManager != null && _fishNetManager.ClientManager != null && _fishNetManager.ClientManager.Started;

        /// <summary>
        /// 获取是否同时作为服务端和客户端（Host 模式）。
        /// 若尚未调用 Initialize 或已 Dispose，始终返回 false。
        /// </summary>
        public bool IsHost => IsServer && IsClient;

        /// <summary>
        /// 使用指定的 FishNet NetworkManager 初始化包装器，并订阅连接事件。
        /// </summary>
        /// <param name="manager">场景中已存在的 FishNet.NetworkManager 实例。</param>
        /// <exception cref="ArgumentNullException">当 manager 为 null 时抛出。</exception>
        public void Initialize(NetworkManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            _fishNetManager = manager;

            // 订阅服务端远程连接事件，用于通知新客户端连接/断开
            if (_fishNetManager.ServerManager != null)
                _fishNetManager.ServerManager.OnRemoteConnectionState += OnServerRemoteConnectionState;
        }

        /// <summary>
        /// 启动服务端，等待客户端连接。
        /// </summary>
        public void StartServer()
        {
            if (_fishNetManager?.ServerManager == null)
                return;

            _fishNetManager.ServerManager.StartConnection();
        }

        /// <summary>
        /// 作为客户端连接到已配置的服务端地址。
        /// </summary>
        public void StartClient()
        {
            if (_fishNetManager?.ClientManager == null)
                return;

            _fishNetManager.ClientManager.StartConnection();
        }

        /// <summary>
        /// 断开当前所有连接（同时停止服务端和客户端）。
        /// </summary>
        public void StopConnection()
        {
            if (_fishNetManager == null)
                return;

            if (_fishNetManager.ServerManager != null)
                _fishNetManager.ServerManager.StopConnection(true);
            if (_fishNetManager.ClientManager != null)
                _fishNetManager.ClientManager.StopConnection();
        }

        /// <summary>
        /// 连接到远程服务端（旧式 API，等价于 <see cref="StartClient"/>）。
        /// </summary>
        public void Connect()
        {
            StartClient();
        }

        /// <summary>
        /// 断开连接（旧式 API，等价于 <see cref="StopConnection"/>）。
        /// </summary>
        public void Disconnect()
        {
            StopConnection();
        }

        /// <summary>
        /// 处理服务端远程连接状态变更，转发为 OnClientConnected / OnClientDisconnected 事件。
        /// </summary>
        /// <param name="conn">远程客户端的网络连接。</param>
        /// <param name="args">连接状态变更参数。</param>
        private void OnServerRemoteConnectionState(FishNet.Connection.NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            int connectionId = args.ConnectionId;

            switch (args.ConnectionState)
            {
                case RemoteConnectionState.Started:
                    OnClientConnected?.Invoke(connectionId);
                    break;
                case RemoteConnectionState.Stopped:
                    OnClientDisconnected?.Invoke(connectionId);
                    break;
            }
        }
    }
}
