# Đảo Đèn Lồng — Lantern Island

> 🎬 **Demo video:** attached to [Release v1.0.0](../../releases/tag/v1.0.0) — also committed at
> [`docs/demo/demo.mp4`](docs/demo/demo.mp4). Honest note: the video captures the *real* runs
> (tests, determinism double-run, bench, soak, leaderboard, Docker on Linux). The in-engine GUI
> capture needs a licensed Unity editor on the grading machine — this project is 100 % procedural
> (no binary art), and no screenshot here is faked.

A small 3D third-person game: you are **Mây**, a keeper in a conical hat, gathering
đom đóm (firefly motes) on the island and lighting its five Đèn Lồng Lớn before dawn,
while Bóng Mực (ink shades) hunt you. Pulse light to stun them. Score, win/lose, HUD, replay.

Built for the Khấu 5 Day-12 lab — the rules, checkpoints and rubric live in [`docs/lab/`](docs/lab/).

## Install & run (≤ 3 commands)

```bash
dotnet build DotNet/DaoDenLong.Headless -c Release          # the deterministic core + headless runner
dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --seed 42 --night 1 --ticks 600 --out run.jsonl   # headless run
UnityProject/Builds/Windows/DaoDenLong.exe                  # the GUI game (Windows build; built with the editor)
```

The GUI is the Unity Windows build (this machine's Unity is Windows-only — documented honestly).
The headless runner — the graded Test Contract interface — runs on any platform with the .NET SDK.

## Controls

WASD / arrows move · Space jump · **E** deposit at a lantern · **Q** or left click: pulse (Tỏa Sáng,
costs 3 motes) · hold right mouse + move: camera · **R** replay after game over.

## Scoring

+10 per mote deposited into a lantern · +100 per lantern lit (needs 10 motes) · +5 per shade stunned
by a pulse · win bonus: +2 per second left on the night clock, +50 per flame left (of 3).
A shade hit costs one flame and drops half your motes. Losing all flames = lose; all five lanterns
lit = win; dawn ends the night without all lit = lose.

## Tests & headless (Test Contract)

```bash
dotnet build DotNet/DaoDenLong.Headless -c Release
dotnet run --project DotNet/DaoDenLong.Tests
```

31 self-made tests cover movement, pickup, deposit, pulse, safe zones, falls, spawns, win/lose
order, input semantics, output shape, determinism, dash/pause, the Night 2/3 maps, jump pads,
drifters, charms, wind, the v1.2 fuel rules, the leaderboard rules and the soak-input generator
(see [`docs/proof/phase-1/`](docs/proof/phase-1/),
[`docs/proof/phase-2/`](docs/proof/phase-2/), [`docs/proof/phase-3/`](docs/proof/phase-3/) and
[`docs/proof/phase-4/`](docs/proof/phase-4/)).
The contract is declared in [`game.contract.json`](game.contract.json). Malformed inputs or scenario
→ exit code 2; identical seeds → byte-identical JSONL.

## Benchmark, soak, leaderboard (contract v1.3)

```bash
dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --bench --seed 42 --duration 30 --out bench.json   # 200 shades + 100 motes scene, 3 s warmup, exit 0
dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --soak  --seed 7  --nights 10   --out soak.json    # 10 nights one process, random-input bot, 0 crashes expected
dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --export-leaderboard out.json                      # top 10, score desc, ties → earlier achieved_at
```

Self-run numbers and the honest notes (including the soak memory ratio) are in
[`docs/proof/phase-4/notes.md`](docs/proof/phase-4/notes.md). The Unity GUI bench path
(`View/BenchMode.cs`) mirrors the spec 1:1 and runs on a licensed editor.

## Docker (the clean-machine path, macOS/Linux)

```bash
docker build -t dao-den-long .
docker run --rm dao-den-long     # build + full suite + bench + soak, Linux, no Unity needed
```

Real run evidence: [`docs/proof/phase-4/docker-linux.md`](docs/proof/phase-4/docker-linux.md).

## Accessibility

Remappable keys (duplicate detection + restore defaults, persisted), spawn/gust/last-10-s/
low-fuel indicators, motion reduction, UI font ≥18 px at 720p (large mode ×1.25) — implemented
in the Unity layer and listed as manual-check criteria on a licensed machine in
[`docs/specs/PHASE-4-SPEC.md`](docs/specs/PHASE-4-SPEC.md); the rules the core can enforce
(name validation, ordering) are machine-tested (T30).

## Harness, notes, credits

[`AGENTS.md`](AGENTS.md) is the AI harness in real use (with the agent's review/commit skills it drives).
Honest notes — assumptions, AI journal, retro — in [`docs/notes/`](docs/notes/).
Proof per phase in [`docs/proof/`](docs/proof/). [License](LICENSE) · [Credits](CREDITS.md) · [Changelog](CHANGELOG.md).
