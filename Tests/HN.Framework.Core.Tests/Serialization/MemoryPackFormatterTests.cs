#nullable enable

using System;
using System.Buffers;
using NUnit.Framework;
using MemoryPack;
using HN.Framework.Core.Capability.Serialization;

namespace HN.Framework.Core.Tests.Serialization
{
    /// <summary>
    /// <see cref="MemoryPackFormatterProvider"/> 和 <see cref="ISerializer"/> 的单元测试，
    /// 覆盖格式化器注册/查找以及非泛型序列化接口。
    /// </summary>
    [TestFixture]
    public class MemoryPackFormatterTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void RegisterAndGetFormatter_Generic_Success()
        {
            var formatter = new TestIntFormatter();

            MemoryPackFormatterProvider.Register<int>(formatter);
            var retrieved = MemoryPackFormatterProvider.GetFormatter<int>();

            Assert.That(retrieved, Is.SameAs(formatter));
        }

        [Test]
        public void Register_NonMemoryPackFormatter_Throws()
        {
            var formatter = new FakeInterfaceFormatter();

            Assert.Throws<ArgumentException>(() =>
            {
                MemoryPackFormatterProvider.Register<int>(formatter);
            });
        }

        [Test]
        public void ISerializer_NonGeneric_SerializeDeserialize()
        {
            var serializer = new MemoryPackSerializerImpl();
            const int expected = 42;

            var data = serializer.Serialize(typeof(int), expected);
            Assert.That(data, Is.Not.Null);
            Assert.That(data.Length, Is.GreaterThan(0));

            var result = serializer.Deserialize(typeof(int), data);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<int>());
            Assert.That((int)result, Is.EqualTo(expected));
        }

        /// <summary>
        /// <see cref="ISerializer"/> 实现，委托给 vendored
        /// <c>global::MemoryPack.MemoryPackSerializer</c>，用于非泛型序列化测试。
        /// </summary>
        private sealed class MemoryPackSerializerImpl : ISerializer
        {
            public byte[] Serialize<T>(T obj)
            {
                return global::MemoryPack.MemoryPackSerializer.Serialize(obj);
            }

            public T Deserialize<T>(byte[] data)
            {
                return global::MemoryPack.MemoryPackSerializer.Deserialize<T>(data)!;
            }

            public byte[] Serialize(Type type, object obj)
            {
                return global::MemoryPack.MemoryPackSerializer.Serialize(type, obj);
            }

            public object Deserialize(Type type, byte[] data)
            {
                return global::MemoryPack.MemoryPackSerializer.Deserialize(type, data)!;
            }
        }

        /// <summary>
        /// 继承 <see cref="MemoryPackFormatter{T}"/> 的测试格式化器，
        /// 用于验证格式化器注册与检索机制。
        /// </summary>
        private sealed class TestIntFormatter : MemoryPackFormatter<int>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref int value)
            {
            }

            public override void Deserialize(ref MemoryPackReader reader, scoped ref int value)
            {
            }
        }

        /// <summary>
        /// 仅实现 <see cref="IMemoryPackFormatter{T}"/> 但未继承
        /// <see cref="MemoryPackFormatter{T}"/> 的格式化器，
        /// 用于验证 <see cref="MemoryPackFormatterProvider.Register{T}"/> 的类型检查。
        /// </summary>
        private sealed class FakeInterfaceFormatter : IMemoryPackFormatter<int>
        {
            public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref int value)
                where TBufferWriter : class, IBufferWriter<byte>
            {
            }

            public void Deserialize(ref MemoryPackReader reader, scoped ref int value)
            {
            }
        }
    }
}
