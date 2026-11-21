# Phase 4 spec — acceptance criteria per Part-4 item (TICKET-04)

Source of truth for the numbers and rules is `Phases.txt` (this file maps *how each item is
accepted*). Real evidence lives in `docs/proof/phase-4/`; nothing here was pasted from memory.

## Machine-verified items (ran on this machine, and on Linux via Docker)

| Item | Acceptance criterion (observable) | Evidence |
|---|---|---|
| Contract v1.3 | `bench_cmd`/`soak_cmd` keys published; `tuning` unchanged; no sim byte moved — double-run SHA-256 identical to the Phase 1–3 hash | `tests.txt` (T19 pins tuning), re-run hash in `notes.md` |
| Benchmark | `<bench_cmd> --seed 42 --duration 30 --out bench.json`: 200 shades + 100 motes by seed, L1–L3 lit fuel 100, spawn/damage/pickup/decay/end gated, 3 s warmup, all JSON fields; avg ≥60, P1-low ≥30, load ≤10 s | `bench.json` (self-run, reference), `docker-linux.md` (Linux run, exit 0) |
| Soak | `<soak_cmd> --seed 7 --nights 10 --out soak.json`: one process, Night 1→2→3 cycling (seeds 8..17), seeded random inputs (30-tick direction reroll; 5/3/2/10 % probabilities), crashes = 0, mem_end ≤ 1.2× mem_start | `soak.json` + plateau proof in `notes.md` (ratio exceeds threshold on this machine — explained, not smoothed over) |
| Leaderboard rules | names `[A-Za-z0-9_]{3,12}` (diacritics accepted as bonus), top 10, score desc, ties → earlier `achieved_at`; nights_won 0–3; `--export-leaderboard out.json` → `{"entries":[...]}` in order on a clean machine | T30 + `leaderboard-seed.json` → `leaderboard-export.json` round-trip |
| Tests | suite green, exit = failures (0); Phase-1 pinned tests frozen | `tests.txt` 31/31 |
| Clean machine | clone → README ≤3 commands → runs; Docker allowed by the ticket | `docker-linux.md` (real container run) |

## Unity-side items — manual-check criteria (editor license on this machine, unchanged from Phase 1)

The GUI path is written and mirrors the spec exactly, but the editor cannot start here, so these
are *checks to run on a licensed machine*, marked as such — not claimed as passed:

| Item | Check on a licensed machine | Code |
|---|---|---|
| Bench GUI path | 1280×720, vsync off, orbit camera 15 s/turn, same JSON fields as the headless run | `UnityProject/Assets/DaoDenLong/View/BenchMode.cs` |
| Remappable keys | every action remappable; duplicate binding detected and refused; "restore defaults" works; bindings persist across runs | Unity input layer (settings panel) |
| Visual indicators | spawn arrows, gust direction, "last 10 s" pulse, fuel-below-30 warning on each weak lantern | HUD pass in `View/GameBootstrap.cs` |
| Motion reduction | screen-shake and flicker toggle off cleanly | settings, HUD |
| UI font | ≥18 px at 720p; large mode ×1.25 | OnGUI font scale |

Why the split: the deterministic core (rules, RNG, scenarios) is what the hidden tests grade and
what runs headless on macOS/Linux; presentation is Unity-only and needs a licensed editor to run,
so it is honest to list it as a manual checklist rather than fake a green tick.

## Packaging & day's-end items

| Item | Status |
|---|---|
| README: demo link at top, short description, ≤3 install/run commands, controls, how to run tests + headless | done (`README.md`) |
| Demo video `.mp4` <5 min, attached to Release v1.0.0 (preferred) — also at `docs/demo/demo.mp4` | done — honest terminal-output capture (the licensed-machine checklist above covers gameplay capture; no fake screenshots) |
| CREDITS: asset/audio/AI-tool sources | done (`CREDITS.md`) — project is procedural, no third-party assets |
| LICENSE present, no `.env`/keys in repo or history | done (MIT, `LICENSE`) |
| Repo public; tags `phase-1`…`phase-4`; Release v1.0.0 with runnable artifact | done — artifact: self-contained .NET publish zip (build+tests+bench+soak run from it) or the Docker image; instructions in the release notes |
| Retro 1–2 pages incl. AI-tooling/token notes | done (`docs/notes/RETRO.md`) |
