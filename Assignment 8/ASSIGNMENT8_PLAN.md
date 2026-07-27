# Assignment 8 Plan — PCG Terrain with Perlin Noise

## Problem Statement

Use the provided PCG generator (`PCGTerrain` component graph) to design a heightmap terrain with
**three visually distinct biomes**, segmented from a **root Perlin noise source** via
TrapezoidFunction (or mapping curve) masks. Save the node graph to `PCGTerrainData.asset`
(ScriptableObject) and submit it with a `readme.txt` and 4–10 screenshots.

Due: 2026-07-26 11:59 PM AoE. Threshold: 70.

## Rubric / Spec Mapping

| Requirement (PDF) | Design response |
|---|---|
| Three distinct areas/biomes | Dunes (low), Mesa (mid), Mountains (high) |
| Each biome generated from a root noise source via TrapezoidFunction or MappingCurve | Root "Biome Control" Perlin node; each biome node = `ApplyTrapezoidFunction` on parent + `Multiply` combine |
| Split root noise into three separate ranges (or hierarchically) | Three trapezoid bands over control values: ≤0.50 / 0.42–0.68 / ≥0.60 |
| Crossfade for smooth transitions | Adjacent bands share fade regions (0.42–0.50 and 0.60–0.68); linear fades sum to ~1 |
| Multiple octaves of Perlin noise per biome | Each biome has 2–3 noise octaves at different PerlinScalar frequencies |
| Intermittent features (rocks, spires, etc.) | Spike-shaped `GenNoiseCurve` mapping (0 below threshold → 1 near max) yields isolated features: Scattered Rocks (dunes), Boulders (mesa), Rock Spires (mountains) |
| Multiple descendant nodes per biome | 4 / 4 / 5 child nodes respectively |
| No reliance on min/max clipping | Height sums bounded ≈[0.08, 0.95]; every flat-ish area still carries a small detail octave |

## How the Framework Evaluates the Graph (from PCGTerrain.cs)

- Each node generates its own Perlin layer (`perlinMat`) → copied to `mat`.
- Its **children receive `mat` as their parent input**; the children's outputs are **summed and
  replace** `mat` (first child copied, remainder added).
- The node then processes its **own parent's** heights via `ProcessParentType`
  (TrapezoidFunction ⇒ 0/fade/1 mask) into `parentMat`, and combines: `CombineType` (Multiply ⇒
  `mask × children-sum`).
- Root's parent is forced `ZeroOut`, so the root's own noise is **only** a control signal consumed
  by the biome nodes' trapezoids — it never adds height directly.
- Trapezoid vars `[lowest, low, high, highest]`: fade-in over [lowest,low], 1 inside [low,high],
  fade-out over [high,highest], 0 outside.
- Offsets accumulate parent→child; `ZOffset` acts as a seed (3rd noise dimension) — each child
  gets a distinct ZOffset to decorrelate octaves.
- Serialization: flat `Config` list in the ScriptableObject; **root must be element 0**;
  parent→child links via `PCGConfigChildren` GUID lists. "Load from ScriptableObject" rebuilds the
  scene hierarchy from the asset, so the asset can be authored directly and loaded.

## Graph Design (17 nodes)

```
Biome Control (root)         Perlin, scalar 3.0, max 1.0            [control signal only]
├── Dunes                    None/0, Trapezoid [-0.05, 0, 0.42, 0.50], Multiply
│   ├── Dune Base            None, max 0.08                         [floor]
│   ├── Dune Swells          Perlin, scalar 5.5,  max 0.10          [octave 1]
│   ├── Dune Ripples         Perlin, scalar 30,   max 0.025         [octave 2]
│   └── Scattered Rocks      Perlin+curve, scalar 18, max 0.10      [intermittent: spike ≥0.72]
├── Mesa                     None/0, Trapezoid [0.42, 0.50, 0.60, 0.68], Multiply
│   ├── Mesa Floor           None, max 0.14                         [floor]
│   ├── Mesa Terraces        Perlin+curve, scalar 2.8, max 0.30     [stair-step curve: 4 levels]
│   ├── Surface Rubble       Perlin, scalar 14, max 0.035           [octave: keeps floors non-flat]
│   └── Boulders             Perlin+curve, scalar 22, max 0.06      [intermittent: spike ≥0.78]
└── Mountains                None/0, Trapezoid [0.60, 0.68, 1.02, 1.05], Multiply
    ├── Mountain Base        None, max 0.18                         [floor]
    ├── Ridge Octave 1       Perlin ABS, scalar 4,   max 0.38       [turbulence ridges]
    ├── Ridge Octave 2       Perlin ABS, scalar 8.5, max 0.16       [turbulence ridges]
    ├── Crag Octave 3        Perlin, scalar 17, max 0.07            [fine detail]
    └── Rock Spires          Perlin+curve, scalar 6, max 0.28       [intermittent: spike ≥0.80]
```

Height budgets (worst-case sums): Dunes ≤ ~0.31, Mesa ≤ ~0.54, Mountains ≤ ~1.07 theoretical but
|Perlin| rarely exceeds ~0.9 and the spike curve is rare ⇒ practical peaks ~0.85. Minimum ≥ 0.08.
No clipping at 0 or 1.

## Implementation Steps

1. Author `Assets/Scripts/StudentWork/PCGTerrainData.asset` YAML directly: 17 configs, root at
   index 0 (keeps the existing root GUID so the scene binding is untouched), children linked by
   fresh v4 GUIDs, curves as serializedVersion-3 keyframes with `tangentMode: 0`
   (SerializableCurve round-trips time/value/slopes/weights only).
2. Write `readme.txt` (name + design paragraph) beside the asset.
3. In Unity: open `PCGTerrain` scene → select Terrain root → **Load from ScriptableObject** →
   **Validate** (expect "Valid!") → visually confirm the three biomes.
4. Screenshots (4–10): `overview_0.png` (all biomes), `dunes_0.png`, `mesa_0.png`,
   `mountains_0.png` — scroll X/Y offsets for good angles if needed, then **re-Load** (do NOT
   save offset changes over the design; or set offsets back before saving).
5. Submit individually (no ZIP): `PCGTerrainData.asset`, `readme.txt`, the four+ screenshots.

## Validation Plan

- Unity `Validate` button prints "Valid!" (asset ↔ hierarchy consistency).
- Visual: three regions visible in one tile at offsets (0,0); crossfades smooth; no flat clipped
  areas; mountains show ridged turbulence; mesa shows terrace steps; dunes low and rolling.
- Debug fields `DB_min/max Noise` on the root confirm control noise spans well across the three
  trapezoid bands (expect roughly 0.15–0.85).
- Check tiling: nudge X Offset ±1 tile — biome boundaries continue seamlessly (offsets are
  additive in noise space, so tiles are inherently consistent).
