# Assignment 7 — Fuzzy Logic Racetrack — PLAN

**Course:** CS7632 O01 Game AI (Summer 2026) · **Due:** 07/19 11:59 PM AoE · **Threshold:** 70
**Submit:** `FuzzyVehicle.cs` only (in `GameAIRacetrack-main/Assets/Scripts/GameAIStudentWork/`)

## Problem Statement
Implement a Fuzzy Logic driving agent for a physically simulated racecar. All control
(Throttle, Steering ∈ [-1, 1]) must be produced exclusively by fuzzy rule evaluation via
`ApplyFuzzyRules(...)`. The car must sustain high average speed with ≤1 wipeout (0 for the two
fast tracks) over fixed-duration runs on four procedurally generated tracks.

## Spec / Rubric Mapping (from `RaceTestConfigs` assets + `RacingTest.cs`)

| Test | Track prefab | Duration | Min kph | Target kph | EC kph | Max wipeouts (full credit) | Grade wt |
|---|---|---|---|---|---|---|---|
| Race_Curvy_5m | RaceTrackRoot | 300 s | 30 | **60** | 70 | 1 (partial to 10) | 0.3 |
| Race_Winding_5m | RaceTrackRoot_WindingRaceTrack | 300 s | 33 | **66** | 75 | 1 (partial to 8) | 0.3 |
| Race_FastSweepers_5m | RaceTrackRoot_FastSweepers | 300 s | 43 | **80** | 125 | 0 (partial to 6) | 0.3 |
| Race_Drag_1m | RaceTrackRoot_DragRace | 60 s | 43 | **86** | 235 | 0 (partial to 2) | 0.1 |

Score per test = speed component (weight 0.6–0.8, `InverseLerp(min, target, LTA)^0.5`)
+ wipeout component (weight 0.2–0.4) + extra credit (≤0.05, only if 0 wipeouts).
"LTA" = long-term average speed = total path distance / elapsed time (kph).

## Constraints (PDF §7)
- No direct writes to Throttle/Steering; no `HardCodeThrottle`/`HardCodeSteering` in submission.
- No `INTERNAL_` access, no reflection; only the public `Racetrack` API.
- Crisp-input preparation must be (nearly) conditional-free — geometric/kinematic quantities only.
- Rule base must cover all inputs (shoulder membership functions guarantee this).
- Rules must overlap in DoM; ≥3 output states; multiple rules active most frames.
- No `Debug.Log`; initialize fuzzy structures in `Start()` (not `Awake()`); keep `base.Update()` last.
- `StudentName = "Zonghao Hou"` (HUD name check).

## Method

### Crisp inputs (pure geometry/kinematics, no conditionals)
1. **Steer error angle (deg)** — `Vector3.SignedAngle(transform.forward, aimPoint − position, Vector3.up)`
   where `aimPoint = Racetrack.GetPointAhead(aimBase + Speed·aimGain)` (the classic
   "steer toward a future point on the track"; inherently corrects lateral offset).
2. **Speed (kph)** — `Speed_kph`.
3. **Upcoming curvature (signed deg)** — angle between the track tangent at the car and the tangent
   a speed-scaled distance ahead: `SignedAngle(dirNow, Racetrack.GetDirectionAhead(curveBase + Speed·curveGain), up)`.
   Measures how much the road bends over the distance we're about to cover.

### Fuzzy variables
- `FzSteerError` {HardLeft, Left, Straight, Right, HardRight} — crossfade set over ± ~50°.
- `FzSpeed` {Slow, Medium, Fast, VeryFast} — crossfade set over 0–150+ kph.
- `FzCurve` {SharpLeft, Left, Straight, Right, SharpRight} — crossfade set over ± ~90°.
- Output `FzOutputWheel` {HardLeft…HardRight} — discrete set, reps ≈ (−1, −0.35, 0, 0.35, 1).
- Output `FzOutputThrottle` {Brake, Coast, Accelerate} — discrete set, reps ≈ (−0.6, 0, 1).

### Rules
Steering: direct 5→5 map from `FzSteerError` to `FzOutputWheel` (crossfade overlap ⇒ smooth
interpolated steering; shoulders ⇒ full coverage).

Throttle (speed × curvature matrix, full coverage):
- Slow → Accelerate (always regain speed).
- Medium ∧ Straight → Accelerate; Medium ∧ (Left ∨ Right) → Accelerate; Medium ∧ Sharp → Brake.
- Fast ∧ Straight → Accelerate; Fast ∧ (Left ∨ Right) → Coast; Fast ∧ Sharp → Brake.
- VeryFast ∧ Straight → Accelerate (drag race); VeryFast ∧ anything curved → Brake.

Defuzzification: library default `MaxAvDefuzzer` (DoM-weighted average of representative values)
⇒ smooth continuous outputs.

## Implementation Steps
1. Replace stub enums/sets/rules in `FuzzyVehicle.cs` per above; keep `// TODO … [implemented]` markers.
2. Set `StudentName = "Zonghao Hou"`; remove `HardCodeSteering/Throttle` calls and the demo viz block.
3. Validate: run PlayMode tests via Unity CLI (batch, simulated time) for all 4 configs; read
   LTA/wipeouts/estimated score from the log.
4. Tune set breakpoints / lookahead gains / output reps until all 4 tests hit target speed with
   allowed wipeouts.
5. Write PROJECT_SUMMARY.md, update CLAUDE.md milestones, commit.

## Provided/Known Test Cases
`Tests.RacingTest`: `Race_Curvy_5m`, `Race_Winding_5m`, `Race_FastSweepers_5m`, `Race_DragRace_1m`
(PlayMode). Each logs `Km/H LTA`, wipeouts, and `Estimated Total Score`.

## Risks / Notes
- Unity project lives in Dropbox — mark `Library/`, `Temp/`, `obj/`, `Logs/` with the
  `com.dropbox.ignored` NTFS stream before first open to avoid EPERM churn.
- Rear-wheel-drive oversteer at high speed: if FastSweepers wipes out, lower the Fast/VeryFast
  crossfade points or widen the Sharp curvature sets (earlier braking).
- Drag race needs sustained full throttle: VeryFast ∧ Straight must resolve to Accelerate ≈ 1.
