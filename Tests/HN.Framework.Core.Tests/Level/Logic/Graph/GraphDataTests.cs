#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Graph;

namespace HN.Framework.Core.Tests.Level.Logic.Graph
{
    [TestFixture]
    public class GraphDataTests
    {
        private static GraphNode MakeNode(string id, string type = "State")
        {
            var node = new GraphNode(id, type);
            node.AddPort(new GraphPort("In", GraphPortDirection.Input, "Flow"));
            node.AddPort(new GraphPort("Out", GraphPortDirection.Output, "Flow"));
            return node;
        }

        [Test]
        public void Constructor_InitializesEmptyCollections()
        {
            var graph = new GraphData("g1", "Main");

            Assert.That(graph.Id, Is.EqualTo("g1"));
            Assert.That(graph.Name, Is.EqualTo("Main"));
            Assert.That(graph.Nodes, Is.Empty);
            Assert.That(graph.Edges, Is.Empty);
            Assert.That(graph.Groups, Is.Empty);
            Assert.That(graph.NodeCount, Is.EqualTo(0));
            Assert.That(graph.EdgeCount, Is.EqualTo(0));
            Assert.That(graph.GroupCount, Is.EqualTo(0));
        }

        [Test]
        public void AddNode_RejectsDuplicateAndEmptyId()
        {
            var graph = new GraphData("g1");

            Assert.That(graph.AddNode(MakeNode("a")), Is.True);
            Assert.That(graph.AddNode(MakeNode("a")), Is.False);
            Assert.That(graph.AddNode(new GraphNode(string.Empty, "State")), Is.False);
            Assert.That(graph.NodeCount, Is.EqualTo(1));
        }

        [Test]
        public void GetNode_ReturnsMatchingNode()
        {
            var graph = new GraphData("g1");
            var node = MakeNode("a");
            graph.AddNode(node);

            Assert.That(graph.GetNode("a"), Is.SameAs(node));
            Assert.That(graph.GetNode("missing"), Is.Null);
            Assert.That(graph.ContainsNode("a"), Is.True);
        }

        [Test]
        public void RemoveNode_RemovesConnectedEdgesAndGroupReferences()
        {
            var graph = new GraphData("g1");
            graph.AddNode(MakeNode("a"));
            graph.AddNode(MakeNode("b"));
            graph.AddNode(MakeNode("c"));
            graph.AddEdge(new GraphEdge("a", "Out", "b", "In", "e1"));
            graph.AddEdge(new GraphEdge("b", "Out", "c", "In", "e2"));

            var group = new GraphGroup("grp", "Group");
            group.AddNode("a");
            group.AddNode("b");
            graph.AddGroup(group);

            Assert.That(graph.RemoveNode("b"), Is.True);

            Assert.That(graph.ContainsNode("b"), Is.False);
            Assert.That(graph.EdgeCount, Is.EqualTo(0));
            Assert.That(group.NodeIds, Does.Not.Contain("b"));
            Assert.That(group.NodeIds, Does.Contain("a"));
        }

        [Test]
        public void AddEdge_RejectsDuplicateId()
        {
            var graph = new GraphData("g1");
            graph.AddNode(MakeNode("a"));
            graph.AddNode(MakeNode("b"));

            Assert.That(graph.AddEdge(new GraphEdge("a", "Out", "b", "In", "e1")), Is.True);
            Assert.That(graph.AddEdge(new GraphEdge("a", "Out", "b", "In", "e1")), Is.False);
            Assert.That(graph.EdgeCount, Is.EqualTo(1));
        }

        [Test]
        public void GetEdgesFromAndToNode_FilterByDirection()
        {
            var graph = new GraphData("g1");
            graph.AddNode(MakeNode("a"));
            graph.AddNode(MakeNode("b"));
            graph.AddNode(MakeNode("c"));
            graph.AddEdge(new GraphEdge("a", "Out", "b", "In", "e1"));
            graph.AddEdge(new GraphEdge("c", "Out", "b", "In", "e2"));

            Assert.That(graph.GetEdgesFromNode("a").Count, Is.EqualTo(1));
            Assert.That(graph.GetEdgesToNode("b").Count, Is.EqualTo(2));
            Assert.That(graph.GetEdgesFromNode("b"), Is.Empty);
        }

        [Test]
        public void RemoveEdge_RemovesById()
        {
            var graph = new GraphData("g1");
            graph.AddNode(MakeNode("a"));
            graph.AddNode(MakeNode("b"));
            graph.AddEdge(new GraphEdge("a", "Out", "b", "In", "e1"));

            Assert.That(graph.RemoveEdge("e1"), Is.True);
            Assert.That(graph.RemoveEdge("e1"), Is.False);
            Assert.That(graph.EdgeCount, Is.EqualTo(0));
        }

        [Test]
        public void AddGroup_RejectsDuplicate()
        {
            var graph = new GraphData("g1");

            Assert.That(graph.AddGroup(new GraphGroup("grp", "G")), Is.True);
            Assert.That(graph.AddGroup(new GraphGroup("grp", "G2")), Is.False);
            Assert.That(graph.RemoveGroup("grp"), Is.True);
            Assert.That(graph.GroupCount, Is.EqualTo(0));
        }

        [Test]
        public void Validate_PassesForConsistentGraph()
        {
            var graph = new GraphData("g1");
            graph.AddNode(MakeNode("a"));
            graph.AddNode(MakeNode("b"));
            graph.AddEdge(new GraphEdge("a", "Out", "b", "In", "e1"));
            var group = new GraphGroup("grp", "G");
            group.AddNode("a");
            graph.AddGroup(group);

            Assert.That(graph.Validate(out var error), Is.True);
            Assert.That(error, Is.Null);
        }

        [Test]
        public void Validate_FailsForMissingEndpoint()
        {
            var graph = new GraphData("g1");
            graph.AddNode(MakeNode("a"));
            graph.AddEdge(new GraphEdge("a", "Out", "missing", "In", "e1"));

            Assert.That(graph.Validate(out var error), Is.False);
            Assert.That(error, Does.Contain("missing"));
        }

        [Test]
        public void Validate_FailsForMissingPort()
        {
            var graph = new GraphData("g1");
            graph.AddNode(MakeNode("a"));
            graph.AddNode(MakeNode("b"));
            graph.AddEdge(new GraphEdge("a", "Unknown", "b", "In", "e1"));

            Assert.That(graph.Validate(out var error), Is.False);
            Assert.That(error, Does.Contain("Unknown"));
        }

        [Test]
        public void Validate_FailsForMissingGroupNode()
        {
            var graph = new GraphData("g1");
            var group = new GraphGroup("grp", "G");
            group.AddNode("ghost");
            graph.AddGroup(group);

            Assert.That(graph.Validate(out var error), Is.False);
            Assert.That(error, Does.Contain("ghost"));
        }
    }
}
