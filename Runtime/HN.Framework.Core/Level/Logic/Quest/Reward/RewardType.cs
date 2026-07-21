#nullable enable

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 奖励类型枚举。
    /// 用于标识 <see cref="RewardDef"/> 对应的奖励发放类别。
    /// </summary>
    public enum RewardType : byte
    {
        /// <summary>物品奖励。</summary>
        Item = 0,

        /// <summary>货币奖励。</summary>
        Currency = 1,

        /// <summary>经验奖励。</summary>
        Experience = 2,

        /// <summary>属性奖励（如升级、属性点）。</summary>
        Attribute = 3,

        /// <summary>解锁内容/功能。</summary>
        Unlock = 4,

        /// <summary>自定义类型，由项目扩展。</summary>
        Custom = 5
    }
}
