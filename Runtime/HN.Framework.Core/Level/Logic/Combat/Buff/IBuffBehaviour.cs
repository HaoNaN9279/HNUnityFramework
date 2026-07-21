#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// Buff 行为上下文。包含 Buff 运行时的全部上下文信息。
    /// 由 <see cref="BuffSystem{TId}"/> 在创建 <see cref="IBuffBehaviour{TId}"/> 时填充，
    /// 通过 IBuffBehaviour 的生命周期方法传递给 Behaviour。
    /// </summary>
    public sealed class BuffBehaviourContext<TId> where TId : IEquatable<TId>
    {
        /// <summary>Buff 定义 ID。</summary>
        public int BuffId { get; set; }

        /// <summary>Buff 显示名称。</summary>
        public string? DisplayName { get; set; }

        /// <summary>Buff 施加者标识。</summary>
        public TId Source { get; set; } = default!;

        /// <summary>Buff 目标标识。</summary>
        public TId Target { get; set; } = default!;

        /// <summary>当前层数。</summary>
        public int Stacks { get; set; } = 1;

        /// <summary>目标属性集（可 null）。</summary>
        public IAttributeSet<TId>? TargetAttributes { get; set; }

        /// <summary>效果管线（可 null）。</summary>
        public IEffectPipeline<TId>? EffectPipeline { get; set; }

        /// <summary>自定义参数表（来自 <see cref="BuffSpec{TId}.BehaviourCustomParams"/>）。</summary>
        public Dictionary<string, Fixed64>? CustomParams { get; set; }
    }

    /// <summary>
    /// Buff 自定义行为接口。
    /// 当纯数据配置（<see cref="BuffSpec{TId}"/>）的条件组合逻辑或跨系统交互
    /// 无法通过 <see cref="IAttributeSet{TId}.AddModifier"/> 和 <see cref="Action"/> 事件表达时，
    /// 通过实现此接口在 Buff 生命周期的各阶段注入自定义逻辑。
    /// </summary>
    /// <remarks>
    /// 纯数值类效果和条件触发类效果优先走 <see cref="AttributeSet{TId}"/> 的 Modifier 系统和
    /// <see cref="IEffectPipeline{TId}"/>。Behaviour 仅在上游路径无法表达时使用。
    /// </remarks>
    /// <typeparam name="TId">实体标识类型（必须实现 IEquatable{TId}）。</typeparam>
    public interface IBuffBehaviour<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// Buff 被应用时调用。此时效果的 ApplyEffects 已施加完毕。
        /// </summary>
        void OnApply(BuffBehaviourContext<TId> context);

        /// <summary>
        /// 每帧 Tick 时调用。由 <see cref="BuffSystem{TId}.Tick"/> 驱动。
        /// </summary>
        /// <param name="deltaTime">本帧逻辑增量时间（秒）。</param>
        void OnTick(Fixed64 deltaTime, BuffBehaviourContext<TId> context);

        /// <summary>
        /// Buff 被移除时调用。此时效果的 RemoveEffects 已施加完毕。
        /// </summary>
        /// <param name="isExpired">true 表示到期自动移除，false 表示手动移除。</param>
        void OnRemove(bool isExpired, BuffBehaviourContext<TId> context);

        /// <summary>
        /// Buff 层数变更时调用（仅 <see cref="BuffStackRule.Multi"/> 规则）。
        /// </summary>
        /// <param name="oldStacks">变更前的层数。</param>
        /// <param name="newStacks">变更后的层数。</param>
        void OnStackChanged(int oldStacks, int newStacks, BuffBehaviourContext<TId> context);
    }
}
