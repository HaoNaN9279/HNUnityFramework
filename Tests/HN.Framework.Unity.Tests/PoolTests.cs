using System;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ObjectPool;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;
using NUnit.Framework;

namespace HN.Framework.Unity.Tests
{
    /// <summary>
    /// EditMode 单元测试，覆盖 Core 层所有池系统：
    /// ReferencePool、ObjectPool、ObjectPoolManager、PooledCollections。
    /// </summary>
    [TestFixture]
    public class PoolTests
    {
        #region Helper Types

        /// <summary>
        /// 用于测试 ReferencePool 的模拟 IReference，跟踪 Clear() 调用。
        /// </summary>
        public class TestReferenceObject : IReference
        {
            /// <summary>标记 Clear() 是否已被调用。</summary>
            public bool WasCleared { get; private set; }

            public void Clear()
            {
                WasCleared = true;
            }
        }

        /// <summary>
        /// 用于测试 ObjectPool 的池化对象，跟踪 Clear() 调用。
        /// </summary>
        public class TestPooledObject : PooledObjectBase
        {
            /// <summary>标记 Clear() 是否已被调用。</summary>
            public bool WasCleared { get; private set; }

            /// <summary>
            /// 重置对象状态并设置 WasCleared 标志。
            /// </summary>
            public override void Clear()
            {
                WasCleared = true;
            }
        }

        /// <summary>
        /// 用于测试的 ObjectPool 具体子类。
        /// Spawn 创建一个新对象入队，Despawn 从队首取出并丢弃。
        /// </summary>
        public class TestObjectPool : ObjectPool<TestPooledObject>
        {
            /// <summary>创建一个新对象并放入池中。</summary>
            public override void Spawn()
            {
                objects.Enqueue(new TestPooledObject());
            }

            /// <summary>从池中取出一个对象并丢弃。</summary>
            public override void Despawn()
            {
                if (objects.Count > 0)
                    objects.Dequeue();
            }

            /// <summary>LateTick 空实现。</summary>
            public override void LateTick()
            {
            }

            /// <summary>返回池中对象的类型。</summary>
            public override Type GetObjectType()
            {
                return typeof(TestPooledObject);
            }
        }

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void SetUp()
        {
            // 确保每个测试从干净的池环境开始
            ReferencePool.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            ReferencePool.ClearAll();
        }

        #endregion

        // ====================================================================
        // A. ReferencePool Tests
        // ====================================================================

        #region ReferencePool

        /// <summary>
        /// 池为空时 Acquire 返回新实例。
        /// </summary>
        [Test]
        public void ReferencePool_Acquire_ReturnsNewInstanceWhenEmpty()
        {
            var obj = ReferencePool.Acquire<TestReferenceObject>();
            Assert.IsNotNull(obj);
            Assert.IsInstanceOf<TestReferenceObject>(obj);
        }

        /// <summary>
        /// Release 将对象回收到池中，后续 Acquire 返回同一实例。
        /// </summary>
        [Test]
        public void ReferencePool_Release_ReturnsToPool_SubsequentAcquireReturnsSameInstance()
        {
            var first = ReferencePool.Acquire<TestReferenceObject>();
            ReferencePool.Release(first);

            var second = ReferencePool.Acquire<TestReferenceObject>();
            Assert.AreSame(first, second);
        }

        /// <summary>
        /// Release 会自动调用对象的 Clear() 方法。
        /// </summary>
        [Test]
        public void ReferencePool_Release_CallsClearOnObject()
        {
            var obj = new TestReferenceObject();
            Assert.IsFalse(obj.WasCleared);

            ReferencePool.Release(obj);

            Assert.IsTrue(obj.WasCleared);
        }

        /// <summary>
        /// Add 向池中预填充指定数量的对象。
        /// </summary>
        [Test]
        public void ReferencePool_Add_PrepopulatesPool()
        {
            ReferencePool.Add<TestReferenceObject>(5);

            // 连续获取 5 个对象应均来自池（非新建）
            for (int i = 0; i < 5; i++)
            {
                var obj = ReferencePool.Acquire<TestReferenceObject>();
                Assert.IsNotNull(obj);
            }
        }

        /// <summary>
        /// Remove 从池中移除指定数量的对象。
        /// </summary>
        [Test]
        public void ReferencePool_Remove_ReducesPoolCount()
        {
            ReferencePool.Add<TestReferenceObject>(5);
            ReferencePool.Remove<TestReferenceObject>(3);

            // 池中应还剩 2 个对象；获取 2 个后第 3 个应为新建
            var a = ReferencePool.Acquire<TestReferenceObject>();
            var b = ReferencePool.Acquire<TestReferenceObject>();
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);

            // 第 3 次获取时池已空，返回新实例（不会抛异常）
            var c = ReferencePool.Acquire<TestReferenceObject>();
            Assert.IsNotNull(c);
        }

        /// <summary>
        /// RemoveAll 清空指定类型的所有池对象。
        /// </summary>
        [Test]
        public void ReferencePool_RemoveAll_EmptiesPoolForType()
        {
            ReferencePool.Add<TestReferenceObject>(5);
            ReferencePool.RemoveAll<TestReferenceObject>();

            // 池已空，Acquire 应返回新实例（不抛异常）
            for (int i = 0; i < 3; i++)
            {
                var obj = ReferencePool.Acquire<TestReferenceObject>();
                Assert.IsNotNull(obj);
            }
        }

        /// <summary>
        /// ClearAll 清空所有类型的引用池。
        /// </summary>
        [Test]
        public void ReferencePool_ClearAll_EmptiesAllPools()
        {
            ReferencePool.Add<TestReferenceObject>(5);
            Assert.DoesNotThrow(() => ReferencePool.ClearAll());

            // 清空后 Acquire 应正常工作（返回新实例）
            var obj = ReferencePool.Acquire<TestReferenceObject>();
            Assert.IsNotNull(obj);
        }

        #endregion

        // ====================================================================
        // B. ObjectPool Tests
        // ====================================================================

        #region ObjectPool

        /// <summary>
        /// Acquire 返回非空对象。
        /// </summary>
        [Test]
        public void ObjectPool_Acquire_ReturnsNonNull()
        {
            var pool = new TestObjectPool();
            pool.Initialize(PoolSettings.Default("Test"));

            var obj = pool.Acquire();

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf<TestPooledObject>(obj);
        }

        /// <summary>
        /// Release 对象后池的存储计数递增。
        /// </summary>
        [Test]
        public void ObjectPool_Release_IncreasesPoolCount()
        {
            var pool = new TestObjectPool();
            pool.Initialize(PoolSettings.Default("Test"));

            var obj = pool.Acquire();
            int beforeCount = pool.CurrentCount;

            pool.Release(obj);

            Assert.AreEqual(beforeCount + 1, pool.CurrentCount);
        }

        /// <summary>
        /// Release 会自动调用池化对象的 Clear() 方法。
        /// </summary>
        [Test]
        public void ObjectPool_Release_CallsClearOnPooledObject()
        {
            var pool = new TestObjectPool();
            pool.Initialize(PoolSettings.Default("Test"));

            var obj = pool.Acquire();
            Assert.IsFalse(obj.WasCleared);

            pool.Release(obj);

            Assert.IsTrue(obj.WasCleared);
        }

        /// <summary>
        /// Release(null) 抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void ObjectPool_Release_Null_ThrowsArgumentNullException()
        {
            var pool = new TestObjectPool();
            pool.Initialize(PoolSettings.Default("Test"));

            Assert.Throws<ArgumentNullException>(() => pool.Release(null));
        }

        /// <summary>
        /// 重复 Release 同一对象抛出 InvalidOperationException。
        /// </summary>
        [Test]
        public void ObjectPool_Release_DoubleRelease_ThrowsInvalidOperationException()
        {
            var pool = new TestObjectPool();
            pool.Initialize(PoolSettings.Default("Test"));

            var obj = pool.Acquire();
            pool.Release(obj);

            Assert.Throws<InvalidOperationException>(() => pool.Release(obj));
        }

        /// <summary>
        /// 池为空时 Acquire 自动调用 Spawn 创建新对象。
        /// </summary>
        [Test]
        public void ObjectPool_Acquire_OnEmptyPool_AutoSpawns()
        {
            var pool = new TestObjectPool();
            pool.Initialize(PoolSettings.Default("Test"));

            Assert.AreEqual(0, pool.CurrentCount);
            var obj = pool.Acquire();
            Assert.IsNotNull(obj);
            Assert.AreEqual(0, pool.CurrentCount); // 取出后池仍为空
        }

        /// <summary>
        /// Tick 在池数量超过 MaxCount 时自动销毁对象。
        /// </summary>
        [Test]
        public void ObjectPool_Tick_AutoShrinksWhenExceedsMaxCount()
        {
            var settings = PoolSettings.Default("ShrinkTest");
            settings.TickFrequency = 0;
            settings.MaxCount = 2;
            settings.MaxLimitCount = 3;

            var pool = new TestObjectPool();
            pool.Initialize(settings);

            // 手动放入 4 个对象
            pool.Spawn();
            pool.Spawn();
            pool.Spawn();
            pool.Spawn();
            Assert.AreEqual(4, pool.CurrentCount);

            // Tick 应销毁多余对象
            pool.Tick();

            // count(4) > MaxLimitCount(3) => 销毁 4-3=1 个
            Assert.AreEqual(3, pool.CurrentCount);
        }

        /// <summary>
        /// Tick 在池数量低于 MinCount 时自动创建对象。
        /// </summary>
        [Test]
        public void ObjectPool_Tick_AutoGrowsWhenBelowMinCount()
        {
            var settings = PoolSettings.Default("GrowTest");
            settings.TickFrequency = 0;
            settings.MinCount = 5;
            settings.MinLimitCount = 0;

            var pool = new TestObjectPool();
            pool.Initialize(settings);

            // 池中有 3 个对象（低于 MinCount=5）
            pool.Spawn();
            pool.Spawn();
            pool.Spawn();
            Assert.AreEqual(3, pool.CurrentCount);

            // Tick 应创建 1 个（count=3 < MinCount=5 且 > MinLimitCount=0）
            pool.Tick();
            Assert.AreEqual(4, pool.CurrentCount);
        }

        /// <summary>
        /// Clear 清空池并重置所有配置字段。
        /// </summary>
        [Test]
        public void ObjectPool_Clear_EmptiesPoolAndResetsFields()
        {
            var pool = new TestObjectPool();
            pool.Initialize(PoolSettings.Default("ClearTest"));
            pool.Spawn();
            pool.Spawn();
            Assert.AreEqual(2, pool.CurrentCount);
            Assert.AreEqual("ClearTest", pool.Name);

            pool.Clear();

            Assert.AreEqual(0, pool.CurrentCount);
            Assert.AreEqual(string.Empty, pool.Name);
            Assert.AreEqual(0, pool.InitialCount);
            Assert.AreEqual(0, pool.TickFrequency);
            Assert.AreEqual(int.MaxValue, pool.MaxCount);
            Assert.AreEqual(0, pool.MinCount);
        }

        #endregion

        // ====================================================================
        // C. ObjectPoolManager Tests
        // ====================================================================

        #region ObjectPoolManager

        /// <summary>
        /// CreateObjectPool 创建池并返回非空实例。
        /// </summary>
        [Test]
        public void ObjectPoolManager_CreateObjectPool_ReturnsNonNull()
        {
            var manager = new ObjectPoolManager();
            var settings = PoolSettings.Default("MyPool");

            var pool = manager.CreateObjectPool<TestObjectPool>(settings);

            Assert.IsNotNull(pool);
            Assert.AreEqual(1, manager.PoolCount);
        }

        /// <summary>
        /// GetObjectPool 通过名称返回正确的池实例。
        /// </summary>
        [Test]
        public void ObjectPoolManager_GetObjectPool_ReturnsCorrectPoolByName()
        {
            var manager = new ObjectPoolManager();
            var settings = PoolSettings.Default("MyPool");

            manager.CreateObjectPool<TestObjectPool>(settings);

            var retrieved = manager.GetObjectPool("MyPool");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("MyPool", retrieved.Name);
        }

        /// <summary>
        /// RemoveObjectPool 移除池并放回 ReferencePool，PoolCount 递减。
        /// </summary>
        [Test]
        public void ObjectPoolManager_RemoveObjectPool_RemovesPoolAndDecrementsCount()
        {
            var manager = new ObjectPoolManager();
            var settings = PoolSettings.Default("MyPool");

            manager.CreateObjectPool<TestObjectPool>(settings);
            Assert.AreEqual(1, manager.PoolCount);

            manager.RemoveObjectPool("MyPool");
            Assert.AreEqual(0, manager.PoolCount);
        }

        /// <summary>
        /// RemoveObjectPool 将池释放到 ReferencePool 中以便重用。
        /// </summary>
        [Test]
        public void ObjectPoolManager_RemoveObjectPool_ReleasesPoolToReferencePool()
        {
            var manager = new ObjectPoolManager();
            var settings = PoolSettings.Default("MyPool");

            manager.CreateObjectPool<TestObjectPool>(settings);

            // 从池中取出一个对象并释放，使池内有对象
            // 记录下当前 TestObjectPool 在 ReferencePool 中的状态
            manager.RemoveObjectPool("MyPool");

            // 从 ReferencePool 取出之前回收的池实例
            var recycled = ReferencePool.Acquire<TestObjectPool>();
            Assert.IsNotNull(recycled);
            Assert.AreEqual(string.Empty, recycled.Name); // Clear 后名称已重置
        }

        /// <summary>
        /// ClearAll 移除所有池并将其释放到 ReferencePool。
        /// </summary>
        [Test]
        public void ObjectPoolManager_ClearAll_RemovesAllPools()
        {
            var manager = new ObjectPoolManager();

            manager.CreateObjectPool<TestObjectPool>(PoolSettings.Default("PoolA"));
            manager.CreateObjectPool<TestObjectPool>(PoolSettings.Default("PoolB"));
            manager.CreateObjectPool<TestObjectPool>(PoolSettings.Default("PoolC"));
            Assert.AreEqual(3, manager.PoolCount);

            manager.ClearAll();

            Assert.AreEqual(0, manager.PoolCount);
        }

        /// <summary>
        /// ClearAll 将所有池释放到 ReferencePool 中。
        /// </summary>
        [Test]
        public void ObjectPoolManager_ClearAll_ReleasesPoolsToReferencePool()
        {
            var manager = new ObjectPoolManager();

            manager.CreateObjectPool<TestObjectPool>(PoolSettings.Default("PoolA"));
            manager.CreateObjectPool<TestObjectPool>(PoolSettings.Default("PoolB"));
            manager.ClearAll();

            // ClearAll 后可从 ReferencePool 拿到已回收的池实例
            var poolA = ReferencePool.Acquire<TestObjectPool>();
            var poolB = ReferencePool.Acquire<TestObjectPool>();
            Assert.IsNotNull(poolA);
            Assert.IsNotNull(poolB);
            Assert.AreNotSame(poolA, poolB);
            Assert.AreEqual(string.Empty, poolA.Name);
            Assert.AreEqual(string.Empty, poolB.Name);
        }

        /// <summary>
        /// 使用重复名称创建池抛出 InvalidOperationException。
        /// </summary>
        [Test]
        public void ObjectPoolManager_CreateObjectPool_DuplicateName_ThrowsInvalidOperationException()
        {
            var manager = new ObjectPoolManager();
            var settings = PoolSettings.Default("DuplicatePool");

            manager.CreateObjectPool<TestObjectPool>(settings);

            Assert.Throws<InvalidOperationException>(
                () => manager.CreateObjectPool<TestObjectPool>(settings));
        }

        #endregion

        // ====================================================================
        // D. PooledCollections Tests
        // ====================================================================

        #region PooledCollections

        /// <summary>
        /// PooledList 可通过 ReferencePool 获取和回收。
        /// </summary>
        [Test]
        public void PooledList_AcquireRelease()
        {
            var list = ReferencePool.Acquire<PooledList<string>>();
            Assert.IsNotNull(list);
            list.Add("hello");

            ReferencePool.Release(list);

            var list2 = ReferencePool.Acquire<PooledList<string>>();
            Assert.AreSame(list, list2);
            Assert.AreEqual(0, list2.Count); // Release 后已 Clear
        }

        /// <summary>
        /// PooledDictionary 可通过 ReferencePool 获取和回收。
        /// </summary>
        [Test]
        public void PooledDictionary_AcquireRelease()
        {
            var dict = ReferencePool.Acquire<PooledDictionary<string, int>>();
            Assert.IsNotNull(dict);
            dict["key"] = 42;

            ReferencePool.Release(dict);

            var dict2 = ReferencePool.Acquire<PooledDictionary<string, int>>();
            Assert.AreSame(dict, dict2);
            Assert.AreEqual(0, dict2.Count);
        }

        /// <summary>
        /// PooledQueue 可通过 ReferencePool 获取和回收。
        /// </summary>
        [Test]
        public void PooledQueue_AcquireRelease()
        {
            var queue = ReferencePool.Acquire<PooledQueue<string>>();
            Assert.IsNotNull(queue);
            queue.Enqueue("item");

            ReferencePool.Release(queue);

            var queue2 = ReferencePool.Acquire<PooledQueue<string>>();
            Assert.AreSame(queue, queue2);
            Assert.AreEqual(0, queue2.Count);
        }

        #endregion
    }
}
