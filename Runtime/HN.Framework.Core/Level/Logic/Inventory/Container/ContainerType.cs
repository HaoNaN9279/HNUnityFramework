#nullable enable

namespace HN.Framework.Core.Level.Logic.Inventory
{
    /// <summary>
    /// 容器类型枚举。
    /// </summary>
    public enum ContainerType : byte
    {
        None = 0,
        Backpack = 1,
        Warehouse = 2,
        Equipment = 3,
        Hotbar = 4,
        Shop = 5,
        Material = 6,
    }
}
