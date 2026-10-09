#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.UI;

namespace HN.Framework.Core.Tests.Capability.UI
{
    /// <summary>
    /// <see cref="UICanvasSorting"/> 的纯逻辑单元测试：层内排序号限制与最终 sortingOrder 计算。
    /// </summary>
    [TestFixture]
    public class UICanvasSortingTests
    {
        /// <summary>
        /// 层内最小排序号为 1。
        /// </summary>
        [Test]
        public void SubOrderMin_IsOne()
        {
            Assert.AreEqual(1, UICanvasSorting.SubOrderMin);
        }

        /// <summary>
        /// 层内最大排序号为 99。
        /// </summary>
        [Test]
        public void SubOrderMax_Is99()
        {
            Assert.AreEqual(99, UICanvasSorting.SubOrderMax);
        }

        /// <summary>
        /// 低于下限的排序号被限制到下限。
        /// </summary>
        [Test]
        public void ClampSubOrder_BelowMin_ReturnsMin()
        {
            Assert.AreEqual(UICanvasSorting.SubOrderMin, UICanvasSorting.ClampSubOrder(-100));
        }

        /// <summary>
        /// 高于上限的排序号被限制到上限。
        /// </summary>
        [Test]
        public void ClampSubOrder_AboveMax_ReturnsMax()
        {
            Assert.AreEqual(UICanvasSorting.SubOrderMax, UICanvasSorting.ClampSubOrder(100000));
        }

        /// <summary>
        /// 范围内的排序号保持不变。
        /// </summary>
        [Test]
        public void ClampSubOrder_InRange_ReturnsSame()
        {
            Assert.AreEqual(42, UICanvasSorting.ClampSubOrder(42));
        }

        /// <summary>
        /// 最终 sortingOrder 等于层基值 + 层内排序号。
        /// </summary>
        [Test]
        public void ComputeSortingOrder_AddsLayerBase()
        {
            Assert.AreEqual((int)UILayer.UI + 5, UICanvasSorting.ComputeSortingOrder(UILayer.UI, 5));
        }

        /// <summary>
        /// 越界的层内排序号在计算时被限制。
        /// </summary>
        [Test]
        public void ComputeSortingOrder_ClampsSubOrder()
        {
            Assert.AreEqual(
                (int)UILayer.UI + UICanvasSorting.SubOrderMax,
                UICanvasSorting.ComputeSortingOrder(UILayer.UI, 9999));
        }

        /// <summary>
        /// 取最大层内排序号时，最终 sortingOrder 仍小于下一层的基值，不会越层。
        /// </summary>
        [Test]
        public void ComputeSortingOrder_MaxSubOrder_DoesNotCrossNextLayer()
        {
            foreach (UILayer layer in System.Enum.GetValues(typeof(UILayer)))
            {
                int order = UICanvasSorting.ComputeSortingOrder(layer, UICanvasSorting.SubOrderMax);
                int nextLayerBase = (int)layer + 100;

                Assert.That(order, Is.LessThan(nextLayerBase),
                    $"Layer '{layer}' 的最大 sortingOrder {order} 不应跨越下一层基值 {nextLayerBase}。");
            }
        }
    }
}
