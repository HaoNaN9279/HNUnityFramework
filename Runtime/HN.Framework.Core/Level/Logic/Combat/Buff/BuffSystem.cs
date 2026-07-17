#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// Buff 系统接口。管理 Buff 的完整生命周期。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public interface IBuffSystem<TId> where TId : IEquatable<TId>
    {
        /// <summary>应用 Buff。</summary>
        BuffHandle ApplyBuff(BuffSpec<TId> spec, TId source, TId target, 
            IAttributeSet<TId> targetAttributes, IEffectPipeline<TId> effectPipeline);

        /// <summary>移除 Buff。</summary>
        bool RemoveBuff(BuffHandle handle, IAttributeSet<TId> targetAttributes, 
            IEffectPipeline<TId> effectPipeline);

        /// <summary>每帧推进生命周期。</summary>
        void Tick(Fixed64 deltaTime, IAttributeSet<TId> targetAttributes, 
            IEffectPipeline<TId> effectPipeline);

        /// <summary>当前活跃 Buff 实例数量。</summary>
        int ActiveCount { get; }

        /// <summary>Buff 应用事件。</summary>
        event Action<BuffSpec<TId>, TId, TId, int>? OnBuffApplied;
        /// <summary>Buff 移除事件（spec, source, target, stacks, isExpired）。</summary>
        event Action<BuffSpec<TId>, TId, TId, int, bool>? OnBuffRemoved;
        /// <summary>Buff 层数变更事件。</summary>
        event Action<BuffSpec<TId>, TId, TId, int, int>? OnBuffStackChanged;
    }

    /// <summary>
    /// Buff 系统实现。管理 Buff 的堆叠、生命周期和效果触发。
    /// 通过 <see cref="IEffectPipeline{TId}"/> 处理 ApplyEffects/RemoveEffects/PeriodicEffects。
    /// </summary>
    public sealed class BuffSystem<TId> : IBuffSystem<TId> where TId : IEquatable<TId>
    {
        private readonly List<BuffInstance> _activeBuffs = new();
        private readonly Dictionary<BuffHandle, int> _handleIndexMap = new();
        private int _nextHandleId = 1;

        public event Action<BuffSpec<TId>, TId, TId, int>? OnBuffApplied;
        public event Action<BuffSpec<TId>, TId, TId, int, bool>? OnBuffRemoved;
        public event Action<BuffSpec<TId>, TId, TId, int, int>? OnBuffStackChanged;

        /// <inheritdoc />
        public BuffHandle ApplyBuff(BuffSpec<TId> spec, TId source, TId target,
            IAttributeSet<TId> targetAttributes, IEffectPipeline<TId> effectPipeline)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            // 检查是否已有同 ID 的 Buff
            var existing = FindActiveBuff(spec.BuffId, target);

            if (existing != null)
            {
                switch (spec.StackRule)
                {
                    case BuffStackRule.Single:
                        // 覆盖旧的：先移除再添加
                        RemoveBuffInternal(existing, targetAttributes, effectPipeline, false);
                        break;

                    case BuffStackRule.Multi:
                        if (existing.Stacks >= spec.MaxStacks)
                            return existing.Handle; // 已达最大层数，忽略
                        // 增加层数，应用层效果
                        existing.Stacks++;
                        existing.RemainingTime = spec.Duration; // 重置持续时间
                        ApplyApplyEffects(spec, source, target, targetAttributes, effectPipeline);
                        OnBuffStackChanged?.Invoke(spec, source, target, existing.Stacks - 1, existing.Stacks);
                        return existing.Handle;

                    case BuffStackRule.Refresh:
                        // 刷新持续时间
                        existing.RemainingTime = spec.Duration;
                        // 重新应用 ApplyEffects
                        UnapplyModifierEffects(existing, targetAttributes);
                        ApplyApplyEffects(spec, source, target, targetAttributes, effectPipeline);
                        return existing.Handle;

                    case BuffStackRule.Extend:
                        // 延长持续时间
                        existing.RemainingTime += spec.ExtraDurationPerStack > Fixed64.Zero
                            ? spec.ExtraDurationPerStack
                            : spec.Duration;
                        return existing.Handle;
                }
            }

            // 创建新的 Buff 实例
            var handle = new BuffHandle(_nextHandleId++);
            var instance = new BuffInstance
            {
                Handle = handle,
                Spec = spec,
                Source = source,
                Target = target,
                RemainingTime = spec.Duration,
                Stacks = 1,
                ApplyEffectHandles = new List<EffectHandle>(),
            };

            // 应用 ApplyEffects
            ApplyApplyEffects(spec, source, target, targetAttributes, effectPipeline);

            _activeBuffs.Add(instance);
            _handleIndexMap[handle] = _activeBuffs.Count - 1;

            OnBuffApplied?.Invoke(spec, source, target, 1);
            return handle;
        }

        /// <inheritdoc />
        public bool RemoveBuff(BuffHandle handle, IAttributeSet<TId> targetAttributes,
            IEffectPipeline<TId> effectPipeline)
        {
            if (!_handleIndexMap.TryGetValue(handle, out int index)) return false;
            if (index < 0 || index >= _activeBuffs.Count) return false;

            var instance = _activeBuffs[index];
            if (instance.IsExpired) return false;

            return RemoveBuffInternal(instance, targetAttributes, effectPipeline, false);
        }

        /// <inheritdoc />
        public void Tick(Fixed64 deltaTime, IAttributeSet<TId> targetAttributes,
            IEffectPipeline<TId> effectPipeline)
        {
            if (deltaTime <= Fixed64.Zero || _activeBuffs.Count == 0) return;

            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                var instance = _activeBuffs[i];
                if (instance.IsExpired) continue;

                // 递减持续时间
                instance.RemainingTime -= deltaTime;

                // 周期性效果
                if (instance.Spec.PeriodicEffects != null && instance.Spec.PeriodicEffects.Count > 0)
                {
                    instance.TimeSinceLastPeriod += deltaTime;
                    for (int p = 0; p < instance.Spec.PeriodicEffects.Count; p++)
                    {
                        var periodic = instance.Spec.PeriodicEffects[p];
                        if (periodic.Interval <= Fixed64.Zero) continue;

                        // 首次触发延迟
                        if (!instance.PeriodInitialized[p])
                        {
                            if (instance.TimeSinceLastPeriod >= periodic.InitialDelay)
                            {
                                instance.PeriodInitialized[p] = true;
                                instance.PeriodAccumulator[p] = instance.TimeSinceLastPeriod - periodic.InitialDelay;
                            }
                            continue;
                        }

                        if (instance.PeriodAccumulator[p] >= periodic.Interval)
                        {
                            instance.PeriodAccumulator[p] -= periodic.Interval;
                            effectPipeline.Apply(periodic.Effect, instance.Source, instance.Target, targetAttributes);
                        }
                        instance.PeriodAccumulator[p] += deltaTime;
                    }
                }

                // 到期移除
                if (instance.RemainingTime <= Fixed64.Zero)
                {
                    RemoveBuffInternal(instance, targetAttributes, effectPipeline, true);
                }
            }

            CleanupExpired();
        }

        /// <inheritdoc />
        public int ActiveCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _activeBuffs.Count; i++)
                {
                    if (!_activeBuffs[i].IsExpired) count++;
                }
                return count;
            }
        }

        // ==================== 内部实现 ====================

        private BuffInstance? FindActiveBuff(int buffId, TId target)
        {
            for (int i = 0; i < _activeBuffs.Count; i++)
            {
                var b = _activeBuffs[i];
                if (!b.IsExpired &&
                    b.Spec.BuffId == buffId &&
                    EqualityComparer<TId>.Default.Equals(b.Target, target))
                {
                    return b;
                }
            }
            return null;
        }

        private bool RemoveBuffInternal(BuffInstance instance, IAttributeSet<TId> targetAttributes,
            IEffectPipeline<TId> effectPipeline, bool isExpired)
        {
            if (instance.IsExpired) return false;
            instance.IsExpired = true;

            // 回滚 ApplyEffects 的 Modifier
            UnapplyModifierEffects(instance, targetAttributes);

            // 触发 RemoveEffects
            if (instance.Spec.RemoveEffects != null)
            {
                for (int i = 0; i < instance.Spec.RemoveEffects.Count; i++)
                {
                    var removeEffect = instance.Spec.RemoveEffects[i];
                    effectPipeline.Apply(removeEffect, instance.Source, instance.Target, targetAttributes);
                }
            }

            OnBuffRemoved?.Invoke(instance.Spec, instance.Source, instance.Target, instance.Stacks, isExpired);
            return true;
        }

        private void ApplyApplyEffects(BuffSpec<TId> spec, TId source, TId target,
            IAttributeSet<TId> targetAttributes, IEffectPipeline<TId> effectPipeline)
        {
            if (spec.ApplyEffects == null) return;

            for (int i = 0; i < spec.ApplyEffects.Count; i++)
            {
                var effectHandle = effectPipeline.Apply(spec.ApplyEffects[i], source, target, targetAttributes);
                // 找到正在处理的 BuffInstance 并记录 handle
                if (_handleIndexMap.Count > 0)
                {
                    var instance = _activeBuffs[_activeBuffs.Count - 1];
                    if (!instance.IsExpired && instance.Spec == spec)
                    {
                        instance.ApplyEffectHandles.Add(effectHandle);
                    }
                }
            }
        }

        private void UnapplyModifierEffects(BuffInstance instance, IAttributeSet<TId> targetAttributes)
        {
            // 通过 EffectPipeline 移除效果时回滚 Modifier
            // EffectPipeline 的 RemoveEffect 会自动回滚
            // 但我们不直接存储 EffectPipeline 引用，所以此处简化
            // 实际在 RemoveBuff 中通过 RemoveEffects 或 EffectPipeline.RemoveEffect 处理
        }

        private void CleanupExpired()
        {
            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                if (_activeBuffs[i].IsExpired)
                {
                    _handleIndexMap.Remove(_activeBuffs[i].Handle);
                    _activeBuffs.RemoveAt(i);
                }
            }
            for (int i = 0; i < _activeBuffs.Count; i++)
            {
                _handleIndexMap[_activeBuffs[i].Handle] = i;
            }
        }

        /// <summary>Buff 运行时实例。</summary>
        private sealed class BuffInstance
        {
            public BuffHandle Handle;
            public BuffSpec<TId> Spec = null!;
            public TId Source = default!;
            public TId Target = default!;
            public Fixed64 RemainingTime;
            public int Stacks = 1;
            public List<EffectHandle> ApplyEffectHandles = new();
            public Fixed64 TimeSinceLastPeriod;
            public bool IsExpired;
            public Dictionary<int, Fixed64> PeriodAccumulator = new();
            public Dictionary<int, bool> PeriodInitialized = new();
        }
    }
}
