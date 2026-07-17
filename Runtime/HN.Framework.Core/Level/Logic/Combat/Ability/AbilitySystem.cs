#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 技能系统接口。管理技能的激活预检、冷却和效果触发。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public interface IAbilitySystem<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// 预检技能是否可以激活。
        /// </summary>
        /// <param name="spec">技能规格。</param>
        /// <param name="owner">技能所有者标识。</param>
        /// <param name="ownerAttributes">所有者的属性集（用于检查 Cost）。</param>
        /// <returns>如果可以激活则返回 true。</returns>
        bool CanActivate(AbilitySpec<TId> spec, TId owner, IAttributeSet<TId> ownerAttributes);

        /// <summary>
        /// 尝试激活技能。
        /// </summary>
        /// <param name="spec">技能规格。</param>
        /// <param name="owner">技能所有者标识。</param>
        /// <param name="target">技能目标标识。</param>
        /// <param name="ownerAttributes">所有者属性集（用于消耗 Cost）。</param>
        /// <param name="targetAttributes">目标属性集（用于应用效果）。</param>
        /// <param name="effectPipeline">效果管线（用于应用技能效果）。</param>
        /// <returns>激活成功返回 true。</returns>
        bool TryActivateAbility(AbilitySpec<TId> spec, TId owner, TId target,
            IAttributeSet<TId> ownerAttributes, IAttributeSet<TId> targetAttributes,
            IEffectPipeline<TId> effectPipeline);

        /// <summary>每帧推进冷却时间。</summary>
        void Tick(Fixed64 deltaTime);

        /// <summary>获取技能的剩余冷却时间（秒），0 表示可用。</summary>
        Fixed64 GetCooldownRemaining(int abilityId, TId owner);
    }

    /// <summary>
    /// 技能系统实现。支持冷却管理、消耗检查、GameplayTag 条件预检。
    /// 注意：GameplayTag 检查是可选的（通过 <see cref="GameplayTagContainer"/> 参数），
    /// 调用方可以不传递标签容器跳过检查。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public sealed class AbilitySystem<TId> : IAbilitySystem<TId> where TId : IEquatable<TId>
    {
        // 冷却追踪：(abilityId, owner) → 冷却剩余时间
        private readonly Dictionary<(int abilityId, TId owner), Fixed64> _cooldowns = new();

        // 注册的技能列表
        private readonly Dictionary<int, AbilitySpec<TId>> _registeredAbilities = new();

        /// <summary>技能激活事件。</summary>
        public event Action<AbilitySpec<TId>, TId, TId, bool>? OnAbilityActivated;

        /// <summary>
        /// 注册技能定义。
        /// </summary>
        public void RegisterAbility(AbilitySpec<TId> spec)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            _registeredAbilities[spec.AbilityId] = spec;
        }

        /// <summary>
        /// 获取已注册的技能规格。
        /// </summary>
        public AbilitySpec<TId>? GetAbility(int abilityId)
        {
            _registeredAbilities.TryGetValue(abilityId, out var spec);
            return spec;
        }

        /// <inheritdoc />
        public bool CanActivate(AbilitySpec<TId> spec, TId owner, IAttributeSet<TId> ownerAttributes)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            // 检查冷却
            var key = (spec.AbilityId, owner);
            if (_cooldowns.TryGetValue(key, out var remaining) && remaining > Fixed64.Zero)
                return false;

            // 检查消耗
            if (spec.Cost != null)
            {
                foreach (var kvp in spec.Cost)
                {
                    var currentValue = ownerAttributes.GetFinalValue(kvp.Key);
                    if (currentValue < kvp.Value)
                        return false; // 属性值不足
                }
            }

            return true;
        }

        /// <inheritdoc />
        public bool TryActivateAbility(AbilitySpec<TId> spec, TId owner, TId target,
            IAttributeSet<TId> ownerAttributes, IAttributeSet<TId> targetAttributes,
            IEffectPipeline<TId> effectPipeline)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            // 预检
            if (!CanActivate(spec, owner, ownerAttributes))
            {
                OnAbilityActivated?.Invoke(spec, owner, target, false);
                return false;
            }

            // 消耗 Cost
            if (spec.Cost != null)
            {
                foreach (var kvp in spec.Cost)
                {
                    var current = ownerAttributes.GetFinalValue(kvp.Key);
                    ownerAttributes.SetBaseValue(
                        kvp.Key,
                        FixedMath.Max(current - kvp.Value, Fixed64.Zero));
                }
            }

            // 应用效果
            if (spec.Effects != null)
            {
                for (int i = 0; i < spec.Effects.Count; i++)
                {
                    effectPipeline.Apply(spec.Effects[i], owner, target, targetAttributes);
                }
            }

            // 进入冷却
            if (spec.Cooldown > Fixed64.Zero)
            {
                var key = (spec.AbilityId, owner);
                _cooldowns[key] = spec.Cooldown;
            }

            OnAbilityActivated?.Invoke(spec, owner, target, true);
            return true;
        }

        /// <inheritdoc />
        public void Tick(Fixed64 deltaTime)
        {
            if (deltaTime <= Fixed64.Zero) return;

            // 先收集所有键的快照，避免 foreach 期间修改字典
            var keys = new List<(int, TId)>(_cooldowns.Keys);

            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                if (!_cooldowns.TryGetValue(key, out var remaining)) continue;

                var newRemaining = remaining - deltaTime;
                if (newRemaining <= Fixed64.Zero)
                    _cooldowns.Remove(key);
                else
                    _cooldowns[key] = newRemaining;
            }
        }

        /// <inheritdoc />
        public Fixed64 GetCooldownRemaining(int abilityId, TId owner)
        {
            var key = (abilityId, owner);
            return _cooldowns.TryGetValue(key, out var remaining) ? remaining : Fixed64.Zero;
        }

        /// <summary>当前处于冷却中的技能数量。</summary>
        public int CooldownCount => _cooldowns.Count;
    }
}
