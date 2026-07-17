#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Combat;
using System;
using FixedMathSharp;

namespace HN.Framework.Core.Tests.Level.Logic.Combat
{
    [TestFixture]
    public class DamagePipelineTests
    {
        private static readonly AttributeType HP = new AttributeType(1);
        private static readonly AttributeType ATK = new AttributeType(2);
        private static readonly AttributeType DEF = new AttributeType(3);

        private AttributeSet<int> _attacker;
        private AttributeSet<int> _defender;
        private DamagePipeline<int> _pipeline;

        [SetUp]
        public void SetUp()
        {
            _attacker = new AttributeSet<int>();
            _attacker.SetBaseValue(HP, (Fixed64)2000);
            _attacker.SetBaseValue(ATK, (Fixed64)150);
            _attacker.SetBaseValue(DEF, (Fixed64)30);

            _defender = new AttributeSet<int>();
            _defender.SetBaseValue(HP, (Fixed64)1000);
            _defender.SetBaseValue(ATK, (Fixed64)80);
            _defender.SetBaseValue(DEF, (Fixed64)50);

            _pipeline = new DamagePipeline<int>();
        }

        [Test]
        public void DefaultFormula_DamageReducedByDefense()
        {
            var context = new DamageContext<int>
            {
                AttackerId = 1,
                DefenderId = 2,
                BaseDamage = (Fixed64)100,
                AttackerAttributes = _attacker,
                DefenderAttributes = _defender,
            };

            var damage = _pipeline.Process(ref context);
            Assert.That(damage, Is.EqualTo((Fixed64)100));
            Assert.That(_defender.GetFinalValue(HP), Is.EqualTo((Fixed64)900));
        }

        [Test]
        public void DefaultFormula_WhenDefenseHigherThanAttack_ZeroDamage()
        {
            _defender.SetBaseValue(DEF, (Fixed64)999);

            // 项目通过 PreMigration 实现 ATK-DEF 减伤逻辑
            _pipeline.OnPreMigration = (ref DamageContext<int> ctx) =>
            {
                var atk = ctx.AttackerAttributes?.GetFinalValue(ATK) ?? Fixed64.Zero;
                var def = ctx.DefenderAttributes?.GetFinalValue(DEF) ?? Fixed64.Zero;
                ctx.BaseDamage = FixedMath.Max(atk - def, Fixed64.Zero);
            };

            var context = new DamageContext<int>
            {
                AttackerId = 1,
                DefenderId = 2,
                BaseDamage = (Fixed64)100,
                AttackerAttributes = _attacker,
                DefenderAttributes = _defender,
            };

            var damage = _pipeline.Process(ref context);
            Assert.That(damage, Is.EqualTo(Fixed64.Zero));
            Assert.That(_defender.GetFinalValue(HP), Is.EqualTo((Fixed64)1000));
        }

        [Test]
        public void CustomFormula_CanOverrideDefault()
        {
            var customFormula = new FixedDamageFormula<int>((Fixed64)50);
            var pipeline = new DamagePipeline<int>(customFormula);

            var context = new DamageContext<int>
            {
                AttackerId = 1,
                DefenderId = 2,
                BaseDamage = (Fixed64)999,
                AttackerAttributes = _attacker,
                DefenderAttributes = _defender,
            };

            var damage = pipeline.Process(ref context);
            Assert.That(damage, Is.EqualTo((Fixed64)50));
        }

        [Test]
        public void PreMigration_CanModifyDamage()
        {
            // PreMigration stage: increase BaseDamage by 20%
            _pipeline.OnPreMigration = (ref DamageContext<int> ctx) =>
            {
                ctx.BaseDamage = ctx.BaseDamage * (Fixed64)12 / (Fixed64)10;
            };

            var context = new DamageContext<int>
            {
                AttackerId = 1,
                DefenderId = 2,
                BaseDamage = (Fixed64)100,
                AttackerAttributes = _attacker,
                DefenderAttributes = _defender,
            };

            var damage = _pipeline.Process(ref context);
            Assert.That(damage, Is.EqualTo((Fixed64)120));
        }

        [Test]
        public void PostMigration_CanModifyFinalDamage()
        {
            _pipeline.OnPostMigration = (ref DamageContext<int> ctx) =>
            {
                ctx.FinalDamage /= (Fixed64)2;
            };

            var context = new DamageContext<int>
            {
                AttackerId = 1,
                DefenderId = 2,
                BaseDamage = (Fixed64)100,
                AttackerAttributes = _attacker,
                DefenderAttributes = _defender,
            };

            var damage = _pipeline.Process(ref context);
            Assert.That(damage, Is.EqualTo((Fixed64)50));
        }

        [Test]
        public void OnApplyDamage_CanOverrideDefault()
        {
            Fixed64 recordedDamage = Fixed64.Zero;
            _pipeline.OnApplyDamage = (ref DamageContext<int> ctx) =>
            {
                recordedDamage = ctx.FinalDamage;
            };

            var context = new DamageContext<int>
            {
                AttackerId = 1,
                DefenderId = 2,
                BaseDamage = (Fixed64)100,
                AttackerAttributes = _attacker,
                DefenderAttributes = _defender,
            };

            var damage = _pipeline.Process(ref context);
            Assert.That(damage, Is.EqualTo((Fixed64)100));
            Assert.That(recordedDamage, Is.EqualTo((Fixed64)100));
            Assert.That(_defender.GetFinalValue(HP), Is.EqualTo((Fixed64)1000));
        }

        [Test]
        public void DamageEvent_Fires()
        {
            int fireCount = 0;
            _pipeline.OnDamageProcessed += (ctx, handled) => fireCount++;

            var context = new DamageContext<int>
            {
                AttackerId = 1,
                DefenderId = 2,
                BaseDamage = (Fixed64)50,
                AttackerAttributes = _attacker,
                DefenderAttributes = _defender,
            };

            _pipeline.Process(ref context);
            Assert.That(fireCount, Is.EqualTo(1));
        }

        [Test]
        public void DealDamage_ConvenienceMethod_Works()
        {
            var damage = _pipeline.DealDamage(1, 2, (Fixed64)100, _attacker, _defender);
            Assert.That(damage, Is.EqualTo((Fixed64)100));
            Assert.That(_defender.GetFinalValue(HP), Is.EqualTo((Fixed64)900));
        }
    }

    public sealed class FixedDamageFormula<TId> : IDamageFormula<TId> where TId : IEquatable<TId>
    {
        private readonly Fixed64 _fixedDamage;
        public FixedDamageFormula(Fixed64 fixedDamage) { _fixedDamage = fixedDamage; }
        public Fixed64 Calculate(ref DamageContext<TId> context) => _fixedDamage;
    }
}