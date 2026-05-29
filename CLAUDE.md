# CS7632 O01 Game AI — Claude Development Guidelines

Standardized guidelines for code generation, documentation, and project organization for the
CS7632 Game AI course. Adapted from the CS7637 Knowledge-Based AI guidelines, retargeted for
**Unity / C#** development and the CS7632 Gradescope autograder workflow.

---

## Project Structure

```
CS7632 O01 Game AI/
├── CLAUDE.md                       # This file - project guidelines
├── Assignment 0/                   # Roll a Ball (Unity tutorial, not graded by autograder)
│   └── Roll a ball/                # Unity project (DO NOT sync Library/ via Dropbox)
├── Assignment 1/                   # Grid Lattice
│   ├── ASSIGNMENT1_PLAN.md         # Plan: problem, method, rubric/spec mapping
│   └── PROJECT_SUMMARY.md          # Summary + current status (created after implementation)
├── Assignment 2/ .. Assignment 8/  # One folder per assignment (PLAN + SUMMARY each)
└── GameAIPathPlanning-master/      # SHARED Unity project for Assignments 1-8
    ├── Assets/Scripts/
    │   ├── GameAIStudentWork/      # <-- YOUR editable files live here
    │   │   ├── GridNavigation/CreateGrid.cs
    │   │   ├── PathNetwork/CreatePathNetwork.cs
    │   │   ├── AStar/AStarPathSearchImpl.cs
    │   │   ├── Navmesh/CreateNavMesh.cs
    │   │   └── Tests/              # NUnit EditMode tests (write your own!)
    │   └── Framework/              # Provided framework - DO NOT MODIFY or submit
    ├── Documents/                  # Assignment PDFs
    ├── Packages/ ProjectSettings/  # Unity config (commit these)
    └── Library/ Temp/ obj/ Logs/   # Generated - never commit, never Dropbox-sync
```

**Key fact:** Assignments 1–8 are all implemented inside the single shared Unity project
`GameAIPathPlanning-master`. The per-assignment `Assignment N/` folders hold the **planning and
summary docs only** (the `PLAN.md` / `PROJECT_SUMMARY.md`), not the code.

---

## Course Info (Summer 2026)

- **Section:** CS7632 O01 (Online, asynchronous, pre-recorded lectures). On-campus equivalent CS4731.
- **Instructor:** Jeff Wilson, PhD — `jeff.wilson@gatech.edu` (email subject must begin
  `[CS4731][CS7632]`). Primary communication via **Ed Discussion** (post privately when case-specific).
  *Note: Dr. Wilson authored the framework's computational-geometry code (`CG`/`ComputationalGeometry.cs`).*
- **Due times:** generally **11:59 PM Anywhere-on-Earth (AoE)** — always confirm in Canvas.
- **Grade scale:** standard, round-half-up (≥90 A, ≥80 B, ≥70 C, …).
- **Grade note:** Canvas does **not** reflect extra credit — compute your own grade from the weights.

## Grading Weights

| Component | Weight |
|-----------|--------|
| 8 Individual Assignments (10% each) | **80%** |
| Module Quizzes (12 quizzes, some modules none) | **20%** |
| Extra Credit — Prison Dodgeball Tournament | bonus (no CIOS EC) |

## The Assignments

Due dates are Summer 2026, **11:59 PM AoE** (≈ next morning ET). Weight is 10% each.

| # | Assignment (Canvas name) | Submit File(s) | Due (AoE) | Threshold | Status |
|---|--------------------------|----------------|-----------|-----------|--------|
| 0 | Unity Warmup (Roll a Ball) | — (not graded) | 05/22 12pm ET | — | Done |
| 1 | Grid Lattice / Grid Navigation | `CreateGrid.cs` | **05/31** | 60 | **Validated locally (8/8 tests) — pending Gradescope submission** |
| 2 | Path Network | `CreatePathNetwork.cs` | 06/07 | 70 | Not started |
| 3 | A* Pathfinding | `AStarPathSearchImpl.cs` | 06/14 | 70 | Not started |
| 4 | Navigation Mesh Generation | `CreateNavMesh.cs` | 06/21 | 70 | Not started |
| 5 | Ballistic Projectiles (Projectile Aiming w/ Prison Dodgeball) | `ThrowMethods.cs`, `ShotSelection.cs` | 06/28 | 70 | Not started |
| 6 | Prison Dodgeball (FSM / Decision Making) | `ThrowMethods.cs`, `ShotSelection.cs`, `MinionStateMachine.cs` | 07/05 | 70 | Not started |
| 7 | Fuzzy Logic Racetrack (Race Track) | `FuzzyVehicle.cs` | 07/19 | 70 | Not started |
| 8 | PCG | multiple (see PDF) | 07/26 | 70 | Not started |

**Module Quizzes** (12 total) all due **07/26 11:59 PM AoE**: Intro to Game AI, Basic Agent Movement,
Computational Geometry Intro, Path Planning #1, Path Planning #2, Steering Behavior, Ballistic
Projectile Aiming, Decision Making #1, Decision Making #2, Coordinated Movement, Learning, PCG.

---

## Gradescope Submission Rules (CS7632-specific)

These rules differ from typical courses — read carefully before every submission.

- **2 submissions per assignment.** Scoring:
  - `average_score = (s1 + s2) / 2`
  - `assigned_score = max(s1, average_score)`
  - With one valid submission: `assigned_score = s1`.
- **Compile failures do NOT count** against your 2 submissions.
- **Below-threshold scores do NOT count** (and you get no test detail). Threshold is **60** for
  Assignment 1, **70** for Assignments ≥ 2.
- **`// compile_check`** as the *first line* of a file makes the autograder only check compilation
  (does not run tests, does not consume a submission). **Re-add it after each submission** so you can
  re-verify compilation later. **Remove it for a real graded run.**
- **Runtime:** wait up to **1 hour** for results (provisioning variance, not code speed).
- **Never** submit again while a submission is active — both attempts will run the same code and burn
  both tries.
- **Encoding:** files must be **UTF-8**. Non-UTF-8 characters cause an autograder execution failure.
- **Remove all print/`Debug.Log` statements** before a graded submission (buffer overflow + timeout risk).
- **No infinite loops.** Each test gets ≥ 10 seconds.
- Submit **only** the listed student file(s). Never submit framework files or the whole engine.

---

## Critical: Unity + Dropbox Do Not Mix

This course folder lives inside Dropbox. Unity's generated folders churn constantly and **Dropbox
file locks cause `EPERM: operation not permitted` errors during package resolution** (already hit on
Assignment 0). Rules:

- Never let Dropbox sync `Library/`, `Temp/`, `obj/`, `Logs/`, `Build(s)/`, `UserSettings/`.
- Only `Assets/`, `Packages/`, and `ProjectSettings/` need to be tracked/synced.
- When packages fail to resolve: close Unity, pause Dropbox, delete `Library/`, reopen Unity.
- Prefer keeping live Unity projects **outside** Dropbox; let git (GitHub) be the sync mechanism.

---

## Academic Integrity — Authorship Rule (MANDATORY)

This is graded coursework submitted under the student's name. **Never attribute the work to Claude,
an AI, an assistant, or any other person — anywhere.**

- No "by Claude", "DONE by Claude", "generated by", "AI", or similar in any submitted file, comment,
  docstring, or marker.
- The only author identity in any file is the student: **`StudentAuthorName = "Zonghao Hou"`**.
- TODO/implementation markers must be neutral (e.g. `[implemented]`, `[helper method]`).
- Git commit messages and PR text for this repo must **not** include `Co-Authored-By: Claude` or any
  AI attribution.
- This rule overrides any default attribution behavior.

## C# / Unity Code Standards

- Target the **Unity-bundled C#/.NET** (the project uses `bool[,]`, `Vector2Int`, `Mathf`, etc.).
- Edit **only** files under `Assets/Scripts/GameAIStudentWork/`. Treat `Framework/` as read-only.
- Public methods are autograded **in isolation** and must be **stateless** — assume no call order.
- Match the surrounding code's style (the stub files set the convention: brace-on-new-line,
  `//` comments, explicit types).
- Prefer the provided helpers (e.g. `CG.*` computational geometry, `Polygon.getIntegerPoints()`,
  `MinIntBounds`/`MaxIntBounds`) over re-deriving geometry.
- Computational geometry runs in **discretized integer space** (float × 1000 via `CG.Convert`).
  Reason in integers to avoid float-equality bugs.
- Keep the `StudentAuthorName` string set to the real student name (the name test checks it is not
  "George P. Burdell").

---

## Implementation Workflow (per assignment / per conversation)

Follow these steps in order for every assignment. **Show the plan, summary, and current status.**

### Step 1 — Plan
- Read the assignment PDF in `GameAIPathPlanning-master/Documents/`.
- Read the stub student file and the framework APIs it depends on.
- Write `Assignment N/ASSIGNMENT<N>_PLAN.md`: problem statement, spec/rubric mapping, method,
  per-method implementation steps, files to submit, and the provided/known test cases.
- Present the plan in-conversation before coding.

### Step 2 — Implement
- Implement only the student file(s) in `GameAIStudentWork/`.
- Use provided helpers; keep methods stateless; no prints.
- **Preserve the original `// TODO IMPLEMENT` markers.** In every stub method that gets implemented,
  keep a `// TODO IMPLEMENT  -->  [implemented]   (original stub returned: ...)` comment showing what
  the placeholder originally returned. For any brand-new helper, add `// [helper method — not part of
  the original stub]`. This keeps the to-do list visible and makes it obvious what changed versus what
  the assignment asked to be written. **No author attribution in these markers** (see Academic
  Integrity rule below).

### Step 3 — Validate
- Reason through every provided test case (e.g. `GridTest.cs`) and confirm expected results.
- Run Unity **Test Runner** (Window → General → Test Runner → EditMode) where applicable.
- Do a **compile check**: add `// compile_check` as line 1 and submit once to confirm it builds
  clean in the autograder environment.
- Only mark an implementation "validated" when tests pass / compile check is clean.

### Step 4 — Summarize + Status
- Write `Assignment N/PROJECT_SUMMARY.md`: overview, approach, method-by-method notes, test results,
  performance/complexity, known limitations, and a **Current Status / Submission checklist**.
- Update the milestones table in this `CLAUDE.md` (status column + version/date footer).

### Step 5 — Commit to GitHub (after validation)
- **After each implementation conversation is validated, commit and push to the GitHub repo.**
- Stage only the relevant student file(s) + the PLAN/SUMMARY docs (never `Library/`, etc.).
- Use the `[Type]` commit convention below; push and verify with `git status`.
- Target repo: `https://github.com/rollinghina123/CS7632-O01-Game-AI`
  *(initial setup still required — see "Git / GitHub" below).*

---

## Git / GitHub

### Repository
- Remote: `https://github.com/rollinghina123/CS7632-O01-Game-AI` (currently **empty**).
- **Auth:** run `gh auth login` yourself (interactive) before the first push.
- **Before first push:** add the Unity `.gitignore` (below) so generated folders are excluded —
  `Assignment 0` alone is ~1.9 GB of `Library/` bloat that GitHub will reject.

### Commit Message Convention
```
[Type] Brief description (<= 50 chars)

- Detail 1
- Detail 2

Assignment: N - <name>
Validation: <test results / compile-check status>
Files:
- path/to/File.cs: what changed
```
Types: `[Implement]` `[Fix]` `[Test]` `[Docs]` `[Refactor]` `[Optimize]`
**No `Co-Authored-By: Claude` or AI attribution** (see Academic Integrity rule).

**Commit trigger:** after Step 3 (validation) of each assignment conversation.

### Unity `.gitignore` (add at repo root)
```gitignore
# Unity generated
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]serSettings/
[Mm]emoryCaptures/
*.csproj
*.sln
*.user

# OS / IDE
.DS_Store
Thumbs.db
.vscode/
.idea/

# Claude local settings
.claude/settings.local.json
```

---

## Documentation File Conventions

| File | Location | Purpose |
|------|----------|---------|
| `CLAUDE.md` | course root | These guidelines + milestones table (this file) |
| `ASSIGNMENT<N>_PLAN.md` | `Assignment N/` | Plan: problem, spec mapping, method, steps, test cases |
| `PROJECT_SUMMARY.md` | `Assignment N/` | Summary + **current status** + submission checklist |

Update the SUMMARY's "Current Status" section as work progresses so status is always visible
across conversations.

---

## Notes

- Update this file as new patterns emerge per assignment.
- Convert relative dates to absolute when recording status.
- Document lessons learned from each Gradescope submission (score, what changed).

---

*Last Updated: 2026-05-28*
*Version: 1.1 (added Summer 2026 schedule, weights, grading from syllabus)*
*Primary Course: CS7632 O01 — Game AI*
