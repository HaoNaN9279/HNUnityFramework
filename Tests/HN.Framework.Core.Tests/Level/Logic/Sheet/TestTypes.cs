#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.Sheet
{
    /// <summary>
    /// 使用整型主键的测试配置行。
    /// </summary>
    [global::MemoryPack.MemoryPackable]
    public partial class TestConfigRow
    {
        /// <summary>
        /// 主键 ID。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 名称。
        /// </summary>
        public string? Name { get; set; }
    }

    /// <summary>
    /// 使用字符串主键的测试配置行。
    /// </summary>
    [global::MemoryPack.MemoryPackable]
    public partial class TestConfigRowWithStringKey
    {
        /// <summary>
        /// 字符串主键。
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// 值。
        /// </summary>
        public int Value { get; set; }
    }
}
