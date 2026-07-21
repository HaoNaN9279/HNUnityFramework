#nullable enable

using System;
using NUnit.Framework;
using HN.Framework.Core.Driver.Common.Debug;

namespace HN.Framework.Core.Tests.Debug
{
    [TestFixture]
    public class DebugHubTests
    {
        private DebugHub _hub = null!;

        [SetUp]
        public void SetUp()
        {
            _hub = new DebugHub();
        }

        [TearDown]
        public void TearDown()
        {
            _hub = null!;
        }

        [Test]
        public void Constructor_InitializesEmptyRegistries()
        {
            Assert.IsNotNull(_hub.Channels);
            Assert.IsNotNull(_hub.Commands);
            Assert.IsNotNull(_hub.RecentEntries);
            Assert.AreEqual(0, _hub.Channels.Count);
            Assert.AreEqual(0, _hub.Commands.Count);
            Assert.AreEqual(0, _hub.RecentEntries.Count);
        }

        [Test]
        public void RegisterChannel_AddsChannel()
        {
            var channel = new TestLogChannel("test");
            _hub.RegisterChannel(channel);

            Assert.AreEqual(1, _hub.Channels.Count);
            Assert.IsTrue(_hub.Channels.ContainsKey("test"));
            Assert.AreSame(channel, _hub.Channels["test"]);
        }

        [Test]
        public void RegisterChannel_DuplicateOverwrites()
        {
            var channel1 = new TestLogChannel("test");
            var channel2 = new TestLogChannel("test");
            _hub.RegisterChannel(channel1);
            _hub.RegisterChannel(channel2);

            Assert.AreEqual(1, _hub.Channels.Count);
            Assert.AreSame(channel2, _hub.Channels["test"]);
        }

        [Test]
        public void Log_AddsToRecentEntries()
        {
            _hub.Log(LogLevel.Info, "test_channel", "hello world");

            Assert.AreEqual(1, _hub.RecentEntries.Count);
            var entry = _hub.RecentEntries[0];
            Assert.AreEqual(LogLevel.Info, entry.Level);
            Assert.AreEqual("test_channel", entry.Channel);
            Assert.AreEqual("hello world", entry.Message);
            Assert.IsNull(entry.Context);
        }

        [Test]
        public void Log_OnLogEventFired()
        {
            LogEntry? captured = null;
            _hub.OnLog += entry => captured = entry;

            _hub.Log(LogLevel.Warning, "ch", "msg");

            Assert.IsNotNull(captured);
            Assert.AreEqual(LogLevel.Warning, captured.Value.Level);
            Assert.AreEqual("ch", captured.Value.Channel);
            Assert.AreEqual("msg", captured.Value.Message);
        }

        [Test]
        public void Log_ExceedsMaxCapacity_DiscardsOldest()
        {
            for (int i = 0; i < 101; i++)
            {
                _hub.Log(LogLevel.Debug, "ch", $"msg_{i}");
            }

            Assert.AreEqual(100, _hub.RecentEntries.Count);
            Assert.AreEqual("msg_1", _hub.RecentEntries[0].Message);
            Assert.AreEqual("msg_100", _hub.RecentEntries[99].Message);
        }

        [Test]
        public void RegisterCommand_AddsCommand()
        {
            var command = new TestDebugCommand("pool.show", "Show pool stats");
            _hub.RegisterCommand(command);

            Assert.AreEqual(1, _hub.Commands.Count);
            Assert.IsTrue(_hub.Commands.ContainsKey("pool.show"));
            Assert.AreSame(command, _hub.Commands["pool.show"]);
        }

        // ---- Test doubles ----

        private sealed class TestLogChannel : ILogChannel
        {
            public string Name { get; }
            public bool Enabled { get; set; }
            public TestLogChannel(string name)
            {
                Name = name;
                Enabled = true;
            }
        }

        private sealed class TestDebugCommand : IDebugCommand
        {
            public string Name { get; }
            public string Description { get; }
            public TestDebugCommand(string name, string description)
            {
                Name = name;
                Description = description;
            }
            public void Execute(string[] args) { }
        }
    }
}
