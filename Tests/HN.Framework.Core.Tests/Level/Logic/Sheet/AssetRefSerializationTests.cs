#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Driver.Common.Serialization;
using HN.Framework.Core.Level.Logic.Sheet;
using HN.Framework.Core.Capability.Serialization;

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
            AssetRefFormatters.RegisterAll();
        }

        [TearDown]
        public void TearDown()
        {
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

        [Test]
        public void Serialize_Deserialize_Roundtrip_StringType()
        {
            var original = new AssetRef<string> { Label = "path/to/asset" };

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<AssetRef<string>>(data);

            Assert.That(result.Label, Is.EqualTo("path/to/asset"));
        }

        [Test]
        public void Serialize_Deserialize_EmptyLabel()
        {
            var original = new AssetRef<int> { Label = string.Empty };

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<AssetRef<int>>(data);

            Assert.That(result.Label, Is.EqualTo(string.Empty));
            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void Serialize_Deserialize_NullLabel()
        {
            var original = new AssetRef<int> { Label = null };

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<AssetRef<int>>(data);

            Assert.That(result.Label, Is.EqualTo(string.Empty));
        }
    }
}
