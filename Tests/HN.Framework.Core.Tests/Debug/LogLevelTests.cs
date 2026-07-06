using NUnit.Framework;
using HN.Framework.Core.Driver.Common.Debug;

namespace HN.Framework.Core.Tests.Debug
{
    [TestFixture]
    public class LogLevelTests
    {
        [Test]
        public void Enum_HasFiveMembers()
        {
            string[] names = System.Enum.GetNames(typeof(LogLevel));
            Assert.AreEqual(5, names.Length);
        }

        [Test]
        public void Enum_ValuesAreDistinct()
        {
            Assert.AreNotEqual(LogLevel.Debug, LogLevel.Info);
            Assert.AreNotEqual(LogLevel.Info, LogLevel.Warning);
            Assert.AreNotEqual(LogLevel.Warning, LogLevel.Error);
            Assert.AreNotEqual(LogLevel.Error, LogLevel.Fatal);
        }

        [Test]
        public void Enum_HasCorrectDefaultIntValues()
        {
            Assert.AreEqual(0, (int)LogLevel.Debug);
            Assert.AreEqual(1, (int)LogLevel.Info);
            Assert.AreEqual(2, (int)LogLevel.Warning);
            Assert.AreEqual(3, (int)LogLevel.Error);
            Assert.AreEqual(4, (int)LogLevel.Fatal);
        }
    }
}
