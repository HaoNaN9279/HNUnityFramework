#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Network;

namespace HN.Framework.Core.Tests.Capability.Network
{
    /// <summary>
    /// FrameInputBuffer 的单元测试。验证环形缓冲的核心行为。
    /// </summary>
    [TestFixture]
    public class FrameInputBufferTests
    {
        #region Construction

        [Test]
        public void Constructor_ValidCapacity_CreatesBuffer()
        {
            var buffer = new FrameInputBuffer(3);
            Assert.That(buffer.Capacity, Is.EqualTo(3));
            Assert.That(buffer.Count, Is.EqualTo(0));
            Assert.That(buffer.IsFull, Is.False);
        }

        [Test]
        public void Constructor_ZeroCapacity_Throws()
        {
            Assert.That(() => new FrameInputBuffer(0), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_NegativeCapacity_Throws()
        {
            Assert.That(() => new FrameInputBuffer(-1), Throws.ArgumentException);
        }

        #endregion

        #region Enqueue

        [Test]
        public void Enqueue_SingleItem_CanRetrieve()
        {
            var buffer = new FrameInputBuffer(3);
            var input = new FrameInput(1) { Actions = new() };
            buffer.Enqueue(input);

            Assert.That(buffer.Count, Is.EqualTo(1));
            Assert.That(buffer.TryGetLatest(out var latest), Is.True);
            Assert.That(latest.FrameNumber, Is.EqualTo(1UL));
        }

        [Test]
        public void Enqueue_MultipleItems_CountMatches()
        {
            var buffer = new FrameInputBuffer(3);
            buffer.Enqueue(new FrameInput(1));
            buffer.Enqueue(new FrameInput(2));
            buffer.Enqueue(new FrameInput(3));

            Assert.That(buffer.Count, Is.EqualTo(3));
            Assert.That(buffer.IsFull, Is.True);
        }

        [Test]
        public void Enqueue_Overflow_OverwritesOldest()
        {
            var buffer = new FrameInputBuffer(3);
            buffer.Enqueue(new FrameInput(1));
            buffer.Enqueue(new FrameInput(2));
            buffer.Enqueue(new FrameInput(3));
            buffer.Enqueue(new FrameInput(4)); // overflows, should overwrite frame 1

            Assert.That(buffer.Count, Is.EqualTo(3));
            Assert.That(buffer.IsFull, Is.True);
            Assert.That(buffer.TryGet(1, out _), Is.False, "Frame 1 should be overwritten");
            Assert.That(buffer.TryGet(4, out var frame4), Is.True, "Frame 4 should exist");
            Assert.That(frame4.FrameNumber, Is.EqualTo(4UL));
        }

        #endregion

        #region TryGet

        [Test]
        public void TryGet_ExistingFrame_ReturnsTrue()
        {
            var buffer = new FrameInputBuffer(3);
            buffer.Enqueue(new FrameInput(10));
            buffer.Enqueue(new FrameInput(20));

            Assert.That(buffer.TryGet(10, out var result), Is.True);
            Assert.That(result.FrameNumber, Is.EqualTo(10UL));
            Assert.That(buffer.TryGet(20, out result), Is.True);
            Assert.That(result.FrameNumber, Is.EqualTo(20UL));
        }

        [Test]
        public void TryGet_NonExistingFrame_ReturnsFalse()
        {
            var buffer = new FrameInputBuffer(3);
            buffer.Enqueue(new FrameInput(1));

            Assert.That(buffer.TryGet(99, out _), Is.False);
        }

        [Test]
        public void TryGet_EmptyBuffer_ReturnsFalse()
        {
            var buffer = new FrameInputBuffer(3);
            Assert.That(buffer.TryGet(1, out _), Is.False);
        }

        [Test]
        public void TryGet_OverflowedFrame_ReturnsFalse()
        {
            var buffer = new FrameInputBuffer(2);
            buffer.Enqueue(new FrameInput(1));
            buffer.Enqueue(new FrameInput(2));
            buffer.Enqueue(new FrameInput(3)); // overwrites frame 1

            Assert.That(buffer.TryGet(1, out _), Is.False);
        }

        #endregion

        #region TryGetLatest

        [Test]
        public void TryGetLatest_NonEmpty_ReturnsLastEnqueued()
        {
            var buffer = new FrameInputBuffer(3);
            buffer.Enqueue(new FrameInput(1));
            buffer.Enqueue(new FrameInput(2));

            Assert.That(buffer.TryGetLatest(out var latest), Is.True);
            Assert.That(latest.FrameNumber, Is.EqualTo(2UL));
        }

        [Test]
        public void TryGetLatest_Empty_ReturnsFalse()
        {
            var buffer = new FrameInputBuffer(3);
            Assert.That(buffer.TryGetLatest(out _), Is.False);
        }

        #endregion

        #region ClearBefore

        [Test]
        public void ClearBefore_RemovesOlderFrames()
        {
            var buffer = new FrameInputBuffer(5);
            buffer.Enqueue(new FrameInput(1));
            buffer.Enqueue(new FrameInput(2));
            buffer.Enqueue(new FrameInput(3));
            buffer.Enqueue(new FrameInput(4));

            buffer.ClearBefore(2);

            Assert.That(buffer.TryGet(1, out _), Is.False);
            Assert.That(buffer.TryGet(2, out _), Is.False);
            Assert.That(buffer.TryGet(3, out var frame3), Is.True);
            Assert.That(frame3.FrameNumber, Is.EqualTo(3UL));
            Assert.That(buffer.TryGet(4, out _), Is.True);
        }

        [Test]
        public void ClearBefore_AllFrames_ClearsAll()
        {
            var buffer = new FrameInputBuffer(3);
            buffer.Enqueue(new FrameInput(1));
            buffer.Enqueue(new FrameInput(2));

            buffer.ClearBefore(100);

            Assert.That(buffer.Count, Is.EqualTo(0));
        }

        #endregion

        #region Clear

        [Test]
        public void Clear_AfterEnqueue_ResetsState()
        {
            var buffer = new FrameInputBuffer(3);
            buffer.Enqueue(new FrameInput(1));
            buffer.Enqueue(new FrameInput(2));
            buffer.Clear();

            Assert.That(buffer.Count, Is.EqualTo(0));
            Assert.That(buffer.IsFull, Is.False);
            Assert.That(buffer.TryGet(1, out _), Is.False);
        }

        #endregion
    }
}