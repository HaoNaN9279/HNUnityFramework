#nullable enable

using HN.Framework.Core.Driver.Common;
using MemoryPack;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// Counter 条件的运行时计数状态。
    /// 用于存储单个计数条件的当前进度，支持 MemoryPack 序列化存档。
    /// 实现 <see cref="IReference"/> 接口，支持通过 ReferencePool 进行对象池复用。
    /// </summary>
    [MemoryPackable]
    public partial class CounterState : IReference
    {
        /// <summary>关联的条件 ID。</summary>
        [MemoryPackOrder(0)]
        public int ConditionId;

        /// <summary>当前计数值。</summary>
        [MemoryPackOrder(1)]
        public int CurrentCount;

        /// <summary>目标计数值。</summary>
        [MemoryPackOrder(2)]
        public int TargetCount;

        /// <summary>是否已完成（CurrentCount >= TargetCount）。</summary>
        [MemoryPackOrder(3)]
        public bool IsCompleted;

        /// <summary>是否已失败（如超时导致）。</summary>
        [MemoryPackOrder(4)]
        public bool IsFailed;

        /// <summary>
        /// 重置计数状态，归还引用池前调用。
        /// </summary>
        public void Clear()
        {
            ConditionId = 0;
            CurrentCount = 0;
            TargetCount = 0;
            IsCompleted = false;
            IsFailed = false;
        }
    }
}
