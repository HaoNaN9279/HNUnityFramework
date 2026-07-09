#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.UI;

namespace HN.Framework.Core.Tests.Capability.UI
{
    [TestFixture]
    public class GuideStepTests
    {
        /// <summary>
        /// 构造函数设置所有属性。
        /// </summary>
        [Test]
        public void Constructor_SetsAllProperties()
        {
            var step = new GuideStep(
                stepId: "step_1",
                targetName: "Button_Confirm",
                description: "点击确认按钮",
                highlightOffsetX: 10f,
                highlightOffsetY: 20f,
                highlightSizeWidth: 300f,
                highlightSizeHeight: 150f);

            Assert.That(step.StepId, Is.EqualTo("step_1"));
            Assert.That(step.TargetName, Is.EqualTo("Button_Confirm"));
            Assert.That(step.Description, Is.EqualTo("点击确认按钮"));
            Assert.That(step.HighlightOffsetX, Is.EqualTo(10f));
            Assert.That(step.HighlightOffsetY, Is.EqualTo(20f));
            Assert.That(step.HighlightSizeWidth, Is.EqualTo(300f));
            Assert.That(step.HighlightSizeHeight, Is.EqualTo(150f));
        }

        /// <summary>
        /// 高亮偏移量默认为 0。
        /// </summary>
        [Test]
        public void HighlightOffsets_DefaultToZero()
        {
            var step = new GuideStep("step_1", "Target", "描述");

            Assert.That(step.HighlightOffsetX, Is.EqualTo(0f));
            Assert.That(step.HighlightOffsetY, Is.EqualTo(0f));
        }

        /// <summary>
        /// 高亮尺寸默认为 200。
        /// </summary>
        [Test]
        public void HighlightSize_DefaultTo200()
        {
            var step = new GuideStep("step_1", "Target", "描述");

            Assert.That(step.HighlightSizeWidth, Is.EqualTo(200f));
            Assert.That(step.HighlightSizeHeight, Is.EqualTo(200f));
        }
    }
}
