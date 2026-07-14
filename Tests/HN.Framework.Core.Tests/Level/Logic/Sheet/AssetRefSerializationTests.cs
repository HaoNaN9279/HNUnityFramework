#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Driver.Common.Serialization;
using HN.Framework.Core.Level.Logic.Sheet;

namespace HN.Framework.Core.Tests.Level.Logic.Sheet
{
    /// <summary>
    /// <see cref="AssetRef{T}"/> 序列化与相等性单元测试。
    /// </summary>
    [TestFixture]
    public class AssetRefSerializationTests
    {
        [SetUp]
        public void SetUp()
        {
            global::MemoryPack.MemoryPackFormatterProvider.Register(new AssetRefIntFormatter());
        }

        [TearDown]
        public void TearDown()
        {
        }

        /// <summary>
        /// AssetRef&lt;int&gt; 的手动 MemoryPack 格式化器。
        /// </summary>
        private sealed class AssetRefIntFormatter : global::MemoryPack.MemoryPackFormatter<AssetRef<int>>
        {
            public override void Serialize<TBufferWriter>(ref global::MemoryPack.MemoryPackWriter<TBufferWriter> writer, scoped ref AssetRef<int> value)
            {
                writer.WriteString(value.Label);
            }

            public override void Deserialize(ref global::MemoryPack.MemoryPackReader reader, scoped ref AssetRef<int> value)
            {
                value.Label = reader.ReadString() ?? string.Empty;
            }
        }

        [Test]
        public void Serialize_Deserialize_Roundtrip()
        {
            var original = new AssetRef<int> { Label = "test" };

            var data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<AssetRef<int>>(data);

            Assert.That(result.Label, Is.EqualTo("test"));
        }

        [Test]
        public void Empty_IsValid_False()
        {
            var empty = AssetRef<int>.Empty;

            Assert.That(empty.IsValid, Is.False);
        }

        [Test]
        public void NonEmpty_IsValid_True()
        {
            var assetRef = new AssetRef<int> { Label = "x" };

            Assert.That(assetRef.IsValid, Is.True);
        }

        [Test]
        public void Equals_SameLabel_ReturnsTrue()
        {
            var a = new AssetRef<int> { Label = "common" };
            var b = new AssetRef<int> { Label = "common" };

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a == b, Is.True);
        }

        [Test]
        public void Equals_DifferentLabel_ReturnsFalse()
        {
            var a = new AssetRef<int> { Label = "alpha" };
            var b = new AssetRef<int> { Label = "beta" };

            Assert.That(a.Equals(b), Is.False);
            Assert.That(a == b, Is.False);
            Assert.That(a != b, Is.True);
        }

        [Test]
        public void Default_Is_Empty()
        {
            var defaultRef = default(AssetRef<int>);
            var empty = AssetRef<int>.Empty;

            Assert.That(defaultRef.Equals(empty), Is.True);
            Assert.That(defaultRef == empty, Is.True);
        }
    }
}
