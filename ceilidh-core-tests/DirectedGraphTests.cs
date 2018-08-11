using System.Linq;
using Ceilidh.Core.Plugin.Exceptions;
using Xunit;
using Ceilidh.Core.Util;

namespace Ceilidh.Core.Tests
{
    public class DirectedGraphTests
    {
        [Fact]
        public void TopologicalSort()
        {
            var directedGraph = new DirectedGraph<int>(Enumerable.Range(0, 5));
            directedGraph.Link(0, 1);
            directedGraph.Link(2, 1);
            directedGraph.Link(1, 3);
            directedGraph.Link(4, 3);

            Assert.Collection(directedGraph.TopologicalSort(), InitialInspector, InitialInspector, InitialInspector, x => Assert.Equal(1, x), x => Assert.Equal(3, x));

            void InitialInspector(int value)
            {
                Assert.True(value == 0 || value == 2 || value == 4);
            }
        }

        [Fact]
        public void CircularDependency()
        {
            var directedGraph = new DirectedGraph<int>(Enumerable.Range(0, 5));
            directedGraph.Link(0, 1);
            directedGraph.Link(2, 1);
            directedGraph.Link(1, 3);
            directedGraph.Link(4, 3);
            directedGraph.Link(3, 0);

            Assert.Throws<CircularDependencyException>(() => directedGraph.TopologicalSort().ToList());
        }
    }
}
