using HN.Framework.Core.Level.Logic.AI;
using HN.Framework.Unity.Level.Logic.AI;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Level.Logic.AI
{
    [TestFixture]
    public class ActionExecutorTests
    {
        private GameObject m_GameObject;
        private DefaultActionExecutor m_Executor;

        [SetUp]
        public void SetUp()
        {
            m_GameObject = new GameObject("TestExecutor");
            m_GameObject.hideFlags = HideFlags.HideAndDontSave;
            m_Executor = m_GameObject.AddComponent<DefaultActionExecutor>();
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
        public void Awake_InitializesAdapters()
        {
            Assert.IsNotNull(m_Executor);
        }

        [Test]
        public void EnqueueAction_AddsToQueue()
        {
            var action = new TestAction();
            m_Executor.EnqueueAction(action);
            Assert.AreEqual(1, m_Executor.PendingCount);
        }

        [Test]
        public void EnqueueActions_BatchAddsToQueue()
        {
            var actions = new System.Collections.Generic.List<IActionCommand>
            {
                new TestAction(),
                new TestAction(),
                new TestAction()
            };
            m_Executor.EnqueueActions(actions);
            Assert.AreEqual(3, m_Executor.PendingCount);
        }

        [Test]
        public void ClearQueue_EmptiesPendingActions()
        {
            m_Executor.EnqueueAction(new TestAction());
            m_Executor.EnqueueAction(new TestAction());
            m_Executor.ClearQueue();
            Assert.AreEqual(0, m_Executor.PendingCount);
            Assert.IsFalse(m_Executor.IsExecuting);
        }

        [Test]
        public void EnqueueAction_NullAction_DoesNothing()
        {
            m_Executor.EnqueueAction(null);
            Assert.AreEqual(0, m_Executor.PendingCount);
        }

        [Test]
        public void EnqueueActions_NullList_DoesNothing()
        {
            m_Executor.EnqueueActions(null);
            Assert.AreEqual(0, m_Executor.PendingCount);
        }

        [Test]
        public void RegisterHandler_WithHandler_DoesNotThrow()
        {
            m_Executor.RegisterHandler<TestAction>(cmd => true);
            Assert.IsNotNull(m_Executor);
        }

        [Test]
        public void RegisterHandler_NullHandler_DoesNotThrow()
        {
            m_Executor.RegisterHandler<TestAction>(null);
            Assert.AreEqual(0, m_Executor.PendingCount);
        }

        private sealed class TestAction : IActionCommand
        {
            public string TypeName { get; set; } = "TestAction";
            public int Priority { get; set; }
            public ActionCommandStatus Status { get; set; }
            public void Initialize() { Status = ActionCommandStatus.Pending; }
            public void Execute() { Status = ActionCommandStatus.Running; }
            public void Cancel() { Status = ActionCommandStatus.Cancelled; }
            public void Clear() { TypeName = "TestAction"; Priority = 0; Status = ActionCommandStatus.Pending; }
        }
    }
}
