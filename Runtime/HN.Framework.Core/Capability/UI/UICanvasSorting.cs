#nullable enable

namespace HN.Framework.Core.Capability.UI
{
    /// <summary>
    /// UI 层内 Canvas 排序工具（纯逻辑，无 Unity 依赖）。
    /// <para>
    /// 每个 <see cref="UILayer"/> 保留一个根 Canvas 作为容器与排序基准
    /// （sortingOrder = 层基值，即枚举值本身）。层内的用户自定义嵌套 SubCanvas
    /// 通过 <see cref="SubOrder"/> 在层内排序，最终
    /// <c>sortingOrder = 层基值 + SubOrder</c>，保证不会跨越到相邻层。
    /// </para>
    /// <para>
    /// 层间隔为 100，层根占用基值（偏移 0），因此层内排序号范围为
    /// [<see cref="SubOrderMin"/>, <see cref="SubOrderMax"/>] = [1, 99]，
    /// 即每层最多容纳 99 个自定义 SubCanvas。
    /// </para>
    /// </summary>
    public static class UICanvasSorting
    {
        /// <summary>
        /// 层内最小排序号，表示最贴近层根 Canvas 的位置。
        /// </summary>
        public const int SubOrderMin = 1;

        /// <summary>
        /// 层内最大排序号。层间隔为 100，层根占用基值（偏移 0），故上限为 99。
        /// </summary>
        public const int SubOrderMax = 99;

        /// <summary>
        /// 将层内排序号限制到 [<see cref="SubOrderMin"/>, <see cref="SubOrderMax"/>] 范围。
        /// </summary>
        /// <param name="subOrder">原始层内排序号。</param>
        /// <returns>限制后的排序号。</returns>
        public static int ClampSubOrder(int subOrder)
        {
            if (subOrder < SubOrderMin)
                return SubOrderMin;
            if (subOrder > SubOrderMax)
                return SubOrderMax;
            return subOrder;
        }

        /// <summary>
        /// 计算嵌套 SubCanvas 的最终 sortingOrder。
        /// </summary>
        /// <param name="layer">所属 UI 层。</param>
        /// <param name="subOrder">层内排序号（会被限制到合法范围）。</param>
        /// <returns>最终 sortingOrder，等于层基值 + 限制后的层内排序号。</returns>
        public static int ComputeSortingOrder(UILayer layer, int subOrder)
        {
            return (int)layer + ClampSubOrder(subOrder);
        }
    }
}
