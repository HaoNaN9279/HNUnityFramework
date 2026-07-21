using System;
using NUnit.Framework;
using HN.Framework.Core.Driver.Common.Debug;

namespace HN.Framework.Core.Tests.Debug
{
    [TestFixture]
    public class LogEntryTests
    {
        [Test]
        public void Constructor_SetsAllProperties_WithContext()
        {
            DateTime timestamp = new DateTime(2026, 7, 6, 12, 0, 0);
            string channel = "test_channel";
            LogLevel level = LogLevel.Info;
            string message = "test message";
            object context = new { Key = "Value" };

            LogEntry entry = new LogEntry(timestamp, channel, level, message, context);

            Assert.AreEqual(timestamp, entry.Timestamp);
            Assert.AreEqual(channel, entry.Channel);
            Assert.AreEqual(level, entry.Level);
            Assert.AreEqual(message, entry.Message);
            Assert.AreSame(context, entry.Context);
        }

        [Test]
        public void Constructor_ContextDefaultsToNull()
        {
            LogEntry entry = new LogEntry(DateTime.Now, "ch", LogLevel.Debug, "msg");

            Assert.IsNull(entry.Context);
        }

        [Test]
        public void Struct_IsReadonly_PropertiesReflectConstructorValues()
        {
            DateTime timestamp = DateTime.Now;
            LogEntry entry = new LogEntry(timestamp, "ch", LogLevel.Debug, "msg");

            Assert.AreEqual(timestamp, entry.Timestamp);
            Assert.AreEqual("ch", entry.Channel);
            Assert.AreEqual(LogLevel.Debug, entry.Level);
            Assert.AreEqual("msg", entry.Message);
        }
    }
}
