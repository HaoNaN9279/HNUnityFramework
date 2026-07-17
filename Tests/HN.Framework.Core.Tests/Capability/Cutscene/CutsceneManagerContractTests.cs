using System;
using HN.Framework.Core.Capability.Cutscene;
using NUnit.Framework;

namespace HN.Framework.Core.Tests.Capability.Cutscene
{
    [TestFixture]
    public class CutsceneManagerContractTests
    {
        private class MockCutscenePlayer : ICutscenePlayer
        {
            public string CutsceneKey { get; set; }
            public CutsceneState State { get; set; }
            public double CurrentTime { get; set; }
            public double Duration { get; set; }
            public event Action<ICutscenePlayer>? OnFinished;
            public event Action<ICutscenePlayer, CutsceneState>? OnStateChanged;

            public void Play() { State = CutsceneState.Playing; OnStateChanged?.Invoke(this, CutsceneState.Playing); }
            public void Pause() { State = CutsceneState.Paused; OnStateChanged?.Invoke(this, CutsceneState.Paused); }
            public void Resume() { State = CutsceneState.Playing; OnStateChanged?.Invoke(this, CutsceneState.Playing); }
            public void Stop() { State = CutsceneState.Idle; OnStateChanged?.Invoke(this, CutsceneState.Idle); }
            public void Skip() { State = CutsceneState.Skipped; OnFinished?.Invoke(this); }
            public void SeekTo(double time) { CurrentTime = time; }

            public void FireFinished() { OnFinished?.Invoke(this); }
        }

        private class MockCutsceneManager : ICutsceneManager
        {
            public CutsceneGlobalSettings GlobalSettings { get; set; } = CutsceneGlobalSettings.Default;
            public bool IsAnyPlaying { get; private set; }
            public int ActivePlayerCount { get; private set; }
            public int QueuedCount { get; private set; }

            public ICutscenePlayer Play(string cutsceneKey, CutscenePriority priority = CutscenePriority.Normal, CutsceneSkipMode skipMode = CutsceneSkipMode.None, CutsceneBindingMap? bindingMap = null)
            {
                IsAnyPlaying = true;
                ActivePlayerCount++;
                return new MockCutscenePlayer { CutsceneKey = cutsceneKey, State = CutsceneState.Playing };
            }

            public void Enqueue(string cutsceneKey, CutscenePriority priority = CutscenePriority.Normal, CutsceneSkipMode skipMode = CutsceneSkipMode.None, CutsceneBindingMap? bindingMap = null)
            {
                QueuedCount++;
            }

            public void StopAll() { IsAnyPlaying = false; ActivePlayerCount = 0; }
            public void PauseAll() { }
            public void ResumeAll() { }
            public void SkipCurrent() { }
            public void ClearQueue() { QueuedCount = 0; }
        }

        [Test]
        public void Player_DefaultState_IsIdle()
        {
            var player = new MockCutscenePlayer();
            Assert.That(player.State, Is.EqualTo(CutsceneState.Idle));
        }

        [Test]
        public void Player_Play_TransitionsToPlaying()
        {
            var player = new MockCutscenePlayer();
            player.Play();
            Assert.That(player.State, Is.EqualTo(CutsceneState.Playing));
        }

        [Test]
        public void Player_Pause_TransitionsToPaused()
        {
            var player = new MockCutscenePlayer();
            player.Play();
            player.Pause();
            Assert.That(player.State, Is.EqualTo(CutsceneState.Paused));
        }

        [Test]
        public void Player_Resume_TransitionsToPlaying()
        {
            var player = new MockCutscenePlayer();
            player.Play();
            player.Pause();
            player.Resume();
            Assert.That(player.State, Is.EqualTo(CutsceneState.Playing));
        }

        [Test]
        public void Player_Stop_TransitionsToIdle()
        {
            var player = new MockCutscenePlayer();
            player.Play();
            player.Stop();
            Assert.That(player.State, Is.EqualTo(CutsceneState.Idle));
        }

        [Test]
        public void Player_Skip_TransitionsToSkipped()
        {
            var player = new MockCutscenePlayer();
            player.Play();
            player.Skip();
            Assert.That(player.State, Is.EqualTo(CutsceneState.Skipped));
        }

        [Test]
        public void Player_OnFinished_InvokedAfterSkip()
        {
            var player = new MockCutscenePlayer();
            bool finished = false;
            player.OnFinished += (p) => finished = true;
            player.Skip();
            Assert.That(finished, Is.True);
        }

        [Test]
        public void Player_SeekTo_SetsCurrentTime()
        {
            var player = new MockCutscenePlayer();
            player.SeekTo(5.5);
            Assert.That(player.CurrentTime, Is.EqualTo(5.5));
        }

        [Test]
        public void Manager_Play_ReturnsPlayerWithCorrectKey()
        {
            var manager = new MockCutsceneManager();
            var player = manager.Play("Intro", CutscenePriority.Normal);
            Assert.That(player.CutsceneKey, Is.EqualTo("Intro"));
            Assert.That(player.State, Is.EqualTo(CutsceneState.Playing));
        }

        [Test]
        public void Manager_Play_SetsIsAnyPlaying()
        {
            var manager = new MockCutsceneManager();
            Assert.That(manager.IsAnyPlaying, Is.False);
            manager.Play("Test");
            Assert.That(manager.IsAnyPlaying, Is.True);
        }

        [Test]
        public void Manager_Enqueue_IncreasesQueuedCount()
        {
            var manager = new MockCutsceneManager();
            manager.Enqueue("Test1");
            manager.Enqueue("Test2");
            Assert.That(manager.QueuedCount, Is.EqualTo(2));
        }

        [Test]
        public void Manager_StopAll_ClearsActivePlayers()
        {
            var manager = new MockCutsceneManager();
            manager.Play("Test1");
            manager.Play("Test2");
            manager.StopAll();
            Assert.That(manager.IsAnyPlaying, Is.False);
            Assert.That(manager.ActivePlayerCount, Is.EqualTo(0));
        }

        [Test]
        public void Manager_ClearQueue_ResetsQueuedCount()
        {
            var manager = new MockCutsceneManager();
            manager.Enqueue("Test");
            manager.ClearQueue();
            Assert.That(manager.QueuedCount, Is.EqualTo(0));
        }
    }
}
