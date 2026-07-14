#nullable enable

using System;
using System.Buffers;
using NUnit.Framework;
using MemoryPack;
using FixedMathSharp;
using FixedMathSharp.Bounds;
using HN.Framework.Core.Capability.Serialization;

namespace HN.Framework.Core.Tests.Serialization
{
    /// <summary>
    /// FixedMathSharp 格式化器的单元测试，
    /// 覆盖 <see cref="Fixed64"/>、向量、四元数、矩阵、包围盒/球等定点数类型的序列化/反序列化。
    /// </summary>
    [TestFixture]
    public class FixedMathSharpFormattersTests
    {
        [SetUp]
        public void SetUp()
        {
            FormattersInitializer.RegisterAll();
            global::MemoryPack.MemoryPackFormatterProvider.Register(new CompositeTestDataFormatter());
        }

        #region Fixed64Formatter Tests

        [Test]
        public void Serialize_Fixed64_Zero_RoundtripSuccess()
        {
            var original = Fixed64.Zero;

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Fixed64>(bytes);

            Assert.AreEqual(original.m_rawValue, result.m_rawValue);
            Assert.AreEqual(0L, result.m_rawValue);
        }

        [Test]
        public void Serialize_Fixed64_Positive_RoundtripSuccess()
        {
            var original = new Fixed64(12345);

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Fixed64>(bytes);

            Assert.AreEqual(original.m_rawValue, result.m_rawValue);
        }

        [Test]
        public void Serialize_Fixed64_Negative_RoundtripSuccess()
        {
            var original = new Fixed64(-999);

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Fixed64>(bytes);

            Assert.AreEqual(original.m_rawValue, result.m_rawValue);
        }

        [Test]
        public void Serialize_Fixed64_Fraction_RoundtripSuccess()
        {
            var original = Fixed64.FromDouble(3.14159);

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Fixed64>(bytes);

            Assert.AreEqual(original.m_rawValue, result.m_rawValue);
        }

        #endregion

        #region Vector2dFormatter Tests

        [Test]
        public void Serialize_Vector2d_RoundtripSuccess()
        {
            var original = new Vector2d(Fixed64.One, new Fixed64(2));

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Vector2d>(bytes);

            Assert.AreEqual(original.X.m_rawValue, result.X.m_rawValue);
            Assert.AreEqual(original.Y.m_rawValue, result.Y.m_rawValue);
        }

        [Test]
        public void Serialize_Vector2d_Negative_RoundtripSuccess()
        {
            var original = new Vector2d(new Fixed64(-5), new Fixed64(-10));

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Vector2d>(bytes);

            Assert.AreEqual(original.X.m_rawValue, result.X.m_rawValue);
            Assert.AreEqual(original.Y.m_rawValue, result.Y.m_rawValue);
        }

        #endregion

        #region Vector3dFormatter Tests

        [Test]
        public void Serialize_Vector3d_RoundtripSuccess()
        {
            var original = new Vector3d(Fixed64.One, new Fixed64(2), Fixed64.Half);

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Vector3d>(bytes);

            Assert.AreEqual(original.X.m_rawValue, result.X.m_rawValue);
            Assert.AreEqual(original.Y.m_rawValue, result.Y.m_rawValue);
            Assert.AreEqual(original.Z.m_rawValue, result.Z.m_rawValue);
        }

        [Test]
        public void Serialize_Vector3d_Negative_RoundtripSuccess()
        {
            var original = new Vector3d(new Fixed64(-3), new Fixed64(-7), new Fixed64(-13));

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Vector3d>(bytes);

            Assert.AreEqual(original.X.m_rawValue, result.X.m_rawValue);
            Assert.AreEqual(original.Y.m_rawValue, result.Y.m_rawValue);
            Assert.AreEqual(original.Z.m_rawValue, result.Z.m_rawValue);
        }

        #endregion

        #region Vector4dFormatter Tests

        [Test]
        public void Serialize_Vector4d_RoundtripSuccess()
        {
            var original = new Vector4d(Fixed64.One, new Fixed64(2), new Fixed64(3), new Fixed64(4));

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Vector4d>(bytes);

            Assert.AreEqual(original.X.m_rawValue, result.X.m_rawValue);
            Assert.AreEqual(original.Y.m_rawValue, result.Y.m_rawValue);
            Assert.AreEqual(original.Z.m_rawValue, result.Z.m_rawValue);
            Assert.AreEqual(original.W.m_rawValue, result.W.m_rawValue);
        }

        [Test]
        public void Serialize_Vector4d_Negative_RoundtripSuccess()
        {
            var original = new Vector4d(new Fixed64(-1), new Fixed64(-2), new Fixed64(-3), new Fixed64(-4));

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Vector4d>(bytes);

            Assert.AreEqual(original.X.m_rawValue, result.X.m_rawValue);
            Assert.AreEqual(original.Y.m_rawValue, result.Y.m_rawValue);
            Assert.AreEqual(original.Z.m_rawValue, result.Z.m_rawValue);
            Assert.AreEqual(original.W.m_rawValue, result.W.m_rawValue);
        }

        #endregion

        #region FixedQuaternionFormatter Tests

        [Test]
        public void Serialize_FixedQuaternion_Identity_RoundtripSuccess()
        {
            var original = FixedQuaternion.Identity;

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<FixedQuaternion>(bytes);

            Assert.AreEqual(original.X.m_rawValue, result.X.m_rawValue);
            Assert.AreEqual(original.Y.m_rawValue, result.Y.m_rawValue);
            Assert.AreEqual(original.Z.m_rawValue, result.Z.m_rawValue);
            Assert.AreEqual(original.W.m_rawValue, result.W.m_rawValue);
        }

        #endregion

        #region Fixed4x4Formatter Tests

        [Test]
        public void Serialize_Fixed4x4_Identity_RoundtripSuccess()
        {
            var original = Fixed4x4.Identity;

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Fixed4x4>(bytes);

            Assert.AreEqual(original.M11.m_rawValue, result.M11.m_rawValue);
            Assert.AreEqual(original.M12.m_rawValue, result.M12.m_rawValue);
            Assert.AreEqual(original.M13.m_rawValue, result.M13.m_rawValue);
            Assert.AreEqual(original.M14.m_rawValue, result.M14.m_rawValue);
            Assert.AreEqual(original.M21.m_rawValue, result.M21.m_rawValue);
            Assert.AreEqual(original.M22.m_rawValue, result.M22.m_rawValue);
            Assert.AreEqual(original.M23.m_rawValue, result.M23.m_rawValue);
            Assert.AreEqual(original.M24.m_rawValue, result.M24.m_rawValue);
            Assert.AreEqual(original.M31.m_rawValue, result.M31.m_rawValue);
            Assert.AreEqual(original.M32.m_rawValue, result.M32.m_rawValue);
            Assert.AreEqual(original.M33.m_rawValue, result.M33.m_rawValue);
            Assert.AreEqual(original.M34.m_rawValue, result.M34.m_rawValue);
            Assert.AreEqual(original.M41.m_rawValue, result.M41.m_rawValue);
            Assert.AreEqual(original.M42.m_rawValue, result.M42.m_rawValue);
            Assert.AreEqual(original.M43.m_rawValue, result.M43.m_rawValue);
            Assert.AreEqual(original.M44.m_rawValue, result.M44.m_rawValue);
        }

        #endregion

        #region FixedBoundBoxFormatter Tests

        [Test]
        public void Serialize_FixedBoundBox_Default_RoundtripSuccess()
        {
            var original = FixedBoundBox.FromMinMax(
                Vector3d.Zero,
                new Vector3d(Fixed64.One, Fixed64.One, Fixed64.One));

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<FixedBoundBox>(bytes);

            Assert.AreEqual(original.Min.X.m_rawValue, result.Min.X.m_rawValue);
            Assert.AreEqual(original.Min.Y.m_rawValue, result.Min.Y.m_rawValue);
            Assert.AreEqual(original.Min.Z.m_rawValue, result.Min.Z.m_rawValue);
            Assert.AreEqual(original.Max.X.m_rawValue, result.Max.X.m_rawValue);
            Assert.AreEqual(original.Max.Y.m_rawValue, result.Max.Y.m_rawValue);
            Assert.AreEqual(original.Max.Z.m_rawValue, result.Max.Z.m_rawValue);
        }

        #endregion

        #region FixedBoundSphereFormatter Tests

        [Test]
        public void Serialize_FixedBoundSphere_Default_RoundtripSuccess()
        {
            var original = new FixedBoundSphere(
                Vector3d.Zero,
                Fixed64.One);

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<FixedBoundSphere>(bytes);

            Assert.AreEqual(original.Center.X.m_rawValue, result.Center.X.m_rawValue);
            Assert.AreEqual(original.Center.Y.m_rawValue, result.Center.Y.m_rawValue);
            Assert.AreEqual(original.Center.Z.m_rawValue, result.Center.Z.m_rawValue);
            Assert.AreEqual(original.Radius.m_rawValue, result.Radius.m_rawValue);
        }

        #endregion

        #region FormattersInitializer Tests

        [Test]
        public void FormattersInitializer_RegisterAll_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                FormattersInitializer.RegisterAll();
            });
        }

        [Test]
        public void FormattersInitializer_RegisterAll_Idempotent()
        {
            // 第一次调用在 SetUp 中已完成，此处验证重复调用不抛异常
            Assert.DoesNotThrow(() =>
            {
                FormattersInitializer.RegisterAll();
            });
        }

        #endregion

        #region Composite Tests

        [Test]
        public void Serialize_CompositeClass_WithVector3dAndFixed64_RoundtripSuccess()
        {
            var original = new CompositeTestData
            {
                Position = new Vector3d(Fixed64.One, new Fixed64(2), Fixed64.Half),
                Health = new Fixed64(100),
            };

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            Assert.That(bytes, Is.Not.Null);
            Assert.That(bytes.Length, Is.GreaterThan(0));

            var result = MemoryPackSerializer.Deserialize<CompositeTestData>(bytes);
            Assert.That(result, Is.Not.Null);

            Assert.AreEqual(original.Position.X.m_rawValue, result.Position.X.m_rawValue);
            Assert.AreEqual(original.Position.Y.m_rawValue, result.Position.Y.m_rawValue);
            Assert.AreEqual(original.Position.Z.m_rawValue, result.Position.Z.m_rawValue);
            Assert.AreEqual(original.Health.m_rawValue, result.Health.m_rawValue);
        }

        #endregion
    }

    /// <summary>
    /// 用于验证含 FixedMathSharp 类型的复合对象的序列化往返正确性。
    /// </summary>
    public sealed class CompositeTestData
    {
        /// <summary>
        /// 3D 位置向量。
        /// </summary>
        public Vector3d Position { get; set; }

        /// <summary>
        /// 生命值。
        /// </summary>
        public Fixed64 Health { get; set; }
    }

    /// <summary>
    /// <see cref="CompositeTestData"/> 的手动格式化器，
    /// 序列化 <see cref="Vector3d"/> 和 <see cref="Fixed64"/> 的原始 long 值。
    /// </summary>
    public sealed class CompositeTestDataFormatter : MemoryPackFormatter<CompositeTestData>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CompositeTestData value)
        {
            writer.WriteUnmanaged(value.Position.X.m_rawValue, value.Position.Y.m_rawValue, value.Position.Z.m_rawValue);
            writer.WriteUnmanaged(value.Health.m_rawValue);
        }

        public override void Deserialize(ref MemoryPackReader reader, ref CompositeTestData value)
        {
            if (value == null)
            {
                value = new CompositeTestData();
            }

            reader.ReadUnmanaged(out long x, out long y, out long z);
            reader.ReadUnmanaged(out long health);
            value.Position = new Vector3d(new Fixed64(x), new Fixed64(y), new Fixed64(z));
            value.Health = new Fixed64(health);
        }
    }
}
