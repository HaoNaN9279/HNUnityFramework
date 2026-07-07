#nullable enable

using System.Collections.Generic;
using NUnit.Framework;
using HN.Framework.Core.Capability.Debug;
using HN.Framework.Core.Driver.Common.Debug;

namespace HN.Framework.Core.Tests.Debug
{
    [TestFixture]
    public class DebugModuleTests
    {
        [Test]
        public void Constructor_SetsNameAndStoresChannelsAndCommands()
        {
            var logChannel = new TestLogChannel("test_log");
            var debugCommand = new TestDebugCommand("test.cmd", "A test command");
            ILogChannel[] channels = { logChannel };
            IDebugCommand[] commands = { debugCommand };

            var module = new DebugModule("TestModule", channels, commands);

            Assert.AreEqual("TestModule", module.Name);
            Assert.AreEqual(1, module.Channels.Count);
            Assert.AreSame(logChannel, module.Channels[0]);
            Assert.AreEqual(1, module.Commands.Count);
            Assert.AreSame(debugCommand, module.Commands[0]);
        }

        [Test]
        public void Constructor_NullChannels_DefaultsToEmpty()
        {
            var module = new DebugModule("TestModule", null, new IDebugCommand[0]);

            Assert.IsNotNull(module.Channels);
            Assert.AreEqual(0, module.Channels.Count);
        }

        [Test]
        public void Constructor_NullCommands_DefaultsToEmpty()
        {
            var module = new DebugModule("TestModule", new ILogChannel[0], null);

            Assert.IsNotNull(module.Commands);
            Assert.AreEqual(0, module.Commands.Count);
        }

        [Test]
        public void Constructor_ChannelsListIsReadOnly()
        {
            var module = new DebugModule("TestModule", new ILogChannel[0], new IDebugCommand[0]);

            Assert.IsTrue(((IList<ILogChannel>)module.Channels).IsReadOnly);
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
