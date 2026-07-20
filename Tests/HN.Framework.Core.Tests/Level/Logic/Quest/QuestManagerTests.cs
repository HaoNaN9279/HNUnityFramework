#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Quest;
using HN.Framework.Core.Capability.Event;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HN.Framework.Core.Tests.Level.Logic.Quest
{
    [TestFixture]
    public class QuestManagerTests
    {
        private sealed class MockEventBus : IEventBus
        {
            private readonly Dictionary<Type, List<Delegate>> _handlers = new();
            public readonly List<object> PublishedEvents = new();

            public void Subscribe<T>(Action<T> handler)
            {
                var type = typeof(T);
                if (!_handlers.ContainsKey(type))
                    _handlers[type] = new List<Delegate>();
                _handlers[type].Add(handler);
            }

            public void Unsubscribe<T>(Action<T> handler)
            {
                if (_handlers.TryGetValue(typeof(T), out var list))
                    list.Remove(handler);
            }

            public void Publish<T>(T eventData)
            {
                PublishedEvents.Add(eventData!);
                if (_handlers.TryGetValue(typeof(T), out var list))
                {
                    foreach (var d in list)
                        ((Action<T>)d)(eventData);
                }
            }

            public bool HasSubscribers<T>()
            {
                return true;
            }

            public void Clear()
            {
                PublishedEvents.Clear();
            }
        }

        private MockEventBus _eventBus = null!;
        private QuestManager<int> _questManager = null!;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new MockEventBus();
            _questManager = new QuestManager<int>(_eventBus);
        }

        [TearDown]
        public void TearDown()
        {
            _eventBus.Clear();
        }

        private static QuestDef MakeQuestDef(int id, string? name = null, List<int>? prereqs = null,
            List<int>? conditionGroups = null, List<int>? rewardGroups = null, int timeLimit = 0)
        {
            return new QuestDef
            {
                Id = id,
                Name = name ?? $"Quest_{id}",
                Description = "Test",
                PrerequisiteQuestIds = prereqs,
                RequiredLevel = 0,
                TimeLimit = timeLimit > 0 ? (FixedMathSharp.Fixed64)timeLimit : null,
                ConditionGroupIds = conditionGroups,
                RewardGroupIds = rewardGroups,
                IsRepeatable = false
            };
        }

        private void Initialize(params QuestDef[] questDefs)
        {
            _questManager.Initialize(
                questDefs,
                Array.Empty<AchievementDef>(),
                Array.Empty<ConditionGroupDef>(),
                Array.Empty<RewardDef>(),
                Array.Empty<QuestChainDef>());
        }

        // ==================== 任务操作测试 ====================

        [Test]
        public void TryAcceptQuest_NoPrereq_ReturnsValidHandle()
        {
            Initialize(MakeQuestDef(1));

            var handle = _questManager.TryAcceptQuest(1, 100);

            Assert.That(handle.IsValid, Is.True);
            Assert.That(_questManager.ActiveQuestCount, Is.EqualTo(1));
        }

        [Test]
        public void TryAcceptQuest_HasPrereqNotDone_ReturnsInvalid()
        {
            var questDef2 = MakeQuestDef(2, prereqs: new List<int> { 1 });
            Initialize(MakeQuestDef(1), questDef2);

            // 尝试接受需要前置任务 1 的任务 2，但任务 1 未完成
            var handle2 = _questManager.TryAcceptQuest(2, 100);
            Assert.That(handle2.IsValid, Is.False);

            // 完成前置任务 1
            var handle1 = _questManager.TryAcceptQuest(1, 100);
            Assert.That(handle1.IsValid, Is.True);
            _questManager.CompleteQuest(handle1);
            _questManager.ClaimQuest(handle1);

            // 现在可以接受任务 2
            var handle2Retry = _questManager.TryAcceptQuest(2, 100);
            Assert.That(handle2Retry.IsValid, Is.True);
        }

        [Test]
        public void TryAcceptQuest_UnknownId_ReturnsInvalid()
        {
            Initialize(MakeQuestDef(1));

            var handle = _questManager.TryAcceptQuest(999, 100);
            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void CompleteQuest_Active_Succeeds()
        {
            Initialize(MakeQuestDef(1));
            var handle = _questManager.TryAcceptQuest(1, 100);

            bool result = _questManager.CompleteQuest(handle);

            Assert.That(result, Is.True);
            var quest = _questManager.GetQuest(handle);
            Assert.That(quest, Is.Not.Null);
            Assert.That(quest!.State, Is.EqualTo(QuestState.Completed));
        }

        [Test]
        public void ClaimQuest_Completed_ReturnsReward()
        {
            var questDef = MakeQuestDef(1, rewardGroups: new List<int> { 1 });
            var rewardDef = new RewardDef { Id = 1, Type = RewardType.Item, TargetId = 100, Amount = 5, Probability = 1.0f };
            _questManager.Initialize(
                new[] { questDef },
                Array.Empty<AchievementDef>(),
                Array.Empty<ConditionGroupDef>(),
                new[] { rewardDef },
                Array.Empty<QuestChainDef>());

            var handle = _questManager.TryAcceptQuest(1, 100);
            _questManager.CompleteQuest(handle);

            var result = _questManager.ClaimQuest(handle);

            Assert.That(result.Success, Is.True);
            var quest = _questManager.GetQuest(handle);
            Assert.That(quest, Is.Not.Null);
            Assert.That(quest!.State, Is.EqualTo(QuestState.Claimed));
        }

        [Test]
        public void ClaimQuest_ActiveState_Fails()
        {
            Initialize(MakeQuestDef(1));
            var handle = _questManager.TryAcceptQuest(1, 100);

            var result = _questManager.ClaimQuest(handle);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void CompleteQuest_TriggersEventBusPublish()
        {
            Initialize(MakeQuestDef(1));
            var handle = _questManager.TryAcceptQuest(1, 100);
            _eventBus.Clear();

            _questManager.CompleteQuest(handle);

            bool hasStateChangedEvent = _eventBus.PublishedEvents
                .Any(e => e is QuestStateChangedEvent<int>);
            Assert.That(hasStateChangedEvent, Is.True);
        }

        [Test]
        public void FailQuest_ActiveQuest_SetsFailed()
        {
            Initialize(MakeQuestDef(1));
            var handle = _questManager.TryAcceptQuest(1, 100);

            bool result = _questManager.FailQuest(handle);

            Assert.That(result, Is.True);
            var quest = _questManager.GetQuest(handle);
            Assert.That(quest, Is.Not.Null);
            Assert.That(quest!.State, Is.EqualTo(QuestState.Failed));
        }

        [Test]
        public void FailQuest_CompletedQuest_Fails()
        {
            Initialize(MakeQuestDef(1));
            var handle = _questManager.TryAcceptQuest(1, 100);
            _questManager.CompleteQuest(handle);

            // FailQuest 内部检查 state != Active → 失败
            bool result = _questManager.FailQuest(handle);
            Assert.That(result, Is.False);
        }

        [Test]
        public void UpdateQuestProgress_ActiveQuest_UpdatesProgress()
        {
            Initialize(MakeQuestDef(1));
            var handle = _questManager.TryAcceptQuest(1, 100);

            _questManager.UpdateQuestProgress(handle, "kills", 3);
            _questManager.UpdateQuestProgress(handle, "kills", 2);

            var quest = _questManager.GetQuest(handle);
            Assert.That(quest, Is.Not.Null);
            Assert.That(quest!.Progress, Is.Not.Null);
            Assert.That(quest.Progress!["kills"], Is.EqualTo(5));
        }

        [Test]
        public void UpdateQuestProgress_CompletedQuest_NoOp()
        {
            Initialize(MakeQuestDef(1));
            var handle = _questManager.TryAcceptQuest(1, 100);
            _questManager.CompleteQuest(handle);

            _questManager.UpdateQuestProgress(handle, "kills", 10);

            var quest = _questManager.GetQuest(handle);
            // Progress 在 Complete 后不会增加（方法内检查 State != Active）
            Assert.That(quest!.Progress, Is.Null);
        }

        // ==================== 查询测试 ====================

        [Test]
        public void GetQuest_ValidHandle_ReturnsInstance()
        {
            Initialize(MakeQuestDef(1));
            var handle = _questManager.TryAcceptQuest(1, 100);

            var quest = _questManager.GetQuest(handle);

            Assert.That(quest, Is.Not.Null);
            Assert.That(quest!.QuestId, Is.EqualTo(1));
        }

        [Test]
        public void GetQuest_InvalidHandle_ReturnsNull()
        {
            Initialize(MakeQuestDef(1));

            var quest = _questManager.GetQuest(QuestHandle.Invalid);

            Assert.That(quest, Is.Null);
        }

        [Test]
        public void ActiveQuestCount_TracksCorrectly()
        {
            Initialize(MakeQuestDef(1), MakeQuestDef(2), MakeQuestDef(3));

            Assert.That(_questManager.ActiveQuestCount, Is.EqualTo(0));

            var h1 = _questManager.TryAcceptQuest(1, 100);
            Assert.That(_questManager.ActiveQuestCount, Is.EqualTo(1));

            var h2 = _questManager.TryAcceptQuest(2, 100);
            Assert.That(_questManager.ActiveQuestCount, Is.EqualTo(2));

            _questManager.CompleteQuest(h1);
            Assert.That(_questManager.ActiveQuestCount, Is.EqualTo(1));
        }

        // ==================== 事件发布测试 ====================

        [Test]
        public void TryAcceptQuest_PublishesAcceptedEvent()
        {
            Initialize(MakeQuestDef(1));
            _eventBus.Clear();

            _questManager.TryAcceptQuest(1, 100);

            bool hasAcceptedEvent = _eventBus.PublishedEvents
                .Any(e => e is QuestAcceptedEvent<int>);
            Assert.That(hasAcceptedEvent, Is.True);
        }

        [Test]
        public void ClaimQuest_PublishesClaimedEvent()
        {
            Initialize(MakeQuestDef(1));
            var handle = _questManager.TryAcceptQuest(1, 100);
            _questManager.CompleteQuest(handle);
            _eventBus.Clear();

            _questManager.ClaimQuest(handle);

            bool hasClaimedEvent = _eventBus.PublishedEvents
                .Any(e => e is QuestClaimedEvent<int>);
            Assert.That(hasClaimedEvent, Is.True);
        }

        [Test]
        public void FailQuest_PublishesFailedEvent()
        {
            Initialize(MakeQuestDef(1));
            var handle = _questManager.TryAcceptQuest(1, 100);
            _eventBus.Clear();

            _questManager.FailQuest(handle);

            bool hasFailedEvent = _eventBus.PublishedEvents
                .Any(e => e is QuestFailedEvent<int>);
            Assert.That(hasFailedEvent, Is.True);
        }

        // ==================== 初始化保护测试 ====================

        [Test]
        public void TryAcceptQuest_BeforeInitialize_ReturnsInvalid()
        {
            var mgr = new QuestManager<int>(_eventBus);
            var handle = mgr.TryAcceptQuest(1, 100);

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void CompleteQuest_BeforeInitialize_Fails()
        {
            var mgr = new QuestManager<int>(_eventBus);
            bool result = mgr.CompleteQuest(QuestHandle.Invalid);

            Assert.That(result, Is.False);
        }

        // ==================== NotifyGameEvent 测试 ====================

        [Test]
        public void NotifyGameEvent_WithoutInit_NoOp()
        {
            var mgr = new QuestManager<int>(_eventBus);
            // 不应抛出异常
            Assert.DoesNotThrow(() => mgr.NotifyGameEvent("TestEvent"));
        }
    }
}
