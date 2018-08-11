using System.Collections.Generic;
using System.Linq;
using Ceilidh.Core.Plugin.Exceptions;

namespace Ceilidh.Core.Util
{
    /// <summary>
    ///     Represents a directed graph
    /// </summary>
    /// <typeparam name="TNode">The type of the nodes in this graph</typeparam>
    internal class DirectedGraph<TNode> where TNode : class
    {
        private readonly HashSet<Edge> _edges = new HashSet<Edge>();
        private readonly HashSet<TNode> _nodes;

        /// <summary>
        ///     Construct an empty graph.
        /// </summary>
        public DirectedGraph() : this(new TNode[0])
        {
        }

        /// <summary>
        ///     Construct a graph with certain nodes
        /// </summary>
        /// <param name="nodes">The nodes to preload the graph with</param>
        public DirectedGraph(IEnumerable<TNode> nodes)
        {
            _nodes = new HashSet<TNode>(nodes);
        }

        /// <summary>
        ///     Create a directed edge between two nodes in the graph.
        /// </summary>
        /// <param name="from">The start of the new edge</param>
        /// <param name="to">The end of the new edge</param>
        public void Link(TNode from, TNode to)
        {
            _nodes.Add(from);
            _nodes.Add(to);

            _edges.Add(new Edge(from, to));
        }

        /// <summary>
        ///     Perform a topologic sort on the graph
        /// </summary>
        /// <returns>Each node, in topological order</returns>
        public IEnumerable<TNode> TopologicalSort()
        {
            var edges = new HashSet<Edge>(_edges);

            var stack = new Stack<TNode>(_nodes.Where(n =>
                edges.All(e => !e.To.Equals(n)))); // Create a stack containing all current nodes with no incoming edges
            while (stack.Count > 0)
            {
                var n = stack.Pop(); // Remove and return the current node

                yield return n;

                foreach (var e in edges.Where(e => e.From.Equals(n)).ToList())
                {   // Remove all edges that come from this node
                    var m = e.To;

                    edges.Remove(e);

                    if (edges.All(me => !me.To.Equals(m))) // Push nodes with no incoming edges into the stack
                        stack.Push(m);
                }
            }

            if (edges.Count > 0) // If we have any leftover edges, this graph isn't acyclic
                throw new CircularDependencyException();
        }

        private struct Edge
        {
            public readonly TNode From;
            public readonly TNode To;

            public Edge(TNode from, TNode to)
            {
                From = from;
                To = to;
            }
        }
    }
}