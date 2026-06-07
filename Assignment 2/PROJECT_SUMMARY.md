# Assignment 2 — Path Network Navigation — SUMMARY

**Submit file:** `Assets/Scripts/GameAIStudentWork/PathNetwork/CreatePathNetwork.cs`
**Due (AoE):** 2026-06-07 · **Threshold:** 70 · **Weight:** 10%

---

## Overview

Implemented `CreatePathNetwork.Create()` for `PathNetworkMode.Predefined`: given predefined
path nodes, polygon obstacles, a rectangular canvas, and an agent radius, it builds the
undirected path-network adjacency list (`pathEdges`) of all node pairs an agent can travel
between in a straight line without colliding with an obstacle or the world boundary.

## Approach

Two-stage filtering, with computational geometry done in the framework's discretized integer
space for crossing tests and in floats for distance (per the assignment hint):

1. **Node validity (precomputed once):** a node is eligible only if it is inside the canvas
   rectangle (inclusive) and not inside/on any obstacle (`IsPointInPolygon`). Invalid nodes
   get no edges.
2. **Per candidate edge `(A,B)`** between two valid nodes — reject unless **both** hold for
   every obstacle edge and every one of the 4 boundary walls:
   - **No crossing:** `CG.Intersect(A,B,C,D)` is false. (Required because a segment passing
     straight through an obstacle still has *positive* endpoint-to-edge distance, so the
     clearance test alone would miss it.)
   - **Clearance ≥ agentRadius:** `SegmentToSegmentDistance(A,B,C,D) >= agentRadius`, where
     the segment-to-segment distance is `min` of the four endpoint→segment distances
     (`DistanceToLineSegment`, floats). Rejection is strict `< agentRadius`, so an edge that
     clears by exactly the radius is kept.

Edges are added bidirectionally for each pair `i<j`, giving the required symmetry with no
self edges and no duplicates by construction.

## Method-by-method notes

| Method | Role |
|---|---|
| `Create()` | Builds one empty `List<int>` per node, precomputes node validity, loops unordered pairs `i<j`, adds bidirectional edges where `IsEdgeTraversable` is true. Early-returns for 0 nodes. |
| `IsNodeValid()` *(new helper)* | Inside-canvas + outside-all-obstacles test. |
| `IsEdgeTraversable()` *(new helper)* | Crossing + clearance test against all obstacle edges and the 4 boundary walls. |
| `SegmentToSegmentDistance()` *(new helper)* | Exact min distance between two non-crossing segments = min of 4 endpoint→segment distances; handles degenerate (coincident-node) edges. |

Provided helpers (`Intersects`, `DistanceToLineSegment`, `IsPointInPolygon`, `Convert*`,
`Left`) left as-is.

## Rubric compliance

- **Reachability:** crossing + agentRadius clearance vs every obstacle edge and boundary wall.
- **Graph characteristics:** lists never null (one per node, may be empty); `pathNodes.Count
  == pathEdges.Count`; pairs visited once → no duplicates, no self edges; both directions
  added → symmetric; only in-range indices added.
- **Coincident / arbitrarily-close nodes:** no min-length rejection; zero-length edges handled
  by `DistanceToLineSegment`'s l²==0 branch → they connect when in valid open space.

## Test results

Hand-validated against framework reference cases (`HardCodedPathNetworkCases.cs`) and
`ExampleTest`:

- **ExampleTest** — (4.5,0) is 0.5 < 1.0 from the right wall → no edge → both lists empty. ✓
- **pn1** — box at origin, r=0.5: axis edges 0–1 / 2–3 cross the box (rejected); 4 diagonals
  clear it by ≈0.92 ≥ 0.5 (kept) → `0:{2,3} 1:{2,3} 2:{0,1} 3:{0,1}`. ✓
- **pn2 tight case** — bottom edge clears the star's lowest vertex by exactly 0.2524 ≥ 0.25
  → kept (matches expected); diagonal 0–1 through obstacle1 center → rejected. ✓

Added EditMode tests in `PathNetworkTest.cs` (not submitted): `Pn1Test`,
`CoincidentNodesConnect`, `EmptyNodes`, plus a graph-characteristics assertion helper.

## Performance / complexity

`O(N² · E)` where `N` = node count and `E` = total obstacle edges (+4 walls). For autograder
scene sizes this is well under the ≥10s/test budget. No allocations in inner loop beyond the
edge lists. No prints / `Debug.Log`.

## Known limitations

- Points of Visibility (`PathNetworkMode.PointsOfVisibility`) not implemented (optional/ungraded).
- The "arbitrarily close nodes still connect" rule is honored for open-space nodes; it does
  not override obstacle/boundary clearance (consistent with the reachability requirement and
  all reference cases).

## Current Status / Submission checklist

- [x] `StudentAuthorName = "Zonghao Hou"`
- [x] `Create()` implemented (Predefined mode)
- [x] No `Debug.Log` / print statements
- [x] No `// compile_check` line (graded-ready). *Prepend `// compile_check` as line 1 for a
      compile-only autograder check; remove for the real graded run.*
- [x] Hand-validated vs reference cases (pn1, pn2 tight case, ExampleTest)
- [ ] Run Unity Test Runner (EditMode) to confirm new tests pass
- [ ] Submit `CreatePathNetwork.cs` only to Gradescope (UTF-8)
- [ ] Commit + push to GitHub after Test Runner validation

**Status:** Implemented; hand-validated against framework reference cases. Pending Unity Test
Runner run and Gradescope submission.
