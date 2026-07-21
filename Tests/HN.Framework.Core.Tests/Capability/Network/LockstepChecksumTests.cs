#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Network;

namespace HN.Framework.Core.Tests.Capability.Network
{
    /// <summary>
    /// XorChecksumProvider 的单元测试。验证确定性、差异检测和边界情况。
    /// </summary>
    [TestFixture]
    public class LockstepChecksumTests
    {
        #region Compute - Determinism

        [Test]
        public void Compute_SameInput_ReturnsSameChecksum()
        {
            var provider = new XorChecksumProvider();
            byte[] data = { 1, 2, 3, 4, 5, 6, 7, 8 };

            ulong first = provider.Compute(data);
            ulong second = provider.Compute(data);

            Assert.That(first, Is.EqualTo(second));
        }

        [Test]
        public void Compute_EmptyArray_ReturnsZero()
        {
            var provider = new XorChecksumProvider();
            Assert.That(provider.Compute(System.Array.Empty<byte>()), Is.EqualTo(0UL));
        }

        [Test]
        public void Compute_NullInput_ReturnsZero()
        {
            var provider = new XorChecksumProvider();
            Assert.That(provider.Compute(null!), Is.EqualTo(0UL));
        }

        #endregion

        #region Compute - Difference Detection

        [Test]
        public void Compute_DifferentInput_ReturnsDifferentChecksum()
        {
            var provider = new XorChecksumProvider();
            byte[] dataA = { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte[] dataB = { 1, 2, 3, 4, 0, 6, 7, 8 };

            Assert.That(provider.Compute(dataA), Is.Not.EqualTo(provider.Compute(dataB)));
        }

        [Test]
        public void Compute_DifferentLength_ReturnsDifferentChecksum()
        {
            var provider = new XorChecksumProvider();
            byte[] dataA = { 1, 2, 3 };
            byte[] dataB = { 1, 2, 3, 4 };

            Assert.That(provider.Compute(dataA), Is.Not.EqualTo(provider.Compute(dataB)));
        }

        [Test]
        public void Compute_SingleByte_ReturnsCorrectValue()
        {
            var provider = new XorChecksumProvider();
            byte[] data = { 0xAB };

            ulong result = provider.Compute(data);
            Assert.That(result, Is.EqualTo(0xABUL));
        }

        [Test]
        public void Compute_Exact8Bytes_ReturnsBlockXor()
        {
            var provider = new XorChecksumProvider();
            byte[] data = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            ulong result = provider.Compute(data);
            ulong expected = 0x0807060504030201UL;
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Compute_MultipleOf8Bytes_XorAcrossBlocks()
        {
            var provider = new XorChecksumProvider();
            byte[] data = new byte[16];
            for (int i = 0; i < 16; i++) data[i] = (byte)i;

            ulong result = provider.Compute(data);
            // Block 1: 0x0706050403020100
            // Block 2: 0x0F0E0D0C0B0A0908
            // XOR:     0x08080A0808080808
            ulong expected = 0x0706050403020100UL ^ 0x0F0E0D0C0B0A0908UL;
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion

        #region Compare

        [Test]
        public void Compare_MatchingData_ReturnsTrue()
        {
            var provider = new XorChecksumProvider();
            byte[] data = { 10, 20, 30, 40 };
            ulong checksum = provider.Compute(data);

            Assert.That(provider.Compare(data, checksum), Is.True);
        }

        [Test]
        public void Compare_DifferentData_ReturnsFalse()
        {
            var provider = new XorChecksumProvider();
            byte[] data = { 10, 20, 30, 40 };
            ulong checksum = provider.Compute(data);

            byte[] differentData = { 10, 20, 30, 41 };
            Assert.That(provider.Compare(differentData, checksum), Is.False);
        }

        #endregion
    }
}