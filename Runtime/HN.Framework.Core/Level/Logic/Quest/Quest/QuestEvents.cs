#nullable enable

using System;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 任务句柄。用于追踪运行中的任务实例。
    /// 与 <see cref="Combat.BuffHandle"/> 相同的 int-based struct 模式。
    /// </summary>
    public readonly struct QuestHandle : IEquatable<QuestHandle>
    {
        internal readonly int Id;

        /// <summary>句柄是否有效（Id > 0）。</summary>
        public bool IsValid => Id > 0;

        /// <summary>初始化任务句柄（仅 QuestSystem 内部使用）。</summary>
        internal QuestHandle(int id) { Id = id; }

        /// <summary>无效句柄（默认值）。</summary>
        public static readonly QuestHandle Invalid = default;

        /// <inheritdoc />
        public bool Equals(QuestHandle other) => Id == other.Id;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is QuestHandle other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Id;

        /// <summary>相等运算符。</summary>
        public static bool operator ==(QuestHandle left, QuestHandle right) => left.Equals(right);

        /// <summary>不等运算符。</summary>
        public static bool operator !=(QuestHandle left, QuestHandle right) => !left.Equals(right);
    }

    /// <summary>
    /// 任务完成类型。
    /// </summary>
    public enum CompletionType : byte
    {
        /// <summary>正常完成（条件全部满足）。</summary>
        Normal = 0,

        /// <summary>自动失败（如条件变更导致无法完成）。</summary>
        AutoFail = 1,

        /// <summary>最大值标记，用于遍历边界。</summary>
        Max = 2,
    }

    /// <summary>
    /// 任务失败原因。
    /// </summary>
    public enum FailReason : byte
    {
        /// <summary>超时。</summary>
        Timeout = 0,

        /// <summary>外部触发（如脚本强制失败）。</summary>
        External = 1,

        /// <summary>前置任务失败导致连锁失败。</summary>
        Prerequisite = 2,
    }

    /// <summary>任务状态变更事件。</summary>
    public readonly struct QuestStateChangedEvent<TId> where TId : IEquatable<TId>
    {
        /// <summary>关联的任务句柄。</summary>
        public readonly QuestHandle Handle;
        /// <summary>拥有者 ID。</summary>
        public readonly TId OwnerId;
        /// <summary>变更前状态。</summary>
        public readonly QuestState OldState;
        /// <summary>变更后状态。</summary>
        public readonly QuestState NewState;
        /// <summary>任务定义 ID。</summary>
        public readonly int QuestId;

        /// <summary>
        /// 初始化 QuestStateChangedEvent 实例。
        /// </summary>
        public QuestStateChangedEvent(QuestHandle handle, TId ownerId, QuestState oldState, QuestState newState, int questId)
        {
            Handle = handle;
            OwnerId = ownerId;
            OldState = oldState;
            NewState = newState;
            QuestId = questId;
        }
    }

    /// <summary>任务进度更新事件。</summary>
    public readonly struct QuestProgressUpdatedEvent
    {
        /// <summary>关联的任务句柄。</summary>
        public readonly QuestHandle Handle;
        /// <summary>进度 Key。</summary>
        public readonly string ProgressKey;
        /// <summary>当前值。</summary>
        public readonly int CurrentValue;
        /// <summary>目标值。</summary>
        public readonly int TargetValue;

        /// <summary>
        /// 初始化 QuestProgressUpdatedEvent 实例。
        /// </summary>
        public QuestProgressUpdatedEvent(QuestHandle handle, string progressKey, int currentValue, int targetValue)
        {
            Handle = handle;
            ProgressKey = progressKey;
            CurrentValue = currentValue;
            TargetValue = targetValue;
        }
    }

    /// <summary>接受任务事件。</summary>
    public readonly struct QuestAcceptedEvent<TId> where TId : IEquatable<TId>
    {
        /// <summary>拥有者 ID。</summary>
        public readonly TId OwnerId;
        /// <summary>任务定义 ID。</summary>
        public readonly int QuestId;
        /// <summary>分配的任务句柄。</summary>
        public readonly QuestHandle Handle;

        /// <summary>
        /// 初始化 QuestAcceptedEvent 实例。
        /// </summary>
        public QuestAcceptedEvent(TId ownerId, int questId, QuestHandle handle)
        {
            OwnerId = ownerId;
            QuestId = questId;
            Handle = handle;
        }
    }

    /// <summary>任务完成事件。</summary>
    public readonly struct QuestCompletedEvent<TId> where TId : IEquatable<TId>
    {
        /// <summary>拥有者 ID。</summary>
        public readonly TId OwnerId;
        /// <summary>任务定义 ID。</summary>
        public readonly int QuestId;
        /// <summary>关联的任务句柄。</summary>
        public readonly QuestHandle Handle;
        /// <summary>完成类型。</summary>
        public readonly CompletionType CompletionType;

        /// <summary>
        /// 初始化 QuestCompletedEvent 实例。
        /// </summary>
        public QuestCompletedEvent(TId ownerId, int questId, QuestHandle handle, CompletionType completionType)
        {
            OwnerId = ownerId;
            QuestId = questId;
            Handle = handle;
            CompletionType = completionType;
        }
    }

    /// <summary>任务奖励领取事件。</summary>
    public readonly struct QuestClaimedEvent<TId> where TId : IEquatable<TId>
    {
        /// <summary>拥有者 ID。</summary>
        public readonly TId OwnerId;
        /// <summary>任务定义 ID。</summary>
        public readonly int QuestId;
        /// <summary>关联的任务句柄。</summary>
        public readonly QuestHandle Handle;

        /// <summary>
        /// 初始化 QuestClaimedEvent 实例。
        /// </summary>
        public QuestClaimedEvent(TId ownerId, int questId, QuestHandle handle)
        {
            OwnerId = ownerId;
            QuestId = questId;
            Handle = handle;
        }
    }

    /// <summary>任务失败事件。</summary>
    public readonly struct QuestFailedEvent<TId> where TId : IEquatable<TId>
    {
        /// <summary>拥有者 ID。</summary>
        public readonly TId OwnerId;
        /// <summary>任务定义 ID。</summary>
        public readonly int QuestId;
        /// <summary>关联的任务句柄。</summary>
        public readonly QuestHandle Handle;
        /// <summary>失败原因。</summary>
        public readonly FailReason FailReason;

        /// <summary>
        /// 初始化 QuestFailedEvent 实例。
        /// </summary>
        public QuestFailedEvent(TId ownerId, int questId, QuestHandle handle, FailReason failReason)
        {
            OwnerId = ownerId;
            QuestId = questId;
            Handle = handle;
            FailReason = failReason;
        }
    }
}
