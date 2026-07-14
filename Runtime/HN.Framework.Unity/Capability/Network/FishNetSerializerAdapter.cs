using System;
using FishNet.Serializing;
using HN.Framework.Core.Capability.Network.Prediction;
using HN.Framework.Core.Driver.Common.Serialization;
using HN.Framework.Unity.Capability.Serialization;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// FishNet 序列化适配器，将 MemoryPack 注册为 FishNet 的自定义序列化器。
    /// 在 GameWorld 初始化时调用 <see cref="RegisterMemoryPackSerializer"/> 以启用。
    /// </summary>
    public static class FishNetSerializerAdapter
    {
        /// <summary>
        /// 注册 MemoryPack 为 FishNet 自定义序列化器。
        /// 为所有已知类型注册 Writer/Reader 代理，使 FishNet 在网络传输中使用 MemoryPack 进行序列化。
        /// 此方法会自动调用 <see cref="UnityFormattersInitializer.RegisterAll"/> 确保 MemoryPack
        /// 格式化器已就绪，再注册 FishNet 委托。
        /// </summary>
        public static void RegisterMemoryPackSerializer()
        {
            // 先确保 Unity 类型的 MemoryPack 格式化器已注册
            UnityFormattersInitializer.RegisterAll();

            // 注册预测类型格式化器
            PredictionInputFormatter.Register();

            // 再将这些类型注册到 FishNet 的泛型序列化管道
            RegisterAllKnownTypes();
        }

        /// <summary>
        /// 批量注册所有已知类型到 FishNet 的泛型序列化管道。
        /// 包含 Unity 基础类型（Vector3、Quaternion 等），这些类型已通过
        /// <see cref="UnityFormattersInitializer.RegisterAll"/> 注册了 MemoryPack 格式化器。
        /// </summary>
        public static void RegisterAllKnownTypes()
        {
            RegisterType<Vector2>();
            RegisterType<Vector3>();
            RegisterType<Vector4>();
            RegisterType<Vector2Int>();
            RegisterType<Vector3Int>();
            RegisterType<Quaternion>();
            RegisterType<Color>();
            RegisterType<Color32>();
            RegisterType<Bounds>();
            RegisterType<BoundsInt>();
            RegisterType<Rect>();
            RegisterType<RectInt>();
            RegisterType<Matrix4x4>();
            RegisterType<LayerMask>();
            RegisterType<AnimationCurve>();
            RegisterType<Gradient>();

            // 注册预测相关类型
            RegisterType<PredictionReconcileData<byte>>();
        }

        /// <summary>
        /// 为指定类型注册 FishNet Writer/Reader 代理。
        /// </summary>
        /// <typeparam name="T">要注册的类型，需已注册 MemoryPack 格式化器。</typeparam>
        /// <remarks>
        /// Writer 流程：MemoryPack 序列化为 byte[] → FishNet Writer 写入长度前缀字节数组。
        /// Reader 流程：FishNet Reader 读取长度前缀字节数组 → MemoryPack 反序列化为 T。
        /// </remarks>
        private static void RegisterType<T>()
        {
            GenericWriter<T>.SetWrite(static (Writer writer, T value) =>
            {
                byte[] data = MemoryPackSerializer.Serialize(value);
                writer.WriteBytesAndSize(data);
            });

            GenericReader<T>.SetRead(static (Reader reader) =>
            {
                byte[] data = reader.ReadBytesAndSizeAllocated();
                return MemoryPackSerializer.Deserialize<T>(data);
            });
        }
    }
}
