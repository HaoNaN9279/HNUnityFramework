#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Network;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Tests.Driver.Common
{
    /// <summary>
    /// HNLogicTime 的单元测试。验证 SetFrameDelta 注入、Initialize 重置及与 LockstepManager 集成。
    /// </summary>
    [TestFixture]
    public class HNLogicTimeTests
    {
        #region SetFrameDelta

        [Test]
        public void SetFrameDelta_UpdatesDeltaTime()
        {
            HNLogicTime.Initialize();
            HNLogicTime.SetFrameDelta(0.5);

            Assert.That(HNLogicTime.DeltaTime, Is.EqualTo(0.5));

            HNLogicTime.SetFrameDelta(0.016);

            Assert.That(HNLogicTime.DeltaTime, Is.EqualTo(0.016));
        }

        #endregion

        #region Initialize

        [Test]
        public void Initialize_ResetsAllValues()
        {
            HNLogicTime.SetFrameDelta(0.5);
            // 通过 internal set 手动修改 Time 和 LogicFrameCount
            HNLogicTime.Time = 10.0;
            HNLogicTime.DeltaTime = 0.2;
            HNLogicTime.LogicFrameCount = 42;

            HNLogicTime.Initialize();

            Assert.That(HNLogicTime.Time, Is.EqualTo(0.0));
            Assert.That(HNLogicTime.DeltaTime, Is.EqualTo(0.0));
            Assert.That(HNLogicTime.LogicFrameCount, Is.EqualTo(0UL));
        }

        #endregion

        #region SetFrameDelta + LockstepManager Integration

        /// <summary>
        /// 验证 SetFrameDelta 驱动 LockstepManager.Tick 推进逻辑帧的完整语义。
        /// GameWorldDriver.Update 调用 SetFrameDelta(Time.deltaTime) 后，
        /// LockstepManager.Tick 累加实际耗时，帧推进时将 DeltaTime 写为固定帧间隔。
        /// </summary>
        [Test]
        public void SetFrameDelta_WithLockstep_AdvancesFrames()
        {
            HNLogicTime.Initialize();
            var manager = new LockstepManager(15);
            HNLogicTime.SetFrameDelta(0.0666667);

            manager.Tick();

            Assert.That(manager.CurrentFrame, Is.EqualTo(1UL));
            Assert.That(HNLogicTime.LogicFrameCount, Is.EqualTo(1UL));
            Assert.That(HNLogicTime.DeltaTime, Is.EqualTo(1.0 / 15.0).Within(0.0001));
        }

        #endregion
    }
}
