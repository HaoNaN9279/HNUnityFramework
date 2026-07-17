#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Inventory;

namespace HN.Framework.Core.Tests.Level.Logic.Inventory.ContainerTests
{
    [TestFixture]
    public class ContainerTests
    {
        private Container _container = null!;
        private const int TestCapacity = 5;

        [SetUp]
        public void SetUp()
        {
            _container = new Container(ContainerType.Backpack, TestCapacity, 0);
        }

        private ItemInstance MakeItem(int defId, int stack = 1)
        {
            var item = new ItemInstance();
            item.Initialize(defId, stack);
            return item;
        }

        [Test]
        public void Constructor_DefaultCapacity_IsCorrect()
        {
            var c = new Container(ContainerType.Backpack);
            Assert.That(c.Capacity, Is.EqualTo(20));
            Assert.That(c.Type, Is.EqualTo(ContainerType.Backpack));
        }

        [Test]
        public void TryAddItem_EmptySlot_AddsSuccessfully()
        {
            var item = MakeItem(1001);
            bool result = _container.TryAddItem(item);

            Assert.That(result, Is.True);
            Assert.That(_container.SlotCount, Is.EqualTo(1));
        }

        [Test]
        public void TryAddItem_FullContainer_ReturnsFalse()
        {
            for (int i = 0; i < TestCapacity; i++)
                _container.TryAddItem(MakeItem(1000 + i));

            var extra = MakeItem(9999);
            bool result = _container.TryAddItem(extra);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TryAddItem_SameItem_Stacks()
        {
            _container.TryAddItem(MakeItem(1001, 1));
            _container.TryAddItem(MakeItem(1001, 1));

            Assert.That(_container.SlotCount, Is.EqualTo(1));
            Assert.That(_container.GetSlot(0).StackCount, Is.EqualTo(2));
        }

        [Test]
        public void RemoveItem_ReducesStack()
        {
            _container.TryAddItem(MakeItem(1001, 5));
            bool result = _container.RemoveItem(0, 2);

            Assert.That(result, Is.True);
            Assert.That(_container.GetSlot(0).StackCount, Is.EqualTo(3));
        }

        [Test]
        public void RemoveItem_RemovesAll_ClearsSlot()
        {
            _container.TryAddItem(MakeItem(1001, 3));
            _container.RemoveItem(0, 3);

            Assert.That(_container.GetSlot(0).IsEmpty, Is.True);
        }

        [Test]
        public void RemoveItem_MoreThanAvailable_ReturnsFalse()
        {
            _container.TryAddItem(MakeItem(1001, 2));
            bool result = _container.RemoveItem(0, 5);

            Assert.That(result, Is.False);
            Assert.That(_container.GetSlot(0).StackCount, Is.EqualTo(2));
        }

        [Test]
        public void RemoveItemAt_RemovesCompletely()
        {
            _container.TryAddItem(MakeItem(1001, 3));
            bool result = _container.RemoveItemAt(0);

            Assert.That(result, Is.True);
            Assert.That(_container.GetSlot(0).IsEmpty, Is.True);
        }

        [Test]
        public void SwapSlots_ExchangesItems()
        {
            _container.TryAddItemAt(0, MakeItem(1001));
            _container.TryAddItemAt(1, MakeItem(1002));

            _container.SwapSlots(0, 1);

            Assert.That(_container.GetSlot(0).Item!.ItemDefId, Is.EqualTo(1002));
            Assert.That(_container.GetSlot(1).Item!.ItemDefId, Is.EqualTo(1001));
        }

        [Test]
        public void Clear_EmptiesAllSlots()
        {
            _container.TryAddItem(MakeItem(1001));
            _container.TryAddItem(MakeItem(1002));
            _container.Clear();

            Assert.That(_container.SlotCount, Is.EqualTo(0));
            for (int i = 0; i < TestCapacity; i++)
                Assert.That(_container.GetSlot(i).IsEmpty, Is.True);
        }

        [Test]
        public void GetItemCount_ReturnsTotal()
        {
            _container.TryAddItem(MakeItem(1001, 3));
            _container.TryAddItem(MakeItem(1001, 2));

            Assert.That(_container.GetItemCount(1001), Is.EqualTo(5));
        }

        [Test]
        public void HasItem_EnoughQuantity_ReturnsTrue()
        {
            _container.TryAddItem(MakeItem(1001, 5));
            Assert.That(_container.HasItem(1001, 3), Is.True);
        }

        [Test]
        public void HasItem_NotEnough_ReturnsFalse()
        {
            _container.TryAddItem(MakeItem(1001, 2));
            Assert.That(_container.HasItem(1001, 5), Is.False);
        }

        [Test]
        public void ItemAddedEvent_IsFired()
        {
            bool fired = false;
            _container.OnItemAdded += (_) => fired = true;

            _container.TryAddItem(MakeItem(1001));

            Assert.That(fired, Is.True);
        }

        [Test]
        public void ItemRemovedEvent_IsFired()
        {
            _container.TryAddItem(MakeItem(1001));
            bool fired = false;
            _container.OnItemRemoved += (_) => fired = true;

            _container.RemoveItemAt(0);

            Assert.That(fired, Is.True);
        }

        [Test]
        public void NullItem_Add_ReturnsFalse()
        {
            Assert.That(_container.TryAddItem(null!), Is.False);
        }
    }
}
