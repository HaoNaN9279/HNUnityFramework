#nullable enable

using System;
using NUnit.Framework;
using HN.Framework.Core.Driver.Common.Math;

namespace HN.Framework.Core.Tests.Math
{
    /// <summary>
    /// <see cref="HNRandom"/> 的单元测试，覆盖确定性、边界条件、状态序列化与范围正确性。
    /// </summary>
    [TestFixture]
    public class HNRandomTests
    {
        private const ulong TestSeed = 1234567890UL;

        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        #region 构造函数测试

        [Test]
        public void Constructor_Default_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new HNRandom());
        }

        [Test]
        public void Constructor_WithSeed_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new HNRandom(TestSeed));
        }

        [Test]
        public void Constructor_WithTwoStateValues_Valid_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new HNRandom(1UL, 2UL));
        }

        [Test]
        public void Constructor_WithTwoStateValues_BothZero_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new HNRandom(0UL, 0UL));
        }

        #endregion

        #region 确定性测试

        [Test]
        public void SameSeed_ProducesIdenticalSequence()
        {
            var rng1 = new HNRandom(TestSeed);
            var rng2 = new HNRandom(TestSeed);

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(rng1.NextUInt64(), rng2.NextUInt64(), $"第 {i} 次调用不一致");
            }
        }

        [Test]
        public void DifferentSeed_ProducesDifferentSequence()
        {
            var rng1 = new HNRandom(TestSeed);
            var rng2 = new HNRandom(TestSeed + 1);

            bool allSame = true;
            for (int i = 0; i < 20; i++)
            {
                if (rng1.NextUInt64() != rng2.NextUInt64())
                {
                    allSame = false;
                    break;
                }
            }

            Assert.IsFalse(allSame, "不同种子应产生不同的随机数序列");
        }

        [Test]
        public void MultipleDefaultInstances_ProduceDifferentValues()
        {
            var rng1 = new HNRandom();
            var rng2 = new HNRandom();

            // 默认构造函数使用 TickCount + Guid，不同的默认实例大概率产生不同值
            bool allSame = true;
            for (int i = 0; i < 10; i++)
            {
                if (rng1.NextUInt64() != rng2.NextUInt64())
                {
                    allSame = false;
                    break;
                }
            }

            Assert.IsFalse(allSame, "两个默认构造的实例应大概率产生不同序列");
        }

        #endregion

        #region Next 测试

        [Test]
        public void Next_ReturnsNonNegativeInteger()
        {
            var rng = new HNRandom(TestSeed);

            for (int i = 0; i < 1000; i++)
            {
                int result = rng.Next();
                Assert.That(result, Is.GreaterThanOrEqualTo(0));
                Assert.That(result, Is.LessThanOrEqualTo(int.MaxValue));
            }
        }

        [Test]
        public void Next_MaxValue_ReturnsValueLessThanMax()
        {
            var rng = new HNRandom(TestSeed);
            const int max = 100;

            for (int i = 0; i < 10000; i++)
            {
                int result = rng.Next(max);
                Assert.That(result, Is.GreaterThanOrEqualTo(0));
                Assert.That(result, Is.LessThan(max));
            }
        }

        [Test]
        public void Next_MaxValue_CoversFullRange()
        {
            var rng = new HNRandom(TestSeed);
            const int max = 10;
            var seen = new bool[max];

            for (int i = 0; i < 10000; i++)
            {
                int result = rng.Next(max);
                seen[result] = true;

                if (AllTrue(seen))
                    break;
            }

            Assert.IsTrue(AllTrue(seen), "Next(max) 应覆盖 [0, max) 范围内所有值");
        }

        [Test]
        public void Next_MaxValue_ZeroOrNegative_ThrowsArgumentOutOfRangeException()
        {
            var rng = new HNRandom(TestSeed);
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(-1));
        }

        [Test]
        public void Next_MinMax_ReturnsValueInRange()
        {
            var rng = new HNRandom(TestSeed);
            const int min = 10;
            const int max = 20;

            for (int i = 0; i < 10000; i++)
            {
                int result = rng.Next(min, max);
                Assert.That(result, Is.GreaterThanOrEqualTo(min));
                Assert.That(result, Is.LessThan(max));
            }
        }

        [Test]
        public void Next_MinMax_CoversFullRange()
        {
            var rng = new HNRandom(TestSeed);
            const int min = 0;
            const int max = 10;
            var seen = new bool[max - min];

            for (int i = 0; i < 10000; i++)
            {
                int result = rng.Next(min, max);
                seen[result - min] = true;

                if (AllTrue(seen))
                    break;
            }

            Assert.IsTrue(AllTrue(seen), "Next(min, max) 应覆盖指定范围内所有值");
        }

        [Test]
        public void Next_MinMax_MinEqualsMax_ThrowsArgumentOutOfRangeException()
        {
            var rng = new HNRandom(TestSeed);
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(5, 5));
        }

        [Test]
        public void Next_MinMax_MinGreaterThanMax_ThrowsArgumentOutOfRangeException()
        {
            var rng = new HNRandom(TestSeed);
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(10, 5));
        }

        #endregion

        #region NextUInt64 测试

        [Test]
        public void NextUInt64_ReturnsFullRangeValues()
        {
            var rng = new HNRandom(TestSeed);
            bool foundHighBit = false;
            bool foundLowBit = false;

            for (int i = 0; i < 10000; i++)
            {
                ulong value = rng.NextUInt64();
                if ((value >> 63) != 0) foundHighBit = true;
                if ((value & 1) != 0) foundLowBit = true;

                if (foundHighBit && foundLowBit)
                    break;
            }

            Assert.IsTrue(foundHighBit, "NextUInt64 应产生第 63 位为 1 的值");
            Assert.IsTrue(foundLowBit, "NextUInt64 应产生第 0 位为 1 的值");
        }

        [Test]
        public void NextUInt64_LargeCount_NoOverflowOrStuckState()
        {
            var rng = new HNRandom(TestSeed);
            ulong previous = rng.NextUInt64();

            for (int i = 0; i < 10000; i++)
            {
                ulong current = rng.NextUInt64();
                Assert.That(current, Is.Not.EqualTo(previous), "连续两次调用不应始终返回相同值");
                previous = current;
            }
        }

        #endregion

        #region NextDouble / NextFloat 测试

        [Test]
        public void NextDouble_ReturnsValueInRange()
        {
            var rng = new HNRandom(TestSeed);

            for (int i = 0; i < 10000; i++)
            {
                double result = rng.NextDouble();
                Assert.That(result, Is.GreaterThanOrEqualTo(0.0));
                Assert.That(result, Is.LessThan(1.0));
            }
        }

        [Test]
        public void NextDouble_DoesNotAlwaysReturnZero()
        {
            var rng = new HNRandom(TestSeed);
            bool foundNonZero = false;

            for (int i = 0; i < 1000; i++)
            {
                if (rng.NextDouble() > 0.0)
                {
                    foundNonZero = true;
                    break;
                }
            }

            Assert.IsTrue(foundNonZero, "NextDouble 不应始终返回 0.0");
        }

        [Test]
        public void NextFloat_ReturnsValueInRange()
        {
            var rng = new HNRandom(TestSeed);

            for (int i = 0; i < 10000; i++)
            {
                float result = rng.NextFloat();
                Assert.That(result, Is.GreaterThanOrEqualTo(0.0f));
                Assert.That(result, Is.LessThan(1.0f));
            }
        }

        [Test]
        public void NextFloat_DoesNotAlwaysReturnZero()
        {
            var rng = new HNRandom(TestSeed);
            bool foundNonZero = false;

            for (int i = 0; i < 1000; i++)
            {
                if (rng.NextFloat() > 0.0f)
                {
                    foundNonZero = true;
                    break;
                }
            }

            Assert.IsTrue(foundNonZero, "NextFloat 不应始终返回 0.0f");
        }

        #endregion

        #region 状态序列化测试

        [Test]
        public void GetState_AfterConstruction_ReturnsValidState()
        {
            var rng = new HNRandom(TestSeed);
            var (s0, s1) = rng.GetState();

            Assert.That(s0, Is.Not.EqualTo(0UL));
            // s0 和 s1 至少有一个非零
            Assert.That(s0 != 0UL || s1 != 0UL, Is.True);
        }

        [Test]
        public void GetState_AfterCalls_StateChanges()
        {
            var rng = new HNRandom(TestSeed);
            var (initialS0, initialS1) = rng.GetState();

            rng.NextUInt64();
            var (afterS0, afterS1) = rng.GetState();

            // 每次调用后状态应当变化
            Assert.That(afterS0 != initialS0 || afterS1 != initialS1, Is.True,
                "调用 NextUInt64 后状态应发生变化");
        }

        [Test]
        public void SetState_RestoresIdenticalSequence()
        {
            var rng1 = new HNRandom(TestSeed);

            // 消费一些随机数
            for (int i = 0; i < 50; i++)
            {
                rng1.NextUInt64();
            }

            // 保存状态
            var (s0, s1) = rng1.GetState();

            // 继续获取更多值作为"未来"参考
            var futureValues = new ulong[30];
            for (int i = 0; i < 30; i++)
            {
                futureValues[i] = rng1.NextUInt64();
            }

            // 用保存的状态创建新实例
            var rng2 = new HNRandom(s0, s1);

            // 验证新实例产生相同的"未来"序列
            for (int i = 0; i < 30; i++)
            {
                Assert.AreEqual(futureValues[i], rng2.NextUInt64(),
                    $"第 {i} 个值不一致，状态恢复失败");
            }
        }

        [Test]
        public void SetState_AfterConstruction_ChangesBehavior()
        {
            var rng = new HNRandom(TestSeed);

            // 记录初始序列
            var initialValues = new ulong[10];
            for (int i = 0; i < 10; i++)
            {
                initialValues[i] = rng.NextUInt64();
            }

            // 恢复状态到初始
            var rng2 = new HNRandom(TestSeed);
            rng.SetState(rng2.GetState().S0, rng2.GetState().S1);

            // 验证恢复后产生相同序列
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(initialValues[i], rng.NextUInt64(),
                    $"第 {i} 个值不一致，SetState 恢复失败");
            }
        }

        [Test]
        public void SetState_BothZero_ThrowsArgumentException()
        {
            var rng = new HNRandom(TestSeed);
            Assert.Throws<ArgumentException>(() => rng.SetState(0UL, 0UL));
        }

        #endregion

        #region 边界条件测试

        [Test]
        public void Seed_Zero_DoesNotThrowAndProducesValidSequence()
        {
            var rng = new HNRandom(0UL);

            // 不应抛出异常
            ulong first = rng.NextUInt64();
            ulong second = rng.NextUInt64();

            // 两次调用应产生不同值
            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void Seed_UInt64Max_ProducesValidSequence()
        {
            var rng = new HNRandom(ulong.MaxValue);

            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    rng.NextUInt64();
                }
            });
        }

        [Test]
        public void Next_MinMax_LargeRange_DoesNotThrow()
        {
            var rng = new HNRandom(TestSeed);
            const int min = int.MinValue;
            const int max = int.MaxValue;

            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    int result = rng.Next(min, max);
                    Assert.That(result, Is.GreaterThanOrEqualTo(min));
                    Assert.That(result, Is.LessThan(max));
                }
            });
        }

        #endregion

        #region 辅助方法

        private static bool AllTrue(bool[] array)
        {
            foreach (bool b in array)
            {
                if (!b) return false;
            }
            return true;
        }

        #endregion
    }
}
