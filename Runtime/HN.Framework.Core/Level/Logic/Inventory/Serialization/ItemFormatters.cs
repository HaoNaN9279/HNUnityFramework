#nullable enable

using MemoryPack;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Inventory.Serialization
{
    /// <summary>
    /// Inventory 模块的 MemoryPack 格式化器注册入口。
    /// 调用 <see cref="RegisterAll"/> 注册所有格式化器以支持序列化。
    /// </summary>
    public static class ItemFormatters
    {
        private static bool _registered;

        /// <summary>
        /// 注册所有 Inventory 格式化器。幂等操作。
        /// 应在程序启动时（MemoryPack 初始化阶段）调用。
        /// </summary>
        public static void RegisterAll()
        {
            if (_registered) return;
            _registered = true;

            MemoryPackFormatterProvider.Register(new ItemInstanceFormatter());
        }
    }

    /// <summary>
    /// ItemInstance 格式化器（引用类型，序列化 8 个字段）。
    /// </summary>
    internal sealed class ItemInstanceFormatter : MemoryPackFormatter<ItemInstance>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ItemInstance? value)
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteObjectHeader(8);
            writer.WriteUnmanaged(value.ItemDefId);
            writer.WriteUnmanaged(value.StackCount);
            writer.WriteValue(value.DurabilityRemaining);
            writer.WriteUnmanaged(value.OwnerEntityId);
            writer.WriteUnmanaged(value.WorldEntityId);
            writer.WriteUnmanaged(value.SlotIndex);
            writer.WriteUnmanaged(value.AllowedSlots);
            writer.WriteValue(value.ExtraData);
        }

        public override void Deserialize(ref MemoryPackReader reader, ref ItemInstance? value)
        {
            if (!reader.TryReadObjectHeader(out byte count))
            {
                value = null;
                return;
            }

            int itemDefId = 0;
            int stackCount = 1;
            Fixed64 durability = Fixed64.Zero;
            uint ownerEntityId = 0;
            uint worldEntityId = 0;
            int slotIndex = -1;
            int allowedSlots = 0;
            Dictionary<string, Fixed64>? extraData = null;

            if (count >= 1) reader.ReadUnmanaged(out itemDefId);
            if (count >= 2) reader.ReadUnmanaged(out stackCount);
            if (count >= 3) durability = reader.ReadValue<Fixed64>();
            if (count >= 4) reader.ReadUnmanaged(out ownerEntityId);
            if (count >= 5) reader.ReadUnmanaged(out worldEntityId);
            if (count >= 6) reader.ReadUnmanaged(out slotIndex);
            if (count >= 7) reader.ReadUnmanaged(out allowedSlots);
            if (count >= 8) extraData = reader.ReadValue<Dictionary<string, Fixed64>?>();

            var instance = new ItemInstance();
            instance.Clear();

            instance.Initialize(itemDefId, stackCount);
            instance.DurabilityRemaining = durability;
            instance.OwnerEntityId = ownerEntityId;
            instance.WorldEntityId = worldEntityId;
            instance.SlotIndex = slotIndex;
            instance.AllowedSlots = allowedSlots;
            instance.ExtraData = extraData;

            value = instance;
        }
    }
}
