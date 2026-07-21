using System;
using HN.Framework.Core.Driver.Common.Pool.ObjectPool;
using HN.Framework.Unity.Driver.Platform;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests
{
    /// <summary>
    /// EditMode 测试 —— GameObjectPool 生命周期与核心行为验证。
    /// </summary>
    [TestFixture]
    public class GameObjectPoolTests
    {
        private TestGameObjectPool m_Pool;
        private GameObject m_Prototype;
        private GameObject m_ManagerRoot;

        [SetUp]
        public void SetUp()
        {
            m_ManagerRoot = new GameObject("ManagerRoot");
            m_ManagerRoot.hideFlags = HideFlags.HideAndDontSave;

            m_Prototype = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_Prototype.hideFlags = HideFlags.HideAndDontSave;

            m_Pool = new TestGameObjectPool();
            var settings = new GameObjectPoolSettings
            {
                BaseSettings = PoolSettings.Default("TestPool"),
                ManagerRoot = m_ManagerRoot,
                Prototype = m_Prototype,
            };
            m_Pool.Initialize(settings);
        }

        [TearDown]
        public void TearDown()
        {
            if (m_Pool != null)
            {
                m_Pool.Clear();
            }

            if (m_Prototype != null)
            {
                UnityEngine.Object.DestroyImmediate(m_Prototype);
            }

            if (m_ManagerRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(m_ManagerRoot);
            }
        }

        // ── 池创建 ──

        [Test]
        public void PoolCreation_WithPrototype_Works()
        {
            Assert.IsNotNull(m_Pool);
            Assert.AreEqual(0, m_Pool.GetCurrentCount());
            Assert.AreEqual(typeof(GameObject), m_Pool.GetObjectType());
        }

        // ── Acquire ──

        [Test]
        public void Acquire_ReturnsNonNullActiveGameObject()
        {
            var obj = m_Pool.Acquire();

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.activeSelf);

            m_Pool.Release(obj);
        }

        // ── Release ──

        [Test]
        public void Release_DeactivatesAndReparents()
        {
            var obj = m_Pool.Acquire();
            m_Pool.Release(obj);

            Assert.IsFalse(obj.activeSelf);
            Assert.AreEqual(m_Pool.Root.transform, obj.transform.parent);
        }

        // ── 多周期 ──

        [Test]
        public void MultipleAcquireRelease_CyclesWork()
        {
            var obj1 = m_Pool.Acquire();
            var obj2 = m_Pool.Acquire();
            var obj3 = m_Pool.Acquire();

            m_Pool.Release(obj1);
            m_Pool.Release(obj2);
            m_Pool.Release(obj3);

            Assert.AreEqual(3, m_Pool.GetCurrentCount());

            // Second cycle
            var objA = m_Pool.Acquire();
            var objB = m_Pool.Acquire();
            var objC = m_Pool.Acquire();

            m_Pool.Release(objA);
            m_Pool.Release(objB);
            m_Pool.Release(objC);

            Assert.AreEqual(3, m_Pool.GetCurrentCount());
        }

        // ── Clear ──

        [Test]
        public void Clear_EmptiesPool_PreservesPrototype()
        {
            var obj1 = m_Pool.Acquire();
            var obj2 = m_Pool.Acquire();
            m_Pool.Release(obj1);
            m_Pool.Release(obj2);

            Assert.AreEqual(2, m_Pool.GetCurrentCount());

            m_Pool.Clear();

            Assert.AreEqual(0, m_Pool.GetCurrentCount());
            Assert.IsNotNull(m_Prototype);
        }

        [Test]
        public void Clear_DestroysPoolRoot()
        {
            var obj = m_Pool.Acquire();
            m_Pool.Release(obj);

            Assert.IsNotNull(m_Pool.Root);

            m_Pool.Clear();

            Assert.IsNull(m_Pool.Root);
        }

        // ── 饥饿自动生成 ──

        [Test]
        public void Acquire_WhenEmpty_AutoSpawns()
        {
            Assert.AreEqual(0, m_Pool.GetCurrentCount());

            var obj = m_Pool.Acquire();

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.activeSelf);

            m_Pool.Release(obj);
        }

        // ── Tick 自动缩容 ──

        [Test]
        public void Tick_AutoShrink_ExceedsMaxCount()
        {
            // Arrange: pool with maxCount=1, pre-spawn 3 objects
            m_Pool.Clear();
            m_Pool = new TestGameObjectPool();
            var shrinkSettings = new GameObjectPoolSettings
            {
                BaseSettings = PoolSettings.Default("ShrinkPool"),
                ManagerRoot = m_ManagerRoot,
                Prototype = m_Prototype,
            };
            shrinkSettings.BaseSettings.MaxCount = 1;
            shrinkSettings.BaseSettings.TickFrequency = 1;
            shrinkSettings.BaseSettings.MaxLimitCount = int.MaxValue;
            shrinkSettings.BaseSettings.InitialCount = 3;
            shrinkSettings.BaseSettings.MinCount = 0;
            m_Pool.Initialize(shrinkSettings);

            Assert.AreEqual(3, m_Pool.GetCurrentCount());

            // Act
            m_Pool.Tick();

            // Assert: one object despawned
            Assert.AreEqual(2, m_Pool.GetCurrentCount());
        }

        // ── Tick 自动扩容 ──

        [Test]
        public void Tick_AutoGrow_BelowMinCount()
        {
            // Arrange: pool with minCount=2, pre-spawn 1 object
            m_Pool.Clear();
            m_Pool = new TestGameObjectPool();
            var growSettings = new GameObjectPoolSettings
            {
                BaseSettings = PoolSettings.Default("GrowPool"),
                ManagerRoot = m_ManagerRoot,
                Prototype = m_Prototype,
            };
            growSettings.BaseSettings.MinCount = 2;
            growSettings.BaseSettings.TickFrequency = 1;
            growSettings.BaseSettings.InitialCount = 1;
            growSettings.BaseSettings.MinLimitCount = 0;
            m_Pool.Initialize(growSettings);

            Assert.AreEqual(1, m_Pool.GetCurrentCount());

            // Act
            m_Pool.Tick();

            // Assert: one object spawned
            Assert.AreEqual(2, m_Pool.GetCurrentCount());
        }

        // ── 测试用 GameObjectPool 子类 ──

        /// <summary>
        /// GameObjectPool 的具象测试子类。
        /// </summary>
        public class TestGameObjectPool : GameObjectPool
        {
            /// <summary>
            /// 暴露 root 字段供测试验证。
            /// </summary>
            public GameObject Root => root;

            public override void OnAcquire(GameObject obj) { }
            public override void OnRelease(GameObject obj) { }
            public override void LateTick() { }
            public override Type GetObjectType() => typeof(GameObject);
        }
    }
}
