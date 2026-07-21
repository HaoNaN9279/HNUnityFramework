using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 传播系统。负责执行效果传播流程。
    /// 三要素分离：Strategy（如何寻找目标）+ Filter（筛选条件）+ Effect（命中后效果）。
    /// 效果施加委托给 <see cref="IEffectPipeline{TId}"/> / <see cref="IBuffSystem{TId}"/>。
    /// 自身不依赖 EventBus，仅通过 C# 事件通知外部。
    /// </summary>
    /// <typeparam name="TId">实体标识类型。</typeparam>
    public sealed class PropagationSystem<TId> where TId : IEquatable<TId>
    {
        private readonly Func<TId, IAttributeSet<TId>?> _getTargetAttributes;
        private int _nextHandleId = 1;

        /// <summary>传播开始时触发。</summary>
        public event Action<PropagationContext<TId>>? OnPropagationStarted;

        /// <summary>命中目标时触发（每个目标触发一次）。</summary>
        public event Action<PropagationContext<TId>>? OnTargetHit;

        /// <summary>传播完成时触发（含结果统计）。</summary>
        public event Action<PropagationContext<TId>, PropagationResult<TId>>? OnPropagationComplete;

        /// <summary>
        /// 初始化传播系统。
        /// </summary>
        /// <param name="getTargetAttributes">
        /// 目标属性解析器。通过实体 ID 获取对应的 <see cref="IAttributeSet{TId}"/>，
        /// 返回 null 表示无法解析该目标的属性（将跳过效果施加）。
        /// </param>
        public PropagationSystem(Func<TId, IAttributeSet<TId>?> getTargetAttributes)
        {
            _getTargetAttributes = getTargetAttributes ?? throw new ArgumentNullException(nameof(getTargetAttributes));
        }

        /// <summary>
        /// 执行一次传播。
        /// </summary>
        /// <param name="spec">传播规格，定义跳数、衰减、命中效果等。</param>
        /// <param name="strategy">传播策略，负责寻找每跳的目标。</param>
        /// <param name="filter">可选过滤器，对候选目标进行二次筛选。</param>
        /// <param name="source">传播源标识。</param>
        /// <param name="sourceAttributes">源属性集，用于强度缩放（当前暂未使用，预留扩展）。</param>
        /// <param name="effectPipeline">效果管线，用于施加命中效果。</param>
        /// <param name="buffSystem">可选 Buff 系统，用于传播相关 Buff 操作（当前暂未使用，预留扩展）。</param>
        /// <returns>传播结果，包含命中列表、跳数、成功与否。</returns>
        public PropagationResult<TId> Propagate(
            PropagationSpec<TId> spec,
            IPropagationStrategy<TId> strategy,
            IPropagationFilter<TId>? filter,
            TId source,
            IAttributeSet<TId>? sourceAttributes,
            IEffectPipeline<TId> effectPipeline,
            IBuffSystem<TId>? buffSystem)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            if (strategy == null) throw new ArgumentNullException(nameof(strategy));
            if (effectPipeline == null) throw new ArgumentNullException(nameof(effectPipeline));

            // 1. 验证跳数
            if (spec.MaxHops < 1)
                return PropagationResult<TId>.Empty;

            // 2. 创建传播上下文
            var context = new PropagationContext<TId>(spec, source);

            // 3. 创建句柄
            var handle = new PropagationHandle(_nextHandleId++);

            // 4. 触发传播开始事件
            OnPropagationStarted?.Invoke(context);
            if (context.IsComplete)
                return BuildResult(handle, new List<TId>(), 0);

            var allHitEntities = new List<TId>();
            int totalHops = 0;

            // 5. 循环每跳
            while (context.CurrentHop < spec.MaxHops && !context.IsComplete)
            {
                var candidates = new List<TId>();
                strategy.FindTargets(context, candidates);

                bool hitAny = false;

                foreach (var candidate in candidates)
                {
                    // 过滤器检查
                    if (filter != null && !filter.Pass(candidate, context))
                        continue;

                    // 去重检查
                    if (context.HitEntities.Contains(candidate))
                        continue;

                    // 标记为已命中
                    context.HitEntities.Add(candidate);
                    allHitEntities.Add(candidate);
                    hitAny = true;

                    // 施加命中效果
                    var targetAttributes = _getTargetAttributes(candidate);
                    if (targetAttributes != null)
                    {
                        foreach (var effect in spec.OnHitEffects)
                        {
                            // NOTE: 当 sourceAttributes 不为空时，应使用 context.IntensityMultiplier 缩放效果值。
                            // EffectSpec 当前不支持内置缩放，项目层可通过以下方式实现：
                            //   1. 创建 EffectSpec 工厂方法，基于模板 + 缩放系数生成新实例
                            //   2. 在 AttributeModifier 中使用相对值 + 外部乘数
                            effectPipeline.Apply(effect, source, candidate, targetAttributes);
                        }
                    }

                    // 触发目标命中事件
                    OnTargetHit?.Invoke(context);

                    if (context.IsComplete)
                        break;
                }

                totalHops = context.CurrentHop + 1;

                // 未命中任何目标或提前完成 → 退出
                if (!hitAny || context.IsComplete)
                    break;

                // 推进跳数和衰减
                context.CurrentHop++;
                context.IntensityMultiplier *= spec.IntensityDecay;
            }

            // 6. 构建结果
            var result = BuildResult(handle, allHitEntities, totalHops);

            // 7. 触发传播完成事件
            OnPropagationComplete?.Invoke(context, result);

            return result;
        }

        private static PropagationResult<TId> BuildResult(PropagationHandle handle, List<TId> hitEntities, int totalHops)
        {
            var success = hitEntities.Count > 0;
            return new PropagationResult<TId>(handle, hitEntities, totalHops, success);
        }
    }
}
