#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Combat;
using System;
using FixedMathSharp;

namespace HN.Framework.Core.Tests.Level.Logic.Combat
{
    [TestFixture]
    public class BuffSystemTests
    {
        private static readonly AttributeType HP = new AttributeType(1);
        private static readonly AttributeType ATK = new AttributeType(2);

        private AttributeSet<int> _attributeSet;
        private EffectPipeline<int> _effectPipeline;
        private BuffSystem<int> _buffSystem;

        [SetUp]
        public void SetUp()
        {
            _attributeSet = new AttributeSet<int>();
            _attributeSet.SetBaseValue(HP, (Fixed64)1000);
            _attributeSet.SetBaseValue(ATK, (Fixed64)100);

            _effectPipeline = new EffectPipeline<int>();
            _buffSystem = new BuffSystem<int>();
        }

        private BuffSpec<int> CreateBuff(int buffId, Fixed64 duration, BuffStackRule rule,
            Fixed64? atkModifier = null, int maxStacks = 1)
        {
            var spec = new BuffSpec<int>
            {
                BuffId = buffId,
                DisplayName = $"Buff_{buffId}",
                Duration = duration,
                StackRule = rule,
                MaxStacks = maxStacks,
            };

            if (atkModifier.HasValue)
            {
                var effect = new EffectSpec<int>
                {
                    EffectId = buffId * 100,
                    Type = EffectType.Duration,
                    Duration = duration,
                    AttributeModifiers =
                    {
                        new AttributeModifier<int>(ATK, new Modifier<int>(0, ModifierOp.Add, atkModifier.Value)),
                    }
                };
                spec.ApplyEffects.Add(effect);
            }

            return spec;
        }

        [Test]
        public void ApplyBuff_Single_AppliesEffects()
        {
            var spec = CreateBuff(1, (Fixed64)10, BuffStackRule.Single, (Fixed64)50);
            var handle = _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);

            Assert.That(handle.IsValid, Is.True);
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));
            // ATK 应增加 50（通过 EffectPipeline 应用）
            Assert.That(_attributeSet.GetFinalValue(ATK), Is.EqualTo((Fixed64)150));
        }

        [Test]
        public void ApplyBuff_Single_OverridesExisting()
        {
            var spec1 = CreateBuff(1, (Fixed64)10, BuffStackRule.Single, (Fixed64)50);
            var handle1 = _buffSystem.ApplyBuff(spec1, 0, 1, _attributeSet, _effectPipeline);

            // 再次应用相同 ID 的 Buff
            var spec2 = CreateBuff(1, (Fixed64)10, BuffStackRule.Single, (Fixed64)100);
            var handle2 = _buffSystem.ApplyBuff(spec2, 0, 1, _attributeSet, _effectPipeline);

            // Single 规则下旧的被覆盖，新的生效
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));
            // 最终效果由新的 buff 决定
        }

        [Test]
        public void ApplyBuff_Multi_StacksUpToMax()
        {
            var spec = CreateBuff(2, (Fixed64)10, BuffStackRule.Multi, (Fixed64)30, maxStacks: 3);

            // 第一次应用
            _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));

            // 第二次应用（堆叠）
            _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));

            // 第三次应用（堆叠到上限）
            _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));

            // 第四次应用（已达 MaxStacks=3，忽略）
            _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));
        }

        [Test]
        public void ApplyBuff_Refresh_ResetsDuration()
        {
            var spec = CreateBuff(3, (Fixed64)10, BuffStackRule.Refresh, (Fixed64)20);
            var handle = _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);

            // Tick 5 秒
            _buffSystem.Tick((Fixed64)5, _attributeSet, _effectPipeline);
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));

            // 再次应用（刷新）
            _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);

            // 再 Tick 8 秒（如果没刷新应该已经到期）
            _buffSystem.Tick((Fixed64)8, _attributeSet, _effectPipeline);
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));

            // 再 Tick 3 秒（总共 5+8+3=16 > 10，应到期）
            _buffSystem.Tick((Fixed64)3, _attributeSet, _effectPipeline);
            // 刷新后剩余 10 秒，已过 8+3=11 → 到期
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(0));
        }

        [Test]
        public void ApplyBuff_Extend_AddsDuration()
        {
            var spec = CreateBuff(4, (Fixed64)5, BuffStackRule.Extend, (Fixed64)10);
            spec.ExtraDurationPerStack = (Fixed64)3;

            var handle = _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);

            // Tick 4 秒
            _buffSystem.Tick((Fixed64)4, _attributeSet, _effectPipeline);
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));

            // 再次应用（延长 3 秒）
            _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);

            // 原来的剩余时间 1 秒 + 延长 3 秒 = 4 秒
            _buffSystem.Tick((Fixed64)3, _attributeSet, _effectPipeline);
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));

            // 再 Tick 2 秒（应到期）
            _buffSystem.Tick((Fixed64)2, _attributeSet, _effectPipeline);
            // 剩余 4 - 3 - 2 = -1 → 到期
        }

        [Test]
        public void RemoveBuff_ManuallyRemoves()
        {
            var spec = CreateBuff(5, (Fixed64)10, BuffStackRule.Single, (Fixed64)50);
            var handle = _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);

            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));

            bool removed = _buffSystem.RemoveBuff(handle, _attributeSet, _effectPipeline);
            Assert.That(removed, Is.True);
            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(0));
        }

        [Test]
        public void RemoveBuff_InvalidHandle_ReturnsFalse()
        {
            bool removed = _buffSystem.RemoveBuff(BuffHandle.Invalid, _attributeSet, _effectPipeline);
            Assert.That(removed, Is.False);
        }

        [Test]
        public void Tick_ExpiredBuff_RemovesAutomatically()
        {
            var spec = CreateBuff(6, (Fixed64)3, BuffStackRule.Single, (Fixed64)100);
            _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);

            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(1));

            _buffSystem.Tick((Fixed64)5, _attributeSet, _effectPipeline);

            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(0));
        }

        [Test]
        public void OnBuffApplied_Fires()
        {
            int fireCount = 0;
            _buffSystem.OnBuffApplied += (spec, src, tgt, stacks) => fireCount++;

            var spec = CreateBuff(7, (Fixed64)10, BuffStackRule.Single);
            _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);

            Assert.That(fireCount, Is.EqualTo(1));
        }

        [Test]
        public void OnBuffRemoved_FiresOnExpiry()
        {
            int fireCount = 0;
            _buffSystem.OnBuffRemoved += (spec, src, tgt, stacks, expired) => fireCount++;

            var spec = CreateBuff(8, (Fixed64)1, BuffStackRule.Single);
            _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);
            _buffSystem.Tick((Fixed64)2, _attributeSet, _effectPipeline);

            Assert.That(fireCount, Is.EqualTo(1));
        }

        [Test]
        public void MultipleBuffs_DifferentTargets_Independent()
        {
            var attrSet2 = new AttributeSet<int>();
            attrSet2.SetBaseValue(ATK, (Fixed64)50);

            var spec = CreateBuff(9, (Fixed64)10, BuffStackRule.Single, (Fixed64)20);

            _buffSystem.ApplyBuff(spec, 0, 1, _attributeSet, _effectPipeline);
            _buffSystem.ApplyBuff(spec, 0, 2, attrSet2, _effectPipeline);

            Assert.That(_buffSystem.ActiveCount, Is.EqualTo(2));
        }
    }
}
