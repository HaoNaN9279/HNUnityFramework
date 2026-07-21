#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Quest;
using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Tests.Level.Logic.Quest
{
    [TestFixture]
    public class QuestSystemTests
    {
        private sealed class TestQuestSystem
        {
            internal static QuestDef CreateTestDef(int id, int timeLimit = 0, bool repeatable = false)
            {
                return new QuestDef
                {
                    Id = id,
                    Name = $"Test Quest {id}",
                    Description = "Test",
                    PrerequisiteQuestIds = null,
                    RequiredLevel = 0,
                    TimeLimit = timeLimit > 0 ? (Fixed64)timeLimit : null,
                    ConditionGroupIds = null,
                    RewardGroupIds = null,
                    IsRepeatable = repeatable
                };
            }

            internal QuestSystem<uint> System { get; private set; }
            internal readonly Dictionary<int, QuestDef> Defs = new();

            internal TestQuestSystem(params int[] questIds) : this()
            {
                foreach (var id in questIds)
                    Defs[id] = CreateTestDef(id);
            }

            private TestQuestSystem()
            {
                System = new QuestSystem<uint>(id => Defs.TryGetValue(id, out var def) ? def : (QuestDef?)null);
            }

            internal void Rebuild()
            {
                System = new QuestSystem<uint>(id => Defs.TryGetValue(id, out var def) ? def : (QuestDef?)null);
            }
        }

        [Test]
        public void AcceptQuest_ValidDef_ReturnsValidHandle()
        {
            var ts = new TestQuestSystem(1);
            var handle = ts.System.AcceptQuest(1, 100u);

            Assert.That(handle.IsValid, Is.True);

            var quest = ts.System.GetQuest(handle);
            Assert.That(quest, Is.Not.Null);
            Assert.That(quest!.State, Is.EqualTo(QuestState.Active));
            Assert.That(quest.QuestId, Is.EqualTo(1));
        }

        [Test]
        public void AcceptQuest_AlreadyActive_ReturnsInvalidHandle()
        {
            var ts = new TestQuestSystem(1);
            var handle1 = ts.System.AcceptQuest(1, 100u);
            Assert.That(handle1.IsValid, Is.True);

            var handle2 = ts.System.AcceptQuest(1, 100u);
            Assert.That(handle2.IsValid, Is.False);
        }

        [Test]
        public void AcceptQuest_UnknownDef_ReturnsInvalidHandle()
        {
            var ts = new TestQuestSystem(1);
            var handle = ts.System.AcceptQuest(999, 100u);

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void CompleteQuest_ActiveState_Succeeds()
        {
            var ts = new TestQuestSystem(1);
            var handle = ts.System.AcceptQuest(1, 100u);

            bool result = ts.System.CompleteQuest(handle);
            Assert.That(result, Is.True);

            var quest = ts.System.GetQuest(handle);
            Assert.That(quest, Is.Not.Null);
            Assert.That(quest!.State, Is.EqualTo(QuestState.Completed));
        }

        [Test]
        public void CompleteQuest_AlreadyCompleted_Fails()
        {
            var ts = new TestQuestSystem(1);
            var handle = ts.System.AcceptQuest(1, 100u);

            ts.System.CompleteQuest(handle);
            bool result = ts.System.CompleteQuest(handle);

            Assert.That(result, Is.False);
        }

        [Test]
        public void CompleteQuest_InvalidHandle_Fails()
        {
            var ts = new TestQuestSystem(1);
            bool result = ts.System.CompleteQuest(QuestHandle.Invalid);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ClaimQuest_CompletedState_Succeeds()
        {
            var ts = new TestQuestSystem(1);
            var handle = ts.System.AcceptQuest(1, 100u);
            ts.System.CompleteQuest(handle);

            var result = ts.System.ClaimQuest(handle);
            Assert.That(result.Success, Is.True);

            var quest = ts.System.GetQuest(handle);
            Assert.That(quest, Is.Not.Null);
            Assert.That(quest!.State, Is.EqualTo(QuestState.Claimed));
        }

        [Test]
        public void ClaimQuest_ActiveState_Fails()
        {
            var ts = new TestQuestSystem(1);
            var handle = ts.System.AcceptQuest(1, 100u);

            var result = ts.System.ClaimQuest(handle);
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void AbandonQuest_ActiveState_Succeeds()
        {
            var ts = new TestQuestSystem(1);
            var handle = ts.System.AcceptQuest(1, 100u);

            bool result = ts.System.AbandonQuest(handle);
            Assert.That(result, Is.True);
            Assert.That(ts.System.GetQuest(handle), Is.Null);
        }

        [Test]
        public void AbandonQuest_InvalidHandle_Fails()
        {
            var ts = new TestQuestSystem(1);
            bool result = ts.System.AbandonQuest(QuestHandle.Invalid);

            Assert.That(result, Is.False);
        }

        [Test]
        public void Tick_TimeLimitExpired_MarksAsFailed()
        {
            var ts = new TestQuestSystem();
            ts.Defs[1] = TestQuestSystem.CreateTestDef(1, timeLimit: 10);
            ts.Rebuild();

            var handle = ts.System.AcceptQuest(1, 100u);
            Assert.That(ts.System.GetQuest(handle)!.State, Is.EqualTo(QuestState.Active));

            ts.System.Tick((Fixed64)15);

            var quest = ts.System.GetQuest(handle);
            Assert.That(quest, Is.Not.Null);
            Assert.That(quest!.State, Is.EqualTo(QuestState.Failed));
        }

        [Test]
        public void Tick_NoTimeLimit_NotAffected()
        {
            var ts = new TestQuestSystem();
            ts.Defs[1] = TestQuestSystem.CreateTestDef(1, timeLimit: 0);
            ts.Rebuild();

            var handle = ts.System.AcceptQuest(1, 100u);

            ts.System.Tick((Fixed64)100);

            var quest = ts.System.GetQuest(handle);
            Assert.That(quest, Is.Not.Null);
            Assert.That(quest!.State, Is.EqualTo(QuestState.Active));
        }

        [Test]
        public void AcceptQuest_Repeatable_AfterClaimed_Succeeds()
        {
            var ts = new TestQuestSystem();
            ts.Defs[1] = TestQuestSystem.CreateTestDef(1, repeatable: true);
            ts.Rebuild();

            var handle1 = ts.System.AcceptQuest(1, 100u);
            ts.System.CompleteQuest(handle1);
            ts.System.ClaimQuest(handle1);

            var handle2 = ts.System.AcceptQuest(1, 100u);

            Assert.That(handle2.IsValid, Is.True);
            Assert.That(handle2.Id, Is.Not.EqualTo(handle1.Id));
        }

        [Test]
        public void GetQuest_ValidHandle_ReturnsInstance()
        {
            var ts = new TestQuestSystem(1);
            var handle = ts.System.AcceptQuest(1, 100u);

            var quest = ts.System.GetQuest(handle);

            Assert.That(quest, Is.Not.Null);
            Assert.That(quest!.QuestId, Is.EqualTo(1));
        }

        [Test]
        public void GetQuest_InvalidHandle_ReturnsNull()
        {
            var ts = new TestQuestSystem(1);

            var quest = ts.System.GetQuest(QuestHandle.Invalid);

            Assert.That(quest, Is.Null);
        }

        [Test]
        public void ActiveCount_TracksActiveQuests()
        {
            var ts = new TestQuestSystem(1, 2, 3);

            Assert.That(ts.System.ActiveCount, Is.EqualTo(0));

            var h1 = ts.System.AcceptQuest(1, 100u);
            Assert.That(ts.System.ActiveCount, Is.EqualTo(1));

            var h2 = ts.System.AcceptQuest(2, 100u);
            Assert.That(ts.System.ActiveCount, Is.EqualTo(2));

            ts.System.CompleteQuest(h1);
            Assert.That(ts.System.ActiveCount, Is.EqualTo(1));

            ts.System.CompleteQuest(h2);
            Assert.That(ts.System.ActiveCount, Is.EqualTo(0));
        }

        [Test]
        public void StateChangedEvent_FiresOnTransition()
        {
            var ts = new TestQuestSystem(1);
            QuestStateChangedEvent<uint> captured = default;
            bool fired = false;
            ts.System.OnQuestStateChanged += e => { captured = e; fired = true; };

            var handle = ts.System.AcceptQuest(1, 100u);

            Assert.That(fired, Is.True);
            Assert.That(captured.NewState, Is.EqualTo(QuestState.Active));
            Assert.That(captured.QuestId, Is.EqualTo(1));
            Assert.That(captured.OldState, Is.EqualTo(QuestState.Locked));
        }

        [Test]
        public void StateChangedEvent_FiresOnComplete()
        {
            var ts = new TestQuestSystem(1);
            var handle = ts.System.AcceptQuest(1, 100u);

            QuestStateChangedEvent<uint> captured = default;
            bool fired = false;
            ts.System.OnQuestStateChanged += e => { captured = e; fired = true; };

            ts.System.CompleteQuest(handle);

            Assert.That(fired, Is.True);
            Assert.That(captured.NewState, Is.EqualTo(QuestState.Completed));
        }

        [Test]
        public void Tick_Expired_FiresFailedEvent()
        {
            var ts = new TestQuestSystem(1);
            ts.Defs[1] = TestQuestSystem.CreateTestDef(1, timeLimit: 5);
            ts.System.AcceptQuest(1, 100u);

            QuestStateChangedEvent<uint> captured = default;
            bool fired = false;
            ts.System.OnQuestStateChanged += e =>
            {
                if (e.NewState == QuestState.Failed) { captured = e; fired = true; }
            };

            ts.System.Tick((Fixed64)10);

            Assert.That(fired, Is.True);
            Assert.That(captured.NewState, Is.EqualTo(QuestState.Failed));
        }
    }
}
