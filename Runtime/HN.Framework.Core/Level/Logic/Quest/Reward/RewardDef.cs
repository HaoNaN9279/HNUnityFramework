#nullable enable

using MemoryPack;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 奖励配置表行定义。
    /// 由 L4 Sheet + Luban 生成，MemoryPack 可序列化。
    /// </summary>
    [MemoryPackable]
    public partial struct RewardDef
    {
        /// <summary>奖励唯一标识。</summary>
        [MemoryPackOrder(0)]
        public int Id;

        /// <summary>奖励类型。</summary>
        [MemoryPackOrder(1)]
        public RewardType Type;

        /// <summary>
        /// 目标 ID。根据 <see cref="Type"/> 指向不同含义：
        /// <list type="bullet">
        ///   <item><see cref="RewardType.Item"/> — 物品 ID。</item>
        ///   <item><see cref="RewardType.Currency"/> — 货币 ID。</item>
        ///   <item><see cref="RewardType.Experience"/> — 经验类别 ID。</item>
        ///   <item><see cref="RewardType.Attribute"/> — 属性 ID。</item>
        ///   <item><see cref="RewardType.Unlock"/> — 解锁内容 ID。</item>
        ///   <item><see cref="RewardType.Custom"/> — 自定义标识，由项目定义。</item>
        /// </list>
        /// </summary>
        [MemoryPackOrder(2)]
        public int TargetId;

        /// <summary>奖励数量。</summary>
        [MemoryPackOrder(3)]
        public int Amount;

        /// <summary>发放概率，取值范围 [0, 1]，默认 1.0 表示必定发放。</summary>
        [MemoryPackOrder(4)]
        public float Probability;
    }
}
