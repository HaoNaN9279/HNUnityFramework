#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using HN.Framework.Core.Driver.Common.Serialization;

namespace HN.Framework.Core.Tests.Serialization
{
    /// <summary>
    /// <see cref="MemoryPackSerializer"/> 包装器的单元测试，
    /// 覆盖基本类型、字符串、复杂对象、流、大数据量等场景。
    /// </summary>
    [TestFixture]
    public class MemoryPackSerializerTests
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
        public void Serialize_Int32_RoundtripSuccess()
        {
            const int expected = 42;

            var data = MemoryPackSerializer.Serialize(expected);
            var result = MemoryPackSerializer.Deserialize<int>(data);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Serialize_String_RoundtripSuccess()
        {
            const string expected = "Hello, MemoryPack!";

            var data = MemoryPackSerializer.Serialize(expected);
            var result = MemoryPackSerializer.Deserialize<string>(data);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Serialize_NullString_HandlesCorrectly()
        {
            string? value = null;

            var data = MemoryPackSerializer.Serialize(value);
            var result = MemoryPackSerializer.Deserialize<string>(data);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Serialize_ComplexObject_RoundtripSuccess()
        {
            var expected = new Dictionary<string, List<int>>
            {
                ["a"] = new List<int> { 1, 2, 3 },
                ["b"] = new List<int> { 4, 5 },
                ["c"] = new List<int>()
            };

            var data = MemoryPackSerializer.Serialize(expected);
            var result = MemoryPackSerializer.Deserialize<Dictionary<string, List<int>>>(data);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result["a"], Is.EquivalentTo(new[] { 1, 2, 3 }));
            Assert.That(result["b"], Is.EquivalentTo(new[] { 4, 5 }));
            Assert.That(result["c"], Is.Empty);
        }

        [Test]
        public void Serialize_EmptyArray_RoundtripSuccess()
        {
            var expected = Array.Empty<int>();

            var data = MemoryPackSerializer.Serialize(expected);
            var result = MemoryPackSerializer.Deserialize<int[]>(data);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(0));
        }

        [Test]
        public void Serialize_LargeData_PerformanceAcceptable()
        {
            var expected = new int[10000];
            for (int i = 0; i < expected.Length; i++)
            {
                expected[i] = i;
            }

            var data = MemoryPackSerializer.Serialize(expected);
            var result = MemoryPackSerializer.Deserialize<int[]>(data);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(expected.Length));
            Assert.That(result[0], Is.EqualTo(0));
            Assert.That(result[expected.Length - 1], Is.EqualTo(expected.Length - 1));
        }

        [Test]
        public void Deserialize_InvalidData_ThrowsException()
        {
            var invalidData = new byte[] { 0x01, 0x02 };

            Assert.Throws<global::MemoryPack.MemoryPackSerializationException>(() =>
            {
                MemoryPackSerializer.Deserialize<int>(invalidData);
            });
        }

        [Test]
        public void Serialize_ToStream_RoundtripSuccess()
        {
            const int expected = 99;

            using var stream = new MemoryStream();
            MemoryPackSerializer.Serialize(stream, expected);

            stream.Position = 0;
            var result = MemoryPackSerializer.Deserialize<int>(stream);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Deserialize_Overwrite_RoundtripSuccess()
        {
            const int expected = 77;
            var data = MemoryPackSerializer.Serialize(expected);

            int? value = 0;
            global::MemoryPack.MemoryPackSerializer.Deserialize<int>(data, ref value);

            Assert.That(value, Is.EqualTo(expected));
        }
    }

    /// <summary>
    /// 用 <c>[MemoryPackable]</c> 标记的测试数据类，
    /// 演示 MemoryPack 序列化的数据模型模式。
    /// </summary>
    [global::MemoryPack.MemoryPackable]
    public partial class TestData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<int>? Values { get; set; }
        public Dictionary<string, int>? Metadata { get; set; }
    }
}
