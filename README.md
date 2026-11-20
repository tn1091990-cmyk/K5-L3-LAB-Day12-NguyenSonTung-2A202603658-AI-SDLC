# Đảo Đèn Lồng — Lantern Island

> 🎬 **Demo video:** will be linked here when attached to [Release v1.0.0](../../releases) (Phase 4, per SUBMISSION).

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

16 self-made tests cover movement, pickup, deposit, pulse, safe zones, falls, spawns, win/lose
order, input semantics, output shape and determinism (see [`docs/proof/phase-1/`](docs/proof/phase-1/)).
The contract is declared in [`game.contract.json`](game.contract.json). Malformed inputs or scenario
→ exit code 2; identical seeds → byte-identical JSONL.

## Harness, notes, credits

[`AGENTS.md`](AGENTS.md) is the AI harness in real use (with the agent's review/commit skills it drives).
Honest notes — assumptions, AI journal, retro — in [`docs/notes/`](docs/notes/).
Proof per phase in [`docs/proof/`](docs/proof/). [License](LICENSE) · [Credits](CREDITS.md) · [Changelog](CHANGELOG.md).
