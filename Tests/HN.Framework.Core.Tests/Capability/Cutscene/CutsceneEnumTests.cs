using HN.Framework.Core.Capability.Cutscene;
using NUnit.Framework;

namespace HN.Framework.Core.Tests.Capability.Cutscene
{
    [TestFixture]
    public class CutsceneEnumTests
    {
        [Test]
        public void CutsceneState_DefaultValue_IsIdle()
        {
            CutsceneState state = default;
            Assert.That(state, Is.EqualTo(CutsceneState.Idle));
        }

        [Test]
        public void CutsceneState_Values_AreInExpectedOrder()
        {
            Assert.That((int)CutsceneState.Idle, Is.EqualTo(0));
            Assert.That((int)CutsceneState.Playing, Is.EqualTo(1));
            Assert.That((int)CutsceneState.Paused, Is.EqualTo(2));
            Assert.That((int)CutsceneState.Stopping, Is.EqualTo(3));
            Assert.That((int)CutsceneState.Finished, Is.EqualTo(4));
            Assert.That((int)CutsceneState.Skipped, Is.EqualTo(5));
        }

        [Test]
        public void CutscenePriority_DefaultValue_IsBackground()
        {
            CutscenePriority priority = default;
            Assert.That(priority, Is.EqualTo(CutscenePriority.Background));
        }

        [Test]
        public void CutscenePriority_Values_AreInExpectedOrder()
        {
            Assert.That((int)CutscenePriority.Background, Is.EqualTo(0));
            Assert.That((int)CutscenePriority.Normal, Is.EqualTo(20));
            Assert.That((int)CutscenePriority.Important, Is.EqualTo(50));
            Assert.That((int)CutscenePriority.Critical, Is.EqualTo(100));
        }

        [Test]
        public void CutsceneSkipMode_DefaultValue_IsNone()
        {
            CutsceneSkipMode skipMode = default;
            Assert.That(skipMode, Is.EqualTo(CutsceneSkipMode.None));
        }

        [Test]
        public void CutsceneSkipMode_Values_AreInExpectedOrder()
        {
            Assert.That((int)CutsceneSkipMode.None, Is.EqualTo(0));
            Assert.That((int)CutsceneSkipMode.Immediate, Is.EqualTo(1));
            Assert.That((int)CutsceneSkipMode.EndOfSegment, Is.EqualTo(2));
        }

        [Test]
        public void CutscenePriority_Comparison_CriticalHighest()
        {
            Assert.That(CutscenePriority.Critical, Is.GreaterThan(CutscenePriority.Important));
            Assert.That(CutscenePriority.Important, Is.GreaterThan(CutscenePriority.Normal));
            Assert.That(CutscenePriority.Normal, Is.GreaterThan(CutscenePriority.Background));
        }
    }
}
