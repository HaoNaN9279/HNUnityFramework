#nullable enable

using System.Collections.Generic;
using FixedMathSharp;
using HN.Framework.Core.Level.Logic.Combat;

namespace HN.Framework.Core.Level.Logic.Inventory
{
    /// <summary>
    /// 物品附带的 Buff 效果定义。
    /// 提供 BuildBuffSpec 方法将自身转换为 L7 的 BuffSpec。
    /// </summary>
    public sealed class ItemBuffSpec
    {
        /// <summary>Buff 定义 ID，对应 L7 BuffSpec.BuffId。</summary>
        public int BuffId { get; set; }

        /// <summary>关联的 Behaviour 定义 ID，0 表示无自定义行为。</summary>
        public int BehaviourId { get; set; }

        /// <summary>传递给 Behaviour 的自定义参数。</summary>
        public Dictionary<string, Fixed64>? CustomParams { get; set; }

        /// <summary>
        /// 构建 L7 BuffSpec 实例。TId 通常为 uint（EntityId）。
        /// </summary>
        public BuffSpec<TId> BuildBuffSpec<TId>(TId source) where TId : System.IEquatable<TId>
        {
            var spec = new BuffSpec<TId>
            {
                BuffId = BuffId,
                BehaviourId = BehaviourId,
                BehaviourCustomParams = CustomParams,
                // Duration/ApplyEffects/RemoveEffects 等由配置表（Luban）根据 BuffId 加载
            };
            return spec;
        }
    }
}
