using HN.Framework.Unity.Capability.Cutscene;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Cutscene
{
    [TestFixture]
    public class CutsceneActorRegistryTests
    {
        private GameObject _go;
        private CutsceneActor _actor;
        private CutsceneActorRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("RegistryTestActor");
            _go.hideFlags = HideFlags.HideAndDontSave;
            _actor = _go.AddComponent<CutsceneActor>();
            _actor.ActorRole = "Hero";
            _actor.ActorTag = "PlayerActor";
            _actor.EntityId = "E100";
            _registry = new CutsceneActorRegistry();
        }

        [TearDown]
        public void TearDown()
        {
            _registry.Dispose();
            if (_go != null)
                Object.DestroyImmediate(_go);
            _go = null;
            _actor = null;
        }

        [Test]
        public void Register_IndexesAllDimensions()
        {
            _registry.Register(_actor);

            Assert.That(_registry.TryResolveByRole("Hero", out var byRole), Is.True);
            Assert.That(byRole, Is.EqualTo(_go));
            Assert.That(_registry.TryResolveByTag("PlayerActor", out var byTag), Is.True);
            Assert.That(byTag, Is.EqualTo(_go));
            Assert.That(_registry.TryResolveByEntity("E100", out var byEntity), Is.True);
            Assert.That(byEntity, Is.EqualTo(_go));
        }

        [Test]
        public void Unregister_RemovesAllDimensions()
        {
            _registry.Register(_actor);
            _registry.Unregister(_actor);

            Assert.That(_registry.TryResolveByRole("Hero", out _), Is.False);
            Assert.That(_registry.TryResolveByTag("PlayerActor", out _), Is.False);
            Assert.That(_registry.TryResolveByEntity("E100", out _), Is.False);
        }

        [Test]
        public void Clear_RemovesEverything()
        {
            _registry.Register(_actor);
            _registry.Clear();

            Assert.That(_registry.TryResolveByRole("Hero", out _), Is.False);
            Assert.That(_registry.TryResolveByTag("PlayerActor", out _), Is.False);
            Assert.That(_registry.TryResolveByEntity("E100", out _), Is.False);
        }

        [Test]
        public void Register_Null_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _registry.Register(null));
        }

        [Test]
        public void Resolve_MissingKey_ReturnsFalse()
        {
            Assert.That(_registry.TryResolveByRole("Nobody", out var go), Is.False);
            Assert.That(go, Is.Null);
        }
    }
}
