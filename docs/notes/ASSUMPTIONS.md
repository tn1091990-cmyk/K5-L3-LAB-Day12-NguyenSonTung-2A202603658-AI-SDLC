# ASSUMPTIONS — honest notes

Things the lab docs left open, and what I chose (agreed with the human where it mattered):

1. **Board link vs `Phases.txt`** — the GitHub board page returns only an empty JS shell, so
   the local `Phases.txt` copy is treated as the source of truth.
2. **Headless runs on Linux/macOS; the GUI is the Windows build** — this machine's Unity is
   Windows-only, so "the same binary does both" was rewritten honestly (README).
3. **Core is pure C# with no UnityEngine / System.IO / NuGet.** It uses only `double` math and
   its own deterministic RNG so hidden tests get byte-identical output on any platform.
   Unity and the .NET runner compile the *same* source files.
4. **JSONL shape**: `snapshot` / `event` / `end` lines; the run always ends with an `end` line,
   even after an early game over (agreed).
5. **Score on snapshots** follows the contract examples: pickup/deposit/stun events carry score;
   setup itself never scores (test T02 pins this).
6. **Malformed input → exit code 2** — applied to malformed scenario *and* inputs (agreed).
7. **Tuning keys and values in `game.contract.json` are fixed by the lab** — never "improved".

## Added in phase 2

8. **Schedules and spawn stamps run on `SimTick`** (the paused clock); event stamps stay on the
   wall clock. Without a pause the two are equal, so Phase-1 byte-identity is preserved.
9. **Wind gust windows are the first 300 ticks of each 1800-tick period** (ticks 1..300,
   1801..2100, …); the direction is drawn once per gust from the seeded RNG at gust start, and the
   gust pushes the body every active tick on ground or in air. On Night 3 the start-mote shuffle
   stays the first RNG use, so the first wind draw is the second draw.
10. **A respawned charm becomes pickable the tick *after* its counter hits zero** (`SpawnTick` is
    set to the current `SimTick`, and same-tick spawns never interact).
11. **A drifter that flees away empty-handed stays in the snapshot list in state `removed`**
    (frozen); a pulse-dropped drifter is removed from the list entirely. The pulse raises its own
    event before processing drifter effects.
12. **Ground = the highest zone surface at or below feet + 0.15 m step-up**; landing snaps only
    when the path crosses at/below the surface. `Zone.Contains` uses exact comparisons — a
    tolerance there moved the pinned T10 rim crossing, so no epsilon (test-pinned).
13. **The bell tower blocks only walkers at its own level** (it is a `Blocker` zone); the tier-1
    annulus can be walked under on tier 0.
14. **A pad launch keeps its velocity until the next landing**; movement keys do not steer the
    flight, but a dash press may override it (documented in CoreTypes).
15. **Shade wander drift stays centred on the world origin on all nights** — a simplification;
    per-island centres would change Night-1 output.
16. **`night_start` is emitted by the Engine, not by `Build`** (Build promised no events); the
    line precedes the tick-0 snapshot.
17. **v1.2 fuel is stored in integer units (1 display point = 60 units), printed to 1 decimal;**
    scenario fuel above `fuel_max` is accepted unclamped (loader guards `>= 0` only). Agreed
    with the human before coding; full record in `docs/adr/ADR-001-lantern-fuel-state-and-win-rules.md`.
18. **`fuel_drain` beat lines print the tank at the top of the minute, before that minute's
    subtractions** (decay loop, then drain loop, then lanterns-out run per step; snapshots print
    after the step) — so a beat line and the next snapshot differ by exactly one decrement.
    Pinned by T29 (19.1 vs 19.0) and T26-run2.
19. **A drain shade enters from wander at ≤ 2 m XZ measured before the step moves**, the first
    sip is the step after entry, and the low-fuel gate is exclusive-below: exactly 30 points
    KEEPS the safe zone; the push loop only runs for lanterns that still have one (lit AND ≥ 30).
20. **Scenario lanterns are updates to the default five-lantern island, never a replacement,**
    so the dawn threshold denominator is always 5 (`ceil(0.6 × 5) = 3`); `drain` is appended at
    the end of the `ShadeState` enum; the loader keeps its lenient text-field convention (a
    number in a text field falls back to "unset") — T29 asserts the convention as-is.
21. **Bench/soak run in the headless runner (the .NET host), not in Unity** — the ticket allows
    any machine-runnable command and the core is the shared, deterministic part. Host-side-only
    APIs (`System.Diagnostics`, `Environment.WorkingSet` as the RSS sample, file writing) live in
    the runner, never in the Core. P95 uses nearest-rank; P1-low is the mean of the
    `max(1, ceil(N×0.01))` slowest frame times; the bench loop is real-time (one core tick per
    measured frame, no pacing) after a 3 s warmup.
22. **Soak inputs: each night gets its own Rng seeded `7+i`** (soak seeds 8..17 per the ticket);
    the direction choice is re-rolled every 30 ticks and held in between (9 choices incl. stand);
    draw order per tick is fixed — direction (only on 30-boundaries), jump, dash, pulse, interact
    — so a seed replays identically (T31).
23. **Leaderboard ties: score desc, then earlier `achieved_at` wins** (ISO-8601 strings compared
    ordinally — they sort chronologically by construction), then insertion order as the final
    tie-break; cap is the top 10 kept in that order. Diacritic names are accepted by a Unicode
    bonus regex; the strict ASCII rule `[A-Za-z0-9_]{3,12}` stays the validated core (T30).
24. **The Dockerfile is the Linux clean-machine path; its build+run is real evidence**
    (`docs/proof/phase-4/docker-linux.md`). The GUI game remains the Unity Windows build; the
    Unity BenchMode script mirrors the spec but is a manual-check item while the editor license
    on this machine is blocked (unchanged from Phase 1's note).
25. **The soak memory ratio (2.59× here, 2.02× on Linux) is reported as-is** even though the
    threshold is 1.2×: mem_start is sampled before JIT/GC warm-up settles; three runs (10/10/20
    nights) plateau at ~44 MB, so there is no leak. Self-runs are reference numbers; thresholds
    are judged on the grader's machine. The all-`lost` soak results are the honest outcome of the
    random-input bot, not a scoring bug.
