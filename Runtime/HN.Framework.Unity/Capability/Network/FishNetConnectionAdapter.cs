namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// 连接状态管理适配器，提供连接状态查询和连接参数管理。
    /// 封装 FishNet 的连接状态信息，供上层业务逻辑使用。
    /// </summary>
    /// <remarks>
    /// 典型使用方式：在网络模块初始化时创建，在 FishNet 的连接/断开回调中调用 <see cref="UpdateConnectionState"/> 更新状态，
    /// 其他模块通过 <see cref="IsConnected"/> 和 <see cref="LocalClientId"/> 查询当前连接信息。
    /// </remarks>
    public class FishNetConnectionAdapter
    {
        /// <summary>
        /// 缓存的网络全局配置。由模块初始化时设置。
        /// </summary>
        internal static NetworkSettings? s_CachedSettings;

        /// <summary>
        /// 是否已成功连接到远程服务端。
        /// 当 FishNet 的 ClientManager 建立连接后由外部调用 <see cref="UpdateConnectionState"/> 设置为 <c>true</c>。
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// 本地客户端在服务端分配的唯一连接 ID。
        /// 对应 <see cref="FishNet.Connection.NetworkConnection.ClientId"/>，可用于 <see cref="FishNetMessageBus.SendToClient{T}(int, T)"/> 等方法。
        /// </summary>
        public int LocalClientId { get; private set; }

        /// <summary>
        /// 要连接的远程服务端 IP 地址或域名。
        /// 优先读取 <see cref="NetworkSettings"/>，回退默认值 <c>"127.0.0.1"</c>。
        /// </summary>
        public string ServerAddress { get; set; } = s_CachedSettings?.DefaultServerAddress ?? "127.0.0.1";

        /// <summary>
        /// 要连接的远程服务端端口号。
        /// 优先读取 <see cref="NetworkSettings"/>，回退默认值 <c>7777</c>。
        /// </summary>
        public ushort ServerPort { get; set; } = s_CachedSettings?.DefaultServerPort ?? 7777;

        /// <summary>
        /// 更新当前连接状态。
        /// 应在 FishNet 的 <c>OnClientConnectionState</c> 等连接事件回调中调用，
        /// 传入 FishNet 提供的连接状态和客户端 ID。
        /// </summary>
        /// <param name="isConnected">新的连接状态：<c>true</c> 表示已连接，<c>false</c> 表示已断开</param>
        /// <param name="localClientId">本地客户端在服务端分配的唯一连接 ID</param>
        public void UpdateConnectionState(bool isConnected, int localClientId)
        {
            IsConnected = isConnected;
            LocalClientId = localClientId;
        }
    }
}
