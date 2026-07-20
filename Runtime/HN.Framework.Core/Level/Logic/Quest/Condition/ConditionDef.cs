#nullable enable

using System.Collections.Generic;
using MemoryPack;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 条件配置表行定义。
    /// 由 L4 Sheet + Luban 生成，MemoryPack 可序列化。
    /// Parameters 用于存储条件参数键值对，如 {"EventTypeName":"MonsterKilled", "TargetCount":"10"}。
    /// </summary>
    [MemoryPackable]
    public partial struct ConditionDef
    {
        /// <summary>条件唯一标识。</summary>
        [MemoryPackOrder(0)]
        public int Id;

        /// <summary>条件类型。</summary>
        [MemoryPackOrder(1)]
        public ConditionType Type;

        /// <summary>
        /// 参数键值对。
        /// 键名和值含义由具体条件类型定义。
        /// 为 null 或空表示无条件参数。
        /// </summary>
        [MemoryPackOrder(2)]
        public Dictionary<string, string>? Parameters;

        /// <summary>
        /// 获取指定 key 的参数值。
        /// </summary>
        /// <param name="key">参数键名。</param>
        /// <param name="defaultValue">未找到时返回的默认值。</param>
        /// <returns>参数值，或未找到时返回 <paramref name="defaultValue"/>。</returns>
        public readonly string GetParameter(string key, string defaultValue = "")
        {
            if (Parameters != null && Parameters.TryGetValue(key, out string? value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}
