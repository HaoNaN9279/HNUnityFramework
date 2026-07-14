using MemoryPack;

namespace HN.Framework.Core.Capability.Network.Prediction
{
    /// <summary>
    /// PredictionInputBase 的 MemoryPack 格式化器注册。
    /// 使用 <see cref="MemoryPackUnionFormatterAttribute"/> 为基础类型注册多态序列化。
    /// 派生类类型通过 <see cref="MemoryPackUnion"/> 特性标记其类型 ID。
    /// 此静态类提供集中的注册入口 <see cref="Register"/>。
    /// </summary>
    public static class PredictionInputFormatter
    {
        /// <summary>
        /// 注册所有预测相关的 MemoryPack 格式化器到全局提供器。
        /// 应在应用启动时调用（由 UnityFormattersInitializer 或 FishNetSerializerAdapter 调用链路触发）。
        /// </summary>
        /// <remarks>
        /// 基类 <see cref="PredictionInputBase"/> 已通过 [MemoryPackable] + [MemoryPackUnion] 注册多态支持。
        /// 扩展类型由业务代码使用 [MemoryPackUnion] 注册。
        /// 此方法供 FishNetSerializerAdapter 注册流程调用，作为类型发现的锚点。
        /// </remarks>
        public static void Register()
        {
            // 基类已通过 [MemoryPackable] + [MemoryPackUnion] 注册多态支持
            // 扩展类型由业务代码使用 [MemoryPackUnion] 注册
            // 此方法供 FishNetSerializerAdapter 注册流程调用，作为类型发现的锚点
        }
    }
}
