# CHANGELOG

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
