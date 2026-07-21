#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.AI
{
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;
    using NUnit.Framework;

    /// <summary>
    /// PerceptionState 单元测试 — 验证多通道感知数据的添加、查询和清理功能。
    /// </summary>
    [TestFixture]
    public class PerceptionStateTests
    {
        private PerceptionState _perception = null!;

        [SetUp]
        public void SetUp()
        {
            _perception = new PerceptionState();
            _perception.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _perception.Clear();
        }

        /// <summary>
        /// 添加可见目标后，VisibleTargets 列表中应包含该目标。
        /// </summary>
        [Test]
        public void AddVisibleTarget_AddsToVisibleTargets()
        {
            var pos = new Vector3Data(10f, 0f, 5f);
            _perception.AddVisibleTarget(entityId: 1, confidence: 0.9f, distance: 15f, position: pos);

            Assert.That(_perception.VisibleTargets.Count, Is.EqualTo(1));
            Assert.That(_perception.VisibleTargets[0].EntityId, Is.EqualTo(1));
            Assert.That(_perception.VisibleTargets[0].Confidence, Is.EqualTo(0.9f));
            Assert.That(_perception.VisibleTargets[0].Distance, Is.EqualTo(15f));
            Assert.That(_perception.VisibleTargets[0].Position, Is.SameAs(pos));
        }

        /// <summary>
        /// 添加声音目标后，AudioTargets 列表中应包含该目标。
        /// </summary>
        [Test]
        public void AddAudioTarget_AddsToAudioTargets()
        {
            var pos = new Vector3Data(0f, 1f, 20f);
            _perception.AddAudioTarget(entityId: 2, confidence: 0.7f, distance: 25f, position: pos, audioType: AudioType.Gunshot);

            Assert.That(_perception.AudioTargets.Count, Is.EqualTo(1));
            Assert.That(_perception.AudioTargets[0].EntityId, Is.EqualTo(2));
            Assert.That(_perception.AudioTargets[0].AudioType, Is.EqualTo(AudioType.Gunshot));
        }

        /// <summary>
        /// 添加威胁目标后，ThreatTargets 列表中应包含该目标。
        /// </summary>
        [Test]
        public void AddThreatTarget_AddsToThreatTargets()
        {
            var pos = new Vector3Data(5f, 0f, 3f);
            _perception.AddThreatTarget(entityId: 3, threatLevel: 0.85f, distance: 8f, position: pos);

            Assert.That(_perception.ThreatTargets.Count, Is.EqualTo(1));
            Assert.That(_perception.ThreatTargets[0].EntityId, Is.EqualTo(3));
            Assert.That(_perception.ThreatTargets[0].Confidence, Is.EqualTo(0.85f));
            Assert.That(_perception.ThreatTargets[0].Distance, Is.EqualTo(8f));
        }

        /// <summary>
        /// ClearFrame 应清空所有通道的数据，但保留列表实例（不释放）。
        /// </summary>
        [Test]
        public void ClearFrame_ClearsAllPerceptionData()
        {
            _perception.AddVisibleTarget(1, 0.9f, 10f, new Vector3Data(1f, 0f, 0f));
            _perception.AddAudioTarget(2, 0.7f, 20f, new Vector3Data(0f, 0f, 10f), AudioType.Footstep);
            _perception.AddThreatTarget(3, 0.8f, 5f, new Vector3Data(0f, 0f, 0f));

            _perception.ClearFrame();

            Assert.That(_perception.VisibleTargets.Count, Is.EqualTo(0));
            Assert.That(_perception.AudioTargets.Count, Is.EqualTo(0));
            Assert.That(_perception.ThreatTargets.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 添加可见目标后，HasVisibleTargets 应返回 true。
        /// </summary>
        [Test]
        public void HasVisibleTargets_WithTargets_ReturnsTrue()
        {
            Assert.That(_perception.HasVisibleTargets, Is.False);

            _perception.AddVisibleTarget(1, 0.9f, 10f, new Vector3Data(0f, 0f, 0f));

            Assert.That(_perception.HasVisibleTargets, Is.True);
        }

        /// <summary>
        /// ClosestVisibleDistance 应返回所有可见目标中的最小距离。
        /// </summary>
        [Test]
        public void ClosestVisibleDistance_ReturnsMinimum()
        {
            Assert.That(_perception.ClosestVisibleDistance, Is.EqualTo(float.MaxValue), "No targets should return MaxValue.");

            _perception.AddVisibleTarget(1, 0.9f, 30f, new Vector3Data(1f, 0f, 0f));
            _perception.AddVisibleTarget(2, 0.8f, 10f, new Vector3Data(2f, 0f, 0f));
            _perception.AddVisibleTarget(3, 0.7f, 20f, new Vector3Data(3f, 0f, 0f));

            Assert.That(_perception.ClosestVisibleDistance, Is.EqualTo(10f));
        }
    }
}
