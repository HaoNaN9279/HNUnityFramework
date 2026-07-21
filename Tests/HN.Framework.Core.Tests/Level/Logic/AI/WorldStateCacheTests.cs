#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.AI
{
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;
    using NUnit.Framework;

    /// <summary>
    /// WorldStateCache 单元测试 — 验证世界状态缓存的过期机制和存取功能。
    /// </summary>
    [TestFixture]
    public class WorldStateCacheTests
    {
        private WorldStateCache _cache = null!;

        [SetUp]
        public void SetUp()
        {
            _cache = new WorldStateCache();
            _cache.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _cache.Clear();
        }

        /// <summary>
        /// 设置不设过期时间的值后应能正确获取。
        /// </summary>
        [Test]
        public void SetAndGet_ValueWithoutExpiry_ReturnsCorrectValue()
        {
            _cache.Set("weather", "rainy");

            var result = _cache.Get<string>("weather");

            Assert.That(result, Is.EqualTo("rainy"));
        }

        /// <summary>
        /// 设置带生命周期的值，在过期前应能正常读取。
        /// </summary>
        [Test]
        public void SetAndGet_ValueWithExpiry_BeforeExpiry_ReturnsValue()
        {
            _cache.Set("enemy_pos", new Vector3Data(1f, 2f, 3f), 5f);

            var result = _cache.Get<Vector3Data>("enemy_pos");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.X, Is.EqualTo(1f));
            Assert.That(result.Y, Is.EqualTo(2f));
            Assert.That(result.Z, Is.EqualTo(3f));
        }

        /// <summary>
        /// 设置带生命周期的值，在过期后应返回 default。
        /// </summary>
        [Test]
        public void SetAndGet_ValueWithExpiry_AfterExpiry_ReturnsDefault()
        {
            _cache.Set("temp_data", 42, 1f);

            // 推进时间使数据过期
            _cache.Tick(2f);

            var result = _cache.Get<int>("temp_data");

            Assert.That(result, Is.EqualTo(0));
        }

        /// <summary>
        /// Tick 推进时间后，过期数据应被清理，未过期数据保留。
        /// </summary>
        [Test]
        public void Tick_AdvancesTimeAndExpiresOldValues()
        {
            _cache.Set("persistent", "keep", -1f); // 不过期
            _cache.Set("transient", "gone", 2f);

            _cache.Tick(3f);

            Assert.That(_cache.Get<string>("persistent"), Is.EqualTo("keep"), "Persistent value should survive.");
            Assert.That(_cache.Get<string>("transient"), Is.Null, "Expired value should return default.");
        }

        /// <summary>
        /// 移除存在的键后，该键不再可访问。
        /// </summary>
        [Test]
        public void Remove_ExistingKey_RemovesSuccessfully()
        {
            _cache.Set("target", 100);

            var removed = _cache.Remove("target");

            Assert.That(removed, Is.True);
            Assert.That(_cache.HasKey("target"), Is.False);
        }

        /// <summary>
        /// HasKey 对已过期的键应返回 false。
        /// </summary>
        [Test]
        public void HasKey_ExpiredKey_ReturnsFalse()
        {
            _cache.Set("short_lived", "data", 0.5f);

            _cache.Tick(1f);

            Assert.That(_cache.HasKey("short_lived"), Is.False);
        }
    }
}
