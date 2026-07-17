using HN.Framework.Core.Capability.Cutscene;
using HN.Framework.Core.Capability.Event;
using HN.Framework.Unity.Capability.Cutscene.Tracks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;

namespace HN.Framework.Unity.Tests.Capability.Cutscene
{
    [TestFixture]
    public class CutsceneTrackTests
    {
        [Test]
        public void SubtitleBehaviour_DefaultValues_AreEmpty()
        {
            var behaviour = new SubtitleBehaviour();
            Assert.That(behaviour.LocalizationKey, Is.Null);
            Assert.That(behaviour.SpeakerRole, Is.Null);
        }

        [Test]
        public void SubtitleClip_CreatePlayable_ReturnsValidPlayable()
        {
            var clip = new SubtitleClip();
            var graph = PlayableGraph.Create("Test");
            var playable = clip.CreatePlayable(graph, new GameObject("Owner"));

            Assert.That(playable.IsValid(), Is.True);
            Assert.That(playable.GetPlayableType(), Is.EqualTo(typeof(SubtitleBehaviour)));

            graph.Destroy();
        }

        [Test]
        public void DialogueBehaviour_DefaultValues_AreEmpty()
        {
            var behaviour = new DialogueBehaviour();
            Assert.That(behaviour.DialogueId, Is.Null);
        }

        [Test]
        public void DialogueTriggerEvent_HasRequiredFields()
        {
            var evt = new DialogueTriggerEvent();
            Assert.That(evt.DialogueId, Is.Null);
            Assert.That(evt.Timestamp, Is.EqualTo(0.0));
        }

        [Test]
        public void TimelineEventBehaviour_DefaultValues_AreEmpty()
        {
            var behaviour = new TimelineEventBehaviour();
            Assert.That(behaviour.EventType, Is.Null);
            Assert.That(behaviour.EventData, Is.Null);
        }

        [Test]
        public void ShakeBehaviour_DefaultValues_AreCorrect()
        {
            var behaviour = new ShakeBehaviour();
            Assert.That(behaviour.Amplitude, Is.EqualTo(1f));
            Assert.That(behaviour.Frequency, Is.EqualTo(1f));
        }

        [Test]
        public void GameStateBehaviour_DefaultValues_AreCorrect()
        {
            var behaviour = new GameStateBehaviour();
            Assert.That(behaviour.SetActive, Is.False);
            Assert.That(behaviour.AnimatorValue, Is.EqualTo(0f));
        }

        [Test]
        public void SubtitleTrack_HasCorrectAttributes()
        {
            var attrs = typeof(SubtitleTrack).GetCustomAttributes(typeof(UnityEngine.Timeline.TrackClipTypeAttribute), false);
            Assert.That(attrs.Length, Is.GreaterThan(0));
        }

        [Test]
        public void DialogueTrack_HasCorrectAttributes()
        {
            var attrs = typeof(DialogueTrack).GetCustomAttributes(typeof(UnityEngine.Timeline.TrackClipTypeAttribute), false);
            Assert.That(attrs.Length, Is.GreaterThan(0));
        }
    }
}