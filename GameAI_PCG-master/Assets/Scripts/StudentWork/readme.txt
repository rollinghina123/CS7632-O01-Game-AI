Zonghao Hou

Terrain design: "Ridge & Rift"

My terrain divides the world into three visually distinct biomes: rolling sand dunes, terraced
mesa badlands, and jagged ridge mountains. A single low-frequency Perlin noise layer at the root
of the graph ("Biome Control", PerlinScalar 3.0) acts purely as a control signal: each of the
three biome nodes applies a TrapezoidFunction bandpass to this control noise (Dunes take control
values up to 0.42, Mesa 0.50-0.60, Mountains 0.68 and above) and multiplies the resulting mask
against its own sub-graph of detail noise. Adjacent trapezoids share overlapping fade regions
(0.42-0.50 and 0.60-0.68) so the biome masks crossfade linearly and always sum to one, producing
smooth transitions with no seams. Each biome is built from multiple octaves plus an intermittent
feature layer driven by a spike-shaped noise mapping curve: the Dunes combine a low base with
gentle swells (scalar 5.5), fine wind ripples (scalar 30), and scattered rock outcrops; the Mesa
uses a stair-step mapping curve on medium-frequency noise to carve four flat-topped terrace
levels, roughened with a rubble octave and occasional boulders so no surface is ever perfectly
flat; the Mountains sum two absolute-value (turbulence) octaves at scalars 4 and 8.5 for sharp
ridgelines, a fine crag octave, and rare tall rock spires. All height contributions are budgeted
so the final terrain stays comfortably inside the 0..1 heightmap range - no biome relies on
clipping against the minimum or maximum elevation, and the design tiles indefinitely since every
layer is pure noise-space evaluation.
