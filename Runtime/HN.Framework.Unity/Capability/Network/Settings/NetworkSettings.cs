using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// 网络系统全局配置 ScriptableObject。
    /// 控制帧同步参数及连接默认值。
    /// </summary>
    public class NetworkSettings : ScriptableObject
    {
        // ── 运行时公开属性 ──

        /// <summary>
        /// 逻辑帧率（tick/s），默认 15。
        /// </summary>
        public int LockstepFrameRate => m_LockstepFrameRate;

        /// <summary>
        /// 帧输入缓冲容量，默认 3。
        /// </summary>
        public int LockstepBufferSize => m_LockstepBufferSize;

        /// <summary>
        /// 追帧保护上限（相对缓冲容量的倍数），默认 3。
        /// 实际上限 = BufferSize × CatchUpMultiplier。
        /// </summary>
        public int CatchUpMultiplier => m_CatchUpMultiplier;

        /// <summary>
        /// 默认服务端 IP 地址。
        /// </summary>
        public string DefaultServerAddress => m_DefaultServerAddress;

        /// <summary>
        /// 默认服务端端口号。
        /// </summary>
        public ushort DefaultServerPort => m_DefaultServerPort;

        // ── 序列化字段 ──

        [SerializeField, Range(1, 60)]
        [Tooltip("逻辑帧率（tick/s）")]
        private int m_LockstepFrameRate = 15;

        [SerializeField, Range(1, 10)]
        [Tooltip("帧输入缓冲容量")]
        private int m_LockstepBufferSize = 3;

        [SerializeField, Range(1, 10)]
        [Tooltip("追帧保护上限倍数（实际上限 = BufferSize × 此值）")]
        private int m_CatchUpMultiplier = 3;

        [SerializeField]
        [Tooltip("默认服务端 IP 地址")]
        private string m_DefaultServerAddress = "127.0.0.1";

        [SerializeField, Range(1, 65535)]
        [Tooltip("默认服务端端口号")]
        private ushort m_DefaultServerPort = 7777;

        // ── Editor 侧工厂方法 ──

#if UNITY_EDITOR
        /// <summary>
        /// 获取或创建网络配置资源。
        /// </summary>
        public static NetworkSettings GetOrCreateSettings()
        {
            return Driver.Platform.HNModuleSettingsUtility
                .GetOrCreateSettings<NetworkSettings>(AssetPath);
        }

        /// <summary>
        /// 获取序列化版本的网络配置资源，用于 Editor 绘制。
        /// </summary>
        public static SerializedObject GetSerializedSettings()
        {
            return Driver.Platform.HNModuleSettingsUtility
                .GetSerializedSettings<NetworkSettings>(AssetPath);
        }

        /// <summary>
        /// 配置资源保存路径。
        /// </summary>
        public static readonly string AssetPath =
            "Assets/Project/RuntimeAssets/Core/NetworkSettings.asset";
#endif
    }
}
