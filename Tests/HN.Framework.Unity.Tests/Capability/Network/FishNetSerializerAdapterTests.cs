#nullable enable

using HN.Framework.Unity.Capability.Network;
using NUnit.Framework;

namespace HN.Framework.Unity.Tests.Capability.Network
{
    /// <summary>
    /// FishNetSerializerAdapter 的 EditMode 测试。验证序列化适配器注册不抛出异常。
    /// </summary>
    [TestFixture]
    public class FishNetSerializerAdapterTests
    {
        /// <summary>
        /// 调用 RegisterMemoryPackSerializer 不抛出异常，验证 FishNet + MemoryPack 集成正常。
        /// </summary>
        [Test]
        public void RegisterMemoryPackSerializer_DoesNotThrow()
        {
            Assert.That(
                () => FishNetSerializerAdapter.RegisterMemoryPackSerializer(),
                Throws.Nothing,
                "RegisterMemoryPackSerializer should register without exceptions.");
        }

        /// <summary>
        /// 调用 RegisterAllKnownTypes 不抛出异常，验证 Unity 类型注册正常。
        /// </summary>
        [Test]
        public void RegisterAllKnownTypes_RegistersUnityTypes()
        {
            Assert.That(
                () => FishNetSerializerAdapter.RegisterAllKnownTypes(),
                Throws.Nothing,
                "RegisterAllKnownTypes should register Unity types without exceptions.");
        }
    }
}
