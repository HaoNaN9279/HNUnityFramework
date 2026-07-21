#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Combat;
using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Tests.Level.Logic.Combat
{
    [TestFixture]
    public class AbilitySystemTests
    {
        private static readonly AttributeType HP = new AttributeType(1);
        private static readonly AttributeType ATK = new AttributeType(2);
        private static readonly AttributeType MP = new AttributeType(4);

        private AttributeSet<int> _ownerAttributes;
        private AttributeSet<int> _targetAttributes;
        private EffectPipeline<int> _effectPipeline;
        private AbilitySystem<int> _abilitySystem;

        [SetUp]
        public void SetUp()
        {
            _ownerAttributes = new AttributeSet<int>();
            _ownerAttributes.SetBaseValue(HP, (Fixed64)2000);
            _ownerAttributes.SetBaseValue(ATK, (Fixed64)150);
            _ownerAttributes.SetBaseValue(MP, (Fixed64)100);

            _targetAttributes = new AttributeSet<int>();
            _targetAttributes.SetBaseValue(HP, (Fixed64)1000);
            _targetAttributes.SetBaseValue(ATK, (Fixed64)80);

            _effectPipeline = new EffectPipeline<int>();
            _abilitySystem = new AbilitySystem<int>();
        }

        private AbilitySpec<int> CreateAttackAbility(int abilityId, Fixed64 cooldown,
            Fixed64? cost = null, Fixed64? damage = null)
        {
            var spec = new AbilitySpec<int>
            {
                AbilityId = abilityId,
                DisplayName = $"Ability_{abilityId}",
                Cooldown = cooldown,
            };

            if (cost.HasValue)
            {
                spec.Cost = new Dictionary<AttributeType, Fixed64> { { MP, cost.Value } };
            }

            if (damage.HasValue)
            {
                var effect = new EffectSpec<int>
                {
                    EffectId = abilityId * 100,
                    Type = EffectType.Duration,
                    Duration = (Fixed64)10,
                    AttributeModifiers =
                    {
                        new AttributeModifier<int>(HP, new Modifier<int>(0, ModifierOp.Add,
                            -damage.Value)), // 扣血
                    }
                };
                spec.Effects.Add(effect);
            }

            return spec;
        }

        [Test]
        public void CanActivate_NoCooldown_ReturnsTrue()
        {
            var spec = CreateAttackAbility(1, (Fixed64)0);
            var result = _abilitySystem.CanActivate(spec, 1, _ownerAttributes);
            Assert.That(result, Is.True);
        }

        [Test]
        public void CanActivate_OnCooldown_ReturnsFalse()
        {
            var spec = CreateAttackAbility(2, (Fixed64)5);
            _abilitySystem.RegisterAbility(spec);

            // 激活后进入冷却
            _abilitySystem.TryActivateAbility(spec, 1, 2, _ownerAttributes, _targetAttributes, _effectPipeline);

            // 冷却中
            var result = _abilitySystem.CanActivate(spec, 1, _ownerAttributes);
            Assert.That(result, Is.False);
        }

        [Test]
        public void CanActivate_AfterCooldownExpired_ReturnsTrue()
        {
            var spec = CreateAttackAbility(3, (Fixed64)3);
            _abilitySystem.RegisterAbility(spec);

            _abilitySystem.TryActivateAbility(spec, 1, 2, _ownerAttributes, _targetAttributes, _effectPipeline);

            // 冷却 5 秒
            _abilitySystem.Tick((Fixed64)5);

            var result = _abilitySystem.CanActivate(spec, 1, _ownerAttributes);
            Assert.That(result, Is.True);
        }

        [Test]
        public void CanActivate_InsufficientCost_ReturnsFalse()
        {
            var spec = CreateAttackAbility(4, (Fixed64)0, cost: (Fixed64)999);
            var result = _abilitySystem.CanActivate(spec, 1, _ownerAttributes);
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryActivateAbility_SufficientCost_ConsumesCost()
        {
            var spec = CreateAttackAbility(5, (Fixed64)0, cost: (Fixed64)50);
            var initialMP = _ownerAttributes.GetFinalValue(MP);

            bool activated = _abilitySystem.TryActivateAbility(spec, 1, 2,
                _ownerAttributes, _targetAttributes, _effectPipeline);
            Assert.That(activated, Is.True);

            // MP 应减少 50
            Assert.That(_ownerAttributes.GetFinalValue(MP), Is.EqualTo(initialMP - (Fixed64)50));
        }

        [Test]
        public void TryActivateAbility_InsufficientCost_ReturnsFalse()
        {
            var spec = CreateAttackAbility(6, (Fixed64)0, cost: (Fixed64)999);

            bool activated = _abilitySystem.TryActivateAbility(spec, 1, 2,
                _ownerAttributes, _targetAttributes, _effectPipeline);
            Assert.That(activated, Is.False);
        }

        [Test]
        public void TryActivateAbility_AppliesEffects()
        {
            var spec = CreateAttackAbility(7, (Fixed64)0, damage: (Fixed64)200);
            var initialHP = _targetAttributes.GetFinalValue(HP);

            bool activated = _abilitySystem.TryActivateAbility(spec, 1, 2,
                _ownerAttributes, _targetAttributes, _effectPipeline);
            Assert.That(activated, Is.True);

            // 目标 HP 应减少 200（通过 Duration Effect）
            Assert.That(_targetAttributes.GetFinalValue(HP), Is.EqualTo(initialHP - (Fixed64)200));
        }

        [Test]
        public void TryActivateAbility_StartsCooldown()
        {
            var spec = CreateAttackAbility(8, (Fixed64)10);

            _abilitySystem.TryActivateAbility(spec, 1, 2,
                _ownerAttributes, _targetAttributes, _effectPipeline);

            var remaining = _abilitySystem.GetCooldownRemaining(8, 1);
            Assert.That(remaining, Is.GreaterThan(Fixed64.Zero));
            Assert.That(_abilitySystem.CooldownCount, Is.EqualTo(1));
        }

        [Test]
        public void MultipleAbilities_IndependentCooldowns()
        {
            var specA = CreateAttackAbility(10, (Fixed64)5);
            var specB = CreateAttackAbility(11, (Fixed64)10);

            _abilitySystem.RegisterAbility(specA);
            _abilitySystem.RegisterAbility(specB);

            _abilitySystem.TryActivateAbility(specA, 1, 2, _ownerAttributes, _targetAttributes, _effectPipeline);
            _abilitySystem.TryActivateAbility(specB, 1, 2, _ownerAttributes, _targetAttributes, _effectPipeline);

            Assert.That(_abilitySystem.CooldownCount, Is.EqualTo(2));
        }

        [Test]
        public void Tick_ReducesCooldown()
        {
            var spec = CreateAttackAbility(12, (Fixed64)10);

            _abilitySystem.TryActivateAbility(spec, 1, 2, _ownerAttributes, _targetAttributes, _effectPipeline);

            _abilitySystem.Tick((Fixed64)4);
            var remaining = _abilitySystem.GetCooldownRemaining(12, 1);
            Assert.That(remaining, Is.EqualTo((Fixed64)6));
        }

        [Test]
        public void GetCooldownRemaining_NoCooldown_ReturnsZero()
        {
            var remaining = _abilitySystem.GetCooldownRemaining(99, 1);
            Assert.That(remaining, Is.EqualTo(Fixed64.Zero));
        }

        [Test]
        public void OnAbilityActivated_FiresOnSuccess()
        {
            int fireCount = 0;
            bool activatedResult = false;
            _abilitySystem.OnAbilityActivated += (spec, owner, target, success) =>
            {
                fireCount++;
                activatedResult = success;
            };

            var spec = CreateAttackAbility(13, (Fixed64)0);
            _abilitySystem.TryActivateAbility(spec, 1, 2, _ownerAttributes, _targetAttributes, _effectPipeline);

            Assert.That(fireCount, Is.EqualTo(1));
            Assert.That(activatedResult, Is.True);
        }

        [Test]
        public void OnAbilityActivated_FiresOnFailure()
        {
            int fireCount = 0;
            bool activatedResult = true;
            _abilitySystem.OnAbilityActivated += (spec, owner, target, success) =>
            {
                fireCount++;
                activatedResult = success;
            };

            var spec = CreateAttackAbility(14, (Fixed64)0, cost: (Fixed64)999);
            _abilitySystem.TryActivateAbility(spec, 1, 2, _ownerAttributes, _targetAttributes, _effectPipeline);

            Assert.That(fireCount, Is.EqualTo(1));
            Assert.That(activatedResult, Is.False);
        }
    }
}
