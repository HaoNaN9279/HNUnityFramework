#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;
using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Inventory;
using HN.Framework.Core.Level.Logic.Combat;

namespace HN.Framework.Core.Tests.Level.Logic.Inventory.EquipmentTests
{
    // 项目自定义槽位 ID 示例（模拟 Luban 配置表定义的槽位）
    // 每个槽位占用 int 位掩码中的一位
    internal static class TestSlots
    {
        public const int None = 0;
        public const int Weapon = 1 << 0;
        public const int Head = 1 << 1;
        public const int Chest = 1 << 2;
        public const int Legs = 1 << 3;
        public const int Feet = 1 << 4;
        public const int Accessory1 = 1 << 5;
        public const int Accessory2 = 1 << 6;
    }

    // 简单的 IBuffSystem stub，记录 ApplyBuff/RemoveBuff 调用
    public sealed class BuffSystemStub : IBuffSystem<uint>
    {
        public int ApplyBuffCallCount;
        public int RemoveBuffCallCount;
        public int ActiveCount => 0;
        public event Action<BuffSpec<uint>, uint, uint, int>? OnBuffApplied;
        public event Action<BuffSpec<uint>, uint, uint, int, bool>? OnBuffRemoved;
        public event Action<BuffSpec<uint>, uint, uint, int, int>? OnBuffStackChanged;

        public BuffHandle ApplyBuff(BuffSpec<uint> spec, uint source, uint target,
            IAttributeSet<uint> targetAttributes, IEffectPipeline<uint> effectPipeline)
        {
            ApplyBuffCallCount++;
            return new BuffHandle(ApplyBuffCallCount);
        }

        public bool RemoveBuff(BuffHandle handle, IAttributeSet<uint> targetAttributes,
            IEffectPipeline<uint> effectPipeline)
        {
            RemoveBuffCallCount++;
            return true;
        }

        public void Tick(Fixed64 deltaTime, IAttributeSet<uint> targetAttributes,
            IEffectPipeline<uint> effectPipeline) { }
    }

    [TestFixture]
    public class EquipmentTests
    {
        // 标准 7 槽位 RPG 配置
        private static readonly int[] StandardSlots = new[]
        {
            TestSlots.Weapon,
            TestSlots.Head,
            TestSlots.Chest,
            TestSlots.Legs,
            TestSlots.Feet,
            TestSlots.Accessory1,
            TestSlots.Accessory2,
        };

        private Equipment _equipment = null!;
        private Container _container = null!;
        private BuffSystemStub _buffStub = null!;

        [SetUp]
        public void SetUp()
        {
            _container = new Container(ContainerType.Equipment, StandardSlots.Length, 0);
            _buffStub = new BuffSystemStub();
            _equipment = new Equipment(_container, StandardSlots, _buffStub, null);
        }

        private ItemInstance MakeItem(int defId, int allowedSlots)
        {
            var item = new ItemInstance();
            item.Initialize(defId, 1);
            item.AllowedSlots = allowedSlots;
            return item;
        }

        [Test]
        public void Equip_ValidSlot_Succeeds()
        {
            var item = MakeItem(1001, TestSlots.Weapon);
            bool result = _equipment.Equip(TestSlots.Weapon, item);

            Assert.That(result, Is.True);
            Assert.That(_equipment.GetEquippedItem(TestSlots.Weapon), Is.Not.Null);
        }

        [Test]
        public void Equip_InvalidSlot_ReturnsFalse()
        {
            var item = MakeItem(1001, TestSlots.None);
            bool result = _equipment.Equip(TestSlots.Weapon, item);

            Assert.That(result, Is.False);
        }

        [Test]
        public void Unequip_RemovesItemAndClearsSlot()
        {
            var item = MakeItem(1001, TestSlots.Weapon);
            _equipment.Equip(TestSlots.Weapon, item);

            var unequipped = _equipment.Unequip(TestSlots.Weapon);

            Assert.That(unequipped, Is.Not.Null);
            Assert.That(_equipment.GetEquippedItem(TestSlots.Weapon), Is.Null);
        }

        [Test]
        public void Unequip_EmptySlot_ReturnsNull()
        {
            var result = _equipment.Unequip(TestSlots.Weapon);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SwapSlots_ExchangesEquipment()
        {
            var sword = MakeItem(1001, TestSlots.Weapon);
            var helmet = MakeItem(1002, TestSlots.Head);
            _equipment.Equip(TestSlots.Weapon, sword);
            _equipment.Equip(TestSlots.Head, helmet);

            _equipment.SwapSlots(TestSlots.Weapon, TestSlots.Head);

            Assert.That(_equipment.GetEquippedItem(TestSlots.Weapon)?.ItemDefId, Is.EqualTo(1002));
            Assert.That(_equipment.GetEquippedItem(TestSlots.Head)?.ItemDefId, Is.EqualTo(1001));
        }

        [Test]
        public void CanEquip_ValidSlot_ReturnsTrue()
        {
            var item = MakeItem(1001, TestSlots.Weapon);
            Assert.That(_equipment.CanEquip(item, TestSlots.Weapon), Is.True);
        }

        [Test]
        public void CanEquip_InvalidSlot_ReturnsFalse()
        {
            var item = MakeItem(1001, TestSlots.None);
            Assert.That(_equipment.CanEquip(item, TestSlots.Weapon), Is.False);
        }

        [Test]
        public void Equip_ReplacesExisting_UnequipsFirst()
        {
            var oldItem = MakeItem(1001, TestSlots.Weapon);
            var newItem = MakeItem(1002, TestSlots.Weapon);
            _equipment.Equip(TestSlots.Weapon, oldItem);

            _equipment.Equip(TestSlots.Weapon, newItem);

            Assert.That(_equipment.GetEquippedItem(TestSlots.Weapon)?.ItemDefId, Is.EqualTo(1002));
        }

        [Test]
        public void GetAllEquipped_ReturnsOnlyOccupiedSlots()
        {
            _equipment.Equip(TestSlots.Weapon, MakeItem(1001, TestSlots.Weapon));
            _equipment.Equip(TestSlots.Head, MakeItem(1002, TestSlots.Head));

            var all = _equipment.GetAllEquipped();

            Assert.That(all.Count, Is.EqualTo(2));
            Assert.That(all.ContainsKey(TestSlots.Weapon), Is.True);
            Assert.That(all.ContainsKey(TestSlots.Head), Is.True);
        }

        [Test]
        public void UnequipAll_ClearsAllSlots()
        {
            _equipment.Equip(TestSlots.Weapon, MakeItem(1001, TestSlots.Weapon));
            _equipment.Equip(TestSlots.Head, MakeItem(1002, TestSlots.Head));

            _equipment.UnequipAll();

            var all = _equipment.GetAllEquipped();
            Assert.That(all.Count, Is.EqualTo(0));
        }

        [Test]
        public void EquippedEvent_IsFired()
        {
            bool fired = false;
            _equipment.OnEquipped += (_) => fired = true;

            _equipment.Equip(TestSlots.Weapon, MakeItem(1001, TestSlots.Weapon));

            Assert.That(fired, Is.True);
        }

        [Test]
        public void UnequippedEvent_IsFired()
        {
            _equipment.Equip(TestSlots.Weapon, MakeItem(1001, TestSlots.Weapon));
            bool fired = false;
            _equipment.OnUnequipped += (_) => fired = true;

            _equipment.Unequip(TestSlots.Weapon);

            Assert.That(fired, Is.True);
        }

        [Test]
        public void CustomSlotConfiguration_WorksWithExtendedSlots()
        {
            // 测试项目自定义 3 槽位配置（简化游戏）
            const int CustomWeapon = 1 << 9;
            const int CustomArmor = 1 << 10;
            const int CustomRing = 1 << 11;

            var customSlots = new[] { CustomWeapon, CustomArmor, CustomRing };
            var customContainer = new Container(ContainerType.Equipment, 3, 0);
            var customEquipment = new Equipment(customContainer, customSlots);

            var sword = MakeItem(2001, CustomWeapon);
            var ring = MakeItem(2002, CustomRing);

            Assert.That(customEquipment.Equip(CustomWeapon, sword), Is.True);
            Assert.That(customEquipment.Equip(CustomRing, ring), Is.True);
            Assert.That(customEquipment.GetAllEquipped().Count, Is.EqualTo(2));
        }

        [Test]
        public void SlotNotInConfig_ReturnsFalse()
        {
            var item = MakeItem(1001, TestSlots.Weapon);
            // 尝试装备到一个未在配置中注册的槽位
            const int UnregisteredSlot = 1 << 20;

            Assert.That(_equipment.CanEquip(item, UnregisteredSlot), Is.False);
            Assert.That(_equipment.Equip(UnregisteredSlot, item), Is.False);
        }

        [Test]
        public void GetEquippableSlots_ReturnsIntersectionWithConfig()
        {
            // 物品允许 Weapon + Head，装备系统只包含 StandardSlots 中注册的槽位
            var item = MakeItem(3001, TestSlots.Weapon | TestSlots.Head);

            int equippable = _equipment.GetEquippableSlots(item);

            Assert.That((equippable & TestSlots.Weapon) != 0, Is.True);
            Assert.That((equippable & TestSlots.Head) != 0, Is.True);
            Assert.That((equippable & TestSlots.Chest) != 0, Is.False);
        }
    }
}
