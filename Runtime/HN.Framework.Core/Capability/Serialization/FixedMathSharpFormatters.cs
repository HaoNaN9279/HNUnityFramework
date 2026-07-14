using MemoryPack;
using FixedMathSharp;
using FixedMathSharp.Bounds;

namespace HN.Framework.Core.Capability.Serialization
{
    /// <summary>
    /// Fixed64 格式化器。
    /// 序列化底层 <c>m_rawValue</c> 的 long 值。
    /// </summary>
    public sealed class Fixed64Formatter : MemoryPackFormatter<Fixed64>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Fixed64 value)
        {
            writer.WriteUnmanaged(value.m_rawValue);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Fixed64 value)
        {
            reader.ReadUnmanaged(out long raw);
            value = new Fixed64(raw);
        }
    }

    /// <summary>
    /// Vector2d 格式化器。
    /// 序列化 X、Y 分量的原始 long 值。
    /// </summary>
    public sealed class Vector2dFormatter : MemoryPackFormatter<Vector2d>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Vector2d value)
        {
            writer.WriteUnmanaged(value.X.m_rawValue, value.Y.m_rawValue);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Vector2d value)
        {
            reader.ReadUnmanaged(out long rawX, out long rawY);
            value = new Vector2d(new Fixed64(rawX), new Fixed64(rawY));
        }
    }

    /// <summary>
    /// Vector3d 格式化器。
    /// 序列化 X、Y、Z 分量的原始 long 值。
    /// </summary>
    public sealed class Vector3dFormatter : MemoryPackFormatter<Vector3d>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Vector3d value)
        {
            writer.WriteUnmanaged(value.X.m_rawValue, value.Y.m_rawValue, value.Z.m_rawValue);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Vector3d value)
        {
            reader.ReadUnmanaged(out long rawX, out long rawY, out long rawZ);
            value = new Vector3d(new Fixed64(rawX), new Fixed64(rawY), new Fixed64(rawZ));
        }
    }

    /// <summary>
    /// Vector4d 格式化器。
    /// 序列化 X、Y、Z、W 分量的原始 long 值。
    /// </summary>
    public sealed class Vector4dFormatter : MemoryPackFormatter<Vector4d>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Vector4d value)
        {
            writer.WriteUnmanaged(value.X.m_rawValue, value.Y.m_rawValue, value.Z.m_rawValue, value.W.m_rawValue);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Vector4d value)
        {
            reader.ReadUnmanaged(out long rawX, out long rawY, out long rawZ, out long rawW);
            value = new Vector4d(new Fixed64(rawX), new Fixed64(rawY), new Fixed64(rawZ), new Fixed64(rawW));
        }
    }

    /// <summary>
    /// FixedQuaternion 格式化器。
    /// 序列化 X、Y、Z、W 分量的原始 long 值。
    /// </summary>
    public sealed class FixedQuaternionFormatter : MemoryPackFormatter<FixedQuaternion>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref FixedQuaternion value)
        {
            writer.WriteUnmanaged(value.X.m_rawValue, value.Y.m_rawValue, value.Z.m_rawValue, value.W.m_rawValue);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref FixedQuaternion value)
        {
            reader.ReadUnmanaged(out long rawX, out long rawY, out long rawZ, out long rawW);
            value = new FixedQuaternion(new Fixed64(rawX), new Fixed64(rawY), new Fixed64(rawZ), new Fixed64(rawW));
        }
    }

    /// <summary>
    /// Fixed4x4 格式化器。
    /// 按行顺序序列化全部 16 个矩阵元素的原始 long 值。
    /// </summary>
    public sealed class Fixed4x4Formatter : MemoryPackFormatter<Fixed4x4>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Fixed4x4 value)
        {
            writer.WriteUnmanaged(value.M11.m_rawValue, value.M12.m_rawValue, value.M13.m_rawValue, value.M14.m_rawValue);
            writer.WriteUnmanaged(value.M21.m_rawValue, value.M22.m_rawValue, value.M23.m_rawValue, value.M24.m_rawValue);
            writer.WriteUnmanaged(value.M31.m_rawValue, value.M32.m_rawValue, value.M33.m_rawValue, value.M34.m_rawValue);
            writer.WriteUnmanaged(value.M41.m_rawValue, value.M42.m_rawValue, value.M43.m_rawValue, value.M44.m_rawValue);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Fixed4x4 value)
        {
            reader.ReadUnmanaged(out long m11, out long m12, out long m13, out long m14);
            reader.ReadUnmanaged(out long m21, out long m22, out long m23, out long m24);
            reader.ReadUnmanaged(out long m31, out long m32, out long m33, out long m34);
            reader.ReadUnmanaged(out long m41, out long m42, out long m43, out long m44);
            value = new Fixed4x4(
                new Fixed64(m11), new Fixed64(m12), new Fixed64(m13), new Fixed64(m14),
                new Fixed64(m21), new Fixed64(m22), new Fixed64(m23), new Fixed64(m24),
                new Fixed64(m31), new Fixed64(m32), new Fixed64(m33), new Fixed64(m34),
                new Fixed64(m41), new Fixed64(m42), new Fixed64(m43), new Fixed64(m44));
        }
    }

    /// <summary>
    /// FixedBoundBox 格式化器。
    /// 序列化 Min 和 Max 两个 Vector3d 的原始 long 值。
    /// </summary>
    public sealed class FixedBoundBoxFormatter : MemoryPackFormatter<FixedBoundBox>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref FixedBoundBox value)
        {
            writer.WriteUnmanaged(value.Min.X.m_rawValue, value.Min.Y.m_rawValue, value.Min.Z.m_rawValue);
            writer.WriteUnmanaged(value.Max.X.m_rawValue, value.Max.Y.m_rawValue, value.Max.Z.m_rawValue);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref FixedBoundBox value)
        {
            reader.ReadUnmanaged(out long minX, out long minY, out long minZ);
            reader.ReadUnmanaged(out long maxX, out long maxY, out long maxZ);
            value = FixedBoundBox.FromMinMax(
                new Vector3d(new Fixed64(minX), new Fixed64(minY), new Fixed64(minZ)),
                new Vector3d(new Fixed64(maxX), new Fixed64(maxY), new Fixed64(maxZ)));
        }
    }

    /// <summary>
    /// FixedBoundSphere 格式化器。
    /// 序列化 Center（Vector3d）和 Radius（Fixed64）的原始 long 值。
    /// </summary>
    public sealed class FixedBoundSphereFormatter : MemoryPackFormatter<FixedBoundSphere>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref FixedBoundSphere value)
        {
            writer.WriteUnmanaged(value.Center.X.m_rawValue, value.Center.Y.m_rawValue, value.Center.Z.m_rawValue);
            writer.WriteUnmanaged(value.Radius.m_rawValue);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref FixedBoundSphere value)
        {
            reader.ReadUnmanaged(out long cx, out long cy, out long cz);
            reader.ReadUnmanaged(out long radiusRaw);
            value = new FixedBoundSphere(
                new Vector3d(new Fixed64(cx), new Fixed64(cy), new Fixed64(cz)),
                new Fixed64(radiusRaw));
        }
    }
}
