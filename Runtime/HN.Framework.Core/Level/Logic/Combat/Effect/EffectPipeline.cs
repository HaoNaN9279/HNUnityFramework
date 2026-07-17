#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 效果管线接口。通过 <see cref="IAttributeSet{TId}"/> 管理 Effect 属性修正的生命周期。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public interface IEffectPipeline<TId> where TId : IEquatable<TId>
    {
        /// <summary>应用效果到目标属性集。</summary>
        EffectHandle Apply(EffectSpec<TId> spec, TId source, TId target, IAttributeSet<TId> targetAttributes);

        /// <summary>通过句柄移除活跃效果，自动回滚 Modifier。</summary>
        bool RemoveEffect(EffectHandle handle, IAttributeSet<TId> targetAttributes);

        /// <summary>每帧推进生命周期（处理 Duration 到期、Periodic 触发）。</summary>
        void Tick(Fixed64 deltaTime, IAttributeSet<TId> targetAttributes);

        /// <summary>当前活跃效果数量（不含已过期）。</summary>
        int ActiveCount { get; }
    }

    /// <summary>
    /// 效果管线实现。
    /// Apply 时通过 <see cref="IAttributeSet{TId}.AddModifier"/> 将 Effect 的 <see cref="AttributeModifier{TId}"/>
    /// 应用到目标属性，记录返回的 <see cref="ModifierHandle"/>。在效果移除/到期时通过
    /// <see cref="IAttributeSet{TId}.RemoveModifier"/> 回滚。
    /// </summary>
    public sealed class EffectPipeline<TId> : IEffectPipeline<TId> where TId : IEquatable<TId>
    {
        private readonly List<ActiveEffect> _activeEffects = new();
        private readonly Dictionary<EffectHandle, int> _handleIndexMap = new();
        private int _nextHandleId = 1;

        public event Action<EffectSpec<TId>, TId, TId>? OnEffectApplied;
        public event Action<EffectSpec<TId>, TId, TId, bool>? OnEffectRemoved;

        /// <inheritdoc />
        public EffectHandle Apply(EffectSpec<TId> spec, TId source, TId target, IAttributeSet<TId> targetAttributes)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            var handle = new EffectHandle(_nextHandleId++);

            // 应用所有 AttributeModifier
            var modHandles = ApplyAttributeModifiers(spec.AttributeModifiers, targetAttributes);

            switch (spec.Type)
            {
                case EffectType.Instant:
                    // Instant：应用后立即移除（效果已完成）
                    RemoveModifierHandles(modHandles, targetAttributes);
                    OnEffectApplied?.Invoke(spec, source, target);
                    return handle;

                case EffectType.Duration:
                case EffectType.Infinite:
                {
                    var remaining = spec.Type == EffectType.Duration ? spec.Duration : Fixed64.MaxValue;
                    var instance = new ActiveEffect
                    {
                        Handle = handle,
                        Spec = spec,
                        Source = source,
                        Target = target,
                        AppliedModHandles = modHandles,
                        RemainingTime = remaining,
                        IsDelayed = spec.HasDelay && spec.DelayTime > Fixed64.Zero,
                        DelayRemaining = spec.HasDelay ? spec.DelayTime : Fixed64.Zero,
                    };

                    _activeEffects.Add(instance);
                    _handleIndexMap[handle] = _activeEffects.Count - 1;
                    OnEffectApplied?.Invoke(spec, source, target);
                    return handle;
                }

                default:
                    return handle;
            }
        }

        /// <inheritdoc />
        public bool RemoveEffect(EffectHandle handle, IAttributeSet<TId> targetAttributes)
        {
            if (!_handleIndexMap.TryGetValue(handle, out int index)) return false;
            if (index < 0 || index >= _activeEffects.Count) return false;

            var instance = _activeEffects[index];
            if (instance.IsExpired) return false;

            instance.IsExpired = true;
            RemoveModifierHandles(instance.AppliedModHandles, targetAttributes);
            OnEffectRemoved?.Invoke(instance.Spec, instance.Source, instance.Target, false);
            return true;
        }

        /// <inheritdoc />
        public void Tick(Fixed64 deltaTime, IAttributeSet<TId> targetAttributes)
        {
            if (deltaTime <= Fixed64.Zero || _activeEffects.Count == 0) return;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var inst = _activeEffects[i];
                if (inst.IsExpired) continue;

                // 延迟逻辑
                if (inst.IsDelayed)
                {
                    inst.DelayRemaining -= deltaTime;
                    if (inst.DelayRemaining <= Fixed64.Zero)
                    {
                        var handles = ApplyAttributeModifiers(inst.Spec.AttributeModifiers, targetAttributes);
                        inst.AppliedModHandles.AddRange(handles);
                        inst.IsDelayed = false;
                    }
                    continue;
                }

                // Duration 时间递减
                if (inst.Spec.Type == EffectType.Duration)
                {
                    inst.RemainingTime -= deltaTime;
                    if (inst.RemainingTime <= Fixed64.Zero)
                    {
                        inst.IsExpired = true;
                        RemoveModifierHandles(inst.AppliedModHandles, targetAttributes);
                        OnEffectRemoved?.Invoke(inst.Spec, inst.Source, inst.Target, true);
                        continue;
                    }
                }

                // 周期触发
                if (inst.Spec.PeriodInterval > Fixed64.Zero)
                {
                    inst.TimeSinceLastPeriod += deltaTime;
                    while (inst.TimeSinceLastPeriod >= inst.Spec.PeriodInterval)
                    {
                        inst.TimeSinceLastPeriod -= inst.Spec.PeriodInterval;
                        ApplyAttributeModifiers(inst.Spec.AttributeModifiers, targetAttributes);
                    }
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
                for (int i = 0; i < _activeEffects.Count; i++)
                {
                    if (!_activeEffects[i].IsExpired) count++;
                }
                return count;
            }
        }

        // ==================== 内部 ====================

        private List<ModifierHandle> ApplyAttributeModifiers(
            List<AttributeModifier<TId>>? attrMods, IAttributeSet<TId> targetAttributes)
        {
            var handles = new List<ModifierHandle>();
            if (attrMods == null) return handles;

            for (int i = 0; i < attrMods.Count; i++)
            {
                var am = attrMods[i];
                var handle = targetAttributes.AddModifier(am.TargetAttribute, am.Modifier);
                handles.Add(handle);
            }
            return handles;
        }

        private static void RemoveModifierHandles(List<ModifierHandle> handles, IAttributeSet<TId> targetAttributes)
        {
            if (handles == null) return;
            for (int i = 0; i < handles.Count; i++)
            {
                targetAttributes.RemoveModifier(handles[i]);
            }
            handles.Clear();
        }

        private void CleanupExpired()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                if (_activeEffects[i].IsExpired)
                {
                    _handleIndexMap.Remove(_activeEffects[i].Handle);
                    _activeEffects.RemoveAt(i);
                }
            }
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                _handleIndexMap[_activeEffects[i].Handle] = i;
            }
        }

        /// <summary>内部活跃效果实例。</summary>
        private sealed class ActiveEffect
        {
            public EffectHandle Handle;
            public EffectSpec<TId> Spec = null!;
            public TId Source = default!;
            public TId Target = default!;
            public List<ModifierHandle> AppliedModHandles = new();
            public Fixed64 RemainingTime;
            public Fixed64 TimeSinceLastPeriod;
            public bool IsDelayed;
            public Fixed64 DelayRemaining;
            public bool IsExpired;
        }
    }
}
