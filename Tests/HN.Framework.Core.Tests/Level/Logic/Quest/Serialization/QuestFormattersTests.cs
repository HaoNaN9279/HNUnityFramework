#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Quest;
using MemoryPack;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Tests.Level.Logic.Quest.Serialization
{
    [TestFixture]
    public class QuestFormattersTests
    {
        [Test]
        public void QuestInstance_RoundTrip_PreservesData()
        {
            var original = new QuestInstance
            {
                QuestId = 42,
                State = QuestState.Active,
                StartTime = (Fixed64)100,
                Progress = new Dictionary<string, int> { { "kills", 5 }, { "collects", 3 } },
                RepeatCount = 2,
                TimeRemaining = (Fixed64)300
            };

            var data = MemoryPackSerializer.Serialize(original);
            var restored = MemoryPackSerializer.Deserialize<QuestInstance>(data);

            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.QuestId, Is.EqualTo(42));
            Assert.That(restored.State, Is.EqualTo(QuestState.Active));
            Assert.That(restored.StartTime, Is.EqualTo((Fixed64)100));
            Assert.That(restored.RepeatCount, Is.EqualTo(2));
            Assert.That(restored.TimeRemaining, Is.EqualTo((Fixed64)300));
            Assert.That(restored.Progress, Is.Not.Null);
            Assert.That(restored.Progress!["kills"], Is.EqualTo(5));
            Assert.That(restored.Progress!["collects"], Is.EqualTo(3));
        }

        [Test]
        public void AchievementInstance_RoundTrip_PreservesData()
        {
            var original = new AchievementInstance
            {
                AchievementId = 99,
                State = AchievementState.Completed,
                ProgressPercent = 0.75f,
                CompletedTime = (Fixed64)500,
                ClaimedTime = null
            };

            var data = MemoryPackSerializer.Serialize(original);
            var restored = MemoryPackSerializer.Deserialize<AchievementInstance>(data);

            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.AchievementId, Is.EqualTo(99));
            Assert.That(restored.State, Is.EqualTo(AchievementState.Completed));
            Assert.That(restored.ProgressPercent, Is.EqualTo(0.75f));
            Assert.That(restored.CompletedTime, Is.EqualTo((Fixed64)500));
            Assert.That(restored.ClaimedTime, Is.Null);
        }

        [Test]
        public void CounterState_RoundTrip_PreservesData()
        {
            var original = new CounterState
            {
                ConditionId = 7,
                CurrentCount = 5,
                TargetCount = 10,
                IsCompleted = false,
                IsFailed = false
            };

            var data = MemoryPackSerializer.Serialize(original);
            var restored = MemoryPackSerializer.Deserialize<CounterState>(data);

            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.ConditionId, Is.EqualTo(7));
            Assert.That(restored.CurrentCount, Is.EqualTo(5));
            Assert.That(restored.TargetCount, Is.EqualTo(10));
            Assert.That(restored.IsCompleted, Is.False);
            Assert.That(restored.IsFailed, Is.False);
        }

        [Test]
        public void QuestInstance_EmptyProgress_RoundTrips()
        {
            var original = new QuestInstance
            {
                QuestId = 1,
                State = QuestState.Locked,
                StartTime = Fixed64.Zero,
                Progress = null,
                RepeatCount = 0,
                TimeRemaining = null
            };

            var data = MemoryPackSerializer.Serialize(original);
            var restored = MemoryPackSerializer.Deserialize<QuestInstance>(data);

            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.QuestId, Is.EqualTo(1));
            Assert.That(restored.State, Is.EqualTo(QuestState.Locked));
            Assert.That(restored.Progress, Is.Null);
            Assert.That(restored.RepeatCount, Is.EqualTo(0));
            Assert.That(restored.TimeRemaining, Is.Null);
        }

        [Test]
        public void CounterState_Completed_RoundTrip()
        {
            var original = new CounterState
            {
                ConditionId = 42,
                CurrentCount = 10,
                TargetCount = 10,
                IsCompleted = true,
                IsFailed = false
            };

            var data = MemoryPackSerializer.Serialize(original);
            var restored = MemoryPackSerializer.Deserialize<CounterState>(data);

            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.IsCompleted, Is.True);
            Assert.That(restored.CurrentCount, Is.EqualTo(10));
            Assert.That(restored.TargetCount, Is.EqualTo(10));
        }

        [Test]
        public void AchievementInstance_ClaimedState_RoundTrip()
        {
            var original = new AchievementInstance
            {
                AchievementId = 50,
                State = AchievementState.Claimed,
                ProgressPercent = 1.0f,
                CompletedTime = (Fixed64)200,
                ClaimedTime = (Fixed64)250
            };

            var data = MemoryPackSerializer.Serialize(original);
            var restored = MemoryPackSerializer.Deserialize<AchievementInstance>(data);

            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.State, Is.EqualTo(AchievementState.Claimed));
            Assert.That(restored.CompletedTime, Is.EqualTo((Fixed64)200));
            Assert.That(restored.ClaimedTime, Is.EqualTo((Fixed64)250));
        }

        [Test]
        public void QuestInstance_FailedState_RoundTrip()
        {
            var original = new QuestInstance
            {
                QuestId = 10,
                State = QuestState.Failed,
                StartTime = (Fixed64)50,
                Progress = new Dictionary<string, int> { { "kills", 1 } },
                RepeatCount = 0,
                TimeRemaining = (Fixed64)0
            };

            var data = MemoryPackSerializer.Serialize(original);
            var restored = MemoryPackSerializer.Deserialize<QuestInstance>(data);

            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.State, Is.EqualTo(QuestState.Failed));
        }
    }
}
