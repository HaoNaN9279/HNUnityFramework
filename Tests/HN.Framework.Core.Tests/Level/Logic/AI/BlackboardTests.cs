#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.AI
{
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;
    using NUnit.Framework;

    /// <summary>
    /// Blackboard 单元测试 — 验证 Agent 内部上下文数据共享的核心功能。
    /// </summary>
    [TestFixture]
    public class BlackboardTests
    {
        private Blackboard _blackboard = null!;

        [SetUp]
        public void SetUp()
        {
            _blackboard = new Blackboard();
            _blackboard.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _blackboard.Clear();
        }

        /// <summary>
        /// 设置字符串值后，通过 Get 可以正确获取。
        /// </summary>
        [Test]
        public void SetAndGet_StringValue_ReturnsCorrectValue()
        {
            _blackboard.Set("name", "hero");
            var result = _blackboard.Get<string>("name");

            Assert.That(result, Is.EqualTo("hero"));
        }

        /// <summary>
        /// 设置 int 值后，通过 Get 可以正确获取。
        /// </summary>
        [Test]
        public void SetAndGet_IntValue_ReturnsCorrectValue()
        {
            _blackboard.Set("hp", 100);
            var result = _blackboard.Get<int>("hp");

            Assert.That(result, Is.EqualTo(100));
        }

        /// <summary>
        /// 获取不存在的键时，应返回类型的 default 值。
        /// </summary>
        [Test]
        public void Get_NonExistentKey_ReturnsDefault()
        {
            var result = _blackboard.Get<int>("missing");

            Assert.That(result, Is.EqualTo(0));
        }

        /// <summary>
        /// TryGet 在键存在时应返回 true 并输出正确值。
        /// </summary>
        [Test]
        public void TryGet_ExistingKey_ReturnsTrueAndValue()
        {
            _blackboard.Set("score", 42);

            var success = _blackboard.TryGet("score", out int value);

            Assert.That(success, Is.True);
            Assert.That(value, Is.EqualTo(42));
        }

        /// <summary>
        /// TryGet 在键不存在时应返回 false。
        /// </summary>
        [Test]
        public void TryGet_NonExistentKey_ReturnsFalse()
        {
            var success = _blackboard.TryGet("missing", out int _);

            Assert.That(success, Is.False);
        }

        /// <summary>
        /// HasKey 在键存在时应返回 true。
        /// </summary>
        [Test]
        public void HasKey_ExistingKey_ReturnsTrue()
        {
            _blackboard.Set("flag", true);

            Assert.That(_blackboard.HasKey("flag"), Is.True);
        }

        /// <summary>
        /// 移除存在的键后，该键不再存在且 HasKey 返回 false。
        /// </summary>
        [Test]
        public void Remove_ExistingKey_RemovesSuccessfully()
        {
            _blackboard.Set("temp", 1);

            var removed = _blackboard.Remove("temp");

            Assert.That(removed, Is.True);
            Assert.That(_blackboard.HasKey("temp"), Is.False);
        }

        /// <summary>
        /// ClearAll 清空所有已设置的数据。
        /// </summary>
        [Test]
        public void ClearAll_ClearsAllData()
        {
            _blackboard.Set("a", 1);
            _blackboard.Set("b", 2);
            _blackboard.Set("c", 3);

            _blackboard.ClearAll();

            Assert.That(_blackboard.Count, Is.EqualTo(0));
            Assert.That(_blackboard.Get<int>("a"), Is.EqualTo(0));
        }

        /// <summary>
        /// Clear 释放内部字典后，重新 Initialize 仍然可用。
        /// </summary>
        [Test]
        public void Clear_ReleasesDictionary_CanReinitialize()
        {
            _blackboard.Set("key", "value");
            _blackboard.Clear();

            // 重新初始化后应可正常使用
            _blackboard.Initialize();
            _blackboard.Set("key2", 99);

            Assert.That(_blackboard.Get<int>("key2"), Is.EqualTo(99));
            Assert.That(_blackboard.Count, Is.EqualTo(1));

            // 需要手动 Clear 以防止 TearDown 重复释放
            _blackboard.Clear();
        }
    }
}
