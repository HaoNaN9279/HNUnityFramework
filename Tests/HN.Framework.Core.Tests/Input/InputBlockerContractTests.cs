#nullable enable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using HN.Framework.Core.Capability.Input;

namespace HN.Framework.Core.Tests.Input
{
    /// <summary>
    /// IInputBlocker 接口的模拟实现，用于合约测试。
    /// 使用基于优先级的屏蔽模型：更高优先级屏蔽较低优先级，相同优先级后 Push 者生效。
    /// </summary>
    internal sealed class MockInputBlocker : IInputBlocker
    {
        private readonly Dictionary<object, int> _blockers = new();

        /// <summary>
        /// 推入一个输入屏蔽令牌。
        /// </summary>
        public void Push(object token, int priority)
        {
            _blockers[token] = priority;
        }

        /// <summary>
        /// 弹出指定的屏蔽令牌。若令牌不存在则为空操作。
        /// </summary>
        public void Pop(object token)
        {
            _blockers.Remove(token);
        }

        /// <summary>
        /// 检查指定优先级的输入动作是否被屏蔽。
        /// </summary>
        public bool IsBlocked(int actionPriority)
        {
            return _blockers.Values.Any() && _blockers.Values.Max() >= actionPriority;
        }

        /// <summary>
        /// 清除所有屏蔽令牌。
        /// </summary>
        public void Clear()
        {
            _blockers.Clear();
        }

        /// <summary>
        /// 获取当前最高屏蔽优先级。
        /// </summary>
        public int EffectivePriority => _blockers.Values.Any() ? _blockers.Values.Max() : 0;
    }

    /// <summary>
    /// IInputBlocker 接口合约测试。
    /// 验证优先级屏蔽契约：高优先级屏蔽低优先级，相同优先级后 Push 者生效。
    /// </summary>
    [TestFixture]
    public class InputBlockerContractTests
    {
        private MockInputBlocker _blocker = null!;
        private readonly object _tokenA = new();
        private readonly object _tokenB = new();

        /// <summary>
        /// 每个测试前创建新的 MockInputBlocker 实例。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _blocker = new MockInputBlocker();
        }

        /// <summary>
        /// 每个测试后清理资源。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _blocker = null!;
        }

        /// <summary>
        /// Push 较高优先级的 token 后，较低优先级的动作应被屏蔽。
        /// </summary>
        [Test]
        public void Push_WithPriority_BlocksLowerPriority()
        {
            _blocker.Push(_tokenA, 10);
            Assert.That(_blocker.IsBlocked(5), Is.True);
        }

        /// <summary>
        /// Push 较高优先级的 token 后，相同优先级的动作应被屏蔽。
        /// </summary>
        [Test]
        public void Push_WithPriority_BlocksEqualPriority()
        {
            _blocker.Push(_tokenA, 10);
            Assert.That(_blocker.IsBlocked(10), Is.True);
        }

        /// <summary>
        /// Push 较低优先级的 token 不应屏蔽更高优先级的动作。
        /// </summary>
        [Test]
        public void Push_LowerPriority_DoesNotBlockHigherPriority()
        {
            _blocker.Push(_tokenA, 5);
            Assert.That(_blocker.IsBlocked(10), Is.False);
        }

        /// <summary>
        /// Pop 移除 token 后，之前被屏蔽的低优先级动作应解除屏蔽。
        /// </summary>
        [Test]
        public void Pop_RemovesToken_UnblocksLowerPriority()
        {
            _blocker.Push(_tokenA, 10);
            _blocker.Pop(_tokenA);
            Assert.That(_blocker.IsBlocked(5), Is.False);
        }

        /// <summary>
        /// Pop 移除高优先级 token 后，剩余的低优先级 token 仍可屏蔽更低优先级。
        /// </summary>
        [Test]
        public void Pop_HigherPriority_RemainingTokenStillBlocks()
        {
            _blocker.Push(_tokenA, 10);
            _blocker.Push(_tokenB, 20);
            _blocker.Pop(_tokenB);
            Assert.That(_blocker.IsBlocked(5), Is.True);
            Assert.That(_blocker.IsBlocked(15), Is.False);
        }

        /// <summary>
        /// Pop 不存在的 token 应无异常。
        /// </summary>
        [Test]
        public void Pop_NonExistentToken_NoOp()
        {
            Assert.DoesNotThrow(() => _blocker.Pop(new object()));
            Assert.That(_blocker.IsBlocked(0), Is.False);
        }

        /// <summary>
        /// 相同优先级时，后 Push 的 token 应覆盖前一个（后来者生效）。
        /// </summary>
        [Test]
        public void Push_SamePriority_LaterWins()
        {
            _blocker.Push(_tokenA, 10);
            _blocker.Push(_tokenB, 10);
            _blocker.Pop(_tokenB);
            // After popping B, A (same priority 10) should still block
            Assert.That(_blocker.IsBlocked(5), Is.True);
        }

        /// <summary>
        /// 相同优先级时，后 Push 的 token 弹出后，前一个 token 仍生效。
        /// </summary>
        [Test]
        public void Push_SamePriority_PopLater_FormerStillBlocks()
        {
            _blocker.Push(_tokenA, 10);
            _blocker.Push(_tokenB, 10);
            _blocker.Pop(_tokenB);
            Assert.That(_blocker.IsBlocked(5), Is.True);
            _blocker.Pop(_tokenA);
            Assert.That(_blocker.IsBlocked(5), Is.False);
        }

        /// <summary>
        /// Clear 应移除所有屏蔽令牌。
        /// </summary>
        [Test]
        public void Clear_RemovesAllTokens()
        {
            _blocker.Push(_tokenA, 10);
            _blocker.Push(_tokenB, 20);
            _blocker.Clear();
            Assert.That(_blocker.IsBlocked(0), Is.False);
            Assert.That(_blocker.IsBlocked(100), Is.False);
        }

        /// <summary>
        /// 空的屏蔽器不应屏蔽任何优先级。
        /// </summary>
        [Test]
        public void IsBlocked_EmptyBlocker_ReturnsFalse()
        {
            Assert.That(_blocker.IsBlocked(0), Is.False);
            Assert.That(_blocker.IsBlocked(int.MaxValue), Is.False);
        }

        /// <summary>
        /// 推入相同 token 应更新其优先级（覆盖旧值）。
        /// </summary>
        [Test]
        public void Push_SameToken_UpdatesPriority()
        {
            _blocker.Push(_tokenA, 10);
            _blocker.Push(_tokenA, 5);
            // Now effective priority is 5, so priority 10 should NOT be blocked
            Assert.That(_blocker.IsBlocked(10), Is.False);
            Assert.That(_blocker.IsBlocked(5), Is.True);
        }
    }
}
