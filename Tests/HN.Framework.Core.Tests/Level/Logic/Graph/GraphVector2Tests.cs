#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Graph;

namespace HN.Framework.Core.Tests.Level.Logic.Graph
{
    [TestFixture]
    public class GraphVector2Tests
    {
        [Test]
        public void Zero_IsDefault()
        {
            Assert.That(GraphVector2.Zero, Is.EqualTo(default(GraphVector2)));
            Assert.That(GraphVector2.Zero.X, Is.EqualTo(0f));
            Assert.That(GraphVector2.Zero.Y, Is.EqualTo(0f));
        }

        [Test]
        public void Constructor_SetsComponents()
        {
            var v = new GraphVector2(3.5f, -2f);
            Assert.That(v.X, Is.EqualTo(3.5f));
            Assert.That(v.Y, Is.EqualTo(-2f));
        }

        [Test]
        public void Equality_ComparesComponents()
        {
            var a = new GraphVector2(1f, 2f);
            var b = new GraphVector2(1f, 2f);
            var c = new GraphVector2(1f, 3f);

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a == b, Is.True);
            Assert.That(a != c, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }
    }
}
