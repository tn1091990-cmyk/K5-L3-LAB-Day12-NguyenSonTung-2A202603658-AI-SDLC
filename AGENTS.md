# AGENTS.md — harness for AI agents working on this repo

This is the instruction file agents are expected to follow (and keep in git history).

## What this project is

Đảo Đèn Lồng ("Lantern Island"): a small 3D third-person game for the Khấu 5 Day-12 lab
(see `docs/lab/`). The rules of the game are fixed by the lab board / `Phases.txt`; the
deterministic core is a pure-C# library shared by the Unity game and the headless test runner.

## Layout

- `UnityProject/Assets/DaoDenLong/Core/` — the deterministic simulation (pure C#, no UnityEngine,
  no `System.IO`, no NuGet). Both Unity and .NET compile these same files.
- `DotNet/DaoDenLong.Headless/` — the Test Contract runner (JSONL, exit 0 / 2).
- `DotNet/DaoDenLong.Tests/` — the self-made test suite (24 tests, exit = number of failures).
- `game.contract.json` — the Test Contract declaration at the repo root.
- `docs/notes/` — honest notes: assumptions, AI journal, retro. `docs/proof/` — per-phase evidence.

## Commands (the whole loop)

```bash
dotnet build DotNet/DaoDenLong.Headless -c Release    # build_cmd in the contract
dotnet run --project DotNet/DaoDenLong.Tests          # run the test suite
```

The test suite prints `PASS`/`FAIL` per test and exits with the failure count.
**Never declare a phase done without running it, and keep the real output as evidence**
— fabricated evidence is worth zero per `docs/lab/RUBRIC.md`.

Determinism check (must stay byte-identical, same seed):

```bash
dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --seed 42 --ticks 18000 --out a.jsonl
dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --seed 42 --ticks 18000 --out b.jsonl
# a.jsonl and b.jsonl must have the same SHA-256
```

## The Test Contract (the graded interface)

- Headless mode runs on macOS/Linux; the GUI is the Unity Windows build (documented honestly).
- Fixed 60 ticks/s. JSONL output: `snapshot` / `event` / `end` lines; the run ends with a
  mandatory `{"type":"end",...}` line, even after an early game over.
- Malformed inputs or scenario → **exit code 2**. Two identical runs must be byte-identical.
- The tuning keys and standard values in `game.contract.json` are fixed by the lab —
  do not "improve" them; the hidden tests use them.

## Rules for agents (from `docs/lab/RULES.md`, honoured in practice)

1. Small, clear commits; one step each; push after each checkpoint; tag `phase-1`…`phase-4`.
2. No `.env`, keys, or tokens in the repo or in history.
3. Evidence must be real output, pasted or saved as files; write an honest "not met" note
   instead of stretching the truth.
4. Assets: CC0 / self-made / AI-made only, listed in `CREDITS.md`. This project is procedural
   on purpose: no third-party art was needed at all.
5. Simple over clever. The core uses only `double` math and its own deterministic RNG so the
   hidden tests get byte-identical results on any platform.
6. Harness lesson (phase 2, repeat offenders): test tolerances must respect printed precision
   (snapshot coords are 3 decimals — never assert tighter); input fields are sticky and
   dash/pause are false→true edges; and Phase-1 pinned tests are frozen — when a new test
   fails, first re-check the test's own reading of the spec.
