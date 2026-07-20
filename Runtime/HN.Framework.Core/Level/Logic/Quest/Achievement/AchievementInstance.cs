#nullable enable

using MemoryPack;
using FixedMathSharp;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 运行时成就实例。支持 MemoryPack 序列化和 IReference 引用池复用。
    /// </summary>
    [MemoryPackable]
    public partial class AchievementInstance : IReference
    {
        /// <summary>成就定义 ID。</summary>
        [MemoryPackOrder(0)]
        public int AchievementId { get; set; }

        /// <summary>当前状态。</summary>
        [MemoryPackOrder(1)]
        public AchievementState State { get; set; }

        /// <summary>完成进度（0~1）。</summary>
        [MemoryPackOrder(2)]
        public float ProgressPercent { get; set; }

        /// <summary>完成时间。</summary>
        [MemoryPackOrder(3)]
        public Fixed64? CompletedTime { get; set; }

        /// <summary>领取时间。</summary>
        [MemoryPackOrder(4)]
        public Fixed64? ClaimedTime { get; set; }

        /// <summary>
        /// 初始化成就实例。由工厂方法或反序列化调用。
        /// </summary>
        /// <param name="def">成就配置定义。</param>
        internal void Initialize(AchievementDef def)
        {
            AchievementId = def.Id;
            State = def.IsHidden ? AchievementState.Hidden : AchievementState.Revealed;
            ProgressPercent = 0f;
            CompletedTime = null;
            ClaimedTime = null;
        }

        /// <summary>
        /// IReference 接口：重置实例，归还引用池前调用。
        /// </summary>
        public void Clear()
        {
            AchievementId = 0;
            State = AchievementState.Hidden;
            ProgressPercent = 0f;
            CompletedTime = null;
            ClaimedTime = null;
        }
    }
}
