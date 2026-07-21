#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Quest;
using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Tests.Level.Logic.Quest
{
    [TestFixture]
    public class AchievementSystemTests
    {
        private sealed class TestAchievementSystem
        {
            internal static AchievementDef CreateTestDef(
                int id, int categoryId = 0, bool isHidden = false, List<int>? prereqIds = null)
            {
                return new AchievementDef
                {
                    Id = id,
                    CategoryId = categoryId,
                    Name = $"Achievement {id}",
                    Description = "Test",
                    IsHidden = isHidden,
                    SortOrder = 0,
                    PrerequisiteAchievementIds = prereqIds,
                    ConditionGroupIds = null,
                    RewardGroupIds = null
                };
            }

            internal AchievementSystem<uint> System { get; private set; }
            internal readonly Dictionary<int, AchievementDef> Defs = new();

            internal TestAchievementSystem(params int[] achievementIds) : this()
            {
                foreach (var id in achievementIds)
                    Defs[id] = CreateTestDef(id);
            }

            private TestAchievementSystem()
            {
                System = new AchievementSystem<uint>(id => Defs.TryGetValue(id, out var def) ? def : (AchievementDef?)null);
            }

            internal void Rebuild()
            {
                System = new AchievementSystem<uint>(id => Defs.TryGetValue(id, out var def) ? def : (AchievementDef?)null);
            }
        }

        [Test]
        public void RevealAchievement_AfterPrereqs_ReturnsTrue()
        {
            var ts = new TestAchievementSystem(1, 2);
            ts.Defs[2] = TestAchievementSystem.CreateTestDef(2);
            ts.Defs[1] = TestAchievementSystem.CreateTestDef(1, prereqIds: new List<int> { 2 });

            // 先揭示并完成前置成就
            ts.System.RevealAchievement(2, 100u);
            ts.System.CompleteAchievement(2, 100u);
            ts.System.ClaimAchievement(2, 100u);

            // 前置成就 Claimed 后，揭示成就 1
            bool result = ts.System.RevealAchievement(1, 100u);
            Assert.That(result, Is.True);
        }

        [Test]
        public void RevealAchievement_MissingPrereq_ReturnsFalse()
        {
            var ts = new TestAchievementSystem(1, 2);
            ts.Defs[2] = TestAchievementSystem.CreateTestDef(2);
            ts.Defs[1] = TestAchievementSystem.CreateTestDef(1, prereqIds: new List<int> { 2 });

            // 成就 2 未揭示，直接尝试揭示成就 1
            bool result = ts.System.RevealAchievement(1, 100u);
            Assert.That(result, Is.False);
        }

        [Test]
        public void CompleteAchievement_RevealedState_Succeeds()
        {
            var ts = new TestAchievementSystem(1);
            ts.System.RevealAchievement(1, 100u);

            bool result = ts.System.CompleteAchievement(1, 100u);

            Assert.That(result, Is.True);
            var ach = ts.System.GetAchievement(1, 100u);
            Assert.That(ach, Is.Not.Null);
            Assert.That(ach!.State, Is.EqualTo(AchievementState.Completed));
            Assert.That(ach.ProgressPercent, Is.EqualTo(1.0f));
        }

        [Test]
        public void CompleteAchievement_AlreadyClaimed_Fails()
        {
            var ts = new TestAchievementSystem(1);
            ts.System.RevealAchievement(1, 100u);
            ts.System.CompleteAchievement(1, 100u);
            ts.System.ClaimAchievement(1, 100u);

            bool result = ts.System.CompleteAchievement(1, 100u);

            Assert.That(result, Is.False);
        }

        [Test]
        public void CompleteAchievement_NotRevealed_Fails()
        {
            var ts = new TestAchievementSystem(1);
            // 不调用 RevealAchievement，直接 Complete
            bool result = ts.System.CompleteAchievement(1, 100u);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ClaimAchievement_CompletedState_Succeeds()
        {
            var ts = new TestAchievementSystem(1);
            ts.System.RevealAchievement(1, 100u);
            ts.System.CompleteAchievement(1, 100u);

            var result = ts.System.ClaimAchievement(1, 100u);

            Assert.That(result.Success, Is.True);
            var ach = ts.System.GetAchievement(1, 100u);
            Assert.That(ach, Is.Not.Null);
            Assert.That(ach!.State, Is.EqualTo(AchievementState.Claimed));
        }

        [Test]
        public void ClaimAchievement_RevealedState_Fails()
        {
            var ts = new TestAchievementSystem(1);
            ts.System.RevealAchievement(1, 100u);

            var result = ts.System.ClaimAchievement(1, 100u);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void GetByCategory_FiltersCorrectly()
        {
            var ts = new TestAchievementSystem();
            ts.Defs[1] = TestAchievementSystem.CreateTestDef(1, categoryId: 10);
            ts.Defs[2] = TestAchievementSystem.CreateTestDef(2, categoryId: 10);
            ts.Defs[3] = TestAchievementSystem.CreateTestDef(3, categoryId: 20);
            ts.Rebuild();

            ts.System.RevealAchievement(1, 100u);
            ts.System.RevealAchievement(2, 100u);
            ts.System.RevealAchievement(3, 100u);

            var cat10 = ts.System.GetByCategory(10, 100u);
            Assert.That(cat10.Count, Is.EqualTo(2));

            var cat20 = ts.System.GetByCategory(20, 100u);
            Assert.That(cat20.Count, Is.EqualTo(1));

            var cat99 = ts.System.GetByCategory(99, 100u);
            Assert.That(cat99.Count, Is.EqualTo(0));
        }

        [Test]
        public void CompletedCount_ReturnsCorrectCount()
        {
            var ts = new TestAchievementSystem(1, 2);
            ts.System.RevealAchievement(1, 100u);
            ts.System.CompleteAchievement(1, 100u);
            ts.System.ClaimAchievement(1, 100u);

            Assert.That(ts.System.CompletedCount, Is.EqualTo(1));

            ts.System.RevealAchievement(2, 100u);
            Assert.That(ts.System.CompletedCount, Is.EqualTo(1));

            ts.System.CompleteAchievement(2, 100u);
            ts.System.ClaimAchievement(2, 100u);
            Assert.That(ts.System.CompletedCount, Is.EqualTo(2));
        }

        [Test]
        public void StateChangedEvent_FiresOnTransition()
        {
            var ts = new TestAchievementSystem(1);
            AchievementStateChangedEvent<uint> captured = default;
            bool fired = false;
            ts.System.OnAchievementStateChanged += e => { captured = e; fired = true; };

            ts.System.RevealAchievement(1, 100u);

            Assert.That(fired, Is.True);
            Assert.That(captured.NewState, Is.EqualTo(AchievementState.Revealed));
            Assert.That(captured.AchievementId, Is.EqualTo(1));
        }

        [Test]
        public void HiddenAchievement_Reveal_SetsRevealed()
        {
            var ts = new TestAchievementSystem();
            ts.Defs[1] = TestAchievementSystem.CreateTestDef(1, isHidden: true);
            ts.Rebuild();

            ts.System.RevealAchievement(1, 100u);

            var ach = ts.System.GetAchievement(1, 100u);
            Assert.That(ach, Is.Not.Null);
            Assert.That(ach!.State, Is.EqualTo(AchievementState.Revealed));
        }

        [Test]
        public void NonHiddenAchievement_Reveal_SetsRevealed()
        {
            var ts = new TestAchievementSystem();
            ts.Defs[1] = TestAchievementSystem.CreateTestDef(1, isHidden: false);
            ts.Rebuild();

            ts.System.RevealAchievement(1, 100u);

            var ach = ts.System.GetAchievement(1, 100u);
            Assert.That(ach, Is.Not.Null);
            Assert.That(ach!.State, Is.EqualTo(AchievementState.Revealed));
        }
    }
}
