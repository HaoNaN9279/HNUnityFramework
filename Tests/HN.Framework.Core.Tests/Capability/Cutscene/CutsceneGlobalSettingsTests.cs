using HN.Framework.Core.Capability.Cutscene;
using NUnit.Framework;

namespace HN.Framework.Core.Tests.Capability.Cutscene
{
    [TestFixture]
    public class CutsceneGlobalSettingsTests
    {
        [Test]
        public void Default_GlobalSkipEnabled_IsTrue()
        {
            var settings = CutsceneGlobalSettings.Default;
            Assert.That(settings.GlobalSkipEnabled, Is.True);
        }

        [Test]
        public void Default_GlobalSpeedMultiplier_IsOne()
        {
            var settings = CutsceneGlobalSettings.Default;
            Assert.That(settings.GlobalSpeedMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void Can_SetAndGet_GlobalSkipEnabled()
        {
            var settings = CutsceneGlobalSettings.Default;
            settings.GlobalSkipEnabled = false;
            Assert.That(settings.GlobalSkipEnabled, Is.False);
            
            settings.GlobalSkipEnabled = true;
            Assert.That(settings.GlobalSkipEnabled, Is.True);
        }

        [Test]
        public void Can_SetAndGet_GlobalSpeedMultiplier()
        {
            var settings = CutsceneGlobalSettings.Default;
            settings.GlobalSpeedMultiplier = 2f;
            Assert.That(settings.GlobalSpeedMultiplier, Is.EqualTo(2f));

            settings.GlobalSpeedMultiplier = 0.5f;
            Assert.That(settings.GlobalSpeedMultiplier, Is.EqualTo(0.5f));
        }

        [Test]
        public void Custom_Constructor_SetsValues()
        {
            var settings = new CutsceneGlobalSettings
            {
                GlobalSkipEnabled = false,
                GlobalSpeedMultiplier = 1.5f
            };

            Assert.That(settings.GlobalSkipEnabled, Is.False);
            Assert.That(settings.GlobalSpeedMultiplier, Is.EqualTo(1.5f));
        }

        [Test]
        public void CutsceneEvent_Constructor_SetsAllProperties()
        {
            var evt = new CutsceneEvent("BossDeath", "Intro", 10.5, "{\"bossId\":1}");
            Assert.That(evt.EventType, Is.EqualTo("BossDeath"));
            Assert.That(evt.CutsceneKey, Is.EqualTo("Intro"));
            Assert.That(evt.Timestamp, Is.EqualTo(10.5));
            Assert.That(evt.EventData, Is.EqualTo("{\"bossId\":1}"));
        }

        [Test]
        public void CutsceneEvent_DefaultConstructor_EmptyStrings()
        {
            CutsceneEvent evt = default;
            Assert.That(evt.EventType, Is.Null);
            Assert.That(evt.CutsceneKey, Is.Null);
            Assert.That(evt.Timestamp, Is.EqualTo(0.0));
        }

        [Test]
        public void CutsceneEvent_NullEventData_BecomesEmpty()
        {
            var evt = new CutsceneEvent("Test", "Key", 1.0, null);
            Assert.That(evt.EventData, Is.EqualTo(string.Empty));
        }
    }
}
