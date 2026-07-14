# Assignment 7 — Fuzzy Logic Racetrack — PROJECT SUMMARY

**Author:** Zonghao Hou · **Course:** CS7632 O01 Game AI (Summer 2026) · **Due:** 07/19 11:59 PM AoE
**Submit:** `FuzzyVehicle.cs` only (in `GameAIRacetrack-main/Assets/Scripts/GameAIStudentWork/`)

## Overview
A Fuzzy Logic racing agent for the Arcade Car. All control comes exclusively from fuzzy rules via
`ApplyFuzzyRules(...)` — no direct Throttle/Steering writes, no hardcoded controls, no `INTERNAL_`
access, no conditional decision logic in crisp-input preparation.

## Approach

### Crisp inputs (pure geometry/kinematics)
1. **Steer error (deg)** — signed angle from vehicle forward to a centerline point
   `5 + 0.7·Speed` m ahead (`Racetrack.GetPointAhead`); "steer toward a future point."
2. **Speed (kph)** — `Speed_kph`.
3. **Accumulated bend (deg ≥ 0)** — sum of |tangent direction changes| across 5 samples spanning a
   `10 + 2.0·Speed` m window ahead. Summing absolute deltas is essential: a plain now-vs-ahead
   angle cancels on S-curves (this exact bug caused two wipeouts on WindingRaceTrack before the fix).

### Fuzzy variables
- `FzInputSteerError` {HardLeft, Left, Straight, Right, HardRight}: crossfade (−45,−24), −9, (−2,2), 9, (24,45).
- `FzInputSpeed` {Slow, Medium, Fast, VeryFast}: crossfade (0,49), 71, 92, (120,150) kph.
- `FzInputBend` {Straight, Mild, Sharp}: crossfade (0,12), 40, (75,120) deg.
- Output `FzOutputWheel`: discrete (−1, −0.45, 0, 0.45, 1).
- Output `FzOutputThrottle`: discrete (−0.6, 0, 1).
- Defuzz: library default `MaxAvDefuzzer` (DoM-weighted average) → smooth continuous outputs;
  crossfade overlap keeps ≥2 rules active nearly every frame; shoulder functions give full coverage.

### Rules
- **Steering (5):** direct map steer-error label → wheel label.
- **Throttle (12):** speed × bend matrix plus misalignment guard:
  - Slow ∧ ¬HardError → Accelerate; Slow ∧ HardError → Coast; ¬Slow ∧ HardError → Brake
    (never accelerate while pointed off-line — that's how excursions become wipeouts).
  - Medium: Accelerate on Straight/Mild, Brake on Sharp.
  - Fast: Accelerate on Straight, Coast on Mild, Brake on Sharp.
  - VeryFast: Accelerate on Straight, Brake on Mild/Sharp.

## Validation (local PlayMode TestRunner via Unity CLI, sim-time 60 FPS)
Final full-suite run on the exact submission logic — **all four tracks, zero wipeouts**:

| Test | LTA km/h (min/target) | Wipeouts | Estimated score |
|---|---|---|---|
| Race_Curvy_5m | **66.1** (30/60) | 0 | 103.9% |
| Race_Winding_5m | **65.3** (33/66) | 0 | 99.4% |
| Race_FastSweepers_5m | **82.6** (43/80) | 0 | 101.2% |
| Race_DragRace_1m | **235.9** (43/86) | 0 | 105.0% |

Weighted assignment estimate ≈ **101.9 / 100** (includes extra credit on Curvy, Sweepers, Drag).

### Tuning history (what mattered)
1. v1: endpoint-angle curvature — S-curves read as "straight" → 2 falls on Winding.
2. v2: accumulated-bend input — falls persisted; telemetry showed the car running wide in tight
   bends (lateral offset → 5 m edge) then flooring throttle while misaligned (Slow→Accelerate rule).
3. v3: closer aim point (5+0.7v), tighter steer-error breakpoints (±9/±24), stronger mid-steer
   (±0.45), throttle gated on hard steer error → 0 wipeouts but over-cautious (LTA 57.7).
4. v4/v5: recalibrated bend set (winding track reads 40–60° nearly everywhere; "Sharp" now means
   hairpin-grade 75°+) and raised speed-set breakpoints → all targets met with 0 wipeouts.

## Performance / Complexity
Per frame: 6 path queries (1 aim point + 5 bend tangents), 3 set evaluations, 17 rules. Trivial
cost; no allocations in the hot path beyond the framework's own.

## Known Limitations
- Winding LTA is ~1 km/h under target (99.4% ≈ −0.4 assignment points with EC elsewhere covering it);
  pushing cruise speed further risked wipeouts on the other tracks, so this is the chosen balance.
- Drag extra credit saturates (LTA ≈ EC threshold 235); small provisioning variance on the
  autograder may land slightly below the full 5% EC there.
- Tracks are procedurally generated but deterministic under the test harness; autograder timing
  variance could shift results a little — margins are ≥3 km/h above full-speed-credit minimums
  everywhere, and 0 wipeouts leaves the full 1-wipeout allowance on Curvy/Winding as buffer.

## Current Status / Submission Checklist
- [x] Implemented; only `FuzzyVehicle.cs` modified (plus planning docs outside the project).
- [x] `StudentName = "Zonghao Hou"`; no `Debug.Log`; no hardcoded steering/throttle; no `INTERNAL_`;
      no reflection; crisp inputs conditional-free; full rule coverage (shoulder MFs on all inputs).
- [x] File is pure ASCII / UTF-8, no BOM.
- [x] **Local PlayMode validation: 4/4 tracks, 0 wipeouts, ≈101.9/100 estimated** (2026-07-14).
- [x] `// compile_check` is currently line 1 (compile-only mode).
- [ ] **Remove the `// compile_check` line for the real graded Gradescope run**, then re-add after.
- [ ] Submit `FuzzyVehicle.cs` to Gradescope (2 submissions max; don't resubmit while one is active).
