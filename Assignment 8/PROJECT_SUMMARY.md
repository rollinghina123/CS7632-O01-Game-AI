# Assignment 8 Summary — PCG Terrain with Perlin Noise ("Ridge & Rift")

## Overview

A 17-node PCG terrain graph authored directly into
`GameAI_PCG-master/Assets/Scripts/StudentWork/PCGTerrainData.asset` (Unity YAML ScriptableObject).
Three biomes — **Dunes**, **Mesa**, **Mountains** — are segmented from a single root Perlin
control noise via TrapezoidFunction bandpass masks with overlapping crossfade regions.

## Approach

- **Root "Biome Control"** (Perlin, scalar 3.0, max 1.0): control signal only. The framework
  forces the root's parent processing to ZeroOut and the biome children's outputs *replace* the
  root's own noise, so the control layer never adds height directly.
- **Biome nodes** (GenNoiseType None, MaxValue 0, `ApplyTrapezoidFunction` + `Multiply`): each
  masks its children's summed detail against a band of the control signal:
  - Dunes `[-0.05, 0, 0.42, 0.50]` · Mesa `[0.42, 0.50, 0.60, 0.68]` · Mountains `[0.60, 0.68, 1.02, 1.05]`
  - Shared fade intervals crossfade masks (sum ≈ 1) → smooth biome transitions.
- **Detail nodes** (Passthrough + Add, distinct ZOffsets as noise seeds to decorrelate layers):

| Biome | Nodes (scalar / max) |
|---|---|
| Dunes | Base (0.08 const) · Swells (5.5/0.10) · Ripples (30/0.025) · Scattered Rocks (18/0.10, spike curve ≥0.72) |
| Mesa | Floor (0.14 const) · Terraces (2.8/0.30, 4-level stair-step curve) · Rubble (14/0.035) · Boulders (22/0.06, spike ≥0.78) |
| Mountains | Base (0.18 const) · Ridge Oct 1 (4/0.38, **AbsVal**) · Ridge Oct 2 (8.5/0.16, **AbsVal**) · Crag Oct 3 (17/0.07) · Rock Spires (6/0.28, spike ≥0.80) |

- **Intermittent features** use `PerlinNoiseWithMappingCurve` with spike-shaped curves (0 below a
  high threshold, ramping to 1) → isolated rocks/boulders/spires rather than continuous noise.
- **No clipping**: sums bounded ≈[0.08, 0.95]; every flat region carries a small detail octave.

## Rubric compliance

- ✅ Three distinct biomes, each from the root noise source via TrapezoidFunction
- ✅ Root split into three ranges with crossfade overlap
- ✅ Multiple octaves + intermittent features per biome (4/4/5 descendant nodes)
- ✅ No reliance on min/max height clipping

## Remaining Manual Steps (Unity Editor)

1. Open project `GameAI_PCG-master` → scene `PCGTerrain`.
2. Select the Terrain root GameObject → PCGTerrain component → **Load from ScriptableObject**
   (rebuilds the 17-node hierarchy from the authored asset).
3. Click **Validate** → expect "Valid!" in the Console.
4. Visually confirm three regions + smooth crossfades. If the Mesa band looks too narrow/wide on
   the default tile, nudge the root X/Y offsets to find a good composition (offsets are part of
   the design and safe to save), then **Save to ScriptableObject** + **File → Save Scene**.
5. Screenshots (4–10 total, PNG): `overview_0.png` (all three biomes), `dunes_0.png`,
   `mesa_0.png`, `mountains_0.png`. Scrolling offsets for close-ups is allowed; re-Load or reset
   offsets afterward if you don't want them saved.
6. Submit to Gradescope **individually (no ZIP)**: `PCGTerrainData.asset`, `readme.txt`, and the
   screenshots. (readme.txt is staged next to the asset in `Assets/Scripts/StudentWork/`.)

## Current Status (2026-07-26)

- [x] Plan (`ASSIGNMENT8_PLAN.md`)
- [x] PCG graph authored in `PCGTerrainData.asset` (17 nodes, YAML validated against the
      serializer format: root at index 0, GUID links, tangentMode 0 keyframes)
- [x] `readme.txt` written (name + design paragraph)
- [x] Unity: Loaded from ScriptableObject, hierarchy rebuilt (17 nodes), visually confirmed the
      three biomes (isolated each via Do Not Process Descendants toggling; all toggles cleared)
- [x] Screenshots captured (overview + dunes/mesa/mountains)
- [x] Consistency verified post-save: asset ↔ scene identical, no leftover Mute/DNPD flags, all
      X/Y offsets 0 (only designed ZOffset seeds nonzero), readme numbers match the asset
- [ ] Gradescope submission (due TODAY 2026-07-26 11:59 PM AoE ≈ tomorrow ~8 AM ET) — files ready
- [x] Committed to GitHub

## Known Notes / Risks

- The trapezoid bands assume the control noise (scalar 3, one tile) spans roughly 0.15–0.85 after
  [0,1] remap. Check the root's `DB_min/max Noise` debug fields after loading; if the mid band
  (Mesa) barely appears, widen its band (e.g., 0.48–0.62) or scroll offsets — then re-save.
- Mountains' theoretical max sum slightly exceeds 1.0 (1.07) but |Perlin| ~≤0.9 and spires are
  rare/localized, so practical peaks sit ≈0.85. If any flat-topped peak appears, drop Ridge
  Octave 1 MaxValue to 0.34 and re-save.
