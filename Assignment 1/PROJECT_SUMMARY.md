# Assignment 1: Grid Lattice — Project Summary

## Current Status

**Validated locally (8/8 Test Runner cases pass) — pending Gradescope submission** (as of 2026-05-29).

- [x] All 5 methods implemented in `CreateGrid.cs`
- [x] Logic reasoned through against the 3 provided `GridTest.cs` cases
- [x] Unity Test Runner (EditMode): **8/8 cases pass** (TestName, TestEmptyGrid ×3, TestObstacleThatNearlyFillsCanvas ×2, TestTraversableWithSingleGridCell ×2)
- [x] `StudentAuthorName` set to `"Zonghao Hou"` (name test verified)
- [x] No `Debug.Log`/print statements
- [ ] Compile-check submission on Gradescope (`// compile_check` present at line 1)
- [ ] Remove `// compile_check` and submit `CreateGrid.cs` for grade
- [ ] Commit + push to GitHub after validation

## Overview

`CreateGrid.cs` discretizes a 2D terrain into a navigable `bool[,]` grid using the **shrink method**:
each grid cell is shrunk slightly before obstacle testing so obstacles that merely touch a cell edge
leave it traversable, while any interior penetration marks it blocked. Implementation lives in
`GameAIPathPlanning-master/Assets/Scripts/GameAIStudentWork/GridNavigation/CreateGrid.cs`.

## Method-by-Method Summary

| Method | Implementation |
|--------|---------------|
| `IsPointInsideAxisAlignedBoundingBox` | Inclusive bounds test on x and y |
| `IsRangeOverlapping` | `min1 <= max2 && min2 <= max1` |
| `IsAxisAlignedBoundingBoxOverlapping` | Range overlap on both axes |
| `IsObstacleOverlappingCell` (private helper) | AABB quick-reject, then 3 cases: obstacle-vertex-in-cell, cell-corner-in-obstacle, edge-crossing |
| `IsTraversable` | Full grid/coord validation; source + neighbor traversable; boundary = blocked; diagonals ignore orthogonal cells |
| `Create` | `FloorToInt` cell counts (min 1); per-cell shrink by 1 integer unit; mark blocked on any obstacle overlap |

## Algorithm Detail

- **Discretization:** world coordinates → integers via `CG.Convert` (×1000) so all geometry tests use
  exact integer arithmetic.
- **Shrink amount:** 1 integer unit (= 0.001 world units) per side. Threshold behavior: obstacle on the
  boundary → traversable; penetration ≥ 1 unit → blocked.
- **Overlap completeness:** vertex-in-box + corner-in-poly + edge-cross is the standard polygon–box
  overlap test and covers obstacle-inside-cell, cell-inside-obstacle, and partial-cross cases.

## Provided-Test Verification (on paper)

| Test | Result |
|------|--------|
| `TestEmptyGrid` (1×1, 0.25, 0.26 cell sizes) | All `true`; dims `Floor(w/cs)` ✓ |
| `TestObstacleThatNearlyFillsCanvas` (cs 1 and 0.25) | All `false` ✓ |
| `TestTraversableWithSingleGridCell` (true/false) | All directions `false` (neighbor out of bounds) ✓ |

## Performance

- **Time:** O(cells × obstacles × (V + E)) worst case; AABB quick-reject prunes far cells.
- **Space:** O(grid_size_x × grid_size_y) for the output array.
- Well within the autograder's ≥ 10 s per-test budget for the provided map sizes.

## Known Considerations / Limitations

- Shrink of 1 unit assumes the reference solution treats sub-0.001-world penetration as non-blocking;
  if a map's reference disagrees on a knife-edge boundary cell, revisit the shrink amount.
- `IsPointInsidePolygon` treats edge/vertex containment as "inside" — acceptable here because the cell
  is already shrunk, so any such contact is genuine interior overlap.
- Assumes obstacle polygons are valid CCW (as the framework guarantees).

## Files

```
Assignment 1/
├── ASSIGNMENT1_PLAN.md     # Plan with spec mapping
└── PROJECT_SUMMARY.md      # This file

GameAIPathPlanning-master/Assets/Scripts/GameAIStudentWork/
├── GridNavigation/CreateGrid.cs   # Implementation (submit to Gradescope)
└── Tests/GridTest.cs              # Provided + custom NUnit tests
```

## Submission Checklist

- [ ] `StudentAuthorName` = real name
- [ ] Unity Test Runner: provided tests pass
- [ ] No `Debug.Log`/print statements
- [ ] Compile-check submission clean (`// compile_check` line 1)
- [ ] Remove `// compile_check`, submit `CreateGrid.cs` for grade (only this file)
- [ ] Record Gradescope score below, then commit + push
- [ ] Gradescope score round 1: ____  | round 2: ____
