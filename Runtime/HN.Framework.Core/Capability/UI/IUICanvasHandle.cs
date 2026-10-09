#nullable enable

namespace HN.Framework.Core.Capability.UI
{
    /// <summary>
    /// 用户自定义嵌套 SubCanvas 的句柄。
    /// <para>
    /// Core 程序集开启了 noEngineReferences，无法引用 <c>UnityEngine.Canvas</c>，
    /// 因此通过此句柄抽象 SubCanvas 的身份、所属层与层内排序号。
    /// Unity 层的具体实现额外持有 <c>UnityEngine.Canvas</c> 引用。
    /// </para>
    /// </summary>
    public interface IUICanvasHandle
    {
        /// <summary>
        /// 句柄唯一标识，由 UIManager 分配。
        /// </summary>
        int Id { get; }

        /// <summary>
        /// SubCanvas 所属的 UI 层。
        /// </summary>
        UILayer Layer { get; }

        /// <summary>
        /// 当前层内排序号（范围 [<see cref="UICanvasSorting.SubOrderMin"/>,
        /// <see cref="UICanvasSorting.SubOrderMax"/>]）。
        /// </summary>
        int SubOrder { get; }
    }
}
