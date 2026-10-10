#nullable enable

using MemoryPack;
using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Graph;

namespace HN.Framework.Core.Tests.Level.Logic.Graph
{
    [TestFixture]
    public class GraphSerializationTests
    {
        private static GraphData BuildSampleGraph()
        {
            var graph = new GraphData("g1", "Sample");

            var entry = new GraphNode("entry", "Entry", new GraphVector2(10f, 20f));
            entry.AddPort(new GraphPort("Out", GraphPortDirection.Output, "Flow"));
            entry.SetProperty("target", "idle");

            var idle = new GraphNode("idle", "State", new GraphVector2(100f, 20f));
            idle.AddPort(new GraphPort("In", GraphPortDirection.Input, "Flow"));
            idle.AddPort(new GraphPort("Out", GraphPortDirection.Output, "Flow"));
            idle.SetProperty("clip", "Idle");

            graph.AddNode(entry);
            graph.AddNode(idle);
            graph.AddEdge(new GraphEdge("entry", "Out", "idle", "In", "e1"));

            var group = new GraphGroup("grp", "Locomotion");
            group.AddNode("idle");
            group.Position = new GraphVector2(80f, 0f);
            group.Size = new GraphVector2(200f, 100f);
            graph.AddGroup(group);

            return graph;
        }

        [Test]
        public void Roundtrip_PreservesGraphStructure()
        {
            var graph = BuildSampleGraph();

            byte[] data = MemoryPackSerializer.Serialize(graph);
            var restored = MemoryPackSerializer.Deserialize<GraphData>(data);

            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.Id, Is.EqualTo("g1"));
            Assert.That(restored.Name, Is.EqualTo("Sample"));
            Assert.That(restored.NodeCount, Is.EqualTo(2));
            Assert.That(restored.EdgeCount, Is.EqualTo(1));
            Assert.That(restored.GroupCount, Is.EqualTo(1));
        }

        [Test]
        public void Roundtrip_PreservesNodeFields()
        {
            var graph = BuildSampleGraph();

            byte[] data = MemoryPackSerializer.Serialize(graph);
            var restored = MemoryPackSerializer.Deserialize<GraphData>(data)!;

            var idle = restored.GetNode("idle");
            Assert.That(idle, Is.Not.Null);
            Assert.That(idle!.Type, Is.EqualTo("State"));
            Assert.That(idle.Position, Is.EqualTo(new GraphVector2(100f, 20f)));
            Assert.That(idle.Ports.Count, Is.EqualTo(2));
            Assert.That(idle.HasPort("In", GraphPortDirection.Input), Is.True);
            Assert.That(idle.HasPort("Out", GraphPortDirection.Output), Is.True);
            Assert.That(idle.TryGetProperty("clip", out var clip), Is.True);
            Assert.That(clip, Is.EqualTo("Idle"));
        }

        [Test]
        public void Roundtrip_PreservesEdgeEndpoints()
        {
            var graph = BuildSampleGraph();

            byte[] data = MemoryPackSerializer.Serialize(graph);
            var restored = MemoryPackSerializer.Deserialize<GraphData>(data)!;

            var edge = restored.GetEdge("e1");
            Assert.That(edge, Is.Not.Null);
            Assert.That(edge!.SourceNodeId, Is.EqualTo("entry"));
            Assert.That(edge.SourcePort, Is.EqualTo("Out"));
            Assert.That(edge.TargetNodeId, Is.EqualTo("idle"));
            Assert.That(edge.TargetPort, Is.EqualTo("In"));
        }

        [Test]
        public void Roundtrip_PreservesGroup()
        {
            var graph = BuildSampleGraph();

            byte[] data = MemoryPackSerializer.Serialize(graph);
            var restored = MemoryPackSerializer.Deserialize<GraphData>(data)!;

            var group = restored.GetGroup("grp");
            Assert.That(group, Is.Not.Null);
            Assert.That(group!.Title, Is.EqualTo("Locomotion"));
            Assert.That(group.NodeIds, Is.EqualTo(new[] { "idle" }));
            Assert.That(group.Position, Is.EqualTo(new GraphVector2(80f, 0f)));
            Assert.That(group.Size, Is.EqualTo(new GraphVector2(200f, 100f)));
        }

        [Test]
        public void Roundtrip_RestoredGraphIsValid()
        {
            var graph = BuildSampleGraph();

            byte[] data = MemoryPackSerializer.Serialize(graph);
            var restored = MemoryPackSerializer.Deserialize<GraphData>(data)!;

            Assert.That(restored.Validate(out var error), Is.True, error);
        }

        [Test]
        public void Roundtrip_EmptyGraph()
        {
            var graph = new GraphData("empty", "Empty");

            byte[] data = MemoryPackSerializer.Serialize(graph);
            var restored = MemoryPackSerializer.Deserialize<GraphData>(data);

            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.Id, Is.EqualTo("empty"));
            Assert.That(restored.Nodes, Is.Empty);
            Assert.That(restored.Edges, Is.Empty);
            Assert.That(restored.Groups, Is.Empty);
        }

        [Test]
        public void Roundtrip_NodeWithNullTitle()
        {
            var graph = new GraphData("g1");
            graph.AddNode(new GraphNode("a", "State"));

            byte[] data = MemoryPackSerializer.Serialize(graph);
            var restored = MemoryPackSerializer.Deserialize<GraphData>(data)!;

            Assert.That(restored.GetNode("a")!.Title, Is.Null);
        }
    }
}
