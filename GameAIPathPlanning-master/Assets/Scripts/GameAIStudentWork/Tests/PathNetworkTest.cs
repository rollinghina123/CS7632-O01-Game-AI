using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using System.Linq;

using GameAICourse;
using System.Text;

namespace Tests
{
    public class PathNetworkTest
    {
        // You can run the tests in this class in the Unity Editor if you open
        // the Test Runner Window and choose the EditMode tab


        // Annotate methods with [Test] or [TestCase(...)] to create tests like this one!
        [Test]
        public void TestName()
        {
            // Tests are performed through assertions. You can Google NUnit Assertion
            // documentation to learn about them. Several examples follow.
            Assert.That(CreatePathNetwork.StudentAuthorName, Is.Not.Contains("George P. Burdell"),
                "You forgot to change to your name!");
        }


        [Test]
        public void ExampleTest()
        {
            // Set up some parameters for testing

            Vector2 origin = new Vector2(-5f, -5f);
            Vector2 size = new Vector2(10f, 10f);
            List<Polygon> obstacles = new List<Polygon>();
            float agentRadius = 1f;
            List<Vector2> pathNodes = new List<Vector2>()
            {
                new Vector2(0f, 0f), //<-- In the middle of the canvas
                new Vector2(4.5f, 0f) //<-- Close to the middle of the right edge of the canvas boundary
            };

            // output param

            List<List<int>> pathEdges;


            //Execute your code!

            CreatePathNetwork.Create(origin, size.x, size.y, obstacles, agentRadius, agentRadius+0.01f, agentRadius*2.5f, ref pathNodes, out pathEdges, PathNetworkMode.Predefined);

            //Various assertions to validate your returned result

            Assert.That(pathEdges, Is.Not.Null);
            Assert.That(pathEdges, Has.Count.EqualTo(pathNodes.Count));
            Assert.That(pathEdges, Is.All.Not.Null);

            for (int i = 0; i < pathEdges.Count; ++i)
            {
                var edges = pathEdges[i];

                Debug.Log($"[{i}]:{string.Join(",", edges)}");

                //TODO check for self edges, dupe edges, out of range edge ends, etc...

            }

            // Nodes are not expected to connect because right node is too close to canvas boundary

            Assert.That(pathEdges, Is.All.Empty);


            // TODO add more asserts for things not tested in this example
        }

        // TODO write more tests!


        // Helper: assert the produced adjacency matches an expected undirected graph.
        // Also enforces the graph characteristics required by the rubric.
        static void AssertGraph(List<List<int>> pathEdges, int[][] expected)
        {
            Assert.That(pathEdges, Is.Not.Null);
            Assert.That(pathEdges, Has.Count.EqualTo(expected.Length));
            Assert.That(pathEdges, Is.All.Not.Null);

            for (int i = 0; i < pathEdges.Count; ++i)
            {
                var edges = pathEdges[i];

                // No self edges, in range, no duplicates
                var seen = new HashSet<int>();
                foreach (var e in edges)
                {
                    Assert.That(e, Is.Not.EqualTo(i), $"self edge at node {i}");
                    Assert.That(e, Is.InRange(0, pathEdges.Count - 1), $"out of range edge {e} at node {i}");
                    Assert.That(seen.Add(e), Is.True, $"duplicate edge {e} at node {i}");
                }

                // Bidirectional
                foreach (var e in edges)
                    Assert.That(pathEdges[e], Contains.Item(i), $"edge {i}->{e} not mirrored");

                // Matches expected set
                Assert.That(new HashSet<int>(edges), Is.EquivalentTo(new HashSet<int>(expected[i])),
                    $"node {i} adjacency mismatch. Got [{string.Join(",", edges)}] expected [{string.Join(",", expected[i])}]");
            }
        }

        static Polygon MakePoly(Vector2[] pts)
        {
            var p = new Polygon();
            p.SetPoints(pts);
            return p;
        }

        // Reproduces framework reference case pn1 (square obstacle centered at origin).
        [Test]
        public void Pn1Test()
        {
            Vector2 origin = new Vector2(-5f, -5f);
            float agentRadius = 0.5f;

            List<Polygon> obstacles = new List<Polygon>()
            {
                MakePoly(new Vector2[] {
                    new Vector2(0.6f, 0.6f), new Vector2(-0.6f, 0.6f),
                    new Vector2(-0.6f, -0.6f), new Vector2(0.6f, -0.6f),
                }),
            };

            List<Vector2> pathNodes = new List<Vector2>()
            {
                new Vector2(-2.5f, 0f), new Vector2(2.5f, 0f),
                new Vector2(0f, -2.5f), new Vector2(0f, 2.5f),
            };

            List<List<int>> pathEdges;
            CreatePathNetwork.Create(origin, 10f, 10f, obstacles, agentRadius,
                agentRadius + 0.01f, agentRadius * 2.5f, ref pathNodes, out pathEdges,
                PathNetworkMode.Predefined);

            AssertGraph(pathEdges, new int[][] {
                new int[] {2, 3},
                new int[] {2, 3},
                new int[] {0, 1},
                new int[] {0, 1},
            });
        }

        // Two nodes coincident (exact same location) in open space must connect.
        [Test]
        public void CoincidentNodesConnect()
        {
            Vector2 origin = new Vector2(-5f, -5f);
            float agentRadius = 0.5f;
            List<Polygon> obstacles = new List<Polygon>();

            List<Vector2> pathNodes = new List<Vector2>()
            {
                new Vector2(0f, 0f), new Vector2(0f, 0f),
            };

            List<List<int>> pathEdges;
            CreatePathNetwork.Create(origin, 10f, 10f, obstacles, agentRadius,
                agentRadius + 0.01f, agentRadius * 2.5f, ref pathNodes, out pathEdges,
                PathNetworkMode.Predefined);

            AssertGraph(pathEdges, new int[][] {
                new int[] {1},
                new int[] {0},
            });
        }

        // Empty node list must yield an empty (non-null) edge list.
        [Test]
        public void EmptyNodes()
        {
            Vector2 origin = new Vector2(-5f, -5f);
            List<Polygon> obstacles = new List<Polygon>();
            List<Vector2> pathNodes = new List<Vector2>();

            List<List<int>> pathEdges;
            CreatePathNetwork.Create(origin, 10f, 10f, obstacles, 1f, 1.01f, 2.5f,
                ref pathNodes, out pathEdges, PathNetworkMode.Predefined);

            Assert.That(pathEdges, Is.Not.Null);
            Assert.That(pathEdges, Has.Count.EqualTo(0));
        }

    }
}
