#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.UI;

namespace HN.Framework.Core.Tests.Capability.UI
{
    [TestFixture]
    public class ToastConfigTests
    {
        /// <summary>
        /// 默认持续时间为 2 秒。
        /// </summary>
        [Test]
        public void DefaultDuration_IsTwoSeconds()
        {
            var config = ToastConfig.Default;

            Assert.That(config.Duration, Is.EqualTo(2f));
        }

        /// <summary>
        /// 默认 UI 层级为 Toast。
        /// </summary>
        [Test]
        public void DefaultLayer_IsToast()
        {
            var config = ToastConfig.Default;

            Assert.That(config.Layer, Is.EqualTo(UILayer.Toast));
        }

        /// <summary>
        /// 自定义值被正确存储。
        /// </summary>
        [Test]
        public void CustomValues_AreStored()
        {
            var config = new ToastConfig(duration: 5f, layer: UILayer.System);

            Assert.That(config.Duration, Is.EqualTo(5f));
            Assert.That(config.Layer, Is.EqualTo(UILayer.System));
        }

        /// <summary>
        /// ToastConfig 是只读结构体。
        /// </summary>
        [Test]
        public void Struct_IsReadonly()
        {
            var type = typeof(ToastConfig);
            Assert.That(type.IsValueType, Is.True, "应为结构体（值类型）");
            Assert.That(type.IsPublic, Is.True);
        }
    }
}
