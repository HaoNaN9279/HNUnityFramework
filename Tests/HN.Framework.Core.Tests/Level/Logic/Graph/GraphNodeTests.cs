#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Graph;

namespace HN.Framework.Core.Tests.Level.Logic.Graph
{
    [TestFixture]
    public class GraphNodeTests
    {
        [Test]
        public void Constructor_InitializesCollections()
        {
            var node = new GraphNode("n1", "State");

            Assert.That(node.Id, Is.EqualTo("n1"));
            Assert.That(node.Type, Is.EqualTo("State"));
            Assert.That(node.Ports, Is.Not.Null);
            Assert.That(node.Ports, Is.Empty);
            Assert.That(node.Properties, Is.Not.Null);
            Assert.That(node.Properties, Is.Empty);
        }

        [Test]
        public void AddPort_AddsAndRejectsDuplicate()
        {
            var node = new GraphNode("n1", "State");
            var port = new GraphPort("Out", GraphPortDirection.Output, "Flow");

            Assert.That(node.AddPort(port), Is.True);
            Assert.That(node.AddPort(port), Is.False);
            Assert.That(node.Ports.Count, Is.EqualTo(1));
            Assert.That(node.HasPort("Out", GraphPortDirection.Output), Is.True);
        }

        [Test]
        public void TryGetPort_RespectsDirection()
        {
            var node = new GraphNode("n1", "State");
            node.AddPort(new GraphPort("P", GraphPortDirection.Input, "Flow"));

            Assert.That(node.TryGetPort("P", GraphPortDirection.Input, out var port), Is.True);
            Assert.That(port.Type, Is.EqualTo("Flow"));
            Assert.That(node.TryGetPort("P", GraphPortDirection.Output, out _), Is.False);
        }

        [Test]
        public void RemovePort_RemovesMatchingDirection()
        {
            var node = new GraphNode("n1", "State");
            node.AddPort(new GraphPort("P", GraphPortDirection.Input, "Flow"));
            node.AddPort(new GraphPort("P", GraphPortDirection.Output, "Flow"));

            Assert.That(node.RemovePort("P", GraphPortDirection.Input), Is.True);
            Assert.That(node.HasPort("P", GraphPortDirection.Input), Is.False);
            Assert.That(node.HasPort("P", GraphPortDirection.Output), Is.True);
        }

        [Test]
        public void Properties_SetGetRemove()
        {
            var node = new GraphNode("n1", "State");
            node.SetProperty("clip", "Run");

            Assert.That(node.TryGetProperty("clip", out var value), Is.True);
            Assert.That(value, Is.EqualTo("Run"));

            node.SetProperty("clip", "Walk");
            Assert.That(node.TryGetProperty("clip", out var updated), Is.True);
            Assert.That(updated, Is.EqualTo("Walk"));

            Assert.That(node.RemoveProperty("clip"), Is.True);
            Assert.That(node.TryGetProperty("clip", out _), Is.False);
        }
    }
}
