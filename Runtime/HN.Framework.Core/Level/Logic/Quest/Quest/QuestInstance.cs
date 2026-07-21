#nullable enable

using MemoryPack;
using System.Collections.Generic;
using FixedMathSharp;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 运行时任务实例。支持 MemoryPack 序列化和 IReference 引用池复用。
    /// 参考 <see cref="HN.Framework.Core.Level.Logic.Inventory.ItemInstance"/> 的 IReference + MemoryPackable 模式。
    /// </summary>
    [MemoryPackable]
    public partial class QuestInstance : IReference
    {
        /// <summary>关联的任务定义 ID。</summary>
        [MemoryPackOrder(0)]
        public int QuestId { get; set; }

        /// <summary>当前状态。</summary>
        [MemoryPackOrder(1)]
        public QuestState State { get; set; }

        /// <summary>任务开始时的游戏内时间。</summary>
        [MemoryPackOrder(2)]
        public Fixed64 StartTime { get; set; }

        /// <summary>进度追踪（条件 Key → 当前值）。</summary>
        [MemoryPackOrder(3)]
        public Dictionary<string, int>? Progress { get; set; }

        /// <summary>重复完成次数（仅可重复任务使用）。</summary>
        [MemoryPackOrder(4)]
        public int RepeatCount { get; set; }

        /// <summary>剩余时间（秒）。null 表示无时间限制或已过期。</summary>
        [MemoryPackOrder(5)]
        public Fixed64? TimeRemaining { get; set; }

        /// <summary>
        /// 初始化任务实例。由工厂方法调用，设置初始状态为 <see cref="QuestState.Active"/>。
        /// StartTime 由 QuestSystem 在接受任务时设置。
        /// </summary>
        /// <param name="def">任务配置表定义。</param>
        internal void Initialize(QuestDef def)
        {
            QuestId = def.Id;
            State = QuestState.Active;
            StartTime = Fixed64.Zero;
            Progress = null;
            RepeatCount = 0;
            TimeRemaining = def.TimeLimit;
        }

        /// <summary>
        /// IReference 接口：重置实例，归还引用池前调用。
        /// </summary>
        public void Clear()
        {
            QuestId = 0;
            State = QuestState.Locked;
            StartTime = Fixed64.Zero;
            Progress = null;
            RepeatCount = 0;
            TimeRemaining = null;
        }
    }
}
