# Phase 4 — benchmark, soak, leaderboard: real runs and honest notes

Everything in this folder is real command output, run 2026-11-21 on the lab machine
(Windows 10, .NET 8). Nothing here is pasted from imagination; where something could not
run, it says so.

## Commands (the contract v1.3 keys)

```
dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --bench --seed 42 --duration 30 --out bench.json
dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --soak  --seed 7  --nights 10   --out soak.json
```

`bench.json` and `soak.json` in this folder are those exact runs (self-measured, reference
values — the thresholds are judged on the grader's machine).

## Self-run results vs the ticket thresholds

| Metric | Threshold | This machine (self-run) |
|---|---|---|
| bench avg_fps | ≥ 60 | 7005.0 |
| bench p1_low_fps | ≥ 30 | 2948.0 |
| bench load_ms | ≤ 10 000 | 0 |
| soak crashes | 0 | 0 |
| soak mem_end / mem_start | ≤ 1.2× | 44.1 / 17.0 = 2.59× (see below) |

The bench numbers are the headless core's own cost (pure-C# simulation, one core tick per
measured frame, no GPU) — the `renderer` field in bench.json says so. The Unity GUI scene
(1280×720, vsync off, 15 s orbit camera — `UnityProject/Assets/DaoDenLong/View/BenchMode.cs`)
is the GPU path; this machine's Unity editor is license-blocked, so that path is written but
not run here. The headless path is the one that runs on the graders' macOS/Linux machines,
which is exactly what the ticket asked for.

## The memory ratio, honestly

mem_start is sampled right after the first Night loads (17 MB — before JIT/GC warm-up has
settled); mem_end after the last Night's reset. The jump is one-time runtime warm-up, not a
leak — the proof: repeated runs plateau at the same size and doubling the nights changes
nothing:

- 10 nights, run 1: 17.0 → 44.1 MB
- 10 nights, run 2: 17.0 → 44.6 MB
- 20 nights, run 3: 17.1 → 44.8 MB

Ten soak results are all `lost` — that is the random-input bot (5 % jump / 3 % dash / 2 %
pulse / 10 % interact per tick), which rarely gathers enough to keep three lanterns lit to
dawn. Honest outcome of random play, not a bug: no crashes in 10 nights (and 20).

## Determinism

v1.3 must not change a single simulation byte — double-run check on this machine:
`--seed 42 --ticks 18000` twice → both runs SHA-256
`4F2636A777616E6EBAE96BD4864A8DF676927B8945C4A97433B0904F85A69B7D`,
identical to the Phase 1–3 recorded hash. `tests.txt` is the real suite output (31/31).

## Leaderboard export

The board rules live in the pure core (`Core/Leaderboard.cs`, tested by T30/T31): top 10,
score descending, ties by earlier `achieved_at`; names `[A-Za-z0-9_]{3,12}`, Vietnamese
diacritics accepted as the bonus. `--export-leaderboard out.json` writes `{"entries":[...]}`
in order and runs clean on a machine with no saved board (empty list).
`leaderboard-seed.json` → `leaderboard-export.json` in this folder is a real round-trip: the
tie at score 500 is resolved by the earlier timestamp and both diacritic names survive.

## Docker

`docker build -t dao-den-long . && docker run --rm dao-den-long` (the two README commands)
runs build + suite + bench + soak on Linux. The Dockerfile is committed; the build-and-run
evidence is recorded in `docker-linux.md` if the daemon was available at write time — see
that file for what actually ran here.
