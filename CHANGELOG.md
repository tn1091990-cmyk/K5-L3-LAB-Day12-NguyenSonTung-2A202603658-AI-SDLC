# CHANGELOG

## [Phase 2] — 2026-11-21
- Contract bumped to v1.1: dash (14 m/s × 0.2 s, 1.5 s cooldown) and pause/resume in the core,
  new snapshot fields (`night`, `wind`, player `vel`/`invuln`/`dash_cd_s`/`pulse_cd_s`/`charm_s`,
  mote `kind`/`ttl_s`, `drifters`, `charms`); tuning keys appended in published order.
- Night 2 (two islands + bridge + jump pads) and Night 3 (three tiers + ramps + pad + bell tower
  + wind gusts) built to the standard numbers; per-night mote/shade schedules and clocks.
- New entities: Drifter "Gió Lạc" (steal/flee/pulse rules) and pickup charms (speed + dash-CD,
  respawn cycle); new events `dash`, `paused`, `resumed`, `night_start`, `charm_picked`,
  `drifter_steal`, `drifter_drop`; `player_hit` from a fall gains the `lost` field.
- Test suite grown to 24 tests, all green, Phase-1 tests untouched — `docs/proof/phase-2/tests.txt`;
  verification plan `docs/proof/phase-2/plan.md`.
- Determinism re-checked on v1.1 output: two 18,000-tick runs byte-identical
  (SHA-256 in `docs/proof/phase-2/determinism.md`); full 21,600-tick Night runs ≈ 1 s (budget 30 s).

## [Phase 1] — 2026-11-20
- Deterministic core built as a pure-C# library (same files compile into both the Unity
  project and the .NET headless runner). Deterministic RNG, fixed 60 ticks/s, `double` math only.
- Test Contract declared as `game.contract.json` v1.0 at the repo root; standard tuning values kept.
- Headless runner implemented (JSONL snapshot/event/end output, exit 0 / exit 2 on malformed input).
- Self-made test suite: 16 tests, all green (`docs/proof/phase-1/tests.txt`).
- Determinism evidence: two identical runs byte-identical, SHA-256 in `docs/proof/phase-1/determinism.md`.
- Harness (AGENTS.md) and license/credits written. Lab docs moved to `docs/lab/`.
- Known issue on this machine: Unity's package manager fails its build on an expired
  editor license token ("No ULF license found / Token not found in cache"); the C# solution and
  headless runner are unaffected. Unity Windows build to be produced after license refresh.

## [Phase 0] — 2026-11-19
- Repo cloned, lab documents read, plan agreed with the human.
