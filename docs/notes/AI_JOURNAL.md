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
