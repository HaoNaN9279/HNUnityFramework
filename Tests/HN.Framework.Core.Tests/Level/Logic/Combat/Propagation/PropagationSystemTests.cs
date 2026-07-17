#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Combat;
using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Tests.Level.Logic.Combat.Propagation
{
    /// <summary>
    /// 固定目标策略：始终返回预设的目标列表。
    /// </summary>
    public sealed class FixedTargetStrategy<TId> : IPropagationStrategy<TId> where TId : IEquatable<TId>
    {
        private readonly List<TId> _targets;

        public FixedTargetStrategy(params TId[] targets)
        {
            _targets = new List<TId>(targets);
        }

        public void FindTargets(PropagationContext<TId> context, List<TId> results)
        {
            results.AddRange(_targets);
        }
    }

    /// <summary>
    /// 递增目标策略：每调用一次返回列表中的下一个目标。
    /// </summary>
    public sealed class ProgressiveTargetStrategy<TId> : IPropagationStrategy<TId> where TId : IEquatable<TId>
    {
        private readonly List<TId> _targets;
        private int _index;

        public ProgressiveTargetStrategy(List<TId> targets)
        {
            _targets = targets;
        }

        public void FindTargets(PropagationContext<TId> context, List<TId> results)
        {
            if (_index < _targets.Count)
                results.Add(_targets[_index++]);
        }
    }

    /// <summary>
    /// 排除过滤器：过滤掉指定的目标。
    /// </summary>
    public sealed class ExcludeFilter<TId> : IPropagationFilter<TId> where TId : IEquatable<TId>
    {
        private readonly TId _excluded;

        public ExcludeFilter(TId excluded)
        {
            _excluded = excluded;
        }

        public bool Pass(TId target, PropagationContext<TId> context)
        {
            return !EqualityComparer<TId>.Default.Equals(target, _excluded);
        }
    }

    [TestFixture]
    public class PropagationSystemTests
    {
        private static readonly AttributeType HP = new AttributeType(1);

        private PropagationSpec<int> _spec = null!;
        private AttributeSet<int> _mockAttrs = null!;
        private EffectPipeline<int> _mockPipeline = null!;
        private PropagationSystem<int> _system = null!;

        [SetUp]
        public void SetUp()
        {
            _spec = new PropagationSpec<int>
            {
                PropagationId = 1,
                MaxHops = 1,
                IntensityDecay = Fixed64.One,
            };
            _spec.OnHitEffects.Clear();

            _mockAttrs = new AttributeSet<int>();
            _mockAttrs.SetBaseValue(HP, Fixed64.MaxValue);

            _mockPipeline = new EffectPipeline<int>();

            _system = new PropagationSystem<int>(id => _mockAttrs);
        }

        [Test]
        public void EmptyStrategy_NoHits_ReturnsFailure()
        {
            var strategy = new FixedTargetStrategy<int>();

            var result = _system.Propagate(_spec, strategy, null, 0, null, _mockPipeline, null);

            Assert.That(result.Success, Is.False);
            Assert.That(result.HitEntities, Is.Empty);
            Assert.That(result.TotalHops, Is.EqualTo(1));
        }

        [Test]
        public void SingleHop_SingleTarget_HitsTarget()
        {
            _spec.MaxHops = 1;
            var strategy = new FixedTargetStrategy<int>(42);

            var result = _system.Propagate(_spec, strategy, null, 0, null, _mockPipeline, null);

            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalHops, Is.EqualTo(1));
            Assert.That(result.HitEntities.Count, Is.EqualTo(1));
            Assert.That(result.HitEntities[0], Is.EqualTo(42));
        }

        [Test]
        public void MaxHopsLimit_Respected_StopsAtLimit()
        {
            _spec.MaxHops = 3;
            var targets = new List<int> { 10, 20, 30, 40, 50 };
            var strategy = new ProgressiveTargetStrategy<int>(targets);

            var result = _system.Propagate(_spec, strategy, null, 0, null, _mockPipeline, null);

            Assert.That(result.TotalHops, Is.EqualTo(3));
            Assert.That(result.HitEntities.Count, Is.EqualTo(3));
            Assert.That(result.HitEntities[0], Is.EqualTo(10));
            Assert.That(result.HitEntities[1], Is.EqualTo(20));
            Assert.That(result.HitEntities[2], Is.EqualTo(30));
        }

        [Test]
        public void HitEntities_Deduplicates_SameTargetOnce()
        {
            _spec.MaxHops = 3;
            var strategy = new FixedTargetStrategy<int>(99);

            var result = _system.Propagate(_spec, strategy, null, 0, null, _mockPipeline, null);

            Assert.That(result.HitEntities.Count, Is.EqualTo(1));
            Assert.That(result.HitEntities[0], Is.EqualTo(99));
            Assert.That(result.TotalHops, Is.EqualTo(2));
        }

        [Test]
        public void IntensityMultiplier_Decays_OnEachHop()
        {
            _spec.MaxHops = 3;
            _spec.IntensityDecay = (Fixed64)0.5;

            var targets = new List<int> { 10, 20, 30 };
            var strategy = new ProgressiveTargetStrategy<int>(targets);

            var multipliers = new List<Fixed64>();
            _system.OnTargetHit += ctx => multipliers.Add(ctx.IntensityMultiplier);

            _system.Propagate(_spec, strategy, null, 0, null, _mockPipeline, null);

            Assert.That(multipliers.Count, Is.EqualTo(3));
            Assert.That(multipliers[0], Is.EqualTo(Fixed64.One));
            Assert.That(multipliers[1], Is.EqualTo((Fixed64)0.5));
            Assert.That(multipliers[2], Is.EqualTo((Fixed64)0.25));
        }

        [Test]
        public void Filter_FiltersOutTargets_ExcludedNotHit()
        {
            _spec.MaxHops = 1;
            var strategy = new FixedTargetStrategy<int>(10, 20, 30);
            var filter = new ExcludeFilter<int>(20);

            var result = _system.Propagate(_spec, strategy, filter, 0, null, _mockPipeline, null);

            Assert.That(result.HitEntities.Count, Is.EqualTo(2));
            Assert.That(result.HitEntities, Has.Member(10));
            Assert.That(result.HitEntities, Has.No.Member(20));
            Assert.That(result.HitEntities, Has.Member(30));
        }

        [Test]
        public void Events_FireCorrectly_InExpectedOrder()
        {
            _spec.MaxHops = 2;
            var targets = new List<int> { 100, 200 };
            var strategy = new ProgressiveTargetStrategy<int>(targets);

            var eventLog = new List<string>();
            _system.OnPropagationStarted += ctx => eventLog.Add("Started");
            _system.OnTargetHit += ctx => eventLog.Add("Hit");
            _system.OnPropagationComplete += (ctx, res) => eventLog.Add("Complete");

            _system.Propagate(_spec, strategy, null, 0, null, _mockPipeline, null);

            Assert.That(eventLog.Count, Is.GreaterThanOrEqualTo(4));
            Assert.That(eventLog[0], Is.EqualTo("Started"));
            Assert.That(eventLog[1], Is.EqualTo("Hit"));
            Assert.That(eventLog[2], Is.EqualTo("Hit"));
            Assert.That(eventLog[eventLog.Count - 1], Is.EqualTo("Complete"));
        }

        [Test]
        public void InvalidSpec_MaxHopsLessThanOne_ReturnsEmpty()
        {
            _spec.MaxHops = 0;
            var strategy = new FixedTargetStrategy<int>(42);

            var result = _system.Propagate(_spec, strategy, null, 0, null, _mockPipeline, null);

            Assert.That(result.Success, Is.False);
            Assert.That(result.HitEntities, Is.Empty);
            Assert.That(result.TotalHops, Is.EqualTo(0));
            Assert.That(result.Handle, Is.EqualTo(PropagationHandle.Invalid));
        }
    }
}
