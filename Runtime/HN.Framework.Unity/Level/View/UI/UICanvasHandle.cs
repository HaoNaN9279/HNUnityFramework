#nullable enable

using HN.Framework.Core.Capability.UI;
using UnityEngine;

namespace HN.Framework.Unity.Level.View.UI
{
    /// <summary>
    /// <see cref="IUICanvasHandle"/> 的 Unity 实现，持有嵌套 Canvas 的运行时引用。
    /// 用户可通过 <see cref="Canvas"/> / <see cref="Root"/> 在自定义 SubCanvas 下挂载 UI 元素。
    /// </summary>
    public sealed class UICanvasHandle : IUICanvasHandle
    {
        private int _subOrder;

        internal UICanvasHandle(int id, UILayer layer, int subOrder, Canvas canvas, Transform root)
        {
            Id = id;
            Layer = layer;
            _subOrder = subOrder;
            Canvas = canvas;
            Root = root;
        }

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public UILayer Layer { get; }

        /// <inheritdoc />
        public int SubOrder => _subOrder;

        /// <summary>
        /// 嵌套 Canvas 组件。
        /// </summary>
        public Canvas Canvas { get; }

        /// <summary>
        /// Canvas 的根 Transform，用户可在此下挂载子 UI。
        /// </summary>
        public Transform Root { get; }

        /// <summary>
        /// 句柄是否仍然有效（Canvas 未被销毁）。
        /// </summary>
        public bool IsValid => Canvas != null;

        /// <summary>
        /// 更新层内排序号并重算 sortingOrder。
        /// </summary>
        /// <param name="subOrder">已限制到合法范围的层内排序号。</param>
        internal void ApplySubOrder(int subOrder)
        {
            _subOrder = subOrder;
            if (Canvas != null)
                Canvas.sortingOrder = UICanvasSorting.ComputeSortingOrder(Layer, subOrder);
        }
    }
}
