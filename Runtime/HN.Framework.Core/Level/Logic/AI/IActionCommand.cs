namespace HN.Framework.Core.Level.Logic.AI
{
    using HN.Framework.Core.Driver.Common;

    /// <summary>
    /// 动作指令接口 — 决策管线输出的原子动作。
    /// 每个 <see cref="IActionCommand"/> 表示 Agent 执行的一个原子级行为单元，
    /// 由 <see cref="IDecisionStrategy"/> 生成并通过决策管线调度执行。
    /// </summary>
    /// <remarks>
    /// <para>框架不提供预设的 Action 类型，项目需自行实现 IActionCommand 并注册到决策管线。</para>
    /// <para>项目自定义 Action 示例：</para>
    /// <code>
    /// // 项目自定义 MoveToAction
    /// public class YourMoveToAction : IReference, IActionCommand
    /// {
    ///     public string TypeName => "MoveTo";
    ///     public int Priority { get; set; }
    ///     public ActionCommandStatus Status { get; set; }
    ///     public float TargetX { get; set; }
    ///     public float TargetY { get; set; }
    ///     public float TargetZ { get; set; }
    ///
    ///     public void Initialize() => Status = ActionCommandStatus.Pending;
    ///     public void Execute() => Status = ActionCommandStatus.Running;
    ///     public void Cancel() => Status = ActionCommandStatus.Cancelled;
    ///     public void Clear() { TargetX = 0f; TargetY = 0f; TargetZ = 0f; Priority = 0; Status = ActionCommandStatus.Pending; }
    /// }
    ///
    /// // 在策略中使用工厂委托生成
    /// strategy.AddAction("Move", ctx => ReferencePool.Acquire&lt;YourMoveToAction&gt;());
    ///
    /// // 在 DefaultActionExecutor 中注册执行处理器
    /// executor.RegisterHandler&lt;YourMoveToAction&gt;(move =>
    /// {
    ///     navAdapter.SetDestination(new Vector3(move.TargetX, move.TargetY, move.TargetZ));
    ///     return navAdapter.HasReachedDestination();
    /// });
    /// </code>
    /// </remarks>
    public interface IActionCommand : IReference
    {
        /// <summary>
        /// 指令类型名称，用于日志追踪和调试。
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// 执行优先级。数值越高越优先执行。
        /// </summary>
        int Priority { get; set; }

        /// <summary>
        /// 指令当前状态。
        /// </summary>
        ActionCommandStatus Status { get; set; }

        /// <summary>
        /// 初始化指令。在指令入队或即将执行前调用一次。
        /// </summary>
        void Initialize();

        /// <summary>
        /// 执行指令。由决策管线在合适的时机调用。
        /// </summary>
        void Execute();

        /// <summary>
        /// 中断指令执行。当更高优先级指令抢占或外部取消时调用。
        /// </summary>
        void Cancel();
    }

    /// <summary>
    /// 动作指令状态枚举。
    /// </summary>
    public enum ActionCommandStatus
    {
        /// <summary>
        /// 等待执行。
        /// </summary>
        Pending,

        /// <summary>
        /// 正在执行。
        /// </summary>
        Running,

        /// <summary>
        /// 已成功完成。
        /// </summary>
        Completed,

        /// <summary>
        /// 已被取消。
        /// </summary>
        Cancelled,

        /// <summary>
        /// 执行失败。
        /// </summary>
        Failed
    }
}
