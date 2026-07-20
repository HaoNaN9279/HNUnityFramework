#nullable enable

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 条件类型枚举。
    /// 标识 <see cref="ConditionDef"/> 对应的条件评估类别。
    /// </summary>
    public enum ConditionType : byte
    {
        /// <summary>累计计数（杀怪数量、收集数量等）。</summary>
        Counter = 0,

        /// <summary>状态检查（等级>=X、持有物品、已完成任务）。</summary>
        State = 1,

        /// <summary>多重计数（多个子计数器，全部满足才算完成）。</summary>
        MultiCounter = 2,
    }
}
