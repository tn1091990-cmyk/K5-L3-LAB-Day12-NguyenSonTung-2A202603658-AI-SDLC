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
