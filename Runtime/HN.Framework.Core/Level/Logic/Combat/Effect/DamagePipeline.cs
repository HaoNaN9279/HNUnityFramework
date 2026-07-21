#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 管线阶段委托。对 <see cref="DamageContext{TId}"/> 进行引用传递修改。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    /// <param name="context">伤害上下文（引用传递）。</param>
    public delegate void DamageStageAction<TId>(ref DamageContext<TId> context) where TId : IEquatable<TId>;

    /// <summary>
    /// 伤害计算公式接口。项目可通过实现此接口替换默认加减法公式。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public interface IDamageFormula<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// 计算最终伤害值。
        /// </summary>
        /// <param name="context">伤害上下文（含攻击力、防御力等）。</param>
        /// <returns>计算后的伤害值。</returns>
        Fixed64 Calculate(ref DamageContext<TId> context);
    }

    /// <summary>
    /// 默认伤害公式。直接使用 PreMigration 阶段处理后的伤害值，最低为 0。
    /// 项目可通过实现 <see cref="IDamageFormula{TId}"/> 替换公式，或在 PreMigration 阶段实现减伤逻辑。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public sealed class DefaultDamageFormula<TId> : IDamageFormula<TId> where TId : IEquatable<TId>
    {
        public Fixed64 Calculate(ref DamageContext<TId> context)
        {
            return FixedMath.Max(context.PreMigrationDamage, Fixed64.Zero);
        }
    }

    /// <summary>
    /// 伤害上下文。数据沿管线四阶段流动。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public struct DamageContext<TId> where TId : IEquatable<TId>
    {
        /// <summary>攻击者标识。</summary>
        public TId AttackerId;

        /// <summary>防御者标识。</summary>
        public TId DefenderId;

        /// <summary>攻击者属性集。</summary>
        public IAttributeSet<TId>? AttackerAttributes;

        /// <summary>防御者属性集。</summary>
        public IAttributeSet<TId>? DefenderAttributes;

        /// <summary>基础伤害值（PreMigration 前设置）。</summary>
        public Fixed64 BaseDamage;

        /// <summary>经 PreMigration 调整后的伤害值。</summary>
        public Fixed64 PreMigrationDamage;

        /// <summary>经公式计算后的伤害值。</summary>
        public Fixed64 CalculatedDamage;

        /// <summary>经 PostMigration 调整后的最终伤害值。</summary>
        public Fixed64 FinalDamage;

        /// <summary>是否暴击。</summary>
        public bool IsCritical;

        /// <summary>各阶段可附加的自定义数据。</summary>
        public Dictionary<string, object>? CustomData;
    }

    /// <summary>
    /// 四阶段伤害管线。
    /// 
    /// 管线流程：
    /// <c>PreMigration → DamageCalculate（IDamageFormula）→ PostMigration → ApplyDamage</c>
    /// 
    /// 各阶段可通过注册委托扩展，无需继承。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public sealed class DamagePipeline<TId> where TId : IEquatable<TId>
    {
        /// <summary>目标 HP 属性类型（默认使用 AttributeType(1)）。</summary>
        public static readonly AttributeType HP = new AttributeType(1);

        private readonly IDamageFormula<TId> _formula;

        /// <summary>PreMigration 阶段委托（增伤/减伤判断）。签名：(ref DamageContext{TId}) → void。</summary>
        public DamageStageAction<TId>? OnPreMigration { get; set; }

        /// <summary>PostMigration 阶段委托（护甲/分摊/反弹）。签名：(ref DamageContext{TId}) → void。</summary>
        public DamageStageAction<TId>? OnPostMigration { get; set; }

        /// <summary>ApplyDamage 阶段委托（替代默认扣血逻辑）。签名：(ref DamageContext{TId}) → void。</summary>
        public DamageStageAction<TId>? OnApplyDamage { get; set; }

        /// <summary>伤害事件（context, 是否已被处理）。</summary>
        public event Action<DamageContext<TId>, bool>? OnDamageProcessed;

        /// <summary>
        /// 初始化伤害管线。
        /// </summary>
        /// <param name="formula">伤害计算公式。为 null 时使用 <see cref="DefaultDamageFormula{TId}"/>。</param>
        public DamagePipeline(IDamageFormula<TId>? formula = null)
        {
            _formula = formula ?? new DefaultDamageFormula<TId>();
        }

        /// <summary>
        /// 执行完整四阶段伤害流程。
        /// </summary>
        /// <param name="context">伤害上下文。</param>
        /// <param name="targetHpAttribute">目标 HP 属性类型。为 null 时使用 <see cref="HP"/>。</param>
        /// <returns>实际造成的伤害值。</returns>
        public Fixed64 Process(ref DamageContext<TId> context, AttributeType? targetHpAttribute = null)
        {
            var hpAttr = targetHpAttribute ?? HP;

            // === 阶段 1: PreMigration（增伤/减伤判断） ===
            context.PreMigrationDamage = context.BaseDamage;
            OnPreMigration?.Invoke(ref context);
            // PreMigration 可能修改了 BaseDamage，重新同步
            context.PreMigrationDamage = context.BaseDamage;

            // === 阶段 2: DamageCalculate（核心公式） ===
            context.CalculatedDamage = _formula.Calculate(ref context);

            // === 阶段 3: PostMigration（护甲/分摊/反弹） ===
            context.FinalDamage = context.CalculatedDamage;
            OnPostMigration?.Invoke(ref context);

            // === 阶段 4: ApplyDamage（应用到目标） ===
            bool handled = false;
            if (OnApplyDamage != null)
            {
                OnApplyDamage(ref context);
                handled = true;
            }
            else
            {
                // 默认：减少目标 HP
                if (context.DefenderAttributes != null)
                {
                    var currentHp = context.DefenderAttributes.GetFinalValue(hpAttr);
                    var newHp = FixedMath.Max(currentHp - context.FinalDamage, Fixed64.Zero);
                    context.DefenderAttributes.SetBaseValue(hpAttr, newHp);
                    handled = true;
                }
            }

            OnDamageProcessed?.Invoke(context, handled);
            return context.FinalDamage;
        }

        /// <summary>
        /// 便捷方法：执行一次完整伤害流程。
        /// </summary>
        public Fixed64 DealDamage(
            TId attackerId,
            TId defenderId,
            Fixed64 baseDamage,
            IAttributeSet<TId> attackerAttributes,
            IAttributeSet<TId> defenderAttributes,
            bool isCritical = false)
        {
            var context = new DamageContext<TId>
            {
                AttackerId = attackerId,
                DefenderId = defenderId,
                BaseDamage = baseDamage,
                AttackerAttributes = attackerAttributes,
                DefenderAttributes = defenderAttributes,
                IsCritical = isCritical,
            };

            return Process(ref context);
        }
    }
}
