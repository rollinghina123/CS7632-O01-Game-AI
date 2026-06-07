using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAICourse
{

    public class CreatePathNetwork
    {

        public const string StudentAuthorName = "Zonghao Hou";




        // Helper method provided to help you implement this file. Leave as is.
        // Returns Vector2 converted to Vector2Int according to default scaling factor (1000)
        public static Vector2Int ConvertToInt(Vector2 v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns float converted to int according to default scaling factor (1000)
        public static int ConvertToInt(float v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns Vector2Int converted to Vector2 according to default scaling factor (1000)
        public static Vector2 ConvertToFloat(Vector2Int v)
        {
            float f = 1f / (float)CG.FloatToIntFactor;
            return new Vector2(v.x * f, v.y * f);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns int converted to float according to default scaling factor (1000)
        public static float ConvertToFloat(int v)
        {
            float f = 1f / (float)CG.FloatToIntFactor;
            return v * f;
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Returns true is segment AB intersects CD properly or improperly
        static public bool Intersects(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d)
        {
            return CG.Intersect(a, b, c, d);
        }


        //Get the shortest distance from a point to a line
        //Line is defined by the lineStart and lineEnd points
        public static float DistanceToLineSegment(Vector2Int point, Vector2Int lineStart, Vector2Int lineEnd)
        {
            return CG.DistanceToLineSegment(point, lineStart, lineEnd);
        }


        //Get the shortest distance from a point to a line
        //Line is defined by the lineStart and lineEnd points
        public static float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            return CG.DistanceToLineSegment(point, lineStart, lineEnd);
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Determines if a point is inside/on a CCW polygon and if so returns true. False otherwise.
        public static bool IsPointInPolygon(Vector2Int[] polyPts, Vector2Int point)
        {
            return CG.PointPolygonIntersectionType.Outside != CG.InPoly1(polyPts, point);
        }

        // Returns true iff p is strictly to the left of the directed
        // line through a to b.
        // You can use this method to determine if 3 adjacent CCW-ordered
        // vertices of a polygon form a convex or concave angle

        public static bool Left(Vector2Int a, Vector2Int b, Vector2Int p)
        {
            return CG.Left(a, b, p);
        }

        // Vector2 version of above
        public static bool Left(Vector2 a, Vector2 b, Vector2 p)
        {
            return CG.Left(CG.Convert(a), CG.Convert(b), CG.Convert(p));
        }




        // [helper method — not part of the original stub]
        // Minimum distance between two line SEGMENTS ab and cd, valid when the segments do
        // NOT cross. The closest approach of two non-intersecting segments always occurs at
        // an endpoint of one segment projected onto the other, so the minimum of the four
        // endpoint->segment distances is exact. Uses unscaled floats (per the assignment
        // hint that DistanceToLineSegment works best with floats). Handles degenerate
        // (zero-length) segments because DistanceToLineSegment returns the point distance
        // when the segment has zero length.
        static float SegmentToSegmentDistance(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float d0 = DistanceToLineSegment(a, c, d);
            float d1 = DistanceToLineSegment(b, c, d);
            float d2 = DistanceToLineSegment(c, a, b);
            float d3 = DistanceToLineSegment(d, a, b);

            return Mathf.Min(Mathf.Min(d0, d1), Mathf.Min(d2, d3));
        }


        // [helper method — not part of the original stub]
        // A node is valid (eligible to have edges) iff it lies inside the canvas rectangle
        // (inclusive) AND is not inside/on any obstacle. Nodes outside the boundary or
        // inside obstacles must not connect to anything.
        static bool IsNodeValid(Vector2 p, Vector2 canvasOrigin, float canvasWidth, float canvasHeight,
            List<Polygon> obstacles)
        {
            if (p.x < canvasOrigin.x || p.x > canvasOrigin.x + canvasWidth ||
                p.y < canvasOrigin.y || p.y > canvasOrigin.y + canvasHeight)
                return false;

            if (obstacles != null)
            {
                Vector2Int pi = ConvertToInt(p);

                foreach (var poly in obstacles)
                {
                    if (poly == null)
                        continue;

                    var ipts = poly.getIntegerPoints();

                    if (ipts == null || ipts.Length < 3)
                        continue;

                    if (IsPointInPolygon(ipts, pi))
                        return false;
                }
            }

            return true;
        }


        // [helper method — not part of the original stub]
        // Returns true iff an agent of radius agentRadius can travel in a straight line from
        // node a to node b: the segment must not cross any obstacle edge or boundary wall,
        // and must stay at least agentRadius away from every obstacle edge and boundary wall.
        static bool IsEdgeTraversable(Vector2 a, Vector2 b, List<Polygon> obstacles,
            Vector2[][] walls, float agentRadius)
        {
            Vector2Int ai = ConvertToInt(a);
            Vector2Int bi = ConvertToInt(b);

            // Obstacles: test against every edge of every obstacle.
            if (obstacles != null)
            {
                foreach (var poly in obstacles)
                {
                    if (poly == null)
                        continue;

                    var fpts = poly.getPoints();
                    var ipts = poly.getIntegerPoints();

                    if (ipts == null || ipts.Length < 3)
                        continue;

                    int len = ipts.Length;

                    for (int i = 0, j = len - 1; i < len; j = i++)
                    {
                        // Crossing test (integer): a positive clearance can still hide a
                        // segment that passes straight through the obstacle, so this is required.
                        if (Intersects(ai, bi, ipts[j], ipts[i]))
                            return false;

                        // Clearance test (float): the agent's body must clear the edge.
                        if (SegmentToSegmentDistance(a, b, fpts[j], fpts[i]) < agentRadius)
                            return false;
                    }
                }
            }

            // Boundary walls: same crossing + clearance tests.
            for (int w = 0; w < walls.Length; ++w)
            {
                Vector2 c = walls[w][0];
                Vector2 d = walls[w][1];

                if (Intersects(ai, bi, ConvertToInt(c), ConvertToInt(d)))
                    return false;

                if (SegmentToSegmentDistance(a, b, c, d) < agentRadius)
                    return false;
            }

            return true;
        }




        //Student code to build the path network from the given pathNodes and Obstacles
        //Obstacles - List of obstacles on the plane
        //agentRadius - the radius of the traversing agent
        //minPoVDist AND maxPoVDist - used for Points of Visibility (see assignment doc)
        //pathNodes - ref parameter that contains the pathNodes to connect (or return if pathNetworkMode is set to PointsOfVisibility)
        //pathEdges - out parameter that will contain the edges you build.
        //  Edges cannot intersect with obstacles or boundaries. Edges must be at least agentRadius distance
        //  from all obstacle/boundary line segments. No self edges, duplicate edges. No null lists (but can be empty)
        //pathNetworkMode - enum that specifies PathNetwork type (see assignment doc)

        public static void Create(Vector2 canvasOrigin, float canvasWidth, float canvasHeight,
            List<Polygon> obstacles, float agentRadius, float minPoVDist, float maxPoVDist, ref List<Vector2> pathNodes, out List<List<int>> pathEdges,
            PathNetworkMode pathNetworkMode)
        {

            // TODO IMPLEMENT  -->  [implemented]   (original stub returned: pathEdges as
            //   one empty List<int> per node, i.e. no edges at all)

            // One (initially empty) adjacency list per node. Never null; same length as pathNodes.
            pathEdges = new List<List<int>>(pathNodes.Count);

            for (int i = 0; i < pathNodes.Count; ++i)
            {
                pathEdges.Add(new List<int>());
            }

            int n = pathNodes.Count;

            if (n == 0)
                return;

            // The four canvas boundary walls, as barrier segments (world-space floats).
            Vector2 bl = new Vector2(canvasOrigin.x, canvasOrigin.y);
            Vector2 br = new Vector2(canvasOrigin.x + canvasWidth, canvasOrigin.y);
            Vector2 tr = new Vector2(canvasOrigin.x + canvasWidth, canvasOrigin.y + canvasHeight);
            Vector2 tl = new Vector2(canvasOrigin.x, canvasOrigin.y + canvasHeight);

            Vector2[][] walls = new Vector2[][]
            {
                new Vector2[] { bl, br },
                new Vector2[] { br, tr },
                new Vector2[] { tr, tl },
                new Vector2[] { tl, bl },
            };

            // Precompute node eligibility once.
            bool[] valid = new bool[n];

            for (int i = 0; i < n; ++i)
            {
                valid[i] = IsNodeValid(pathNodes[i], canvasOrigin, canvasWidth, canvasHeight, obstacles);
            }

            // Consider each unordered pair exactly once: no self edges, no duplicates.
            for (int i = 0; i < n; ++i)
            {
                if (!valid[i])
                    continue;

                for (int j = i + 1; j < n; ++j)
                {
                    if (!valid[j])
                        continue;

                    if (IsEdgeTraversable(pathNodes[i], pathNodes[j], obstacles, walls, agentRadius))
                    {
                        // Bidirectional edge.
                        pathEdges[i].Add(j);
                        pathEdges[j].Add(i);
                    }
                }
            }

            // END STUDENT CODE

        }


    }

}
