
using HN.Framework.Core.Capability.Cutscene;
using HN.Framework.Unity.Capability.Cutscene;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Cutscene
{
    [TestFixture]
    public class CutsceneBindingResolverTests
    {
        private GameObject _lastCreated;
        private CutsceneActorRegistry _registry;

        private GameObject CreateTestGO(string name)
        {
            var go = new GameObject(name);
            _lastCreated = go;
            return go;
        }

        [TearDown]
        public void TearDown()
        {
            _registry?.Dispose();
            _registry = null;

            if (_lastCreated != null)
            {
                Object.DestroyImmediate(_lastCreated);
                _lastCreated = null;
            }
        }

        [Test]
        public void Resolve_ScenePath_FindsGameObject()
        {
            var go = CreateTestGO("ResolverTest_ScenePath");
            var bindings = new CutsceneBindingMap(BindingResolveMode.ScenePath);
            bindings.AddBinding("Hero", "ResolverTest_ScenePath");

            var resolver = new CutsceneBindingResolver(bindings);
            var resolved = resolver.GetReferenceValue(new UnityEngine.PropertyName("Hero"), out var valid);
            Assert.That(valid, Is.True, "Should find GameObject by ScenePath");
            Assert.That(resolved, Is.Not.Null);
        }

        [Test]
        public void Resolve_ScenePath_NotFound_ReturnsNull()
        {
            var bindings = new CutsceneBindingMap(BindingResolveMode.ScenePath);
            bindings.AddBinding("Missing", "NonExistentObject__");

            var resolver = new CutsceneBindingResolver(bindings);
            resolver.GetReferenceValue(new UnityEngine.PropertyName("Missing"), out var valid);
            Assert.That(valid, Is.False);
        }

        [Test]
        public void Resolve_Tag_FindsGameObject()
        {
            var go = CreateTestGO("ResolverTest_Tag");
            go.tag = "EditorOnly";
            var bindings = new CutsceneBindingMap(BindingResolveMode.Tag);
            bindings.AddBinding("Hero", "EditorOnly");

            var resolver = new CutsceneBindingResolver(bindings);
            resolver.GetReferenceValue(new UnityEngine.PropertyName("Hero"), out var valid);
            Assert.That(valid, Is.True, "Should find GameObject by Tag");
        }

        [Test]
        public void Resolve_Cached_ReturnsCachedValue()
        {
            var go = CreateTestGO("ResolverTest_Cache");
            var bindings = new CutsceneBindingMap(BindingResolveMode.ScenePath);
            bindings.AddBinding("Hero", "ResolverTest_Cache");

            var resolver = new CutsceneBindingResolver(bindings);
            resolver.GetReferenceValue(new UnityEngine.PropertyName("Hero"), out var firstValid);
            Assert.That(firstValid, Is.True, "First resolve should succeed");
        }

        [Test]
        public void ClearCache_ForcesReResolve()
        {
            var bindings = new CutsceneBindingMap(BindingResolveMode.ScenePath);
            bindings.AddBinding("Missing", "NonExistent__");

            var resolver = new CutsceneBindingResolver(bindings);
            resolver.GetReferenceValue(new UnityEngine.PropertyName("Missing"), out _);
            resolver.ClearCache();
            resolver.GetReferenceValue(new UnityEngine.PropertyName("Missing"), out var valid);
            Assert.That(valid, Is.False);
        }

        [Test]
        public void SetReferenceValue_StoresAndReturns()
        {
            var go = CreateTestGO("ResolverTest_SetRef");
            var bindings = new CutsceneBindingMap(BindingResolveMode.ScenePath);
            var resolver = new CutsceneBindingResolver(bindings);

            var name = new UnityEngine.PropertyName("Custom");
            resolver.SetReferenceValue(name, go);
            resolver.GetReferenceValue(name, out var valid);
            Assert.That(valid, Is.True);
        }

        [Test]
        public void Resolve_WithInjectedRegistry_UsesRegistryInsteadOfFind()
        {
            var go = CreateTestGO("RegistryActor");
            var actor = go.AddComponent<CutsceneActor>();
            actor.ActorRole = "Hero";

            _registry = new CutsceneActorRegistry();
            _registry.Register(actor);

            // targetId 为不存在的场景路径：若无注入注册表，Find 会失败
            var bindings = new CutsceneBindingMap(BindingResolveMode.ScenePath);
            bindings.AddBinding("Hero", "NonExistentScenePath__");

            var resolver = new CutsceneBindingResolver(bindings, _registry);
            var resolved = resolver.GetReferenceValue(new UnityEngine.PropertyName("Hero"), out var valid);

            Assert.That(valid, Is.True, "注入注册表后应按角色名 O(1) 命中");
            Assert.That(resolved, Is.EqualTo(go));
        }

        [Test]
        public void Resolve_NullRegistry_FallsBackToFind()
        {
            var go = CreateTestGO("ResolverTest_FallbackNullRegistry");
            var bindings = new CutsceneBindingMap(BindingResolveMode.ScenePath);
            bindings.AddBinding("Hero", "ResolverTest_FallbackNullRegistry");

            var resolver = new CutsceneBindingResolver(bindings, null);
            var resolved = resolver.GetReferenceValue(new UnityEngine.PropertyName("Hero"), out var valid);

            Assert.That(valid, Is.True, "注册表为 null 时应回退到 GameObject.Find");
            Assert.That(resolved, Is.EqualTo(go));
        }
    }
}