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
