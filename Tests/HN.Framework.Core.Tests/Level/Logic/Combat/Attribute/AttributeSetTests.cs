#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Combat;
using System;
using FixedMathSharp;

namespace HN.Framework.Core.Tests.Level.Logic.Combat
{
    [TestFixture]
    public class AttributeSetTests
    {
        private static readonly AttributeType HP = new AttributeType(1);
        private static readonly AttributeType ATK = new AttributeType(2);
        private static readonly AttributeType DEF = new AttributeType(3);

        private AttributeSet<int> _set;

        [SetUp]
        public void SetUp()
        {
            _set = new AttributeSet<int>();
            _set.SetBaseValue(HP, (Fixed64)1000);
            _set.SetBaseValue(ATK, (Fixed64)100);
            _set.SetBaseValue(DEF, (Fixed64)50);
        }

        [Test]
        public void GetBaseValue_AfterSet_ReturnsCorrectValue()
        {
            Assert.That(_set.GetBaseValue(HP), Is.EqualTo((Fixed64)1000));
        }

        [Test]
        public void GetBaseValue_Unregistered_ReturnsZero()
        {
            var unknown = new AttributeType(99);
            Assert.That(_set.GetBaseValue(unknown), Is.EqualTo(Fixed64.Zero));
        }

        [Test]
        public void GetFinalValue_NoModifier_EqualsBaseValue()
        {
            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)1000));
        }

        [Test]
        public void AddModifier_AddOp_IncreasesFinalValue()
        {
            _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.Add, (Fixed64)200));
            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)1200));
        }

        [Test]
        public void AddModifier_MultiplyOp_MultipliesFinalValue()
        {
            _set.AddModifier(ATK, new Modifier<int>(0, ModifierOp.Multiply, (Fixed64)1.5));
            // 100 * 1.5 = 150
            Assert.That(_set.GetFinalValue(ATK), Is.EqualTo((Fixed64)150));
        }

        [Test]
        public void AddModifier_AddAndMultiply_ComputedCorrectly()
        {
            _set.AddModifier(ATK, new Modifier<int>(0, ModifierOp.Add, (Fixed64)50));
            _set.AddModifier(ATK, new Modifier<int>(0, ModifierOp.Multiply, (Fixed64)2));
            // (100 + 50) * 2 = 300
            Assert.That(_set.GetFinalValue(ATK), Is.EqualTo((Fixed64)300));
        }

        [Test]
        public void AddModifier_Override_ReplacesBaseValue()
        {
            _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.Override, (Fixed64)5000));
            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)5000));
        }

        [Test]
        public void MultipleOverride_HighestPriority_Wins()
        {
            _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.Override, (Fixed64)2000, priority: 0));
            _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.Override, (Fixed64)3000, priority: 10));
            // 优先�?10 �?Override 生效
            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)3000));
        }

        [Test]
        public void AddModifier_MinCap_ClampsBelowMinimum()
        {
            // 先降低到 100
            _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.Add, (Fixed64)(-1500)));
            // 设置下限�?200
            _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.MinCap, (Fixed64)200));
            // -500 �?MinCap(200) �?200
            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)200));
        }

        [Test]
        public void AddModifier_MaxCap_ClampsAboveMaximum()
        {
            _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.Add, (Fixed64)5000));
            _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.MaxCap, (Fixed64)3000));
            // 6000 �?MaxCap(3000) �?3000
            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)3000));
        }

        [Test]
        public void AddPriority_MultipleAdds_SortedByPriority()
        {
            // 优先�?10 的后加但是值大 (300)，优先级 0 的先加值小 (100)
            // 但计算按 Priority 排序�?00 + 200 + 300 = 1600
            _set.AddModifier(HP, new Modifier<int>(1, ModifierOp.Add, (Fixed64)300, priority: 10));
            _set.AddModifier(HP, new Modifier<int>(2, ModifierOp.Add, (Fixed64)200, priority: 5));
            _set.AddModifier(HP, new Modifier<int>(3, ModifierOp.Add, (Fixed64)100, priority: 0));
            // 1000 + 100(P0) + 200(P5) + 300(P10) = 1600
            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)1600));
        }

        [Test]
        public void RemoveModifier_AfterRemoval_ValueReturns()
        {
            var handle = _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.Add, (Fixed64)500));
            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)1500));

            bool removed = _set.RemoveModifier(handle);
            Assert.That(removed, Is.True);
            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)1000));
        }

        [Test]
        public void RemoveModifier_InvalidHandle_ReturnsFalse()
        {
            bool removed = _set.RemoveModifier(ModifierHandle.Invalid);
            Assert.That(removed, Is.False);
        }

        [Test]
        public void RemoveModifier_AlreadyRemoved_ReturnsFalse()
        {
            var handle = _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.Add, (Fixed64)500));
            _set.RemoveModifier(handle);
            bool removedAgain = _set.RemoveModifier(handle);
            Assert.That(removedAgain, Is.False);
        }

        [Test]
        public void SetBaseValue_TriggersRecalculation()
        {
            _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.Add, (Fixed64)500));
            _set.SetBaseValue(HP, (Fixed64)2000);
            // 2000 + 500 = 2500
            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)2500));
        }

        [Test]
        public void OnValueChanged_FiresOnModification()
        {
            int fireCount = 0;
            AttributeType changedType = AttributeType.Empty;
            Fixed64 oldVal = Fixed64.Zero;
            Fixed64 newVal = Fixed64.Zero;

            _set.OnValueChanged += (type, oldv, newv) =>
            {
                fireCount++;
                changedType = type;
                oldVal = oldv;
                newVal = newv;
            };

            _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.Add, (Fixed64)300));

            Assert.That(fireCount, Is.EqualTo(1));
            Assert.That(changedType, Is.EqualTo(HP));
            Assert.That(newVal, Is.EqualTo((Fixed64)1300));
        }

        [Test]
        public void MultipleAttributes_IndependentValues()
        {
            _set.AddModifier(HP, new Modifier<int>(0, ModifierOp.Add, (Fixed64)500));
            _set.AddModifier(ATK, new Modifier<int>(0, ModifierOp.Multiply, (Fixed64)2));

            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)1500));
            Assert.That(_set.GetFinalValue(ATK), Is.EqualTo((Fixed64)200)); // 100 * 2
            Assert.That(_set.GetFinalValue(DEF), Is.EqualTo((Fixed64)50)); // unchanged
        }

        [Test]
        public void AddAndMultiply_CalculationOrder_Correct()
        {
            // 计算顺序：Override �?Add �?Multiply �?MinCap �?MaxCap
            _set.AddModifier(ATK, new Modifier<int>(0, ModifierOp.Add, (Fixed64)50));
            _set.AddModifier(ATK, new Modifier<int>(0, ModifierOp.Multiply, (Fixed64)2));
            _set.AddModifier(ATK, new Modifier<int>(0, ModifierOp.Add, (Fixed64)30));
            // (100 + 50 + 30) * 2 = 360
            Assert.That(_set.GetFinalValue(ATK), Is.EqualTo((Fixed64)360));
        }

        [Test]
        public void DiffAttributeType_DiffSources_Stacks()
        {
            // 来源1 �?来源2 各自�?HP
            _set.AddModifier(HP, new Modifier<int>(1, ModifierOp.Add, (Fixed64)100));
            _set.AddModifier(HP, new Modifier<int>(2, ModifierOp.Add, (Fixed64)200));
            Assert.That(_set.GetFinalValue(HP), Is.EqualTo((Fixed64)1300));
        }

        [Test]
        public void GetAllAttributeTypes_ReturnsRegistered()
        {
            var types = _set.GetAllAttributeTypes();
            Assert.That(types, Does.Contain(HP));
            Assert.That(types, Does.Contain(ATK));
            Assert.That(types, Does.Contain(DEF));
        }
    }
}