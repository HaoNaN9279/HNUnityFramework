#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Network;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Tests.Capability.Network
{
    /// <summary>
    /// LockstepManager 的单元测试。验证帧率驱动、输入缓冲、校验和及事件生命周期。
    /// </summary>
    [TestFixture]
    public class LockstepManagerTests
    {
        #region Construction

        [Test]
        public void Constructor_DefaultValues_InitializedCorrectly()
        {
            var manager = new LockstepManager();
            Assert.That(manager.FrameRate, Is.EqualTo(15));
            Assert.That(manager.BufferSize, Is.EqualTo(3));
            Assert.That(manager.CurrentFrame, Is.EqualTo(0UL));
            Assert.That(manager.IsEnabled, Is.True);
        }

        [Test]
        public void Constructor_CustomValues_Applied()
        {
            var manager = new LockstepManager(30, 5);
            Assert.That(manager.FrameRate, Is.EqualTo(30));
            Assert.That(manager.BufferSize, Is.EqualTo(5));
        }

        [Test]
        public void Constructor_ZeroFrameRate_Throws()
        {
            Assert.That(() => new LockstepManager(0), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_ZeroBufferSize_Throws()
        {
            Assert.That(() => new LockstepManager(15, 0), Throws.ArgumentException);
        }

        #endregion

        #region Tick - Frame Rate

        [Test]
        public void Tick_SingleFrame_AdvancesFrameCount()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(15); // 1/15 = 0.0666... sec per frame
            HNLogicTime.DeltaTime = 0.0666667; // ~1 frame

            manager.Tick();

            Assert.That(manager.CurrentFrame, Is.EqualTo(1UL));
        }

        [Test]
        public void Tick_AccumulatedTwoFrames_AdvancesTwoFrames()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(15);
            HNLogicTime.DeltaTime = 0.1333334; // ~2 frames

            manager.Tick();

            Assert.That(manager.CurrentFrame, Is.EqualTo(2UL));
        }

        [Test]
        public void Tick_MultipleTicks_AccumulatesCorrectly()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(30); // 1/30 sec per frame
            HNLogicTime.DeltaTime = 1.0 / 30.0;

            manager.Tick(); // frame 1
            manager.Tick(); // frame 2
            manager.Tick(); // frame 3

            Assert.That(manager.CurrentFrame, Is.EqualTo(3UL));
        }

        [Test]
        public void Tick_NotEnoughTime_DoesNotAdvance()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(15);
            HNLogicTime.DeltaTime = 0.01; // much less than frame interval (0.0666)

            manager.Tick();

            Assert.That(manager.CurrentFrame, Is.EqualTo(0UL));
        }

        #endregion

        #region Tick - HNLogicTime Integration

        [Test]
        public void Tick_UpdatesHNLogicTime()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(15);
            HNLogicTime.DeltaTime = 0.0666667;

            manager.Tick();

            Assert.That(HNLogicTime.LogicFrameCount, Is.EqualTo(1UL));
            Assert.That(HNLogicTime.DeltaTime, Is.EqualTo(1.0 / 15.0).Within(0.0001));
        }

        #endregion

        #region Tick - CatchUp Protection

        [Test]
        public void Tick_ExcessiveDelta_CappedByCatchUpLimit()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(15, 3); // buffer=3, maxCatchUp=9
            HNLogicTime.DeltaTime = 10.0; // huge delta, would be ~150 frames

            manager.Tick();

            // Should be capped at maxCatchUpFrames = 9
            Assert.That(manager.CurrentFrame, Is.LessThanOrEqualTo(9UL));
        }

        #endregion

        #region Tick - IsEnabled

        [Test]
        public void Tick_Disabled_DoesNotAdvance()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(15);
            manager.IsEnabled = false;
            HNLogicTime.DeltaTime = 0.0666667;

            manager.Tick();

            Assert.That(manager.CurrentFrame, Is.EqualTo(0UL));
        }

        [Test]
        public void Tick_ReEnabled_ResumesAdvancing()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(15);
            manager.IsEnabled = false;
            HNLogicTime.DeltaTime = 0.0666667;
            manager.Tick();
            Assert.That(manager.CurrentFrame, Is.EqualTo(0UL));

            manager.IsEnabled = true;
            manager.Tick();
            Assert.That(manager.CurrentFrame, Is.EqualTo(1UL));
        }

        #endregion

        #region OnFrameStart Event

        [Test]
        public void OnFrameStart_FrameAdvanced_EventFired()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(15);
            HNLogicTime.DeltaTime = 0.0666667;

            ulong firedFrame = 0;
            manager.OnFrameStart += (frame) => firedFrame = frame;

            manager.Tick();

            Assert.That(firedFrame, Is.EqualTo(1UL));
        }

        [Test]
        public void OnFrameStart_MultipleFrames_FiredForEachFrame()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(30); // 1/30 interval
            HNLogicTime.DeltaTime = 0.1; // ~3 frames

            int fireCount = 0;
            manager.OnFrameStart += (frame) => fireCount++;

            manager.Tick();

            Assert.That(fireCount, Is.EqualTo(3));
        }

        [Test]
        public void OnFrameStart_Disabled_EventNotFired()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(15);
            manager.IsEnabled = false;
            HNLogicTime.DeltaTime = 0.0666667;

            int fireCount = 0;
            manager.OnFrameStart += (frame) => fireCount++;

            manager.Tick();

            Assert.That(fireCount, Is.EqualTo(0));
        }

        #endregion

        #region SubmitInput / TryGetInput

        [Test]
        public void SubmitInput_And_TryGetInput_Roundtrip()
        {
            var manager = new LockstepManager(15);
            var input = new FrameInput(1);
            input.AddAction("Horizontal", FixedMathSharp.Fixed64.FromDouble(0.5));

            manager.SubmitInput(0, input);
            Assert.That(manager.TryGetInput(1, out var retrieved), Is.True);
            Assert.That(retrieved.FrameNumber, Is.EqualTo(1UL));
        }

        [Test]
        public void TryGetInput_NonExistingFrame_ReturnsFalse()
        {
            var manager = new LockstepManager(15);
            Assert.That(manager.TryGetInput(99, out _), Is.False);
        }

        #endregion

        #region Checksum

        [Test]
        public void RegisterChecksum_And_GetChecksum_Roundtrip()
        {
            var manager = new LockstepManager(15);
            manager.RegisterChecksum(1, 0xABCD1234UL);

            Assert.That(manager.GetChecksum(1), Is.EqualTo(0xABCD1234UL));
        }

        [Test]
        public void GetChecksum_NotRegistered_ReturnsZero()
        {
            var manager = new LockstepManager(15);
            Assert.That(manager.GetChecksum(42), Is.EqualTo(0UL));
        }

        #endregion

        #region Reset

        [Test]
        public void Reset_ClearsAllState()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(15);
            HNLogicTime.DeltaTime = 0.0666667;

            manager.Tick(); // frame 1
            manager.SubmitInput(0, new FrameInput(1));
            manager.RegisterChecksum(1, 0xFF);

            manager.Reset();

            Assert.That(manager.CurrentFrame, Is.EqualTo(0UL));
            Assert.That(manager.TryGetInput(1, out _), Is.False);
            Assert.That(manager.GetChecksum(1), Is.EqualTo(0UL));
        }

        #endregion
    }
}