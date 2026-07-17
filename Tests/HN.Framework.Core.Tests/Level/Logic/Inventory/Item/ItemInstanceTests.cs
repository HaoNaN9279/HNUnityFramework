#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Inventory;

namespace HN.Framework.Core.Tests.Level.Logic.Inventory.Item
{
    [TestFixture]
    public class ItemInstanceTests
    {
        [Test]
        public void Create_DefaultValues_AreCorrect()
        {
            var item = new ItemInstance();
            Assert.That(item.ItemDefId, Is.EqualTo(0));
            Assert.That(item.StackCount, Is.EqualTo(0));
            Assert.That(item.SlotIndex, Is.EqualTo(-1));
            Assert.That(item.OwnerEntityId, Is.EqualTo(0));
            Assert.That(item.WorldEntityId, Is.EqualTo(0));
            Assert.That(item.AllowedSlots, Is.EqualTo(0));
        }

        [Test]
        public void Initialize_SetsFieldsCorrectly()
        {
            var item = new ItemInstance();
            item.Initialize(1001, 5);
            Assert.That(item.ItemDefId, Is.EqualTo(1001));
            Assert.That(item.StackCount, Is.EqualTo(5));
        }

        [Test]
        public void Clear_ResetsAllFields()
        {
            var item = new ItemInstance();
            item.Initialize(1001, 3);
            item.OwnerEntityId = 42;
            item.WorldEntityId = 99;
            item.SlotIndex = 2;
            item.Clear();

            Assert.That(item.ItemDefId, Is.EqualTo(0));
            Assert.That(item.StackCount, Is.EqualTo(1));
            Assert.That(item.SlotIndex, Is.EqualTo(-1));
            Assert.That(item.OwnerEntityId, Is.EqualTo(0));
            Assert.That(item.WorldEntityId, Is.EqualTo(0));
        }

        [Test]
        public void CanStackWith_SameItemDefId_ReturnsTrue()
        {
            var a = new ItemInstance();
            a.Initialize(1001, 1);
            var b = new ItemInstance();
            b.Initialize(1001, 1);

            Assert.That(a.CanStackWith(b), Is.True);
        }

        [Test]
        public void CanStackWith_DifferentItemDefId_ReturnsFalse()
        {
            var a = new ItemInstance();
            a.Initialize(1001, 1);
            var b = new ItemInstance();
            b.Initialize(1002, 1);

            Assert.That(a.CanStackWith(b), Is.False);
        }

        [Test]
        public void CanStackWith_Null_ReturnsFalse()
        {
            var item = new ItemInstance();
            item.Initialize(1001, 1);
            Assert.That(item.CanStackWith(null!), Is.False);
        }

        [Test]
        public void CanStackWith_ZeroStack_ReturnsFalse()
        {
            var a = new ItemInstance();
            a.Initialize(1001, 0);
            var b = new ItemInstance();
            b.Initialize(1001, 1);

            Assert.That(a.CanStackWith(b), Is.False);
        }

        [Test]
        public void Initialize_DefaultStackCount_IsOne()
        {
            var item = new ItemInstance();
            item.Initialize(1001);
            Assert.That(item.StackCount, Is.EqualTo(1));
        }
    }
}
