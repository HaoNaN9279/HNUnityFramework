using System.Collections.Generic;
using HN.Framework.Unity.Capability.Asset;
using HN.Framework.Unity.Driver.Platform;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests
{
    [TestFixture]
    public class AssetManagerTests
    {
        private AssetManager m_Manager;
        private MockOperator m_Mock;

        [SetUp]
        public void SetUp()
        {
            m_Manager = new AssetManager();
            m_Mock = new MockOperator();
            m_Manager.SetOperator(m_Mock);
        }

        [TearDown]
        public void TearDown()
        {
            m_Manager.Clear();
            m_Mock.DestroyAll();
        }

        [Test]
        public void LoadAsset_IncrementsRefCount()
        {
            m_Manager.LoadAsset("test1");

            Assert.AreEqual(1, m_Manager.GetReferenceCount("test1"));
            Assert.IsTrue(m_Manager.IsLoaded("test1"));
        }

        [Test]
        public void ReleaseAsset_DecrementsRefCount()
        {
            m_Manager.LoadAsset("test1");
            m_Manager.ReleaseAsset("test1");

            Assert.AreEqual(0, m_Manager.GetReferenceCount("test1"));
            Assert.IsFalse(m_Manager.IsLoaded("test1"));
        }

        [Test]
        public void DoubleRelease_NoCrash()
        {
            m_Manager.LoadAsset("test1");
            m_Manager.ReleaseAsset("test1");

            Assert.DoesNotThrow(() => m_Manager.ReleaseAsset("test1"));
            // ReleaseAsset does not clamp at zero; double-release yields -1
            Assert.AreEqual(-1, m_Manager.GetReferenceCount("test1"));
        }

        [Test]
        public void Release_UnknownKey_NoCrash()
        {
            Assert.DoesNotThrow(() => m_Manager.ReleaseAsset("nonexistent"));
        }

        [Test]
        public void GetLoadedAssetCount_ReflectsState()
        {
            m_Manager.LoadAsset("key1");
            m_Manager.LoadAsset("key2");
            m_Manager.LoadAsset("key3");
            Assert.AreEqual(3, m_Manager.GetLoadedAssetCount());

            m_Manager.ReleaseAsset("key1");
            m_Manager.ReleaseAsset("key2");
            Assert.AreEqual(1, m_Manager.GetLoadedAssetCount());

            m_Manager.ReleaseAsset("key3");
            Assert.AreEqual(0, m_Manager.GetLoadedAssetCount());
        }

        [Test]
        public void IsLoaded_FalseForUnknownKey()
        {
            Assert.IsFalse(m_Manager.IsLoaded("nonexistent"));
        }

        // --- Scene Group Lifecycle & Tick Cleanup Tests ---

        [Test]
        public void PreloadSceneGroup_EnqueuesToQueue()
        {
            m_Manager.PreloadSceneGroup("L1");
            Assert.AreEqual(1, m_Manager.PendingPreloadCount);
        }

        [Test]
        public void Tick_ProcessesPreloadQueue()
        {
            m_Manager.PreloadSceneGroup("L1");
            m_Manager.PreloadSceneGroup("L2");
            Assert.AreEqual(2, m_Manager.PendingPreloadCount);

            m_Manager.Tick();

            Assert.AreEqual(1, m_Manager.PendingPreloadCount);
        }

        [Test]
        public void Tick_EvictsExpired()
        {
            m_Manager.LoadAsset("k");
            m_Manager.ReleaseAsset("k");
            m_Manager.SetAutoUnloadDelay(-1f);

            m_Manager.Tick();

            Assert.IsFalse(m_Manager.IsLoaded("k"));
        }

        [Test]
        public void Clear_ResetsAllState()
        {
            m_Manager.LoadAsset("a");
            m_Manager.LoadAsset("b");
            m_Manager.PreloadSceneGroup("L1");

            m_Manager.Clear();

            Assert.AreEqual(0, m_Manager.GetLoadedAssetCount());
            Assert.AreEqual(0, m_Manager.PendingPreloadCount);
        }

        [Test]
        public void GetGroupProgress_ReturnsZeroForUnknown()
        {
            Assert.AreEqual(0f, m_Manager.GetGroupProgress("nonexistent"));
        }

        [Test]
        public void Release_AfterClear_NoCrash()
        {
            m_Manager.LoadAsset("k");

            m_Manager.Clear();

            Assert.DoesNotThrow(() => m_Manager.ReleaseAsset("k"));
        }

        /// <summary>
        /// Mock resource operator that creates/destroys GameObjects for testing AssetManager reference counting.
        /// All created objects use HideFlags.HideAndDontSave to avoid scene pollution.
        /// </summary>
        public class MockOperator : IAssetOperator
        {
            private readonly List<GameObject> m_LoadedObjects = new List<GameObject>();

            /// <summary>
            /// Returns the most recently loaded GameObject, or null if none.
            /// </summary>
            public GameObject LastLoaded => m_LoadedObjects.Count > 0
                ? m_LoadedObjects[m_LoadedObjects.Count - 1]
                : null;

            public Object LoadAsset(string name)
            {
                var go = new GameObject("MockAsset_" + name);
                go.hideFlags = HideFlags.HideAndDontSave;
                m_LoadedObjects.Add(go);
                return go;
            }

            public AsyncLoadHandle LoadAssetAsync<T>(string name) where T : Object
            {
                throw new System.NotSupportedException("Async not supported in EditMode tests");
            }

            public void ReleaseAsset(Object asset)
            {
                if (asset != null)
                {
                    m_LoadedObjects.Remove((GameObject)asset);
                    Object.DestroyImmediate(asset);
                }
            }

            /// <summary>
            /// Destroys all remaining GameObjects created by this mock.
            /// Called in TearDown to prevent leaked objects between tests.
            /// </summary>
            public void DestroyAll()
            {
                foreach (var go in m_LoadedObjects)
                {
                    if (go != null)
                    {
                        Object.DestroyImmediate(go);
                    }
                }

                m_LoadedObjects.Clear();
            }
        }
    }
}
