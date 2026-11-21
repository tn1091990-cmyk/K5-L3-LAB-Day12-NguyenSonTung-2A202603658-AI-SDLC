# CHANGELOG

## [Phase 3] — 2026-11-21
- **Breaking** contract bumped to v1.2 for the lantern fuel rules. Eight tuning keys appended
  after `night3_duration_s` in published order, then the score block; standard values locked
  (durations 240/270/300 s; FuelMax 100, decay 1.0/s, +10 per mote, relight 5, WinRatio 0.6,
  drain 5.0/s, low-fuel threshold 30 — exactly 30 keeps the zone; capacity max 20; chase
  range 6.0; weak-lantern walk max 2.0; score per_mote 5, per_lantern 100, per_relight 40).
- New events `refuel`, `lantern_out`, `fuel_drain` (one line per 60 ticks, top-of-minute
  value — see ADR-001 #4); `lantern_lit` gains `relight: true|false`; `game_over.reason` gains
  `dawn_survived`; **`all_lit` no longer ends the Night early**.
- Snapshot: lanterns gain `fuel`/`ever_lit`, player gains `successful_lights`, top level gains
  `capacity`; scenario: player `successful_lights`, shades `state`/`target_lantern_id`,
  lanterns `fuel`/`ever_lit` (lit without fuel means `fuel: 100`).
- Win rule changed to the dawn threshold `ceil(0.6 x 5) = 3` lit; dawn bonuses only on a
  won-at-dawn; flames-0 loss still checked first. Fuel stored in integer units (1 point = 60),
  printed to 1 decimal (ADR-001). Design record: `docs/adr/ADR-001-lantern-fuel-state-and-win-rules.md`.
- Tests updated because v1.2 replaces their rules (reasons in-file, nothing skipped): T06
  scoring pins (200→5 per mote, no time bonus), T11 (no early all-lit win). Suite grown to 29
  tests (T25 relight/top-up, T26 drain walk-in + beats + lantern_out, T27 dawn threshold,
  T28 capacity, T29 loader/contract), all green — `docs/proof/phase-3/tests.txt`; determinism
  double-run in `docs/proof/phase-3/determinism.md`; rules demo `docs/proof/phase-3/rules_demo.md`.
- Part-3 acceptance criteria (drifters, charms, 3-night chain, pause, settings, sound, HUD,
  palette incl. colourblind/flicker items): `docs/specs/PHASE-3-SPEC.md`. Presentation items
  stay manual checks — the Unity build on this machine is still blocked by the editor license
  token (unchanged from Phase 1's note).

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
