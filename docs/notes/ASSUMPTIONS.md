# ASSUMPTIONS — honest notes

Things the lab docs left open, and what I chose (agreed with the human where it mattered):

1. **Board link vs `Phases.txt`** — the GitHub board page returns only an empty JS shell, so
   the local `Phases.txt` copy is treated as the source of truth.
2. **Headless runs on Linux/macOS; the GUI is the Windows build** — this machine's Unity is
   Windows-only, so "the same binary does both" was rewritten honestly (README).
3. **Core is pure C# with no UnityEngine / System.IO / NuGet.** It uses only `double` math and
   its own deterministic RNG so hidden tests get byte-identical output on any platform.
   Unity and the .NET runner compile the *same* source files.
4. **JSONL shape**: `snapshot` / `event` / `end` lines; the run always ends with an `end` line,
   even after an early game over (agreed).
5. **Score on snapshots** follows the contract examples: pickup/deposit/stun events carry score;
   setup itself never scores (test T02 pins this).
6. **Malformed input → exit code 2** — applied to malformed scenario *and* inputs (agreed).
7. **Tuning keys and values in `game.contract.json` are fixed by the lab** — never "improved".
