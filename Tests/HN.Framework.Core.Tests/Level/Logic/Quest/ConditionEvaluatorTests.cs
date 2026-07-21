#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Quest;
using System.Collections.Generic;

namespace HN.Framework.Core.Tests.Level.Logic.Quest
{
    [TestFixture]
    public class ConditionEvaluatorTests
    {
        private static ConditionDef MakeCounterCondition(int id, string eventTypeName, int targetCount)
        {
            return new ConditionDef
            {
                Id = id,
                Type = ConditionType.Counter,
                Parameters = new Dictionary<string, string>
                {
                    { "EventTypeName", eventTypeName },
                    { "TargetCount", targetCount.ToString() }
                }
            };
        }

        private static ConditionGroupDef MakeGroup(int id, List<int> conditionIds)
        {
            return new ConditionGroupDef
            {
                Id = id,
                ConditionIds = conditionIds
            };
        }

        [Test]
        public void RegisterCounterCondition_StartsTracking()
        {
            var condDef = MakeCounterCondition(100, "KillMonster", 3);
            var groupDef = MakeGroup(1, new List<int> { 100 });

            var eval = new ConditionEvaluator<uint>();
            eval.LoadConfigs(new[] { condDef }, new[] { groupDef });
            eval.RegisterQuestConditions(1, new List<int> { 1 });

            Assert.That(eval.AreAllGroupsMet(1), Is.False);
        }

        [Test]
        public void NotifyEvent_IncrementsCounter()
        {
            var condDef = MakeCounterCondition(100, "KillMonster", 3);
            var groupDef = MakeGroup(1, new List<int> { 100 });

            var eval = new ConditionEvaluator<uint>();
            eval.LoadConfigs(new[] { condDef }, new[] { groupDef });
            eval.RegisterQuestConditions(1, new List<int> { 1 });

            // 通知 2 次，仍未达到目标
            eval.NotifyEvent("KillMonster");
            eval.NotifyEvent("KillMonster");
            Assert.That(eval.AreAllGroupsMet(1), Is.False);
        }

        [Test]
        public void NotifyEvent_ReachesTarget_FiresOnConditionMet()
        {
            var condDef = MakeCounterCondition(100, "KillMonster", 3);
            var groupDef = MakeGroup(1, new List<int> { 100 });

            var eval = new ConditionEvaluator<uint>();
            eval.LoadConfigs(new[] { condDef }, new[] { groupDef });
            eval.RegisterQuestConditions(1, new List<int> { 1 });

            bool met = false;
            int metConditionId = 0;
            int metHandleId = 0;
            eval.OnConditionMet += (condId, handleId) =>
            {
                met = true;
                metConditionId = condId;
                metHandleId = handleId;
            };

            eval.NotifyEvent("KillMonster");
            eval.NotifyEvent("KillMonster");
            eval.NotifyEvent("KillMonster");

            Assert.That(met, Is.True);
            Assert.That(metConditionId, Is.EqualTo(100));
            Assert.That(metHandleId, Is.EqualTo(1));
            Assert.That(eval.AreAllGroupsMet(1), Is.True);
        }

        [Test]
        public void NotifyEvent_WrongEventType_DoesNotIncrement()
        {
            var condDef = MakeCounterCondition(100, "KillMonster", 3);
            var groupDef = MakeGroup(1, new List<int> { 100 });

            var eval = new ConditionEvaluator<uint>();
            eval.LoadConfigs(new[] { condDef }, new[] { groupDef });
            eval.RegisterQuestConditions(1, new List<int> { 1 });

            // 使用错误的事件类型通知
            eval.NotifyEvent("CollectItem");
            eval.NotifyEvent("CollectItem");
            eval.NotifyEvent("CollectItem");

            Assert.That(eval.AreAllGroupsMet(1), Is.False);
        }

        [Test]
        public void NotifyEvent_TargetCountOne_FiresImmediately()
        {
            var condDef = MakeCounterCondition(100, "KillMonster", 1);
            var groupDef = MakeGroup(1, new List<int> { 100 });

            var eval = new ConditionEvaluator<uint>();
            eval.LoadConfigs(new[] { condDef }, new[] { groupDef });
            eval.RegisterQuestConditions(1, new List<int> { 1 });

            bool met = false;
            eval.OnConditionMet += (condId, handleId) => met = true;

            eval.NotifyEvent("KillMonster");

            Assert.That(met, Is.True);
        }

        [Test]
        public void Evaluate_CounterNotMet_ReturnsNotMet()
        {
            var condDef = MakeCounterCondition(100, "KillMonster", 5);
            var groupDef = MakeGroup(1, new List<int> { 100 });

            var eval = new ConditionEvaluator<uint>();
            eval.LoadConfigs(new[] { condDef }, new[] { groupDef });
            eval.RegisterQuestConditions(1, new List<int> { 1 });

            eval.NotifyEvent("KillMonster");
            eval.NotifyEvent("KillMonster");

            Assert.That(eval.AreAllGroupsMet(1), Is.False);
        }

        [Test]
        public void Evaluate_AllConditionsMet_ReturnsMet()
        {
            var condDef = MakeCounterCondition(100, "KillMonster", 3);
            var groupDef = MakeGroup(1, new List<int> { 100 });

            var eval = new ConditionEvaluator<uint>();
            eval.LoadConfigs(new[] { condDef }, new[] { groupDef });
            eval.RegisterQuestConditions(1, new List<int> { 1 });

            eval.NotifyEvent("KillMonster");
            eval.NotifyEvent("KillMonster");
            eval.NotifyEvent("KillMonster");

            Assert.That(eval.AreAllGroupsMet(1), Is.True);
        }

        [Test]
        public void MultipleConditions_AllMet_ReturnsMet()
        {
            var condDef1 = MakeCounterCondition(100, "KillMonster", 2);
            var condDef2 = MakeCounterCondition(101, "CollectItem", 1);
            var groupDef = MakeGroup(1, new List<int> { 100, 101 });

            var eval = new ConditionEvaluator<uint>();
            eval.LoadConfigs(new[] { condDef1, condDef2 }, new[] { groupDef });
            eval.RegisterQuestConditions(1, new List<int> { 1 });

            // 满足第一个条件
            eval.NotifyEvent("KillMonster");
            eval.NotifyEvent("KillMonster");
            // 组内 AND，第二个未满足
            Assert.That(eval.AreAllGroupsMet(1), Is.False);

            // 满足第二个条件
            eval.NotifyEvent("CollectItem");
            Assert.That(eval.AreAllGroupsMet(1), Is.True);
        }

        [Test]
        public void NoGroups_ReturnsMet()
        {
            var eval = new ConditionEvaluator<uint>();
            Assert.That(eval.AreAllGroupsMet(1), Is.True);
        }
    }
}
