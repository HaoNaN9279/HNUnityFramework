using System;
using System.Buffers;
using HN.Framework.Unity.Capability.Serialization;
using MemoryPack;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Serialization
{
    /// <summary>
    /// EditMode 单元测试，覆盖 MemoryPack UnityFormatters 的序列化往返验证。
    /// 测试所有 Unity 内置类型的 MemoryPack 格式化器（Vector3, Quaternion, Color, Matrix4x4,
    /// Vector2Int, Bounds, AnimationCurve, Gradient 等）。
    /// </summary>
    [TestFixture]
    public class UnityFormattersTests
    {
        #region Helper Types

        /// <summary>
        /// 用于测试复合 Unity 类型的 MemoryPack 序列化数据类。
        /// 使用公共字段以兼容 MemoryPack 的反射回退序列化。
        /// </summary>
        public class TestUnityData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Color Color;
            public Vector3 Scale;
        }

        /// <summary>
        /// TestUnityData 的手动 MemoryPack 格式化器。
        /// 因 MemoryPack 源码生成器未在测试程序集运行，需手动注册此格式化器。
        /// </summary>
        public sealed class TestUnityDataFormatter : MemoryPackFormatter<TestUnityData>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TestUnityData? value)
            {
                if (value == null) return;

                writer.WriteValue(value.Position);
                writer.WriteValue(value.Rotation);
                writer.WriteValue(value.Color);
                writer.WriteValue(value.Scale);
            }

            public override void Deserialize(ref MemoryPackReader reader, ref TestUnityData? value)
            {
                value ??= new TestUnityData();
                value.Position = reader.ReadValue<Vector3>();
                value.Rotation = reader.ReadValue<Quaternion>();
                value.Color = reader.ReadValue<Color>();
                value.Scale = reader.ReadValue<Vector3>();
            }
        }

        /// <summary>
        /// 用于测试包含可空 Unity 类型的复合数据类。
        /// </summary>
        public class ExtendedUnityData
        {
            public Vector3? NullablePosition;
            public Vector2Int GridCoord;
            public Color32 Color32;
        }

        #endregion

        /// <summary>
        /// 每个测试前注册所有 Unity 格式化器，确保 MemoryPack 可以序列化 Unity 类型。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            UnityFormattersInitializer.RegisterAll();
            MemoryPackFormatterProvider.Register(new TestUnityDataFormatter());
        }

        /// <summary>
        /// 空清理方法，保持与项目测试风格的统一性。
        /// 本测试未创建临时 GameObject，因此无需额外资源清理。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            // 无临时对象需要清理
        }

        // ====================================================================
        // 1. Vector3 Roundtrip
        // ====================================================================

        [Test]
        public void Serialize_Vector3_RoundtripSuccess()
        {
            var original = new Vector3(1.5f, 2.5f, 3.5f);
            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Vector3>(bytes);

            Assert.AreEqual(original.x, result.x, 0.0001f);
            Assert.AreEqual(original.y, result.y, 0.0001f);
            Assert.AreEqual(original.z, result.z, 0.0001f);
        }

        // ====================================================================
        // 2. Quaternion Roundtrip
        // ====================================================================

        [Test]
        public void Serialize_Quaternion_RoundtripSuccess()
        {
            var original = Quaternion.identity;
            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Quaternion>(bytes);

            Assert.AreEqual(original.x, result.x, 0.0001f);
            Assert.AreEqual(original.y, result.y, 0.0001f);
            Assert.AreEqual(original.z, result.z, 0.0001f);
            Assert.AreEqual(original.w, result.w, 0.0001f);
        }

        // ====================================================================
        // 3. Color Roundtrip
        // ====================================================================

        [Test]
        public void Serialize_Color_RoundtripSuccess()
        {
            var original = new Color(0.2f, 0.5f, 0.8f, 0.5f);
            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Color>(bytes);

            Assert.AreEqual(original.r, result.r, 0.0001f);
            Assert.AreEqual(original.g, result.g, 0.0001f);
            Assert.AreEqual(original.b, result.b, 0.0001f);
            Assert.AreEqual(original.a, result.a, 0.0001f);
        }

        // ====================================================================
        // 4. Matrix4x4 Roundtrip
        // ====================================================================

        [Test]
        public void Serialize_Matrix4x4_RoundtripSuccess()
        {
            var original = Matrix4x4.identity;
            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Matrix4x4>(bytes);

            Assert.AreEqual(original.m00, result.m00, 0.0001f);
            Assert.AreEqual(original.m11, result.m11, 0.0001f);
            Assert.AreEqual(original.m22, result.m22, 0.0001f);
            Assert.AreEqual(original.m33, result.m33, 0.0001f);
            // 验证非对角元素为零
            Assert.AreEqual(0f, result.m01, 0.0001f);
            Assert.AreEqual(0f, result.m02, 0.0001f);
            Assert.AreEqual(0f, result.m10, 0.0001f);
        }

        // ====================================================================
        // 5. Vector3 Negative Values
        // ====================================================================

        [Test]
        public void Serialize_Vector3_NegativeValues()
        {
            var original = new Vector3(-10.5f, -0.001f, -99.9f);
            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Vector3>(bytes);

            Assert.AreEqual(original.x, result.x, 0.0001f);
            Assert.AreEqual(original.y, result.y, 0.0001f);
            Assert.AreEqual(original.z, result.z, 0.0001f);
        }

        // ====================================================================
        // 6. Composite Object with Multiple Unity Types
        // ====================================================================

        [Test]
        public void Serialize_AllUnityTypes_CompositeObject()
        {
            var original = new TestUnityData
            {
                Position = new Vector3(10f, 20f, 30f),
                Rotation = Quaternion.Euler(45f, 90f, 0f),
                Color = Color.red,
                Scale = new Vector3(1f, 2f, 3f)
            };

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<TestUnityData>(bytes);

            Assert.IsNotNull(result);
            Assert.AreEqual(original.Position.x, result.Position.x, 0.0001f);
            Assert.AreEqual(original.Position.y, result.Position.y, 0.0001f);
            Assert.AreEqual(original.Position.z, result.Position.z, 0.0001f);
            Assert.AreEqual(original.Rotation.x, result.Rotation.x, 0.0001f);
            Assert.AreEqual(original.Rotation.y, result.Rotation.y, 0.0001f);
            Assert.AreEqual(original.Rotation.z, result.Rotation.z, 0.0001f);
            Assert.AreEqual(original.Rotation.w, result.Rotation.w, 0.0001f);
            Assert.AreEqual(original.Color.r, result.Color.r, 0.0001f);
            Assert.AreEqual(original.Color.g, result.Color.g, 0.0001f);
            Assert.AreEqual(original.Color.b, result.Color.b, 0.0001f);
            Assert.AreEqual(original.Scale.x, result.Scale.x, 0.0001f);
            Assert.AreEqual(original.Scale.y, result.Scale.y, 0.0001f);
            Assert.AreEqual(original.Scale.z, result.Scale.z, 0.0001f);
        }

        // ====================================================================
        // 7. Registration Does Not Throw
        // ====================================================================

        [Test]
        public void UnityFormattersInitializer_RegisterAll_DoesNotThrow()
        {
            // 已在 SetUp 中调用，此处再次调用验证幂等性和无异常
            Assert.DoesNotThrow(() =>
            {
                UnityFormattersInitializer.RegisterAll();
            });
        }

        // ====================================================================
        // 8. Vector2Int Roundtrip
        // ====================================================================

        [Test]
        public void Serialize_Vector2Int_RoundtripSuccess()
        {
            var original = new Vector2Int(128, 256);
            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Vector2Int>(bytes);

            Assert.AreEqual(original.x, result.x);
            Assert.AreEqual(original.y, result.y);
        }

        // ====================================================================
        // 9. Bounds Roundtrip
        // ====================================================================

        [Test]
        public void Serialize_Bounds_RoundtripSuccess()
        {
            var original = new Bounds(
                new Vector3(1f, 2f, 3f),
                new Vector3(4f, 5f, 6f));

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Bounds>(bytes);

            Assert.AreEqual(original.center.x, result.center.x, 0.0001f);
            Assert.AreEqual(original.center.y, result.center.y, 0.0001f);
            Assert.AreEqual(original.center.z, result.center.z, 0.0001f);
            Assert.AreEqual(original.size.x, result.size.x, 0.0001f);
            Assert.AreEqual(original.size.y, result.size.y, 0.0001f);
            Assert.AreEqual(original.size.z, result.size.z, 0.0001f);
        }

        // ====================================================================
        // 10. AnimationCurve Roundtrip
        // ====================================================================

        [Test]
        public void Serialize_AnimationCurve_RoundtripSuccess()
        {
            var original = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f))
            {
                preWrapMode = WrapMode.Clamp,
                postWrapMode = WrapMode.Clamp
            };

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<AnimationCurve>(bytes);

            Assert.IsNotNull(result);
            Assert.AreEqual(original.keys.Length, result.keys.Length);
            Assert.AreEqual((int)original.preWrapMode, (int)result.preWrapMode);
            Assert.AreEqual((int)original.postWrapMode, (int)result.postWrapMode);

            for (int i = 0; i < original.keys.Length; i++)
            {
                Assert.AreEqual(original.keys[i].time, result.keys[i].time, 0.0001f);
                Assert.AreEqual(original.keys[i].value, result.keys[i].value, 0.0001f);
            }
        }

        // ====================================================================
        // 11. Gradient Roundtrip
        // ====================================================================

        [Test]
        public void Serialize_Gradient_RoundtripSuccess()
        {
            var original = new Gradient();
            original.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.red, 0f),
                    new GradientColorKey(Color.blue, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });

            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Gradient>(bytes);

            Assert.IsNotNull(result);
            Assert.AreEqual(original.colorKeys.Length, result.colorKeys.Length);
            Assert.AreEqual(original.alphaKeys.Length, result.alphaKeys.Length);

            for (int i = 0; i < original.colorKeys.Length; i++)
            {
                Assert.AreEqual(original.colorKeys[i].time, result.colorKeys[i].time, 0.0001f);
                Assert.AreEqual(original.colorKeys[i].color.r, result.colorKeys[i].color.r, 0.0001f);
                Assert.AreEqual(original.colorKeys[i].color.g, result.colorKeys[i].color.g, 0.0001f);
                Assert.AreEqual(original.colorKeys[i].color.b, result.colorKeys[i].color.b, 0.0001f);
            }

            for (int i = 0; i < original.alphaKeys.Length; i++)
            {
                Assert.AreEqual(original.alphaKeys[i].time, result.alphaKeys[i].time, 0.0001f);
                Assert.AreEqual(original.alphaKeys[i].alpha, result.alphaKeys[i].alpha, 0.0001f);
            }
        }
    }
}
