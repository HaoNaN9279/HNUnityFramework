#nullable enable

using MemoryPack;
using NUnit.Framework;
using FixedMathSharp;
using HN.Framework.Core.Level.Logic.Inventory;
using HN.Framework.Core.Level.Logic.Inventory.Serialization;

namespace HN.Framework.Core.Tests.Level.Logic.Inventory.Serialization
{
    [TestFixture]
    public class ItemFormattersTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // 确保格式化器已注册
            ItemFormatters.RegisterAll();
        }

        private static byte[] Serialize(ItemInstance item)
        {
            return MemoryPackSerializer.Serialize(item);
        }

        private static ItemInstance Deserialize(byte[] data)
        {
            return MemoryPackSerializer.Deserialize<ItemInstance>(data)!;
        }

        [Test]
        public void SerializeRoundtrip_PreservesBasicFields()
        {
            var item = new ItemInstance();
            item.Initialize(1001, 5);

            byte[] data = Serialize(item);
            var deserialized = Deserialize(data);

            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized.ItemDefId, Is.EqualTo(1001));
            Assert.That(deserialized.StackCount, Is.EqualTo(5));
        }

        [Test]
        public void SerializeRoundtrip_PreservesOwnerAndWorld()
        {
            var item = new ItemInstance();
            item.Initialize(1002);
            item.OwnerEntityId = 42;
            item.WorldEntityId = 99;

            byte[] data = Serialize(item);
            var deserialized = Deserialize(data);

            Assert.That(deserialized.OwnerEntityId, Is.EqualTo(42));
            Assert.That(deserialized.WorldEntityId, Is.EqualTo(99));
        }

        [Test]
        public void SerializeRoundtrip_PreservesDurability()
        {
            var item = new ItemInstance();
            item.Initialize(1003);
            item.DurabilityRemaining = (Fixed64)50.5m;

            byte[] data = Serialize(item);
            var deserialized = Deserialize(data);

            Assert.That(deserialized.DurabilityRemaining, Is.EqualTo((Fixed64)50.5m));
        }

        [Test]
        public void SerializeRoundtrip_PreservesSlotIndex()
        {
            var item = new ItemInstance();
            item.Initialize(1004);
            item.SlotIndex = 3;

            byte[] data = Serialize(item);
            var deserialized = Deserialize(data);

            Assert.That(deserialized.SlotIndex, Is.EqualTo(3));
        }

        [Test]
        public void SerializeRoundtrip_DefaultSlotIndex_IsNegativeOne()
        {
            var item = new ItemInstance();
            item.Initialize(1005);

            byte[] data = Serialize(item);
            var deserialized = Deserialize(data);

            Assert.That(deserialized.SlotIndex, Is.EqualTo(-1));
        }

        [Test]
        public void SerializeRoundtrip_PreservesExtraData()
        {
            var item = new ItemInstance();
            item.Initialize(1006);
            item.ExtraData = new()
            {
                { "atk", (Fixed64)10m },
                { "def", (Fixed64)5m }
            };

            byte[] data = Serialize(item);
            var deserialized = Deserialize(data);

            Assert.That(deserialized.ExtraData, Is.Not.Null);
            Assert.That(deserialized.ExtraData!["atk"], Is.EqualTo((Fixed64)10m));
            Assert.That(deserialized.ExtraData!["def"], Is.EqualTo((Fixed64)5m));
        }

        [Test]
        public void SerializeRoundtrip_NullExtraData_Roundtrips()
        {
            var item = new ItemInstance();
            item.Initialize(1007);
            item.ExtraData = null;

            byte[] data = Serialize(item);
            var deserialized = Deserialize(data);

            Assert.That(deserialized.ExtraData, Is.Null);
        }

        [Test]
        public void SerializeRoundtrip_BoundaryValues()
        {
            var item = new ItemInstance();
            item.Initialize(int.MaxValue, int.MaxValue);
            item.OwnerEntityId = uint.MaxValue;
            item.WorldEntityId = uint.MaxValue;

            byte[] data = Serialize(item);
            var deserialized = Deserialize(data);

            Assert.That(deserialized.ItemDefId, Is.EqualTo(int.MaxValue));
            Assert.That(deserialized.StackCount, Is.EqualTo(int.MaxValue));
            Assert.That(deserialized.OwnerEntityId, Is.EqualTo(uint.MaxValue));
            Assert.That(deserialized.WorldEntityId, Is.EqualTo(uint.MaxValue));
        }

        [Test]
        public void SerializeRoundtrip_MultipleItems_AreIndependent()
        {
            var item1 = new ItemInstance();
            item1.Initialize(1, 10);
            item1.OwnerEntityId = 100;

            var item2 = new ItemInstance();
            item2.Initialize(2, 20);
            item2.OwnerEntityId = 200;

            byte[] data1 = Serialize(item1);
            byte[] data2 = Serialize(item2);
            var deserialized1 = Deserialize(data1);
            var deserialized2 = Deserialize(data2);

            Assert.That(deserialized1.ItemDefId, Is.EqualTo(1));
            Assert.That(deserialized1.StackCount, Is.EqualTo(10));
            Assert.That(deserialized2.ItemDefId, Is.EqualTo(2));
            Assert.That(deserialized2.StackCount, Is.EqualTo(20));
        }
    }
}
