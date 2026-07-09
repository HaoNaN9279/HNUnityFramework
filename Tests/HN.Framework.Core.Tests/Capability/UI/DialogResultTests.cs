#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.UI;

namespace HN.Framework.Core.Tests.Capability.UI
{
    [TestFixture]
    public class DialogResultTests
    {
        /// <summary>
        /// None 枚举值应为 0。
        /// </summary>
        [Test]
        public void None_HasValueZero()
        {
            Assert.That((int)DialogResult.None, Is.EqualTo(0));
        }

        /// <summary>
        /// Confirm 枚举值应为 1。
        /// </summary>
        [Test]
        public void Confirm_HasValueOne()
        {
            Assert.That((int)DialogResult.Confirm, Is.EqualTo(1));
        }

        /// <summary>
        /// Cancel 枚举值应为 2。
        /// </summary>
        [Test]
        public void Cancel_HasValueTwo()
        {
            Assert.That((int)DialogResult.Cancel, Is.EqualTo(2));
        }

        /// <summary>
        /// 枚举应恰好有 3 个成员。
        /// </summary>
        [Test]
        public void Enum_HasThreeMembers()
        {
            var values = System.Enum.GetValues(typeof(DialogResult));
            Assert.That(values.Length, Is.EqualTo(3));
        }
    }
}
