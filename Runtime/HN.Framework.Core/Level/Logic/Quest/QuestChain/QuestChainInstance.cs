#nullable enable

using HN.Framework.Core.Driver.Common;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 运行时任务链实例。管理任务链的推进状态，支持 <see cref="IReference"/> 引用池复用。
    /// </summary>
    public sealed class QuestChainInstance : IReference
    {
        /// <summary>关联的 QuestChainDef.Id。</summary>
        public int ChainId { get; private set; }

        /// <summary>当前运行状态。</summary>
        public QuestChainState State { get; internal set; }

        /// <summary>当前所在任务在 QuestIds 列表中的索引。</summary>
        public int CurrentQuestIndex { get; private set; }

        /// <summary>已完成的任务 ID 列表。为 <c>null</c> 表示尚无已完成任务。</summary>
        public List<int>? CompletedQuestIds { get; private set; }

        /// <summary>任务链中的全部任务 ID 列表（初始化时设置）。</summary>
        private List<int>? _questIds;

        /// <summary>
        /// 是否存在下一个任务。
        /// </summary>
        public bool HasNextQuest
        {
            get
            {
                if (_questIds == null)
                {
                    return false;
                }

                return CurrentQuestIndex + 1 < _questIds.Count;
            }
        }

        /// <summary>
        /// 已完成任务占比，范围 [0, 1]。
        /// </summary>
        public float ProgressPercent
        {
            get
            {
                if (_questIds == null || _questIds.Count == 0)
                {
                    return 0f;
                }

                int completedCount = CompletedQuestIds?.Count ?? 0;
                return (float)completedCount / _questIds.Count;
            }
        }

        /// <summary>
        /// 内部初始化方法。由 QuestManager 等管理类调用，不对外暴露。
        /// </summary>
        /// <param name="chainId">关联的 QuestChainDef.Id。</param>
        /// <param name="questIds">有序任务 ID 列表。</param>
        internal void Initialize(int chainId, List<int> questIds)
        {
            ChainId = chainId;
            _questIds = questIds;
            State = QuestChainState.Active;
            CurrentQuestIndex = 0;
            CompletedQuestIds = null;
        }

        /// <summary>
        /// 获取当前任务 ID。
        /// </summary>
        /// <returns>当前任务 ID；如果任务列表为空或未初始化则返回 0。</returns>
        public int GetCurrentQuestId()
        {
            if (_questIds == null || _questIds.Count == 0)
            {
                return 0;
            }

            return _questIds[CurrentQuestIndex];
        }

        /// <summary>
        /// 推进到下一个任务。调用前应通过 <see cref="HasNextQuest"/> 确认存在下一个任务。
        /// </summary>
        public void AdvanceToNext()
        {
            if (!HasNextQuest)
            {
                return;
            }

            int currentQuestId = GetCurrentQuestId();

            // 记录当前任务为已完成
            CompletedQuestIds ??= new List<int>();
            CompletedQuestIds.Add(currentQuestId);

            CurrentQuestIndex++;

            // 推进后检查是否已完成全部任务
            if (CurrentQuestIndex >= (_questIds?.Count ?? 0))
            {
                State = QuestChainState.Completed;
            }
        }

        /// <summary>
        /// 重置实例，归还引用池前调用。
        /// </summary>
        public void Clear()
        {
            ChainId = 0;
            State = QuestChainState.Locked;
            CurrentQuestIndex = 0;
            CompletedQuestIds = null;
            _questIds = null;
        }
    }
}
