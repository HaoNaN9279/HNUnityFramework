#nullable enable

using System;
using NUnit.Framework;
using HN.Framework.Core.Capability.UI;
using HN.Framework.Unity.Level.View.UI;

namespace HN.Framework.Unity.Tests.Level.View.UI
{
    /// <summary>
    /// RedDotManager 的 EditMode 单元测试，覆盖路径式注册、父子聚合、
    /// 订阅/取消订阅及生命周期管理。
    /// </summary>
    [TestFixture]
    public class RedDotManagerTests
    {
        private RedDotManager _manager = null!;

        [SetUp]
        public void SetUp()
        {
            _manager = new RedDotManager();
        }

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
            _manager = null!;
        }

        // ── Register ──

        /// <summary>
        /// Register("Mail") 返回非空节点且 Key == "Mail"。
        /// </summary>
        [Test]
        public void Register_SinglePath_ReturnsNode()
        {
            var node = _manager.Register("Mail");

            Assert.That(node, Is.Not.Null);
            Assert.That(node.Key, Is.EqualTo("Mail"));
        }

        /// <summary>
        /// Register("A/B/C") 应自动创建中间节点 A 和 A/B。
        /// </summary>
        [Test]
        public void Register_MultiLevelPath_CreatesParents()
        {
            _manager.Register("A/B/C");

            Assert.That(_manager.GetNode("A"), Is.Not.Null, "中间节点 A 应被创建");
            Assert.That(_manager.GetNode("A/B"), Is.Not.Null, "中间节点 A/B 应被创建");
            Assert.That(_manager.GetNode("A/B/C"), Is.Not.Null, "叶节点 A/B/C 应被创建");

            Assert.That(_manager.GetNode("A")!.Key, Is.EqualTo("A"));
            Assert.That(_manager.GetNode("A/B")!.Key, Is.EqualTo("B"));
            Assert.That(_manager.GetNode("A/B/C")!.Key, Is.EqualTo("C"));
        }

        /// <summary>
        /// 对同一路径重复 Register 返回同一对象。
        /// </summary>
        [Test]
        public void DupRegister_ReturnsExisting()
        {
            var first = _manager.Register("Mail");
            var second = _manager.Register("Mail");

            Assert.That(second, Is.SameAs(first));
        }

        // ── SetCount / GetCount ──

        /// <summary>
        /// Register 后 SetCount 再 GetCount 应返回相同值。
        /// </summary>
        [Test]
        public void SetCount_ThenGetCount_ReturnsValue()
        {
            _manager.Register("Mail");
            _manager.SetCount("Mail", 5);

            Assert.That(_manager.GetCount("Mail"), Is.EqualTo(5));
        }

        /// <summary>
        /// 子节点 SetCount 后父节点应聚合子节点的 Count。
        /// </summary>
        [Test]
        public void ParentChild_Aggregation()
        {
            _manager.Register("A/B");
            _manager.SetCount("A/B", 3);

            Assert.That(_manager.GetCount("A"), Is.EqualTo(3));
        }

        /// <summary>
        /// 多个子节点的 Count 应正确累加到父节点。
        /// </summary>
        [Test]
        public void MultipleChildren_Aggregation()
        {
            _manager.Register("A/B1");
            _manager.Register("A/B2");
            _manager.SetCount("A/B1", 2);
            _manager.SetCount("A/B2", 3);

            Assert.That(_manager.GetCount("A"), Is.EqualTo(5));
        }

        /// <summary>
        /// GetCount 对不存在的路径返回 0。
        /// </summary>
        [Test]
        public void GetCount_NonExistent_ReturnsZero()
        {
            Assert.That(_manager.GetCount("NonExistent"), Is.EqualTo(0));
        }

        // ── Unregister ──

        /// <summary>
        /// Unregister 后 GetNode 返回 null。
        /// </summary>
        [Test]
        public void Unregister_RemovesNode()
        {
            _manager.Register("A/B");
            _manager.Unregister("A/B");

            Assert.That(_manager.GetNode("A/B"), Is.Null);
        }

        /// <summary>
        /// 移除子节点后父节点不再聚合该子节点的 Count。
        /// </summary>
        [Test]
        public void Unregister_StopsAggregation()
        {
            _manager.Register("A/B");
            _manager.SetCount("A/B", 5);

            _manager.Unregister("A/B");

            Assert.That(_manager.GetCount("A"), Is.EqualTo(0));
        }

        /// <summary>
        /// Unregister 应移除目标节点及其所有子孙节点。
        /// </summary>
        [Test]
        public void Unregister_RemovesDescendants()
        {
            _manager.Register("A/B/C");
            _manager.Register("A/B/D");

            _manager.Unregister("A/B");

            Assert.That(_manager.GetNode("A/B"), Is.Null);
            Assert.That(_manager.GetNode("A/B/C"), Is.Null);
            Assert.That(_manager.GetNode("A/B/D"), Is.Null);
            // 中间节点 A 应保留（它是独立的注册项）
            Assert.That(_manager.GetNode("A"), Is.Not.Null);
        }

        /// <summary>
        /// Unregister 对不存在的路径返回 false。
        /// </summary>
        [Test]
        public void Unregister_NonExistent_ReturnsFalse()
        {
            Assert.That(_manager.Unregister("NonExistent"), Is.False);
        }

        // ── Subscribe / Unsubscribe ──

        /// <summary>
        /// Subscribe 后 SetCount 触发回调并收到正确值。
        /// </summary>
        [Test]
        public void Subscribe_CallbackOnSetCount()
        {
            _manager.Register("A");

            int received = -1;
            _manager.Subscribe("A", c => received = c);

            _manager.SetCount("A", 5);

            Assert.That(received, Is.EqualTo(5));
        }

        /// <summary>
        /// Unsubscribe 后 SetCount 不再触发回调。
        /// </summary>
        [Test]
        public void Unsubscribe_CallbackNotTriggered()
        {
            _manager.Register("A");

            int received = -1;
            Action<int> callback = c => received = c;
            _manager.Subscribe("A", callback);
            _manager.Unsubscribe("A", callback);

            _manager.SetCount("A", 5);

            Assert.That(received, Is.EqualTo(-1));
        }

        /// <summary>
        /// 父节点订阅后，子节点 SetCount 通过聚合触发父节点回调。
        /// </summary>
        [Test]
        public void Subscribe_ParentTriggeredByChild()
        {
            _manager.Register("A/B");

            int received = -1;
            _manager.Subscribe("A", c => received = c);

            _manager.SetCount("A/B", 3);

            Assert.That(received, Is.EqualTo(3));
        }

        /// <summary>
        /// Unregister 后自动取消该路径上的所有订阅。
        /// </summary>
        [Test]
        public void Unregister_RemovesSubscriptions()
        {
            _manager.Register("A");

            int received = -1;
            _manager.Subscribe("A", c => received = c);

            _manager.Unregister("A");
            // 重新注册新节点（旧回调已随 Unregister 清理）
            _manager.Register("A");
            _manager.SetCount("A", 5);

            Assert.That(received, Is.EqualTo(-1));
        }

        // ── Clear ──

        /// <summary>
        /// Clear() 应移除所有注册的节点。
        /// </summary>
        [Test]
        public void Clear_RemovesAll()
        {
            _manager.Register("A");
            _manager.Register("B/C");
            _manager.Register("B/D");

            _manager.Clear();

            Assert.That(_manager.GetNode("A"), Is.Null);
            Assert.That(_manager.GetNode("B"), Is.Null);
            Assert.That(_manager.GetNode("B/C"), Is.Null);
            Assert.That(_manager.GetNode("B/D"), Is.Null);
        }

        /// <summary>
        /// Clear() 后 GetCount 返回 0。
        /// </summary>
        [Test]
        public void Clear_ResetsCount()
        {
            _manager.Register("Mail");
            _manager.SetCount("Mail", 5);

            _manager.Clear();

            Assert.That(_manager.GetCount("Mail"), Is.EqualTo(0));
        }

        /// <summary>
        /// Clear() 应取消所有订阅。
        /// </summary>
        [Test]
        public void Clear_RemovesSubscriptions()
        {
            _manager.Register("A");

            int received = -1;
            _manager.Subscribe("A", c => received = c);

            _manager.Clear();
            // Clear 后重新注册，旧回调不应被触发
            _manager.Register("A");
            _manager.SetCount("A", 5);

            Assert.That(received, Is.EqualTo(-1));
        }

        // ── Dispose ──

        /// <summary>
        /// Dispose() 后 GetNode 返回 null。
        /// </summary>
        [Test]
        public void Dispose_ClearsAllNodes()
        {
            _manager.Register("A/B");

            _manager.Dispose();

            Assert.That(_manager.GetNode("A"), Is.Null);
            Assert.That(_manager.GetNode("A/B"), Is.Null);
        }

        // ── 边缘场景 ──

        /// <summary>
        /// 对包含多余斜杠的路径 Register 应正确解析。
        /// </summary>
        [Test]
        public void Register_ExtraSlashes_Handled()
        {
            var node = _manager.Register("/A//B/");

            Assert.That(node, Is.Not.Null);
            Assert.That(node.Key, Is.EqualTo("B"));
            Assert.That(_manager.GetNode("A"), Is.Not.Null);
            Assert.That(_manager.GetNode("A/B"), Is.Not.Null);
        }

        /// <summary>
        /// 空路径或纯斜杠路径 Register 抛出 ArgumentException（或 ArgumentNullException）。
        /// SplitPath 会过滤空段，无有效段时抛出异常。
        /// </summary>
        [Test]
        public void Register_EmptyPath_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.Register(null!));
            Assert.Throws<ArgumentException>(() => _manager.Register(""));
            Assert.Throws<ArgumentException>(() => _manager.Register("///"));
        }

        /// <summary>
        /// 同时注册父子节点路径，父节点自身直接 Count + 子节点 Count 聚合。
        /// </summary>
        [Test]
        public void Parent_OwnDirectCountPlusChildren()
        {
            _manager.Register("A");
            _manager.Register("A/B");

            _manager.SetCount("A", 2);
            _manager.SetCount("A/B", 3);

            Assert.That(_manager.GetCount("A"), Is.EqualTo(5));
            Assert.That(_manager.GetCount("A/B"), Is.EqualTo(3));
        }
    }
}
