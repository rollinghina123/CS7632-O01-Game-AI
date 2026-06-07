# Assignment 2 — Path Network Navigation — PLAN

**Submit file:** `Assets/Scripts/GameAIStudentWork/PathNetwork/CreatePathNetwork.cs`
**Due (AoE):** 2026-06-07 · **Threshold:** 70 · **Weight:** 10%

---

## Problem Statement

Given a continuous terrain with polygon obstacles, a rectangular canvas boundary, an agent
radius, and a set of **predefined** path nodes, generate the **path network**: the list of
valid edges (by node index) connecting nodes that an agent of the given radius can travel
between in a straight line without colliding with an obstacle or the world boundary.

We only implement `PathNetworkMode.Predefined` (Points of Visibility is optional/ungraded).

## Spec / Rubric Mapping

| Rubric item | How we satisfy it |
|---|---|
| **Reachability** — agent can traverse any edge w/o hitting obstacle or boundary | Per candidate edge: (1) no segment intersection with any obstacle edge or boundary wall; (2) min segment-to-segment distance ≥ `agentRadius` from every obstacle edge AND boundary wall |
| No edges out of bounds / inside obstacles | Node validity precheck: node must be inside the canvas rectangle and **not** inside/on any obstacle; both endpoints must be valid for an edge to be considered |
| **Bidirectional** edges | Iterate pairs `i<j`; on success add `j→edges[i]` AND `i→edges[j]` |
| **No duplicate** edges | Each unordered pair visited exactly once (`j` starts at `i+1`) |
| **No self edges** | `j` starts at `i+1`, never `i==j` |
| Edge lists never null (empty allowed) | Initialize `pathEdges` with one empty `List<int>` per node up front |
| No non-existent node indexes | Only indices in `[0,n)` are ever added |
| `pathNodes.Count == pathEdges.Count` | One list per node, always |
| Arbitrarily close / coincident nodes still connect | Degenerate (zero-length) candidate edges handled by `DistanceToLineSegment` (l²==0 → point distance); no minimum-edge-length rejection |

## Method

For each candidate edge between two **valid** nodes `A`, `B`:

1. **Intersection test (integer space):** for every obstacle edge `CD` and every boundary
   wall `CD`, reject if `CG.Intersect(A,B,C,D)` (catches edges that cross through an
   obstacle/boundary — the clearance formula alone cannot, since a crossing yields a
   *positive* endpoint-to-segment distance).
2. **Clearance test (float space):** reject if the minimum distance between segment `AB`
   and any obstacle edge / boundary wall is `< agentRadius`. Min distance between two
   non-crossing segments = `min` of the four endpoint→segment distances
   (`DistanceToLineSegment`). Uses unscaled floats per the PDF hint and the float
   `agentRadius`. Rejection is **strict less-than** (`dist < agentRadius`), so an edge
   that clears by exactly `agentRadius` is kept.

**Node validity** (precomputed once): inside canvas rectangle `[origin, origin+size]`
(inclusive) AND `IsPointInPolygon` is false for every obstacle (inside/on-edge → invalid).

Boundary walls are the 4 canvas edges, treated as barrier segments for both tests. A node
that is inside the canvas but within `agentRadius` of a wall therefore gets **no** edges
(its edges fail wall clearance) — matching the provided `ExampleTest`.

## Validation (hand-checked against framework reference cases)

- **`ExampleTest`** (origin (-5,-5), 10×10, r=1, nodes (0,0)&(4.5,0)): (4.5,0) is 0.5 < 1.0
  from the right wall → edge fails wall clearance → both lists empty. ✓
- **`pn1`** (1.2×1.2 box at origin, r=0.5, nodes at ±2.5 on axes): straight-through edges
  (0–1 horizontal, 2–3 vertical) intersect the box → rejected; 4 diagonals clear the box
  by ≈0.92 ≥ 0.5 → kept. Expected `0:{2,3} 1:{2,3} 2:{0,1} 3:{0,1}`. ✓
- **`pn2`** tight case: bottom edge node0(-14.5,-14.5)→node2(-5.5,-14.5) clears the star's
  lowest vertex (-12.447,-14.248) by exactly 0.2524 ≥ 0.25 = `agentRadius` → kept (matches
  expected `0:{...,2,...}`). Confirms float clearance + strict `<` rejection. ✓
- **`pn2`** diagonal node0→node1 passes through obstacle1's center (-7.5,-7.5) → intersects →
  rejected (matches expected: 1 ∉ edges[0]). ✓

## Files to submit

- `CreatePathNetwork.cs` only. (Do **not** submit `PathNetworkTest.cs` / `CustomPresetConfig.cs`.)
- Remove all `Debug.Log` / prints. UTF-8 encoding. No `// compile_check` for a graded run.
