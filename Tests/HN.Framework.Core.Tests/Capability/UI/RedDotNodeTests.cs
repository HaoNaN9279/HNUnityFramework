#nullable enable

using System;
using NUnit.Framework;
using HN.Framework.Core.Capability.UI;

namespace HN.Framework.Core.Tests.Capability.UI
{
    [TestFixture]
    public class RedDotNodeTests
    {
        private RedDotNode _root = null!;

        [SetUp]
        public void SetUp()
        {
            _root = new RedDotNode("root");
        }

        [TearDown]
        public void TearDown()
        {
            _root = null!;
        }

        /// <summary>
        /// SetCount 更新单个节点的 Count 值。
        /// </summary>
        [Test]
        public void SetCount_UpdatesCountValue()
        {
            _root.SetCount(5);

            Assert.AreEqual(5, _root.Count);
        }

        /// <summary>
        /// 构造时传入初始 Count，属性应反映该值。
        /// </summary>
        [Test]
        public void Constructor_WithInitialCount_SetsCount()
        {
            RedDotNode node = new RedDotNode("item", 10);

            Assert.AreEqual(10, node.Count);
        }

        /// <summary>
        /// SetCount 修改后应触发 OnCountChanged 事件。
        /// </summary>
        [Test]
        public void SetCount_FiresOnCountChanged()
        {
            int receivedCount = -1;
            _root.OnCountChanged += (c) => receivedCount = c;

            _root.SetCount(3);

            Assert.AreEqual(3, receivedCount);
        }

        /// <summary>
        /// SetCount(0) 将 Count 清零，并触发 OnCountChanged。
        /// </summary>
        [Test]
        public void SetCount_Zero_ClearsCount()
        {
            _root.SetCount(5);
            _root.SetCount(0);

            Assert.AreEqual(0, _root.Count);
        }

        /// <summary>
        /// SetCount(0) 后 OnCountChanged 收到值 0。
        /// </summary>
        [Test]
        public void SetCount_Zero_FiresOnCountChangedWithZero()
        {
            _root.SetCount(5);

            int receivedCount = -1;
            _root.OnCountChanged += (c) => receivedCount = c;

            _root.SetCount(0);

            Assert.AreEqual(0, receivedCount);
        }

        /// <summary>
        /// 子节点 SetCount 后父节点 Count 应聚合所有子节点的值。
        /// </summary>
        [Test]
        public void Parent_AggregatesChildCount()
        {
            RedDotNode child = new RedDotNode("child");
            _root.AddChild(child);

            child.SetCount(5);

            Assert.AreEqual(5, _root.Count);
        }

        /// <summary>
        /// 多个子节点的 Count 应正确累加到父节点。
        /// </summary>
        [Test]
        public void Parent_AggregatesMultipleChildren()
        {
            RedDotNode child1 = new RedDotNode("child1");
            RedDotNode child2 = new RedDotNode("child2");
            _root.AddChild(child1);
            _root.AddChild(child2);

            child1.SetCount(3);
            child2.SetCount(7);

            Assert.AreEqual(10, _root.Count);
        }

        /// <summary>
        /// 两个同父节点的子节点互相独立，修改一个不影响另一个的 Count。
        /// </summary>
        [Test]
        public void Siblings_AreIndependent()
        {
            RedDotNode child1 = new RedDotNode("child1");
            RedDotNode child2 = new RedDotNode("child2");
            _root.AddChild(child1);
            _root.AddChild(child2);

            child1.SetCount(3);

            Assert.AreEqual(0, child2.Count);
        }

        /// <summary>
        /// 移除子节点后父节点不再聚合该子节点的 Count。
        /// </summary>
        [Test]
        public void RemoveChild_StopsChildAggregation()
        {
            RedDotNode child = new RedDotNode("child");
            _root.AddChild(child);
            child.SetCount(5);

            _root.RemoveChild(child);

            Assert.AreEqual(0, _root.Count);
        }

        /// <summary>
        /// RemoveChild 返回 true 表示成功移除。
        /// </summary>
        [Test]
        public void RemoveChild_ReturnsTrue_WhenChildExists()
        {
            RedDotNode child = new RedDotNode("child");
            _root.AddChild(child);

            bool result = _root.RemoveChild(child);

            Assert.IsTrue(result);
        }

        /// <summary>
        /// RemoveChild 对不存在的子节点返回 false。
        /// </summary>
        [Test]
        public void RemoveChild_ReturnsFalse_WhenChildNotExists()
        {
            RedDotNode child = new RedDotNode("child");

            bool result = _root.RemoveChild(child);

            Assert.IsFalse(result);
        }

        /// <summary>
        /// AddChild 为 null 时抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void AddChild_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _root.AddChild(null!));
        }

        /// <summary>
        /// RemoveChild 为 null 时抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void RemoveChild_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _root.RemoveChild(null!));
        }

        /// <summary>
        /// Key 属性返回构造时传入的键值。
        /// </summary>
        [Test]
        public void Key_ReturnsConstructorValue()
        {
            RedDotNode node = new RedDotNode("inbox");

            Assert.AreEqual("inbox", node.Key);
        }

        /// <summary>
        /// Children 集合初始为空。
        /// </summary>
        [Test]
        public void Children_InitiallyEmpty()
        {
            Assert.AreEqual(0, _root.Children.Count);
        }

        /// <summary>
        /// Parent 属性在 AddChild 后指向父节点。
        /// </summary>
        [Test]
        public void AddChild_SetsChildParent()
        {
            RedDotNode child = new RedDotNode("child");
            _root.AddChild(child);

            Assert.AreSame(_root, child.Parent);
        }

        /// <summary>
        /// 多层嵌套子节点聚合：子节点的子节点 Count 变化也应传播到根节点。
        /// </summary>
        [Test]
        public void Grandchild_AggregatesToRoot()
        {
            RedDotNode child = new RedDotNode("child");
            RedDotNode grandchild = new RedDotNode("grandchild");
            _root.AddChild(child);
            child.AddChild(grandchild);

            grandchild.SetCount(5);

            Assert.AreEqual(5, child.Count);
            Assert.AreEqual(5, _root.Count);
        }

        /// <summary>
        /// 父节点自身也有直接 Count 时，聚合结果 = 自身 Count + 所有子节点 Count。
        /// </summary>
        [Test]
        public void Parent_OwnCountPlusChildrenCount()
        {
            RedDotNode child = new RedDotNode("child");
            _root.AddChild(child);
            _root.SetCount(3);
            child.SetCount(5);

            Assert.AreEqual(8, _root.Count);
        }
    }
}
