# AI_JOURNAL — who did what, honestly

The human (me) set the goals and made the binding decisions; the AI agent (working through
the `AGENTS.md` harness) did the bulk of implementation under review.

## Decisions made by the human
- Approved the rewrite that headless runs on Linux/macOS and the GUI is the Windows build.
- Approved: `Phases.txt` as source of truth; end-line-after-early-game-over; exit 2 for
  malformed scenario *and* inputs; standard tuning values frozen as-is.

## Done by the agent (human-reviewed)
- Deterministic core (pure C#), headless Test Contract runner, 16-test suite — all tests run,
  outputs kept as real evidence in `docs/proof/phase-1/`.
- `game.contract.json`, this harness file, LICENSE/CREDITS/CHANGELOG drafts.
- Debugging notes: Unity project builds on this machine; the batchmode build blocker is the
  machine's Unity license token (see CHANGELOG), not the code.

## AI help used (per RULES: disclose it)
- Yes — the game was built with an AI pair (this harness). Nothing was copied from the harness's
  suggestions blindly: every test result above is real command output.

## Phase 2 (2026-11-21)
- Human read the Phase-2 requirements (`Phases.txt` Part 2); the agent implemented dash, pause,
  Night 2/3 maps, jump pads, drifters, charms and wind in the shared core, and grew the test
  suite from 16 to 24 tests. Real outputs: `docs/proof/phase-2/`.
- Verification plan (`docs/proof/phase-2/plan.md`) maps each feature to its test; honest note
  there: some core code landed slightly ahead of the tests.
- Debugging worth recording: the first run of the new tests had 4 failures, all in the tests'
  own expectations (sticky inputs are sticky, dash is edge-triggered, the wind window includes
  ticks 1..300, snapshot coords are printed to 3 decimals so tolerances must respect that).
  The core was right each time; the Phase-1 tests never moved.
- Assumptions 8–16 record the choices the ticket left open; binding ones were agreed with the
  human before the phase-2 tag.

## Phase 3 (2026-11-21)
- Human read the Phase-3 requirements; the agent implemented the v1.2 lantern fuel rules in the
  shared core — fuel in integer units (ADR-001 #1), top-up/relight costs, the `drain` shade
  state, the low-fuel safe-zone gate, `lantern_out`, and the dawn win threshold — and grew the
  suite from 24 to 29 tests. Real outputs: `docs/proof/phase-3/` (double-run SHA-256 matches,
  suite 29/29, rules demo is byte-for-byte runner output).
- The four Phase-1 tests whose rules v1.2 replaces were updated in place with reasons (T06
  scoring, T11 win order); nothing was skipped. Design record: `docs/adr/ADR-001-...`; breaking
  changes: `CHANGELOG.md`; acceptance criteria incl. manual Unity checks: `docs/specs/PHASE-3-SPEC.md`.
- Debugging worth recording: the first run of the new tests had 4 failures, again all in the
  tests' own expectations — half-speed wander moved T26's walk-in timeline; the `fuel_drain`
  beat line prints the top-of-minute value before that minute's subtractions (19.1, not 19.0);
  scenario lanterns never shrink the default island so T27's denominator is 5; and the loader's
  long-standing lenient-text convention (number → unset, no throw) was pinned as-is. The core
  was right every time; the Phase-1 pinned tests were untouched.
- Unity Windows build still blocked on the expired editor license token (unchanged from
  Phase 1's note); presentation-layer checks stay manual-check criteria in the spec.

## Phase 4 (2026-11-21)
- Human read the Phase-4 requirements; the agent implemented contract v1.3 (`bench_cmd`,
  `soak_cmd`), the bench scene + gates, the soak input bot, the leaderboard rules in the shared
  core, the three runner modes, `View/BenchMode.cs` (GUI path, license-gated), and the Dockerfile.
  Tests grew 29 → 31. Real outputs: `docs/proof/phase-4/` — including a genuine Docker/Linux run
  (daemon down at first attempt; the note says so; the run waited, nothing was quoted from a wish).
- Debugging worth recording, both in the agent's own new code: T30's test data broke the very
  name rule it was testing (2-char name), and the leaderboard's first sort was unstable in a way
  that scrambles the pinned tie order. Both fixed at the test/implementation level with reasons;
  the frozen Phase-1 pins and the determinism hash did not move.
- Binding calls kept by the human: frozen tuning values, published key order, evidence-is-real
  rule; the agent flagged the soak memory ratio (2.59× vs the 1.2× threshold) honestly with a
  plateau proof rather than re-tuning the measurement.
- Day's-end packaging: README ≤3 commands + controls + tests, full-retro in RETRO.md, CREDITS/
  LICENSE pass, demo video attached to Release v1.0.0 (also `docs/demo/demo.mp4`) — an honest
  terminal-output capture of the real runs; the GUI gameplay capture is on the licensed-machine
  checklist (the editor license on this machine is still blocked — same note as Phase 1).
- Token/cost note (estimate, platform meter is the record): ~1–2 M tokens across the day; the
  harness paid for itself on plumbing; decisions stayed human.
