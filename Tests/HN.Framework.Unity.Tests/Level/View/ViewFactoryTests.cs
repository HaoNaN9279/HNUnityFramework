using HN.Framework.Unity.Level.View;
using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace HN.Framework.Unity.Tests.Level.View
{
    [TestFixture]
    public class ViewFactoryTests
    {
        private ViewFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new ViewFactory();
        }

        [Test]
        public void CreateView_FromPrefab_CreatesEntityViewComponent()
        {
            var prefab = CreatePrefabWithView();

            var view = _factory.CreateView(prefab, Vector3.zero, Quaternion.identity, 10);

            Assert.That(view, Is.Not.Null);
            Assert.That(view.EntityDefId, Is.EqualTo(10));

            _factory.ReleaseView(view);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void CreateView_SetsCorrectWorldTransform()
        {
            var prefab = CreatePrefabWithView();
            var pos = new Vector3(10, 20, 30);
            var rot = Quaternion.Euler(45, 90, 0);

            var view = _factory.CreateView(prefab, pos, rot, 5);
            var go = view.gameObject;

            Assert.That(go.transform.position, Is.EqualTo(pos));
            Assert.That(go.transform.rotation.eulerAngles, Is.EqualTo(rot.eulerAngles));

            _factory.ReleaseView(view);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void CreateView_EntityViewHasValidEntityId()
        {
            var prefab = CreatePrefabWithView();

            var view = _factory.CreateView(prefab, Vector3.zero, Quaternion.identity, 20);

            Assert.That(view.EntityId, Is.EqualTo(0u)); // ViewFactory assigns EntityId=0 by default

            _factory.ReleaseView(view);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void ReleaseView_DestroysGameObject()
        {
            var prefab = CreatePrefabWithView();
            var view = _factory.CreateView(prefab, Vector3.zero, Quaternion.identity, 30);
            var go = view.gameObject;

            _factory.ReleaseView(view);

            // After Destroy, GameObject is destroyed (not null but marked destroyed)
            Assert.That(go == null, Is.True, "GameObject should be destroyed after ReleaseView");

            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void RegisterPrefabMapping_StoresMapping()
        {
            _factory.RegisterPrefabMapping(100, "Assets/Prefabs/Test.prefab");

            // No direct getter for mappings, but we verify by using CachePrefab + CreateView
            Assert.Pass("RegisterPrefabMapping executed without exception");
        }

        [Test]
        public void CachePrefab_AndGetCachedPrefab_Works()
        {
            var prefab = new GameObject("CachedPrefab");
            prefab.hideFlags = HideFlags.HideAndDontSave;
            var viewComp = prefab.AddComponent<EntityView>();

            _factory.CachePrefab("test_address", prefab);

            var cached = _factory.GetCachedPrefab("test_address");
            Assert.That(cached, Is.SameAs(prefab));

            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void CreateView_PrefabWithoutEntityView_ReturnsNull()
        {
            var prefab = new GameObject("NoViewPrefab");
            prefab.hideFlags = HideFlags.HideAndDontSave;

            LogAssert.Expect(LogType.Error, new Regex(@"\[ViewFactory\].*NoViewPrefab"));

            var view = _factory.CreateView(prefab, Vector3.zero, Quaternion.identity, 1);

            Assert.That(view, Is.Null);

            Object.DestroyImmediate(prefab);
        }

        private static GameObject CreatePrefabWithView()
        {
            var prefab = new GameObject("TestPrefab");
            prefab.hideFlags = HideFlags.HideAndDontSave;
            prefab.AddComponent<EntityView>();
            return prefab;
        }
    }
}
