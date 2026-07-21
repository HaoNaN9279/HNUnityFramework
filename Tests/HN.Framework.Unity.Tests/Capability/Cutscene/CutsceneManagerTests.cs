
using HN.Framework.Core.Capability.Cutscene;
using HN.Framework.Core.Capability.Input;
using HN.Framework.Core.Capability.Camera;
using HN.Framework.Unity.Capability.Cutscene;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Cutscene
{
    [TestFixture]
    public class CutsceneManagerTests
    {
        private class MockInputBlocker : IInputBlocker
        {
            public List<object> PushCalls = new List<object>();
            public List<object> PopCalls = new List<object>();
            public void Push(object token, int priority) { PushCalls.Add(token); }
            public void Pop(object token) { PopCalls.Add(token); }
            public bool IsBlocked(int actionPriority) => false;
            public void Clear() { PushCalls.Clear(); PopCalls.Clear(); }
        }

        [Test]
        public void Manager_DefaultState_NotPlaying()
        {
            var manager = new CutsceneManager();
            Assert.That(manager.IsAnyPlaying, Is.False);
            Assert.That(manager.ActivePlayerCount, Is.EqualTo(0));
            Assert.That(manager.QueuedCount, Is.EqualTo(0));
        }

        [Test]
        public void Manager_DefaultSettings_AreCorrect()
        {
            var manager = new CutsceneManager();
            Assert.That(manager.GlobalSettings.GlobalSkipEnabled, Is.True);
            Assert.That(manager.GlobalSettings.GlobalSpeedMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void Manager_Enqueue_IncreasesQueuedCount()
        {
            var manager = new CutsceneManager();
            manager.Enqueue("cut1");
            manager.Enqueue("cut2");
            Assert.That(manager.QueuedCount, Is.EqualTo(2));
        }

        [Test]
        public void Manager_ClearQueue_ResetsCount()
        {
            var manager = new CutsceneManager();
            manager.Enqueue("cut1");
            manager.Enqueue("cut2");
            manager.ClearQueue();
            Assert.That(manager.QueuedCount, Is.EqualTo(0));
        }

        [Test]
        public void Manager_StopAll_DoesNotThrow()
        {
            var manager = new CutsceneManager();
            Assert.DoesNotThrow(() => manager.StopAll());
        }

        [Test]
        public void Manager_PauseAll_DoesNotThrow()
        {
            var manager = new CutsceneManager();
            Assert.DoesNotThrow(() => manager.PauseAll());
        }

        [Test]
        public void Manager_ResumeAll_DoesNotThrow()
        {
            var manager = new CutsceneManager();
            Assert.DoesNotThrow(() => manager.ResumeAll());
        }

        [Test]
        public void Manager_Tick_EmptyQueue_DoesNothing()
        {
            var manager = new CutsceneManager();
            Assert.DoesNotThrow(() => manager.Tick());
        }

        [Test]
        public void Manager_Enqueue_ThenStopAll_DoesNotThrow()
        {
            var manager = new CutsceneManager();
            manager.Enqueue("cut1");
            Assert.DoesNotThrow(() => manager.StopAll());
        }
    }
}