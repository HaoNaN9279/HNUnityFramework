using global::MemoryPack;
using global::MemoryPack.Internal;

namespace HN.Framework.Core.Capability.Network.Prediction
{
    /// <summary>
    /// 预测类型格式化器注册入口。
    /// 提供 <see cref="Register"/> 方法注册预测相关的所有 MemoryPack 格式化器。
    /// </summary>
    public static class PredictionInputFormatter
    {
        private static volatile bool _isRegistered;

        /// <summary>
        /// 注册所有预测相关的 MemoryPack 格式化器。幂等操作。
        /// 应在应用启动时（由 FishNetSerializerAdapter 注册流程）调用。
        /// </summary>
        public static void Register()
        {
            if (_isRegistered) return;
            _isRegistered = true;

            // 注册开放泛型 PredictionReconcileData<T> 格式化器
            MemoryPackFormatterProvider.RegisterGenericType(
                typeof(PredictionReconcileData<>),
                typeof(PredictionReconcileDataFormatter<>));

            // 注册 PredictionInputBase 基类格式化器
            MemoryPackFormatterProvider.Register(new PredictionInputBaseFormatter());
        }

        /// <summary>
        /// PredictionReconcileData 格式化器（开放泛型，T 必须为 unmanaged）。
        /// 序列化 ClientTick（uint）/ ServerTick（uint）/ AuthoritativeState（T）。
        /// </summary>
        [Preserve]
        internal sealed class PredictionReconcileDataFormatter<T> : MemoryPackFormatter<PredictionReconcileData<T>>
            where T : unmanaged
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PredictionReconcileData<T> value)
            {
                writer.WriteUnmanaged(value.ClientTick, value.ServerTick);
                writer.WriteUnmanaged(value.AuthoritativeState);
            }

            public override void Deserialize(ref MemoryPackReader reader, ref PredictionReconcileData<T> value)
            {
                reader.ReadUnmanaged(out uint clientTick, out uint serverTick);
                reader.ReadUnmanaged(out T state);

                value = new PredictionReconcileData<T>
                {
                    ClientTick = clientTick,
                    ServerTick = serverTick,
                    AuthoritativeState = state
                };
            }
        }

        /// <summary>
        /// PredictionInputBase 格式化器（抽象基类，仅序列化 Tick 字段）。
        /// </summary>
        /// <remarks>
        /// 此格式化器用于序列化基类 PredictionInputBase 的公共字段。
        /// 反序列化时会抛出 <see cref="System.NotSupportedException"/>，因为无法实例化抽象类型。
        /// 具体子类的序列化需通过业务代码注册对应的封闭类型格式化器或 MemoryPackUnionFormatter。
        /// </remarks>
        [Preserve]
        internal sealed class PredictionInputBaseFormatter : MemoryPackFormatter<PredictionInputBase>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PredictionInputBase? value)
            {
                if (value == null)
                {
                    writer.WriteNullObjectHeader();
                    return;
                }

                writer.WriteObjectHeader(1);
                writer.WriteUnmanaged(value.Tick);
            }

            public override void Deserialize(ref MemoryPackReader reader, ref PredictionInputBase? value)
            {
                throw new System.NotSupportedException(
                    "PredictionInputBase is abstract and cannot be deserialized directly. " +
                    "Use a concrete subclass formatter or a MemoryPackUnionFormatter.");
            }
        }
    }
}
