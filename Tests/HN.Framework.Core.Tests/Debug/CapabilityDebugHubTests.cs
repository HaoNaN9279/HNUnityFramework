#nullable enable

using System;
using NUnit.Framework;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Capability.Debug;
using HN.Framework.Core.Driver.Common.Debug;

namespace HN.Framework.Core.Tests.Debug
{
    [TestFixture]
    public class CapabilityDebugHubTests
    {
        private HN.Framework.Core.Capability.Debug.DebugHub _hub = null!;

        [SetUp]
        public void SetUp()
        {
            _hub = new HN.Framework.Core.Capability.Debug.DebugHub();
        }

        [TearDown]
        public void TearDown()
        {
            _hub = null!;
        }

        [Test]
        public void Constructor_ModulesEmpty()
        {
            Assert.IsNotNull(_hub.Modules);
            Assert.AreEqual(0, _hub.Modules.Count);
        }

        [Test]
        public void RegisterModule_AddsModuleAndRegistersChannelsAndCommands()
        {
            var channel = new TestLogChannel("pool");
            var command = new TestDebugCommand("pool.show", "Show pool stats");
            var module = new DebugModule("Pool", new ILogChannel[] { channel }, new IDebugCommand[] { command });

            _hub.RegisterModule(module);

            // Modules list contains the module
            Assert.AreEqual(1, _hub.Modules.Count);
            Assert.AreSame(module, _hub.Modules[0]);

            // Channels dict has the channel (via base class RegisterChannel)
            Assert.AreEqual(1, _hub.Channels.Count);
            Assert.IsTrue(_hub.Channels.ContainsKey("pool"));
            Assert.AreSame(channel, _hub.Channels["pool"]);

            // Commands dict has the command (via base class RegisterCommand)
            Assert.AreEqual(1, _hub.Commands.Count);
            Assert.IsTrue(_hub.Commands.ContainsKey("pool.show"));
            Assert.AreSame(command, _hub.Commands["pool.show"]);
        }

        [Test]
        public void RegisterModule_NullModule_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _hub.RegisterModule(null!));
        }

        [Test]
        public void ExecuteCommand_Found_ReturnsTrueAndExecutes()
        {
            var executed = false;
            var command = new CallbackDebugCommand("pool.show", "desc", _ => executed = true);
            _hub.RegisterCommand(command);

            var result = _hub.ExecuteCommand("pool.show", Array.Empty<string>());

            Assert.IsTrue(result);
            Assert.IsTrue(executed);
        }

        [Test]
        public void ExecuteCommand_NotFound_ReturnsFalse()
        {
            var result = _hub.ExecuteCommand("nonexistent", Array.Empty<string>());
            Assert.IsFalse(result);
        }

        [Test]
        public void ExecuteCommand_Throws_ReturnsFalse()
        {
            var command = new ThrowingDebugCommand("bad.command", "throws");
            _hub.RegisterCommand(command);

            var result = _hub.ExecuteCommand("bad.command", Array.Empty<string>());

            Assert.IsFalse(result);
        }

        [Test]
        public void SearchCommands_ByPrefix()
        {
            _hub.RegisterCommand(new TestDebugCommand("pool.show", "Show pool stats"));
            _hub.RegisterCommand(new TestDebugCommand("pool.clear", "Clear pool"));
            _hub.RegisterCommand(new TestDebugCommand("net.stats", "Network stats"));

            var results = _hub.SearchCommands("pool");

            Assert.AreEqual(2, results.Count);
        }

        [Test]
        public void SearchCommands_NoMatch_ReturnsEmptyList()
        {
            _hub.RegisterCommand(new TestDebugCommand("pool.show", "Show pool stats"));

            var results = _hub.SearchCommands("xyz");

            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void SetLogProvider_ForwardsLog()
        {
            var provider = new TestLogProvider();
            _hub.SetLogProvider(provider);

            _hub.Log(LogLevel.Info, "ch", "hello");

            Assert.AreEqual(1, provider.LogCallCount);
            Assert.AreEqual(LogLevel.Info, provider.LastLevel);
            Assert.AreEqual("ch", provider.LastChannel);
            Assert.AreEqual("hello", provider.LastMessage);
        }

        [Test]
        public void SetLogProvider_CalledTwice_NoSubscriptionLeak()
        {
            var providerA = new TestLogProvider();
            var providerB = new TestLogProvider();

            _hub.SetLogProvider(providerA);
            _hub.SetLogProvider(providerB);

            // Log should only go to providerB
            _hub.Log(LogLevel.Warning, "ch", "test");

            Assert.AreEqual(0, providerA.LogCallCount, "providerA should NOT receive logs after being replaced");
            Assert.AreEqual(1, providerB.LogCallCount, "providerB should receive the log");
        }

        [Test]
        public void SetLogProvider_Null_CancelsForwarding()
        {
            var provider = new TestLogProvider();
            _hub.SetLogProvider(provider);

            _hub.Log(LogLevel.Info, "ch", "first");
            Assert.AreEqual(1, provider.LogCallCount);

            _hub.SetLogProvider(null);
            _hub.Log(LogLevel.Info, "ch", "second");

            // Should still be 1 — no second log forwarded
            Assert.AreEqual(1, provider.LogCallCount);
        }

        [Test]
        public void InheritsBaseBehavior_RegisterChannelAndCommand()
        {
            var channel = new TestLogChannel("base_ch");
            var command = new TestDebugCommand("base.cmd", "desc");

            _hub.RegisterChannel(channel);
            _hub.RegisterCommand(command);

            Assert.AreEqual(1, _hub.Channels.Count);
            Assert.IsTrue(_hub.Channels.ContainsKey("base_ch"));
            Assert.AreEqual(1, _hub.Commands.Count);
            Assert.IsTrue(_hub.Commands.ContainsKey("base.cmd"));
        }

        [Test]
        public void InheritsBaseBehavior_LogWorks()
        {
            var captured = false;
            _hub.OnLog += _ => captured = true;

            _hub.Log(LogLevel.Debug, "ch", "msg");

            Assert.IsTrue(captured);
            Assert.AreEqual(1, _hub.RecentEntries.Count);
        }

        [Test]
        public void RegisterModule_MultipleModules_AllRegistered()
        {
            var module1 = new DebugModule("M1",
                new ILogChannel[] { new TestLogChannel("ch1") },
                new IDebugCommand[] { new TestDebugCommand("cmd1", "desc") });
            var module2 = new DebugModule("M2",
                new ILogChannel[] { new TestLogChannel("ch2") },
                new IDebugCommand[] { new TestDebugCommand("cmd2", "desc") });

            _hub.RegisterModule(module1);
            _hub.RegisterModule(module2);

            Assert.AreEqual(2, _hub.Modules.Count);
            Assert.AreEqual(2, _hub.Channels.Count);
            Assert.AreEqual(2, _hub.Commands.Count);
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

        private sealed class CallbackDebugCommand : IDebugCommand
        {
            private readonly Action<string[]> _callback;
            public string Name { get; }
            public string Description { get; }
            public CallbackDebugCommand(string name, string description, Action<string[]> callback)
            {
                Name = name;
                Description = description;
                _callback = callback;
            }
            public void Execute(string[] args) => _callback(args);
        }

        private sealed class ThrowingDebugCommand : IDebugCommand
        {
            public string Name { get; }
            public string Description { get; }
            public ThrowingDebugCommand(string name, string description)
            {
                Name = name;
                Description = description;
            }
            public void Execute(string[] args) => throw new InvalidOperationException("Test exception");
        }

        private sealed class TestLogProvider : ILogProvider
        {
            public LogLevel? LastLevel { get; private set; }
            public string? LastChannel { get; private set; }
            public string? LastMessage { get; private set; }
            public int LogCallCount { get; private set; }

            public void Log(string message) { }

            public void Log(LogLevel level, string channel, string message, object context = null)
            {
                LastLevel = level;
                LastChannel = channel;
                LastMessage = message;
                LogCallCount++;
            }
        }
    }
}
