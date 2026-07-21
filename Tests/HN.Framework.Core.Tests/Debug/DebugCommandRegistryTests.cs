#nullable enable

using System.Collections.Generic;
using NUnit.Framework;
using HN.Framework.Core.Capability.Debug;
using HN.Framework.Core.Driver.Common.Debug;

namespace HN.Framework.Core.Tests.Debug
{
    [TestFixture]
    public class DebugCommandRegistryTests
    {
        [Test]
        public void Constructor_StoresDictionaryReference()
        {
            var dict = new Dictionary<string, IDebugCommand>
            {
                ["cmd.a"] = new TestDebugCommand("cmd.a", "desc")
            };
            var registry = new DebugCommandRegistry(dict);

            // Add to dict AFTER construction — registry should see it (reference, not copy)
            dict["cmd.b"] = new TestDebugCommand("cmd.b", "desc2");

            Assert.AreEqual(2, registry.Count);
        }

        [Test]
        public void Find_ExactMatch_ReturnsCommand()
        {
            var cmd = new TestDebugCommand("pool.show", "Show pool stats");
            var dict = new Dictionary<string, IDebugCommand> { ["pool.show"] = cmd };
            var registry = new DebugCommandRegistry(dict);

            var result = registry.Find("pool.show");

            Assert.IsNotNull(result);
            Assert.AreSame(cmd, result);
        }

        [Test]
        public void Find_NoMatch_ReturnsNull()
        {
            var dict = new Dictionary<string, IDebugCommand>
            {
                ["cmd.a"] = new TestDebugCommand("cmd.a", "desc")
            };
            var registry = new DebugCommandRegistry(dict);

            var result = registry.Find("nonexistent");

            Assert.IsNull(result);
        }

        [Test]
        public void Search_PrefixMatch_ReturnsAllMatching()
        {
            var dict = new Dictionary<string, IDebugCommand>
            {
                ["pool.show"] = new TestDebugCommand("pool.show", "Show pool"),
                ["pool.clear"] = new TestDebugCommand("pool.clear", "Clear pool"),
                ["net.stats"] = new TestDebugCommand("net.stats", "Net stats")
            };
            var registry = new DebugCommandRegistry(dict);

            var results = registry.Search("pool");

            Assert.AreEqual(2, results.Count);
        }

        [Test]
        public void Search_NoMatch_ReturnsEmpty()
        {
            var dict = new Dictionary<string, IDebugCommand>
            {
                ["cmd.a"] = new TestDebugCommand("cmd.a", "desc")
            };
            var registry = new DebugCommandRegistry(dict);

            var results = registry.Search("xyz");

            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void Search_CaseInsensitive()
        {
            var cmd = new TestDebugCommand("POOL.SHOW", "Show pool");
            var dict = new Dictionary<string, IDebugCommand> { ["POOL.SHOW"] = cmd };
            var registry = new DebugCommandRegistry(dict);

            var results = registry.Search("pool");

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(cmd, results[0]);
        }

        [Test]
        public void Commands_ReturnsSameDictionary()
        {
            var dict = new Dictionary<string, IDebugCommand>
            {
                ["cmd.a"] = new TestDebugCommand("cmd.a", "desc")
            };
            var registry = new DebugCommandRegistry(dict);

            Assert.AreSame(dict, registry.Commands);
        }

        [Test]
        public void Count_DelegatesToDictionary()
        {
            var dict = new Dictionary<string, IDebugCommand>
            {
                ["cmd.a"] = new TestDebugCommand("cmd.a", "desc"),
                ["cmd.b"] = new TestDebugCommand("cmd.b", "desc"),
                ["cmd.c"] = new TestDebugCommand("cmd.c", "desc")
            };
            var registry = new DebugCommandRegistry(dict);

            Assert.AreEqual(dict.Count, registry.Count);
            Assert.AreEqual(3, registry.Count);
        }

        // ---- Test doubles ----

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
