#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Sheet;
using HN.Framework.Unity.Capability.Sheet;

namespace HN.Framework.Unity.Tests.Capability.Sheet
{
    /// <summary>
    /// <see cref="AssetRefExtensions"/> 的单元测试。
    /// 验证扩展方法的行为约束，不触发实际的 Addressables 加载。
    /// </summary>
    [TestFixture]
    public class AddressableResolverTests
    {
        [Test]
        public void LoadAssetAsync_InvalidRef_ThrowsInvalidOperationException()
        {
            var invalidRef = AssetRef<UnityEngine.Object>.Empty;

            Assert.That(
                () => invalidRef.LoadAssetAsync<UnityEngine.Object>(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void ReleaseAsset_NullArgument_DoesNotThrow()
        {
            var validRef = new AssetRef<UnityEngine.Object> { Label = "test" };

            Assert.DoesNotThrow(() => validRef.ReleaseAsset(null));
        }
    }
}
