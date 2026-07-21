using HN.Framework.Unity.Level.Logic.AI;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Level.Logic.AI
{
    [TestFixture]
    public class SensorTests
    {
        private GameObject m_GameObject;

        [SetUp]
        public void SetUp()
        {
            m_GameObject = new GameObject("TestSensor");
            m_GameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        [TearDown]
        public void TearDown()
        {
            if (m_GameObject != null)
            {
                Object.DestroyImmediate(m_GameObject);
            }
        }

        [Test]
        public void VisionSensor_Added_ImplementsIPerceptionSensor()
        {
            var sensor = m_GameObject.AddComponent<VisionSensor>();
            Assert.IsNotNull(sensor as IPerceptionSensor);
        }

        [Test]
        public void VisionSensor_DefaultValues_AreReasonable()
        {
            var sensor = m_GameObject.AddComponent<VisionSensor>();
            Assert.AreEqual("Vision", sensor.SensorName);
            Assert.IsTrue(sensor.DetectionRange > 0f);
            Assert.IsTrue(sensor.IsEnabled);
        }

        [Test]
        public void VisionSensor_DetectionRange_CanBeSet()
        {
            var sensor = m_GameObject.AddComponent<VisionSensor>();
            sensor.DetectionRange = 50f;
            Assert.AreEqual(50f, sensor.DetectionRange);
        }

        [Test]
        public void VisionSensor_IsEnabled_CanBeToggled()
        {
            var sensor = m_GameObject.AddComponent<VisionSensor>();
            sensor.IsEnabled = false;
            Assert.IsFalse(sensor.IsEnabled);
        }

        [Test]
        public void AudioSensor_Added_ImplementsIPerceptionSensor()
        {
            var sensor = m_GameObject.AddComponent<AudioSensor>();
            Assert.IsNotNull(sensor as IPerceptionSensor);
        }

        [Test]
        public void AudioSensor_DefaultValues_AreReasonable()
        {
            var sensor = m_GameObject.AddComponent<AudioSensor>();
            Assert.AreEqual("Audio", sensor.SensorName);
            Assert.IsTrue(sensor.DetectionRange > 0f);
            Assert.IsTrue(sensor.IsEnabled);
        }

        [Test]
        public void AudioSensor_IsEnabled_CanBeToggled()
        {
            var sensor = m_GameObject.AddComponent<AudioSensor>();
            sensor.IsEnabled = false;
            Assert.IsFalse(sensor.IsEnabled);
        }
    }
}
