#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.UI;

namespace HN.Framework.Core.Tests.Capability.UI
{
    [TestFixture]
    public class UILayerTests
    {
        /// <summary>
        /// 验证 Background 层的序号为 0。
        /// </summary>
        [Test]
        public void Background_HasValue_Zero()
        {
            Assert.AreEqual(0, (int)UILayer.Background);
        }

        /// <summary>
        /// 验证 Scene 层的序号为 100。
        /// </summary>
        [Test]
        public void Scene_HasValue_100()
        {
            Assert.AreEqual(100, (int)UILayer.Scene);
        }

        /// <summary>
        /// 验证 UI 层的序号为 200。
        /// </summary>
        [Test]
        public void UI_HasValue_200()
        {
            Assert.AreEqual(200, (int)UILayer.UI);
        }

        /// <summary>
        /// 验证 Popup 层的序号为 300。
        /// </summary>
        [Test]
        public void Popup_HasValue_300()
        {
            Assert.AreEqual(300, (int)UILayer.Popup);
        }

        /// <summary>
        /// 验证 Toast 层的序号为 400。
        /// </summary>
        [Test]
        public void Toast_HasValue_400()
        {
            Assert.AreEqual(400, (int)UILayer.Toast);
        }

        /// <summary>
        /// 验证 Guide 层的序号为 500。
        /// </summary>
        [Test]
        public void Guide_HasValue_500()
        {
            Assert.AreEqual(500, (int)UILayer.Guide);
        }

        /// <summary>
        /// 验证 System 层的序号为 600。
        /// </summary>
        [Test]
        public void System_HasValue_600()
        {
            Assert.AreEqual(600, (int)UILayer.System);
        }

        /// <summary>
        /// 验证枚举值总数为 7。
        /// </summary>
        [Test]
        public void Enum_Count_IsSeven()
        {
            string[] names = System.Enum.GetNames(typeof(UILayer));
            Assert.AreEqual(7, names.Length);
        }

        /// <summary>
        /// 验证各层的 SortOrder 值严格递增。
        /// </summary>
        [Test]
        public void SortOrder_Values_AreStrictlyIncreasing()
        {
            int background = (int)UILayer.Background;
            int scene = (int)UILayer.Scene;
            int ui = (int)UILayer.UI;
            int popup = (int)UILayer.Popup;
            int toast = (int)UILayer.Toast;
            int guide = (int)UILayer.Guide;
            int system = (int)UILayer.System;

            Assert.That(background, Is.LessThan(scene));
            Assert.That(scene, Is.LessThan(ui));
            Assert.That(ui, Is.LessThan(popup));
            Assert.That(popup, Is.LessThan(toast));
            Assert.That(toast, Is.LessThan(guide));
            Assert.That(guide, Is.LessThan(system));
        }
    }
}
