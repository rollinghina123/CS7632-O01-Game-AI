// compile_check
// Remove the line above if you are subitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAICourse {

    public class CreateGrid
    {

        // Please change this string to your name
        public const string StudentAuthorName = "Zonghao Hou";


        // Helper method provided to help you implement this file. Leave as is.
        // Returns true if point p is inside (or on edge) the polygon defined by pts (CCW winding). False, otherwise
        static bool IsPointInsidePolygon(Vector2Int[] pts, Vector2Int p)
        {
            return CG.InPoly1(pts, p) != CG.PointPolygonIntersectionType.Outside;
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Returns float converted to int according to default scaling factor (1000)
        static int Convert(float v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns Vector2 converted to Vector2Int according to default scaling factor (1000)
        static Vector2Int Convert(Vector2 v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns true if segment AB intersects CD properly or improperly
        static bool Intersects(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d)
        {
            return CG.Intersect(a, b, c, d);
        }


        // IsPointInsideAxisAlignedBoundingBox(): Determines whether a point (Vector2Int:p) is On/Inside a bounding box (such as a grid cell) defined by
        // minCellBounds and maxCellBounds (both Vector2Int's).
        // Returns true if the point is ON/INSIDE the cell and false otherwise
        // This method should return true if the point p is on one of the edges of the cell.
        // This is more efficient than PointInsidePolygon() for an equivalent dimension poly
        // Preconditions: minCellBounds <= maxCellBounds, per dimension
        static bool IsPointInsideAxisAlignedBoundingBox(Vector2Int minCellBounds, Vector2Int maxCellBounds, Vector2Int p)
        {
            // TODO IMPLEMENT  -->  [implemented]   (original stub returned: true)
            // On/inside test is inclusive on all four edges.
            return p.x >= minCellBounds.x && p.x <= maxCellBounds.x &&
                   p.y >= minCellBounds.y && p.y <= maxCellBounds.y;
        }




        // IsRangeOverlapping(): Determines if the range (inclusive) from min1 to max1 overlaps the range (inclusive) from min2 to max2.
        // The ranges are considered to overlap if one or more values is within the range of both.
        // Returns true if overlap, false otherwise.
        // Preconditions: min1 <= max1 AND min2 <= max2
        static bool IsRangeOverlapping(int min1, int max1, int min2, int max2)
        {
            // TODO IMPLEMENT  -->  [implemented]   (original stub returned: true)
            // Two inclusive ranges overlap unless one ends before the other starts.
            return min1 <= max2 && min2 <= max1;
        }

        // IsAxisAlignedBoundingBoxOverlapping(): Determines if the AABBs defined by min1,max1 and min2,max2 overlap or touch
        // Returns true if overlap, false otherwise.
        // Preconditions: min1 <= max1, per dimension. min2 <= max2 per dimension
        static bool IsAxisAlignedBoundingBoxOverlapping(Vector2Int min1, Vector2Int max1, Vector2Int min2, Vector2Int max2)
        {
            // TODO IMPLEMENT  -->  [implemented]   (original stub returned: true)
            // HINT (from stub): Call IsRangeOverlapping()
            // Boxes overlap iff their projections onto both axes overlap.
            return IsRangeOverlapping(min1.x, max1.x, min2.x, max2.x) &&
                   IsRangeOverlapping(min1.y, max1.y, min2.y, max2.y);
        }




        // Returns true if the obstacle polygon overlaps the INTERIOR of the (already shrunk)
        // axis-aligned cell defined by sMin/sMax. "Just touching" the cell boundary does not
        // count because the cell has been shrunk by the caller (the "shrink method").
        //
        // Polygon-vs-box overlap is detected by the three classic cases:
        //   1. an obstacle vertex lies inside the cell box
        //   2. a cell corner lies inside the obstacle polygon (cell fully contained by obstacle)
        //   3. an obstacle edge crosses a cell edge
        static bool IsObstacleOverlappingCell(Vector2Int sMin, Vector2Int sMax, Polygon poly)
        {
            // [helper method — not part of the original stub; recommended by the
            //  assignment as an optional helper for Create().]
            var pts = poly.getIntegerPoints();

            if (pts == null || pts.Length < 3)
                return false;

            // Quick rejection: if the obstacle's bounding box doesn't even touch the cell,
            // there is no way it overlaps the (smaller) shrunk cell interior.
            if (!IsAxisAlignedBoundingBoxOverlapping(sMin, sMax, poly.MinIntBounds, poly.MaxIntBounds))
                return false;

            // Cell corners, CCW winding
            Vector2Int c0 = new Vector2Int(sMin.x, sMin.y);
            Vector2Int c1 = new Vector2Int(sMax.x, sMin.y);
            Vector2Int c2 = new Vector2Int(sMax.x, sMax.y);
            Vector2Int c3 = new Vector2Int(sMin.x, sMax.y);

            // Case 1: any obstacle vertex inside the cell box
            for (int i = 0; i < pts.Length; ++i)
            {
                if (IsPointInsideAxisAlignedBoundingBox(sMin, sMax, pts[i]))
                    return true;
            }

            // Case 2: any cell corner inside the obstacle polygon
            if (IsPointInsidePolygon(pts, c0) ||
                IsPointInsidePolygon(pts, c1) ||
                IsPointInsidePolygon(pts, c2) ||
                IsPointInsidePolygon(pts, c3))
                return true;

            // Case 3: any cell edge intersects any obstacle edge
            Vector2Int[] cell = { c0, c1, c2, c3 };

            for (int ci = 0, cj = cell.Length - 1; ci < cell.Length; cj = ci++)
            {
                Vector2Int a = cell[cj];
                Vector2Int b = cell[ci];

                for (int pi = 0, pj = pts.Length - 1; pi < pts.Length; pj = pi++)
                {
                    if (Intersects(a, b, pts[pj], pts[pi]))
                        return true;
                }
            }

            return false;
        }




        // IsTraversable(): returns true if the grid is traversable from grid[x,y] in the direction dir, false otherwise.
        // The grid boundaries are not traversable. If the grid position x,y is itself not traversable but the grid cell in direction
        // dir is traversable, the function will return false.
        // returns false if the grid is null, grid rank is not 2 dimensional, or any dimension of grid is zero length
        // returns false if x,y is out of range
        // Note: public methods are autograded
        public static bool IsTraversable(bool[,] grid, int x, int y, TraverseDirection dir)
        {
            // TODO IMPLEMENT  -->  [implemented]   (original stub returned: true)
            // Validate the grid itself
            if (grid == null || grid.Rank != 2)
                return false;

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            if (width == 0 || height == 0)
                return false;

            // Validate the source cell coordinates
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            // The source cell must itself be traversable
            if (!grid[x, y])
                return false;

            // Compute the neighbor cell in the requested direction.
            // Up/Down is the Y axis (Up positive). Left/Right is the X axis (Right positive).
            int nx = x;
            int ny = y;

            switch (dir)
            {
                case TraverseDirection.Up:
                    ny += 1;
                    break;
                case TraverseDirection.Down:
                    ny -= 1;
                    break;
                case TraverseDirection.Left:
                    nx -= 1;
                    break;
                case TraverseDirection.Right:
                    nx += 1;
                    break;
                case TraverseDirection.UpLeft:
                    nx -= 1;
                    ny += 1;
                    break;
                case TraverseDirection.UpRight:
                    nx += 1;
                    ny += 1;
                    break;
                case TraverseDirection.DownLeft:
                    nx -= 1;
                    ny -= 1;
                    break;
                case TraverseDirection.DownRight:
                    nx += 1;
                    ny -= 1;
                    break;
                default:
                    return false;
            }

            // Neighbor must be in range (boundaries are not traversable)
            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                return false;

            // Neighbor must be traversable. For diagonals we deliberately do NOT
            // consider the adjacent orthogonal cells (per the assignment spec).
            return grid[nx, ny];
        }


        // Create(): Creates a grid lattice discretized space for navigation.
        // canvasOrigin: bottom left corner of navigable region in world coordinates
        // canvasWidth: width of navigable region in world dimensions
        // canvasHeight: height of navigable region in world dimensions
        // cellWidth: target cell width (of a grid cell) in world dimensions
        // obstacles: a list of collider obstacles
        // grid: an array of bools. A cell is true if navigable, false otherwise
        //    Example: grid[x_pos, y_pos]

        public static void Create(Vector2 canvasOrigin, float canvasWidth, float canvasHeight, float cellWidth,
            List<Polygon> obstacles,
            out bool[,] grid
            )
        {
            // TODO IMPLEMENT  -->  [implemented]   (original stub returned: grid = new bool[1,1]; grid[0,0] = true;)
            // Largest integer number of cells that fit within the canvas dimensions.
            int gridSizeX = Mathf.FloorToInt(canvasWidth / cellWidth);
            int gridSizeY = Mathf.FloorToInt(canvasHeight / cellWidth);

            // Per spec: a zero dimension is clamped to a minimum of 1.
            if (gridSizeX < 1) gridSizeX = 1;
            if (gridSizeY < 1) gridSizeY = 1;

            grid = new bool[gridSizeX, gridSizeY];

            // Shrink amount (in discretized integer units) applied to each cell so that
            // obstacles that merely touch a cell edge/corner do NOT block it. This is the
            // "shrink method" from the grid lattice lecture. 1 unit == 0.001 world units.
            const int shrink = 1;

            // j is the memory-contiguous (major) axis, so iterate it innermost for cache coherency.
            for (int i = 0; i < gridSizeX; ++i)
            {
                for (int j = 0; j < gridSizeY; ++j)
                {
                    // World-space bounds of this cell. grid[0,0]'s bottom-left == canvasOrigin.
                    float lx = canvasOrigin.x + i * cellWidth;
                    float by = canvasOrigin.y + j * cellWidth;
                    float rx = canvasOrigin.x + (i + 1) * cellWidth;
                    float ty = canvasOrigin.y + (j + 1) * cellWidth;

                    // Convert to discretized integer space.
                    Vector2Int cellMin = Convert(new Vector2(lx, by));
                    Vector2Int cellMax = Convert(new Vector2(rx, ty));

                    // Shrunk cell bounds.
                    Vector2Int sMin = new Vector2Int(cellMin.x + shrink, cellMin.y + shrink);
                    Vector2Int sMax = new Vector2Int(cellMax.x - shrink, cellMax.y - shrink);

                    bool navigable = true;

                    if (obstacles != null)
                    {
                        for (int o = 0; o < obstacles.Count; ++o)
                        {
                            var poly = obstacles[o];

                            if (poly == null)
                                continue;

                            if (IsObstacleOverlappingCell(sMin, sMax, poly))
                            {
                                navigable = false;
                                break;
                            }
                        }
                    }

                    grid[i, j] = navigable;
                }
            }
        }

    }

}
