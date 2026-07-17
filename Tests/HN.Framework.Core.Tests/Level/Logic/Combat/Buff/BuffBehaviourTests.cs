#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;
using HN.Framework.Core.Level.Logic.Combat;
using NUnit.Framework;

namespace HN.Framework.Core.Tests.Level.Logic.Combat
{
    /// <summary>
    /// 测试行为辅助类。记录 IBuffBehaviour 生命周期调用的次数和参数。
    /// </summary>
    public sealed class TestBuffBehaviour<TId> : IBuffBehaviour<TId> where TId : IEquatable<TId>
    {
        public int ApplyCalls;
        public int TickCalls;
        public int RemoveCalls;
        public int StackChangedCalls;
        public bool LastIsExpired;
        public int LastOldStacks;
        public int LastNewStacks;
        public List<Fixed64> TickDeltas = new();

        public void OnApply(BuffBehaviourContext<TId> context)
        {
            ApplyCalls++;
        }

        public void OnTick(Fixed64 deltaTime, BuffBehaviourContext<TId> context)
        {
            TickCalls++;
            TickDeltas.Add(deltaTime);
        }

        public void OnRemove(bool isExpired, BuffBehaviourContext<TId> context)
        {
            RemoveCalls++;
            LastIsExpired = isExpired;
        }

        public void OnStackChanged(int oldStacks, int newStacks, BuffBehaviourContext<TId> context)
        {
            StackChangedCalls++;
            LastOldStacks = oldStacks;
            LastNewStacks = newStacks;
        }

        public void Reset()
        {
            ApplyCalls = 0;
            TickCalls = 0;
            RemoveCalls = 0;
            StackChangedCalls = 0;
            TickDeltas.Clear();
        }
    }

    /// <summary>
    /// BuffBehaviour 集成测试。验证 BuffSystem 在正确的生命周期点调用 IBuffBehaviour。
    /// </summary>
    [TestFixture]
    public class BuffBehaviourTests
    {
        private BuffSystem<int> _buffSystem = null!;
        private TestBuffBehaviour<int> _behaviour = null!;
        private IAttributeSet<int> _targetAttrs = null!;
        private IEffectPipeline<int> _effectPipeline = null!;

        [SetUp]
        public void Setup()
        {
            _behaviour = new TestBuffBehaviour<int>();
            _buffSystem = new BuffSystem<int>(id => _behaviour);
            _targetAttrs = new AttributeSet<int>();
            _targetAttrs.SetBaseValue(new AttributeType(1), Fixed64.MaxValue);
            _effectPipeline = new EffectPipeline<int>();
        }

        [TearDown]
        public void Teardown()
        {
            _behaviour.Reset();
        }

        /// <summary>
        /// 应用 Buff 时应当调用 Behaviour.OnApply。
        /// </summary>
        [Test]
        public void OnApply_CalledWhenBuffApplied()
        {
            var spec = CreateBuffSpec(behaviourId: 1, duration: (Fixed64)10.0);
            _buffSystem.ApplyBuff(spec, 1, 2, _targetAttrs, _effectPipeline);

            Assert.That(_behaviour.ApplyCalls, Is.EqualTo(1), "OnApply should be called once when Buff is applied.");
        }

        /// <summary>
        /// Tick 时应当调用活跃 Buff 的 Behaviour.OnTick。
        /// </summary>
        [Test]
        public void OnTick_CalledEachTick()
        {
            var spec = CreateBuffSpec(behaviourId: 1, duration: (Fixed64)10.0);
            _buffSystem.ApplyBuff(spec, 1, 2, _targetAttrs, _effectPipeline);

            _buffSystem.Tick((Fixed64)1.0, _targetAttrs, _effectPipeline);

            Assert.That(_behaviour.TickCalls, Is.GreaterThanOrEqualTo(1), "OnTick should be called on Tick().");
            Assert.That(_behaviour.TickDeltas[0], Is.EqualTo((Fixed64)1.0), "Tick deltaTime should match.");
        }

        /// <summary>
        /// Buff 到期时应当调用 OnRemove(isExpired=true)。
        /// </summary>
        [Test]
        public void OnRemove_CalledWhenExpired()
        {
            var spec = CreateBuffSpec(behaviourId: 1, duration: (Fixed64)0.1);
            _buffSystem.ApplyBuff(spec, 1, 2, _targetAttrs, _effectPipeline);

            // 推进时间使 Buff 到期
            _buffSystem.Tick((Fixed64)0.2, _targetAttrs, _effectPipeline);

            Assert.That(_behaviour.RemoveCalls, Is.EqualTo(1), "OnRemove should be called when Buff expires.");
            Assert.That(_behaviour.LastIsExpired, Is.True, "isExpired should be true when Buff expires naturally.");
        }

        /// <summary>
        /// 手动移除 Buff 时应当调用 OnRemove(isExpired=false)。
        /// </summary>
        [Test]
        public void OnRemove_CalledWhenManuallyRemoved()
        {
            var spec = CreateBuffSpec(behaviourId: 1, duration: (Fixed64)10.0);
            var handle = _buffSystem.ApplyBuff(spec, 1, 2, _targetAttrs, _effectPipeline);

            _behaviour.Reset(); // 清除 Apply 阶段的调用计数
            _buffSystem.RemoveBuff(handle, _targetAttrs, _effectPipeline);

            Assert.That(_behaviour.RemoveCalls, Is.EqualTo(1), "OnRemove should be called when Buff is manually removed.");
            Assert.That(_behaviour.LastIsExpired, Is.False, "isExpired should be false when manually removed.");
        }

        /// <summary>
        /// Multi 堆叠规则中层数变更时应调用 OnStackChanged。
        /// </summary>
        [Test]
        public void OnStackChanged_CalledForMultiStack()
        {
            var spec = CreateBuffSpec(behaviourId: 1, duration: (Fixed64)10.0,
                stackRule: BuffStackRule.Multi, maxStacks: 3);
            _buffSystem.ApplyBuff(spec, 1, 2, _targetAttrs, _effectPipeline);

            _behaviour.Reset(); // 清除 Apply 阶段的调用计数
            _buffSystem.ApplyBuff(spec, 1, 2, _targetAttrs, _effectPipeline); // 第二次 → 层数 2

            Assert.That(_behaviour.StackChangedCalls, Is.EqualTo(1), "OnStackChanged should be called when stack count increases.");
            Assert.That(_behaviour.LastOldStacks, Is.EqualTo(1), "Old stacks should be 1.");
            Assert.That(_behaviour.LastNewStacks, Is.EqualTo(2), "New stacks should be 2.");
        }

        /// <summary>
        /// BehaviourId=0 时不应调用任何 Behaviour 方法，且不抛异常。
        /// </summary>
        [Test]
        public void NoBehaviour_NoException()
        {
            // 使用无参构造器（factory=null）
            var system = new BuffSystem<int>();
            var spec = CreateBuffSpec(behaviourId: 0, duration: (Fixed64)10.0);

            Assert.DoesNotThrow(() =>
            {
                var handle = system.ApplyBuff(spec, 1, 2, _targetAttrs, _effectPipeline);
                system.Tick((Fixed64)1.0, _targetAttrs, _effectPipeline);
                system.RemoveBuff(handle, _targetAttrs, _effectPipeline);
            }, "No exception should be thrown when no Behaviour is configured.");
        }

        /// <summary>
        /// BehaviourCustomParams 应当被正确传递到 BuffBehaviourContext。
        /// </summary>
        [Test]
        public void CustomParams_ArePassedToContext()
        {
            var receivedParams = new Dictionary<string, Fixed64>();
            var system = new BuffSystem<int>(_ => new CustomParamCapturingBehaviour<int>(receivedParams));

            var spec = CreateBuffSpec(behaviourId: 1, duration: (Fixed64)10.0);
            spec.BehaviourCustomParams = new Dictionary<string, Fixed64>
            {
                { "bonus", (Fixed64)0.5 },
                { "count", (Fixed64)3 },
            };

            system.ApplyBuff(spec, 1, 2, _targetAttrs, _effectPipeline);

            Assert.That(receivedParams.ContainsKey("bonus"), Is.True, "CustomParams should contain 'bonus'.");
            Assert.That(receivedParams.ContainsKey("count"), Is.True, "CustomParams should contain 'count'.");
        }

        // ==================== 辅助方法 ====================

        private static BuffSpec<int> CreateBuffSpec(
            int behaviourId,
            Fixed64 duration,
            BuffStackRule stackRule = BuffStackRule.Single,
            int maxStacks = 1)
        {
            return new BuffSpec<int>
            {
                BuffId = 1,
                BehaviourId = behaviourId,
                DisplayName = "TestBuff",
                Duration = duration,
                StackRule = stackRule,
                MaxStacks = maxStacks,
                ApplyEffects = new List<EffectSpec<int>>(),
                RemoveEffects = new List<EffectSpec<int>>(),
            };
        }
    }

    /// <summary>
    /// 辅助类：在 OnApply 时记录 CustomParams。
    /// </summary>
    public sealed class CustomParamCapturingBehaviour<TId> : IBuffBehaviour<TId> where TId : IEquatable<TId>
    {
        private readonly Dictionary<string, Fixed64> _capturedParams;

        public CustomParamCapturingBehaviour(Dictionary<string, Fixed64> capturedParams)
        {
            _capturedParams = capturedParams;
        }

        public void OnApply(BuffBehaviourContext<TId> context)
        {
            if (context.CustomParams != null)
            {
                foreach (var kvp in context.CustomParams)
                {
                    _capturedParams[kvp.Key] = kvp.Value;
                }
            }
        }

        public void OnTick(Fixed64 deltaTime, BuffBehaviourContext<TId> context) { }
        public void OnRemove(bool isExpired, BuffBehaviourContext<TId> context) { }
        public void OnStackChanged(int oldStacks, int newStacks, BuffBehaviourContext<TId> context) { }
    }
}
