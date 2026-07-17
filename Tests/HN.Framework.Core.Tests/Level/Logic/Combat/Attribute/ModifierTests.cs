#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Combat;
using FixedMathSharp;

namespace HN.Framework.Core.Tests.Level.Logic.Combat
{
    [TestFixture]
    public class ModifierTests
    {
        [Test]
        public void Constructor_SetsAllProperties()
        {
            var mod = new Modifier<int>(42, ModifierOp.Add, (Fixed64)50, 10);
            Assert.That(mod.Source, Is.EqualTo(42));
            Assert.That(mod.Op, Is.EqualTo(ModifierOp.Add));
            Assert.That(mod.Value, Is.EqualTo((Fixed64)50));
            Assert.That(mod.Priority, Is.EqualTo(10));
        }

        [Test]
        public void Constructor_DefaultPriority_IsZero()
        {
            var mod = new Modifier<int>(1, ModifierOp.Multiply, (Fixed64)2);
            Assert.That(mod.Priority, Is.EqualTo(0));
        }

        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new Modifier<int>(1, ModifierOp.Add, (Fixed64)10, 0);
            var b = new Modifier<int>(1, ModifierOp.Add, (Fixed64)10, 0);
            Assert.That(a.Equals(b), Is.True);
            Assert.That(a == b, Is.True);
        }

        [Test]
        public void Equals_DifferentSource_ReturnsFalse()
        {
            var a = new Modifier<int>(1, ModifierOp.Add, (Fixed64)10);
            var b = new Modifier<int>(2, ModifierOp.Add, (Fixed64)10);
            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void Equals_DifferentOp_ReturnsFalse()
        {
            var a = new Modifier<int>(1, ModifierOp.Add, (Fixed64)10);
            var b = new Modifier<int>(1, ModifierOp.Multiply, (Fixed64)10);
            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void ModifierHandle_Default_IsInvalid()
        {
            var handle = ModifierHandle.Invalid;
            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void ModifierHandle_Constructed_IsValid()
        {
            var handle = new ModifierHandle(1);
            Assert.That(handle.IsValid, Is.True);
        }

        [Test]
        public void ModifierHandle_Equality()
        {
            var a = new ModifierHandle(5);
            var b = new ModifierHandle(5);
            var c = new ModifierHandle(3);
            Assert.That(a == b, Is.True);
            Assert.That(a != c, Is.True);
        }
    }
}