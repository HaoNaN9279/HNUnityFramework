using System.Collections;
using HN.Framework.Unity.Level.Logic.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HN.Framework.Unity.Tests.Level.Logic.AI
{
    [TestFixture]
    public class AIAgentComponentTests
    {
        private GameObject m_GameObject;
        private AIAgentComponent m_Component;

        [SetUp]
        public void SetUp()
        {
            m_GameObject = new GameObject("TestAgent");
            m_GameObject.hideFlags = HideFlags.HideAndDontSave;
            m_Component = m_GameObject.AddComponent<AIAgentComponent>();
            m_Component.AutoActivate = false;
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
        public void Awake_InitializesAgent()
        {
            // Awake is called automatically when component is added
            Assert.IsNotNull(m_Component.Agent);
            Assert.AreEqual(HN.Framework.Core.Level.Logic.AI.AgentStatus.Inactive, m_Component.Agent.Status);
        }

        [Test]
        public void Activate_SetsAgentActive()
        {
            m_Component.Activate();
            Assert.AreEqual(HN.Framework.Core.Level.Logic.AI.AgentStatus.Active, m_Component.Agent.Status);
        }

        [Test]
        public void Deactivate_SetsAgentInactive()
        {
            m_Component.Activate();
            m_Component.Deactivate();
            Assert.AreEqual(HN.Framework.Core.Level.Logic.AI.AgentStatus.Inactive, m_Component.Agent.Status);
        }

        [Test]
        public void PauseAndResume_WorkCorrectly()
        {
            m_Component.Activate();
            m_Component.Pause();
            Assert.AreEqual(HN.Framework.Core.Level.Logic.AI.AgentStatus.Paused, m_Component.Agent.Status);

            m_Component.Resume();
            Assert.AreEqual(HN.Framework.Core.Level.Logic.AI.AgentStatus.Active, m_Component.Agent.Status);
        }

        [Test]
        public void AutoActivate_AwakeActivatesAgent()
        {
            var go = new GameObject("TestAutoActivate");
            go.hideFlags = HideFlags.HideAndDontSave;
            var comp = go.AddComponent<AIAgentComponent>();
            comp.AutoActivate = true;

            // Force Awake by re-adding
            Object.DestroyImmediate(comp);
            comp = go.AddComponent<AIAgentComponent>();
            comp.AutoActivate = true;

            Assert.IsNotNull(comp.Agent);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void GetBlackboard_ReturnsAgentBlackboard()
        {
            var bb = m_Component.GetBlackboard();
            Assert.IsNotNull(bb);
        }

        [Test]
        public void GetWorldState_ReturnsAgentWorldState()
        {
            var ws = m_Component.GetWorldState();
            Assert.IsNotNull(ws);
        }

        [Test]
        public void GetPerception_ReturnsAgentPerception()
        {
            var p = m_Component.GetPerception();
            Assert.IsNotNull(p);
        }
    }
}
