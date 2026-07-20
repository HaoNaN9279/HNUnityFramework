#nullable enable

using NUnit.Framework;
using UnityEngine;
using HN.Framework.Core.Level.Logic.Quest;
using HN.Framework.Unity.Level.View.Quest;
using HN.Framework.Core.Capability.Event;

namespace HN.Framework.Unity.Tests.Level.View.Quest
{
    [TestFixture]
    public class QuestManagerBridgeTests
    {
        private GameObject? _go;
        private QuestManagerBridge? _bridge;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("QuestManagerBridgeTest")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _bridge = _go.AddComponent<QuestManagerBridge>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
            _bridge = null;
        }

        [Test]
        public void Initialize_SetsEntityId()
        {
            Assert.That(_bridge, Is.Not.Null);
            _bridge!.Initialize(42u);
            Assert.That(_bridge.EntityId, Is.EqualTo(42u));
        }

        [Test]
        public void GetQuestManager_InitialNull()
        {
            Assert.That(_bridge, Is.Not.Null);
            Assert.That(_bridge!.GetQuestManager(), Is.Null);
        }

        [Test]
        public void SetQuestManager_ReturnsInjectedValue()
        {
            var bridge = _bridge!;
            bridge.Initialize(1u);

            var eventBus = new MockEventBus();
            var qm = new QuestManager<uint>(eventBus, null, null);

            bridge.SetQuestManager(qm);
            var retrieved = bridge.GetQuestManager();

            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved, Is.SameAs(qm));
        }

        private sealed class MockEventBus : IEventBus
        {
            public void Subscribe<T>(System.Action<T> handler) { }
            public void Unsubscribe<T>(System.Action<T> handler) { }
            public void Publish<T>(T eventData) { }
            public bool HasSubscribers<T>() => false;
        }
    }
}
