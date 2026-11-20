# Phase 2 verification plan (written before the tests)

Each Part-2 feature maps to a test in the self-made suite; all of them run in
`dotnet run --project DotNet/DaoDenLong.Tests` (real output: `tests.txt` here).
Honest note: the plan was written before the test-writing step; the core code for some
features was implemented slightly ahead of it. The tests below are the verification.

| Feature (Phases.txt, Part 2)                          | Verified by |
|-------------------------------------------------------|-------------|
| Dash 14 m/s × 12 ticks, 1.5 s cooldown, air dash      | T17         |
| Pause: frozen sim, tick advances, `paused` snapshot   | T18         |
| Contract/snapshot v1.1 fields + format rejections     | T19         |
| Night 2/3 maps, clocks, per-night schedules, ids      | T20         |
| Jump pads: launch kept until landing, keys vs dash    | T21         |
| Drifters: steal once, flee 240, pulse-flee/-drop      | T22         |
| Charm: 7.2 m/s run, dash off cooldown, respawn cycle  | T23         |
| Night 3 wind: gust windows, seeded direction          | T24         |
| Everything from Phase 1 (regression)                  | T01–T16     |
| Determinism (two runs byte-identical)                 | T01 + `determinism.md` |
