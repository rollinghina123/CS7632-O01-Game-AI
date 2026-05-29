# Assignment 1: Grid Lattice — Plan

## Problem Statement

Superimpose a discretized grid lattice over an arbitrary 2D game terrain containing static obstacles,
so an agent can path-plan without ever colliding with an obstacle. The grid is a 2D `bool[,]` where
`true` = navigable and `false` = blocked. A cell is blocked if an obstacle overlaps the cell's
**interior** — merely *touching* an edge stays traversable (the "shrink method"). Also implement the
traversability query used to build graph adjacency.

- **Submit file:** `Assets/Scripts/GameAIStudentWork/GridNavigation/CreateGrid.cs`
- **Threshold:** 60 (below this does not count as a submission)
- **Autograder:** tests grid cells against convex/concave obstacle polygons across several maps.

## Spec Mapping

| Method | Required behavior |
|--------|-------------------|
| `IsPointInsideAxisAlignedBoundingBox(min,max,p)` | True if `p` is on/inside the box (inclusive on edges) |
| `IsRangeOverlapping(min1,max1,min2,max2)` | True if two inclusive ranges share ≥1 value |
| `IsAxisAlignedBoundingBoxOverlapping(min1,max1,min2,max2)` | Range overlap on both x and y |
| `IsTraversable(grid,x,y,dir)` (public, autograded) | Validates grid/coords; source + neighbor must be traversable; boundaries not traversable; diagonals ignore orthogonal neighbors |
| `Create(origin,w,h,cellWidth,obstacles,out grid)` (public, autograded) | Build the `bool[x,y]` grid using the shrink method |

## Method / Approach

### Grid sizing (`Create`)
- `grid_size_x = FloorToInt(canvasWidth / cellWidth)`, `grid_size_y = FloorToInt(canvasHeight / cellWidth)`.
- Clamp each to a minimum of 1.
- `grid = new bool[grid_size_x, grid_size_y]`; `i` indexes +x, `j` indexes +y (j is contiguous/major axis).
- Cell `[i,j]` world bounds: `origin + (i·cw, j·cw)` to `origin + ((i+1)·cw, (j+1)·cw)`; `grid[0,0]`
  bottom-left == `canvasOrigin`.

### Shrink method (interior-only overlap)
- Convert each cell's world bounds to integer space via `CG.Convert` (×1000).
- **Shrink** the integer cell by 1 unit per side (`min+1`, `max-1`). This excludes obstacles that only
  touch the original boundary (touching = traversable) while catching any real interior penetration
  (≥ 0.001 world units).

### Obstacle-vs-cell overlap (3 classic cases)
For each obstacle polygon (with an AABB quick-reject using `MinIntBounds`/`MaxIntBounds` vs the shrunk cell):
1. **Obstacle vertex inside cell** → `IsPointInsideAxisAlignedBoundingBox`.
2. **Cell corner inside obstacle** → `IsPointInsidePolygon` (`CG.InPoly1 != Outside`) for all 4 corners
   (handles a cell fully contained by a large obstacle).
3. **Edge crossing** → `Intersects` (`CG.Intersect`) for each cell edge × each obstacle edge.
Any hit ⇒ cell is blocked (`false`).

### `IsTraversable`
- Return false if: grid null, `Rank != 2`, any dimension 0, `x/y` out of range, or `grid[x,y]` is false.
- Compute neighbor by `dir` (Up=+y, Down=−y, Right=+x, Left=−x; diagonals combine).
- Return false if neighbor out of bounds (boundaries not traversable); else return `grid[neighbor]`.
- Diagonals: only current + diagonal cell matter (do **not** require orthogonal neighbors), per spec.

## Files

| File | Purpose | Submit? |
|------|---------|---------|
| `CreateGrid.cs` | Main implementation | **Gradescope** |
| `GridTest.cs` | NUnit EditMode tests (extend with own cases) | No |

## Provided Test Cases (`GridTest.cs`)

| Test | Setup | Expected |
|------|-------|----------|
| `TestEmptyGrid` | no obstacles, various cell sizes | all cells `true`; dims = `Floor(w/cs)`,`Floor(h/cs)` |
| `TestObstacleThatNearlyFillsCanvas` | one obstacle inset 0.1 from canvas | all cells `false` |
| `TestTraversableWithSingleGridCell` | 1×1 grid, cell true/false | every direction `false` (neighbor out of bounds) |

## Validation Checklist
- [ ] All 3 provided tests reasoned through / pass in Test Runner
- [ ] `StudentAuthorName` set to real name
- [ ] No `Debug.Log` / print statements
- [ ] `// compile_check` submission is clean
- [ ] Removed `// compile_check` for graded run
